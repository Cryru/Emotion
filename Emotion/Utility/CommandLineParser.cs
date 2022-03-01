#region Using

using System;
using System.Collections.Generic;

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
        /// <param name="offset">Number of occurrences to skip. Allows for multiple of the same arg.</param>
        /// <returns>Whether the argument was found,</returns>
        public static bool FindArgument(IEnumerable<string> args, string identifier, out string arg, int offset = 0)
        {
            identifier = identifier.ToLower();

            arg = null;
            args ??= Array.Empty<string>();
            foreach (string a in args)
            {
                string curA = a.ToLower();
                if (curA.Contains(identifier))
                {
                    offset--;
                    if (offset < 0)
                    {
                        arg = curA.Replace(identifier, "");
                        break;
                    }
                }
            }

            return arg != null;
        }
    }
}