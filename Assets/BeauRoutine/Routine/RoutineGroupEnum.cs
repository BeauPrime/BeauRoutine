/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    RoutineGroupEnum.cs
 * Purpose: Attribute that marks an enum as the inspector-viewable
 *          list of routine groups, along with the inspector
 *          for fields explicitly marked as a reference to a group.
*/

using UnityEngine;
using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BeauRoutine
{
    /// <summary>
    /// Reference to a routine group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RoutineGroupRefAttribute : PropertyAttribute { }

    /// <summary>
    /// Identifies an enum as the type to use when specifying routine groups.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
    public sealed class RoutineGroupEnum : Attribute
    {
#if UNITY_EDITOR

        static private Type s_Type;
        static private bool s_Initialized = false;

        static private void Initialize()
        {
            if (s_Initialized)
                return;
            s_Initialized = true;

            ProcessAssembly(Assembly.GetAssembly(typeof(RoutineGroupEnum)));
        }

        static private void ProcessAssembly(Assembly inAssembly)
        {
            var types = inAssembly.GetTypes();
            for (int i = 0; i < types.Length; ++i)
            {
                if (ProcessType(types[i]))
                    return;
            }
        }

        static private bool ProcessType(Type inType)
        {
            if (!inType.IsEnum)
                return false;

            RoutineGroupEnum[] groups = (RoutineGroupEnum[])inType.GetCustomAttributes(typeof(RoutineGroupEnum), false);
            if (groups.Length > 0)
            {
                s_Type = inType;
                return true;
            }

            return false;
        }

        #region Editor

        [CustomPropertyDrawer(typeof(RoutineGroupRefAttribute), true)]
        private class Editor : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                Initialize();

                Rect indentedPos = EditorGUI.IndentedRect(position);
                float toShift = EditorGUIUtility.labelWidth - 2 * (indentedPos.x - position.x);

                label = EditorGUI.BeginProperty(indentedPos, label, property);
                {
                    GUI.Label(indentedPos, label);

                    Rect popupRect = indentedPos;
                    popupRect.x += toShift;
                    popupRect.width -= toShift;

                    Type type = RoutineGroupEnum.s_Type;
                    if (type == null)
                    {
                        property.intValue = EditorGUI.IntField(popupRect, property.intValue);
                    }
                    else
                    {
                        Enum currentEnum = (Enum)Enum.ToObject(type, property.intValue);
                        Enum newValue = EditorGUI.EnumPopup(popupRect, currentEnum);
                        property.intValue = (int)Convert.ChangeType(newValue, typeof(int));
                    }
                }
                EditorGUI.EndProperty();
            }
        }

        #endregion

#endif // UNITY_EDITOR
    }
}
