using UnityEngine;
using UnityEditor;
using NeuralWaveBureau.AI;

namespace NeuralWaveBureau.Editor
{
    [CustomEditor(typeof(CitizenSpawner))]
    public class CitizenSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            CitizenSpawner spawner = (CitizenSpawner)target;

            GUILayout.Space(10);
            GUILayout.Label("Debug Controls", EditorStyles.boldLabel);

            if (GUILayout.Button("Spawn Citizen"))
            {
                spawner.SpawnCitizen();
            }

            if (GUILayout.Button("Finish Current Citizen"))
            {
                spawner.FinishCurrentCitizen();
            }

            if (GUILayout.Button("Destroy All Citizens"))
            {
                spawner.DestroyAllCitizens();
            }
        }
    }
}
