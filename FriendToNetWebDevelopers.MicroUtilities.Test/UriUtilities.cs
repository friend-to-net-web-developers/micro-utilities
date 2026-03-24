namespace FriendToNetWebDevelopers.MicroUtilities.Test;

[TestFixture]
public class UriUtilities
{
    [Test]
    public void PathSegmentIsValid()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Utilities.Url.IsValidPathSegment("foo"), Is.True);
            Assert.That(Utilities.Url.IsValidPathSegment(null), Is.False);
            Assert.That(Utilities.Url.IsValidPathSegment(null, true), Is.False);
            Assert.That(Utilities.Url.IsValidPathSegment("", true), Is.True);
            Assert.That(Utilities.Url.IsValidPathSegment("@foo"), Is.False);
            Assert.That(Utilities.Url.IsValidPathSegment(" "), Is.False);
            Assert.That(Utilities.Url.IsValidPathSegment("Foo Bar"), Is.False);
        });
    }

    [Test]
    public void IsValidUriSlug()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Utilities.Url.IsValidUriSlug("foo"));
            Assert.That(Utilities.Url.IsValidUriSlug("foo-bar"));
            Assert.That(Utilities.Url.IsValidUriSlug(""), Is.False);
            Assert.That(Utilities.Url.IsValidUriSlug(null), Is.False);
            Assert.That(Utilities.Url.IsValidUriSlug("Foo"), Is.False);
            Assert.That(Utilities.Url.IsValidUriSlug("Foo Bar"), Is.False);
        });
    }

    [Test]
    public void IsValidUsername()
    {
        Assert.Multiple(() =>
        {
            // Null and Empty checks
            Assert.That(Utilities.Url.IsValidUsername(null), Is.False, "Null should be false by default");
            Assert.That(Utilities.Url.IsValidUsername(null, true), Is.True,
                "Null should be true if emptyIsOkay is true");
            Assert.That(Utilities.Url.IsValidUsername(""), Is.False, "Empty should be false by default");
            Assert.That(Utilities.Url.IsValidUsername("", true), Is.True,
                "Empty should be true if emptyIsOkay is true");

            // Valid RFC 3986 UserInfo characters
            Assert.That(Utilities.Url.IsValidUsername("user"), Is.True);
            Assert.That(Utilities.Url.IsValidUsername("user123"), Is.True);
            Assert.That(Utilities.Url.IsValidUsername("user.name"), Is.True);
            Assert.That(Utilities.Url.IsValidUsername("user_name"), Is.True);
            Assert.That(Utilities.Url.IsValidUsername("user-name"), Is.True);
            Assert.That(Utilities.Url.IsValidUsername("user~name"), Is.True);
            Assert.That(Utilities.Url.IsValidUsername("user:password"), Is.True, "Colon is valid in userinfo");
            Assert.That(Utilities.Url.IsValidUsername("!$&'()*+,;="), Is.True, "Sub-delims are valid");

            // Invalid characters
            Assert.That(Utilities.Url.IsValidUsername("user name"), Is.False, "Spaces are invalid");
            Assert.That(Utilities.Url.IsValidUsername("user@host"), Is.False,
                "At-sign is the delimiter, not part of username");
            Assert.That(Utilities.Url.IsValidUsername("user#fragment"), Is.False, "Hash is invalid");
            Assert.That(Utilities.Url.IsValidUsername("user/path"), Is.False, "Slash is invalid");
            Assert.That(Utilities.Url.IsValidUsername("user?query"), Is.False, "Question mark is invalid");
            Assert.That(Utilities.Url.IsValidUsername("user[bracket]"), Is.False,
                "Square brackets are invalid in userinfo");
        });
    }

    [Test]
    public void IsValidQueryParameterName()
    {
        Assert.Multiple(() =>
        {
            // Valid names
            Assert.That(Utilities.Url.IsValidQueryParameterName("foo"), Is.True);
            Assert.That(Utilities.Url.IsValidQueryParameterName("foo_bar"), Is.True);
            Assert.That(Utilities.Url.IsValidQueryParameterName("foo.bar"), Is.True);
            Assert.That(Utilities.Url.IsValidQueryParameterName("foo-bar"), Is.True);
            Assert.That(Utilities.Url.IsValidQueryParameterName("FOO123"), Is.True);
            Assert.That(Utilities.Url.IsValidQueryParameterName("_foo"), Is.True);

            // Invalid names
            Assert.That(Utilities.Url.IsValidQueryParameterName(null), Is.False);
            Assert.That(Utilities.Url.IsValidQueryParameterName(""), Is.False);
            Assert.That(Utilities.Url.IsValidQueryParameterName(" "), Is.False);
            Assert.That(Utilities.Url.IsValidQueryParameterName("foo bar"), Is.False);
            Assert.That(Utilities.Url.IsValidQueryParameterName("foo@bar"), Is.False);
            Assert.That(Utilities.Url.IsValidQueryParameterName("foo=bar"), Is.False);
            Assert.That(Utilities.Url.IsValidQueryParameterName("foo?bar"), Is.False);
            Assert.That(Utilities.Url.IsValidQueryParameterName("foo&bar"), Is.False);
            Assert.That(Utilities.Url.IsValidQueryParameterName("foo!"), Is.False);
        });
    }

    [Test]
    public void TryToConvertToSlug()
    {
        var okay = Utilities.Url.TryToConvertToSlug("foo-bar", out var fooBarOkay);
        Assert.Multiple(() =>
        {
            Assert.That(okay);
            Assert.That(fooBarOkay, Is.EqualTo("foo-bar"));
        });
        okay = Utilities.Url.TryToConvertToSlug("Foo Bar", out var fooBarOkay2);
        Assert.Multiple(() =>
        {
            Assert.That(okay);
            Assert.That(fooBarOkay2, Is.EqualTo("foo-bar"));
        });
        okay = Utilities.Url.TryToConvertToSlug("-", out var dash);
        Assert.Multiple(() =>
        {
            Assert.That(okay, Is.False);
            Assert.That(dash, Is.Empty);
        });
        okay = Utilities.Url.TryToConvertToSlug("fooBarBaz", out var fooBarBazCamel);
        Assert.Multiple(() =>
        {
            Assert.That(okay);
            Assert.That(fooBarBazCamel, Is.EqualTo("foo-bar-baz"));
        });
        okay = Utilities.Url.TryToConvertToSlug("FooBarBaz", out var fooBarBazPascal);
        Assert.Multiple(() =>
        {
            Assert.That(okay);
            Assert.That(fooBarBazPascal, Is.EqualTo("foo-bar-baz"));
        });
    }

    [Test]
    public void HasValidTld()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Utilities.Url.HasValidTopLevelDomain(new Uri("https://example.com")), Is.True);
            Assert.That(Utilities.Url.HasValidTopLevelDomain(new Uri("https://example.museum")), Is.True);
            Assert.That(Utilities.Url.HasValidTopLevelDomain(new Uri("https://example.web")), Is.False);
        });
    }

    [Test]
    public void BuildUrl_Dictionary()
    {
        var queryObject = new Dictionary<string, string>
        {
            ["foo"] = "bar",
            ["bob"] = "dole"
        };
        Assert.That(Utilities.Url.BuildUrl("/baz", queryObject), Is.EqualTo("/baz?foo=bar&bob=dole"));
    }

    [Test]
    public void BuildUrl_KVP()
    {
        var queryObject = new List<KeyValuePair<string, string>>
        {
            new("foo[]", "bar"),
            new("foo[]", "baz"),
            new("bob", "dole")
        };
        var url = Utilities.Url.BuildUrl("/baz", queryObject);
        Assert.That(url, Is.EqualTo("/baz?foo[]=bar&foo[]=baz&bob=dole"));
    }
    [Test]
    public void TryNormalizeAndPunycodeDomain()
    {
        Assert.Multiple(() =>
        {
            // Simple domain
            Assert.That(Utilities.Url.TryNormalizeAndPunycodeDomain("example.com", out var puny, out var uni), Is.True);
            Assert.That(puny, Is.EqualTo("example.com"));
            Assert.That(uni, Is.EqualTo("example.com"));

            // Internationalized domain (.ελ -> xn--qxam)
            Assert.That(Utilities.Url.TryNormalizeAndPunycodeDomain("παράδειγμα.ελ", out puny, out uni), Is.True);
            Assert.That(puny, Is.EqualTo("xn--hxajbheg2az3al.xn--qxam"));
            Assert.That(uni, Is.EqualTo("παράδειγμα.ελ"));

            // Another one from TLDs (.бг -> xn--90ae)
            Assert.That(Utilities.Url.TryNormalizeAndPunycodeDomain("сайт.бг", out puny, out uni), Is.True);
            Assert.That(puny, Is.EqualTo("xn--80aswg.xn--90ae"));
            Assert.That(uni, Is.EqualTo("сайт.бг"));

            // Normalize: trim dots and spaces
            Assert.That(Utilities.Url.TryNormalizeAndPunycodeDomain(" .example.com. ", out puny, out uni), Is.True);
            Assert.That(puny, Is.EqualTo("example.com"));

            // Invalid domain
            Assert.That(Utilities.Url.TryNormalizeAndPunycodeDomain("not a domain", out puny, out uni), Is.False);
            Assert.That(puny, Is.Null);
            Assert.That(uni, Is.Null);
        });
    }

    [Test]
    public void PunycodeDomainMatchRegex_ShouldMatchAllTlds()
    {
        var method = typeof(Utilities).GetMethod("PunycodeDomainMatchRegex",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.That(method, Is.Not.Null, "Could not find PunycodeDomainMatchRegex method via reflection.");

        var regex = (System.Text.RegularExpressions.Regex)method!.Invoke(null, null)!;

        using var reader = new StringReader(FriendToNetWebDevelopers.MicroUtilities.Database.Tlds.Domains);
        while (reader.ReadLine() is { } line)
        {
            line = line.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

            // PunycodeDomainMatchRegex expects a full domain (with dots),
            // but we can test if the TLD itself matches the segment pattern if we adapt it or just append it to a dummy domain.
            // The regex is: ^([a-z0-9](?:[a-z0-9\-]{0,61}[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9\-]{0,61}[a-z0-9])?$
            // It requires at least one dot.

            var dummyDomain = $"example.{line.ToLowerInvariant()}";
            Assert.That(regex.IsMatch(dummyDomain), Is.True, $"TLD '{line}' failed the regex match in dummy domain '{dummyDomain}'");
        }
    }
}