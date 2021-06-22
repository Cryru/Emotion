#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Emotion.Common.Serialization
{
    /// <summary>
    /// Exclude members from serialization of the class applied to, going down the inheritance.
    /// If applied to a field it will exclude the members from that field's instance.
    /// If the fields exist in a deserialization document, they will be set.
    /// </summary>
    public class DontSerializeMembers : Attribute
    {
        public HashSet<string> Members;

        public DontSerializeMembers(params string[] members)
        {
            Members = new HashSet<string>();
            for (var i = 0; i < members.Length; i++)
            {
                Members.Add(members[i]);
            }
        }
    }
}