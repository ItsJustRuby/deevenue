namespace Deevenue.Infrastructure.Jobs;

internal class TemporaryDirectory : IDisposable
{
    private bool disposedValue;
    private readonly string tempPath;

    public TemporaryDirectory(params string[] pathSegments)
    {
        tempPath = Path.Combine([Path.GetTempPath(), .. pathSegments]);
        Directory.CreateDirectory(tempPath);
    }

    public static implicit operator string(TemporaryDirectory t) => t.tempPath;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Directory.Delete(tempPath, recursive: true);
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
