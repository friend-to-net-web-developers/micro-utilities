// ReSharper disable InconsistentNaming

namespace FriendToNetWebDevelopers.MicroUtilities.Enum;

public class YoutubeThumbnailEnum
{
    private const string MAX_RES_DEFAULT = "maxresdefault";
    private const string HQ_DEFAULT = "hqdefault";

    private string Name { get; }

    private YoutubeThumbnailEnum(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }

    public static readonly YoutubeThumbnailEnum MaxResDefault = new(MAX_RES_DEFAULT);
    public static readonly YoutubeThumbnailEnum HqDefault = new(HQ_DEFAULT);
}