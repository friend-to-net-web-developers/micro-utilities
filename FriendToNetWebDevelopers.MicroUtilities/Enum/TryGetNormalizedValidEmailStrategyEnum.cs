namespace FriendToNetWebDevelopers.MicroUtilities.Enum;

/// <summary>
/// Defines a set of strategies for normalizing and validating email addresses.
/// </summary>
/// <remarks>
/// This enumeration represents individual and combined strategies for processing email
/// addresses during normalization and validation operations. The flags can be
/// combined to implement multiple strategies.
/// </remarks>
[Flags]
public enum TryGetNormalizedValidEmailStrategyEnum
{
    // Nothing is a strategy, of sorts
    None = 0,
    // Actual Options
    ToLower = 1 << 0,
    Trim = 1 << 1,
    DropTag = 1 << 2,
    DropDot = 1 << 3,
    // Combinations
    All = ToLower | Trim | DropTag | DropDot,
    LowerAndTrim = ToLower | Trim,
    LowerAndDropTag = ToLower | DropTag,
    LowerAndDropDot = ToLower | DropDot,
    LowerAndTrimAndDropTag = ToLower | Trim | DropTag,
    LowerAndTrimAndDropDot = ToLower | Trim | DropDot,
    LowerAndDropTagAndDropDot = ToLower | DropTag | DropDot,
    TrimAndDropTag = Trim | DropTag,
    TrimAndDropDot = Trim | DropDot,
    TrimAndDropDotAndDropTag = Trim | DropDot | DropTag,
    DropTagAndDropDot = DropTag | DropDot,
}