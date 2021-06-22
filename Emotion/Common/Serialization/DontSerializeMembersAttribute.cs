#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Emotion.Common.Serialization
{
    /// <summary>
    /// Exclude members from serialization of the class applied to, going down the inheritance.
    /// If applied to a field it will exclude the members from that field's instance.
    /// </summary>
    public class DontSerializeMembersAttribute : Attribute
    {
        public HashSet<string> Members;

        public DontSerializeMembersAttribute(params string[] members)
        {
            Members = new HashSet<string>();
            for (var i = 0; i < members.Length; i++)
            {
                Members.Add(members[i]);
            }
        }
    }
}