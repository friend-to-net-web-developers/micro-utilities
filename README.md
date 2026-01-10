# Micro Utilities by Friend to .NET Web Developers

![.NET Test](https://github.com/FriendToNetWebDevelopers/micro-utilities/actions/workflows/dotnet-test-on-pr-net.yml/badge.svg)

## Status

- **Unit Tests:** [Latest Test Results](https://github.com/FriendToNetWebDevelopers/micro-utilities/actions/workflows/dotnet-test-on-pr-net.yml)

## Summary
A set of tiny utilities to help on web projects

## Installation
`Install-Package FriendToNetWebDevelopers.MicroUtilities`

## Usage & Available Utilities

For each of these, you'll need to include this.
```csharp
using FriendToNetWebDeveloper.MicroUtilities;
```

### Email validation

This utility attempts to validate email emails by checking for formatting and also checking a
    list of valid top-level domains as provided by [icann.org](https://www.icann.org/resources/pages/tlds-2012-02-25-en).

Testing was based on this [gist](https://gist.github.com/cjaoude/fd9910626629b53c4d25).
However, it makes no attempt to accept the Strange Valid email addresses category.

```csharp
var okay = Utilities.Email.IsValidEmail("none@none.com");
//return true

okay = Utilities.Email.IsValidEmail("foo@bar");
//returns false
```

### Uri Utilities

This utility has to do with validation and generation of urls.

#### Build Absolute URL

Specifically for taking the correct portions of a URI and making them build based on whether or not
  a developer is running using localhost with a port.

```csharp
var urlString = Utilities.Url.BuildAbsoluteUrl(uri);
//                 No port included ↓
//On the server: https://example.com/some-file.jpg
//                 Has a port                ↓
//Debug on local machine: https://localhost:44328/some-file.jpg
```

#### Uri Slug Generation & Validation

Slugs are used to safely build out a url segment based on, for instance, the title of a document.
Generating them can be somewhat tricky.  These functions serve to simplify that for the developer.

Regex for valid slug: `^[a-z0-9]+(?:-[a-z0-9]+)*$`

*Validation*
```csharp
Utilities.Url.IsValidUriSlug("foo-bar");
//returns true

Utilities.Url.IsValidUriSlug("Foo Bar");
//returns false
```

*Generation*
```csharp
var okay = Utilities.Url.TryToConvertToSlug("Foo Bar", out var slug);
// okay = true
// slug = "foo-bar"

var okay = Utilities.Url.TryToConvertToSlug("-", out var slug);
// okay = false
// slug = ""

var okay = Utilities.Url.TryToConvertToSlug(null, out var slug);
// okay = false
// slug = ""
```

#### Uri Username Validation

This is used to validate usernames for use in urls as defined by [RFC 3986](http://www.faqs.org/rfcs/rfc3986.html).

```csharp
var okay = Utilities.Url.IsValidUsername("foobar");
//returns true

var okay = Utilities.Url.IsValidUsername("foo bar");
//returns false

var okay = Utilities.Url.IsValidUsername(null);
//returns false

var okay = Utilities.Url.IsValidUsername(null, true);
//returns true                    because this ↑↑↑↑ allows for null values
```

#### Uri Path Segment Validation

Determines whether a given string is a valid path segment.

```csharp
Utilities.Url.IsValidPathSegment("foo");
//returns true

Utilities.Url.IsValidPathSegment("foo bar");
//returns false

Utilities.Url.IsValidPathSegment(null);
//returns false
```

#### Uri Query Parameter Name Validation

Validates whether the provided name is a valid query parameter name.

```csharp
Utilities.Url.IsValidQueryParameterName("foo_bar");
//returns true

Utilities.Url.IsValidQueryParameterName("foo@bar");
//returns false
```

#### Url Building Based On A Query Object

Use this to take a known base url (as a string) and dynamically append a query string to it
  based on either `IDictionary<string, string>` or `IEnumerable<KeyValuePair<string, string>>`.

The dictionary is simple in that it avoid repetition.  However, the list of key value pairs can allow for multiple
 of one key.  For instance, allowing `something[]=1` and `something[]=2` which would come in
 at the server level as a list on the receiving server.

```csharp
//          This is the dictionary or enumerable object for the query ↓
var finalUrl = Utilities.Url.BuildUrl("https://api.foobar.com", queryObject);
```

#### Top-level Domain Validation

Checks if the Top-level domain within the host of the given URI is a valid domain.  Queries against the
  text [file provided by ICANN / IANA](https://data.iana.org/TLD/tlds-alpha-by-domain.txt) for
  the final check.

```csharp
Utilities.Url.HasValidTopLevelDomain(new Uri("https://foobar.com"));
//Returns true
Utilities.Url.HasValidTopLevelDomain(new Uri("https://foobar.web"));
//Returns false
```

### ID Utilities

The ID utilities are meant to quickly get, validate, and return valid id attributes.

#### Generate IDs

This is used when creating elements which need to refer to each other by id but there can be many
on the page at the same time (accordion elements, sliders, etc).

A prefix is required and included by default.  You can change what the prefix is by adding it in. As suffix may also be included.

```C#
//Generates a valid id attribute value based on a guid with a prefix of "id"
//example: Returns id02bfd4e04f0b43f9bf407d3162db9289 (generated from new Guid)
Utilities.Id.GetValidHtmlId();

//Formats various types of data into a valid id value
// types include Guid, int, uint, long, ulong
// also includes:                |    prefix   suffix
//                               ↓       ↓        ↓                 
Utilities.Id.GetValidHtmlId(4444, "foo_", "_bar");
// ↑ Returns "foo_4444_bar"
```

#### Validate IDs
This utility can also be used to validate IDs which can be specified in an unsafe way (user input).
```C#
string? nullId = null;
Utilities.Id.IsValidId(nullId);
// ↑ Returns false

Utilities.Id.IsValidId("bob dole");
// ↑ Returns false

Utilities.ID.IsValidId("bob_dole");
// ↑ Returns true
```

It can also parse an unsafe, nullable proposed id value and ensure a non-nullable string is output.
```C#
Utilities.Id.TryGetAsValidId("bob dole", TryGetValidIdDefaultStrategyEnum.EmptyOnInvalid, var out thisWillBeEmpty);
// ↑ Returns false | thisWilBeEmpty will return string.Empty - this is so that other transformations can be handled

Utilities.Id.TryGetAsValidId("foo bar", TryGetValidIdDefaultStrategyEnum.GenerateOnInvalid, var out thisWillBeAGeneratedId);
// ↑ Returns false | thisWillBeAGeneratedId will return default value from GetValidHtmlId()

Utilities.Id.TryGetAsValidId("foo_bar_baz", TryGetValidIdDefaultStrategyEnum.GenerateOnInvalid, var out thisWillBeFooBarBaz);
// ↑ Returns true | thisWillBeFooBarBaz will be "foo_bar_baz"
```

### Youtube Utilities

#### ID Validation

Checks if the given ID is valid based on matching the regex pattern: `[a-zA-Z0-9_-]{11}`

```csharp
Utilities.Youtube.IsValidYoutubeId("SrN4A9rVXj0");
// ↑ Returns true
Utilities.Youtube.IsValidYoutubeId("foo-bar");
// ↑ Returns false
```

#### Thumbnail
Retrieves the thumbnail for the given youtube id.

```csharp
var thumbnailUrl = Utilities.Youtube.GetYoutubeThumbnail("SrN4A9rVXj0");
//Returns "https://i.ytimg.com/vi/SrN4A9rVXj0/hqdefault.jpg"
thumbnailUrl = Utilities.Youtube.GetYoutubeThumbnail("SrN4A9rVXj0", YoutubeThumbnailEnum.MaxResDefault);
//Returns "https://i.ytimg.com/vi/SrN4A9rVXj0/maxresdefault.jpg"
thumbnailUrl = Utilities.Youtube.GetYoutubeThumbnail("foo-bar", YoutubeThumbnailEnum.MaxResDefault);
//Throws BadYoutubeIdException
```

#### Embed

Retrieves the url for embedding youtube on a page.

```csharp
Utilities.Youtube.GetYoutubeIframeUrl("SrN4A9rVXj0");
//Returns https://www.youtube.com/embed/SrN4A9rVXj0
Utilities.Youtube.GetYoutubeIframeUrl("foo-bar");
//Throws BadYoutubeIdException
```

## Notes & Compatibility

### .NET Version Support
This library currently supports .NET 8, .NET 9, and .NET 10. 
Please note that support for **.NET 8** and **.NET 9** is planned to be dropped in the future as they reach their end-of-life. We recommend migrating to **.NET 10** or newer for continued support and security updates.

### Thread Safety
All utility functions in this library are static and designed to be thread-safe. They do not maintain internal state that could lead to race conditions.

### Performance
Many of these utilities use `GeneratedRegex` for optimal performance. This ensures that regex patterns are compiled at build time, reducing overhead during execution.

### TLD Updates
The `HasValidTopLevelDomain` and `IsValidEmail` functions rely on a list of TLDs provided by ICANN/IANA. This list is updated periodically within the library to ensure accuracy.

