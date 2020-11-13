#region Using

using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

#endregion

// ReSharper disable CheckNamespace
namespace Khronos
{
    /// <summary>
    /// Version abstraction for Khrono APIs.
    /// </summary>
    [DebuggerDisplay("KhronosVersion: Version={Major}.{Minor}.{Revision} API='{Api}' Profile={Profile}")]
    public class KhronosVersion : IEquatable<KhronosVersion>, IComparable<KhronosVersion>
    {
        #region Constructors

        /// <summary>
        /// Construct a KhronosVersion specifying the version numbers.
        /// </summary>
        /// <param name="major">
        /// A <see cref="Int32" /> that specifies that major version number.
        /// </param>
        /// <param name="minor">
        /// A <see cref="Int32" /> that specifies that minor version number.
        /// </param>
        /// <param name="api">
        /// A <see cref="String" /> that specifies the API name.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Exception thrown if <paramref name="major" /> is less or equals to 0, or if <paramref name="minor" /> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Exception thrown if <paramref name="api" /> is null.
        /// </exception>
        public KhronosVersion(int major, int minor, string api) :
            this(major, minor, 0, api)
        {
        }

        /// <summary>
        /// Construct a KhronosVersion specifying the version numbers.
        /// </summary>
        /// <param name="major">
        /// A <see cref="Int32" /> that specifies that major version number.
        /// </param>
        /// <param name="minor">
        /// A <see cref="Int32" /> that specifies that minor version number.
        /// </param>
        /// <param name="revision">
        /// A <see cref="Int32" /> that specifies that revision version number.
        /// </param>
        /// <param name="api">
        /// A <see cref="String" /> that specifies the API name.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Exception thrown if <paramref name="major" /> is less or equals to 0, or if <paramref name="minor" /> or
        /// <paramref name="revision" /> are less than 0.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Exception thrown if <paramref name="api" /> is null.
        /// </exception>
        public KhronosVersion(int major, int minor, int revision, string api) :
            this(major, minor, revision, api, null)
        {
        }

