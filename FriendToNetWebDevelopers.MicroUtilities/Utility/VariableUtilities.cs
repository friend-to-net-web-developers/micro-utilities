using System.Reflection;
using System.Text.RegularExpressions;
using FriendToNetWebDevelopers.MicroUtilities.Enum;
using FriendToNetWebDevelopers.MicroUtilities.Exception;

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    public static class Variable
    {
        /// <summary>
        /// Determines the format type of a given variable name based on its structure and characters.
        /// </summary>
        /// <param name="variableName">The variable name whose format is to be determined.</param>
        /// <returns>A <see cref="ResultsVariableNameTypeEnum"/> value indicating the format type of the variable name. Returns <c>Unknown</c> if the format cannot be determined or the variable name is invalid.</returns>
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
        /// Tries to determine the format type of a given variable name based on its structure and characters.
        /// </summary>
        /// <param name="variableName">The variable name whose format is to be analyzed.</param>
        /// <param name="type">When this method returns, contains the determined format type of the variable. If the format is unknown or the input is invalid, this will be set to <c>ResultsVariableNameTypeEnum.Unknown</c>.</param>
        /// <returns><c>true</c> if the format of the variable name is identified successfully; otherwise, <c>false</c>.</returns>
        public static bool TryGetVariableFormat(string variableName, out ResultsVariableNameTypeEnum type)
        {
            type = GetVariableFormat(variableName);
            return type != ResultsVariableNameTypeEnum.Unknown && type != ResultsVariableNameTypeEnum.Words;
        }

        /// <summary>
        /// Converts the format of a given variable name to a specified target format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted.</param>
        /// <param name="targetType">The target format type to which the variable name should be converted.</param>
        /// <returns>A tuple containing the converted variable name, the original format type, and the target format type.</returns>
        /// <exception cref="ArgumentException">Thrown when the target format type is <c>Unknown</c>.</exception>
        /// <exception cref="NotAValidVariableNameException">Thrown when the provided variable name is invalid or its format cannot be determined.</exception>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) ConvertTo(string variableName,
            RequestedVariableNameTypeEnum targetType)
        {
            var from = GetVariableFormat(variableName);
            if (from == ResultsVariableNameTypeEnum.Unknown)
                throw new NotAValidVariableNameException(variableName);

            if (from.ToString() == targetType.ToString()) return (variableName, from, targetType);

            var words = _getWords(variableName, from);
            var result = _internalConvert(words, targetType);
            return (result, from, targetType);
        }

        /// <summary>
        /// Attempts to convert the given variable name to the specified format type.
        /// </summary>
        /// <param name="variableName">The variable name to be converted.</param>
        /// <param name="targetType">The target format type to which the variable name should be converted.</param>
        /// <param name="output">An output tuple containing the converted variable name, the original format type, and the target format type.</param>
        /// <returns>A boolean value indicating whether the conversion was successful. Returns <c>false</c> if the target type is <c>Unknown</c>, if the input variable name format cannot be determined, or if the conversion fails.</returns>
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
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts a variable name to camelCase format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted.</param>
        /// <returns>A tuple containing the converted variable name, the original variable name type, and the target variable name type.
        /// The original variable name type is determined during the conversion process.</returns>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToCamelCase(string variableName) => ConvertTo(variableName, RequestedVariableNameTypeEnum.CamelCase);

        /// <summary>
        /// Attempts to convert a variable name to camelCase format while providing detailed transformation information.
        /// </summary>
        /// <param name="variableName">The variable name to be converted to camelCase format.</param>
        /// <param name="output">
        /// An output tuple containing the converted variable name as <c>result</c>,
        /// the original format of the variable name as <c>from</c>,
        /// and the target format (<see cref="RequestedVariableNameTypeEnum.CamelCase"/>) as <c>to</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the conversion to camelCase is successful; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryConvertToCamelCase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.CamelCase, out output);

        /// <summary>
        /// Converts the given variable name to PascalCase format.
        /// </summary>
        /// <param name="variableName">The variable name to convert to PascalCase.</param>
        /// <returns>A tuple containing the converted variable name, the original format as a <see cref="ResultsVariableNameTypeEnum"/> value, and the target format as <c>PascalCase</c>.</returns>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToPascalCase(string variableName) => ConvertTo(variableName, RequestedVariableNameTypeEnum.PascalCase);

        /// <summary>
        /// Attempts to convert a given variable name to PascalCase format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted.</param>
        /// <param name="output">
        /// A tuple containing the converted variable name, the original variable name format,
        /// and the target format. If conversion fails, the tuple values will be set to their defaults.
        /// </param>
        /// <returns>
        /// <c>true</c> if the variable name is successfully converted to PascalCase; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryConvertToPascalCase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.PascalCase, out output);

        /// <summary>
        /// Converts a given variable name to snake_case format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted to snake_case.</param>
        /// <returns>A tuple containing the converted variable name as <c>result</c>,
        /// the original variable name format as <c>from</c>, and the target format
        /// (<see cref="RequestedVariableNameTypeEnum.SnakeCase"/>) as <c>to</c>.</returns>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToSnakeCase(string variableName) => ConvertTo(variableName, RequestedVariableNameTypeEnum.SnakeCase);

        /// <summary>
        /// Attempts to convert the given variable name into snake_case format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted.</param>
        /// <param name="output">An output tuple containing the converted variable name, the original format type, and the target format type.</param>
        /// <returns><c>true</c> if the conversion is successful; otherwise, <c>false</c>.</returns>
        public static bool TryConvertToSnakeCase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.SnakeCase, out output);

        /// <summary>
        /// Converts the given variable name to Screaming Snake Case format.
        /// </summary>
        /// <param name="variableName">The variable name to be transformed into Screaming Snake Case.</param>
        /// <returns>A tuple containing the converted variable name, the original variable's naming format, and the target naming format (Screaming Snake Case).</returns>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToScreamingSnakeCase(string variableName) =>
            ConvertTo(variableName, RequestedVariableNameTypeEnum.ScreamingSnakeCase);

        /// <summary>
        /// Attempts to convert a given variable name to the ScreamingSnakeCase format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted to ScreamingSnakeCase.</param>
        /// <param name="output">
        /// An output tuple containing the converted variable name in ScreamingSnakeCase format,
        /// the original format of the variable name, and the target format (ScreamingSnakeCase).
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the conversion was successful. Returns <c>true</c> if the conversion succeeded; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryConvertToScreamingSnakeCase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.ScreamingSnakeCase, out output);

        /// <summary>
        /// Converts the given variable name to kebab-case format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted to kebab-case.</param>
        /// <returns>A tuple containing the converted variable name in kebab-case, the original variable name format as a <see cref="ResultsVariableNameTypeEnum"/>, and the target format <see cref="RequestedVariableNameTypeEnum.KebabCase"/>.</returns>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToKebabCase(string variableName) => ConvertTo(variableName, RequestedVariableNameTypeEnum.KebabCase);

        /// <summary>
        /// Attempts to convert the provided variable name to kebab-case format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted.</param>
        /// <param name="output">
        /// When this method returns, contains a tuple with the result string in kebab-case format,
        /// the original format of the variable name, and the target format, if the conversion was successful.
        /// Otherwise, it contains default values.
        /// </param>
        /// <returns>
        /// <c>true</c> if the variable name was successfully converted to kebab-case; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryConvertToKebabCase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.KebabCase, out output);

        /// <summary>
        /// Converts a variable name to TrainCase format while returning metadata about the conversion process.
        /// </summary>
        /// <param name="variableName">The variable name to be converted to TrainCase.</param>
        /// <returns>A tuple containing the converted variable name in TrainCase format as <c>result</c>,
        /// the original variable name format as <c>from</c>, and <c>to</c> as the target TrainCase format.</returns>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToTrainCase(string variableName) => ConvertTo(variableName, RequestedVariableNameTypeEnum.TrainCase);

        /// <summary>
        /// Attempts to convert the given variable name to TrainCase format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted.</param>
        /// <param name="output">
        /// When this method returns, contains a tuple with the converted variable name,
        /// the original format of the variable name, and the target format. This parameter
        /// is passed uninitialized and will only contain valid data if the conversion succeeds.
        /// </param>
        /// <returns>
        /// <c>true</c> if the variable name was successfully converted to TrainCase; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryConvertToTrainCase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.TrainCase, out output);

        /// <summary>
        /// Converts a variable name to the Unicase format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted to the Unicase format.</param>
        /// <returns>A tuple containing the resulting Unicase-formatted variable name, the original variable name's format as a <see cref="ResultsVariableNameTypeEnum"/> value, and the target format type as <see cref="RequestedVariableNameTypeEnum.Unicase"/>.</returns>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToUnicase(string variableName) => ConvertTo(variableName, RequestedVariableNameTypeEnum.Unicase);

        /// <summary>
        /// Attempts to convert the specified variable name to the Unicase format.
        /// </summary>
        /// <param name="variableName">The variable name to convert to the Unicase format.</param>
        /// <param name="output">
        /// When this method returns, contains a tuple with the converted variable name,
        /// the original format of the variable name, and the target format (Unicase), if the conversion was successful.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <c>true</c> if the variable name was successfully converted to the Unicase format;
        /// otherwise, <c>false</c>.
        /// </returns>
        public static bool TryConvertToUnicase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.Unicase, out output);

        /// <summary>
        /// Converts the given variable name to the TrollCase format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted.</param>
        /// <returns>A tuple containing the converted variable name in TrollCase format, the original format of the variable name, and the target format (TrollCase). Returns <c>Unknown</c> as the original format if it cannot be determined.</returns>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToTrollCase(string variableName) => ConvertTo(variableName, RequestedVariableNameTypeEnum.TrollCase);

        /// <summary>
        /// Attempts to convert the given variable name to TrollCase format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted to TrollCase format.</param>
        /// <param name="output">
        /// An output tuple that contains the converted variable name, the original format type,
        /// and the target format type (TrollCase).
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the conversion was successful. Returns <c>false</c>
        /// if the conversion could not be performed, such as when the variable name is invalid
        /// or unrecognized.
        /// </returns>
        public static bool TryConvertToTrollCase(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.TrollCase, out output);

        /// <summary>
        /// Converts the given variable name to the TitleWords format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted.</param>
        /// <returns>A tuple containing the converted variable name in TitleWords format, the original format of the variable name, and the target format (TitleWords).</returns>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToTitleWords(string variableName) => ConvertTo(variableName, RequestedVariableNameTypeEnum.TitleWords);

        /// <summary>
        /// Attempts to convert the given variable name to TitleWords format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted to TitleWords format.</param>
        /// <param name="output">
        /// An output tuple that contains the converted variable name, the original format type,
        /// and the target format type (TitleWords).
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the conversion was successful.
        /// </returns>
        public static bool TryConvertToTitleWords(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.TitleWords, out output);

        /// <summary>
        /// Converts the given variable name to the SentenceWords format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted.</param>
        /// <returns>A tuple containing the converted variable name in SentenceWords format, the original format of the variable name, and the target format (SentenceWords).</returns>
        public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to)
            ConvertToSentenceWords(string variableName) => ConvertTo(variableName, RequestedVariableNameTypeEnum.SentenceWords);

        /// <summary>
        /// Attempts to convert the given variable name to SentenceWords format.
        /// </summary>
        /// <param name="variableName">The variable name to be converted to SentenceWords format.</param>
        /// <param name="output">
        /// An output tuple that contains the converted variable name, the original format type,
        /// and the target format type (SentenceWords).
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the conversion was successful.
        /// </returns>
        public static bool TryConvertToSentenceWords(string variableName,
            out (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) output) =>
            TryConvertTo(variableName, RequestedVariableNameTypeEnum.SentenceWords, out output);

        /// <summary>
        /// Gets a formatted variable name from an object's type name.
        /// </summary>
        /// <param name="obj">The object whose type name will be used.</param>
        /// <param name="targetType">The desired variable name format (default is camelCase).</param>
        /// <returns>A formatted variable name string.</returns>
        public static string GetVariableName(object obj, RequestedVariableNameTypeEnum targetType = RequestedVariableNameTypeEnum.CamelCase)
        {
            return obj == null ? string.Empty : GetVariableName(obj.GetType(), targetType);
        }

        /// <summary>
        /// Gets a formatted variable name from a type or member name.
        /// </summary>
        /// <param name="member">The member or type whose name will be used.</param>
        /// <param name="targetType">The desired variable name format (default is camelCase).</param>
        /// <returns>A formatted variable name string.</returns>
        public static string GetVariableName(MemberInfo member, RequestedVariableNameTypeEnum targetType = RequestedVariableNameTypeEnum.CamelCase)
        {
            if (member == null) return string.Empty;
            var name = member.Name;
            if (name.Contains('`'))
            {
                name = name.Substring(0, name.IndexOf('`'));
            }
            return ConvertTo(name, targetType).result;
        }

        /// <summary>
        /// Gets a formatted class name from an object's type.
        /// </summary>
        /// <param name="obj">The object whose type name will be used.</param>
        /// <returns>A PascalCase formatted class name string.</returns>
        public static string GetClassName(object obj)
        {
            return obj == null ? string.Empty : GetClassName(obj.GetType());
        }

        /// <summary>
        /// Gets a formatted class name from a type.
        /// </summary>
        /// <param name="type">The type whose name will be used.</param>
        /// <returns>A PascalCase formatted class name string.</returns>
        public static string GetClassName(Type type)
        {
            return GetVariableName(type, RequestedVariableNameTypeEnum.PascalCase);
        }

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
                    return Regex.Split(variableName, @"(?=[A-Z])").Where(s => !string.IsNullOrEmpty(s)).ToArray();
                case ResultsVariableNameTypeEnum.Words:
                    return Regex.Split(variableName, @"[^a-zA-Z0-9]+")
                        .Where(s => !string.IsNullOrEmpty(s))
                        .SelectMany(p => Regex.Split(p, @"(?=[A-Z])"))
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToArray();
                case ResultsVariableNameTypeEnum.Unicase:
                case ResultsVariableNameTypeEnum.TrollCase:
                default:
                    return [variableName];
            }
        }

        private static readonly string[] _titleCaseMinorWords = 
        { 
            "a", "an", "the", 
            "and", "but", "for", "or", "nor", 
            "at", "by", "in", "of", "on", "to", "with", "from" 
        };

        private static string _internalConvert(string[] words, RequestedVariableNameTypeEnum targetType)
        {
            switch (targetType)
            {
                case RequestedVariableNameTypeEnum.CamelCase:
                    return words[0].ToLower() +
                           string.Concat(words.Skip(1).Select(w => char.ToUpper(w[0]) + w[1..].ToLower()));
                case RequestedVariableNameTypeEnum.PascalCase:
                    return string.Concat(words.Select(w => char.ToUpper(w[0]) + w[1..].ToLower()));
                case RequestedVariableNameTypeEnum.SnakeCase:
                    return string.Join("_", words.Select(w => w.ToLower()));
                case RequestedVariableNameTypeEnum.ScreamingSnakeCase:
                    return string.Join("_", words.Select(w => w.ToUpper()));
                case RequestedVariableNameTypeEnum.KebabCase:
                    return string.Join("-", words.Select(w => w.ToLower()));
                case RequestedVariableNameTypeEnum.TrainCase:
                    return string.Join("-", words.Select(w => char.ToUpper(w[0]) + w[1..].ToLower()));
                case RequestedVariableNameTypeEnum.Unicase:
                    return string.Concat(words).ToLower();
                case RequestedVariableNameTypeEnum.TrollCase:
                    return string.Concat(words).ToUpper();
                case RequestedVariableNameTypeEnum.TitleWords:
                    return string.Join(" ", words.Select((w, i) =>
                    {
                        var lower = w.ToLower();
                        if (i > 0 && i < words.Length - 1 && _titleCaseMinorWords.Contains(lower))
                        {
                            return lower;
                        }
                        return char.ToUpper(lower[0]) + lower[1..];
                    }));
                case RequestedVariableNameTypeEnum.SentenceWords:
                    return char.ToUpper(words[0][0]) + words[0][1..].ToLower() + 
                           (words.Length > 1 ? " " + string.Join(" ", words.Skip(1).Select(w => w.ToLower())) : "");
                default:
                    return string.Concat(words);
            }
        }
    }
}

