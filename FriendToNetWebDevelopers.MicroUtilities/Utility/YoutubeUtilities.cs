using System.Text.RegularExpressions;
using FriendToNetWebDevelopers.MicroUtilities.Enum;
using FriendToNetWebDevelopers.MicroUtilities.Exception;

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    /// <summary>
    /// Handles YouTube-related URL tasks including ID validation, thumbnail URL generation,
    /// and embed URL generation.
    /// </summary>
    /// <remarks>
    /// Makes deliberate use of URI building and validation to harden against malformed or
    /// hostile input.
    /// </remarks>
    public static partial class Youtube
    {
        /// <summary>
        /// Gets the HQ default thumbnail URL for the specified YouTube video.
        /// </summary>
        /// <param name="youtubeVideoId">The YouTube video ID to get the thumbnail for.</param>
        /// <returns>The thumbnail URL string.</returns>
        /// <exception cref="BadYoutubeIdException">
        /// Thrown when <paramref name="youtubeVideoId"/> is not a valid YouTube video ID.
        /// </exception>
        public static string GetYoutubeThumbnail(string youtubeVideoId) =>
            GetYoutubeThumbnail(youtubeVideoId, YoutubeThumbnail.HqDefault);

        /// <summary>
        /// Gets the thumbnail URL for the specified YouTube video at the specified quality.
        /// </summary>
        /// <param name="youtubeVideoId">The YouTube video ID.</param>
        /// <param name="thumbnail">The desired thumbnail quality.</param>
        /// <returns>The thumbnail URL string.</returns>
        /// <exception cref="BadYoutubeIdException">
        /// Thrown when <paramref name="youtubeVideoId"/> is not a valid YouTube video ID.
        /// </exception>
        public static string GetYoutubeThumbnail(string youtubeVideoId, YoutubeThumbnail thumbnail)
        {
            if (!IsValidYoutubeId(youtubeVideoId))
                throw new BadYoutubeIdException(youtubeVideoId);

            return Url.BuildAbsoluteUrl(new Uri($"https://i.ytimg.com/vi/{youtubeVideoId}/{thumbnail}.jpg"));
        }

        /// <summary>
        /// Gets the iframe embed URL for the specified YouTube video.
        /// </summary>
        /// <param name="youtubeVideoId">The YouTube video ID.</param>
        /// <returns>The embed URL string.</returns>
        /// <exception cref="BadYoutubeIdException">
        /// Thrown when <paramref name="youtubeVideoId"/> is not a valid YouTube video ID.
        /// </exception>
        public static string GetYoutubeIframeUrl(string youtubeVideoId)
        {
            if (!IsValidYoutubeId(youtubeVideoId)) throw new BadYoutubeIdException(youtubeVideoId);
            return Url.BuildAbsoluteUrl(new Uri($"https://www.youtube.com/embed/{youtubeVideoId}"));
        }

        /// <summary>
        /// Determines whether a string is a valid YouTube video ID.
        /// </summary>
        /// <remarks>
        /// Validation checks in order:
        /// <list type="number">
        ///   <item>Not null or empty.</item>
        ///   <item>Exactly 11 characters long.</item>
        ///   <item>Matches the pattern <c>^[a-zA-Z0-9_-]{11}$</c>.</item>
        /// </list>
        /// </remarks>
        /// <param name="youtubeVideoId">The YouTube video ID to validate.</param>
        /// <returns>True if the ID is valid; otherwise, false.</returns>
        public static bool IsValidYoutubeId(string? youtubeVideoId) =>
            !string.IsNullOrEmpty(youtubeVideoId)
            && youtubeVideoId.Length == 11
            && YoutubeIdRegex().IsMatch(youtubeVideoId);

        // Anchored with ^ and $ for correctness-by-construction.
        // The Length == 11 check above provides a redundant guard, but anchoring
        // ensures the regex is self-contained and safe if ever used independently.
        [GeneratedRegex("^[a-zA-Z0-9_-]{11}$")]
        private static partial Regex YoutubeIdRegex();
    }
}