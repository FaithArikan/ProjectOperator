using UnityEngine;
using UnityEngine.AI;
using System;

namespace NeuralWaveBureau.AI
{
    /// <summary>
    /// Handles citizen movement and navigation to monitoring station.
    /// Supports both NavMesh and simple linear movement.
    /// </summary>
    [RequireComponent(typeof(CitizenController))]
    public class CitizenMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField]
        [Tooltip("Walking speed in units per second")]
        private float _walkSpeed = 1.5f;

        [SerializeField]
        [Tooltip("Rotation speed when turning")]
        private float _rotationSpeed = 180f;

        [SerializeField]
        [Tooltip("Distance to destination considered as 'arrived'")]
        private float _arrivalDistance = 0.2f;

        [SerializeField]
        [Tooltip("Use NavMesh for navigation (if false, uses simple linear movement)")]
        private bool _useNavMesh = false;

        [Header("Animation")]
        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private string _walkAnimationParameter = "isWalking";

        [SerializeField]
        private string _speedAnimationParameter = "walkSpeed";

        // Components
        private NavMeshAgent _navAgent;
        private CitizenController _citizenController;

        // State
        private Vector3 _destination;
        private bool _isMoving = false;
        private bool _hasArrived = false;
        private MonitoringStation _targetStation;

        // Events
        public event Action<CitizenMovement> OnArrived;
        public event Action<CitizenMovement> OnStartedWalking;

        public bool IsMoving => _isMoving;
        public bool HasArrived => _hasArrived;
        public MonitoringStation TargetStation => _targetStation;
        public CitizenController CitizenController => _citizenController;


        private void Awake()
        {
            _citizenController = GetComponent<CitizenController>();

            // Try to get NavMeshAgent if using NavMesh
            if (_useNavMesh)
            {
                _navAgent = GetComponent<NavMeshAgent>();
                if (_navAgent == null)
                {
                    Debug.LogWarning($"[CitizenMovement] NavMesh enabled but no NavMeshAgent found on {gameObject.name}. Switching to simple movement.");
                    _useNavMesh = false;
                }
                else
                {
                    _navAgent.speed = _walkSpeed;
                    _navAgent.angularSpeed = _rotationSpeed;
                    _navAgent.stoppingDistance = _arrivalDistance;
                }
            }

            // Auto-assign animator
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }

        /// <summary>
        /// Moves citizen to a specific monitoring station
        /// </summary>
        public void MoveToStation(MonitoringStation station)
        {
            if (station == null)
            {
                Debug.LogError("[CitizenMovement] Cannot move to null station!");
                return;
            }

            _targetStation = station;
            MoveTo(station.GetCitizenPosition());
        }

        /// <summary>
        /// Moves citizen to a specific position
        /// </summary>
        public void MoveTo(Vector3 destination)
        {
            _destination = destination;
            _isMoving = true;
            _hasArrived = false;

            if (_useNavMesh && _navAgent != null)
            {
                _navAgent.SetDestination(_destination);
                _navAgent.isStopped = false;
            }

            // Update animation
            UpdateWalkAnimation(true);

            OnStartedWalking?.Invoke(this);
            Debug.Log($"[CitizenMovement] {_citizenController.CitizenId} started walking to {_destination}");
        }

        /// <summary>
        /// Stops movement immediately
        /// </summary>
        public void Stop()
        {
            _isMoving = false;

            if (_useNavMesh && _navAgent != null)
            {
                _navAgent.isStopped = true;
            }

            UpdateWalkAnimation(false);
        }

        private void Update()
        {
            if (!_isMoving)
                return;

            if (_useNavMesh)
            {
                UpdateNavMeshMovement();
            }
            else
            {
                UpdateSimpleMovement();
            }
        }

        /// <summary>
        /// Updates movement using NavMeshAgent
        /// </summary>
        private void UpdateNavMeshMovement()
        {
            if (_navAgent == null)
                return;

            // Check if arrived
            if (!_navAgent.pathPending && _navAgent.remainingDistance <= _navAgent.stoppingDistance)
            {
                if (!_navAgent.hasPath || _navAgent.velocity.sqrMagnitude == 0f)
                {
                    ArriveAtDestination();
                }
            }

            // Update animation speed based on actual velocity
            if (_animator != null)
            {
                float speed = _navAgent.velocity.magnitude / _walkSpeed;
                _animator.SetFloat(_speedAnimationParameter, speed);
            }
        }

        /// <summary>
        /// Updates movement using simple linear interpolation
        /// </summary>
        private void UpdateSimpleMovement()
        {
            Vector3 currentPos = transform.position;
            float distanceToDestination = Vector3.Distance(currentPos, _destination);

            // Check if arrived
            if (distanceToDestination <= _arrivalDistance)
            {
                ArriveAtDestination();
                return;
            }

            // Move towards destination
            Vector3 direction = (_destination - currentPos).normalized;
            Vector3 movement = direction * _walkSpeed * Time.deltaTime;
            transform.position = currentPos + movement;

            // Rotate towards movement direction
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }

            // Update animation
            if (_animator != null)
            {
                _animator.SetFloat(_speedAnimationParameter, 1f);
            }
        }

        /// <summary>
        /// Called when citizen arrives at destination
        /// </summary>
        private void ArriveAtDestination()
        {
            if (_hasArrived)
                return;

            _isMoving = false;
            _hasArrived = true;

            // Snap to exact position
            transform.position = _destination;

            // Face the monitoring station if we have one
            if (_targetStation != null)
            {
                Vector3 lookDirection = _targetStation.GetFacingDirection();
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }

            UpdateWalkAnimation(false);

            Debug.Log($"[CitizenMovement] {_citizenController.CitizenId} arrived at destination");
            OnArrived?.Invoke(this);

            // Notify station
            if (_targetStation != null)
            {
                _targetStation.OnCitizenArrived(this);
            }
        }

        /// <summary>
        /// Updates walk animation state
        /// </summary>
        private void UpdateWalkAnimation(bool isWalking)
        {
            if (_animator == null)
                return;

            _animator.SetBool(_walkAnimationParameter, isWalking);

            if (!isWalking)
            {
                _animator.SetFloat(_speedAnimationParameter, 0f);
            }
        }

        /// <summary>
        /// Teleports citizen to position (no walking)
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            Stop();
            transform.position = position;
            _destination = position;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw destination
            if (_isMoving)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_destination, _arrivalDistance);
                Gizmos.DrawLine(transform.position, _destination);
            }
        }
    }
}
