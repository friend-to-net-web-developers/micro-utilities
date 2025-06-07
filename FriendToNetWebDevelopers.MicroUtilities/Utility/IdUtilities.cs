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
        /// Default functionality for getting a dynamic id.  Generates a new guid.
        /// </summary>
        /// <param name="prefix">REQUIRED NON-EMPTY, VALID, AT LEAST 2 CHARACTERS - default "id"</param>
        /// <param name="suffix">OPTIONAL non-empty and valid</param>
        /// <returns>Valid html id value</returns>
        public static string GetValidHtmlId(string prefix = DefaultPrefix, string suffix = "")
            => GetValidHtmlId(Guid.NewGuid(), prefix, suffix);

        /// <summary>
        /// Generates a dynamic, valid html id based on the specified guid
        /// </summary>
        /// <param name="guid">The id which forms the base of the final dynamic id</param>
        /// <param name="prefix">REQUIRED NON-EMPTY, VALID, AT LEAST 2 CHARACTERS - default "id"</param>
        /// <param name="suffix">OPTIONAL non-empty and valid</param>
        /// <returns>Valid html id value</returns>
        public static string GetValidHtmlId(Guid guid, string prefix = DefaultPrefix, string suffix = "")
        {
            if (!_isValidIdPrefix(prefix)) throw new BadIdPrefixException(prefix);
            return _finalIdValidate($"{prefix}{guid:N}{suffix.Trim()}");
        }
        
        /// <summary>
        /// Generates a dynamic, valid html id based on the specified id
        /// </summary>
        /// <param name="id">The id which forms the base of the final dynamic id</param>
        /// <param name="prefix">REQUIRED NON-EMPTY, VALID, AT LEAST 2 CHARACTERS - default "id"</param>
        /// <param name="suffix">OPTIONAL non-empty and valid</param>
        /// <returns>Valid html id value</returns>
        public static string GetValidHtmlId(int id, string prefix = DefaultPrefix, string suffix = "")
        {
            if (!_isValidIdPrefix(prefix)) throw new BadIdPrefixException(prefix);
            return _finalIdValidate($"{prefix}{id:0}{suffix.Trim()}");
        }
        
        /// <summary>
        /// Generates a dynamic, valid html id based on the specified id
        /// </summary>
        /// <param name="id">The id which forms the base of the final dynamic id</param>
        /// <param name="prefix">REQUIRED NON-EMPTY, VALID, AT LEAST 2 CHARACTERS - default "id"</param>
        /// <param name="suffix">OPTIONAL non-empty and valid</param>
        /// <returns>Valid html id value</returns>
        public static string GetValidHtmlId(uint id, string prefix = DefaultPrefix, string suffix = "")
        {
            if (!_isValidIdPrefix(prefix)) throw new BadIdPrefixException(prefix);
            return _finalIdValidate($"{prefix}{id:0}{suffix.Trim()}");
        }
        
        /// <summary>
        /// Generates a dynamic, valid html id based on the specified id
        /// </summary>
        /// <param name="id">The id which forms the base of the final dynamic id</param>
        /// <param name="prefix">REQUIRED NON-EMPTY, VALID, AT LEAST 2 CHARACTERS - default "id"</param>
        /// <param name="suffix">OPTIONAL non-empty and valid</param>
        /// <returns>Valid html id value</returns>
        public static string GetValidHtmlId(long id, string prefix = DefaultPrefix, string suffix = "")
        {
            if (!_isValidIdPrefix(prefix)) throw new BadIdPrefixException(prefix);
            return _finalIdValidate($"{prefix.Trim()}{id:0}{suffix.Trim()}");
        }
        
        /// <summary>
        /// Generates a dynamic, valid html id based on the specified id
        /// </summary>
        /// <param name="id">The id which forms the base of the final dynamic id</param>
        /// <param name="prefix">REQUIRED NON-EMPTY, VALID, AT LEAST 2 CHARACTERS - default "id"</param>
        /// <param name="suffix">OPTIONAL non-empty and valid</param>
        /// <returns>Valid html id value</returns>
        public static string GetValidHtmlId(ulong id, string prefix = DefaultPrefix, string suffix = "")
        {
            if (!_isValidIdPrefix(prefix)) throw new BadIdPrefixException(prefix);
            return _finalIdValidate($"{prefix.Trim()}{id:0}{suffix.Trim()}");
        }

        /// <summary>
        /// Checks if an id is valid
        /// </summary>
        /// <param name="id">The proposed id attribute value</param>
        /// <returns></returns>
        public static bool IsValidId(string? id) => _isValidId(id);

        /// <summary>
        /// Ensures the proposed id attribute value is valid. If invalid, it uses the fallback strategies to ensure the output is always non-nullable
        /// </summary>
        /// <param name="id">The proposed id attribute value</param>
        /// <param name="fallbackStrategy">empty or generate</param>
        /// <param name="validId">the output</param>
        /// <returns></returns>
        /// <exception cref="BadIdFormatException"></exception>
        public static bool TryGetAsValidId(string? id, TryGetValidIdDefaultStrategyEnum fallbackStrategy, out string validId)
        {
            if (_isValidId(id))
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
        
        private static string _finalIdValidate(string id)
        {
            if (!_isValidId(id)) throw new BadIdFormatException(id);
            return id;
        }
        
        private static bool _isValidIdPrefix(string? idString)
            => DefaultPrefix == idString || 
               (!string.IsNullOrWhiteSpace(idString) && IdPrefixValueRegex.IsMatch(idString));
        
        private static bool _isValidId(string? idString)
            => !string.IsNullOrWhiteSpace(idString) && IdValueRegex.IsMatch(idString);
    }
    
    [GeneratedRegex("^[a-zA-Z][a-zA-Z0-9_\\-]*[a-zA-Z0-9]$")]
    private static partial Regex HtmlIdValueInternalRegex();
    [GeneratedRegex("^[a-zA-Z][a-zA-Z0-9_\\-]*$")]
    private static partial Regex HtmlIdPrefixValueInternalRegex();
}