namespace Deevenue.Infrastructure.MediaProcessing;

internal class TemporaryVideoFile : IDisposable
{
    private bool disposedValue;
    private readonly string fileName;

    public static TemporaryVideoFile From(Stream stream)
    {
        var fileName = Path.GetTempFileName();
        using var tempInputFile = File.OpenWrite(fileName);
        stream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(tempInputFile);
        stream.Seek(0, SeekOrigin.Begin);
        return new TemporaryVideoFile(fileName);
    }

    private TemporaryVideoFile(string fileName)
    {
        this.fileName = fileName;
    }

    public static implicit operator string(TemporaryVideoFile temporaryVideoFile)
        => temporaryVideoFile.fileName;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                File.Delete(fileName);
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
