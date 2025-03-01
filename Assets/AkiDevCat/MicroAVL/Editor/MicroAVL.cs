using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AkiDevCat.MicroAVL.Editor
{
    [InitializeOnLoad]
    public static class MicroAVL
    {
        private const string DefaultMaterialPath = "MicroAVL/M_DefaultLight";

        static MicroAVL()
        {
            if (!SessionState.GetBool("MicroAVL/ProjectOpened", false))
            {
                SessionState.SetBool("MicroAVL/ProjectOpened", true);
                EditorApplication.update += InitializeWindow;
            }
            
        }

        static void InitializeWindow()
        {
            EditorApplication.update -= InitializeWindow;
            if (EditorPrefs.GetBool("MicroAVL/ShowOnStart"))
            {
                WelcomeWindow.ShowWindow();
            }
        }
        
        [MenuItem("GameObject/Light/MicroAVL Light")]
        private static void CreateLight()
        {
            var result = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            result.name = "MicroAVL Light";
            result.transform.localScale = Vector3.one * 2.0f;

            var collider = result.GetComponent<Collider>();
            if (collider)
                Object.DestroyImmediate(collider);

            var renderer = result.GetComponent<MeshRenderer>();
            renderer.material = Resources.Load<Material>(DefaultMaterialPath);

            Selection.activeGameObject = result;
        }
    }


    public class WelcomeWindow : EditorWindow
    {
        private const string AssetStoreURL = "https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/analytical-volumetric-lighting-urp-performant-raytraced-volumetr-266586";
        
        private static Texture _icon = null;
        
        [MenuItem("Help/About MicroAVL", priority=-1)]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(WelcomeWindow), false, "Welcome");
            window.minSize = new Vector2(250, 500);
            window.maxSize = new Vector2(250, 500);
        }

        private void OnGUI()
        {
            if (_icon == null)
            {
                _icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/AkiDevCat/MicroAVL/Resources/MicroAVL/AVLogo.png");
            }
            
            Rect rect;
            
            EditorGUILayout.Space(15.0f);
            EditorGUILayout.LabelField("  MicroAVL", new GUIStyle("AM MixerHeader"));
            EditorGUILayout.Space(30.0f);
            EditorGUILayout.LabelField("Thank you for using MicroAVL!", new GUIStyle("WhiteLargeCenterLabel"));

            if (_icon)
            {
                rect = EditorGUILayout.GetControlRect(false, 100);
                GUI.DrawTexture(rect, _icon, ScaleMode.ScaleToFit);
            }

            // EditorGUILayout.Space(5.0f);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (EditorGUILayout.LinkButton("Check out our full Analytical Volumetric"))
            {
                Application.OpenURL(AssetStoreURL);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (EditorGUILayout.LinkButton("Lighting package on the Asset Store"))
            {
                Application.OpenURL(AssetStoreURL);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            var style = new GUIStyle("BoldLabel");
            style.normal.textColor = new Color(0.882f, 0.78f, 1);
            
            EditorGUILayout.Space(15.0f);
            EditorGUILayout.LabelField("   • More Light Shapes", style);
            EditorGUILayout.LabelField("   • Optimized Clustered Rendering", style);
            EditorGUILayout.LabelField("   • Depth Culling", style);
            EditorGUILayout.LabelField("   • Upscaling Feature", style);
            EditorGUILayout.LabelField("   • Deeper Customization", style);
            EditorGUILayout.LabelField("   • Mask Volumes", style);
            EditorGUILayout.LabelField("   • Global Fog", style);
            EditorGUILayout.LabelField("   • Distance Culling", style);
            
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(10.0f);
            EditorGUILayout.LabelField("Show this on launch");
            GUILayout.FlexibleSpace();
            var flShowOnStart = EditorGUILayout.Toggle(EditorPrefs.GetBool("MicroAVL/ShowOnStart", true));
            EditorPrefs.SetBool("MicroAVL/ShowOnStart", flShowOnStart);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Open Example Scene", new GUIStyle("LargeButton")))
            {
                EditorSceneManager.OpenScene("Assets/AkiDevCat/MicroAVL/Examples/MicroAVL Example.unity");
            }
            if (GUILayout.Button("Close", new GUIStyle("LargeButton")))
            {
                Close();
            }
            EditorGUILayout.Space();
        }
    }
}