        /// <summary>
        /// Construct a KhronosVersion specifying the version numbers.
        /// </summary>
        /// <param name="major">
        /// A <see cref="Int32" /> that specifies that major version number.
        /// </param>
        /// <param name="minor">
        /// A <see cref="Int32" /> that specifies that minor version number.
        /// </param>
        /// <param name="revision">
        /// A <see cref="Int32" /> that specifies that revision version number.
        /// </param>
        /// <param name="api">
        /// A <see cref="String" /> that specifies the API name.
        /// </param>
        /// <param name="profile">
        /// A <see cref="String" /> that specifies the API profile.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Exception thrown if <paramref name="major" /> is less or equals to 0, or if <paramref name="minor" /> or
        /// <paramref name="revision" /> are less than 0.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Exception thrown if <paramref name="api" /> is null.
        /// </exception>
        public KhronosVersion(int major, int minor, int revision, string api, string profile)
        {
            if (major < 0)
                throw new ArgumentException("less than 0 not allowed", nameof(major));
            if (minor < 0)
                throw new ArgumentException("less than 0 not allowed", nameof(minor));
            if (revision < 0)
                throw new ArgumentException("less than 0 not allowed", nameof(revision));
            if (api == null)
                throw new ArgumentNullException(nameof(api));

            Major = major;
            Minor = minor;
            Revision = revision;
            Api = api;
            Profile = profile;
            GLES = Api == API_GLES1 || Api == API_GLES2 || Api == API_ESSL;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="other">
        /// The other <see cref="KhronosVersion" /> to be copied.
        /// </param>
        public KhronosVersion(KhronosVersion other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            Major = other.Major;
            Minor = other.Minor;
            Revision = other.Revision;
            Api = other.Api;
            Profile = other.Profile;
            GLES = other.GLES;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="other">
        /// The other <see cref="KhronosVersion" /> to be copied.
        /// </param>
        /// <param name="profile">
        /// A <see cref="String" /> that specifies the API profile. It can be null to indicate the default profile.
        /// </param>
        public KhronosVersion(KhronosVersion other, string profile) :
            this(other)
        {
            Profile = profile;
        }

        #endregion

        #region API Description

        /// <summary>
        /// OpenGL API.
        /// </summary>
        public const string API_GL = "gl";

        /// <summary>
        /// OpenGL for Windows API.
        /// </summary>
        public const string API_WGL = "wgl";

        /// <summary>
        /// OpenGL on EGL API.
        /// </summary>
        public const string API_EGL = "egl";

        /// <summary>
        /// OpenGL ES 1.x API.
        /// </summary>
        public const string API_GLES1 = "gles1";

        /// <summary>
        /// OpenGL ES 2.x+ API.
        /// </summary>
        public const string API_GLES2 = "gles2";

        /// <summary>
        /// OpenGL GLSL.
        /// </summary>
        public const string API_GLSL = "glsl";

        /// <summary>
        /// OpenGL ESSL.
        /// </summary>
        public const string API_ESSL = "essl";

        /// <summary>
        /// The Khronos API description.
        /// </summary>
        public readonly string Api;

        /// <summary>
        /// Whether the API is a version of OpenGL ES
        /// </summary>
        public readonly bool GLES;

        #endregion

        #region Version Numbers

        /// <summary>
        /// Major version number.
        /// </summary>
        public readonly int Major;

        /// <summary>
        /// Minor version number.
        /// </summary>
        public readonly int Minor;

        /// <summary>
        /// Revision version number.
        /// </summary>
        public readonly int Revision;

        #endregion

        #region Version Identifier

        /// <summary>
        /// Get the version identifier of this KhronosVersion.
        /// </summary>
        public virtual int VersionId
        {
            get => Major * 100 + Minor * 10;
        }

        #endregion

        #region Profile

        /// <summary>
        /// Specific to GL API: Core profile.
        /// </summary>
        public const string PROFILE_CORE = "core";

        /// <summary>
        /// Specific to GL API: Compatibility profile.
        /// </summary>
        public const string PROFILE_COMPATIBILITY = "compatibility";

        /// <summary>
        /// Specific to GLES1 API: Common profile.
        /// </summary>
        public const string PROFILE_COMMON = "common";

        /// <summary>
        /// Specific to WebGL.
        /// </summary>
        public const string PROFILE_WEBGL = "webgl";

        /// <summary>
        /// API profile. In the case of null profile, the meaning is determined by the specific method.
        /// </summary>
        public readonly string Profile;

        #endregion

        #region Operators

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="left">
        /// A <see cref="KhronosVersion" /> to compare with <paramref name="right" />.
        /// </param>
        /// <param name="right">
        /// A <see cref="KhronosVersion" /> to compare with <paramref name="left" />.
        /// </param>
        /// <returns>
        /// It returns a boolean value indicating whether <paramref name="left" /> equals <paramref name="right" />.
        /// </returns>
        public static bool operator ==(KhronosVersion left, KhronosVersion right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null))
                return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="left">
        /// A <see cref="KhronosVersion" /> to compare with <paramref name="right" />.
        /// </param>
        /// <param name="right">
        /// A <see cref="KhronosVersion" /> to compare with <paramref name="left" />.
        /// </param>
        /// <returns>
        /// It returns a boolean value indicating whether <paramref name="left" /> doesn't equals <paramref name="right" />.
        /// </returns>
        public static bool operator !=(KhronosVersion left, KhronosVersion right)
        {
            if (ReferenceEquals(left, right))
                return false;
            if (ReferenceEquals(left, null))
                return false;

            return !left.Equals(right);
        }

        /// <summary>
        /// Greater than operator.
        /// </summary>
        /// <param name="left">
        /// A <see cref="KhronosVersion" /> to compare with <paramref name="right" />.
        /// </param>
        /// <param name="right">
        /// A <see cref="KhronosVersion" /> to compare with <paramref name="left" />.
        /// </param>
        /// <returns>
        /// It returns a boolean value indicating whether <paramref name="left" /> is greater than <paramref name="right" />.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The API names of this KhronosVersion and <paramref name="right" /> does not match.
        /// </exception>
        public static bool operator >(KhronosVersion left, KhronosVersion right)
        {
            if (ReferenceEquals(left, right))
                return false;
            if (ReferenceEquals(left, null))
                return false;
            if (ReferenceEquals(right, null))
                return false;

            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Lower than operator.
        /// </summary>
        /// <param name="left">
        /// A <see cref="KhronosVersion" /> to compare with <paramref name="right" />.
        /// </param>
        /// <param name="right">
        /// A <see cref="KhronosVersion" /> to compare with <paramref name="left" />.
        /// </param>
        /// <returns>
        /// It returns a boolean value indicating whether <paramref name="left" /> is lower than <paramref name="right" />.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The API names of this KhronosVersion and <paramref name="right" /> does not match.
        /// </exception>
        public static bool operator <(KhronosVersion left, KhronosVersion right)
        {
            if (ReferenceEquals(left, right))
                return false;
            if (ReferenceEquals(left, null))
                return false;
            if (ReferenceEquals(right, null))
                return false;

            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Greater than or equal to operator.
        /// </summary>
        /// <param name="left">
        /// A <see cref="KhronosVersion" /> to compare with <paramref name="right" />.
        /// </param>
        /// <param name="right">
        /// A <see cref="KhronosVersion" /> to compare with <paramref name="left" />.
        /// </param>
        /// <returns>
        /// It returns a boolean value indicating whether <paramref name="left" /> is greater than or equal to
        /// <paramref name="right" />.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The API names of this KhronosVersion and <paramref name="right" /> does not match.
        /// </exception>
        public static bool operator >=(KhronosVersion left, KhronosVersion right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null))
                return false;
            if (ReferenceEquals(right, null))
                return false;

            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Lower than or equal to operator.
        /// </summary>
        /// <param name="left">
        /// A <see cref="KhronosVersion" /> to compare with <paramref name="right" />.
        /// </param>
        /// <param name="right">
        /// A <see cref="KhronosVersion" /> to compare with <paramref name="left" />.
        /// </param>
        /// <returns>
        /// It returns a boolean value indicating whether <paramref name="left" /> is lower than or equal to
        /// <paramref name="right" />.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The API names of this KhronosVersion and <paramref name="right" /> does not match.
        /// </exception>
        public static bool operator <=(KhronosVersion left, KhronosVersion right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null))
                return false;
            if (ReferenceEquals(right, null))
                return false;

            return left.CompareTo(right) <= 0;
        }

