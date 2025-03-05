using UnityEditor;
using UnityEngine;

public class ProjectSettingsInjector : EditorWindow
{
    private static ProjectSettingsInjector instance;
    
    [MenuItem("Tools/Active Ragdoll Project Settings Injector")]
    static void APRPlayerSettingsInjectorWindow()
    {
        if(instance == null)
        {
            ProjectSettingsInjector window = CreateInstance(typeof(ProjectSettingsInjector)) as ProjectSettingsInjector;
            window.maxSize = new Vector2(350f, 190f);
            window.minSize = window.maxSize;
            window.ShowUtility();
        }
    }
    
    void OnEnable()
    {
        instance = this;
    }
    
    void OnGUI()
    { 
        GUI.skin.label.wordWrap = true;

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
        GUILayout.Label("This tool will apply all the required project settings needed like new layers, tags, world gravity and physics iterations");
        EditorGUILayout.Space();
        if(GUILayout.Button("Inject Project Settings"))
        {
            Physics.gravity = new Vector3(0, -25, 0);
            Physics.defaultSolverIterations = 25;
            Physics.defaultSolverVelocityIterations = 15;
            
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
 
            SerializedProperty layersProp = tagManager.FindProperty("layers");
 
 
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);

            }
 
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(0);
                SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);

            }
            

 

            tagManager.ApplyModifiedProperties();
            
            Debug.Log("The project settings has been successfully injected");
            
            this.Close();
        }
    }
    
    void OnDisable()
    {
        instance = null;
    }
}