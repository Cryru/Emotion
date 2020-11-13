#region Using

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Khronos;

#endregion

// ReSharper disable CheckNamespace
namespace OpenGL
{
    /// <summary>
    /// Version abstraction for OpenGL Shading Language APIs.
    /// </summary>
    [DebuggerDisplay("GlslVersion: Version={Major}.{Minor}.{Revision} (API='{Api}')")]
    public class GlslVersion : KhronosVersion
    {
        #region Constructors

        /// <summary>
        /// Construct a KhronosVersion specifying the version numbers.
        /// </summary>
        /// <param name="major">
        /// A <see cref="int" /> that specifies that major version number.
        /// </param>
        /// <param name="minor">
        /// A <see cref="int" /> that specifies that minor version number.
        /// </param>
        /// <param name="api">
        /// A <see cref="string" /> that specifies the API name.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Exception thrown if <paramref name="major" /> is less or equals to 0, or if <paramref name="minor" /> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Exception thrown if <paramref name="api" /> is null.
        /// </exception>
        public GlslVersion(int major, int minor, string api) :
            base(major, minor, 0, api)
        {
        }

        #endregion

        #region String Parsing

        /// <summary>
        /// Parse a GlslVersion from a string.
        /// </summary>
        /// <param name="input">
        /// A <see cref="String" /> that specifies the API version.
        /// </param>
        /// <returns>
        /// It returns a <see cref="KhronosVersion" /> based on the pattern recognized in <paramref name="input" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Exception thrown if <paramref name="input" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Exception thrown if no pattern is recognized in <paramref name="input" />.
        /// </exception>
        public new static GlslVersion Parse(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // Determine version value (support up to 3 version numbers)
            Match versionMatch = Regex.Match(input, @"(?<Major>\d+)\.(?<Minor>\d+)(\.(?<Rev>\d+))?( .*)?");
            if (versionMatch.Success == false)
                throw new ArgumentException("unrecognized pattern", nameof(input));

            int versionMajor = int.Parse(versionMatch.Groups["Major"].Value);
            int versionMinor = int.Parse(versionMatch.Groups["Minor"].Value);

            string api = API_GLSL;

            if (Regex.IsMatch(input, @"\sES\s?"))
                api = API_ESSL;

            return new GlslVersion(versionMajor, versionMinor >= 10 ? versionMinor / 10 : versionMinor, api);
        }

        /// <summary>
        /// Parse a GlslVersion from a string.
        /// </summary>
        /// <param name="input">
        /// A <see cref="String" /> that specifies the API version.
        /// </param>
        /// <param name="api">
        /// </param>
        /// <returns>
        /// It returns a <see cref="KhronosVersion" /> based on the pattern recognized in <paramref name="input" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Exception thrown if <paramref name="input" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Exception thrown if no pattern is recognized in <paramref name="input" />.
        /// </exception>
        public static GlslVersion Parse(string input, string api)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            GlslVersion glslVersion = Parse(input);

            switch (api)
            {
                case null:
                    break;
                case API_GL:
                case API_GLSL:
                    glslVersion = new GlslVersion(glslVersion.Major, glslVersion.Minor, API_GLSL);
                    break;
                case API_GLES2:
                case API_ESSL:
                    glslVersion = new GlslVersion(glslVersion.Major, glslVersion.Minor, API_ESSL);
                    break;
                default:
                    throw new NotSupportedException($"api \'{api}\' not supported");
            }

            return glslVersion;
        }

        #endregion

        #region KhronosVersion Overrides

        /// <summary>
        /// Get the version identifier of this KhronosVersion.
        /// </summary>
        public override int VersionId
        {
            get => Major * 100 + (Minor >= 10 ? Minor : Minor * 10);
        }

        #endregion
    }
}