public static class StringExtensions
{
    /// <summary>
    /// Converts the given variable name into the specified target format type.
    /// </summary>
    /// <param name="variableName">The variable name to be converted.</param>
    /// <param name="targetType">The target format type to which the variable name will be converted.</param>
    /// <returns>A tuple containing the converted variable name, the original format type, and the target format type. The original format type is determined based on the structure of the input variable name.</returns>
    /// <exception cref="ArgumentException">Thrown when the target format type is <c>Unknown</c>.</exception>
    /// <exception cref="NotAValidVariableNameException">Thrown when the provided variable name is invalid or its format cannot be determined.</exception>
    public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) ConvertTo(
        this string variableName, RequestedVariableNameTypeEnum targetType)
    {
        return Utilities.Variable.ConvertTo(variableName, targetType);
    }

    /// <summary>
    /// Checks whether the specified string represents a valid variable name.
    /// </summary>
    /// <param name="variableName">The string to be validated as a variable name.</param>
    /// <returns>A boolean value indicating whether the string is a valid variable name. Returns <c>false</c> if the validation fails or the format cannot be determined.</returns>
    public static bool IsVariableName(this string variableName)
    {
        return Utilities.Variable.TryGetVariableFormat(variableName, out var foundType) &&
               foundType != ResultsVariableNameTypeEnum.Unknown && foundType != ResultsVariableNameTypeEnum.Words;
    }

    /// <summary>
    /// Determines if the given variable name conforms to the specified variable format type.
    /// </summary>
    /// <param name="variableName">The variable name to be evaluated.</param>
    /// <param name="ofDesiredType">The desired <see cref="ResultsVariableNameTypeEnum"/> to check against.</param>
    /// <returns><c>true</c> if the variable name matches the specified format; otherwise, <c>false</c>.</returns>
    public static bool IsVariableName(this string variableName, ResultsVariableNameTypeEnum ofDesiredType)
    {
        var okay = Utilities.Variable.TryGetVariableFormat(variableName, out var foundType);
        return okay && foundType == ofDesiredType;
    }
}

