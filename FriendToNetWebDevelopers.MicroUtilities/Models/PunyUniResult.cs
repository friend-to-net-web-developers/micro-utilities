using System.Globalization;
using FriendToNetWebDevelopers.MicroUtilities.Models.EmailAnnotator;

namespace FriendToNetWebDevelopers.MicroUtilities.Models;

public record PunyUniResult(
    string? Input,
    string? Unicode,
    string? Punycode,
    DomainInputForm InputForm,
    bool IsSuspicious)
{
    /// <summary>
    /// Creates a new instance of the <see cref="PunyUniResult"/> class by analyzing the provided domain input.
    /// This method determines whether the input is in Punycode, Unicode, or a mixed form and evaluates its validity and suspiciousness.
    /// </summary>
    /// <param name="input">The domain input to be analyzed. This can be in Plain ASCII, Unicode, or Punycode formats.</param>
    /// <returns>Returns an instance of the <see cref="PunyUniResult"/> class containing the analyzed input,
    /// derived Unicode and Punycode representations, the detected input form, and an indicator of whether the input is suspicious.
    /// </returns>
    public static PunyUniResult From(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return new PunyUniResult(input, null, null, DomainInputForm.Invalid, true);

        var form = Detect(input);
        var suspicious = form is DomainInputForm.Punycode
            or DomainInputForm.Mixed
            or DomainInputForm.Invalid;
        try
        {
            var idn = new IdnMapping();
            var unicode = form is DomainInputForm.Punycode or DomainInputForm.Mixed
                ? idn.GetUnicode(input)
                : input;
            var punycode = form is DomainInputForm.Unicode or DomainInputForm.Mixed
                ? idn.GetAscii(input)
                : input;

            return new PunyUniResult(input, unicode, punycode, form, suspicious);
        }
        catch
        {
            return new PunyUniResult(input, null, null, DomainInputForm.Invalid, true);
        }
    }

    private static DomainInputForm Detect(string domain)
    {
        var labels = domain.Split('.');
        var hasPuny = labels.Any(l => l.StartsWith("xn--", StringComparison.OrdinalIgnoreCase));
        var hasUnicode = labels.Any(l => l.Any(c => c > 0x7F));

        return (hasPuny, hasUnicode) switch
        {
            (true, true) => DomainInputForm.Mixed,
            (true, false) => DomainInputForm.Punycode,
            (false, true) => DomainInputForm.Unicode,
            (false, false) => DomainInputForm.PlainAscii
        };
    }
}