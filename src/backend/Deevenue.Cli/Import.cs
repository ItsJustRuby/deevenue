using System.Net;
using System.Text.Json;
using Deevenue.Cli.Networking;
using RestSharp;
using Spectre.Console.Cli;

namespace Deevenue.Cli;

internal class Import : AsyncCommand<Import.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<directory>")]
        public required string InputDirectory { get; set; }
    }

    private static readonly RestClient apiClient = new(new RestClientOptions("https://localhost/")
    {
        CookieContainer = new CookieContainer(),
        // Screw security, we are just routing localhost to localhost.
        RemoteCertificateValidationCallback = (_, _, _, _) => true
    });

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var inputDirectory = Path.GetFullPath(settings.InputDirectory);
        if (!Directory.Exists(inputDirectory))
        {
            Console.WriteLine("Input directory not found ({0})", inputDirectory);
            return 1;
        }

        using var reverseProxy = ReverseProxy.StartNew();

        // This is necessary to be allowed to do everything down the line
        // without getting 403s back.
        var restResponse = await apiClient.PatchAsync(
            new RestRequest("/session").AddJsonBody(new SessionPatchBody { IsSfw = false }));

        await ImportRulesFileAsync(inputDirectory);

        var success = await ImportMediaAsync(inputDirectory);
        if (!success)
        {
            Console.WriteLine("Failed to import media, aborting...");
            return 2;
        }

        success = await ImportTagsAsync(inputDirectory);
        if (!success)
        {
            Console.WriteLine("Failed to import tags, aborting...");
            return 4;
        }

        return 0;
    }

    private async Task<bool> ImportMediaAsync(string inputDirectory)
    {
        var mediaDirectory = Path.Combine(inputDirectory, "media");
        if (!Path.Exists(mediaDirectory))
        {
            Console.WriteLine("Found no media directory.");
            return false;
        }

        var allSubdirectories = Directory.GetDirectories(mediaDirectory);

        if (allSubdirectories.Length == 0)
        {
            Console.WriteLine("Found no subdirectories in media directory.");
            return false;
        }

        // Sanity check each directory beforehand: needs both a medium.{ext} and a metadata.json
        Console.WriteLine("Running sanity check over the entire media directory.");
        var mediaCount = 0;
        foreach (var subDirectory in allSubdirectories)
        {
            var files = Directory.GetFiles(subDirectory);
            if (files.Where(f => Path.GetFileName(f) != "metadata.json").SingleOrDefault() == null)
            {
                Console.WriteLine("Directory {0} contains no medium file", subDirectory);
                return false;
            }

            if (files.Where(f => Path.GetFileName(f) == "metadata.json").SingleOrDefault() == null)
            {
                Console.WriteLine("Directory {0} contains no metadata.json", subDirectory);
                return false;
            }
            mediaCount++;
        }
        Console.WriteLine("Looks good, importing {0} media!", mediaCount);

        foreach (var subDirectory in allSubdirectories)
        {
            var files = Directory.GetFiles(subDirectory);
            var mediumFilePath = files.Where(f => Path.GetFileName(f) != "metadata.json").Single();
            var metadataFilePath = files.Where(f => Path.GetFileName(f) == "metadata.json").Single();

            using var metadataFile = File.OpenRead(metadataFilePath);
            var metadata = await JsonSerializer.DeserializeAsync<MetadataFile>(metadataFile, JsonSerializerSettings.Default);

            if (metadata == null)
            {
                Console.WriteLine("Could not read metadata for file {0}", mediumFilePath);
                return false;
            }

            // The filename won't be taggy and that's fine
            Console.WriteLine("Uploading file {0}", subDirectory);

            var uploadResponse = await apiClient.ExecutePostAsync(
                new RestRequest("medium")
                .AddFile("file", mediumFilePath, metadata.ContentType));

            // Note: "break" in this case means "continue with this loop iteration, just exist this switch/case.
            switch (uploadResponse.StatusCode)
            {
                case HttpStatusCode.Conflict:
                    Console.WriteLine("File at {0} already known, skipping it", subDirectory);
                    break;
                case HttpStatusCode.OK:
                    break;
                default:
                    throw new Exception($"Failed to upload Medium, got status code {uploadResponse.StatusCode}");
            }

            var notification = JsonSerializer.Deserialize<Notification>(uploadResponse.Content!, JsonSerializerSettings.Default);
            var mediumId = notification!.Contents.SingleOrDefault(p => p.Id != null)?.Id;

            if (mediumId == null)
            {
                Console.WriteLine("Could not get mediumId after POSTing file {0}", mediumFilePath);
                return false;
            }

            await apiClient.PutAsync(
                new RestRequest($"medium/{mediumId}/rating/{metadata.Rating}"));

            foreach (var tag in metadata.Tags)
                await apiClient.PutAsync(new RestRequest($"medium/{mediumId}/tags/{tag}"));

            foreach (var absentTag in metadata.AbsentTags)
                await apiClient.PutAsync(new RestRequest($"medium/{mediumId}/absentTags/{absentTag}"));
        }

        return true;
    }

    private async Task<bool> ImportTagsAsync(string inputDirectory)
    {
        var tagsFilePath = Path.Combine(inputDirectory, "tags.json");
        if (!Path.Exists(tagsFilePath))
        {
            Console.WriteLine("Found no tags file.");
            return false;
        }
        using var tagsFile = File.OpenRead(tagsFilePath);
        var tagsObj = await JsonSerializer.DeserializeAsync<TagsFile>(tagsFile, JsonSerializerSettings.Default);

        if (tagsObj == null)
        {
            Console.WriteLine("Failed to deserialize tags file");
            return false;
        }

        foreach (var tag in tagsObj.Tags)
        {
            await apiClient.PutAsync(new RestRequest($"tag/{tag.Name}/rating/{tag.Rating}"));

            foreach (var alias in tag.Aliases)
                await apiClient.ExecutePostAsync(new RestRequest($"tag/{tag.Name}/aliases/{alias}"));

            foreach (var impliedTagName in tag.ImpliedTagNames)
                await apiClient.ExecutePostAsync(new RestRequest($"tag/{tag.Name}/implications/{impliedTagName}"));
        }

        return true;
    }

    private static async Task ImportRulesFileAsync(string inputDirectory)
    {
        var rulesFilePath = Path.Combine(inputDirectory, "rules.json");
        if (!Path.Exists(rulesFilePath))
        {
            Console.WriteLine("Found no rules file.");
            return;
        }

        var rulesText = File.ReadAllText(rulesFilePath);

        var response = await apiClient.PostAsync(new RestRequest("rule").AddJsonBody(rulesText));
        Console.WriteLine("Response status code: {0}", response.StatusCode);
    }

    private class SessionPatchBody
    {
        public required bool IsSfw { get; set; }
    }

    private class MetadataFile
    {
        public required string[] Tags { get; set; }
        public required string[] AbsentTags { get; set; }

        public required string Rating { get; set; }
        public required string ContentType { get; set; }
    }

    private class TagsFile
    {
        public required Tag[] Tags { get; set; }
    }

    private class Tag
    {
        public required string Name { get; set; }
        public required string Rating { get; set; }
        public required string[] Aliases { get; set; }
        public required string[] ImpliedTagNames { get; set; }
    }

    private class Notification
    {
        public required NotificationPart[] Contents { get; set; }
    }

    private class NotificationPart
    {
        public Guid? Id { get; set; }
    }
}
