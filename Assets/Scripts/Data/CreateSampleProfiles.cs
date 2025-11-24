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

            Debug.Log("[CreateSampleProfiles] Created sample profiles and settings in Assets/Data/Profiles/");
        }
#endif
    }
}
