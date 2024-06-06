namespace FriendToNetWebDevelopers.MicroUtilities.Exception;

public class BadIdFormatException(string? id)
    : ApplicationException($"The given html id is invalid: \"{id}\"");