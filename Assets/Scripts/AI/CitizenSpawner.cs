using UnityEngine;
using System.Collections.Generic;
using NeuralWaveBureau.Data;
using NeuralWaveBureau.UI;

namespace NeuralWaveBureau.AI
{
    /// <summary>
    /// Spawns citizen prefabs and sends them to monitoring stations.
    /// Can spawn citizens on demand or automatically at intervals.
    /// </summary>
    public class CitizenSpawner : MonoBehaviour
    {
        public static CitizenSpawner Instance { get; private set; }

        [Header("Prefab Settings")]
        [SerializeField]
        [Tooltip("List of citizen prefabs to spawn (must have CitizenController and CitizenMovement)")]
        private List<NeuralProfile> _citizenPrefabs = new();

        public int TotalCitizens => _citizenPrefabs.Count;

        [SerializeField]
        [Tooltip("Random selection mode for prefabs")]
        private PrefabSelectionMode _prefabSelectionMode = PrefabSelectionMode.Random;

        public enum PrefabSelectionMode
        {
            Random,      // Pick random prefab each time
            Sequential,  // Cycle through prefabs in order
            Weighted     // Use weights (future feature)
        }

        // Sequential spawn tracking
        private int _currentPrefabIndex = 0;

        [Header("Spawn Settings")]
        [SerializeField]
        [Tooltip("Where citizens spawn")]
        private Transform _spawnPoint;

        [SerializeField]
        [Tooltip("Monitoring station where citizens should go")]
        private MonitoringStation _targetStation;

        [SerializeField]
        [Tooltip("Where citizens go when they are done")]
        private Transform _exitPoint;

        // State
        public List<GameObject> _spawnedCitizens = new List<GameObject>();
        private float _spawnTimer = 0f;
        private int _totalSpawned = 0;
        private bool _isCurrentCitizenDone = true; // Default to true so we can spawn the first one

        // Track obedience of citizens who have exited
        private List<float> _exitedCitizenObedience = new List<float>();

        public int TotalSpawned => _totalSpawned;
        public int ActiveCitizens => _spawnedCitizens.Count;

        /// <summary>
        /// Gets the average obedience of all exited citizens
        /// </summary>
        public float GetAverageExitedObedience()
        {
            if (_exitedCitizenObedience.Count == 0)
                return 100f;

            float sum = 0f;
            foreach (float obedience in _exitedCitizenObedience)
            {
                sum += obedience;
            }
            return sum / _exitedCitizenObedience.Count;
        }


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Validate setup
            if (_citizenPrefabs == null || _citizenPrefabs.Count == 0)
            {
                Debug.LogError("[CitizenSpawner] No citizen prefabs assigned!");
                enabled = false;
                return;
            }

            if (_spawnPoint == null)
            {
                Debug.LogWarning("[CitizenSpawner] No spawn point assigned, using spawner position");
                _spawnPoint = transform;
            }

            if (_targetStation == null)
            {
                Debug.LogWarning("[CitizenSpawner] No target station assigned! Citizens will spawn but not move.");
            }
        }

