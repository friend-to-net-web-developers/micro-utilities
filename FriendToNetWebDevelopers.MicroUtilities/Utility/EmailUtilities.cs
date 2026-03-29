using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using System.Text.RegularExpressions;
using FriendToNetWebDevelopers.MicroUtilities.Enum;
using FriendToNetWebDevelopers.MicroUtilities.Extensions;
using FriendToNetWebDevelopers.MicroUtilities.Models;
using FriendToNetWebDevelopers.MicroUtilities.Models.EmailAnnotator;

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    /// <summary>
    /// Handles various email validation and normalization operations.
    /// </summary>
    /// <remarks>
    /// Internationalized domain names (IDN) are fully supported via Punycode normalization.
    /// Local parts are treated as case-sensitive per RFC 5321 — normalize before comparing
    /// if case-insensitive matching is required.
    /// </remarks>
    public static class Email
    {
        /// <summary>
        /// Determines whether the given email address is valid based on a series of checks.
        /// The validation includes checking for proper format, valid hostname, and top-level domain.
        /// </summary>
        /// <remarks>
        /// Deliberately does not use built-in .NET email validation due to its limitations in
        /// handling certain edge cases. Treats the email as a part of a URI.
        /// <para>
        /// <b>Case sensitivity:</b> Local parts are case-sensitive per RFC 5321.
        /// <c>User@example.com</c> and <c>user@example.com</c> are technically distinct addresses.
        /// This method will return <c>false</c> for an email whose local part differs in case from
        /// what the URI parser reconstructs. Normalize (e.g. <see cref="TryGetNormalizedValidPunyEmail(string?,out PunyUniResult?,out AddressPartAnnotation?)"/>)
        /// before calling this if case-insensitive matching is desired.
        /// </para>
        /// </remarks>
        /// <param name="email">The email address to validate. Can be null or empty, in which case this returns false.</param>
        /// <returns>True if the provided email address passes all validation checks; otherwise, false.</returns>
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

            // RFC 5321: local parts are case-sensitive. Uri preserves UserInfo casing but
            // lowercases the host, so this equality check is intentionally exact on the local part.
            var spawned = $"{uri.UserInfo.Split(':')[0]}@{uri.DnsSafeHost}";
            return spawned == email;
        }

        /// <summary>
        /// Attempts to validate and normalize the provided email address using all normalization strategies.
        /// </summary>
        /// <remarks>
        /// <b>Deprecated.</b> Use <see cref="TryGetNormalizedValidPunyEmail(string?,out PunyUniResult?,out AddressPartAnnotation?)"/> instead,
        /// which returns both Punycode and Unicode forms and exposes suspicious-input flags.
        /// </remarks>
        /// <param name="email">The email address to process. Can be null or empty, in which case this returns false.</param>
        /// <param name="normalizedPure">The normalized email address if successful; otherwise, null.</param>
        /// <returns>True if the email is valid and normalization succeeded; otherwise, false.</returns>
        [Obsolete("This method is deprecated. Please use TryGetNormalizedValidPunyEmail instead.")]
        public static bool TryGetNormalizedValidEmail(string? email, [NotNullWhen(true)] out string? normalizedPure)
            => TryGetNormalizedValidEmail(email, out normalizedPure, TryGetNormalizedValidEmailStrategyEnum.All);

        /// <summary>
        /// Attempts to validate and normalize the provided email address using all normalization strategies,
        /// with optional skipping of internal validation.
        /// </summary>
        /// <remarks>
        /// <b>Deprecated.</b> Use <see cref="TryGetNormalizedValidPunyEmail(string?,out PunyUniResult?,out AddressPartAnnotation?)"/> instead.
        /// </remarks>
        /// <param name="email">The email address to process. Can be null or empty, in which case this returns false.</param>
        /// <param name="normalizedPure">The normalized email address if successful; otherwise, null.</param>
        /// <param name="skipInternalValidation">If true, skips internal validation and applies only normalization strategies.</param>
        /// <returns>True if the email is valid and normalization succeeded; otherwise, false.</returns>
        [Obsolete("This method is deprecated. Please use TryGetNormalizedValidPunyEmail instead.")]
        public static bool TryGetNormalizedValidEmail(string? email, [NotNullWhen(true)] out string? normalizedPure,
            bool skipInternalValidation)
            => TryGetNormalizedValidEmail(email, out normalizedPure, TryGetNormalizedValidEmailStrategyEnum.All,
                skipInternalValidation);

        /// <summary>
        /// Attempts to normalize and validate an email address based on a customizable set of strategies.
        /// </summary>
        /// <remarks>
        /// <b>Deprecated.</b> Use <see cref="TryGetNormalizedValidPunyEmail(string?,TryGetNormalizedValidEmailStrategyEnum,out PunyUniResult?,out AddressPartAnnotation?,bool)"/> instead.
        /// </remarks>
        /// <param name="email">The email address to validate and normalize. Can be null, in which case this returns false.</param>
        /// <param name="normalizedPure">The normalized email address if successful; otherwise, null.</param>
        /// <param name="strategy">The normalization strategies to apply.</param>
        /// <param name="skipInternalValidation">If true, bypasses internal validation checks.</param>
        /// <returns>True if the email is valid and normalization succeeded; otherwise, false.</returns>
        [Obsolete("This method is deprecated. Please use TryGetNormalizedValidPunyEmail instead.")]
        public static bool TryGetNormalizedValidEmail(
            string? email,
            [NotNullWhen(true)] out string? normalizedPure,
            TryGetNormalizedValidEmailStrategyEnum strategy,
            bool skipInternalValidation = false)
        {
            var okay = TryGetNormalizedValidPunyEmail(email, strategy, out var punyResult, out _,
                skipInternalValidation);

            if (!okay || punyResult == null ||
                punyResult.InputForm is DomainInputForm.Invalid or DomainInputForm.Mixed ||
                punyResult.Unicode == null)
            {
                normalizedPure = null;
                return false;
            }

            normalizedPure = punyResult.Unicode;
            return true;
        }

        /// <summary>
        /// Attempts to validate and normalize a provided email address into both Punycode and Unicode forms,
        /// using all normalization strategies.
        /// </summary>
        /// <remarks>
        /// This is the preferred overload for most callers. It applies all strategies: <c>ToLower</c>,
        /// <c>Trim</c>, <c>DropTag</c>, and <c>DropDot</c>.
        /// <para>
        /// <b>Warning:</b> <c>DropTag</c> (plus-addressing) and <c>DropDot</c> are provider-specific
        /// behaviours. <c>DropTag</c> is appropriate for Gmail and similar providers but may mangle
        /// addresses on systems where <c>+</c> is not a sub-address delimiter (e.g. Fastmail, ProtonMail).
        /// <c>DropDot</c> is Gmail-specific. If normalizing addresses for providers other than Gmail,
        /// use the strategy overload and omit these flags.
        /// </para>
        /// </remarks>
        /// <param name="email">The email address to normalize and validate. Can be null or empty.</param>
        /// <param name="result">
        /// The normalized result containing both Unicode and Punycode forms, the detected domain input form,
        /// and a suspicious-input flag. Null if this method returns false.
        /// </param>
        /// <param name="inputAnnotation">
        /// Character-level annotation of the input, including homoglyph detection and Unicode analysis.
        /// Null if this method returns false.
        /// </param>
        /// <returns>True if the email is valid and normalization succeeded; otherwise, false.</returns>
        public static bool TryGetNormalizedValidPunyEmail(string? email,
            [NotNullWhen(true)] out PunyUniResult? result,
            [NotNullWhen(true)] out AddressPartAnnotation? inputAnnotation)
            => TryGetNormalizedValidPunyEmail(email, TryGetNormalizedValidEmailStrategyEnum.All, out result,
                out inputAnnotation);

        /// <summary>
        /// Attempts to validate, normalize, and convert an email address to both Punycode and Unicode forms,
        /// based on a specified set of strategies.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>DropTag</b> removes plus sub-addressing (<c>foo+tag@example.com</c> → <c>foo@example.com</c>).
        /// This is appropriate for Gmail but not for all providers. Do not include in <paramref name="strategy"/>
        /// when normalizing addresses for Fastmail, ProtonMail, or other providers where <c>+</c> is not semantic.
        /// </para>
        /// <para>
        /// <b>DropDot</b> removes dots from the local part (<c>first.last@gmail.com</c> → <c>firstlast@gmail.com</c>).
        /// This is a Gmail-specific behaviour and should not be applied generally.
        /// </para>
        /// <para>
        /// <b>Colon stripping</b> is always applied regardless of strategy — only hostile actors submit
        /// colons in the local part.
        /// </para>
        /// </remarks>
        /// <param name="email">The email address to validate and normalize. Can be null or empty, which results in false.</param>
        /// <param name="strategy">
        /// The normalization strategies to apply. Flags may be combined. See <see cref="TryGetNormalizedValidEmailStrategyEnum"/>
        /// for available options and pre-built combinations.
        /// </param>
        /// <param name="result">
        /// The normalized result if successful; otherwise, null. Contains Unicode and Punycode canonical forms,
        /// the detected domain input form, and a suspicious-input flag.
        /// </param>
        /// <param name="inputAnnotation">
        /// Character-level annotation of the raw input. Null if this method returns false.
        /// </param>
        /// <param name="skipInternalValidation">
        /// If true, skips <see cref="IsValidEmail"/> checks. Useful for internal or custom domains
        /// (e.g. <c>admin@internal</c>) that would not pass TLD validation.
        /// </param>
        /// <returns>True if the email was successfully validated, normalized, and converted; otherwise, false.</returns>
        public static bool TryGetNormalizedValidPunyEmail(
            string? email,
            TryGetNormalizedValidEmailStrategyEnum strategy,
            [NotNullWhen(true)] out PunyUniResult? result,
            [NotNullWhen(true)] out AddressPartAnnotation? inputAnnotation,
            bool skipInternalValidation = false)
        {
            if (email == null)
            {
                result = null;
                inputAnnotation = null;
                return false;
            }

            inputAnnotation = email.Annotate(InputMode.Email);

            var okay = MailAddress.TryCreate(email, out var parsedResult);
            if (!okay || parsedResult == null || (!skipInternalValidation && !IsValidEmail(parsedResult.Address)))
            {
                result = null;
                return false;
            }

            string normalized;

            // DropTag: remove plus sub-addressing (foo+tag@example.com → foo@example.com).
            // Provider-specific — appropriate for Gmail; do not use for Fastmail, ProtonMail, etc.
            if (strategy.HasFlag(TryGetNormalizedValidEmailStrategyEnum.DropTag))
            {
                var user = parsedResult.User.Split('+')[0];
                normalized = $"{user}@{parsedResult.Host}";
            }
            else
            {
                normalized = parsedResult.Address;
            }

            // Trim: defensive — shouldn't be necessary after MailAddress.TryCreate, but
            // included for correctness when skipInternalValidation is true.
            if (strategy.HasFlag(TryGetNormalizedValidEmailStrategyEnum.Trim))
            {
                normalized = normalized.Trim();
            }

            // ToLower: recommended for all systems unless the downstream is explicitly case-sensitive.
            if (strategy.HasFlag(TryGetNormalizedValidEmailStrategyEnum.ToLower))
            {
                normalized = normalized.ToLowerInvariant();
            }

            // DropDot: removes dots from the local part (first.last@gmail.com → firstlast@gmail.com).
            // Gmail-specific behaviour — do not apply to other providers.
            if (strategy.HasFlag(TryGetNormalizedValidEmailStrategyEnum.DropDot))
            {
                var parts = normalized.Split('@');
                if (parts.Length == 2)
                {
                    normalized = $"{parts[0].Replace(".", "")}@{parts[1]}";
                }
            }

            // Colon stripping — always applied, not negotiable.
            // Colons in the local part have no legitimate use; only hostile actors submit them.
            {
                var parts = normalized.Split('@');
                if (parts.Length == 2)
                {
                    normalized = $"{parts[0].Replace(":", "")}@{parts[1]}";
                }
            }

            // Punycode/Unicode normalization — always applied, not negotiable.
            {
                var normalizedBaseParts = normalized.Split('@');
                if (normalizedBaseParts.Length != 2)
                {
                    result = null;
                    return false;
                }

                // Assume hostile input. Check whether domain has already been Punycode-encoded.
                var domainInput = normalizedBaseParts[1];
                var punyResult = PunyUniResult.From(domainInput);

                // Invalid or mixed-encoding domain — bail immediately.
                if (punyResult.InputForm is DomainInputForm.Invalid or DomainInputForm.Mixed
                    || punyResult.Punycode == null)
                {
                    result = punyResult;
                    return false;
                }

                // Raw Punycode submitted by a human is suspicious, but still normalizable.
                // IsSuspicious is already set on punyResult for Punycode/Mixed/Invalid inputs.
                // The annotation surfaces this to the caller.
                var localPart = normalizedBaseParts[0];

                result = new PunyUniResult(
                    Input: normalized,
                    Unicode: $"{localPart}@{punyResult.Unicode}",
                    Punycode: $"{localPart}@{punyResult.Punycode}",
                    InputForm: punyResult.InputForm,
                    IsSuspicious: punyResult.IsSuspicious || inputAnnotation.ContainsSuspiciousChars
                );
            }

            return true;
        }
    }

    /// <summary>
    /// Matches a valid email local part per RFC 5321.
    /// Rules:
    ///   - Must start and end with an alphanumeric character or underscore.
    ///   - Middle characters may include: letters, digits, underscores, hyphens, plus signs, and dots.
    ///   - Single-character local parts (e.g. "a@example.com") are valid.
    /// Note: This intentionally excludes the full RFC 5321 quoted-string form ("<c>"foo bar"@example.com</c>"),
    /// which is technically valid but essentially never seen in practice and commonly rejected by mail servers.
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9_]([.\-+a-z0-9_]*[a-z0-9_])?$")]
    private static partial Regex EmailUsernameRegex();
}