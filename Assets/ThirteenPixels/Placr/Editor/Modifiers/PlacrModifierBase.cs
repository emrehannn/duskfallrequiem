// Copyright © Sascha Graeff/13Pixels.

namespace ThirteenPixels.Placr
{
    using UnityEngine;
    using UnityEditor;

    public abstract class PlacrModifierBase : ScriptableObject
    {
        public abstract string title { get; }
        public abstract void ApplyTo(GameObject gameObject);
        private SerializedObject serializedObject;

        internal void DisplayProperties()
        {
            if (serializedObject == null)
            {
                serializedObject = new SerializedObject(this);
            }

            serializedObject.Update();

            var property = serializedObject.GetIterator();
            property.NextVisible(true);
            while (property.NextVisible(false))
            {
                EditorGUILayout.PropertyField(property);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
