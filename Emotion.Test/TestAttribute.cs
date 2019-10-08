#region Using

using System;

#endregion

namespace Emotion.Test
{
    public class TestAttribute : Attribute
    {
        public string Tag;
        public bool TagOnly;

        /// <summary>
        /// A test class or function.
        /// </summary>
        /// <param name="tag">A tag can be specified for the test to be filterable.</param>
        /// <param name="tagOnly">If true this test can only be ran if its tag is explicitly filtered by.</param>
        public TestAttribute(string tag = null, bool tagOnly = false)
        {
            Tag = tag;
            TagOnly = tagOnly;
        }
    }
}