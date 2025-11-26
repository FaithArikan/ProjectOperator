using UnityEngine;
using UnityEngine.AI;
using System;
using UnityEngine.Events;

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
        [Tooltip("Name of the idle animation state")]
        private string _idleStateName = "Idle";

        [SerializeField]
        [Tooltip("Name of the walking animation state")]
        private string _walkStateName = "Walking";

        [SerializeField]
        [Tooltip("Minimum velocity to consider as 'walking' for animation (helps prevent animation glitches)")]
        private float _walkAnimationVelocityThreshold = 0.1f;

        [SerializeField]
        [Tooltip("Force immediate animation transition (skips exit time)")]
        private bool _forceImmediateTransition = true;

        [SerializeField]
        [Tooltip("Use CrossFade for smoother transition (if false, uses instant Play)")]
        private bool _useCrossFade = false;

        [SerializeField]
        [Tooltip("CrossFade transition duration in seconds")]
        private float _crossFadeDuration = 0.1f;

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

        [Header("Events")]
        public UnityEvent OnArrivedAtDestination;

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

            if (_animator != null && _animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning($"[CitizenMovement] Animator found on {gameObject.name} but no AnimatorController is assigned. Animations will not play.");
            }

            // Validate idle state name exists
            ValidateAnimatorState();
        }

        /// <summary>
        /// Validates that the idle state exists in the animator
        /// </summary>
        private void ValidateAnimatorState()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return;

            // Try to check if the state exists (this is a basic check)
            bool stateExists = false;
            foreach (UnityEngine.AnimatorControllerParameter param in _animator.parameters)
            {
                if (param.name == _walkAnimationParameter)
                {
                    stateExists = true;
                    break;
                }
            }

            if (!stateExists)
            {
                Debug.LogWarning($"[CitizenMovement] Animation parameter '{_walkAnimationParameter}' not found in animator on {gameObject.name}. Make sure your Animator Controller has this parameter.");
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
                _navAgent.ResetPath(); // Clear the path to ensure velocity drops to zero
            }

            UpdateWalkAnimation(false);
        }

        private void Update()
        {
            if (!_isMoving)
            {
                // Even when not moving, check if we need to update animation (important for NavMesh)
                UpdateWalkAnimationBasedOnVelocity();
                return;
            }

            if (_useNavMesh)
            {
                UpdateNavMeshMovement();
            }
            else
            {
                UpdateSimpleMovement();
            }

            // Continuously update animation based on actual velocity
            UpdateWalkAnimationBasedOnVelocity();
        }

        /// <summary>
        /// Updates movement using NavMeshAgent
        /// </summary>
        private void UpdateNavMeshMovement()
        {
            if (_navAgent == null)
                return;

            // Check if arrived - using a more strict velocity check
            if (!_navAgent.pathPending)
            {
                // Check if we're within stopping distance AND velocity is very low
                float distanceToTarget = _navAgent.remainingDistance;
                float velocityMagnitude = _navAgent.velocity.magnitude;

                // Agent has arrived if: close to destination AND nearly stopped
                if (distanceToTarget <= _navAgent.stoppingDistance &&
                    velocityMagnitude <= _walkAnimationVelocityThreshold)
                {
                    if (!_navAgent.hasPath || distanceToTarget == 0f)
                    {
                        ArriveAtDestination();
                    }
                }
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

            // Stop NavMeshAgent if using NavMesh
            if (_useNavMesh && _navAgent != null)
            {
                _navAgent.isStopped = true;
                _navAgent.ResetPath();
            }

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

            OnArrivedAtDestination?.Invoke();
        }

        /// <summary>
        /// Updates walk animation state
        /// </summary>
        private void UpdateWalkAnimation(bool isWalking)
        {
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return;

            Debug.Log($"[CitizenMovement] {_citizenController.CitizenId} is walking: {isWalking}");

            // Always update the boolean parameter first
            _animator.SetBool(_walkAnimationParameter, isWalking);

            // Force immediate transition if enabled
            if (_forceImmediateTransition)
            {
                if (isWalking)
                {
                    ForceWalkState();
                }
                else
                {
                    ForceIdleState();
                }
            }
        }

        /// <summary>
        /// Forces immediate transition to idle state
        /// </summary>
        private void ForceIdleState()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return;

            if (_useCrossFade)
            {
                // Use CrossFade for smoother transition
                _animator.CrossFade(_idleStateName, _crossFadeDuration, 0);
            }
            else
            {
                // Instant transition
                _animator.Play(_idleStateName, 0, 0f);
            }
        }

        /// <summary>
        /// Forces immediate transition to walk state
        /// </summary>
        private void ForceWalkState()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return;

            if (_useCrossFade)
            {
                // Use CrossFade for smoother transition
                _animator.CrossFade(_walkStateName, _crossFadeDuration, 0);
            }
            else
            {
                // Instant transition
                _animator.Play(_walkStateName, 0, 0f);
            }
        }

        /// <summary>
        /// Updates walk animation based on actual velocity
        /// </summary>
        private void UpdateWalkAnimationBasedOnVelocity()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return;

            float currentSpeed = 0f;

            if (_useNavMesh && _navAgent != null)
            {
                // Use NavMeshAgent's velocity
                currentSpeed = _navAgent.velocity.magnitude;
            }
            else
            {
                // For simple movement, calculate velocity from position change
                // This is less accurate but works for non-NavMesh movement
                currentSpeed = _isMoving ? _walkSpeed : 0f;
            }

            // Only play walk animation if actually moving above threshold
            bool shouldWalk = currentSpeed > _walkAnimationVelocityThreshold;
            bool isCurrentlyWalking = _animator.GetBool(_walkAnimationParameter);

            // Only update if the state has changed
            if (isCurrentlyWalking != shouldWalk)
            {
                // Always update the boolean parameter first
                _animator.SetBool(_walkAnimationParameter, shouldWalk);

                // Force immediate transition if enabled
                if (_forceImmediateTransition)
                {
                    if (shouldWalk)
                    {
                        ForceWalkState();
                    }
                    else
                    {
                        ForceIdleState();
                    }
                }
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
