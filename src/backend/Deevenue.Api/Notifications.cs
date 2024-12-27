using System.Diagnostics;
using System.Text.Json.Serialization;
using Deevenue.Domain;

namespace Deevenue.Api;

[JsonConverter(typeof(LowercaseJsonStringEnumConverter))]
public enum NotificationLevel
{
    Error,
    Warning,
    Info
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Link), "link")]
[JsonDerivedType(typeof(TextPart), "text")]
[JsonDerivedType(typeof(Entity), "entity")]
public interface INotificationPartViewModel { }

[DebuggerDisplay("{ToString()}")]
internal class Entity : INotificationPartViewModel
{
    public required EntityKind EntityKind { get; init; }
    public required Guid Id { get; init; }
}

[DebuggerDisplay("{ToString()}")]
internal class Link : INotificationPartViewModel
{
    public required string Location { get; init; }
    public required string Text { get; init; }
}

[DebuggerDisplay("{ToString()}")]
internal class TextPart : INotificationPartViewModel
{
    public required string Text { get; init; }
}

[DebuggerDisplay("{ToString()}")]
public class NotificationViewModel
{
    public required NotificationLevel Level { get; init; }

    public required IList<INotificationPartViewModel> Contents { get; init; } = [];

    public override string? ToString() => $"{Level}: {string.Join(" ", Contents.Select(c => c.ToString()))}";
}

internal static class Notifications
{
    public static INotificationBuilder CreateBuilder(NotificationLevel level)
        => new NotificationBuilder(level);

    public interface INotificationBuilder
    {
        INotificationBuilder WithEntity(EntityKind medium, Guid id);
        INotificationBuilder WithLink(string location, string text);
        INotificationBuilder WithText(string text);

        NotificationViewModel Build();
    }

    private class NotificationBuilder : INotificationBuilder
    {
        private readonly NotificationLevel level;
        private readonly IList<INotificationPartViewModel> contents = [];

        internal NotificationBuilder(NotificationLevel level)
        {
            this.level = level;
        }

        public INotificationBuilder WithEntity(EntityKind entityKind, Guid id)
        {
            contents.Add(new Entity { EntityKind = entityKind, Id = id });
            return this;
        }

        public INotificationBuilder WithLink(string location, string text)
        {
            contents.Add(new Link { Location = location, Text = text });
            return this;
        }

        public INotificationBuilder WithText(string text)
        {
            contents.Add(new TextPart { Text = text });
            return this;
        }

        public NotificationViewModel Build()
        {
            return new NotificationViewModel
            {
                Level = level,
                Contents = contents,
            };
        }
    }
}
