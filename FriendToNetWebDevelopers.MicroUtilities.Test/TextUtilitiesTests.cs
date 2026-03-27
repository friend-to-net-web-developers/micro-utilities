using FriendToNetWebDevelopers.MicroUtilities;

namespace FriendToNetWebDevelopers.MicroUtilities.Test;

[TestFixture]
public class TextUtilitiesTests
{
    [Test]
    public void EncodeUnicodeEscapes_WithAsciiOnly_ReturnsSameString()
    {
        var input = "Hello, World! 123";
        var result = Utilities.Text.EncodeUnicodeEscapes(input);
        Assert.That(result, Is.EqualTo(input));
    }

    [Test]
    public void EncodeUnicodeEscapes_WithBmpUnicode_ReturnsUxxxxEscapes()
    {
        var input = "A\u00A9\u0394"; // A, Copyright, Delta
        var result = Utilities.Text.EncodeUnicodeEscapes(input);
        Assert.That(result, Is.EqualTo("A\\u00A9\\u0394"));
    }

    [Test]
    public void EncodeUnicodeEscapes_WithSupplementaryUnicode_ReturnsUXXXXXXXXEscapes()
    {
        var input = "\U0001F600"; // 😀 Grinning Face
        var result = Utilities.Text.EncodeUnicodeEscapes(input);
        Assert.That(result, Is.EqualTo("\\U0001F600"));
    }

    [Test]
    public void EncodeUnicodeEscapes_Mixed_ReturnsCorrectEscapes()
    {
        var input = "A\u00A9\U0001F600B";
        var result = Utilities.Text.EncodeUnicodeEscapes(input);
        Assert.That(result, Is.EqualTo("A\\u00A9\\U0001F600B"));
    }

    [Test]
    public void DecodeUnicodeEscapes_WithUxxxx_ReturnsCorrectCharacters()
    {
        var input = "A\\u00A9\\u0394";
        var result = Utilities.Text.DecodeUnicodeEscapes(input);
        Assert.That(result, Is.EqualTo("A\u00A9\u0394"));
    }

    [Test]
    public void DecodeUnicodeEscapes_WithUXXXXXXXX_ReturnsCorrectCharacters()
    {
        var input = "\\U0001F600";
        var result = Utilities.Text.DecodeUnicodeEscapes(input);
        Assert.That(result, Is.EqualTo("\U0001F600"));
    }

    [Test]
    public void DecodeUnicodeEscapes_Mixed_ReturnsCorrectCharacters()
    {
        var input = "A\\u00A9\\U0001F600B";
        var result = Utilities.Text.DecodeUnicodeEscapes(input);
        Assert.That(result, Is.EqualTo("A\u00A9\U0001F600B"));
    }

    [Test]
    public void DecodeUnicodeEscapes_WithNoEscapes_ReturnsSameString()
    {
        var input = "Hello, World! 123";
        var result = Utilities.Text.DecodeUnicodeEscapes(input);
        Assert.That(result, Is.EqualTo(input));
    }
}
