using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace Deevenue.Api.Framework;

public class NotificationViewModelFormatter : TextOutputFormatter
{
    private readonly SystemTextJsonOutputFormatter parent;

    public NotificationViewModelFormatter(SystemTextJsonOutputFormatter parent)
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
        this.parent = parent;
    }

    protected override bool CanWriteType(Type? type) => typeof(NotificationViewModel).IsAssignableFrom(type);

    public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        context.HttpContext.Response.Headers.Append("X-Deevenue-Schema", "Notification");
        return parent.WriteResponseBodyAsync(context, selectedEncoding);
    }
}
