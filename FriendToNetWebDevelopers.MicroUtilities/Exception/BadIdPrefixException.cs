namespace FriendToNetWebDevelopers.MicroUtilities.Exception;

public class BadIdPrefixException(string? prefix)
    : ApplicationException($"The given html id prefix is invalid: \"{prefix}\"");