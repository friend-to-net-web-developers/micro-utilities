using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Net.Mail;
using FriendToNetWebDevelopers.MicroUtilities.Enum;

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
        /// Determines whether the given email address is valid based on a series of checks.
        /// The validation includes checking for proper format, valid hostname, and top-level domain.
        /// </summary>
        /// <remarks>
        /// Deliberately does not use built-in .NET email validation due to its limitations in handling certain edge cases.
        /// This method focuses on a pure email address. Treats the email as a part of a Uri.
        /// </remarks>
        /// <param name="email">The email address to validate. Can be null or empty, but will return false in such cases.</param>
        /// <returns>
        /// True if the provided email address passes validation checks; otherwise, false.
        /// </returns>
        /// <see cref="Uri.TryCreate(string, UriKind, out Uri)"/>
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

        /// <summary>
        /// Attempts to validate and normalize the provided email address using specific normalization strategies.
        /// Normalization includes transformations like converting to lowercase, trimming whitespace, and applying
        /// other specified rules to produce a "pure" email representation.
        /// </summary>
        /// <remarks>
        /// This method can ignore internal validation if explicitly instructed. The result is deemed valid
        /// only if the normalization and validation operations succeed based on the specified strategy.
        /// </remarks>
        /// <param name="email">The email address to process. Can be null or empty, in which case normalization will fail.</param>
        /// <param name="normalizedPure">Outputs the normalized, "pure" email address if processing succeeds; otherwise, null.</param>
        /// <returns>
        /// True if the email is valid and normalization is successful; otherwise, false.
        /// </returns>
        public static bool TryGetNormalizedValidEmail(string? email, [NotNullWhen(true)] out string? normalizedPure)
            => TryGetNormalizedValidEmail(email, out normalizedPure, TryGetNormalizedValidEmailStrategyEnum.All);

        /// <summary>
        /// Attempts to validate and normalize the given email address based on specified strategies.
        /// If successful, returns a normalized version of the email address, otherwise returns false.
        /// </summary>
        /// <remarks>
        /// This method applies various strategies to normalize email addresses, such as removing tags,
        /// trimming spaces, converting to lowercase, and dropping unnecessary dots (all strategies).
        /// The email is considered valid only after passing internal validation (if not skipped).
        /// </remarks>
        /// <param name="email">
        /// The email address to process. Can be null or empty. The method will return false in such cases.
        /// </param>
        /// <param name="normalizedPure">
        /// The output parameter that contains the normalized email address if the method returns true.
        /// Will be null if the method returns false.
        /// </param>
        /// <param name="skipInternalValidation">
        /// If set to true, skips internal validation logic and applies only the normalization strategies.
        /// If false, email validation is performed before normalization.
        /// </param>
        /// <returns>
        /// True if the email passes validation and is successfully normalized; otherwise, false.
        /// </returns>
        public static bool TryGetNormalizedValidEmail(string? email, [NotNullWhen(true)] out string? normalizedPure,
            bool skipInternalValidation)
            => TryGetNormalizedValidEmail(email, out normalizedPure, TryGetNormalizedValidEmailStrategyEnum.All, skipInternalValidation);

        /// <summary>
        /// Attempts to normalize and validate an email address based on a customizable set of strategies.
        /// Normalization processes can include converting to lowercase, removing tags, trimming whitespace,
        /// and dropping dots from the username portion of the email address.
        /// </summary>
        /// <remarks>
        /// If the email address is valid and normalization is successful, the method outputs the normalized
        /// version of the email and returns true. Otherwise, it returns false.
        /// The validation process may omit certain checks if specified by the parameters.
        /// </remarks>
        /// <param name="email">The input email address to validate and normalize.
        /// Can be null, in which case the method returns false.</param>
        /// <param name="normalizedPure">Outputs the normalized version of the email if the input is valid;
        /// otherwise, null.</param>
        /// <param name="strategy">Defines the specific actions to perform during the normalization
        /// process, such as converting to lowercase, removing tags, or trimming whitespace.</param>
        /// <param name="skipInternalValidation">Indicates whether to bypass additional validation steps
        /// within the method. When true, some internal safety checks will be skipped.</param>
        /// <returns>
        /// True if the email is valid according to the provided parameters and normalization
        /// succeeds; otherwise, false.
        /// </returns>
        public static bool TryGetNormalizedValidEmail(
            string? email,
            [NotNullWhen(true)] out string? normalizedPure,
            TryGetNormalizedValidEmailStrategyEnum strategy,
            bool skipInternalValidation = false)
        {
            if (email == null)
            {
                normalizedPure = null;
                return false;
            }

            var okay = MailAddress.TryCreate(email, out var parsedResult);
            if (!okay || parsedResult == null || !skipInternalValidation && !IsValidEmail(parsedResult.Address))
            {
                normalizedPure = null;
                return false;
            }
            
            string normalized;
            // Get rid of +(?) in an email address username (foo+1@bar.com)
            if (((int)strategy & (int)TryGetNormalizedValidEmailStrategyEnum.DropTag) != 0)
            {
                var user = parsedResult.User;
                user = user.Split('+')[0];
                normalized = $"{user}@{parsedResult.Host}";
            }
            else
            {
                normalized = parsedResult.Address;
            }
            // This shouldn't be NECESSARY but because other safety measures can be skipped, I'm leaving it in
            if (((int)strategy & (int)TryGetNormalizedValidEmailStrategyEnum.Trim) != 0)
            {
                normalized = normalized.Trim();
            }
            // While I think this should ALWAYS be done, I'm leaving it in as an option for case-sensitive systems
            if (((int)strategy & (int)TryGetNormalizedValidEmailStrategyEnum.ToLower) != 0)
            {
                normalized = normalized.ToLowerInvariant();
            }
            // Some systems, like Gmail, ignore dots in email addresses. This normalizes around that.
            if (((int)strategy & (int)TryGetNormalizedValidEmailStrategyEnum.DropDot) != 0)
            {
                var parts = normalized.Split('@');
                if (parts.Length == 2)
                {
                    normalized = $"{parts[0].Replace(".", "")}@{parts[1]}";
                }
            }
            //Stripping ":" → I wasn't asking, for the most obvious reasons. Only hostile actors would try.
            //   AND this only needs to be checked if internal validation is skipped
            {
                var parts = normalized.Split('@');
                if (parts.Length == 2)
                {
                    normalized = $"{parts[0].Replace(":", "")}@{parts[1]}";
                }
            }

            normalizedPure = normalized;
            return true;
        }
    }

    [GeneratedRegex(@"^[a-z0-9_](\.{0,1}[\-\+a-z0-9_])*[a-z0-9_]$")]
    private static partial Regex EmailUsernameRegex();
}