public static class MemberExtensions
{
    /// <summary>
    /// Gets a formatted variable name from a member's name.
    /// </summary>
    /// <param name="member">The member to format.</param>
    /// <param name="targetType">The target variable naming convention.</param>
    /// <returns>A string representing the member name in the requested format.</returns>
    public static string ToVariableName(this MemberInfo member, RequestedVariableNameTypeEnum targetType = RequestedVariableNameTypeEnum.CamelCase)
        => Utilities.Variable.GetVariableName(member, targetType);

    /// <summary>
    /// Gets a PascalCase formatted class name from a type.
    /// </summary>
    /// <param name="type">The type to format.</param>
    /// <returns>A PascalCase formatted string of the type name.</returns>
    public static string ToClassName(this Type type)
        => Utilities.Variable.GetClassName(type);
}

public static class ObjectExtensions
{
    /// <summary>
    /// Gets a formatted variable name from an object instance's type.
    /// </summary>
    /// <param name="obj">The object whose type name will be formatted.</param>
    /// <param name="targetType">The target variable naming convention.</param>
    /// <returns>A string representing the instance's type name in the requested format.</returns>
    public static string ToVariableName(this object obj, RequestedVariableNameTypeEnum targetType = RequestedVariableNameTypeEnum.CamelCase)
        => Utilities.Variable.GetVariableName(obj, targetType);

    /// <summary>
    /// Gets a PascalCase formatted class name from an object instance's type.
    /// </summary>
    /// <param name="obj">The object whose type name will be formatted.</param>
    /// <returns>A PascalCase formatted string of the instance's type name.</returns>
    public static string ToClassName(this object obj)
        => Utilities.Variable.GetClassName(obj);
}