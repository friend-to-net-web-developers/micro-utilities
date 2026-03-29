using System.Reflection;
using System.Text.RegularExpressions;
using FriendToNetWebDevelopers.MicroUtilities.Enum;
using FriendToNetWebDevelopers.MicroUtilities.Exception;

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    public static class Variable
    {
        // Minor words that should not be capitalised in title case unless they are
        // the first or last word. Based on standard English title-case conventions
        // (Chicago Manual of Style / AP style).
        private static readonly HashSet<string> TitleCaseMinorWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "the",
            "and", "but", "for", "or", "nor",
            "at", "by", "in", "of", "on", "to", "with", "from"
        };

        /// <summary>
        /// Determines the naming convention of a given variable name based on its structure and characters.
        /// </summary>
        /// <param name="variableName">The variable name to analyse.</param>
        /// <returns>
        /// A <see cref="ResultsVariableNameTypeEnum"/> value indicating the detected convention.
        /// Returns <see cref="ResultsVariableNameTypeEnum.Words"/> for space-separated or mixed input
        /// that does not match a variable naming convention.
        /// Returns <see cref="ResultsVariableNameTypeEnum.Unknown"/> if the input is null, whitespace,
        /// or contains no letters or digits.
        /// </returns>
        public static ResultsVariableNameTypeEnum GetVariableFormat(string variableName)
        {
            if (string.IsNullOrWhiteSpace(variableName)) return ResultsVariableNameTypeEnum.Unknown;
            if (!variableName.Any(char.IsLetterOrDigit)) return ResultsVariableNameTypeEnum.Unknown;

            var isPotentialVariable = char.IsLetter(variableName[0]) &&
                                      !variableName.EndsWith('_') &&
                                      !variableName.EndsWith('-') &&
                                      variableName.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-');

            if (isPotentialVariable)
            {
                var hasUnderscore = variableName.Contains('_');
                var hasHyphen = variableName.Contains('-');

                if (!hasUnderscore && !hasHyphen)
                {
                    var allLower = variableName.All(char.IsLower);
                    var allUpper = variableName.All(char.IsUpper);

                    if (allLower) return ResultsVariableNameTypeEnum.Unicase;
                    if (allUpper) return ResultsVariableNameTypeEnum.TrollCase;

                    if (char.IsUpper(variableName[0])) return ResultsVariableNameTypeEnum.PascalCase;
                    if (char.IsLower(variableName[0])) return ResultsVariableNameTypeEnum.CamelCase;
                }

                if (hasUnderscore && !hasHyphen && !variableName.Contains("__"))
                {
                    var hasLower = variableName.Any(char.IsLower);
                    var hasUpper = variableName.Any(char.IsUpper);

                    if (hasLower && !hasUpper) return ResultsVariableNameTypeEnum.SnakeCase;
                    if (hasUpper && !hasLower) return ResultsVariableNameTypeEnum.ScreamingSnakeCase;
                }

                if (hasHyphen && !hasUnderscore && !variableName.Contains("--"))
                {
                    var hasLower = variableName.Any(char.IsLower);
                    var hasUpper = variableName.Any(char.IsUpper);

                    if (hasLower && !hasUpper) return ResultsVariableNameTypeEnum.KebabCase;

                    var words = variableName.Split('-');
                    if (words.All(w => w.Length > 0 && char.IsUpper(w[0]) && w.Skip(1).All(char.IsLower)))
                        return ResultsVariableNameTypeEnum.TrainCase;

                    if (hasUpper && !hasLower) return ResultsVariableNameTypeEnum.TrainCase;
                }
            }

            return ResultsVariableNameTypeEnum.Words;
        }

        /// <summary>
        /// Attempts to determine the naming convention of a given variable name.
        /// </summary>
        /// <remarks>
        /// Returns <c>false</c> for <see cref="ResultsVariableNameTypeEnum.Words"/> and
        /// <see cref="ResultsVariableNameTypeEnum.Unknown"/>, even though <c>Words</c> input
        /// is accepted by <see cref="ConvertTo"/> and <see cref="TryConvertTo"/>. If you need
        /// to convert a space-separated or freeform string, call those methods directly rather
        /// than gating on this one.
        /// </remarks>
        /// <param name="variableName">The variable name to analyse.</param>
        /// <param name="type">
        /// The detected naming convention, or <see cref="ResultsVariableNameTypeEnum.Unknown"/>
        /// if detection failed.
        /// </param>
        /// <returns>
        /// <c>true</c> if a specific naming convention was identified; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryGetVariableFormat(string variableName, out ResultsVariableNameTypeEnum type)
        {
            type = GetVariableFormat(variableName);
            return type != ResultsVariableNameTypeEnum.Unknown && type != ResultsVariableNameTypeEnum.Words;
        }

        /// <summary>
        /// Converts a variable name from its detected naming convention to the specified target convention.
        /// </summary>
        /// <remarks>
        /// Space-separated or mixed input (<see cref="ResultsVariableNameTypeEnum.Words"/>) is accepted
        /// and will be split on non-alphanumeric boundaries before conversion.
        /// <para>
        /// <b>Acronym casing:</b> When converting to CamelCase, PascalCase, or similar mixed-case formats,
        /// the tail of each word is lowercased. This means <c>HTMLParser</c> converts to
        /// <c>htmlParser</c> (camelCase) or <c>HtmlParser</c> (PascalCase) rather than preserving
        /// the original acronym casing. This is consistent with common conventions (e.g. .NET's own
        /// <c>HtmlEncoder</c>, <c>JsonSerializer</c>) and is intentional.
        /// </para>
        /// <para>
        /// <b>Unicase / TrollCase inputs:</b> Word boundaries cannot be recovered from all-lower or
        /// all-upper input. Such inputs are treated as a single word.
        /// </para>
        /// </remarks>
        /// <param name="variableName">The variable name to convert.</param>
        /// <param name="targetType">The target naming convention.</param>
        /// <returns>
        /// A tuple of the converted name, the detected source convention, and the target convention.
        /// </returns>
        /// <exception cref="NotAValidVariableNameException">
        /// Thrown when the input is null, empty, or contains no recognisable letters or digits.
        /// </exception>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) ConvertTo(
            string variableName,
            RequestedVariableNameTypeEnum targetType)
        {
            var from = GetVariableFormat(variableName);
            if (from == ResultsVariableNameTypeEnum.Unknown)
                throw new NotAValidVariableNameException(variableName);

            if (IsSameFormat(from, targetType)) return (variableName, from, targetType);

            var words = _getWords(variableName, from);
            var result = _internalConvert(words, targetType);
            return (result, from, targetType);
        }

        /// <summary>
        /// Attempts to convert a variable name from its detected naming convention to the specified target convention.
        /// </summary>
        /// <param name="variableName">The variable name to convert.</param>
        /// <param name="targetType">The target naming convention.</param>
        /// <param name="output">
        /// A tuple of the converted name, the detected source convention, and the target convention.
        /// Contains default values if this method returns false.
        /// </param>
        /// <returns>
        /// <c>true</c> if conversion succeeded; <c>false</c> if the input is unrecognisable
        /// or conversion fails.
        /// </returns>
        public static bool TryConvertTo(string variableName, RequestedVariableNameTypeEnum targetType,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output)
        {
            output = default;
            var currentType = GetVariableFormat(variableName);
            if (currentType == ResultsVariableNameTypeEnum.Unknown) return false;

            try
            {
                var words = _getWords(variableName, currentType);
                var result = _internalConvert(words, targetType);
                output = (result, currentType, targetType);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>Converts a variable name to camelCase.</summary>
        /// <inheritdoc cref="ConvertTo"/>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToCamelCase(string variableName) => ConvertTo(variableName, RequestedVariableNameTypeEnum.CamelCase);

        /// <summary>Attempts to convert a variable name to camelCase.</summary>
        /// <inheritdoc cref="TryConvertTo"/>
        public static bool TryConvertToCamelCase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.CamelCase, out output);

        /// <summary>Converts a variable name to PascalCase.</summary>
        /// <inheritdoc cref="ConvertTo"/>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToPascalCase(string variableName) =>
            ConvertTo(variableName, RequestedVariableNameTypeEnum.PascalCase);

        /// <summary>Attempts to convert a variable name to PascalCase.</summary>
        /// <inheritdoc cref="TryConvertTo"/>
        public static bool TryConvertToPascalCase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.PascalCase, out output);

        /// <summary>Converts a variable name to snake_case.</summary>
        /// <inheritdoc cref="ConvertTo"/>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToSnakeCase(string variableName) =>
            ConvertTo(variableName, RequestedVariableNameTypeEnum.SnakeCase);

        /// <summary>Attempts to convert a variable name to snake_case.</summary>
        /// <inheritdoc cref="TryConvertTo"/>
        public static bool TryConvertToSnakeCase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.SnakeCase, out output);

        /// <summary>Converts a variable name to SCREAMING_SNAKE_CASE.</summary>
        /// <inheritdoc cref="ConvertTo"/>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToScreamingSnakeCase(string variableName) =>
            ConvertTo(variableName, RequestedVariableNameTypeEnum.ScreamingSnakeCase);

        /// <summary>Attempts to convert a variable name to SCREAMING_SNAKE_CASE.</summary>
        /// <inheritdoc cref="TryConvertTo"/>
        public static bool TryConvertToScreamingSnakeCase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.ScreamingSnakeCase, out output);

        /// <summary>Converts a variable name to kebab-case.</summary>
        /// <inheritdoc cref="ConvertTo"/>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToKebabCase(string variableName) =>
            ConvertTo(variableName, RequestedVariableNameTypeEnum.KebabCase);

        /// <summary>Attempts to convert a variable name to kebab-case.</summary>
        /// <inheritdoc cref="TryConvertTo"/>
        public static bool TryConvertToKebabCase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.KebabCase, out output);

        /// <summary>Converts a variable name to Train-Case.</summary>
        /// <inheritdoc cref="ConvertTo"/>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToTrainCase(string variableName) =>
            ConvertTo(variableName, RequestedVariableNameTypeEnum.TrainCase);

        /// <summary>Attempts to convert a variable name to Train-Case.</summary>
        /// <inheritdoc cref="TryConvertTo"/>
        public static bool TryConvertToTrainCase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.TrainCase, out output);

        /// <summary>Converts a variable name to unicase.</summary>
        /// <inheritdoc cref="ConvertTo"/>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToUnicase(string variableName) =>
            ConvertTo(variableName, RequestedVariableNameTypeEnum.Unicase);

        /// <summary>Attempts to convert a variable name to unicase.</summary>
        /// <inheritdoc cref="TryConvertTo"/>
        public static bool TryConvertToUnicase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.Unicase, out output);

        /// <summary>Converts a variable name to TROLLCASE.</summary>
        /// <inheritdoc cref="ConvertTo"/>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToTrollCase(string variableName) =>
            ConvertTo(variableName, RequestedVariableNameTypeEnum.TrollCase);

        /// <summary>Attempts to convert a variable name to TROLLCASE.</summary>
        /// <inheritdoc cref="TryConvertTo"/>
        public static bool TryConvertToTrollCase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.TrollCase, out output);

        /// <summary>Converts a variable name to Title Words.</summary>
        /// <inheritdoc cref="ConvertTo"/>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToTitleWords(string variableName) =>
            ConvertTo(variableName, RequestedVariableNameTypeEnum.TitleWords);

        /// <summary>Attempts to convert a variable name to Title Words.</summary>
        /// <inheritdoc cref="TryConvertTo"/>
        public static bool TryConvertToTitleWords(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.TitleWords, out output);

        /// <summary>Converts a variable name to Sentence words.</summary>
        /// <inheritdoc cref="ConvertTo"/>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToSentenceWords(string variableName) =>
            ConvertTo(variableName, RequestedVariableNameTypeEnum.SentenceWords);

        /// <summary>Attempts to convert a variable name to Sentence words.</summary>
        /// <inheritdoc cref="TryConvertTo"/>
        public static bool TryConvertToSentenceWords(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.SentenceWords, out output);

        /// <summary>
        /// Gets a formatted variable name from an object instance's type.
        /// </summary>
        /// <param name="obj">The object whose type name will be formatted.</param>
        /// <param name="targetType">The target naming convention. Defaults to camelCase.</param>
        /// <returns>A formatted variable name string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is null.</exception>
        public static string GetVariableName(object obj,
            RequestedVariableNameTypeEnum targetType = RequestedVariableNameTypeEnum.CamelCase)
        {
            ArgumentNullException.ThrowIfNull(obj);
            return GetVariableName(obj.GetType(), targetType);
        }

        /// <summary>
        /// Gets a formatted variable name from a type or member name.
        /// </summary>
        /// <param name="member">The member or type whose name will be used.</param>
        /// <param name="targetType">The target naming convention. Defaults to camelCase.</param>
        /// <returns>A formatted variable name string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> is null.</exception>
        public static string GetVariableName(MemberInfo member,
            RequestedVariableNameTypeEnum targetType = RequestedVariableNameTypeEnum.CamelCase)
        {
            ArgumentNullException.ThrowIfNull(member);

            var name = member.Name;

            // Strip generic arity suffix (e.g. "List`1" → "List")
            if (name.Contains('`'))
                name = name[..name.IndexOf('`')];

            return ConvertTo(name, targetType).result;
        }

        /// <summary>
        /// Gets a PascalCase class name from an object instance's type.
        /// </summary>
        /// <param name="obj">The object whose type name will be used.</param>
        /// <returns>A PascalCase formatted class name string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is null.</exception>
        public static string GetClassName(object obj)
        {
            ArgumentNullException.ThrowIfNull(obj);
            return GetClassName(obj.GetType());
        }

        /// <summary>
        /// Gets a PascalCase class name from a type.
        /// </summary>
        /// <param name="type">The type whose name will be used.</param>
        /// <returns>A PascalCase formatted class name string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
        public static string GetClassName(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            return GetVariableName(type, RequestedVariableNameTypeEnum.PascalCase);
        }

        /// <summary>
        /// Maps a <see cref="ResultsVariableNameTypeEnum"/> to its equivalent
        /// <see cref="RequestedVariableNameTypeEnum"/> and checks whether it matches
        /// <paramref name="to"/>. Used to short-circuit conversion when source and
        /// target are the same format, without relying on fragile string comparisons
        /// across two separate enum types.
        /// </summary>
        private static bool IsSameFormat(ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) =>
            from switch
            {
                ResultsVariableNameTypeEnum.CamelCase => to == RequestedVariableNameTypeEnum.CamelCase,
                ResultsVariableNameTypeEnum.PascalCase => to == RequestedVariableNameTypeEnum.PascalCase,
                ResultsVariableNameTypeEnum.SnakeCase => to == RequestedVariableNameTypeEnum.SnakeCase,
                ResultsVariableNameTypeEnum.ScreamingSnakeCase =>
                    to == RequestedVariableNameTypeEnum.ScreamingSnakeCase,
                ResultsVariableNameTypeEnum.KebabCase => to == RequestedVariableNameTypeEnum.KebabCase,
                ResultsVariableNameTypeEnum.TrainCase => to == RequestedVariableNameTypeEnum.TrainCase,
                ResultsVariableNameTypeEnum.Unicase => to == RequestedVariableNameTypeEnum.Unicase,
                ResultsVariableNameTypeEnum.TrollCase => to == RequestedVariableNameTypeEnum.TrollCase,
                _ => false
            };

        private static string[] _getWords(string variableName, ResultsVariableNameTypeEnum type)
        {
            switch (type)
            {
                case ResultsVariableNameTypeEnum.SnakeCase:
                case ResultsVariableNameTypeEnum.ScreamingSnakeCase:
                    return variableName.Split('_', StringSplitOptions.RemoveEmptyEntries);

                case ResultsVariableNameTypeEnum.KebabCase:
                case ResultsVariableNameTypeEnum.TrainCase:
                    return variableName.Split('-', StringSplitOptions.RemoveEmptyEntries);

                case ResultsVariableNameTypeEnum.CamelCase:
                case ResultsVariableNameTypeEnum.PascalCase:
                    // Split on word boundaries, correctly handling consecutive uppercase runs
                    // (acronyms) by keeping them together:
                    //   "HTMLParser"    → ["HTML", "Parser"]
                    //   "parseHTMLDoc"  → ["parse", "HTML", "Doc"]
                    //   "MyClassName"   → ["My", "Class", "Name"]
                    // Pattern explanation:
                    //   (?<=[a-z0-9])(?=[A-Z])     — split before an uppercase that follows a lowercase/digit
                    //   (?<=[A-Z])(?=[A-Z][a-z])   — split before the last uppercase in a run (e.g. "HTMLParser" → "HTML|Parser")
                    return PascalSplitRegex().Split(variableName)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToArray();

                case ResultsVariableNameTypeEnum.Words:
                    return Regex.Split(variableName, @"[^a-zA-Z0-9]+")
                        .Where(s => !string.IsNullOrEmpty(s))
                        .SelectMany(p => PascalSplitRegex().Split(p))
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToArray();

                case ResultsVariableNameTypeEnum.Unicase:
                case ResultsVariableNameTypeEnum.TrollCase:
                default:
                    // Word boundaries are unrecoverable from all-lower or all-upper input.
                    // Treated as a single word.
                    return [variableName];
            }
        }

        private static string _internalConvert(string[] words, RequestedVariableNameTypeEnum targetType)
        {
            switch (targetType)
            {
                case RequestedVariableNameTypeEnum.CamelCase:
                    return words[0].ToLowerInvariant() +
                           string.Concat(words.Skip(1).Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant()));

                case RequestedVariableNameTypeEnum.PascalCase:
                    return string.Concat(words.Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant()));

                case RequestedVariableNameTypeEnum.SnakeCase:
                    return string.Join("_", words.Select(w => w.ToLowerInvariant()));

                case RequestedVariableNameTypeEnum.ScreamingSnakeCase:
                    return string.Join("_", words.Select(w => w.ToUpperInvariant()));

                case RequestedVariableNameTypeEnum.KebabCase:
                    return string.Join("-", words.Select(w => w.ToLowerInvariant()));

                case RequestedVariableNameTypeEnum.TrainCase:
                    return string.Join("-",
                        words.Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant()));

                case RequestedVariableNameTypeEnum.Unicase:
                    return string.Concat(words).ToLowerInvariant();

                case RequestedVariableNameTypeEnum.TrollCase:
                    return string.Concat(words).ToUpperInvariant();

                case RequestedVariableNameTypeEnum.TitleWords:
                    return string.Join(" ", words.Select((w, i) =>
                    {
                        var lower = w.ToLowerInvariant();
                        // Minor words are lowercased unless they are first or last
                        if (i > 0 && i < words.Length - 1 && TitleCaseMinorWords.Contains(lower))
                            return lower;
                        return char.ToUpperInvariant(lower[0]) + lower[1..];
                    }));

                case RequestedVariableNameTypeEnum.SentenceWords:
                    return char.ToUpperInvariant(words[0][0]) + words[0][1..].ToLowerInvariant() +
                           (words.Length > 1
                               ? " " + string.Join(" ", words.Skip(1).Select(w => w.ToLowerInvariant()))
                               : "");

                default:
                    return string.Concat(words);
            }
        }
    }

    /// <summary>
    /// Splits a PascalCase or camelCase string on word boundaries while keeping
    /// consecutive-uppercase runs (acronyms) together as a single token.
    /// Examples:
    ///   "HTMLParser"   → ["HTML", "Parser"]
    ///   "parseHTMLDoc" → ["parse", "HTML", "Doc"]
    ///   "MyClass"      → ["My", "Class"]
    /// </summary>
    [GeneratedRegex(@"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])")]
    private static partial Regex PascalSplitRegex();
}