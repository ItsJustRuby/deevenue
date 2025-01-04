namespace Deevenue.Domain;

public interface ITracingSpans
{
    IDisposable Create(string operation, params string[] nameSegments);
}

internal class TracingSpans(IHub sentryHub) : ITracingSpans
{
    public IDisposable Create(string operation, params string[] nameSegments)
    {
        var sentrySpan = sentryHub.GetSpan()?.StartChild(operation, string.Join(".", nameSegments));
        return new TracingSpan(sentrySpan);
    }

    private class TracingSpan(ISpan? sentrySpan) : IDisposable
    {
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;

            if (disposing)
                sentrySpan?.Finish();

            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
