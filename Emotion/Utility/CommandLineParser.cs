#region Using

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Emotion.Utility
{
    public static class CommandLineParser
    {
        /// <summary>
        /// Finds the argument which matches the specified identifier.
        /// </summary>
        /// <param name="args">The runtime arguments.</param>
        /// <param name="identifier">The identifier to match.</param>
        /// <param name="arg">The found argument.</param>
        /// <returns>Whether the argument was found,</returns>
        public static bool FindArgument(IEnumerable<string> args, string identifier, out string arg)
        {
            arg = args.FirstOrDefault(a => a.Contains(identifier));
            if (arg == null) return false;
            arg = arg.Replace(identifier, "");
            return true;
        }
    }
}