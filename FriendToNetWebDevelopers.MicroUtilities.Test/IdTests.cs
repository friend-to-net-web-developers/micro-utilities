using FriendToNetWebDevelopers.MicroUtilities;
using FriendToNetWebDevelopers.MicroUtilities.Exception;
using NUnit.Framework.Constraints;

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
        Assert.That(id, Does.EndWith("_stuff"));
    }

    [Test]
    public void Id_Invalid()
    {
        Assert.Throws<BadIdPrefixException>(() =>
        {
            Utilities.Id.GetValidHtmlId(" ");
        });
        Assert.Throws<BadIdPrefixException>(() =>
        {
            Utilities.Id.GetValidHtmlId("");
        });
        Assert.Throws<BadIdFormatException>(() =>
        {
            Utilities.Id.GetValidHtmlId("id", "@");
        });
    }
}