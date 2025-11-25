using UnityEngine;
using UnityEditor;
using NeuralWaveBureau.AI;

namespace NeuralWaveBureau.Editor
{
    /// <summary>
    /// Utility to automatically set up ragdoll references on CitizenController components
    /// </summary>
    public class RagdollSetupUtility : EditorWindow
    {
        [MenuItem("Neural Wave Bureau/Fix Ragdoll References")]
        public static void FixAllRagdollReferences()
        {
            // Find all CitizenController instances in prefabs
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/POLYGONCityCharacters/Prefabs" });
            int fixedCount = 0;
            int skippedCount = 0;

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null)
                {
                    CitizenController controller = prefab.GetComponent<CitizenController>();

                    if (controller != null)
                    {
                        bool wasFixed = SetupRagdollReferences(prefab, controller);
                        if (wasFixed)
                        {
                            EditorUtility.SetDirty(prefab);
                            fixedCount++;
                            Debug.Log($"[RagdollSetup] Fixed ragdoll references for: {prefab.name}");
                        }
                        else
                        {
                            skippedCount++;
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[RagdollSetup] Complete! Fixed {fixedCount} prefabs, skipped {skippedCount} (no issues found).");
            EditorUtility.DisplayDialog("Ragdoll Setup Complete",
                $"Fixed ragdoll references for {fixedCount} citizen prefabs.\n\n" +
                $"Skipped {skippedCount} prefabs (already configured or no ragdoll components found).",
                "OK");
        }

        private static bool SetupRagdollReferences(GameObject prefab, CitizenController controller)
        {
            // Get all Rigidbody and Collider components in children
            Rigidbody[] allRigidbodies = prefab.GetComponentsInChildren<Rigidbody>(true);
            Collider[] allColliders = prefab.GetComponentsInChildren<Collider>(true);

            // Filter out the main collider (usually on root or main body)
            // Ragdoll components are typically on child bones
            Rigidbody mainRigidbody = prefab.GetComponent<Rigidbody>();
            Collider mainCollider = prefab.GetComponent<Collider>();

            // Collect ragdoll-specific rigidbodies and colliders
            System.Collections.Generic.List<Rigidbody> ragdollBodies = new System.Collections.Generic.List<Rigidbody>();
            System.Collections.Generic.List<Collider> ragdollColliders = new System.Collections.Generic.List<Collider>();

            foreach (var rb in allRigidbodies)
            {
                // Skip the main rigidbody if it exists
                if (rb != mainRigidbody && rb != null)
                {
                    ragdollBodies.Add(rb);
                }
            }

            foreach (var col in allColliders)
            {
                // Skip the main collider if it exists
                if (col != mainCollider && col != null)
                {
                    ragdollColliders.Add(col);
                }
            }

            // If no ragdoll components found, nothing to fix
            if (ragdollBodies.Count == 0 && ragdollColliders.Count == 0)
            {
                return false;
            }

            // Use SerializedObject to modify private fields
            SerializedObject so = new SerializedObject(controller);

            SerializedProperty ragdollBodiesProperty = so.FindProperty("_ragdollBodies");
            SerializedProperty ragdollCollidersProperty = so.FindProperty("_ragdollColliders");

            if (ragdollBodiesProperty != null)
            {
                ragdollBodiesProperty.arraySize = ragdollBodies.Count;
                for (int i = 0; i < ragdollBodies.Count; i++)
                {
                    ragdollBodiesProperty.GetArrayElementAtIndex(i).objectReferenceValue = ragdollBodies[i];
                }
            }

            if (ragdollCollidersProperty != null)
            {
                ragdollCollidersProperty.arraySize = ragdollColliders.Count;
                for (int i = 0; i < ragdollColliders.Count; i++)
                {
                    ragdollCollidersProperty.GetArrayElementAtIndex(i).objectReferenceValue = ragdollColliders[i];
                }
            }

            so.ApplyModifiedProperties();

            Debug.Log($"[RagdollSetup] {prefab.name}: Found {ragdollBodies.Count} rigidbodies and {ragdollColliders.Count} colliders");
            return true;
        }

        [MenuItem("Neural Wave Bureau/Clear All Ragdoll References")]
        public static void ClearAllRagdollReferences()
        {
            if (!EditorUtility.DisplayDialog("Clear Ragdoll References",
                "This will clear all ragdoll references from citizen prefabs. This is useful if you want to disable the ragdoll feature.\n\nAre you sure?",
                "Yes, Clear Them", "Cancel"))
            {
                return;
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/POLYGONCityCharacters/Prefabs" });
            int clearedCount = 0;

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null)
                {
                    CitizenController controller = prefab.GetComponent<CitizenController>();

                    if (controller != null)
                    {
                        SerializedObject so = new SerializedObject(controller);

                        SerializedProperty ragdollBodiesProperty = so.FindProperty("_ragdollBodies");
                        SerializedProperty ragdollCollidersProperty = so.FindProperty("_ragdollColliders");

                        if (ragdollBodiesProperty != null)
                        {
                            ragdollBodiesProperty.ClearArray();
                        }

                        if (ragdollCollidersProperty != null)
                        {
                            ragdollCollidersProperty.ClearArray();
                        }

                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(prefab);
                        clearedCount++;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[RagdollSetup] Cleared ragdoll references for {clearedCount} prefabs.");
        }
    }
}
