using UnityEngine;
using System.Collections.Generic;
using NeuralWaveBureau.Data;

namespace NeuralWaveBureau.AI
{
    /// <summary>
    /// Spawns citizen prefabs and sends them to monitoring stations.
    /// Can spawn citizens on demand or automatically at intervals.
    /// </summary>
    public class CitizenSpawner : MonoBehaviour
    {
        [Header("Prefab Settings")]
        [SerializeField]
        [Tooltip("List of citizen prefabs to spawn (must have CitizenController and CitizenMovement)")]
        private List<GameObject> _citizenPrefabs = new List<GameObject>();

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

        // State
        private List<GameObject> _spawnedCitizens = new List<GameObject>();
        private float _spawnTimer = 0f;
        private int _totalSpawned = 0;

        public int TotalSpawned => _totalSpawned;
        public int ActiveCitizens => _spawnedCitizens.Count;

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

            // Destroy previous citizen if enabled
            if (_spawnedCitizens.Count > 0)
            {
                DestroyOldestCitizen();
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

            // Register with AIManager
            if (aiManager != null)
            {
                aiManager.RegisterCitizen(controller);
            }

            // Send to monitoring station
            if (_targetStation != null)
            {
                movement.MoveToStation(_targetStation);
            }

            // Track spawned citizen
            _spawnedCitizens.Add(citizenObj);
            _totalSpawned++;

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
                    selectedPrefab = _citizenPrefabs[randomIndex];
                    break;

                case PrefabSelectionMode.Sequential:
                    selectedPrefab = _citizenPrefabs[_currentPrefabIndex];
                    _currentPrefabIndex = (_currentPrefabIndex + 1) % _citizenPrefabs.Count;
                    break;

                case PrefabSelectionMode.Weighted:
                    // Future feature - for now use random
                    randomIndex = Random.Range(0, _citizenPrefabs.Count);
                    selectedPrefab = _citizenPrefabs[randomIndex];
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
                // Unregister from AIManager
                CitizenController controller = oldest.GetComponent<CitizenController>();
                if (controller != null && AIManager.Instance != null)
                {
                    AIManager.Instance.UnregisterCitizen(controller);
                }

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
                    // Unregister from AIManager
                    CitizenController controller = citizen.GetComponent<CitizenController>();
                    if (controller != null && AIManager.Instance != null)
                    {
                        AIManager.Instance.UnregisterCitizen(controller);
                    }

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
