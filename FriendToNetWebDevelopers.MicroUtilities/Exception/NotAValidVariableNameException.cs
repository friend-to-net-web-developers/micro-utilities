namespace FriendToNetWebDevelopers.MicroUtilities.Exception;

public class NotAValidVariableNameException(string message) : IOException(message);