        /// <summary>
        /// Spawns a new citizen with a specific profile
        /// </summary>
        public GameObject SpawnCitizen()
        {
            // Get a prefab to spawn
            GameObject prefabToSpawn = GetNextPrefab();

            if (prefabToSpawn == null)
            {
                Debug.LogError("[CitizenSpawner] Cannot spawn - no valid prefab available!");
                return null;
            }

            // Check if we have an active citizen that is not done
            if (_spawnedCitizens.Count > 0)
            {
                if (!_isCurrentCitizenDone)
                {
                    // Check if obedience is high enough to allow early finish
                    if (ObedienceController.Instance != null && ObedienceController.Instance.CurrentObedience > 90f)
                    {
                        Debug.Log("[CitizenSpawner] Obedience > 90%, finishing current citizen.");
                        // Proceed to send to exit below
                    }
                    else
                    {
                        Debug.LogWarning("[CitizenSpawner] Cannot spawn new citizen - current citizen is not done yet!");
                        return null;
                    }
                }

                // If we are here, we are spawning a new citizen, so the old one must leave.
                // Ensure the previous citizen is sent to the exit.
                if (BrainActivityMonitor.Instance != null)
                    BrainActivityMonitor.Instance.StopMonitoring();

                GameObject previousCitizen = _spawnedCitizens[_spawnedCitizens.Count - 1];
                SendCitizenToExit(previousCitizen);

                _isCurrentCitizenDone = true;
            }

            // Destroy previous citizen if it exists and we are enforcing single citizen (optional, but good for cleanup)
            // For now, we'll assume FinishCurrentCitizen handles the cleanup of the previous one, 
            // or we let them coexist if the previous one is walking away.
            // If you want strictly ONE citizen at a time, uncomment the line below:
            // if (_spawnedCitizens.Count > 0) DestroyOldestCitizen();

            // Reset knobs for the new citizen
            if (WaveKnobManager.Instance != null)
            {
                WaveKnobManager.Instance.ResetAllKnobs();
            }

            // Spawn the citizen
            Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : transform.position;
            Quaternion spawnRot = _spawnPoint != null ? _spawnPoint.rotation : transform.rotation;

            GameObject citizenObj = Instantiate(prefabToSpawn, spawnPos, spawnRot);
            citizenObj.name = $"Citizen_{_totalSpawned:D3}";

            // Get components
            CitizenController controller = citizenObj.GetComponent<CitizenController>();
            CitizenMovement movement = citizenObj.GetComponent<CitizenMovement>();

            if (controller == null)
            {
                Debug.LogError("[CitizenSpawner] Spawned prefab doesn't have CitizenController component!");
                Destroy(citizenObj);
                return null;
            }

            if (movement == null)
            {
                Debug.LogError("[CitizenSpawner] Spawned prefab doesn't have CitizenMovement component!");
                Destroy(citizenObj);
                return null;
            }

            // Initialize with AISettings
            AIManager aiManager = AIManager.Instance;
            if (aiManager != null && aiManager.Settings != null)
            {
                controller.Initialize(aiManager.Settings);
            }
            else
            {
                Debug.LogWarning("[CitizenSpawner] AIManager not found! Citizen may not be properly initialized.");
            }

            // Register with AIManager - REMOVED (AIManager no longer tracks all citizens)
            // if (aiManager != null)
            // {
            //     aiManager.RegisterCitizen(controller);
            // }

            // Send to monitoring station
            if (_targetStation != null)
            {
                movement.MoveToStation(_targetStation);
            }

            // Track spawned citizen
            _spawnedCitizens.Add(citizenObj);
            _totalSpawned++;
            _isCurrentCitizenDone = false;

            string prefabName = prefabToSpawn.name;
            Debug.Log($"[CitizenSpawner] Spawned citizen #{_totalSpawned} ({prefabName}) with profile: {controller.Profile}");

            return citizenObj;
        }

        /// <summary>
        /// Gets the next prefab to spawn based on selection mode
        /// </summary>
        private GameObject GetNextPrefab()
        {
            if (_citizenPrefabs == null || _citizenPrefabs.Count == 0)
            {
                return null;
            }

            GameObject selectedPrefab = null;

            switch (_prefabSelectionMode)
            {
                case PrefabSelectionMode.Random:
                    int randomIndex = Random.Range(0, _citizenPrefabs.Count);
                    selectedPrefab = _citizenPrefabs[randomIndex].prefab;
                    break;

                case PrefabSelectionMode.Sequential:
                    selectedPrefab = _citizenPrefabs[_currentPrefabIndex].prefab;
                    _currentPrefabIndex = (_currentPrefabIndex + 1) % _citizenPrefabs.Count;
                    break;

                case PrefabSelectionMode.Weighted:
                    // Future feature - for now use random
                    randomIndex = Random.Range(0, _citizenPrefabs.Count);
                    selectedPrefab = _citizenPrefabs[randomIndex].prefab;
                    break;
            }

            return selectedPrefab;
        }

        /// <summary>
        /// Destroys the oldest spawned citizen
        /// </summary>
        private void DestroyOldestCitizen()
        {
            if (_spawnedCitizens.Count == 0)
                return;

            GameObject oldest = _spawnedCitizens[0];
            _spawnedCitizens.RemoveAt(0);

            if (oldest != null)
            {
                // Unregister from AIManager - REMOVED
                // CitizenController controller = oldest.GetComponent<CitizenController>();
                // if (controller != null && AIManager.Instance != null)
                // {
                //     AIManager.Instance.UnregisterCitizen(controller);
                // }

                Destroy(oldest);
                Debug.Log("[CitizenSpawner] Destroyed oldest citizen");
            }
        }

