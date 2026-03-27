using FriendToNetWebDevelopers.MicroUtilities;
using FriendToNetWebDevelopers.MicroUtilities.Models.TextAnnotator;

namespace FriendToNetWebDevelopers.MicroUtilities.Test;

[TestFixture]
public class TextAnnotatorTests
{
    [Test]
    public void Annotate_EmptyString_ReturnsEmptyEnumerable()
    {
        var result = Utilities.TextAnnotator.Annotate(string.Empty);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Annotate_WithLettersAndDigits_ReturnsCorrectTypes()
    {
        var input = "a1 B2";
        var result = Utilities.TextAnnotator.Annotate(input).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(5));
            Assert.That(result[0].Type, Is.EqualTo(TextCharacterType.Letter));
            Assert.That(result[0].Character, Is.EqualTo("a"));
            Assert.That(result[1].Type, Is.EqualTo(TextCharacterType.Digit));
            Assert.That(result[1].Character, Is.EqualTo("1"));
            Assert.That(result[2].Type, Is.EqualTo(TextCharacterType.Whitespace));
            Assert.That(result[3].Type, Is.EqualTo(TextCharacterType.Letter));
            Assert.That(result[4].Type, Is.EqualTo(TextCharacterType.Digit));
        });
    }

    [Test]
    public void Annotate_WithSpecialChars_ReturnsCorrectTypes()
    {
        var input = "!@#";
        var result = Utilities.TextAnnotator.Annotate(input).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result.All(t => t.Type == TextCharacterType.Special), Is.True);
        });
    }

    [Test]
    public void Annotate_WithBmpUnicode_ReturnsUnicodeType()
    {
        var input = "\u00A9"; // ©
        var result = Utilities.TextAnnotator.Annotate(input).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Type, Is.EqualTo(TextCharacterType.Unicode));
            Assert.That(result[0].Character, Is.EqualTo("\u00A9"));
            Assert.That(result[0].UnicodeEscape, Is.EqualTo("\\u00A9"));
        });
    }

    [Test]
    public void Annotate_WithSupplementaryUnicode_ReturnsUnicodeTypeAndHandlesSurrogatePairs()
    {
        var input = "\U0001F600"; // 😀
        var result = Utilities.TextAnnotator.Annotate(input).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Type, Is.EqualTo(TextCharacterType.Unicode));
            Assert.That(result[0].Character, Is.EqualTo("\U0001F600"));
            Assert.That(result[0].Character.Length, Is.EqualTo(2)); // Surrogate pair
            Assert.That(result[0].CodePoint, Is.EqualTo(0x1F600));
            Assert.That(result[0].UnicodeEscape, Is.EqualTo("\\U0001F600"));
        });
    }

    [Test]
    public void Annotate_MixedInput_MaintainsCorrectIndices()
    {
        var input = "A\U0001F600B";
        var result = Utilities.TextAnnotator.Annotate(input).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0].Character, Is.EqualTo("A"));
            Assert.That(result[0].Index, Is.EqualTo(0));
            Assert.That(result[1].Character, Is.EqualTo("\U0001F600"));
            Assert.That(result[1].Index, Is.EqualTo(1));
            Assert.That(result[2].Character, Is.EqualTo("B"));
            Assert.That(result[2].Index, Is.EqualTo(3)); // After surrogate pair
        });
    }
}
