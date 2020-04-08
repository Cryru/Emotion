﻿#region Using

using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Emotion.Common;
using Emotion.Platform.Input;

#endregion

namespace Emotion.Utility
{
    /// <summary>
    /// Various helper functions.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// The assemblies the engine considers associated. Will load embedded resources from, will load XML types from etc.
        /// </summary>
        public static Assembly[] AssociatedAssemblies;

        /// <summary>
        /// Regex for capturing Windows line endings.
        /// </summary>
        private static Regex _newlineRegex = new Regex("\r\n");

        /// <summary>
        /// Replaces windows new lines with unix new lines.
        /// </summary>
        public static string NormalizeNewLines(string source)
        {
            return _newlineRegex.Replace(source, "\n");
        }

        /// <summary>
        /// Converts a path to the platform equivalent on the currently running platform.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>A cross platform path.</returns>
        public static string CrossPlatformPath(string path)
        {
            return path.Replace('/', '$').Replace('\\', '$').Replace('$', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Converts the string to one which is safe for use in the file system.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>A string safe to use in the file system.</returns>
        public static string MakeStringPathSafe(string str)
        {
            return Path.GetInvalidPathChars().Aggregate(str, (current, c) => current.Replace(c, ' '));
        }

        /// <summary>
        /// Safely parses the text to an int. If the parse fails returns a default value.
        /// </summary>
        /// <param name="text">The text to parse to an int.</param>
        /// <param name="invalidValue">The value to return if the parsing fails. 0 by default.</param>
        /// <returns>The text parsed as an int, or a default value.</returns>
        public static int StringToInt(string text, int invalidValue = 0)
        {
            bool parsed = int.TryParse(text, out int result);
            return parsed ? result : invalidValue;
        }

        /// <summary>
        /// The generator to be used for generating randomness.
        /// </summary>
        private static Random _generator = new Random();

        /// <summary>
        /// Returns a randomly generated number within specified constraints.
        /// </summary>
        /// <param name="min">The lowest number that can be generated.</param>
        /// <param name="max">The highest number that can be generated.</param>
        /// <returns></returns>
        public static int GenerateRandomNumber(int min = 0, int max = 100)
        {
            //We add one because Random.Next does not include max.
            return _generator.Next(min, max + 1);
        }

        /// <summary>
        /// Add this to your update loop to control the camera with WASD.
        /// </summary>
        /// <param name="speed">The speed to move the camera at.</param>
        // ReSharper disable once InconsistentNaming
        public static void CameraWASDUpdate(float speed = 0.35f)
        {
            Vector2 dir = Vector2.Zero;
            if (Engine.InputManager.IsKeyHeld(Key.W)) dir.Y -= 1;
            if (Engine.InputManager.IsKeyHeld(Key.A)) dir.X -= 1;
            if (Engine.InputManager.IsKeyHeld(Key.S)) dir.Y += 1;
            if (Engine.InputManager.IsKeyHeld(Key.D)) dir.X += 1;

            dir *= new Vector2(speed, speed) * Engine.DeltaTime;
            Engine.Renderer.Camera.Position += new Vector3(dir, 0);
        }

        /// <summary>
        /// Exclusive or conditional. Returns true only if one of the two is true.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool XOR(bool a, bool b)
        {
            return (a || b) && a != b;
        }

        private static readonly byte[] Utf16Le = {0xFF, 0xFE};
        private static readonly byte[] Utf8Le = {0xEF, 0xBB, 0xBF};
        private static readonly byte[] Utf32Le = {0xFF, 0xFE, 0, 0};
        private static readonly byte[] Utf16Be = {0xFE, 0xFF};

        // <?xml search
        private static readonly byte[] Utf16LeAlt = {0x3C, 0, 0x3F, 0};
        private static readonly byte[] Utf8LeAlt = {0x3C, 0x3F, 0x78, 0x6D};
        private static readonly byte[] Utf32LeAlt = {0x3C, 0, 0, 0};
        private static readonly byte[] Utf16BeAlt = {0, 0x3C, 00, 0x3F};

        /// <summary>
        /// Guess the string encoding of the data array.
        /// https://stackoverflow.com/questions/581318/c-sharp-detect-xml-encoding-from-byte-array
        /// </summary>
        public static string GuessStringEncoding(byte[] data)
        {
            // "utf-16" - Unicode UTF-16, little endian byte order
            if (BytesEqual(data, Utf16LeAlt)) return Encoding.Unicode.GetString(data);
            if (XOR(data[2] == 0, data[3] == 0) && BytesEqual(data, Utf16Le))
            {
                return Encoding.Unicode.GetString(data);
            }

            // "utf-8" - Unicode (UTF-8)
            if (BytesEqual(data, Utf8Le) || BytesEqual(data, Utf8LeAlt))
            {
                return Encoding.UTF8.GetString(data);
            }

            // "utf-32" - Unicode UTF-32, little endian byte order
            if (BytesEqual(data, Utf32Le) || BytesEqual(data, Utf32LeAlt))
            {
                return Encoding.UTF32.GetString(data);
            }

            // "unicodeFFFE" - Unicode UTF-16, big endian byte order
            if (BytesEqual(data,Utf16BeAlt))
            {
                return Encoding.BigEndianUnicode.GetString(data);
            }
            if (data[2] != 0 && data[3] != 0 && BytesEqual(data, Utf16Be))
            {
                return Encoding.BigEndianUnicode.GetString(data);
            }

            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Compare two byte arrays. If the first n bytes of the "compare" array match, returns true.
        /// Where n is the length of the "compare" array.
        /// </summary>
        /// <param name="bytes">The bytes to compare against.</param>
        /// <param name="compare">The bytes to compare.</param>
        public static bool BytesEqual(byte[] bytes, byte[] compare)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < compare.Length; i++)
            {
                if (compare[i] != bytes[i]) return false;
            }
            return true;
        }
    }
}