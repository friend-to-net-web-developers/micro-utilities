using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;
using FriendToNetWebDevelopers.MicroUtilities.Models.Annotator;

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    /// <summary>
    /// Provides methods to process and annotate address strings, such as email addresses or URIs,
    /// by analyzing their structure, tokens, and character properties.
    /// </summary>
    public static class AddressAnnotator
    {
        // Known homoglyphs: Unicode char → visually similar ASCII char.
        // Extend this table as needed; sourced from Unicode TR39 confusables.
        private static readonly IReadOnlyDictionary<int, char> Homoglyphs =
            new Dictionary<int, char>
            {
                // Cyrillic
                [0x0430] = 'a', [0x0435] = 'e', [0x043E] = 'o', [0x0440] = 'p',
                [0x0441] = 'c', [0x0445] = 'x', [0x0443] = 'y', [0x0456] = 'i',
                // Greek
                [0x03BF] = 'o', [0x03B1] = 'a', [0x03B5] = 'e', [0x03BD] = 'v',
                // Latin lookalikes (extended)
                [0x00F6] = 'o', [0x00FC] = 'u', [0x00E4] = 'a',
            };

        // Email-allowed ASCII special characters per RFC 5321 local-part
        private static readonly HashSet<char> EmailSpecialChars =
            ['!', '#', '$', '%', '&', '\'', '*', '+', '-', '/', '=', '?', '^', '_', '`', '{', '|', '}', '~', '.'];

        // URI userinfo allowed ASCII special characters per RFC 3986
        private static readonly HashSet<char> UriSpecialChars =
            ['!', '$', '&', '\'', '(', ')', '*', '+', ',', ';', '=', '-', '.', '_', '~', '%'];

        /// <summary>
        /// Analyzes and annotates the user information portion of a given URI by evaluating its structure,
        /// tokens, and relevant character properties, excluding sensitive parts such as passwords.
        /// </summary>
        /// <param name="uri">
        /// The URI containing the user information to be analyzed. This should be a valid URI from which
        /// the user information will be extracted and annotated.
        /// </param>
        /// <returns>
        /// An <see cref="AddressPartAnnotation"/> object containing the annotated user information
        /// along with associated metadata including tokens, suspicious or invalid characters, and Unicode indicators.
        /// </returns>
        public static AddressPartAnnotation AnnotateUserInfo(Uri uri)
        {
            // Re-parse from the raw original string so nothing has been decoded
            var original = uri.OriginalString;
    
            // Find the authority section (after :// )
            var authorityStart = original.IndexOf("://", StringComparison.Ordinal);
            var authority = authorityStart >= 0
                ? original[(authorityStart + 3)..]
                : original;

            // Strip anything after the authority (path, query, fragment)
            var authorityEnd = authority.IndexOfAny(['/', '?', '#']);
            if (authorityEnd >= 0)
                authority = authority[..authorityEnd];

            // Find @ to identify userInfo
            var atIndex = authority.IndexOf('@');
            if (atIndex < 0)
                return Annotate(string.Empty, InputMode.Uri);

            var userInfo = authority[..atIndex];
            var passwordColon = userInfo.IndexOf(':');
            if (passwordColon >= 0)
                userInfo = userInfo[..passwordColon]; // drop ":password" part

            return Annotate(userInfo, InputMode.Uri);
        }

        /// <summary>
        /// Analyzes and annotates the host part of a given URI by evaluating its structure,
        /// tokens, and relevant character properties.
        /// </summary>
        /// <param name="uri">
        /// The URI to be analyzed. This parameter should contain a valid URI in which
        /// the host part will be extracted and annotated.
        /// </param>
        /// <returns>
        /// An <see cref="AddressPartAnnotation"/> object that includes the raw host,
        /// tokenized representation, and metadata about the host's character types
        /// such as suspicious or invalid characters.
        /// </returns>
        public static AddressPartAnnotation AnnotateHost(Uri uri) => Annotate(uri.Host, InputMode.Uri);

        /// <summary>
        /// Processes the given email address and generates an annotation containing detailed
        /// information about its structure, tokens, and character properties.
        /// </summary>
        /// <param name="address">
        /// The email address to be annotated. This is passed as a <see cref="MailAddress"/> object
        /// and contains both the local and domain parts of the email.
        /// </param>
        /// <returns>
        /// An <see cref="AddressPartAnnotation"/> object containing the raw email address,
        /// separated local and domain parts, tokens for each part, and metadata such as whether
        /// it contains Unicode, suspicious characters, or invalid characters.
        /// </returns>
        public static AddressPartAnnotation Annotate(MailAddress address) => Annotate(address.Address, InputMode.Email);
        
        /// <summary>
        /// Processes the given address string and generates an annotation containing detailed
        /// information about its structure, tokens, and character properties.
        /// </summary>
        /// <param name="address">
        /// The input address string to be annotated. This can represent an email address or a URI,
        /// depending on the provided input mode.
        /// </param>
        /// <param name="mode">
        /// Specifies the processing mode for the annotation. Set to <see cref="InputMode.Email"/>
        /// for email processing or <see cref="InputMode.Uri"/> for URI processing. Defaults to
        /// <see cref="InputMode.Email"/>.
        /// </param>
        /// <returns>
        /// An <see cref="AddressPartAnnotation"/> object containing the raw address,
        /// separated local and domain parts, tokens for each part, and metadata such as
        /// whether it contains Unicode, suspicious characters, or invalid characters.
        /// </returns>
        public static AddressPartAnnotation Annotate(string address, InputMode mode = InputMode.Email)
        {
            var atIndex = address.LastIndexOf('@');

            var local = atIndex >= 0 ? address[..atIndex] : address;
            var domain = atIndex >= 0 ? address[(atIndex + 1)..] : string.Empty;

            var localTokens = _tokenizeSegment(local, mode);
            var domainTokens = _tokenizeSegment(domain, mode);

            var allTokens = localTokens.Concat(domainTokens).ToList();

            return new AddressPartAnnotation(
                Raw: address,
                LocalPart: local,
                Domain: domain,
                Mode: mode,
                LocalTokens: localTokens,
                DomainTokens: domainTokens,
                ContainsUnicode: allTokens.Any(t => t.Type == CharacterType.Unicode),
                ContainsSuspiciousChars: allTokens.Any(t => t.IsSuspicious),
                ContainsInvalidChars: allTokens.Any(t => t.Type == CharacterType.Invalid)
            );
        }

        private static IReadOnlyList<CharacterToken> _tokenizeSegment(string segment, InputMode mode)
        {
            var tokens = new List<CharacterToken>();
            var i = 0;

            while (i < segment.Length)
            {
                // ── Percent-encoded sequence (%XX) ──────────────────────────────
                if (mode == InputMode.Uri && segment[i] == '%'
                                          && i + 2 < segment.Length
                                          && _isHexChar(segment[i + 1]) && _isHexChar(segment[i + 2]))
                {
                    var encoded = segment.Substring(i, 3);
                    tokens.Add(new CharacterToken(
                        Char: encoded,
                        Codepoint: $"U+{(int)segment[i]:X4}",
                        Type: CharacterType.PercentEncoded,
                        Script: null,
                        HomoglyphOf: null,
                        IsEmailStructural: false,
                        IsSuspicious: false
                    ));
                    i += 3;
                    continue;
                }

                // ── Multi-char grapheme cluster (surrogate pairs, combining chars) ─
                var codepoint = char.IsHighSurrogate(segment[i]) && i + 1 < segment.Length
                    ? char.ConvertToUtf32(segment[i], segment[i + 1])
                    : segment[i];
                var charLen = codepoint > 0xFFFF ? 2 : 1;
                var ch = segment.Substring(i, charLen);

                tokens.Add(_classifyChar(ch, codepoint, mode));
                i += charLen;
            }

            return tokens;
        }

        private static CharacterToken _classifyChar(string ch, int codepoint, InputMode mode)
        {
            var codepointStr = $"U+{codepoint:X4}";

            switch (codepoint)
            {
                // ── @ separator ─────────────────────────────────────────────────────
                case '@':
                    return new CharacterToken(ch, codepointStr,
                        mode == InputMode.Email
                            ? CharacterType.EmailStructural
                            : CharacterType.UriStructural,
                        null, null, true, false);
                // ── ASCII range ──────────────────────────────────────────────────────
                case <= 0x7F:
                {
                    var c = (char)codepoint;

                    if (char.IsLetter(c))
                        return new CharacterToken(ch, codepointStr,
                            CharacterType.AsciiAlpha, "Basic Latin", null, false, false);

                    if (char.IsDigit(c))
                        return new CharacterToken(ch, codepointStr,
                            CharacterType.AsciiDigit, "Basic Latin", null, false, false);

                    // Structural: dot is structural in both modes;
                    // + is structural/semantic in email only
                    var isDot = c == '.';
                    var isPlus = c == '+';
                    var isStructural = isDot || (isPlus && mode == InputMode.Email);

                    if (isStructural)
                        return new CharacterToken(ch, codepointStr,
                            mode == InputMode.Email
                                ? CharacterType.EmailStructural
                                : CharacterType.UriStructural,
                            null, null, isStructural, false);

                    // Allowed specials
                    var allowed = mode == InputMode.Email
                        ? EmailSpecialChars.Contains(c)
                        : UriSpecialChars.Contains(c);

                    return new CharacterToken(ch, codepointStr,
                        allowed ? CharacterType.AsciiSpecial : CharacterType.Invalid,
                        null, null, false, !allowed);
                }
            }

            // ── Unicode (non-ASCII) ──────────────────────────────────────────────
            var script = _getScript(codepoint);
            var homoglyph = Homoglyphs.TryGetValue(codepoint, out char similar)
                ? similar.ToString()
                : null;
            var suspicious = homoglyph != null;

            return new CharacterToken(ch, codepointStr,
                CharacterType.Unicode, script, homoglyph, false, suspicious);
        }

        private static string _getScript(int codepoint) => codepoint switch
        {
            >= 0x0400 and <= 0x04FF => "Cyrillic",
            >= 0x0370 and <= 0x03FF => "Greek",
            >= 0x0600 and <= 0x06FF => "Arabic",
            >= 0x4E00 and <= 0x9FFF => "CJK Unified Ideographs",
            >= 0x3040 and <= 0x309F => "Hiragana",
            >= 0x30A0 and <= 0x30FF => "Katakana",
            >= 0x0080 and <= 0x024F => "Latin Extended",
            >= 0x0250 and <= 0x02AF => "IPA Extensions", //... and who doesn't love a good IPA?
            >= 0x1E00 and <= 0x1EFF => "Latin Extended Additional",
            _ => "Unknown"
        };

        private static bool _isHexChar(char c) => c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';

        // ── JSON helper ───────────────────────────────────────────────────────────

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string ToJson(AddressPartAnnotation annotation) =>
            JsonSerializer.Serialize(annotation, JsonOptions);
    }
}

