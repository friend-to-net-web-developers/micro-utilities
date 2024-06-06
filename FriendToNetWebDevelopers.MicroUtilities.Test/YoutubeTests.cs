using System.Net;
using FriendToNetWebDevelopers.MicroUtilities.Enum;
using FriendToNetWebDevelopers.MicroUtilities.Exception;

namespace FriendToNetWebDevelopers.MicroUtilities.Test;

[TestFixture]
public class YoutubeTests
{
    private const string ValidYoutubeId = "SrN4A9rVXj0";
    //Valid YouTube https://www.youtube.com/watch?v=SrN4A9rVXj0

    [Test]
    public void YoutubeIdTest()
    {
        Assert.That(Utilities.Youtube.IsValidYoutubeId(ValidYoutubeId), Is.True);
        Assert.That(Utilities.Youtube.IsValidYoutubeId("f"), Is.False);
    }
    
    [Test]
    public void ThumbnailTest()
    {
        var client = new HttpClient();
        
        var hqDefaultOkay = Utilities.Youtube.GetYoutubeThumbnail(ValidYoutubeId, YoutubeThumbnailEnum.HqDefault);
        var hqDefaultOkayResponse = client.GetAsync(new Uri(hqDefaultOkay)).Result;
        Assert.That(hqDefaultOkayResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var maxResDefaultOkay = Utilities.Youtube.GetYoutubeThumbnail(ValidYoutubeId, YoutubeThumbnailEnum.MaxResDefault);
        var maxResDefaultOkayResponse = client.GetAsync(new Uri(maxResDefaultOkay)).Result;
        Assert.That(maxResDefaultOkayResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        Assert.Throws<BadYoutubeIdException>(() => Utilities.Youtube.GetYoutubeThumbnail("f"));
    }

    [Test]
    public void IframeTest()
    {
        var iframeOkay = Utilities.Youtube.GetYoutubeIframeUrl(ValidYoutubeId);
        var client = new HttpClient();
        var iframeOkayResponse = client.GetAsync(new Uri(iframeOkay)).Result;
        Assert.That(iframeOkayResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        Assert.Throws<BadYoutubeIdException>(() => Utilities.Youtube.GetYoutubeIframeUrl("f"));
    }
}