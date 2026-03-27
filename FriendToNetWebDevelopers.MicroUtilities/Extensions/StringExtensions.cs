using FriendToNetWebDevelopers.MicroUtilities.Enum;
using FriendToNetWebDevelopers.MicroUtilities.Exception;
using FriendToNetWebDevelopers.MicroUtilities.Models;
using FriendToNetWebDevelopers.MicroUtilities.Models.EmailAnnotator;
using FriendToNetWebDevelopers.MicroUtilities.Models.TextAnnotator;

namespace FriendToNetWebDevelopers.MicroUtilities.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Annotates the specified address and provides detailed analysis of its structural components.
    /// </summary>
    /// <param name="address">The address string to be annotated. This can be an email address or URI.</param>
    /// <param name="mode">The input mode indicating the type of address being provided.</param>
    /// <returns>An <c>AddressPartAnnotation</c> object containing detailed information about the parts of the address, including tokens, unicode presence, and structural properties.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="address"/> parameter is null.</exception>
    public static AddressPartAnnotation Annotate(this string address, InputMode mode)
        => Utilities.AddressAnnotator.Annotate(address, mode);

    /// <summary>
    /// Annotates the given text and provides detailed character-level analysis, including Unicode information and type classification.
    /// </summary>
    /// <param name="text">The input text to be annotated. Each character in the text will be analyzed individually.</param>
    /// <returns>A collection of <c>TextCharacterToken</c> objects, where each token represents an analyzed character from the input text, including its index, type, code point, and Unicode escape sequence.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="text"/> parameter is null.</exception>
    public static IEnumerable<TextCharacterToken> Annotate(this string text)
        => Utilities.TextAnnotator.Annotate(text);

    /// <summary>
    /// Converts the given variable name into the specified target format type.
    /// </summary>
    /// <param name="address">The variable name string to convert.</param>
    /// <param name="targetType">The target format type to which the variable name will be converted.</param>
    /// <returns>A tuple containing the converted variable name, the original format type, and the target format type. The original format type is determined based on the structure of the input variable name.</returns>
    /// <exception cref="ArgumentException">Thrown when the target format type is <c>Unknown</c>.</exception>
    /// <exception cref="NotAValidVariableNameException">Thrown when the provided variable name is invalid or its format cannot be determined.</exception>
    public static (string result, ResultsVariableNameTypeEnum from, RequestedVariableNameTypeEnum to) ConvertTo(this string address, RequestedVariableNameTypeEnum targetType)
    {
        return Utilities.Variable.ConvertTo(address, targetType);
    }

    /// <summary>
    /// Checks whether the specified string represents a valid variable name.
    /// </summary>
    /// <param name="text">The string to validate.</param>
    /// <returns>A boolean value indicating whether the string is a valid variable name. Returns <c>false</c> if the validation fails or the format cannot be determined.</returns>
    public static bool IsVariableName(this string text)
    {
        return Utilities.Variable.TryGetVariableFormat(text, out var foundType) &&
               foundType != ResultsVariableNameTypeEnum.Unknown && foundType != ResultsVariableNameTypeEnum.Words;
    }

    /// <summary>
    /// Determines if the given variable name conforms to the specified variable format type.
    /// </summary>
    /// <param name="text">The variable name string to check.</param>
    /// <param name="ofDesiredType">The desired <see cref="ResultsVariableNameTypeEnum"/> to check against.</param>
    /// <returns><c>true</c> if the variable name matches the specified format; otherwise, <c>false</c>.</returns>
    public static bool IsVariableName(this string text, ResultsVariableNameTypeEnum ofDesiredType)
    {
        var okay = Utilities.Variable.TryGetVariableFormat(text, out var foundType);
        return okay && foundType == ofDesiredType;
    }

    /// <summary>
    /// Converts the specified input string into a <c>PunyUniResult</c> object, analyzing its format, derived representations, and suspiciousness.
    /// </summary>
    /// <param name="address">The domain string to parse.</param>
    /// <returns>A <c>PunyUniResult</c> object containing the analyzed input, its Unicode and Punycode representations,
    /// the detected input form, and a flag indicating whether it is suspicious.</returns>
    public static PunyUniResult ToParsedDomainPunyUniResult(this string address)
        => PunyUniResult.From(address);

    /// <summary>
    /// Converts the specified text into its Unicode-escaped ASCII representation.
    /// </summary>
    /// <param name="text">The input string to be converted. This string may contain Unicode characters.</param>
    /// <returns>A string where all Unicode characters in the input have been replaced with their escaped ASCII representations.</returns>
    public static string ToUnicodeEscapedAscii(this string text)
        => Utilities.Text.EncodeUnicodeEscapes(text);

    /// <summary>
    /// Converts a string containing Unicode-escaped ASCII sequences into their decoded Unicode character equivalents.
    /// </summary>
    /// <param name="text">The input string containing Unicode-escaped sequences (e.g., "\u0041" for 'A').</param>
    /// <returns>A string in which all Unicode-escaped sequences are replaced with their corresponding Unicode characters.</returns>
    public static string ToUnicodeDecoded(this string text)
        => Utilities.Text.DecodeUnicodeEscapes(text);
}