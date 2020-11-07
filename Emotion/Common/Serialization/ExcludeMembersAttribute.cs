#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Emotion.Common.Serialization
{
    /// <summary>
    /// Exclude members from serialization of this member object.
    /// If the fields exist in a deserialization document, they will be set.
    /// </summary>
    public class ExcludeMembersAttribute : Attribute
    {
        public HashSet<string> Members;

        public ExcludeMembersAttribute(params string[] members)
        {
            Members = new HashSet<string>();
            for (var i = 0; i < members.Length; i++)
            {
                Members.Add(members[i]);
            }
        }
    }
}