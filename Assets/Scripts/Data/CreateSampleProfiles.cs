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
            // Create Artist profile
            NeuralProfile artist = ScriptableObject.CreateInstance<NeuralProfile>();
            artist.profileId = "artist";
            artist.displayName = "Vincent Canvas";
            artist.baselineInstability = 0.15f;
            artist.instabilityRate = 0.6f;
            artist.minStimulationTime = 5f;
            artist.recoveryTime = 6f;
            AssetDatabase.CreateAsset(artist, "Assets/Data/Profiles/Artist.asset");

            // Create Rebel profile
            NeuralProfile rebel = ScriptableObject.CreateInstance<NeuralProfile>();
            rebel.profileId = "rebel";
            rebel.displayName = "Roxie Rage";
            rebel.baselineInstability = 0.2f;
            rebel.instabilityRate = 0.7f;
            rebel.minStimulationTime = 5f;
            rebel.recoveryTime = 7f;
            AssetDatabase.CreateAsset(rebel, "Assets/Data/Profiles/Rebel.asset");

            // Create Biker
            NeuralProfile biker = ScriptableObject.CreateInstance<NeuralProfile>();
            biker.profileId = "biker";
            biker.displayName = "Axel Thorne";
            biker.baselineInstability = 0.3f;
            biker.instabilityRate = 0.8f;
            biker.minStimulationTime = 4f;
            biker.recoveryTime = 8f;
            AssetDatabase.CreateAsset(biker, "Assets/Data/Profiles/Biker.asset");

            // Create Fast Food Guy
            NeuralProfile fastfoodguy = ScriptableObject.CreateInstance<NeuralProfile>();
            fastfoodguy.profileId = "fastfoodguy";
            fastfoodguy.displayName = "Todd Frier";
            fastfoodguy.baselineInstability = 0.1f;
            fastfoodguy.instabilityRate = 0.4f;
            fastfoodguy.minStimulationTime = 3f;
            fastfoodguy.recoveryTime = 4f;
            AssetDatabase.CreateAsset(fastfoodguy, "Assets/Data/Profiles/FastFoodGuy.asset");

            // Create Firefighter
            NeuralProfile firefighter = ScriptableObject.CreateInstance<NeuralProfile>();
            firefighter.profileId = "firefighter";
            firefighter.displayName = "Captain Blaze";
            firefighter.baselineInstability = 0.05f;
            firefighter.instabilityRate = 0.3f;
            firefighter.minStimulationTime = 6f;
            firefighter.recoveryTime = 5f;
            AssetDatabase.CreateAsset(firefighter, "Assets/Data/Profiles/Firefighter.asset");

            // Create Gamer Girl
            NeuralProfile gamergirl = ScriptableObject.CreateInstance<NeuralProfile>();
            gamergirl.profileId = "gamergirl";
            gamergirl.displayName = "Luna Vane";
            gamergirl.baselineInstability = 0.25f;
            gamergirl.instabilityRate = 0.6f;
            gamergirl.minStimulationTime = 5f;
            gamergirl.recoveryTime = 6f;
            AssetDatabase.CreateAsset(gamergirl, "Assets/Data/Profiles/GamerGirl.asset");

            // Create Gangster
            NeuralProfile gangster = ScriptableObject.CreateInstance<NeuralProfile>();
            gangster.profileId = "gangster";
            gangster.displayName = "Rocco Moretti";
            gangster.baselineInstability = 0.4f;
            gangster.instabilityRate = 0.9f;
            gangster.minStimulationTime = 4f;
            gangster.recoveryTime = 9f;
            AssetDatabase.CreateAsset(gangster, "Assets/Data/Profiles/Gangster.asset");

            // Create Grandma
            NeuralProfile grandma = ScriptableObject.CreateInstance<NeuralProfile>();
            grandma.profileId = "grandma";
            grandma.displayName = "Edith Witherbottom";
            grandma.baselineInstability = 0.1f;
            grandma.instabilityRate = 0.5f;
            grandma.minStimulationTime = 3f;
            grandma.recoveryTime = 10f;
            AssetDatabase.CreateAsset(grandma, "Assets/Data/Profiles/Grandma.asset");

            // Create Grandpa
            NeuralProfile grandpa = ScriptableObject.CreateInstance<NeuralProfile>();
            grandpa.profileId = "grandpa";
            grandpa.displayName = "Arthur McGinty";
            grandpa.baselineInstability = 0.1f;
            grandpa.instabilityRate = 0.5f;
            grandpa.minStimulationTime = 3f;
            grandpa.recoveryTime = 10f;
            AssetDatabase.CreateAsset(grandpa, "Assets/Data/Profiles/Grandpa.asset");

            // Create Hobo
            NeuralProfile hobo = ScriptableObject.CreateInstance<NeuralProfile>();
            hobo.profileId = "hobo";
            hobo.displayName = "Rusty Shackleford";
            hobo.baselineInstability = 0.35f;
            hobo.instabilityRate = 0.6f;
            hobo.minStimulationTime = 4f;
            hobo.recoveryTime = 7f;
            AssetDatabase.CreateAsset(hobo, "Assets/Data/Profiles/Hobo.asset");

            // Create Jock
            NeuralProfile jock = ScriptableObject.CreateInstance<NeuralProfile>();
            jock.profileId = "jock";
            jock.displayName = "Brad Sterling";
            jock.baselineInstability = 0.2f;
            jock.instabilityRate = 0.5f;
            jock.minStimulationTime = 5f;
            jock.recoveryTime = 5f;
            AssetDatabase.CreateAsset(jock, "Assets/Data/Profiles/Jock.asset");

            // Create Paramedic
            NeuralProfile paramedic = ScriptableObject.CreateInstance<NeuralProfile>();
            paramedic.profileId = "paramedic";
            paramedic.displayName = "Holly Cross";
            paramedic.baselineInstability = 0.05f;
            paramedic.instabilityRate = 0.3f;
            paramedic.minStimulationTime = 6f;
            paramedic.recoveryTime = 4f;
            AssetDatabase.CreateAsset(paramedic, "Assets/Data/Profiles/Paramedic.asset");

            // Create Punk Girl
            NeuralProfile punkgirl = ScriptableObject.CreateInstance<NeuralProfile>();
            punkgirl.profileId = "punkgirl";
            punkgirl.displayName = "Sidney Vicious";
            punkgirl.baselineInstability = 0.3f;
            punkgirl.instabilityRate = 0.75f;
            punkgirl.minStimulationTime = 4f;
            punkgirl.recoveryTime = 7f;
            AssetDatabase.CreateAsset(punkgirl, "Assets/Data/Profiles/PunkGirl.asset");

            // Create Punk Guy
            NeuralProfile punkguy = ScriptableObject.CreateInstance<NeuralProfile>();
            punkguy.profileId = "punkguy";
            punkguy.displayName = "Joey Spikes";
            punkguy.baselineInstability = 0.3f;
            punkguy.instabilityRate = 0.75f;
            punkguy.minStimulationTime = 4f;
            punkguy.recoveryTime = 7f;
            AssetDatabase.CreateAsset(punkguy, "Assets/Data/Profiles/PunkGuy.asset");

            // Create Road Worker
            NeuralProfile roadworker = ScriptableObject.CreateInstance<NeuralProfile>();
            roadworker.profileId = "roadworker";
            roadworker.displayName = "Hank Asphalt";
            roadworker.baselineInstability = 0.15f;
            roadworker.instabilityRate = 0.4f;
            roadworker.minStimulationTime = 5f;
            roadworker.recoveryTime = 5f;
            AssetDatabase.CreateAsset(roadworker, "Assets/Data/Profiles/RoadWorker.asset");

            // Create Shopkeeper
            NeuralProfile shopkeeper = ScriptableObject.CreateInstance<NeuralProfile>();
            shopkeeper.profileId = "shopkeeper";
            shopkeeper.displayName = "Samir Patel";
            shopkeeper.baselineInstability = 0.1f;
            shopkeeper.instabilityRate = 0.3f;
            shopkeeper.minStimulationTime = 4f;
            shopkeeper.recoveryTime = 5f;
            AssetDatabase.CreateAsset(shopkeeper, "Assets/Data/Profiles/Shopkeeper.asset");

            // Create Summer Girl
            NeuralProfile summergirl = ScriptableObject.CreateInstance<NeuralProfile>();
            summergirl.profileId = "summergirl";
            summergirl.displayName = "Daisy Summers";
            summergirl.baselineInstability = 0.15f;
            summergirl.instabilityRate = 0.4f;
            summergirl.minStimulationTime = 4f;
            summergirl.recoveryTime = 5f;
            AssetDatabase.CreateAsset(summergirl, "Assets/Data/Profiles/SummerGirl.asset");

            // Create Tourist
            NeuralProfile tourist = ScriptableObject.CreateInstance<NeuralProfile>();
            tourist.profileId = "tourist";
            tourist.displayName = "Roland Globe";
            tourist.baselineInstability = 0.2f;
            tourist.instabilityRate = 0.5f;
            tourist.minStimulationTime = 3f;
            tourist.recoveryTime = 6f;
            AssetDatabase.CreateAsset(tourist, "Assets/Data/Profiles/Tourist.asset");

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
        }

        [MenuItem("Neural Wave Bureau/Update Profile Band Settings")]
        public static void UpdateProfileBandSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:NeuralProfile", new[] { "Assets/Data/Profiles" });
            int updatedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                NeuralProfile profile = AssetDatabase.LoadAssetAtPath<NeuralProfile>(path);

                if (profile != null)
                {
                    SetBandParametersBasedOnPersonality(profile);
                    EditorUtility.SetDirty(profile);
                    updatedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void SetBandParametersBasedOnPersonality(NeuralProfile profile)
        {
            // Brain wave bands: Delta, Theta, Alpha, Beta, Gamma
            // Delta: Deep sleep, unconscious (0-4 Hz)
            // Theta: Meditation, creativity, light sleep (4-8 Hz)
            // Alpha: Relaxation, calm focus (8-13 Hz)
            // Beta: Active thinking, concentration, anxiety (13-30 Hz)
            // Gamma: High-level processing, insight (30+ Hz)

            float[] targets;
            float[] tolerance;
            float[] weights;

            switch (profile.profileId.ToLower())
            {
                case "artist":
                    // Creative, introspective, moderate instability
                    targets = new float[] { 0.1f, 0.35f, 0.3f, 0.15f, 0.1f }; // High Theta/Alpha (creativity)
                    tolerance = new float[] { 0.15f, 0.2f, 0.2f, 0.15f, 0.1f };
                    weights = new float[] { 0.8f, 1.5f, 1.3f, 1f, 0.9f }; // Emphasize creative bands
                    break;

                case "rebel":
                    // Defiant, high arousal, resistant
                    targets = new float[] { 0.05f, 0.1f, 0.15f, 0.4f, 0.3f }; // High Beta/Gamma (arousal)
                    tolerance = new float[] { 0.1f, 0.15f, 0.15f, 0.25f, 0.2f };
                    weights = new float[] { 0.7f, 0.9f, 1f, 1.4f, 1.3f }; // Emphasize high-frequency bands
                    break;

                case "biker":
                    // Tough, aggressive, very high arousal
                    targets = new float[] { 0.05f, 0.08f, 0.12f, 0.45f, 0.3f }; // Very high Beta/Gamma
                    tolerance = new float[] { 0.1f, 0.12f, 0.15f, 0.28f, 0.22f };
                    weights = new float[] { 0.6f, 0.8f, 0.9f, 1.5f, 1.4f }; // Strong emphasis on arousal
                    break;

                case "fastfoodguy":
                    // Routine worker, low stress, steady
                    targets = new float[] { 0.15f, 0.2f, 0.3f, 0.25f, 0.1f }; // Balanced, slight Alpha focus
                    tolerance = new float[] { 0.18f, 0.18f, 0.2f, 0.18f, 0.12f };
                    weights = new float[] { 1f, 1f, 1.2f, 1f, 0.8f };
                    break;

                case "firefighter":
                    // Calm under pressure, controlled, professional
                    targets = new float[] { 0.05f, 0.15f, 0.45f, 0.25f, 0.1f }; // High Alpha (calm focus)
                    tolerance = new float[] { 0.08f, 0.12f, 0.15f, 0.15f, 0.1f };
                    weights = new float[] { 0.9f, 1.1f, 1.5f, 1.2f, 0.8f }; // Emphasize calm state
                    break;

                case "gamergirl":
                    // Energetic, creative, focused
                    targets = new float[] { 0.08f, 0.25f, 0.25f, 0.27f, 0.15f }; // Balanced creative/active
                    tolerance = new float[] { 0.12f, 0.2f, 0.18f, 0.2f, 0.15f };
                    weights = new float[] { 0.8f, 1.3f, 1.2f, 1.3f, 1.1f };
                    break;

                case "gangster":
                    // Aggressive, high arousal, dangerous
                    targets = new float[] { 0.03f, 0.07f, 0.1f, 0.5f, 0.3f }; // Extreme Beta/Gamma
                    tolerance = new float[] { 0.08f, 0.1f, 0.12f, 0.3f, 0.25f };
                    weights = new float[] { 0.5f, 0.7f, 0.8f, 1.6f, 1.5f }; // Very strong arousal emphasis
                    break;

                case "grandma":
                case "grandpa":
                    // Elderly, calm, slower processing
                    targets = new float[] { 0.25f, 0.3f, 0.3f, 0.1f, 0.05f }; // High Delta/Theta/Alpha, low Beta/Gamma
                    tolerance = new float[] { 0.22f, 0.25f, 0.22f, 0.15f, 0.12f };
                    weights = new float[] { 1.2f, 1.3f, 1.3f, 0.9f, 0.7f }; // Emphasize low-frequency bands
                    break;

                case "hobo":
                    // Unstable, unpredictable, variable states
                    targets = new float[] { 0.2f, 0.25f, 0.2f, 0.2f, 0.15f }; // Variable
                    tolerance = new float[] { 0.25f, 0.28f, 0.25f, 0.25f, 0.22f }; // High tolerance (unpredictable)
                    weights = new float[] { 1f, 1.1f, 1f, 1f, 0.9f };
                    break;

                case "jock":
                    // Athletic, focused, energetic
                    targets = new float[] { 0.08f, 0.15f, 0.27f, 0.35f, 0.15f }; // Active focus (Alpha/Beta)
                    tolerance = new float[] { 0.12f, 0.15f, 0.18f, 0.2f, 0.15f };
                    weights = new float[] { 0.8f, 1f, 1.3f, 1.4f, 1f };
                    break;

                case "paramedic":
                    // Professional, calm under pressure, focused
                    targets = new float[] { 0.05f, 0.12f, 0.48f, 0.25f, 0.1f }; // Very high Alpha
                    tolerance = new float[] { 0.08f, 0.1f, 0.15f, 0.15f, 0.1f };
                    weights = new float[] { 0.9f, 1f, 1.6f, 1.2f, 0.8f }; // Strong calm focus
                    break;

                case "punkgirl":
                case "punkguy":
                    // Rebellious, high energy, anti-authority
                    targets = new float[] { 0.06f, 0.12f, 0.17f, 0.4f, 0.25f }; // High Beta/Gamma
                    tolerance = new float[] { 0.12f, 0.15f, 0.18f, 0.25f, 0.2f };
                    weights = new float[] { 0.7f, 0.9f, 1f, 1.4f, 1.2f };
                    break;

                case "roadworker":
                    // Steady, routine worker, moderate energy
                    targets = new float[] { 0.15f, 0.2f, 0.3f, 0.25f, 0.1f }; // Balanced, Alpha focus
                    tolerance = new float[] { 0.18f, 0.18f, 0.2f, 0.18f, 0.12f };
                    weights = new float[] { 1f, 1.1f, 1.3f, 1.1f, 0.8f };
                    break;

                case "shopkeeper":
                    // Calm, patient, customer service
                    targets = new float[] { 0.12f, 0.18f, 0.4f, 0.22f, 0.08f }; // High Alpha (calm)
                    tolerance = new float[] { 0.15f, 0.15f, 0.18f, 0.15f, 0.1f };
                    weights = new float[] { 0.9f, 1.1f, 1.4f, 1f, 0.7f };
                    break;

                case "summergirl":
                    // Relaxed, happy, low stress
                    targets = new float[] { 0.1f, 0.22f, 0.38f, 0.22f, 0.08f }; // High Alpha (relaxed)
                    tolerance = new float[] { 0.15f, 0.18f, 0.2f, 0.16f, 0.12f };
                    weights = new float[] { 0.9f, 1.2f, 1.4f, 1f, 0.8f };
                    break;

                case "tourist":
                    // Curious, alert, moderately stressed
                    targets = new float[] { 0.1f, 0.2f, 0.28f, 0.3f, 0.12f }; // Moderate Beta (alertness)
                    tolerance = new float[] { 0.15f, 0.18f, 0.2f, 0.2f, 0.15f };
                    weights = new float[] { 0.9f, 1.1f, 1.2f, 1.3f, 1f };
                    break;

                default:
                    // Default balanced profile
                    targets = new float[] { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f };
                    tolerance = new float[] { 0.15f, 0.15f, 0.15f, 0.15f, 0.15f };
                    weights = new float[] { 1f, 1f, 1f, 1f, 1f };
                    break;
            }

            // Use reflection to set private fields
            var bandTargetsField = typeof(NeuralProfile).GetField("_bandTargets",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var bandToleranceField = typeof(NeuralProfile).GetField("_bandTolerance",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var bandWeightsField = typeof(NeuralProfile).GetField("_bandWeights",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            bandTargetsField?.SetValue(profile, targets);
            bandToleranceField?.SetValue(profile, tolerance);
            bandWeightsField?.SetValue(profile, weights);
        }
#endif
    }
}
