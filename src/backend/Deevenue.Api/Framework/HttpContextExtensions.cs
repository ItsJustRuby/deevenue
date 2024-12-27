using System.Text.Json;

namespace Deevenue.Api.Framework;

public static class HttpContextExtensions
{
    public static DeevenueSession DeevenueSession(this HttpContext context)
    {
        var maybeSession = context.Session.GetString(Framework.DeevenueSession.Key);
        DeevenueSession? deserialized = null;
        if (maybeSession != null)
            deserialized = JsonSerializer.Deserialize<DeevenueSession>(maybeSession)!;

        var result = Framework.DeevenueSession.Create(context.Session);

        if (deserialized != null)
            result.UpdateWith(deserialized);

        return result;
    }
}

public record SessionViewModel(bool IsSfw);

public class DeevenueSession
{
    internal const string Key = "DeevenueSession";

    public static DeevenueSession Create(ISession origin)
    {
        return new DeevenueSession { origin = origin, IsSfw = true };
    }

    internal void UpdateWith(DeevenueSession other)
    {
        IsSfw = other.IsSfw;
    }

    public bool IsSfw
    {
        get
        {
            return isSfw;
        }
        set
        {
            isSfw = value;
            Persist();
        }
    }
    private bool isSfw;

    private ISession? origin;

    private void Persist() =>
        origin?.SetString(Key, JsonSerializer.Serialize(this));
}
