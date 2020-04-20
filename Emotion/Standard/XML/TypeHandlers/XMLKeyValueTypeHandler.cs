#region Using

using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XmlKeyValueTypeHandler : XMLTypeHandler
    {
        public override bool CanBeInherited { get => false; }
        public override bool RecursiveType
        {
            get
            {
                if(_recursiveType != null)
                {
                    return _recursiveType.Value;
                }
                _recursiveType = _keyHandler.Value.TypeHandler.IsRecursiveWith(Type) || _valueHandler.Value.TypeHandler.IsRecursiveWith(Type);
                return _recursiveType.Value;
            }
            protected set => _recursiveType = value;
        }
        private bool? _recursiveType;

        private Lazy<XmlFieldHandler> _keyHandler;
        private Lazy<XmlFieldHandler> _valueHandler;

        public XmlKeyValueTypeHandler(Type type) : base(type)
        {
            PropertyInfo[] properties = Type.GetProperties();

            for (var i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                switch (property.Name)
                {
                    case "Key":
                    {
                        _keyHandler = new Lazy<XmlFieldHandler>(() => XmlHelpers.ResolveFieldHandler(property.PropertyType, new XmlReflectionHandler(property)));
                        break;
                    }
                    case "Value":
                    {
                        _valueHandler = new Lazy<XmlFieldHandler>(() => XmlHelpers.ResolveFieldHandler(property.PropertyType, new XmlReflectionHandler(property)));
                        break;
                    }
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        public override bool IsRecursiveWith(Type type)
        {
            return base.IsRecursiveWith(type) || _keyHandler.Value.TypeHandler.IsRecursiveWith(type) || _valueHandler.Value.TypeHandler.IsRecursiveWith(type);
        }

        public override void Serialize(object obj, StringBuilder output, int indentation, XmlRecursionChecker recursionChecker)
        {
            if (obj == null) return;

            Debug.Assert(Type.IsInstanceOfType(obj));
            output.Append("\n");
            _keyHandler.Value.Serialize(GetKey(obj), output, indentation, recursionChecker);
            _valueHandler.Value.Serialize(GetValue(obj), output, indentation, recursionChecker);
            output.AppendJoin(XmlFormat.IndentChar, new string[indentation]);
        }

        public override object Deserialize(XmlReader input)
        {
            int depth = input.Depth;
            object key = null;
            object value = null;

            input.GoToNextTag();
            while (input.Depth >= depth && !input.Finished)
            {
                string currentTag = input.ReadTag(out string typeAttribute);
                XmlFieldHandler handler;
                switch (currentTag)
                {
                    case "Key":
                        handler = _keyHandler.Value;
                        break;
                    case "Value":
                        handler = _valueHandler.Value;
                        break;
                    default:
                        continue;
                }

                // Derived type.
                if (typeAttribute != null)
                {
                    Type derivedType = XmlHelpers.GetTypeByName(typeAttribute);
                    if (derivedType == null)
                        Engine.Log.Warning($"Couldn't find derived type of name {typeAttribute}.", MessageSource.XML);
                    else
                        handler = XmlHelpers.ResolveFieldHandler(derivedType, handler.ReflectionInfo);
                }

                switch (currentTag)
                {
                    case "Key":
                        key = handler.Deserialize(input);
                        break;
                    case "Value":
                        value = handler.Deserialize(input);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }

                input.GoToNextTag();
            }

            object newObj = Activator.CreateInstance(Type, key, value);
            return newObj;
        }

        public object GetKey(object val)
        {
            return _keyHandler.Value.ReflectionInfo.GetValue(val);
        }

        public object GetValue(object val)
        {
            return _valueHandler.Value.ReflectionInfo.GetValue(val);
        }
    }
}