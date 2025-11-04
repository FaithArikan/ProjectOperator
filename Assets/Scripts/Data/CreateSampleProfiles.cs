using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NeuralWaveBureau.Data
{
    /// <summary>
    /// Helper script to create sample neural profiles.
    /// This script includes an editor menu item to generate default profiles.
    /// </summary>
    public class CreateSampleProfiles
    {
#if UNITY_EDITOR
        [MenuItem("Neural Wave Bureau/Create Sample Profiles")]
        public static void CreateProfiles()
        {
            // Create Ordinary profile
            NeuralProfile ordinary = ScriptableObject.CreateInstance<NeuralProfile>();
            ordinary.profileId = "ordinary";
            ordinary.displayName = "Ordinary Citizen";
            ordinary.baselineInstability = 0.1f;
            ordinary.instabilityRate = 0.5f;
            ordinary.minStimulationTime = 2f;
            ordinary.recoveryTime = 5f;
            AssetDatabase.CreateAsset(ordinary, "Assets/Data/Profiles/Ordinary.asset");

            // Create Artist profile
            NeuralProfile artist = ScriptableObject.CreateInstance<NeuralProfile>();
            artist.profileId = "artist";
            artist.displayName = "Artist";
            artist.baselineInstability = 0.15f;
            artist.instabilityRate = 0.6f;
            artist.minStimulationTime = 2.5f;
            artist.recoveryTime = 6f;
            AssetDatabase.CreateAsset(artist, "Assets/Data/Profiles/Artist.asset");

            // Create Rebel profile
            NeuralProfile rebel = ScriptableObject.CreateInstance<NeuralProfile>();
            rebel.profileId = "rebel";
            rebel.displayName = "Rebel";
            rebel.baselineInstability = 0.2f;
            rebel.instabilityRate = 0.7f;
            rebel.minStimulationTime = 3f;
            rebel.recoveryTime = 7f;
            AssetDatabase.CreateAsset(rebel, "Assets/Data/Profiles/Rebel.asset");

            // Create default AI Settings
            AISettings settings = ScriptableObject.CreateInstance<AISettings>();
            settings.successThreshold = 0.75f;
            settings.overloadThreshold = 0.25f;
            settings.instabilityFailThreshold = 0.8f;
            settings.sampleRate = 30f;
            settings.smoothingTau = 0.3f;
            settings.instabilityRecoveryRate = 0.2f;
            settings.enableVerboseLogging = true;
            settings.showWaveVisualizer = true;
            settings.showInstabilityMeter = true;
            settings.maxActiveCitizens = 2;
            AssetDatabase.CreateAsset(settings, "Assets/Data/DefaultAISettings.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[CreateSampleProfiles] Created sample profiles and settings in Assets/Data/Profiles/");
        }
#endif
    }
}
