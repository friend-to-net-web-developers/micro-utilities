using System.Text.RegularExpressions;
using FriendToNetWebDevelopers.MicroUtilities.Enum;
using FriendToNetWebDevelopers.MicroUtilities.Exception;

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    public static class Id
    {
        private static readonly Regex IdValueRegex = HtmlIdValueInternalRegex();
        private static readonly Regex IdPrefixValueRegex = HtmlIdPrefixValueInternalRegex();

        private const string DefaultPrefix = "id";

        /// <summary>
        /// Generates a valid HTML <c>id</c> attribute value from a new <see cref="Guid"/>.
        /// </summary>
        /// <param name="prefix">
        /// Required. Must be at least one character, start with a letter, and contain only
        /// letters, digits, hyphens, and underscores. Defaults to <c>"id"</c>.
        /// </param>
        /// <param name="suffix">
        /// Optional. If provided, must be a valid id fragment (letters, digits, hyphens,
        /// underscores). Whitespace is trimmed before use.
        /// </param>
        /// <returns>A valid HTML id attribute value.</returns>
        /// <exception cref="BadIdPrefixException">Thrown when <paramref name="prefix"/> is invalid.</exception>
        /// <exception cref="BadIdFormatException">Thrown when the assembled id fails final validation.</exception>
        public static string GetValidHtmlId(string prefix = DefaultPrefix, string suffix = "")
            => GetValidHtmlId(Guid.NewGuid(), prefix, suffix);

        /// <summary>
        /// Generates a valid HTML <c>id</c> attribute value from the specified <see cref="Guid"/>.
        /// </summary>
        /// <param name="guid">The GUID that forms the base of the id value.</param>
        /// <param name="prefix">
        /// Required. Must be at least one character, start with a letter, and contain only
        /// letters, digits, hyphens, and underscores. Defaults to <c>"id"</c>.
        /// </param>
        /// <param name="suffix">
        /// Optional. Whitespace is trimmed before use.
        /// </param>
        /// <returns>A valid HTML id attribute value.</returns>
        /// <exception cref="BadIdPrefixException">Thrown when <paramref name="prefix"/> is invalid.</exception>
        /// <exception cref="BadIdFormatException">Thrown when the assembled id fails final validation.</exception>
        public static string GetValidHtmlId(Guid guid, string prefix = DefaultPrefix, string suffix = "")
        {
            if (!IsValidIdPrefix(prefix)) throw new BadIdPrefixException(prefix);
            return FinalIdValidate($"{prefix}{guid:N}{suffix.Trim()}");
        }

        /// <summary>
        /// Generates a valid HTML <c>id</c> attribute value from the specified <see cref="int"/> id.
        /// </summary>
        /// <param name="id">The id value that forms the base of the HTML id.</param>
        /// <param name="prefix">
        /// Required. Must be at least one character, start with a letter, and contain only
        /// letters, digits, hyphens, and underscores. Defaults to <c>"id"</c>.
        /// </param>
        /// <param name="suffix">Optional. Whitespace is trimmed before use.</param>
        /// <returns>A valid HTML id attribute value.</returns>
        /// <exception cref="BadIdPrefixException">Thrown when <paramref name="prefix"/> is invalid.</exception>
        /// <exception cref="BadIdFormatException">Thrown when the assembled id fails final validation.</exception>
        public static string GetValidHtmlId(int id, string prefix = DefaultPrefix, string suffix = "")
        {
            if (!IsValidIdPrefix(prefix)) throw new BadIdPrefixException(prefix);
            return FinalIdValidate($"{prefix}{id:0}{suffix.Trim()}");
        }

        /// <summary>
        /// Generates a valid HTML <c>id</c> attribute value from the specified <see cref="uint"/> id.
        /// </summary>
        /// <param name="id">The id value that forms the base of the HTML id.</param>
        /// <param name="prefix">
        /// Required. Must be at least one character, start with a letter, and contain only
        /// letters, digits, hyphens, and underscores. Defaults to <c>"id"</c>.
        /// </param>
        /// <param name="suffix">Optional. Whitespace is trimmed before use.</param>
        /// <returns>A valid HTML id attribute value.</returns>
        /// <exception cref="BadIdPrefixException">Thrown when <paramref name="prefix"/> is invalid.</exception>
        /// <exception cref="BadIdFormatException">Thrown when the assembled id fails final validation.</exception>
        public static string GetValidHtmlId(uint id, string prefix = DefaultPrefix, string suffix = "")
        {
            if (!IsValidIdPrefix(prefix)) throw new BadIdPrefixException(prefix);
            return FinalIdValidate($"{prefix}{id:0}{suffix.Trim()}");
        }

        /// <summary>
        /// Generates a valid HTML <c>id</c> attribute value from the specified <see cref="long"/> id.
        /// </summary>
        /// <param name="id">The id value that forms the base of the HTML id.</param>
        /// <param name="prefix">
        /// Required. Must be at least one character, start with a letter, and contain only
        /// letters, digits, hyphens, and underscores. Defaults to <c>"id"</c>.
        /// </param>
        /// <param name="suffix">Optional. Whitespace is trimmed before use.</param>
        /// <returns>A valid HTML id attribute value.</returns>
        /// <exception cref="BadIdPrefixException">Thrown when <paramref name="prefix"/> is invalid.</exception>
        /// <exception cref="BadIdFormatException">Thrown when the assembled id fails final validation.</exception>
        public static string GetValidHtmlId(long id, string prefix = DefaultPrefix, string suffix = "")
        {
            if (!IsValidIdPrefix(prefix)) throw new BadIdPrefixException(prefix);
            return FinalIdValidate($"{prefix}{id:0}{suffix.Trim()}");
        }

        /// <summary>
        /// Generates a valid HTML <c>id</c> attribute value from the specified <see cref="ulong"/> id.
        /// </summary>
        /// <param name="id">The id value that forms the base of the HTML id.</param>
        /// <param name="prefix">
        /// Required. Must be at least one character, start with a letter, and contain only
        /// letters, digits, hyphens, and underscores. Defaults to <c>"id"</c>.
        /// </param>
        /// <param name="suffix">Optional. Whitespace is trimmed before use.</param>
        /// <returns>A valid HTML id attribute value.</returns>
        /// <exception cref="BadIdPrefixException">Thrown when <paramref name="prefix"/> is invalid.</exception>
        /// <exception cref="BadIdFormatException">Thrown when the assembled id fails final validation.</exception>
        public static string GetValidHtmlId(ulong id, string prefix = DefaultPrefix, string suffix = "")
        {
            if (!IsValidIdPrefix(prefix)) throw new BadIdPrefixException(prefix);
            return FinalIdValidate($"{prefix}{id:0}{suffix.Trim()}");
        }

        /// <summary>
        /// Determines whether the proposed string is a valid HTML <c>id</c> attribute value.
        /// </summary>
        /// <remarks>
        /// Valid ids must start with a letter, end with a letter or digit, and contain only
        /// letters, digits, hyphens, and underscores. Single-character ids (one letter) are valid.
        /// Null, empty, and whitespace-only strings return false.
        /// </remarks>
        /// <param name="id">The proposed id attribute value.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool IsValidId(string? id) => IsValidIdInternal(id);

        /// <summary>
        /// Validates the proposed id attribute value and returns a guaranteed non-null string
        /// using the specified fallback strategy when the input is invalid.
        /// </summary>
        /// <param name="id">The proposed id attribute value.</param>
        /// <param name="fallbackStrategy">
        /// The strategy to apply when <paramref name="id"/> is invalid.
        /// <see cref="TryGetValidIdDefaultStrategyEnum.EmptyOnInvalid"/> returns <see cref="string.Empty"/>;
        /// <see cref="TryGetValidIdDefaultStrategyEnum.GenerateOnInvalid"/> returns a generated id
        /// via <see cref="GetValidHtmlId()"/>.
        /// </param>
        /// <param name="validId">
        /// The original id if valid, or the fallback value if not.
        /// </param>
        /// <returns>True if <paramref name="id"/> was already valid; false if the fallback was used.</returns>
        /// <exception cref="BadIdFormatException">
        /// Thrown when <paramref name="fallbackStrategy"/> is not a recognised value.
        /// </exception>
        public static bool TryGetAsValidId(string? id, TryGetValidIdDefaultStrategyEnum fallbackStrategy,
            out string validId)
        {
            if (IsValidIdInternal(id))
            {
                validId = id!;
                return true;
            }

            validId = fallbackStrategy switch
            {
                TryGetValidIdDefaultStrategyEnum.EmptyOnInvalid => string.Empty,
                TryGetValidIdDefaultStrategyEnum.GenerateOnInvalid => GetValidHtmlId(),
                _ => throw new BadIdFormatException(fallbackStrategy.ToString())
            };
            return false;
        }

        private static string FinalIdValidate(string id)
        {
            if (!IsValidIdInternal(id)) throw new BadIdFormatException(id);
            return id;
        }

        /// <summary>
        /// Validates a proposed id prefix string.
        /// A valid prefix must start with a letter and contain only letters, digits, hyphens,
        /// and underscores. The default prefix <c>"id"</c> is always valid.
        /// Whitespace in a prefix is a caller error — it is not trimmed silently.
        /// </summary>
        private static bool IsValidIdPrefix(string? prefix)
            => !string.IsNullOrWhiteSpace(prefix) && IdPrefixValueRegex.IsMatch(prefix);

        private static bool IsValidIdInternal(string? idString)
            => !string.IsNullOrWhiteSpace(idString) && IdValueRegex.IsMatch(idString);
    }

    /// <summary>
    /// Matches a valid HTML id attribute value.
    /// Rules:
    ///   - Must start with a letter (a-z, A-Z).
    ///   - May contain letters, digits, hyphens, and underscores in the middle.
    ///   - Must end with a letter or digit — not a hyphen or underscore.
    ///   - Single-character ids (one letter) are valid via the optional middle+end group.
    /// </summary>
    [GeneratedRegex("^[a-zA-Z]([a-zA-Z0-9_\\-]*[a-zA-Z0-9])?$")]
    private static partial Regex HtmlIdValueInternalRegex();

    /// <summary>
    /// Matches a valid HTML id prefix.
    /// Rules:
    ///   - Must start with a letter (a-z, A-Z).
    ///   - May be followed by any number of letters, digits, hyphens, and underscores.
    ///   - No minimum length beyond the leading letter — a single-letter prefix is valid.
    /// </summary>
    [GeneratedRegex("^[a-zA-Z][a-zA-Z0-9_\\-]*$")]
    private static partial Regex HtmlIdPrefixValueInternalRegex();
}