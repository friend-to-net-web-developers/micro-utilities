using System.Text.RegularExpressions;
using FriendToNetWebDevelopers.MicroUtilities.Enum;
using FriendToNetWebDevelopers.MicroUtilities.Exception;

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    /// <summary>
    /// Used to handle miscellaneous youtube-related url tasks.<br/>
    /// Makes heavy use of Uri building and validation.  Should make life more difficult for a hostile actor. 
    /// </summary>
    public static partial class Youtube
    {
        /// <summary>
        /// Gets the default size thumbnail
        /// </summary>
        /// <param name="youtubeVideoId">youtube id</param>
        /// <returns></returns>
        public static string GetYoutubeThumbnail(string youtubeVideoId) =>
            GetYoutubeThumbnail(youtubeVideoId, YoutubeThumbnailEnum.HqDefault);

        /// <summary>
        /// Gets the youtube thumbnail url based on the provided youtubeVideoId
        /// </summary>
        /// <param name="youtubeVideoId"></param>
        /// <param name="thumbnail"></param>
        /// <returns></returns>
        public static string GetYoutubeThumbnail(string youtubeVideoId, YoutubeThumbnailEnum thumbnail)
        {
            if (!IsValidYoutubeId(youtubeVideoId))
                throw new BadYoutubeIdException(youtubeVideoId);
            
            return Url.BuildAbsoluteUrl(new Uri($"https://i.ytimg.com/vi/{youtubeVideoId}/{thumbnail}.jpg"));
        }

        /// <summary>
        /// Gets the youtube iframe source url based on the provided youtubeVideoId
        /// </summary>
        /// <param name="youtubeVideoId"></param>
        /// <returns></returns>
        public static string GetYoutubeIframeUrl(string youtubeVideoId)
        {
            if (!IsValidYoutubeId(youtubeVideoId)) throw new BadYoutubeIdException(youtubeVideoId);
            return Url.BuildAbsoluteUrl(new Uri($"https://www.youtube.com/embed/{youtubeVideoId}"));
        }

        

        /// <summary>
        /// Checks if a youtube id matches standard youtube forms meaning:
        /// <ul>
        /// <li><em>CHECK 1</em> - It is not null or empty <b>AND</b></li>
        /// <li><em>CHECK 2</em> - It is exactly 11 characters long <b>AND</b></li>
        /// <li><em>CHECK 3</em> - It matches the pattern "[a-zA-Z0-9_-]{11}"</li>
        /// </ul>
        /// In that order
        /// </summary>
        /// <param name="youtubeVideoId">The Youtube Id to match against</param>
        /// <returns><ul>
        /// <li><b>true</b> - If it passes all checks in order</li>
        /// <li><b>false</b> - In all other cases</li>
        /// </ul>
        /// </returns>
        public static bool IsValidYoutubeId(string? youtubeVideoId) =>
            !string.IsNullOrEmpty(youtubeVideoId) //Not null                  AND
            && youtubeVideoId.Length == 11 //Is 11 characters long     AND
            && YoutubeRegex().IsMatch(youtubeVideoId); //Matches the pattern
        
        /// <summary>
        /// The youtube video id matching regex pattern
        /// </summary>
        private const string YoutubeIdMatchPattern = "[a-zA-Z0-9_-]{11}";
        [GeneratedRegex(YoutubeIdMatchPattern)]
        private static partial Regex YoutubeRegex();
    }
}