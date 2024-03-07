using System.Text.Json.Serialization;

namespace Notifications.Core;

[JsonDerivedType(typeof(UserConnected))]
[JsonDerivedType(typeof(LikeUpdate))]
public record Message(string Type);
public record UserConnected(string Name) : Message(nameof(UserConnected));
public record LikeUpdate(string PostId, int Count) : Message(nameof(LikeUpdate));