#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.Utility;
using ImGuiNET;

#endregion

#nullable enable

namespace Emotion.Tools.Editors
{
    public class EditorHelpers
    {
        public static object? TransformClass(object window, Type newType)
        {
            var oldTypeHandler = (XMLComplexBaseTypeHandler?) XMLHelpers.GetTypeHandler(window.GetType());
            var newTypeHandler = (XMLComplexBaseTypeHandler?) XMLHelpers.GetTypeHandler(newType);
            if (oldTypeHandler == null || newTypeHandler == null) return null;

            object? newTypeObject = Activator.CreateInstance(newType, true);
            Debug.Assert(newTypeObject != null);

            IEnumerator<XMLFieldHandler>? oldFields = oldTypeHandler.EnumFields();
            while (oldFields.MoveNext())
            {
                XMLFieldHandler oldField = oldFields.Current;

                XMLFieldHandler? newFieldsMatch = null;
                IEnumerator<XMLFieldHandler>? newFields = newTypeHandler.EnumFields();
                while (newFields.MoveNext())
                {
                    XMLFieldHandler newField = newFields.Current;
                    if (newField.Name != oldField.Name || newField.TypeHandler != oldField.TypeHandler) continue;
                    newFieldsMatch = newField;
                    break;
                }

                if (newFieldsMatch == null) continue;
                object value = oldField.ReflectionInfo.GetValue(window);
                newFieldsMatch.ReflectionInfo.SetValue(newTypeObject, value);
            }

            return newTypeObject;
        }

