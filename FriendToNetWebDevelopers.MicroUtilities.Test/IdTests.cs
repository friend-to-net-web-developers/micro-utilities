using FriendToNetWebDevelopers.MicroUtilities.Exception;

namespace FriendToNetWebDevelopers.MicroUtilities.Test;

[TestFixture]
public class IdTests
{
    [Test]
    public void Id_Valid()
    {
        var id = Utilities.Id.GetValidHtmlId();
        Assert.That(id, Does.StartWith("id"));
        id = Utilities.Id.GetValidHtmlId(65535, "id", "_stuff");
        Assert.That(id, Does.StartWith("id"));
        Assert.Multiple(() =>
        {
            Assert.That(id, Does.EndWith("_stuff"));
            Assert.That(Utilities.Id.GetValidHtmlId((uint)1), Is.EqualTo("id1"));
            Assert.That(Utilities.Id.GetValidHtmlId((long)1), Is.EqualTo("id1"));
            Assert.That(Utilities.Id.GetValidHtmlId((ulong)1), Is.EqualTo("id1"));
        });
    }

    [Test]
    public void Id_Invalid()
    {
        Assert.Throws<BadIdPrefixException>(() => { Utilities.Id.GetValidHtmlId(" "); });
        Assert.Throws<BadIdPrefixException>(() => { Utilities.Id.GetValidHtmlId(""); });
        Assert.Throws<BadIdFormatException>(() => { Utilities.Id.GetValidHtmlId(Guid.NewGuid(),"id", "@"); });
    }
}