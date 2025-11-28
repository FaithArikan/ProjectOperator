using UnityEngine;
using NeuralWaveBureau.UI;

namespace NeuralWaveBureau.AI
{
    /// <summary>
    /// Monitors citizen processing completion and triggers victory screen
    /// when all citizens have been successfully processed.
    /// </summary>
    public class VictoryController : MonoBehaviour
    {
        public static VictoryController Instance { get; private set; }

        // State tracking
        private int _citizensProcessed = 0;
        private bool _victoryTriggered = false;

        public int CitizensProcessed => _citizensProcessed;
        public bool VictoryTriggered => _victoryTriggered;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Called when a citizen has been successfully processed and exited.
        /// </summary>
        public void OnCitizenProcessed()
        {
            if (_victoryTriggered)
                return;

            _citizensProcessed++;
            Debug.Log($"[VictoryController] Citizen processed: {_citizensProcessed}");

            CheckVictoryConditions();
        }

        /// <summary>
        /// Checks if victory conditions are met and triggers victory screen
        /// </summary>
        public void CheckVictoryConditions()
        {
            if (_victoryTriggered)
                return;

            // Check if we've processed enough citizens
            if (_citizensProcessed >= CitizenSpawner.Instance.TotalCitizens)
            {
                TriggerVictory();
            }
        }

        /// <summary>
        /// Manually trigger the victory screen
        /// </summary>
        [ContextMenu("Trigger Victory")]
        public void TriggerVictory()
        {
            if (_victoryTriggered)
            {
                Debug.LogWarning("[VictoryController] Victory already triggered!");
                return;
            }

            _victoryTriggered = true;
            Debug.Log("[VictoryController] VICTORY! Showing victory screen...");

            // Show victory screen
            if (VictoryScreen.Instance != null)
            {
                // Use average obedience from all exited citizens
                float finalObedience = CitizenSpawner.Instance != null
                    ? CitizenSpawner.Instance.GetAverageExitedObedience()
                    : 100f;

                VictoryScreen.Instance.ShowVictoryScreen(_citizensProcessed, finalObedience);
            }
            else
            {
                Debug.LogError("[VictoryController] Cannot show victory screen - not assigned!");
            }
        }

        /// <summary>
        /// Test victory screen with high obedience (95%)
        /// </summary>
        [ContextMenu("Test Victory - High Obedience (95%)")]
        public void TestVictoryHighObedience()
        {
            TestVictoryWithObedience(95f);
        }

        /// <summary>
        /// Test victory screen with medium obedience (60%)
        /// </summary>
        [ContextMenu("Test Victory - Medium Obedience (60%)")]
        public void TestVictoryMediumObedience()
        {
            TestVictoryWithObedience(60f);
        }

        /// <summary>
        /// Test victory screen with low obedience (20%)
        /// </summary>
        [ContextMenu("Test Victory - Low Obedience (20%)")]
        public void TestVictoryLowObedience()
        {
            TestVictoryWithObedience(20f);
        }

        /// <summary>
        /// Test victory screen with zero obedience (0%) - triggers execution sequence
        /// </summary>
        [ContextMenu("Test Victory - Zero Obedience (0%) [SPY EXECUTION]")]
        public void TestVictoryZeroObedience()
        {
            TestVictoryWithObedience(0f);
        }

        /// <summary>
        /// Helper method to test victory screen with specific obedience level
        /// </summary>
        private void TestVictoryWithObedience(float obediencePercentage)
        {
            Debug.Log($"[VictoryController] Testing victory screen with {obediencePercentage}% obedience");

            if (VictoryScreen.Instance != null)
            {
                VictoryScreen.Instance.ShowVictoryScreen(10, obediencePercentage);
            }
            else
            {
                Debug.LogError("[VictoryController] Cannot show victory screen - VictoryScreen.Instance not found!");
            }
        }

        /// <summary>
        /// Reset victory state (useful for restarting)
        /// </summary>
        public void ResetVictory()
        {
            _citizensProcessed = 0;
            _victoryTriggered = false;
            Debug.Log("[VictoryController] Victory state reset");
        }
    }
}