        #endregion

        #region String Parsing

        /// <summary>
        /// Internal method for parsing GL specification features.
        /// </summary>
        /// <param name="featureName">
        /// A <see cref="string" /> that specify the feature name (i.e. GL_VERSION_1_0).
        /// </param>
        /// <returns>
        /// It returns the <see cref="KhronosVersion" /> corresponding to <paramref name="featureName" />.
        /// </returns>
        internal static KhronosVersion ParseFeature(string featureName)
        {
            if (featureName == null)
                throw new ArgumentNullException(nameof(featureName));

            // Shortcut for ES1
            if (featureName == "GL_VERSION_ES_CM_1_0")
                return new KhronosVersion(1, 0, 0, API_GLES1);

            // Match GL|GLES|GLSC|WGL|GLX|EGL versions
            Match versionMatch = Regex.Match(featureName, @"(?<Api>GL(_(ES|SC|))?|WGL|GLX|EGL)_VERSION_(?<Major>\d+)_(?<Minor>\d+)");
            if (versionMatch.Success == false)
                return null;

            string api = versionMatch.Groups["Api"].Value;
            int major = int.Parse(versionMatch.Groups["Major"].Value);
            int minor = int.Parse(versionMatch.Groups["Minor"].Value);

            switch (api)
            {
                case "GL":
                    api = API_GL;
                    break;
                case "GL_ES":
                    api = API_GLES2;
                    break;
                case "WGL":
                    api = API_WGL;
                    break;
                case "EGL":
                    api = API_EGL;
                    break;
            }

            return new KhronosVersion(major, minor, api);
        }

        /// <summary>
        /// Parse a KhronosVersion from a string.
        /// </summary>
        public static KhronosVersion Parse(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // Determine version value (support up to 3 version numbers)
            Match versionMatch = Regex.Match(input, @"(?<Major>\d+)\.(?<Minor>\d+)(\.(?<Rev>\d+))?");
            if (versionMatch.Success == false)
                throw new ArgumentException($"unrecognized pattern '{input}'", nameof(input));

            string api;
            int versionMajor = int.Parse(versionMatch.Groups["Major"].Value);
            int versionMinor = int.Parse(versionMatch.Groups["Minor"].Value);
            int versionRev = versionMatch.Groups["Rev"].Success ? int.Parse(versionMatch.Groups["Rev"].Value) : 0;

            if (versionMinor >= 10 && versionMinor % 10 == 0)
                versionMinor /= 10;

            if (Regex.IsMatch(input, "ES"))
                switch (versionMajor)
                {
                    case 1:
                        api = API_GLES1;
                        break;
                    default:
                        api = API_GLES2;
                        break;
                }
            else
                api = API_GL;

            // Regex will catch 2.0 as major version. WebGL 2.0 is always 3.0
            // Example: WebGL 2.0 (OpenGL ES 3.0 Chromium)
            if (Regex.IsMatch(input, "WebGL 2")) versionMajor = 3;

            return new KhronosVersion(versionMajor, versionMinor, versionRev, api);
        }