        /// <summary>
        /// Destroys all spawned citizens
        /// </summary>
        public void DestroyAllCitizens()
        {
            foreach (GameObject citizen in _spawnedCitizens)
            {
                if (citizen != null)
                {
                    // Unregister from AIManager - REMOVED
                    // CitizenController controller = citizen.GetComponent<CitizenController>();
                    // if (controller != null && AIManager.Instance != null)
                    // {
                    //     AIManager.Instance.UnregisterCitizen(controller);
                    // }

                    Destroy(citizen);
                }
            }

            _spawnedCitizens.Clear();
            Debug.Log("[CitizenSpawner] Destroyed all citizens");
        }

        /// <summary>
        /// Manually trigger spawn (useful for button clicks)
        /// </summary>
        [ContextMenu("Spawn Citizen Now")]
        public void SpawnCitizenNow()
        {
            SpawnCitizen();
        }

        /// <summary>
        /// Marks the current citizen as done and sends them to the exit point.
        /// </summary>
        public void FinishCurrentCitizen()
        {
            if (_isCurrentCitizenDone)
            {
                Debug.LogWarning("[CitizenSpawner] Current citizen is already done or no citizen active!");
                return;
            }

            if (_spawnedCitizens.Count == 0)
            {
                _isCurrentCitizenDone = true;
                return;
            }

            BrainActivityMonitor.Instance.StopMonitoring();

            // Get the current active citizen (last one spawned)
            GameObject currentCitizen = _spawnedCitizens[_spawnedCitizens.Count - 1];
            SendCitizenToExit(currentCitizen);

            _isCurrentCitizenDone = true;
        }

        private void SendCitizenToExit(GameObject citizen)
        {
            if (citizen != null)
            {
                CitizenMovement movement = citizen.GetComponent<CitizenMovement>();
                if (movement != null && _exitPoint != null)
                {
                    movement.MoveTo(_exitPoint.position);

                    // Subscribe to OnArrived to destroy them
                    // Remove first to avoid double subscription
                    movement.OnArrived -= HandleCitizenExit;
                    movement.OnArrived += HandleCitizenExit;

                    Debug.Log($"[CitizenSpawner] Sending citizen {citizen.name} to exit.");
                }
                else
                {
                    // If no movement or exit point, just destroy them or mark done
                    if (_exitPoint == null)
                    {
                        Debug.LogWarning("[CitizenSpawner] No exit point assigned. Destroying citizen immediately.");
                        DestroyOldestCitizen();
                    }
                }
            }
        }

        private void HandleCitizenExit(CitizenMovement movement)
        {
            movement.OnArrived -= HandleCitizenExit;

            // Unregister from AIManager - REMOVED
            // CitizenController controller = movement.GetComponent<CitizenController>();
            // if (controller != null && AIManager.Instance != null)
            // {
            //     AIManager.Instance.UnregisterCitizen(controller);
            // }

            // Save the citizen's obedience score when they exit
            if (ObedienceController.Instance != null)
            {
                float exitObedience = ObedienceController.Instance.CurrentObedience;
                _exitedCitizenObedience.Add(exitObedience);
                Debug.Log($"[CitizenSpawner] Citizen {movement.gameObject.name} exited with obedience: {exitObedience:F1}%");
            }

            if (_spawnedCitizens.Contains(movement.gameObject))
            {
                _spawnedCitizens.Remove(movement.gameObject);
            }

            // Notify VictoryController that a citizen has been successfully processed
            if (VictoryController.Instance != null)
            {
                VictoryController.Instance.OnCitizenProcessed();
            }

            Destroy(movement.gameObject);
            Debug.Log($"[CitizenSpawner] Citizen {movement.gameObject.name} has left the area and was destroyed.");
        }

        private void OnDestroy()
        {
            // Clean up all spawned citizens
            DestroyAllCitizens();
        }

        private void OnDrawGizmos()
        {
            // Draw spawn point
            if (_spawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_spawnPoint.position, 0.5f);
                Gizmos.DrawLine(_spawnPoint.position, _spawnPoint.position + _spawnPoint.forward * 1f);
            }

            // Draw line to target station
            if (_spawnPoint != null && _targetStation != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(_spawnPoint.position, _targetStation.GetCitizenPosition());
            }
        }
    }
}
