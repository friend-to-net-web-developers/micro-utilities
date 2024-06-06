using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    public static partial class Url
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

        /// <summary>
        /// Checks the proposed url segment to see if it is valid
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="emptyIsOkay"></param>
        /// <returns></returns>
        public static bool PathSegmentIsValid(string? segment, bool emptyIsOkay = false)
        {
            if (segment == null) return false;
            if (string.IsNullOrEmpty(segment)) return emptyIsOkay;
            if (SingleDot.Equals(segment) || DoubleDot.Equals(segment)) return true;
            if (HttpUtility.UrlEncode(segment) != segment) return false;
            return UrlSegmentRegex().IsMatch(segment);
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
            using var reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tlds.txt"));
            while (reader.ReadLine() is { } line)
            {
                if (line == tld) return true;
            }
            return false;
        }

        
    }
    
    [GeneratedRegex("[A-Z]")]
    private static partial Regex UpperCaseMatchRegex();
    
    [GeneratedRegex("\\-{2,}")]
    private static partial Regex MultipleDashesReplacementRegex();
    
    [GeneratedRegex(@"^[a-zA-Z][-a-zA-Z0-9_]*$")]
    private static partial Regex UrlSegmentRegex();
    
    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex UrlSlugRegex();
    
    [GeneratedRegex("[^a-z0-9\\-]")]
    private static partial Regex UrlSlugReplaceRegex();
    
}