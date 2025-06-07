using FriendToNetWebDevelopers.MicroUtilities.Enum;
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
        Assert.That(Utilities.Id.IsValidId("id"), Is.True);

        var okayResult =
            Utilities.Id.TryGetAsValidId("id", TryGetValidIdDefaultStrategyEnum.EmptyOnInvalid, out var okayIdResult);
        Assert.Multiple(() =>
        {
            Assert.That(okayResult, Is.True);
            Assert.That(okayIdResult, Is.EqualTo("id"));
        });
    }

    [Test]
    public void Id_Invalid()
    {
        Assert.Throws<BadIdPrefixException>(() => { Utilities.Id.GetValidHtmlId(" "); });
        Assert.Throws<BadIdPrefixException>(() => { Utilities.Id.GetValidHtmlId(""); });
        Assert.Throws<BadIdFormatException>(() => { Utilities.Id.GetValidHtmlId(Guid.NewGuid(), "id", "@"); });
        Assert.That(Utilities.Id.IsValidId("bob dole"), Is.False);
        
        var failedResult = Utilities.Id.TryGetAsValidId("bob dole", TryGetValidIdDefaultStrategyEnum.GenerateOnInvalid, out var failedIdResult);
        Assert.Multiple(() =>
        {
            Assert.That(failedResult, Is.False);
            Assert.That(Utilities.Id.IsValidId(failedIdResult), Is.True);
        });
    }
}