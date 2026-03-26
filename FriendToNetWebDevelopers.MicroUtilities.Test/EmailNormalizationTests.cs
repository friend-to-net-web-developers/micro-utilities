namespace FriendToNetWebDevelopers.MicroUtilities.Test;

using FriendToNetWebDevelopers.MicroUtilities.Enum;
using FriendToNetWebDevelopers.MicroUtilities.Models;

[TestFixture]
public class EmailNormalizationTests
{
    [Test]
    public void TryGetNormalizedValidPunyEmail_Default_AllStrategies()
    {
        // Default uses TryGetNormalizedValidEmailStrategyEnum.All (ToLower | Trim | DropTag | DropDot)
        var input = "first.last+tag@example.com";
        var expected = "firstlast@example.com";

        var result = Utilities.Email.TryGetNormalizedValidPunyEmail(input, out var punyResult, out _);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(punyResult?.Unicode, Is.EqualTo(expected));
        });
    }

    [TestCase("email@example.com", "email@example.com", TryGetNormalizedValidEmailStrategyEnum.ToLower)]
    [TestCase("  email@example.com  ", "email@example.com", TryGetNormalizedValidEmailStrategyEnum.Trim)]
    [TestCase("user+tag@example.com", "user@example.com", TryGetNormalizedValidEmailStrategyEnum.DropTag)]
    [TestCase("first.last@example.com", "firstlast@example.com", TryGetNormalizedValidEmailStrategyEnum.DropDot)]
    public void TryGetNormalizedValidPunyEmail_IndividualStrategies(string input, string expected, TryGetNormalizedValidEmailStrategyEnum strategy)
    {
        var result = Utilities.Email.TryGetNormalizedValidPunyEmail(input, strategy, out var punyResult, out _);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(punyResult?.Unicode, Is.EqualTo(expected));
        });
    }

    [Test]
    public void TryGetNormalizedValidPunyEmail_NoneStrategy()
    {
        var input = "user.name+tag@example.com";
        
        var result = Utilities.Email.TryGetNormalizedValidPunyEmail(input, TryGetNormalizedValidEmailStrategyEnum.None, out var punyResult, out _);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            // System.Net.Mail.MailAddress.Address usually lowercases the host part.
            Assert.That(punyResult?.Unicode?.ToLowerInvariant(), Is.EqualTo(input.ToLowerInvariant()));
        });
    }

    [Test]
    public void TryGetNormalizedValidPunyEmail_SkipInternalValidation()
    {
        // "email@example" is invalid by IsValidEmail (no TLD check usually fails it or host check)
        // But MailAddress.TryCreate might accept it.
        var input = "email@example";
        
        // Should fail with internal validation
        var resultWithValidation = Utilities.Email.TryGetNormalizedValidPunyEmail(input, out _, out _);
        // Should pass if we skip internal validation (assuming MailAddress accepts it)
        var resultSkippingValidation = Utilities.Email.TryGetNormalizedValidPunyEmail(input, TryGetNormalizedValidEmailStrategyEnum.All, out var punyResult, out _, true);

        Assert.Multiple(() =>
        {
            Assert.That(resultWithValidation, Is.False);
            Assert.That(resultSkippingValidation, Is.True);
            Assert.That(punyResult?.Unicode, Is.Not.Null);
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("not-an-email")]
    public void TryGetNormalizedValidPunyEmail_InvalidInputs_ReturnsFalse(string? input)
    {
        var result = Utilities.Email.TryGetNormalizedValidPunyEmail(input, out var punyResult, out _);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(punyResult, Is.Null);
        });
    }

    [Test]
    public void TryGetNormalizedValidPunyEmail_StrategyCombinations()
    {
        var input = "  user.name+tag@example.com  ";
        
        // Lower + Trim
        Utilities.Email.TryGetNormalizedValidPunyEmail(input, TryGetNormalizedValidEmailStrategyEnum.LowerAndTrim, out var lowerTrim, out _);
        Assert.That(lowerTrim?.Unicode, Is.EqualTo("user.name+tag@example.com"));

        // DropTag + DropDot
        Utilities.Email.TryGetNormalizedValidPunyEmail(input, TryGetNormalizedValidEmailStrategyEnum.DropTagAndDropDot, out var tagDot, out _);
        // Note: MailAddress.Address (used when DropTag is active) will lowercase the host
        Assert.That(tagDot?.Unicode, Is.EqualTo("username@example.com").IgnoreCase);
    }
    
    [Test]
    public void TryGetNormalizedValidPunyEmail_HandlesColons_WhenSkippingValidation()
    {
        // The code has a specific block for stripping colons
        // MailAddress.TryCreate fails if there are colons in the local part UNLESS it's quoted.
        var input = "\"user:name\"@example.com";
        
        // IsValidEmail returns false if email contains ':'
        // So we must skip internal validation to reach that block
        var result = Utilities.Email.TryGetNormalizedValidPunyEmail(input, TryGetNormalizedValidEmailStrategyEnum.All, out var punyResult, out _, skipInternalValidation: true);
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            // The code does Replace(":", "") on the local part.
            // Result will be "\"username\"@example.com" because quotes are preserved by MailAddress.Address
            Assert.That(punyResult?.Unicode, Is.EqualTo("\"username\"@example.com"));
        });
    }

    [Test]
    public void TryGetNormalizedValidPunyEmail_PunycodeDomain()
    {
        var input = "user@xn--mnchen-3ya.de";
        var expectedUnicode = "user@münchen.de";
        var expectedPuny = "user@xn--mnchen-3ya.de";

        var result = Utilities.Email.TryGetNormalizedValidPunyEmail(input, out var punyResult, out _);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(punyResult?.Unicode, Is.EqualTo(expectedUnicode));
            Assert.That(punyResult?.Punycode, Is.EqualTo(expectedPuny));
        });
    }
}
