using System.Text.RegularExpressions;

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    /// <summary>
    /// Handles various email validation
    /// Limitations: Currently does not support email addresses with foreign characters
    /// </summary>
    public static class Email
    {
        /// <summary>
        /// Validates email address
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            if (email.IndexOf('@') < 1) return false;
            if (email.Contains(':')) return false;
            var uriOkay = Uri.TryCreate($"https://{email}", UriKind.Absolute, out var uri);
            if (!uriOkay || uri == null) return false;
            var username = uri.UserInfo.Split(':')[0];
            if (!EmailUsernameRegex().IsMatch(username)) return false;
            if (uri.HostNameType is not (UriHostNameType.Dns or UriHostNameType.IPv4 or UriHostNameType.IPv6))
                return false;
            if (uri.HostNameType is UriHostNameType.Dns && uri.Host.IndexOf('.') < 1) return false;
            if (!Url.HasValidTopLevelDomain(uri)) return false;
            var spawned = $"{uri.UserInfo.Split(':')[0]}@{uri.DnsSafeHost}";
            return spawned == email;
        }
    }

    [GeneratedRegex(@"^[a-z0-9_](\.{0,1}[\-\+a-z0-9_])*[a-z0-9_]$")]
    private static partial Regex EmailUsernameRegex();
}