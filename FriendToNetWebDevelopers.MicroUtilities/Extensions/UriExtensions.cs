using FriendToNetWebDevelopers.MicroUtilities.Models;
using FriendToNetWebDevelopers.MicroUtilities.Models.EmailAnnotator;

namespace FriendToNetWebDevelopers.MicroUtilities.Extensions;


public static class UriExtensions
{
    /// <summary>
    /// Analyzes the user information component of the specified URI and provides a detailed annotation.
    /// </summary>
    /// <param name="uri">The URI containing the user information to annotate.</param>
    /// <returns>An <see cref="AddressPartAnnotation"/> instance containing details about the user information,
    /// including its structure, tokens, and potential issues such as invalid or suspicious characters.
    /// </returns>
    public static AddressPartAnnotation AnnotateUserInfo(this Uri uri)
        => Utilities.AddressAnnotator.AnnotateUserInfo(uri);

    /// <summary>
    /// Analyzes the host component of the specified URI and provides a detailed annotation.
    /// </summary>
    /// <param name="uri">The URI containing the host to annotate.</param>
    /// <returns>An <see cref="AddressPartAnnotation"/> instance containing details about the host,
    /// including its structure, tokens, and potential issues such as invalid or suspicious characters.
    /// </returns>
    public static AddressPartAnnotation AnnotateHost(this Uri uri)
        => Utilities.AddressAnnotator.AnnotateHost(uri);

    /// <summary>
    /// Converts the specified input string into a <c>PunyUniResult</c> object, analyzing its format, derived representations, and suspiciousness.
    /// </summary>
    /// <param name="uri">The URI whose host will be parsed.</param>
    /// <returns>A <c>PunyUniResult</c> object containing the analyzed input, its Unicode and Punycode representations,
    /// the detected input form, and a flag indicating whether it is suspicious.</returns>
    public static PunyUniResult ToParsedDomainPunyUniResult(this Uri uri)
        => PunyUniResult.From(uri.Host);
}
