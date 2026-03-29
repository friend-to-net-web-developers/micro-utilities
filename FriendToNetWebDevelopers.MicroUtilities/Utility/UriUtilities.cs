using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using FriendToNetWebDevelopers.MicroUtilities.Database;

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    public static partial class Url
    {
        private const string SingleDot = ".";
        private const string DoubleDot = "..";

        /// <summary>
        /// Builds an absolute URL string from a URI resource.
        /// </summary>
        /// <remarks>
        /// In debug mode (localhost), the port number is included in the output so that
        /// local development servers are addressed correctly (e.g. <c>https://localhost:44328/file.jpg</c>).
        /// In non-debug (production) mode, the port is stripped for clean public-facing URLs
        /// (e.g. <c>https://example.com/file.jpg</c>).
        /// </remarks>
        /// <param name="url">The URI resource to build from.</param>
        /// <param name="forceIncludePort">
        /// When true, forces the port number to be included regardless of debug mode.
        /// </param>
        /// <returns>An absolute URL string.</returns>
        public static string BuildAbsoluteUrl(Uri url, bool forceIncludePort = false)
        {
            // Include port in debug/local mode so dev servers are reachable.
            // Strip port in production for clean public-facing URLs.
            return IsDebug() || forceIncludePort
                ? url.GetComponents(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped)
                : url.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Port, UriFormat.SafeUnescaped);
        }

        /// <summary>
        /// Determines whether a given string is a valid URI path segment.
        /// </summary>
        /// <param name="segment">The path segment to validate. Can be null or empty.</param>
        /// <param name="emptyIsOkay">Whether an empty segment is considered valid. Defaults to false.</param>
        /// <returns>
        /// True if the segment is valid; otherwise, false.
        /// A segment is considered valid if it is URL-encoded and matches the allowed segment pattern.
        /// Single dot (<c>.</c>) and double dot (<c>..</c>) are always considered valid.
        /// </returns>
        public static bool IsValidPathSegment(string? segment, bool emptyIsOkay = false)
        {
            if (segment == null) return false;
            if (string.IsNullOrEmpty(segment)) return emptyIsOkay;
            if (SingleDot.Equals(segment) || DoubleDot.Equals(segment)) return true;
            return HttpUtility.UrlEncode(segment) == segment && UrlSegmentRegex().IsMatch(segment);
        }

        /// <inheritdoc cref="IsValidPathSegment(string?, bool)"/>
        [Obsolete("Use IsValidPathSegment instead.", false)]
        public static bool PathSegmentIsValid(string? segment, bool emptyIsOkay = false)
            => IsValidPathSegment(segment, emptyIsOkay);

        /// <summary>
        /// Validates whether the provided string is a valid URI userinfo component per RFC 3986.
        /// </summary>
        /// <param name="username">The username to validate. Can be null or empty.</param>
        /// <param name="emptyIsOkay">Whether a null or empty value is considered valid. Defaults to false.</param>
        /// <returns>True if the username is valid; otherwise, false.</returns>
        public static bool IsValidUsername(string? username, bool emptyIsOkay = false)
        {
            if (string.IsNullOrEmpty(username)) return emptyIsOkay;
            // RFC 3986 userinfo: unreserved / pct-encoded / sub-delims / ":"
            return UserInfoRegex().IsMatch(username);
        }

        /// <summary>
        /// Validates whether the provided string is a valid URI query parameter name.
        /// </summary>
        /// <param name="name">The query parameter name to validate.</param>
        /// <returns>True if the name is valid; otherwise, false.</returns>
        public static bool IsValidQueryParameterName(string? name)
        {
            return !string.IsNullOrWhiteSpace(name) && UrlQueryParameterNameRegex().IsMatch(name);
        }

        #region Sluggery

        /// <summary>
        /// Determines whether the proposed slug is a valid URI slug.
        /// </summary>
        /// <remarks>
        /// Valid slugs match the pattern <c>^[a-z0-9]+(?:-[a-z0-9]+)*$</c> —
        /// lowercase alphanumeric segments separated by single hyphens.
        /// </remarks>
        /// <param name="slug">The proposed slug to validate.</param>
        /// <returns>True if the slug is valid; otherwise, false.</returns>
        public static bool IsValidUriSlug(string? slug)
        {
            return !string.IsNullOrWhiteSpace(slug) && UrlSlugRegex().IsMatch(slug);
        }

        /// <summary>
        /// Attempts to convert a string into a valid URI slug.
        /// </summary>
        /// <remarks>
        /// Conversion steps applied in order:
        /// <list type="number">
        ///   <item>CamelCase/PascalCase boundaries are split with a hyphen.</item>
        ///   <item>Non-alphanumeric characters are replaced with hyphens.</item>
        ///   <item>Consecutive hyphens are collapsed to a single hyphen.</item>
        ///   <item>The result is validated against the slug pattern.</item>
        /// </list>
        /// </remarks>
        /// <param name="convert">The string to attempt to convert.</param>
        /// <param name="slug">The formed slug on success; <see cref="string.Empty"/> on failure.</param>
        /// <returns>True if a valid slug was produced; false if the input cannot be converted.</returns>
        public static bool TryToConvertToSlug(string? convert, out string slug)
        {
            if (string.IsNullOrWhiteSpace(convert))
            {
                slug = string.Empty;
                return false;
            }

            // Split CamelCase/PascalCase boundaries with a hyphen
            var converting = char.ToLower(convert[0]).ToString();
            converting += UpperCaseMatchRegex().Replace(convert[1..], match => "-" + char.ToLower(match.Value[0]));

            // Replace non-alphanumeric characters with hyphens
            converting = UrlSlugReplaceRegex().Replace(converting, "-");

            // Collapse multiple consecutive hyphens to a single hyphen
            converting = MultipleDashesReplacementRegex().Replace(converting, "-");

            if (IsValidUriSlug(converting))
            {
                slug = converting;
                return true;
            }

            slug = string.Empty;
            return false;
        }

        #endregion

        /// <summary>
        /// Builds a full URL by appending URL-encoded query parameters to a base URL.
        /// </summary>
        /// <remarks>
        /// Bracket notation (<c>key[]</c>) is preserved — <c>%5B%5D</c> sequences are restored
        /// to literal <c>[]</c> to support array-style query parameters
        /// (e.g. <c>ids[]=1&amp;ids[]=2</c>).
        /// </remarks>
        /// <param name="baseUrl">The base URL to append the query string to.</param>
        /// <param name="queryObject">The query parameters as a dictionary. Keys are unique.</param>
        /// <returns>The fully constructed URL with encoded query string.</returns>
        public static string BuildUrl(string baseUrl, IDictionary<string, string> queryObject)
            => BuildUrl(baseUrl, queryObject.ToArray());

        /// <summary>
        /// Builds a full URL by appending URL-encoded query parameters to a base URL.
        /// </summary>
        /// <remarks>
        /// Accepts an enumerable of key-value pairs to allow duplicate keys, which is useful
        /// for array-style query parameters (e.g. <c>ids[]=1&amp;ids[]=2</c>).
        /// Bracket notation (<c>key[]</c>) is preserved — <c>%5B%5D</c> sequences are restored
        /// to literal <c>[]</c> regardless of hex casing.
        /// </remarks>
        /// <param name="baseUrl">The base URL to append the query string to.</param>
        /// <param name="queryObject">The query parameters as key-value pairs. Duplicate keys are permitted.</param>
        /// <returns>The fully constructed URL with encoded query string.</returns>
        public static string BuildUrl(string baseUrl, IEnumerable<KeyValuePair<string, string>> queryObject)
        {
            var s = new StringBuilder(baseUrl);
            var first = true;
            foreach (var (key, value) in queryObject)
            {
                var k = HttpUtility.UrlEncode(key);

                // Restore bracket notation for array-style params (e.g. ids[]=1&ids[]=2).
                // HttpUtility.UrlEncode may produce either %5b%5d or %5B%5D depending on runtime —
                // replace case-insensitively to handle both.
                if (k.Contains("%5b%5d", StringComparison.OrdinalIgnoreCase))
                    k = k.Replace("%5b%5d", "[]", StringComparison.OrdinalIgnoreCase);

                var v = HttpUtility.UrlEncode(value);
                s.Append(first ? '?' : '&');
                first = false;
                s.Append(k).Append('=').Append(v);
            }

            return s.ToString();
        }

        /// <summary>
        /// Checks whether the top-level domain within the host of the given URI is a recognised TLD.
        /// </summary>
        /// <remarks>
        /// Validates against the IANA TLD list (<see href="https://data.iana.org/TLD/tlds-alpha-by-domain.txt"/>).
        /// Non-DNS host types (IPv4, IPv6) are handled by <paramref name="okayIfNotDnsType"/>.
        /// </remarks>
        /// <param name="uri">The URI to check. Host must be in ASCII/Punycode form.</param>
        /// <param name="okayIfNotDnsType">
        /// Whether to return true for non-DNS host types (e.g. IPv4, IPv6). Defaults to true.
        /// Set to false to reject IP-addressed URIs explicitly.
        /// </param>
        /// <returns>True if the TLD is valid or the host is a non-DNS type and <paramref name="okayIfNotDnsType"/> is true; otherwise, false.</returns>
        public static bool HasValidTopLevelDomain(Uri uri, bool okayIfNotDnsType = true)
        {
            if (uri.HostNameType is not UriHostNameType.Dns || string.IsNullOrEmpty(uri.Host))
                return okayIfNotDnsType;

            var tld = uri.DnsSafeHost.Split('.')[^1].ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(tld)) return false;

            // NOTE: intentionally throws if Tlds.Domains is unavailable — a missing TLD list
            // is a configuration error, not a validation failure, and must not silently pass.
            using var reader = new StringReader(Tlds.Domains);
            while (reader.ReadLine() is { } line)
                if (!line.StartsWith('#') && line == tld) return true;

            return false;
        }

        /// <summary>
        /// Attempts to normalize and convert a domain name to its Punycode and Unicode representations.
        /// </summary>
        /// <remarks>
        /// Normalization steps applied before conversion:
        /// <list type="bullet">
        ///   <item>Leading/trailing whitespace is trimmed.</item>
        ///   <item>Leading/trailing dots are trimmed.</item>
        ///   <item>The result is lowercased.</item>
        /// </list>
        /// </remarks>
        /// <param name="inputDomain">The domain name to normalize and convert. Can be null or empty.</param>
        /// <param name="normalizedDomainPunycode">
        /// The Punycode (ASCII-compatible encoding) representation if successful; otherwise, null.
        /// </param>
        /// <param name="normalizedDomainUnicode">
        /// The Unicode representation if successful; otherwise, null.
        /// </param>
        /// <param name="skipInternalValidation">
        /// If true, skips TLD validation. Useful for internal or custom domains.
        /// </param>
        /// <returns>True if normalization and conversion succeeded; otherwise, false.</returns>
        public static bool TryNormalizeAndPunycodeDomain(
            string? inputDomain,
            [NotNullWhen(true)] out string? normalizedDomainPunycode,
            [NotNullWhen(true)] out string? normalizedDomainUnicode,
            bool skipInternalValidation = false)
        {
            if (string.IsNullOrWhiteSpace(inputDomain))
            {
                normalizedDomainPunycode = null;
                normalizedDomainUnicode = null;
                return false;
            }

            var proposedUnicode = inputDomain.Trim().Trim('.');
            var idn = new IdnMapping();

            try
            {
                normalizedDomainPunycode = idn.GetAscii(proposedUnicode).ToLowerInvariant();
            }
            catch (ArgumentException)
            {
                // Input is not a valid domain name — malformed labels, illegal characters, etc.
                normalizedDomainPunycode = null;
                normalizedDomainUnicode = null;
                return false;
            }

            if (!PunycodeDomainMatchRegex().IsMatch(normalizedDomainPunycode))
            {
                normalizedDomainPunycode = null;
                normalizedDomainUnicode = null;
                return false;
            }

            if (!skipInternalValidation &&
                !HasValidTopLevelDomain(new Uri($"https://{normalizedDomainPunycode}")))
            {
                normalizedDomainPunycode = null;
                normalizedDomainUnicode = null;
                return false;
            }

            try
            {
                normalizedDomainUnicode = idn.GetUnicode(normalizedDomainPunycode);
                return true;
            }
            catch (ArgumentException)
            {
                // Punycode produced by GetAscii could not be round-tripped back to Unicode —
                // treat as invalid rather than propagating a corrupt result.
                normalizedDomainPunycode = null;
                normalizedDomainUnicode = null;
                return false;
            }
        }
    }

    [GeneratedRegex(@"^([a-z0-9](?:[a-z0-9\-]{0,61}[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9\-]{0,61}[a-z0-9])?$",
        RegexOptions.IgnoreCase)]
    private static partial Regex PunycodeDomainMatchRegex();

    [GeneratedRegex("[A-Z]")]
    private static partial Regex UpperCaseMatchRegex();

    [GeneratedRegex("\\-{2,}")]
    private static partial Regex MultipleDashesReplacementRegex();

    [GeneratedRegex("^[a-zA-Z][-a-zA-Z0-9_]*$")]
    private static partial Regex UrlSegmentRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9\-\._~!$&'()*+,;=:]+$")]
    private static partial Regex UserInfoRegex();

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex UrlSlugRegex();

    [GeneratedRegex("[^a-z0-9\\-]")]
    private static partial Regex UrlSlugReplaceRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9_.-]+$")]
    private static partial Regex UrlQueryParameterNameRegex();
}