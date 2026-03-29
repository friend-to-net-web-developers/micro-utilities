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
    /// Creates a <see cref="PunyUniResult"/> by analysing the provided domain string.
    /// Determines whether the input is PlainAscii, Unicode, Punycode, or Mixed, and
    /// produces both the Unicode and Punycode canonical forms.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Suspicious flag:</b> Results are marked suspicious when the input form is
    /// <see cref="DomainInputForm.Punycode"/>, <see cref="DomainInputForm.Mixed"/>, or
    /// <see cref="DomainInputForm.Invalid"/>. A human submitting raw Punycode
    /// (<c>xn--mnchen-3ya.de</c> instead of <c>münchen.de</c>) is unusual and worth
    /// surfacing to the caller for review, even though normalization can still succeed.
    /// </para>
    /// <para>
    /// <b>PlainAscii validation:</b> Plain ASCII domains (no Unicode, no <c>xn--</c> labels)
    /// are still run through <see cref="IdnMapping.GetAscii"/> to validate structural
    /// correctness — label length limits, illegal characters, and so on. A string like
    /// <c>"not a domain!"</c> will produce <see cref="DomainInputForm.Invalid"/> rather
    /// than passing through unchecked.
    /// </para>
    /// </remarks>
    /// <param name="input">The domain string to analyse. Null or empty returns an Invalid result.</param>
    /// <returns>
    /// A <see cref="PunyUniResult"/> containing the input, its Unicode and Punycode forms,
    /// the detected <see cref="DomainInputForm"/>, and a suspicious-input flag.
    /// </returns>
    public static PunyUniResult From(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return new PunyUniResult(input, null, null, DomainInputForm.Invalid, true);

        var form = Detect(input);

        if (form is DomainInputForm.Invalid)
            return new PunyUniResult(input, null, null, DomainInputForm.Invalid, true);

        var suspicious = form is DomainInputForm.Punycode
            or DomainInputForm.Mixed
            or DomainInputForm.Invalid;

        var idn = new IdnMapping();

        try
        {
            string punycode;
            string unicode;

            switch (form)
            {
                case DomainInputForm.Punycode:
                case DomainInputForm.Mixed:
                    // Input is already (partially) Punycode — decode to Unicode first,
                    // then re-encode to get a clean canonical Punycode form.
                    unicode = idn.GetUnicode(input);
                    punycode = idn.GetAscii(unicode);
                    break;

                case DomainInputForm.Unicode:
                    // Pure Unicode input — encode to Punycode.
                    punycode = idn.GetAscii(input);
                    unicode = idn.GetUnicode(punycode);
                    break;

                case DomainInputForm.PlainAscii:
                default:
                    // Plain ASCII — run through GetAscii to validate structure
                    // (label lengths, illegal characters, etc.) even though no
                    // encoding is needed. A structurally invalid domain like
                    // "not a domain!" will throw ArgumentException here.
                    punycode = idn.GetAscii(input);
                    unicode = idn.GetUnicode(punycode);
                    break;
            }

            return new PunyUniResult(input, unicode, punycode, form, suspicious);
        }
        catch (ArgumentException)
        {
            // IdnMapping throws ArgumentException for malformed labels —
            // illegal characters, labels exceeding 63 chars, empty labels, etc.
            return new PunyUniResult(input, null, null, DomainInputForm.Invalid, true);
        }
    }

    /// <summary>
    /// Detects the <see cref="DomainInputForm"/> of a domain string by inspecting its labels.
    /// </summary>
    /// <remarks>
    /// Detection is based on the presence of <c>xn--</c> prefixed labels (Punycode) and
    /// code points above U+007F (Unicode). A domain with both is <see cref="DomainInputForm.Mixed"/>,
    /// which is structurally malformed and treated as suspicious.
    /// </remarks>
    /// <param name="domain">The domain string to inspect. Must not be null.</param>
    /// <returns>The detected <see cref="DomainInputForm"/>.</returns>
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