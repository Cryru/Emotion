﻿#region Using

using System;
using System.Runtime.CompilerServices;
using System.Text;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XmlEnumTypeHandler : XMLTypeHandler
    {
        public override bool CanBeInherited { get => false; }
        protected object _defaultValue;
        protected bool _opaque;

        public XmlEnumTypeHandler(Type type) : base(type)
        {
            Type = XmlHelpers.GetOpaqueType(type, out _opaque);
            _defaultValue = _opaque ? Activator.CreateInstance(Type, true) : null;
        }

        public override void Serialize(object obj, StringBuilder output, int indentation, XmlRecursionChecker recursionChecker)
        {
            output.Append($"{obj}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool ShouldSerialize(object obj)
        {
            return obj != null && !obj.Equals(_defaultValue);
        }

        public override object Deserialize(XmlReader input)
        {
            string readValue = input.GoToNextTag();
            return Enum.Parse(Type, readValue);
        }
    }
}