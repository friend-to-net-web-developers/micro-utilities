# Micro Utilities by Friend to .NET Web Developers

![.NET Test](https://github.com/friend-to-net-web-developers/micro-utilities/actions/workflows/dotnet-test-on-pr-net.yml/badge.svg)

## Status

- **Unit Tests:** [Latest Test Results](https://github.com/friend-to-net-web-developers/micro-utilities/actions/workflows/dotnet-test-on-pr-net.yml)

## Summary

A set of small, focused utilities for .NET web projects covering email validation and normalization, URI handling, HTML ID generation, YouTube URL helpers, variable name conversion, and Unicode text analysis.

## Installation

```
Install-Package FriendToNetWebDevelopers.MicroUtilities
```

## Migration from 1.x

Version 2.0.0 contains the following breaking changes:

- **`YoutubeThumbnailEnum` renamed to `YoutubeThumbnail`** — the `Enum` suffix was misleading; this type is a smart enum (type-safe class), not a `System.Enum`.
- **`CharacterToken.IsEmailStructural` renamed to `IsStructural`** — the old name was inaccurate; the field applies to both email and URI structural characters.
- **`GetVariableName` and `GetClassName` now throw `ArgumentNullException` for null arguments** — previously returned `string.Empty` silently.
- **`BuildAbsoluteUrl` logic corrected** — the port-inclusion/exclusion branches were inverted in 1.x. If you were working around this bug, remove the workaround.
- **.NET 8 and .NET 9 support dropped** — .NET 10 is required.

## Usage

All utilities are accessed via the static `Utilities` class:

```csharp
using FriendToNetWebDevelopers.MicroUtilities;
```

Extension methods require an additional using:

```csharp
using FriendToNetWebDevelopers.MicroUtilities.Extensions;
```

---

### Email Validation & Normalization

Validates email addresses by checking format, hostname structure, and top-level domain against the [IANA TLD list](https://data.iana.org/TLD/tlds-alpha-by-domain.txt). Internationalized domain names (IDN) are fully supported via Punycode normalization.

> Validation testing was based on [this gist](https://gist.github.com/cjaoude/fd9910626629b53c4d25). The "Strange Valid" category is intentionally not supported.

#### Validation

```csharp
Utilities.Email.IsValidEmail("user@example.com");
// returns true

Utilities.Email.IsValidEmail("foo@bar");
// returns false
```

> **Note on case sensitivity:** Local parts are case-sensitive per RFC 5321. `User@example.com` and `user@example.com` are technically distinct addresses. `IsValidEmail` will return `false` for an address whose local part casing differs from what the URI parser reconstructs. Normalize before validating if case-insensitive matching is required.

#### Normalization (Punycode & Unicode)

`TryGetNormalizedValidPunyEmail` is the primary normalization entry point. It returns both the Unicode and Punycode canonical forms of the address, along with a character-level annotation and a suspicious-input flag.

```csharp
// Default normalization — applies all strategies: ToLower, Trim, DropTag, DropDot
var okay = Utilities.Email.TryGetNormalizedValidPunyEmail(
    "  First.Last+tag@München.de  ",
    out var punyResult,
    out var annotation);
// okay = true
// punyResult.Unicode  = "firstlast@münchen.de"
// punyResult.Punycode = "firstlast@xn--mnchen-3ya.de"
// punyResult.IsSuspicious = false

// Drop sub-addressing tag only
okay = Utilities.Email.TryGetNormalizedValidPunyEmail(
    "user+tag@example.com",
    TryGetNormalizedValidEmailStrategyEnum.DropTag,
    out punyResult,
    out _);
// okay = true
// punyResult.Unicode = "user@example.com"

// Lowercase and trim only
okay = Utilities.Email.TryGetNormalizedValidPunyEmail(
    "  User@Example.com  ",
    TryGetNormalizedValidEmailStrategyEnum.LowerAndTrim,
    out punyResult,
    out _);
// okay = true
// punyResult.Unicode = "user@example.com"

// Skip TLD validation — useful for internal/custom domains
okay = Utilities.Email.TryGetNormalizedValidPunyEmail(
    "admin@internal",
    TryGetNormalizedValidEmailStrategyEnum.All,
    out punyResult,
    out _,
    skipInternalValidation: true);
// okay = true
// punyResult.Unicode = "admin@internal"
```

> **Provider-specific strategies:** `DropTag` (plus sub-addressing) and `DropDot` are Gmail-specific behaviours. Do not apply them when normalizing addresses for providers where `+` is not a sub-address delimiter (e.g. Fastmail, ProtonMail) or where dots are significant. Use the strategy overload and omit those flags for general-purpose normalization.

Available strategy flags and pre-built combinations are defined in `TryGetNormalizedValidEmailStrategyEnum`.

---

### Address Annotation & Security Analysis

`AddressAnnotator` provides character-level inspection of email addresses and URI components, identifying suspicious characters (homoglyphs), Unicode usage, and invalid characters. Useful for detecting phishing and spoofing attempts.

```csharp
var email = "tеst@example.com"; // Contains Cyrillic 'е' — a homoglyph of ASCII 'e'
var annotation = Utilities.AddressAnnotator.Annotate(email);

if (annotation.ContainsSuspiciousChars)
{
    var suspiciousToken = annotation.LocalTokens.First(t => t.IsSuspicious);
    // suspiciousToken.Char       = "е"
    // suspiciousToken.HomoglyphOf = "e"
}

// annotation.LocalPart           = "tеst"
// annotation.Domain              = "example.com"
// annotation.ContainsUnicode     = true
// annotation.Mode                = InputMode.Email
```

> **Homoglyph coverage:** The built-in table covers high-priority confusables from [Unicode TR39](https://www.unicode.org/reports/tr39/#confusables) — Cyrillic, Greek, and Latin Extended lookalikes. It is intentionally partial; extend it in your own code for broader coverage.

Extension methods are available on `string`, `Uri`, and `MailAddress`:

```csharp
var annotation    = "user@example.com".Annotate(InputMode.Email);
var uriAnnotation = new Uri("https://user:pass@example.com").AnnotateUserInfo();
```

---

### URI Utilities

#### Build Absolute URL

Builds an absolute URL string from a URI, automatically including or excluding the port based on whether the application is running in debug mode.

```csharp
var urlString = Utilities.Url.BuildAbsoluteUrl(uri);
// Debug / localhost:  https://localhost:44328/some-file.jpg  (port included)
// Production:         https://example.com/some-file.jpg      (port stripped)
```

#### Slug Generation & Validation

Slugs are lowercase, hyphen-separated URL path segments. Valid slugs match `^[a-z0-9]+(?:-[a-z0-9]+)*$`.

```csharp
// Validation
Utilities.Url.IsValidUriSlug("foo-bar");  // true
Utilities.Url.IsValidUriSlug("Foo Bar");  // false

// Generation
Utilities.Url.TryToConvertToSlug("Foo Bar", out var slug);
// returns true, slug = "foo-bar"

Utilities.Url.TryToConvertToSlug("-", out var slug);
// returns false, slug = ""

Utilities.Url.TryToConvertToSlug(null, out var slug);
// returns false, slug = ""
```

#### Username Validation

Validates a userinfo component per [RFC 3986](https://www.rfc-editor.org/rfc/rfc3986).

```csharp
Utilities.Url.IsValidUsername("foobar");       // true
Utilities.Url.IsValidUsername("foo bar");      // false
Utilities.Url.IsValidUsername(null);           // false
Utilities.Url.IsValidUsername(null, true);     // true — null explicitly allowed
```

#### Path Segment Validation

```csharp
Utilities.Url.IsValidPathSegment("foo");      // true
Utilities.Url.IsValidPathSegment("foo bar");  // false
Utilities.Url.IsValidPathSegment(null);       // false
```

#### Query Parameter Name Validation

```csharp
Utilities.Url.IsValidQueryParameterName("foo_bar");  // true
Utilities.Url.IsValidQueryParameterName("foo@bar");  // false
```

#### URL Building from a Query Object

Appends a URL-encoded query string to a base URL. Accepts either `IDictionary<string, string>` (unique keys) or `IEnumerable<KeyValuePair<string, string>>` (duplicate keys allowed, useful for array-style parameters like `ids[]=1&ids[]=2`).

```csharp
var finalUrl = Utilities.Url.BuildUrl("https://api.example.com", queryObject);
```

#### Top-Level Domain Validation

Validates the TLD of a URI's host against the [IANA TLD list](https://data.iana.org/TLD/tlds-alpha-by-domain.txt).

```csharp
Utilities.Url.HasValidTopLevelDomain(new Uri("https://foobar.com"));  // true
Utilities.Url.HasValidTopLevelDomain(new Uri("https://foobar.web"));  // false
```

#### Punycode & IDN Domain Normalization

Normalizes a domain name and produces both its Punycode and Unicode representations.

```csharp
Utilities.Url.TryNormalizeAndPunycodeDomain("παράδειγμα.ελ", out var puny, out var uni);
// puny = "xn--hxajbheg2az3al.xn--qxam"
// uni  = "παράδειγμα.ελ"

Utilities.Url.TryNormalizeAndPunycodeDomain(" .EXAMPLE.com. ", out puny, out uni);
// puny = "example.com"
// uni  = "example.com"
```

---

### ID Utilities

Generates, validates, and coerces valid HTML `id` attribute values. Useful when multiple dynamic elements on a page (accordions, sliders, tabs) need stable, unique IDs that reference each other.

#### Generation

A prefix is required (defaults to `"id"`). An optional suffix may also be supplied. Supported base types: `Guid`, `int`, `uint`, `long`, `ulong`.

```csharp
// From a new Guid — returns e.g. "id02bfd4e04f0b43f9bf407d3162db9289"
Utilities.Id.GetValidHtmlId();

// From a numeric value with custom prefix and suffix
Utilities.Id.GetValidHtmlId(4444, "foo_", "_bar");
// returns "foo_4444_bar"
```

#### Validation

```csharp
Utilities.Id.IsValidId(null);       // false
Utilities.Id.IsValidId("bob dole"); // false
Utilities.Id.IsValidId("bob_dole"); // true
```

Single-letter IDs are valid:

```csharp
Utilities.Id.IsValidId("a"); // true
```

#### Coercion with Fallback

Returns a guaranteed non-null string using a fallback strategy when the input is invalid:

```csharp
Utilities.Id.TryGetAsValidId(
    "bob dole",
    TryGetValidIdDefaultStrategyEnum.EmptyOnInvalid,
    out var result);
// returns false, result = ""

Utilities.Id.TryGetAsValidId(
    "foo bar",
    TryGetValidIdDefaultStrategyEnum.GenerateOnInvalid,
    out result);
// returns false, result = generated id from GetValidHtmlId()

Utilities.Id.TryGetAsValidId(
    "foo_bar_baz",
    TryGetValidIdDefaultStrategyEnum.GenerateOnInvalid,
    out result);
// returns true, result = "foo_bar_baz"
```

---

### YouTube Utilities

#### ID Validation

Checks that a string is a valid YouTube video ID — exactly 11 characters matching `^[a-zA-Z0-9_-]{11}$`.

```csharp
Utilities.Youtube.IsValidYoutubeId("SrN4A9rVXj0");  // true
Utilities.Youtube.IsValidYoutubeId("foo-bar");       // false
```

#### Thumbnail URLs

```csharp
// HQ default thumbnail (always available)
Utilities.Youtube.GetYoutubeThumbnail("SrN4A9rVXj0");
// returns "https://i.ytimg.com/vi/SrN4A9rVXj0/hqdefault.jpg"

// Max resolution thumbnail (may not be available for all videos)
Utilities.Youtube.GetYoutubeThumbnail("SrN4A9rVXj0", YoutubeThumbnail.MaxResDefault);
// returns "https://i.ytimg.com/vi/SrN4A9rVXj0/maxresdefault.jpg"

// Invalid ID throws
Utilities.Youtube.GetYoutubeThumbnail("foo-bar", YoutubeThumbnail.MaxResDefault);
// throws BadYoutubeIdException
```

#### Embed URL

```csharp
Utilities.Youtube.GetYoutubeIframeUrl("SrN4A9rVXj0");
// returns "https://www.youtube.com/embed/SrN4A9rVXj0"

Utilities.Youtube.GetYoutubeIframeUrl("foo-bar");
// throws BadYoutubeIdException
```

---

### Variable & Class Naming Utilities

Identifies, converts, and generates variable and class names across naming conventions.

Supported conventions: `CamelCase`, `PascalCase`, `SnakeCase`, `ScreamingSnakeCase`, `KebabCase`, `TrainCase`, `Unicase`, `TrollCase`, `TitleWords`, `SentenceWords`.

> **Acronym casing:** When converting to mixed-case formats, word tails are lowercased. `HTMLParser` → `HtmlParser` (PascalCase) or `htmlParser` (camelCase). This is consistent with common .NET conventions (`HtmlEncoder`, `JsonSerializer`) and is intentional.

> **Unicase / TrollCase inputs:** Word boundaries cannot be recovered from all-lower or all-upper input. Such inputs are treated as a single word when converting to other formats.

#### Format Detection

```csharp
Utilities.Variable.GetVariableFormat("snake_case_example");
// returns ResultsVariableNameTypeEnum.SnakeCase

"camelCase".IsVariableName(ResultsVariableNameTypeEnum.CamelCase);
// returns true
```

#### Conversion

```csharp
Utilities.Variable.ConvertToPascalCase("hello-world").result;
// returns "HelloWorld"

"This is a test".ConvertTo(RequestedVariableNameTypeEnum.CamelCase).result;
// returns "thisIsATest"

Utilities.Variable.ConvertToTitleWords("a tale of two cities").result;
// returns "A Tale of Two Cities"

Utilities.Variable.ConvertToSentenceWords("snake_case_variable").result;
// returns "Snake case variable"
```

#### Naming from Reflection Metadata

```csharp
var myInstance = new MyCustomClass();

myInstance.ToVariableName();
// returns "myCustomClass"

typeof(MyCustomClass).ToClassName();
// returns "MyCustomClass"

typeof(List<int>).ToVariableName();
// returns "list"  (generic arity suffix stripped)
```

> `GetVariableName` and `GetClassName` throw `ArgumentNullException` for null arguments.

---

### Text & Unicode Utilities

Character-level analysis and Unicode-safe encoding/decoding. Handles surrogate pairs (supplementary plane characters such as emoji) correctly as single units.

#### Unicode Escape Encoding & Decoding

Converts between Unicode characters and ASCII-compatible escape sequences (`\uXXXX` for BMP characters, `\UXXXXXXXX` for supplementary plane characters).

```csharp
Utilities.Text.EncodeUnicodeEscapes("A©😀");
// returns "A\u00A9\U0001F600"

Utilities.Text.DecodeUnicodeEscapes("A\\u00A9\\U0001F600");
// returns "A©😀"

// Extension methods
"A©😀".ToUnicodeEscapedAscii();
"A\\u00A9\\U0001F600".ToUnicodeDecoded();
```

#### Text Character Annotation

Tokenizes a string character-by-character, classifying each as `Letter`, `Digit`, `Whitespace`, `Special`, or `Unicode`, with code point and escape sequence metadata.

```csharp
var tokens = "A 😀 1".Annotate().ToList();

// tokens[0]: "A",  Type: Letter,     Index: 0
// tokens[1]: " ",  Type: Whitespace,  Index: 1
// tokens[2]: "😀", Type: Unicode,     Index: 2, CodePoint: 0x1F600, UnicodeEscape: "\U0001F600"
// tokens[3]: " ",  Type: Whitespace,  Index: 4  (index advanced past surrogate pair)
// tokens[4]: "1",  Type: Digit,       Index: 5
```

---

## Notes & Compatibility

### .NET Version Support

This library targets **.NET 10** only. Earlier versions are not supported.

### Thread Safety

All utility methods are static and stateless. The library is thread-safe.

### Performance

Regex-heavy paths use `[GeneratedRegex]` for compile-time source generation. `TitleCaseMinorWords` lookups use `HashSet<string>` for O(1) access.

### TLD Updates

`HasValidTopLevelDomain` and `IsValidEmail` validate against the IANA TLD list bundled with the library. This list is updated periodically with new releases.