using System.Net.Mail;
using FriendToNetWebDevelopers.MicroUtilities.Models.Annotator;

namespace FriendToNetWebDevelopers.MicroUtilities.Extensions;

public static class MailAddressExtensions
{
    /// <summary>
    /// Analyzes a specified <see cref="MailAddress"/> object and returns a detailed annotation
    /// identifying components such as local part, domain, and various character attributes.
    /// </summary>
    /// <param name="address">The email address to annotate, represented as a <see cref="MailAddress"/> object.</param>
    /// <returns>An <see cref="AddressPartAnnotation"/> object containing detailed information about the email address.</returns>
    public static AddressPartAnnotation Annotate(this MailAddress address)
        => Utilities.AddressAnnotator.Annotate(address);
}