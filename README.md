# Micro Utilities by Friend to .NET Web Developers
## Summary
A set of tiny utilities to help on web projects

## Installation
`Install-Package FriendToNetWebDevelopers.HtmlAttributeDictionary`

## Usage & Available Utilities

For each of these, you'll need to include this.
```csharp
using FriendToNetWebDeveloper.MicroUtilities;
```

### Dynamically Generated Html IDs

This is used when creating elements which need to refer to each other by id but there can be many
   on the page at the same time (accordion elements, sliders, etc).

A prefix is required and included by default.  You can change what the prefix is by adding it in.

```csharp
//Use this when you don't have any information to go off of or don't want that to be public
var id = Utilities.GetValidHtmlId();
//Generates something like this: "id12810faad82640c09a9025c9f4909345"

//Use this to pass in an existing GUID
id = Utilities.GetValidHtmlId(new Guid("aeda0af0-dbc7-4d83-a568-557d27074781"));
//Generates: "idaeda0af0dbc74d83a568557d27074781"

//                          Prefix ↓   Suffix ↓
id = Utilities.GetValidHtmlId(44, "id__", "__suffix");
//Generates: "id__44__suffix"
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

### Youtube Utilities

#### ID Validation

Checks if the given ID is valid based on matching the regex pattern: `[a-zA-Z0-9_-]{11}`

```csharp
Utilities.Youtube.IsValidYoutubeId("SrN4A9rVXj0");
//Returns true
Utilities.Youtube.IsValidYoutubeId("foo-bar");
//Returns false
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