        public static bool ImGuiEditorForType<T>(T obj, XMLFieldHandler xmlHandler)
        {
            XMLTypeHandler? typeHandler = xmlHandler.TypeHandler;
            object value = xmlHandler.ReflectionInfo.GetValue(obj);
            switch (typeHandler)
            {
                case XMLStringTypeHandler:
                {
                    var stringValue = (string) (value ?? "");
                    if (ImGui.InputText(xmlHandler.Name, ref stringValue, 1000))
                    {
                        xmlHandler.ReflectionInfo.SetValue(obj, stringValue);
                        return true;
                    }

                    break;
                }
                case XMLEnumTypeHandler enumHandler:
                {
                    string[] enumValueNames = Enum.GetNames(enumHandler.Type);
                    int currentItem = enumValueNames.IndexOf(value.ToString());
                    if (ImGui.Combo(xmlHandler.Name, ref currentItem, enumValueNames, enumValueNames.Length))
                    {
                        value = Enum.Parse(enumHandler.Type, enumValueNames[currentItem]);
                        xmlHandler.ReflectionInfo.SetValue(obj, value);
                        return true;
                    }

                    break;
                }
                case XMLPrimitiveTypeHandler primitive:
                {
                    if (primitive.Type == typeof(int))
                    {
                        var intValue = (int) (value ?? 0);
                        if (ImGui.InputInt(xmlHandler.Name, ref intValue))
                        {
                            xmlHandler.ReflectionInfo.SetValue(obj, intValue);
                            return true;
                        }

                        break;
                    }

                    if (primitive.Type == typeof(bool))
                    {
                        var boolValue = (bool) (value ?? false);
                        if (ImGui.Checkbox(xmlHandler.Name, ref boolValue))
                        {
                            xmlHandler.ReflectionInfo.SetValue(obj, boolValue);
                            return true;
                        }

                        break;
                    }

                    if (primitive.Type == typeof(float))
                    {
                        var floatVal = (float) value;
                        if (ImGui.InputFloat(xmlHandler.Name, ref floatVal))
                        {
                            xmlHandler.ReflectionInfo.SetValue(obj, floatVal);
                            return true;
                        }
                    }

                    break;
                }
                case XMLComplexValueTypeHandler valueType:
                {
                    if (valueType.Type == typeof(Vector2))
                    {
                        var vec2Value = (Vector2) (value ?? Vector2.Zero);
                        if (ImGui.InputFloat2(xmlHandler.Name, ref vec2Value))
                        {
                            xmlHandler.ReflectionInfo.SetValue(obj, vec2Value);
                            return true;
                        }

                        break;
                    }

                    if (valueType.Type == typeof(Vector3))
                    {
                        var vec3Value = (Vector3) (value ?? Vector3.Zero);
                        if (ImGui.InputFloat3(xmlHandler.Name, ref vec3Value))
                        {
                            xmlHandler.ReflectionInfo.SetValue(obj, vec3Value);
                            return true;
                        }

                        break;
                    }

                    if (valueType.Type == typeof(Color))
                    {
                        var colorValue = (Color) (value ?? Color.White);
                        Vector4 colorAsVec4 = colorValue.ToVec4();
                        ImGui.Text(xmlHandler.Name);
                        ImGui.SameLine();
                        ImGui.Text($"(RGBA) {colorValue}");
                        ImGui.SameLine();

                        if (ImGui.ColorButton(xmlHandler.Name, colorAsVec4)) ImGui.OpenPopup(xmlHandler.Name);
                        if (ImGui.BeginPopup(xmlHandler.Name))
                        {
                            if (ImGui.ColorPicker4($"Edit: {xmlHandler.Name}", ref colorAsVec4))
                            {
                                colorValue = new Color(colorAsVec4);
                                xmlHandler.ReflectionInfo.SetValue(obj, colorValue);
                                return true;
                            }

                            ImGui.EndPopup();
                        }

                        break;
                    }

                    if (valueType.Type == typeof(Rectangle))
                    {
                        var rectValue = (Rectangle) (value ?? Rectangle.Empty);
                        var vec4Value = new Vector4(rectValue.X, rectValue.Y, rectValue.Width, rectValue.Height);
                        if (ImGui.InputFloat4(xmlHandler.Name, ref vec4Value))
                        {
                            var r = new Rectangle(vec4Value.X, vec4Value.Y, vec4Value.Z, vec4Value.W);
                            xmlHandler.ReflectionInfo.SetValue(obj, r);
                            return true;
                        }
                    }

                    break;
                }
                default:
                {
                    ImGui.Text($"{xmlHandler.Name}: {value}");
                    break;
                }
            }

            object defaultVal = xmlHandler.DefaultValue;
            bool valueIsNotDefault = value != null && !value.Equals(defaultVal);
            bool defaultIsNotValue = defaultVal != null && !defaultVal.Equals(value);
            if (valueIsNotDefault || defaultIsNotValue)
            {
                ImGui.PushID(xmlHandler.Name);
                ImGui.SameLine();
                if (ImGui.SmallButton("X"))
                {
                    xmlHandler.ReflectionInfo.SetValue(obj, defaultVal);
                    return true;
                }

                ImGui.PopID();
            }

            return false;
        }

        public static List<Type> GetTypesWhichInherit<T>()
        {
            List<Type> inheritors = new();
            Type type = typeof(T);
            foreach (Assembly assembly in Helpers.AssociatedAssemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type assemblyType in types)
                {
                    if (type.IsAssignableFrom(assemblyType)) inheritors.Add(assemblyType);
                }
            }

            return inheritors;
        }

        public static void RenderToolGrid(RenderComposer c, Vector3 position, Vector2 size, Color background, int gridSize)
        {
            c.RenderSprite(position, size, background);

            Vector2 posVec2 = position.ToVec2();
            for (var x = 0; x < size.X; x += gridSize)
            {
                c.RenderLine(posVec2 + new Vector2(x, 0), posVec2 + new Vector2(x, size.Y), Color.White * 0.2f);
            }

            for (var y = 0; y < size.Y; y += gridSize)
            {
                c.RenderLine(posVec2 + new Vector2(0, y), posVec2 + new Vector2(size.X, y), Color.White * 0.2f);
            }
        }

        public static void CenteredText(string text)
        {
            float windowWidth = ImGui.GetWindowSize().X;
            float textWidth   = ImGui.CalcTextSize(text).X;

            ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
            ImGui.Text(text);
        }
    }
}