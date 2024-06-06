namespace FriendToNetWebDevelopers.MicroUtilities.Exception;

public class BadYoutubeIdException(string youtubeId)
    : ApplicationException($"The given youtube id prefix is invalid: \"{youtubeId}\"");