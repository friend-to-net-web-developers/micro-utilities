using System.Diagnostics;
#pragma warning disable CS0162 // Unreachable code detected

namespace FriendToNetWebDevelopers.MicroUtilities;

public static partial class Utilities
{
    /// <summary>
    /// Checks at both compile-time and runtime if debugging is being used
    /// </summary>
    /// <returns></returns>
    internal static bool IsDebug()
    {
        //Compile-time check (this is faster)
#if DEBUG
        return true;
#endif
        //Run-time check
        return Debugger.IsAttached;
    }
}