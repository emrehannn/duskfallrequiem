// Copyright © Sascha Graeff/13Pixels.

namespace ThirteenPixels.Placr
{
    using UnityEngine;
    using UnityEditor;
    using System;
    using System.Linq;
    
    internal class PlacrModifierList : ScriptableObject
    {
        [Serializable]
        private class ModifierSlot
        {
            private readonly Type type;
            public PlacrModifierBase modifier;
            public bool active = false;


            public ModifierSlot(Type type)
            {
                this.type = type;
                modifier = (PlacrModifierBase)CreateInstance(type);
            }

            public void ApplyTo(GameObject gameObject)
            {
                if (active)
                {
                    modifier.ApplyTo(gameObject);
                }
            }

            public void SetActive(bool active)
            {
                this.active = active;
            }
        }

        private SerializedObject serializedObject;

        [SerializeField]
        private ModifierSlot[] slots = default;
        public int activeModifierCount => slots.Count(slot => slot.active);


        private void Awake()
        {
            InitializeTypes();
            serializedObject = new SerializedObject(this);
        }

        private void InitializeTypes()
        {
            slots = TypeCache.GetTypesDerivedFrom<PlacrModifierBase>()
                .Where(type => !type.IsAbstract)
                .Select(type => new ModifierSlot(type))
                .ToArray();
        }

        public void ApplyTo(GameObject gameObject)
        {
            foreach (var slot in slots)
            {
                slot.ApplyTo(gameObject);
            }
        }

        public void Display()
        {
            if (serializedObject == null)
            {
                serializedObject = new SerializedObject(this);
            }
            serializedObject.Update();

            foreach (var slot in slots)
            {
                GUILayout.BeginVertical(GUI.skin.GetStyle("Box"));

                var wasActive = slot.active;
                var active = GUILayout.Toggle(wasActive, slot.modifier.title);

                if (active != wasActive)
                {
                    slot.SetActive(active);
                }

                if (active)
                {
                    slot.modifier.DisplayProperties();
                }

                GUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
