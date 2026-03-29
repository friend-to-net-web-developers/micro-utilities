// ReSharper disable InconsistentNaming

namespace FriendToNetWebDevelopers.MicroUtilities.Enum;

/// <summary>
/// Represents a YouTube thumbnail quality level.
/// </summary>
/// <remarks>
/// This is a type-safe enum (smart enum) rather than a <see cref="System.Enum"/> — the string
/// values map directly to YouTube's thumbnail filename conventions and are used as URL path
/// segments via <see cref="ToString"/>.
/// </remarks>
public sealed class YoutubeThumbnail
{
    private const string MaxResDefaultValue = "maxresdefault";
    private const string HqDefaultValue = "hqdefault";

    private string Name { get; }

    private YoutubeThumbnail(string name)
    {
        Name = name;
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>Maximum resolution thumbnail (1280×720). May not be available for all videos.</summary>
    public static readonly YoutubeThumbnail MaxResDefault = new(MaxResDefaultValue);

    /// <summary>High quality thumbnail (480×360). Available for all videos.</summary>
    public static readonly YoutubeThumbnail HqDefault = new(HqDefaultValue);
}