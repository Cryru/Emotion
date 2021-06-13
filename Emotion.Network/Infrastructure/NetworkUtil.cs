#region Using

using System;
using System.Threading;

#endregion

namespace Emotion.Network.Infrastructure
{
    public static class NetworkUtil
    {
        private static int _idIncr;

        public static string GenerateId(string usage)
        {
            int myId = Interlocked.Increment(ref _idIncr);
            return usage[0] + Convert.ToHexString(BitConverter.GetBytes(myId));
        }
    }
}