        #endregion

        #region Compatibility

        /// <summary>
        /// Check whether this version is compatible with another instance.
        /// </summary>
        /// <param name="other">
        /// A <see cref="KhronosVersion" /> to check for compatibility.
        /// </param>
        /// <returns>
        /// It returns a boolean value indicating whether this KhronosVersion is compatible with <paramref name="other" />.
        /// </returns>
        public bool IsCompatible(KhronosVersion other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            // Different API are incompatible
            if (Api != other.Api)
                return false;

            if (Major < other.Major)
                return false;
            if (Minor < other.Minor)
                return false;
            if (Revision < other.Revision)
                return false;

            return true;
        }

        #endregion

        #region Object Overrides

        /// <summary>
        /// Converts this KhronosVersion into a human-legible string representation.
        /// </summary>
        /// <returns>
        /// The string representation of this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Version={0}.{1}", Major, Minor);
            if (Revision != 0)
                sb.AppendFormat(".{0}", Revision);
            if (string.IsNullOrEmpty(Api) == false)
                sb.AppendFormat(" API={0}", Api);

            if (Profile != null)
                sb.AppendFormat(" Profile={0}", Profile);

            return sb.ToString();
        }

        #endregion

        #region IEquatable<KhronosVersion> Implementation

        /// <summary>
        /// Returns a boolean value indicating whether this instance is equal to <paramref name="other" />.
        /// </summary>
        /// <param name="other">
        /// The KhronosVersion to be compared with this KhronosVersion.
        /// </param>
        /// <returns>
        /// It returns a boolean value indicating whether <paramref name="other" /> equals to this instance.
        /// </returns>
        public bool Equals(KhronosVersion other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            if (Api != other.Api)
                return false;
            if (Major != other.Major)
                return false;
            if (Minor != other.Minor)
                return false;
            if (Revision != other.Revision)
                return false;

            // Note: any null profile match any other profile, and viceversa
            if (Profile != null && other.Profile != null && Profile != other.Profile)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.
        /// </param>
        /// <returns>
        /// It returns true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />;
        /// otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != typeof(KhronosVersion) && obj.GetType().GetTypeInfo().IsSubclassOf(typeof(KhronosVersion)) == false)
                return false;

            return Equals((KhronosVersion) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode" /> is suitable for
        /// use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;

                result = (result * 397) ^ Api.GetHashCode();
                result = (result * 397) ^ Major.GetHashCode();
                result = (result * 397) ^ Minor.GetHashCode();
                result = (result * 397) ^ Revision.GetHashCode();
                if (Profile != null)
                    result = (result * 397) ^ Profile.GetHashCode();

                return result;
            }
        }

        #endregion

        #region IComparable<KhronosVersion> Implementation

        /// <summary>
        /// Compares this instance to a specified KhronosVersion and returns an integer
        /// that indicates whether the value of this instance is less than, equal to, or
        /// greater than the value of the specified KhronosVersion.
        /// </summary>
        /// <param name="other">
        /// A <see cref="KhronosVersion" /> to compare.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The API names of this KhronosVersion and <paramref name="other" /> does not match.
        /// </exception>
        public int CompareTo(KhronosVersion other)
        {
            if (ReferenceEquals(this, other))
                return 0;
            if (ReferenceEquals(null, other))
                return +1;

            if (Api != other.Api)
                throw new InvalidOperationException("different API version are not comparable");

            int majorCompareTo = Major.CompareTo(other.Major);
            if (majorCompareTo != 0)
                return majorCompareTo;

            int minorCompareTo = Minor.CompareTo(other.Minor);
            if (minorCompareTo != 0)
                return minorCompareTo;

            int revCompareTo = Revision.CompareTo(other.Revision);
            if (revCompareTo != 0)
                return revCompareTo;

            return 0;
        }

        #endregion
    }
}