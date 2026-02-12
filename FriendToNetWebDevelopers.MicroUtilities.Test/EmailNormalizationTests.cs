namespace FriendToNetWebDevelopers.MicroUtilities.Test;

using FriendToNetWebDevelopers.MicroUtilities.Enum;

[TestFixture]
public class EmailNormalizationTests
{
    [Test]
    public void TryGetNormalizedValidEmail_Default_AllStrategies()
    {
        // Default uses TryGetNormalizedValidEmailStrategyEnum.All (ToLower | Trim | DropTag | DropDot)
        var input = "first.last+tag@example.com";
        var expected = "firstlast@example.com";

        var result = Utilities.Email.TryGetNormalizedValidEmail(input, out var normalized);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(normalized, Is.EqualTo(expected));
        });
    }

    [TestCase("email@example.com", "email@example.com", TryGetNormalizedValidEmailStrategyEnum.ToLower)]
    [TestCase("  email@example.com  ", "email@example.com", TryGetNormalizedValidEmailStrategyEnum.Trim)]
    [TestCase("user+tag@example.com", "user@example.com", TryGetNormalizedValidEmailStrategyEnum.DropTag)]
    [TestCase("first.last@example.com", "firstlast@example.com", TryGetNormalizedValidEmailStrategyEnum.DropDot)]
    public void TryGetNormalizedValidEmail_IndividualStrategies(string input, string expected, TryGetNormalizedValidEmailStrategyEnum strategy)
    {
        var result = Utilities.Email.TryGetNormalizedValidEmail(input, out var normalized, strategy);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(normalized, Is.EqualTo(expected));
        });
    }

    [Test]
    public void TryGetNormalizedValidEmail_NoneStrategy()
    {
        var input = "user.name+tag@example.com";
        // Even with None, MailAddress.TryCreate might normalize some aspects (like casing of host depending on implementation, but usually it keeps it if not specified)
        // Actually, the implementation says: normalized = parsedResult.Address;
        // MailAddress.Address typically lowercases the host.
        
        var result = Utilities.Email.TryGetNormalizedValidEmail(input, out var normalized, TryGetNormalizedValidEmailStrategyEnum.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            // System.Net.Mail.MailAddress.Address usually lowercases the host part.
            Assert.That(normalized?.ToLowerInvariant(), Is.EqualTo(input.ToLowerInvariant()));
        });
    }

    [Test]
    public void TryGetNormalizedValidEmail_SkipInternalValidation()
    {
        // "email@example" is invalid by IsValidEmail (no TLD check usually fails it or host check)
        // But MailAddress.TryCreate might accept it.
        var input = "email@example";
        
        // Should fail with internal validation
        var resultWithValidation = Utilities.Email.TryGetNormalizedValidEmail(input, out _);
        // Should pass if we skip internal validation (assuming MailAddress accepts it)
        var resultSkippingValidation = Utilities.Email.TryGetNormalizedValidEmail(input, out var normalized, true);

        Assert.Multiple(() =>
        {
            Assert.That(resultWithValidation, Is.False);
            Assert.That(resultSkippingValidation, Is.True);
            Assert.That(normalized, Is.Not.Null);
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("not-an-email")]
    public void TryGetNormalizedValidEmail_InvalidInputs_ReturnsFalse(string? input)
    {
        var result = Utilities.Email.TryGetNormalizedValidEmail(input, out var normalized);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(normalized, Is.Null);
        });
    }

    [Test]
    public void TryGetNormalizedValidEmail_StrategyCombinations()
    {
        var input = "  user.name+tag@example.com  ";
        
        // Lower + Trim
        Utilities.Email.TryGetNormalizedValidEmail(input, out var lowerTrim, TryGetNormalizedValidEmailStrategyEnum.LowerAndTrim);
        Assert.That(lowerTrim, Is.EqualTo("user.name+tag@example.com"));

        // DropTag + DropDot
        Utilities.Email.TryGetNormalizedValidEmail(input, out var tagDot, TryGetNormalizedValidEmailStrategyEnum.DropTagAndDropDot);
        // Note: MailAddress.Address (used when DropTag is active) will lowercase the host
        Assert.That(tagDot, Is.EqualTo("username@example.com").IgnoreCase);
    }
    
    [Test]
    public void TryGetNormalizedValidEmail_HandlesColons_WhenSkippingValidation()
    {
        // The code has a specific block for stripping colons
        // MailAddress.TryCreate fails if there are colons in the local part UNLESS it's quoted.
        var input = "\"user:name\"@example.com";
        
        // IsValidEmail returns false if email contains ':'
        // So we must skip internal validation to reach that block
        var result = Utilities.Email.TryGetNormalizedValidEmail(input, out var normalized, skipInternalValidation: true);
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            // The code does Replace(":", "") on the local part.
            // Result will be "\"username\"@example.com" because quotes are preserved by MailAddress.Address
            Assert.That(normalized, Is.EqualTo("\"username\"@example.com"));
        });
    }
}
