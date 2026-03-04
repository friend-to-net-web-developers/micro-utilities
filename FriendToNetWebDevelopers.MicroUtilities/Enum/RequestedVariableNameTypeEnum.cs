namespace FriendToNetWebDevelopers.MicroUtilities.Enum;

public enum ResultsVariableNameTypeEnum
{
    /// <summary>
    /// Represents the camelCase naming convention where the first letter of the first word
    /// is lowercase and the first letter of subsequent words is uppercase, with no separating characters.
    /// </summary>
    /// <remarks>
    /// camelCase
    /// </remarks>
    CamelCase,

    /// <summary>
    /// Represents the PascalCase naming convention where the first letter of each word,
    /// including the first word, is uppercase with no separating characters.
    /// </summary>
    /// <remarks>
    /// PascalCase
    /// </remarks>
    PascalCase,

    /// <summary>
    /// Represents the snake_case naming convention where words are separated by underscores,
    /// and all characters are typically in lowercase.
    /// </summary>
    /// <remarks>
    /// snake_case
    /// </remarks>
    SnakeCase,

    /// <summary>
    /// Represents the SCREAMING_SNAKE_CASE naming convention where all letters are uppercase
    /// and words are separated by an underscore.
    /// </summary>
    /// <remarks>
    /// SCREAMING_SNAKE_CASE
    /// </remarks>
    ScreamingSnakeCase,

    /// <summary>
    /// Represents the kebab-case naming convention where words are lowercase
    /// and separated by hyphens (-) without spaces or additional capitalization.
    /// </summary>
    /// <remarks>
    /// kebab-case
    /// </remarks>
    KebabCase,
    
    /// <summary>
    /// Represents the Train-Case naming convention where words are uppercase
    /// and separated by hyphens (-) without spaces or additional capitalization.
    /// </summary>
    /// <remarks>
    /// Train-Case
    /// </remarks>
    TrainCase,

    /// <summary>
    /// Represents a naming convention where all characters in the variable name are lowercase,
    /// with no separators or distinctive capitalization for word boundaries.
    /// </summary>
    /// <remarks>
    /// unicasevariablename
    /// </remarks>
    Unicase,

    /// <summary>
    /// Represents a naming convention where all characters in the variable name are uppercase,
    /// with no separators or distinctive capitalization for word boundaries. Can be included in
    /// unicase naming conventions but kept separate for clarity.
    /// </summary>
    /// <remarks>
    /// TROLLCASEVARIABLENAME
    /// </remarks>
    TrollCase,
    
    /// <summary>
    /// It's a collection of words and is not a valid variable format
    /// </summary>
    Words,
    
    /// <summary>
    /// Looks like words but does not have any characters which can be converted into a variable name
    /// </summary>
    Unknown
}

public enum RequestedVariableNameTypeEnum
{
    /// <summary>
    /// Represents the camelCase naming convention where the first letter of the first word
    /// is lowercase and the first letter of subsequent words is uppercase, with no separating characters.
    /// </summary>
    /// <remarks>
    /// camelCase
    /// </remarks>
    CamelCase,

    /// <summary>
    /// Represents the PascalCase naming convention where the first letter of each word,
    /// including the first word, is uppercase with no separating characters.
    /// </summary>
    /// <remarks>
    /// PascalCase
    /// </remarks>
    PascalCase,

    /// <summary>
    /// Represents the snake_case naming convention where words are separated by underscores,
    /// and all characters are typically in lowercase.
    /// </summary>
    /// <remarks>
    /// snake_case
    /// </remarks>
    SnakeCase,

    /// <summary>
    /// Represents the SCREAMING_SNAKE_CASE naming convention where all letters are uppercase
    /// and words are separated by an underscore.
    /// </summary>
    /// <remarks>
    /// SCREAMING_SNAKE_CASE
    /// </remarks>
    ScreamingSnakeCase,

    /// <summary>
    /// Represents the kebab-case naming convention where words are lowercase
    /// and separated by hyphens (-) without spaces or additional capitalization.
    /// </summary>
    /// <remarks>
    /// kebab-case
    /// </remarks>
    KebabCase,
    
    /// <summary>
    /// Represents the Train-Case naming convention where words are uppercase
    /// and separated by hyphens (-) without spaces or additional capitalization.
    /// </summary>
    /// <remarks>
    /// Train-Case
    /// </remarks>
    TrainCase,

    /// <summary>
    /// Represents a naming convention where all characters in the variable name are lowercase,
    /// with no separators or distinctive capitalization for word boundaries.
    /// </summary>
    /// <remarks>
    /// unicasevariablename
    /// </remarks>
    Unicase,

    /// <summary>
    /// Represents a naming convention where all characters in the variable name are uppercase,
    /// with no separators or distinctive capitalization for word boundaries. Can be included in
    /// unicase naming conventions but kept separate for clarity.
    /// </summary>
    /// <remarks>
    /// TROLLCASEVARIABLENAME
    /// </remarks>
    TrollCase,

    /// <summary>
    /// Represents a naming convention where each word in the variable name starts with an uppercase letter
    /// and is separated by a single space.
    /// </summary>
    /// <remarks>
    /// Title Words
    /// </remarks>
    TitleWords,

    /// <summary>
    /// Represents a naming convention where words in a sentence are separated by spaces,
    /// and the first letter of the first word is uppercase.
    /// </summary>
    /// <remarks>
    /// Sentence words
    /// </remarks>
    SentenceWords,
}