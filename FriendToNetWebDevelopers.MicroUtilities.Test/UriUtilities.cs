namespace FriendToNetWebDevelopers.MicroUtilities.Test;

[TestFixture]
public class UriUtilities
{
    [Test]
    public void PathSegmentIsValid()
    {
        Assert.That(Utilities.Url.PathSegmentIsValid("foo"), Is.True);
        Assert.That(Utilities.Url.PathSegmentIsValid(null), Is.False);
        Assert.That(Utilities.Url.PathSegmentIsValid(null, true), Is.False);
        Assert.That(Utilities.Url.PathSegmentIsValid("", true), Is.True);
        Assert.That(Utilities.Url.PathSegmentIsValid("@foo"), Is.False);
        Assert.That(Utilities.Url.PathSegmentIsValid(" "), Is.False);
        Assert.That(Utilities.Url.PathSegmentIsValid("Foo Bar"), Is.False);
    }

    [Test]
    public void IsValidUriSlug()
    {
        Assert.That(Utilities.Url.IsValidUriSlug("foo"));
        Assert.That(Utilities.Url.IsValidUriSlug("foo-bar"));
        Assert.That(Utilities.Url.IsValidUriSlug(""), Is.False);
        Assert.That(Utilities.Url.IsValidUriSlug(null), Is.False);
        Assert.That(Utilities.Url.IsValidUriSlug("Foo"), Is.False);
        Assert.That(Utilities.Url.IsValidUriSlug("Foo Bar"), Is.False);
    }
    
    [Test]
    public void TryToConvertToSlug()
    {
        var okay = Utilities.Url.TryToConvertToSlug("foo-bar", out var fooBarOkay);
        Assert.That(okay);
        Assert.That(fooBarOkay, Is.EqualTo("foo-bar"));
        
        okay = Utilities.Url.TryToConvertToSlug("Foo Bar", out var fooBarOkay2);
        Assert.That(okay);
        Assert.That(fooBarOkay2, Is.EqualTo("foo-bar"));
        
        okay = Utilities.Url.TryToConvertToSlug("-", out var dash);
        Assert.That(okay, Is.False);
        Assert.That(dash, Is.Empty);
        
        okay = Utilities.Url.TryToConvertToSlug("fooBarBaz", out var fooBarBazCamel);
        Assert.That(okay);
        Assert.That(fooBarBazCamel, Is.EqualTo("foo-bar-baz"));
        
        okay = Utilities.Url.TryToConvertToSlug("FooBarBaz", out var fooBarBazPascal);
        Assert.That(okay);
        Assert.That(fooBarBazPascal, Is.EqualTo("foo-bar-baz"));
        
    }
    
    [Test]
    public void HasValidTld()
    {
        Assert.That(Utilities.Url.HasValidTopLevelDomain(new Uri("https://example.com")), Is.True);
        Assert.That(Utilities.Url.HasValidTopLevelDomain(new Uri("https://example.museum")), Is.True);
        Assert.That(Utilities.Url.HasValidTopLevelDomain(new Uri("https://example.web")), Is.False);
    }
}