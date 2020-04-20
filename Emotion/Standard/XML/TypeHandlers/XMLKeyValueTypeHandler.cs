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
        private XmlFieldHandler _keyHandler;
        private XmlFieldHandler _valueHandler;

        public XmlKeyValueTypeHandler(Type type) : base(type)
        {
        }

        public override void Init()
        {
            PropertyInfo[] properties = Type.GetProperties();

            for (var i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                switch (property.Name)
                {
                    case "Key":
                    {
                        _keyHandler = XmlHelpers.ResolveFieldHandler(property.PropertyType, new XmlReflectionHandler(property));
                        break;
                    }
                    case "Value":
                    {
                        _valueHandler = XmlHelpers.ResolveFieldHandler(property.PropertyType, new XmlReflectionHandler(property));
                        break;
                    }
                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            RecursiveType = _keyHandler.TypeHandler.IsRecursiveWith(Type) || _valueHandler.TypeHandler.IsRecursiveWith(Type);
        }

        public override bool IsRecursiveWith(Type type)
        {
            return base.IsRecursiveWith(type) || _keyHandler.TypeHandler.IsRecursiveWith(type) || _valueHandler.TypeHandler.IsRecursiveWith(type);
        }

        public override void Serialize(object obj, StringBuilder output, int indentation, XmlRecursionChecker recursionChecker)
        {
            if (obj == null) return;

            Debug.Assert(Type.IsInstanceOfType(obj));
            _keyHandler.Serialize(GetKey(obj), output, indentation, recursionChecker);
            _valueHandler.Serialize(GetValue(obj), output, indentation, recursionChecker);
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
                        handler = _keyHandler;
                        break;
                    case "Value":
                        handler = _valueHandler;
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
            return _keyHandler.ReflectionInfo.GetValue(val);
        }

        public object GetValue(object val)
        {
            return _valueHandler.ReflectionInfo.GetValue(val);
        }
    }
}