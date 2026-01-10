using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using FriendToNetWebDevelopers.MicroUtilities.Database;

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    public static class Url
    {
        private const string SingleDot = ".";
        private const string DoubleDot = "..";

        
        /// <summary>
        /// Uri resource is built into a string.<br/>
        /// In debug mode (localhost), a port number is included.
        /// </summary>
        /// <param name="url">The Uri resource</param>
        /// <param name="forceIncludePort">Will FORCE a port number to be included in the final url, ignoring debug mode</param>
        /// <returns>An absolute url</returns>
        public static string BuildAbsoluteUrl(Uri url, bool forceIncludePort = false)
        {
            return IsDebug() || forceIncludePort
                ? url.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Port, UriFormat.SafeUnescaped)
                : url.GetComponents(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
        }
        
        [Obsolete("Use IsValidPathSegment instead", false)]
        public static bool PathSegmentIsValid(string? segment, bool emptyIsOkay = false) => IsValidPathSegment(segment, emptyIsOkay);

        /// <summary>
        /// Determines whether a given string is a valid path segment.
        /// </summary>
        /// <param name="segment">The path segment to validate. It can be null or empty.</param>
        /// <param name="emptyIsOkay">Specifies whether an empty segment is considered valid.</param>
        /// <returns>
        /// <c>true</c> if the segment is valid; otherwise, <c>false</c>.
        /// A segment is considered valid if it is URL-encoded and matches the allowed segment pattern.
        /// </returns>
        public static bool IsValidPathSegment(string? segment, bool emptyIsOkay = false)
        {
            if (segment == null) return false;
            if (string.IsNullOrEmpty(segment)) return emptyIsOkay;
            if (SingleDot.Equals(segment) || DoubleDot.Equals(segment)) return true;
            return HttpUtility.UrlEncode(segment) == segment && UrlSegmentRegex().IsMatch(segment);
        }

        /// <summary>
        /// Validates whether the provided username is a valid userinfo component according to RFC 3986.
        /// </summary>
        /// <param name="username">The username to validate. Can be null or empty.</param>
        /// <param name="emptyIsOkay">Indicates whether an empty or null username is considered valid.</param>
        /// <returns>True if the username is valid; otherwise, false.</returns>
        public static bool IsValidUsername(string? username, bool emptyIsOkay = false)
        {
            if (string.IsNullOrEmpty(username)) return emptyIsOkay;
            // RFC 3986 userinfo: unreserved / pct-encoded / sub-delims / ":"
            return UserInfoRegex().IsMatch(username);
        }

        /// <summary>
        /// Validates whether the provided name is a valid query parameter name.
        /// </summary>
        /// <param name="name">The query parameter name to validate.</param>
        /// <returns>True if the name is valid; otherwise, false.</returns>
        public static bool IsValidQueryParameterName(string? name)
        {
            return !string.IsNullOrWhiteSpace(name) && UrlQueryParameterNameRegex().IsMatch(name);
        }

        #region Sluggery

        /// <summary>
        /// Is the proposed slug valid
        /// </summary>
        /// <param name="slug">proposed slug</param>
        /// <returns></returns>
        public static bool IsValidUriSlug(string? slug)
        {
            return !string.IsNullOrWhiteSpace(slug) && UrlSlugRegex().IsMatch(slug);
        }

        /// <summary>
        /// Convert a string into a slug
        /// </summary>
        /// <param name="convert">The string to attempt to convert</param>
        /// <param name="slug">The formed slug - will be blank on failure</param>
        /// <returns>true on success, false on failure</returns>
        public static bool TryToConvertToSlug(string? convert, out string slug)
        {
            //It's most certainly not going to work
            if (string.IsNullOrWhiteSpace(convert))
            {
                slug = string.Empty;
                return false;
            }
            
            //Convert camel case to something slug safe
            var converting = char.ToLower(convert[0]).ToString();
            converting += UpperCaseMatchRegex().Replace(convert[1..], match => "-" + char.ToLower(match.Value[0]));
            
            //Replace non-alphanumeric with dashes
            converting = UrlSlugReplaceRegex().Replace(converting, "-");

            //Replace multiple dashes with a single dash
            converting = MultipleDashesReplacementRegex().Replace(converting, "-");

            //Validate
            if (IsValidUriSlug(converting))
            {
                //It's okay.  Give the new slug
                slug = converting;
                return true;
            }

            //Something is very wrong
            slug = string.Empty;
            return false;
        }

        #endregion

        /// <summary>
        /// Build a full url, url encoding the contents of the query object
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="queryObject"></param>
        /// <returns></returns>
        public static string BuildUrl(string baseUrl, IDictionary<string, string> queryObject)
            => BuildUrl(baseUrl, queryObject.ToArray());
        
        /// <summary>
        /// Build a full url, url encoding the contents of the query object
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="queryObject"></param>
        /// <returns></returns>
        public static string BuildUrl(string baseUrl, IEnumerable<KeyValuePair<string, string>> queryObject)
        {
            var s = new StringBuilder(baseUrl);
            var first = true;
            foreach (var (key, member) in queryObject)
            {
                var k = HttpUtility.UrlEncode(key);
                if (k.Contains("%5b%5d")) k = k.Replace("%5b%5d", "[]");
                var v = HttpUtility.UrlEncode(member);
                s.Append(first ? "?" : "&");
                first = false;
                s.Append($"{k}={v}");
            }

            return s.ToString();
        }


        /// <summary>
        /// Checks if the Top-level domain within the host of the given URI is a valid domain
        /// </summary>
        /// <param name="uri">The Uri resource to check against</param>
        /// <param name="okayIfNotDnsType">This is for catching edge cases like disallowing ipv4 hosts to get through.  default = true</param>
        /// <remarks>File pulled from https://data.iana.org/TLD/tlds-alpha-by-domain.txt</remarks>
        /// <returns></returns>
        public static bool HasValidTopLevelDomain(Uri uri, bool okayIfNotDnsType = true)
        {
            if (uri.HostNameType is not UriHostNameType.Dns || string.IsNullOrEmpty(uri.Host)) return okayIfNotDnsType;
            var tld = uri.Host.Split('.')[^1].ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(tld)) return false;
            //NOTE: I do want this to throw an exception if the file isn't there for some reason
            using var reader = new StringReader(Tlds.Domains);
            while (reader.ReadLine() is { } line) if (!line.StartsWith('#') && line == tld) return true;
            return false;
        }

        
    }
    
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