namespace FriendToNetWebDevelopers.MicroUtilities.Enum;

/// <summary>
/// Defines the fallback strategy used by
/// <see cref="Utilities.Id.TryGetAsValidId(string?, TryGetValidIdDefaultStrategyEnum, out string)"/>
/// when the proposed id value is invalid.
/// </summary>
public enum TryGetValidIdDefaultStrategyEnum
{
    /// <summary>
    /// Returns <see cref="string.Empty"/> when the proposed id is invalid.
    /// Use this when you want to handle the invalid case yourself — for example,
    /// to omit the id attribute entirely or apply a custom fallback.
    /// </summary>
    EmptyOnInvalid,

    /// <summary>
    /// Returns a freshly generated valid id (via <see cref="Utilities.Id.GetValidHtmlId()"/>)
    /// when the proposed id is invalid.
    /// Use this when a valid id is always required and the specific value does not matter.
    /// </summary>
    GenerateOnInvalid
}