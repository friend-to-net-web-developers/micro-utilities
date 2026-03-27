using System.Net.Mail;
using FriendToNetWebDevelopers.MicroUtilities.Extensions;
using FriendToNetWebDevelopers.MicroUtilities.Models.EmailAnnotator;

namespace FriendToNetWebDevelopers.MicroUtilities.Test;

[TestFixture]
public class AddressAnnotatorTests
{
    [Test]
    public void Annotate_SimpleEmail_ReturnsCorrectAnnotation()
    {
        var email = "test@example.com";
        var annotation = Utilities.AddressAnnotator.Annotate(email);

        Assert.Multiple(() =>
        {
            Assert.That(annotation.Raw, Is.EqualTo(email));
            Assert.That(annotation.LocalPart, Is.EqualTo("test"));
            Assert.That(annotation.Domain, Is.EqualTo("example.com"));
            Assert.That(annotation.Mode, Is.EqualTo(InputMode.Email));
            Assert.That(annotation.ContainsUnicode, Is.False);
            Assert.That(annotation.ContainsSuspiciousChars, Is.False);
            Assert.That(annotation.LocalTokens, Has.Count.EqualTo(4));
            Assert.That(annotation.DomainTokens, Has.Count.EqualTo(11));
        });
    }

    [Test]
    public void Annotate_UnicodeEmail_DetectsUnicode()
    {
        var email = "あいうえお@example.com";
        var annotation = Utilities.AddressAnnotator.Annotate(email);

        Assert.Multiple(() =>
        {
            Assert.That(annotation.ContainsUnicode, Is.True);
            Assert.That(annotation.LocalTokens.Any(t => t.Type == CharacterType.Unicode), Is.True);
        });
    }

    [Test]
    public void Annotate_EmailWithSubAddress_IdentifiesStructuralChar()
    {
        var email = "user+tag@example.com";
        var annotation = Utilities.AddressAnnotator.Annotate(email);

        var plusToken = annotation.LocalTokens.FirstOrDefault(t => t.Char == "+");
        Assert.Multiple(() =>
        {
            Assert.That(plusToken, Is.Not.Null);
            Assert.That(plusToken!.Type, Is.EqualTo(CharacterType.EmailStructural));
        });
    }

    [Test]
    public void Annotate_UriMode_IdentifiesPercentEncoded()
    {
        var userInfo = "user%20name";
        // We use the string overload with Uri mode
        var annotation = Utilities.AddressAnnotator.Annotate(userInfo, InputMode.Uri);

        Assert.Multiple(() =>
        {
            Assert.That(annotation.Mode, Is.EqualTo(InputMode.Uri));
            Assert.That(annotation.LocalTokens.Any(t => t.Type == CharacterType.PercentEncoded), Is.True);
        });
    }

    [Test]
    public void Annotate_SuspiciousHomoglyph_DetectsSuspicious()
    {
        // Using a Cyrillic 'а' (U+0430) instead of Latin 'a' (U+0061)
        var email = "tеst@example.com"; // The 'е' is Cyrillic IE U+0435
        var annotation = Utilities.AddressAnnotator.Annotate(email);

        Assert.Multiple(() =>
        {
            Assert.That(annotation.ContainsSuspiciousChars, Is.True);
            Assert.That(annotation.LocalTokens.Any(t => t.IsSuspicious), Is.True);
        });
    }

    [Test]
    public void Annotate_InvalidChars_DetectsInvalid()
    {
        var email = "test\0@example.com";
        var annotation = Utilities.AddressAnnotator.Annotate(email);

        Assert.Multiple(() =>
        {
            Assert.That(annotation.ContainsInvalidChars, Is.True);
            Assert.That(annotation.LocalTokens.Any(t => t.Type == CharacterType.Invalid), Is.True);
        });
    }

    [Test]
    public void AnnotateUserInfo_WithPassword_DropsPassword()
    {
        var uri = new Uri("https://user:password@example.com");
        var annotation = Utilities.AddressAnnotator.AnnotateUserInfo(uri);

        Assert.Multiple(() =>
        {
            Assert.That(annotation.Raw, Is.EqualTo("user"));
            Assert.That(annotation.LocalPart, Is.EqualTo("user"));
            Assert.That(annotation.Domain, Is.Empty);
        });
    }

    [Test]
    public void AnnotateHost_Works()
    {
        var uri = new Uri("https://example.com");
        var annotation = Utilities.AddressAnnotator.AnnotateHost(uri);

        Assert.That(annotation.Raw, Is.EqualTo("example.com"));
    }

    [Test]
    public void StringExtensions_Annotate_Works()
    {
        var email = "test@example.com";
        var annotation = email.Annotate(InputMode.Email);

        Assert.That(annotation.LocalPart, Is.EqualTo("test"));
    }

    [Test]
    public void MailAddressExtensions_Annotate_Works()
    {
        var mail = new MailAddress("test@example.com");
        var annotation = mail.Annotate();

        Assert.That(annotation.LocalPart, Is.EqualTo("test"));
    }
}