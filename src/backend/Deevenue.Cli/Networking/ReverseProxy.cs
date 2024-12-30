using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Deevenue.Domain;
using RestSharp;

namespace Deevenue.Cli.Networking;
internal class ReverseProxy : IDisposable
{
    private bool isDisposed;
    private Process? caddyProcess;

    public static IDisposable StartNew()
    {
        var result = new ReverseProxy();
        result.Start();
        return result;
    }

    private ReverseProxy() { }

    private void Start()
    {
        caddyProcess = new Process
        {
            StartInfo = new ProcessStartInfo(
                "caddy",
                [
                    "reverse-proxy",
                    "--disable-redirects",
                    "--internal-certs",
                    "--from", "localhost:443",
                    "--to", "localhost:8080"
                ]
            )
            {
                // Even if we don't display it, this captures it and
                // prevents it from showing up in our own
                // stdout/stderr instead
                //RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };

        Console.WriteLine("Starting up reverse proxy.");

        var awaitCorrectLogMessage = new TaskCompletionSource();
        caddyProcess.ErrorDataReceived += (object _, DataReceivedEventArgs e) =>
        {
            var caddyLine = JsonSerializer.Deserialize<CaddyLine>(e.Data!, JsonSerialization.DefaultOptions);
            if (caddyLine!.From != null && caddyLine.To != null)
                awaitCorrectLogMessage.SetResult();
        };
        caddyProcess.Start();
        caddyProcess.BeginErrorReadLine();

        Console.WriteLine("Waiting for startup...");
        awaitCorrectLogMessage.Task.Wait();
        Console.WriteLine("Reverse proxy started.");

        var didTimeOut = true;
        var options = new RestClientOptions("https://localhost")
        {
            // Screw security, we are just routing localhost to localhost.
            RemoteCertificateValidationCallback = (_, _, _, _) => true
        };
        using var pingClient = new RestClient(options);

        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Checking health of backend.");
        while (stopwatch.Elapsed < TimeSpan.FromMinutes(1))
        {
            var response = pingClient.ExecuteGet(new RestRequest("/health"));
            if (response.Content != null)
                Console.WriteLine("Status {0}, Content {1}", response.StatusCode, response.Content);
            else
                Console.WriteLine("Status {0}", response.StatusCode);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                didTimeOut = false;
                break;
            }
            else
                Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        if (didTimeOut)
        {
            Console.WriteLine("Timed out setting up the reverse proxy.");
            throw new Exception("Timed out setting up the reverse proxy.");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                caddyProcess?.Kill();
                caddyProcess?.Dispose();
            }

            isDisposed = true;
        }
    }

    private record CaddyLine(string Level, string Msg, string? From, string[]? To);
}
