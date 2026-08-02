using System;
using System.Collections.Generic;
using Alien.Data;
using UnityEditor;
using UnityEngine;

namespace Alien.Editor
{
    [CustomPropertyDrawer(typeof(ConsumableEffectEntry))]
    public class ConsumableEffectEntryDrawer : PropertyDrawer
    {
        private const string EffectPropertyName = "effect";
        private const float VerticalSpacing = 2f;

        private static readonly List<Type> EffectTypes = FindEffectTypes();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty effectProperty = property.FindPropertyRelative(EffectPropertyName);
            bool isEditingMultipleObjects = property.serializedObject.isEditingMultipleObjects;
            EditorGUI.BeginProperty(position, label, property);

            Rect selectorRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            GUIContent selectorLabel = new(isEditingMultipleObjects ? "Multiple Effects" : GetEffectDisplayName(effectProperty));

            using (new EditorGUI.DisabledScope(isEditingMultipleObjects))
                if (EditorGUI.DropdownButton(selectorRect, selectorLabel, FocusType.Keyboard, EditorStyles.popup))
                    ShowEffectMenu(effectProperty, selectorRect);

            if (!isEditingMultipleObjects && effectProperty.managedReferenceValue != null)
            {
                Rect fieldsRect = new(position.x, selectorRect.yMax + VerticalSpacing, position.width, 0f);
                DrawEffectFields(fieldsRect, effectProperty);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects) return EditorGUIUtility.singleLineHeight;

            SerializedProperty effectProperty = property.FindPropertyRelative(EffectPropertyName);
            float fieldsHeight = GetEffectFieldsHeight(effectProperty);

            return EditorGUIUtility.singleLineHeight + (fieldsHeight > 0f ? VerticalSpacing + fieldsHeight : 0f);
        }

        private static void ShowEffectMenu(SerializedProperty effectProperty, Rect selectorRect)
        {
            GenericMenu menu = new();
            Type currentType = effectProperty.managedReferenceValue?.GetType();
            SerializedObject serializedObject = effectProperty.serializedObject;
            string propertyPath = effectProperty.propertyPath;

            menu.AddItem(new GUIContent("None"), currentType == null, () => AssignEffect(serializedObject, propertyPath, null));
            menu.AddSeparator(string.Empty);

            foreach (Type effectType in EffectTypes)
            {
                Type capturedType = effectType;
                string displayName = ObjectNames.NicifyVariableName(effectType.Name);

                if (currentType == effectType)
                    menu.AddDisabledItem(new GUIContent(displayName), true);
                else
                    menu.AddItem(new GUIContent(displayName), false, () => AssignEffect(serializedObject, propertyPath, capturedType));
            }

            menu.DropDown(selectorRect);
        }

        private static void AssignEffect(SerializedObject serializedObject, string propertyPath, Type effectType)
        {
            Undo.RecordObjects(serializedObject.targetObjects, "Change Consumable Effect");
            serializedObject.Update();

            SerializedProperty effectProperty = serializedObject.FindProperty(propertyPath);

            if (effectProperty == null) return;

            effectProperty.managedReferenceValue = effectType == null ? null : Activator.CreateInstance(effectType);
            effectProperty.isExpanded = true;
            serializedObject.ApplyModifiedProperties();
        }

        private static string GetEffectDisplayName(SerializedProperty effectProperty)
        {
            object effect = effectProperty.managedReferenceValue;

            if (effect != null) return ObjectNames.NicifyVariableName(effect.GetType().Name);

            return string.IsNullOrEmpty(effectProperty.managedReferenceFullTypename) ? "Select Effect" : "Missing Effect Type";
        }

        private static void DrawEffectFields(Rect position, SerializedProperty effectProperty)
        {
            SerializedProperty current = effectProperty.Copy();
            SerializedProperty end = effectProperty.GetEndProperty();
            float currentY = position.y;
            bool enterChildren = true;

            int previousIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            while (current.NextVisible(enterChildren) && !SerializedProperty.EqualContents(current, end))
            {
                enterChildren = false;

                if (current.depth != effectProperty.depth + 1) continue;

                float height = EditorGUI.GetPropertyHeight(current, true);
                Rect fieldRect = new(position.x, currentY, position.width, height);

                EditorGUI.PropertyField(fieldRect, current, true);
                currentY += height + VerticalSpacing;
            }

            EditorGUI.indentLevel = previousIndent;
        }

        private static float GetEffectFieldsHeight(SerializedProperty effectProperty)
        {
            if (effectProperty.managedReferenceValue == null) return 0f;

            SerializedProperty current = effectProperty.Copy();
            SerializedProperty end = effectProperty.GetEndProperty();
            float height = 0f;
            bool hasField = false;
            bool enterChildren = true;

            while (current.NextVisible(enterChildren) && !SerializedProperty.EqualContents(current, end))
            {
                enterChildren = false;

                if (current.depth != effectProperty.depth + 1) continue;

                if (hasField) height += VerticalSpacing;
                height += EditorGUI.GetPropertyHeight(current, true);
                hasField = true;
            }

            return height;
        }

        private static List<Type> FindEffectTypes()
        {
            List<Type> effectTypes = new();

            foreach (Type type in TypeCache.GetTypesDerivedFrom<ConsumableEffect>())
                if (type.IsSerializable && !type.IsAbstract && !type.IsGenericType && type.GetConstructor(Type.EmptyTypes) != null)
                    effectTypes.Add(type);

            effectTypes.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal));
            return effectTypes;
        }
    }
}
