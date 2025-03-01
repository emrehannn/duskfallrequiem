using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RagdollCopyTool : EditorWindow
{
    private GameObject sourceObject;
    private GameObject targetObject;
    private bool includeColliders = true;
    private bool includeJoints = true;
    private bool includeScripts = true;
    private Vector2 scrollPos;

    [MenuItem("Tools/Ragdoll Copy Tool")]
    static void Init()
    {
        RagdollCopyTool window = (RagdollCopyTool)EditorWindow.GetWindow(typeof(RagdollCopyTool));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Ragdoll Component Copier", EditorStyles.boldLabel);

        sourceObject = (GameObject)EditorGUILayout.ObjectField("Source Skeleton (Working)", sourceObject, typeof(GameObject), true);
        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Skeleton (New)", targetObject, typeof(GameObject), true);

        EditorGUILayout.Space();
        
        includeColliders = EditorGUILayout.Toggle("Copy Colliders", includeColliders);
        includeJoints = EditorGUILayout.Toggle("Copy Joints", includeJoints);
        includeScripts = EditorGUILayout.Toggle("Copy Scripts", includeScripts);

        EditorGUILayout.Space();

        if (GUILayout.Button("Copy Components"))
        {
            if (sourceObject == null || targetObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both source and target objects!", "OK");
                return;
            }

            CopyComponents();
        }

        EditorGUILayout.Space();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        if (copyResults.Count > 0)
        {
            GUILayout.Label("Copy Results:", EditorStyles.boldLabel);
            foreach (string result in copyResults)
            {
                GUILayout.Label(result);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private List<string> copyResults = new List<string>();

    private void CopyComponents()
    {
        copyResults.Clear();
        Undo.RegisterFullObjectHierarchyUndo(targetObject, "Copy Ragdoll Components");

        Dictionary<string, Transform> targetBones = new Dictionary<string, Transform>();
        foreach (Transform child in targetObject.GetComponentsInChildren<Transform>())
        {
            targetBones[child.name] = child;
        }

        foreach (Transform sourceChild in sourceObject.GetComponentsInChildren<Transform>())
        {
            if (targetBones.TryGetValue(sourceChild.name, out Transform targetChild))
            {
                CopyComponentsForBone(sourceChild.gameObject, targetChild.gameObject);
            }
            else
            {
                copyResults.Add($"Warning: No matching bone found for {sourceChild.name}");
            }
        }

        copyResults.Add("Component copy completed!");
    }

    private void CopyComponentsForBone(GameObject source, GameObject target)
    {
        // Copy tag and layer
        target.tag = source.tag;
        target.layer = source.layer;

        // Copy Rigidbody first
        CopyRigidbody(source, target);

        if (includeColliders)
        {
            CopyColliders(source, target);
        }
        
        if (includeJoints)
        {
            CopyJoints(source, target);
        }
        
        if (includeScripts)
        {
            CopyScripts(source, target);
        }

        // Special handling for enemyuncollider
        if (source.name == "Spine.002")
        {
            CopyEnemyUncollider(source, target);
        }
    }

    private void CopyRigidbody(GameObject source, GameObject target)
    {
        Rigidbody sourceRB = source.GetComponent<Rigidbody>();
        if (sourceRB != null)
        {
            Rigidbody targetRB = target.GetComponent<Rigidbody>();
            if (targetRB == null)
            {
                targetRB = target.AddComponent<Rigidbody>();
            }

            targetRB.mass = sourceRB.mass;
            targetRB.linearDamping = sourceRB.linearDamping;
            targetRB.angularDamping = sourceRB.angularDamping;
            targetRB.useGravity = sourceRB.useGravity;
            targetRB.isKinematic = sourceRB.isKinematic;
            targetRB.interpolation = sourceRB.interpolation;
            targetRB.collisionDetectionMode = sourceRB.collisionDetectionMode;
            targetRB.constraints = sourceRB.constraints;

            copyResults.Add($"Copied Rigidbody settings to {target.name} (Mass: {targetRB.mass})");
        }
    }

    private void CopyColliders(GameObject source, GameObject target)
{
    foreach (Collider sourceCollider in source.GetComponents<Collider>())
    {
        // Remove existing collider of the same type
        Collider existingCollider = target.GetComponent(sourceCollider.GetType()) as Collider;
        if (existingCollider != null)
        {
            DestroyImmediate(existingCollider);
        }

        // Copy the appropriate collider type
        if (sourceCollider is CapsuleCollider)
        {
            CapsuleCollider capsule = sourceCollider as CapsuleCollider;
            CapsuleCollider newCollider = target.AddComponent<CapsuleCollider>();
            newCollider.center = capsule.center;
            newCollider.radius = capsule.radius;
            newCollider.height = capsule.height;
            newCollider.direction = capsule.direction;
            newCollider.isTrigger = capsule.isTrigger;
            newCollider.material = capsule.material;
            copyResults.Add($"Copied CapsuleCollider to {target.name}");
        }
        else if (sourceCollider is BoxCollider)
        {
            BoxCollider box = sourceCollider as BoxCollider;
            BoxCollider newCollider = target.AddComponent<BoxCollider>();
            newCollider.center = box.center;
            newCollider.size = box.size;
            newCollider.isTrigger = box.isTrigger;
            newCollider.material = box.material;
            copyResults.Add($"Copied BoxCollider to {target.name}");
        }
        else if (sourceCollider is SphereCollider)
        {
            SphereCollider sphere = sourceCollider as SphereCollider;
            SphereCollider newCollider = target.AddComponent<SphereCollider>();
            newCollider.center = sphere.center;
            newCollider.radius = sphere.radius;
            newCollider.isTrigger = sphere.isTrigger;
            newCollider.material = sphere.material;
            copyResults.Add($"Copied SphereCollider to {target.name}");
        }
    }
}

    private void CopyJoints(GameObject source, GameObject target)
{
    foreach (CharacterJoint sourceJoint in source.GetComponents<CharacterJoint>())
    {
        CharacterJoint existingJoint = target.GetComponent<CharacterJoint>();
        if (existingJoint != null)
        {
            DestroyImmediate(existingJoint);
        }

        CharacterJoint newJoint = target.AddComponent<CharacterJoint>();

        // Copy basic properties
        newJoint.anchor = sourceJoint.anchor;
        newJoint.axis = sourceJoint.axis;
        newJoint.autoConfigureConnectedAnchor = sourceJoint.autoConfigureConnectedAnchor;
        newJoint.connectedAnchor = sourceJoint.connectedAnchor;
        newJoint.swingAxis = sourceJoint.swingAxis;

        // Copy limits
        SoftJointLimit lowTwist = sourceJoint.lowTwistLimit;
        SoftJointLimit highTwist = sourceJoint.highTwistLimit;
        SoftJointLimit swing1 = sourceJoint.swing1Limit;
        SoftJointLimit swing2 = sourceJoint.swing2Limit;

        newJoint.lowTwistLimit = lowTwist;
        newJoint.highTwistLimit = highTwist;
        newJoint.swing1Limit = swing1;
        newJoint.swing2Limit = swing2;

        // Copy advanced settings
        newJoint.enablePreprocessing = sourceJoint.enablePreprocessing;
        newJoint.enableProjection = sourceJoint.enableProjection;
        newJoint.projectionAngle = sourceJoint.projectionAngle;
        newJoint.projectionDistance = sourceJoint.projectionDistance;
        newJoint.massScale = sourceJoint.massScale;
        newJoint.connectedMassScale = sourceJoint.connectedMassScale;
        newJoint.breakForce = sourceJoint.breakForce;
        newJoint.breakTorque = sourceJoint.breakTorque;

        // Find and set connected body by searching entire hierarchy
        if (sourceJoint.connectedBody != null)
        {
            string connectedBodyName = sourceJoint.connectedBody.gameObject.name;
            Transform[] allTargetTransforms = targetObject.GetComponentsInChildren<Transform>();
            
            foreach (Transform t in allTargetTransforms)
            {
                if (t.name == connectedBodyName)
                {
                    Rigidbody connectedRB = t.GetComponent<Rigidbody>();
                    if (connectedRB != null)
                    {
                        newJoint.connectedBody = connectedRB;
                        copyResults.Add($"Connected joint on {target.name} to {connectedBodyName}");
                        break;
                    }
                }
            }
        }

        copyResults.Add($"Copied CharacterJoint to {target.name} (Mass Scale: {newJoint.massScale})");
    }
}

    private void CopyScripts(GameObject source, GameObject target)
    {
        MonoBehaviour[] sourceScripts = source.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in sourceScripts)
        {
            if (script == null) continue;

            System.Type scriptType = script.GetType();
            if (scriptType == typeof(RagdollCopyTool)) continue;

            MonoBehaviour existingScript = target.GetComponent(scriptType) as MonoBehaviour;
            if (existingScript != null)
            {
                DestroyImmediate(existingScript);
            }

            MonoBehaviour newScript = target.AddComponent(scriptType) as MonoBehaviour;
            UnityEditorInternal.ComponentUtility.CopyComponent(script);
            UnityEditorInternal.ComponentUtility.PasteComponentValues(newScript);
            
            copyResults.Add($"Copied {scriptType.Name} to {target.name}");
        }
    }

    private void CopyEnemyUncollider(GameObject sourceSpine, GameObject targetSpine)
    {
        Transform sourceUncollider = sourceSpine.transform.Find("enemyuncollider");
        if (sourceUncollider != null)
        {
            Transform existingUncollider = targetSpine.transform.Find("enemyuncollider");
            if (existingUncollider != null)
            {
                DestroyImmediate(existingUncollider.gameObject);
            }

            GameObject newUncollider = new GameObject("enemyuncollider");
            newUncollider.transform.SetParent(targetSpine.transform, false);

            newUncollider.transform.localPosition = sourceUncollider.localPosition;
            newUncollider.transform.localRotation = sourceUncollider.localRotation;
            newUncollider.transform.localScale = sourceUncollider.localScale;

            newUncollider.tag = sourceUncollider.gameObject.tag;
            newUncollider.layer = sourceUncollider.gameObject.layer;

            foreach (Component sourceComp in sourceUncollider.GetComponents<Component>())
            {
                if (sourceComp is Transform) continue;

                if (sourceComp is Collider)
                {
                    CopyColliderExact(sourceComp as Collider, newUncollider);
                }
                else
                {
                    UnityEditorInternal.ComponentUtility.CopyComponent(sourceComp);
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(newUncollider);
                }
            }

            copyResults.Add($"Copied enemyuncollider to {targetSpine.name}");
        }
    }

    private void CopyColliderExact(Collider sourceCollider, GameObject target)
    {
        if (sourceCollider is BoxCollider)
        {
            BoxCollider source = sourceCollider as BoxCollider;
            BoxCollider new_collider = target.AddComponent<BoxCollider>();
            new_collider.center = source.center;
            new_collider.size = source.size;
            new_collider.isTrigger = source.isTrigger;
            new_collider.material = source.material;
        }
        else if (sourceCollider is SphereCollider)
        {
            SphereCollider source = sourceCollider as SphereCollider;
            SphereCollider new_collider = target.AddComponent<SphereCollider>();
            new_collider.center = source.center;
            new_collider.radius = source.radius;
            new_collider.isTrigger = source.isTrigger;
            new_collider.material = source.material;
        }
        else if (sourceCollider is CapsuleCollider)
        {
            CapsuleCollider source = sourceCollider as CapsuleCollider;
            CapsuleCollider new_collider = target.AddComponent<CapsuleCollider>();
            new_collider.center = source.center;
            new_collider.radius = source.radius;
            new_collider.height = source.height;
            new_collider.direction = source.direction;
            new_collider.isTrigger = source.isTrigger;
            new_collider.material = source.material;
        }
    }
}