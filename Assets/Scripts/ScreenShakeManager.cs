using UnityEngine;
using Unity.Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace NeuralWaveBureau
{
    /// <summary>
    /// Types of screen shake effects for different game events
    /// </summary>
    public enum ShakeType
    {
        /// <summary>Small horizontal shake, side-to-side movement</summary>
        LeftRight,

        /// <summary>Vertical punch, quick upward/downward jolt</summary>
        UpDown,

        /// <summary>Diagonal shake combining horizontal + vertical motion</summary>
        Diagonal,

        /// <summary>Strong burst shake for explosions or heavy impacts</summary>
        ExplosionImpact,

        /// <summary>Fast micro-jitter for horror glitch effects</summary>
        GlitchJitter,

        /// <summary>Slow but powerful shake for giant footsteps or heavy hits</summary>
        HeavyStomp,

        /// <summary>Low-frequency rumble, earthquake-like vibration</summary>
        Rumble,

        /// <summary>Quick, short tremor that flickers the camera</summary>
        FlickerShake,

        /// <summary>One strong hit followed by small fading shakes</summary>
        Aftershock,

        /// <summary>Pulsing shake that grows and fades — good for tension buildup</summary>
        DreadPulse
    }

    /// <summary>
    /// Configuration for a screen shake effect
    /// </summary>
    [System.Serializable]
    public class ShakeConfig
    {
        [Tooltip("Duration of the shake in seconds")]
        public float duration;

        [Tooltip("Intensity/amplitude of the shake")]
        public float amplitude;

        [Tooltip("Speed of oscillation")]
        public float frequency;

        [Tooltip("Directional bias for the shake")]
        public Vector3 direction = Vector3.one;

        [Tooltip("Attack time - how quickly shake reaches full intensity")]
        public float attackTime = 0f;

        [Tooltip("Decay time - how quickly shake fades out")]
        public float decayTime = 0.3f;

        [Tooltip("Use rotation shake (camera tilt)")]
        public bool useRotationShake = false;

        [Tooltip("Rotation intensity if enabled")]
        public float rotationIntensity = 0f;

        public ShakeConfig(float duration, float amplitude, float frequency, Vector3 direction,
            float attackTime = 0f, float decayTime = 0.3f, bool useRotation = false, float rotationIntensity = 0f)
        {
            this.duration = duration;
            this.amplitude = amplitude;
            this.frequency = frequency;
            this.direction = direction;
            this.attackTime = attackTime;
            this.decayTime = decayTime;
            this.useRotationShake = useRotation;
            this.rotationIntensity = rotationIntensity;
        }
    }

    /// <summary>
    /// Manages screen shake effects using Cinemachine Impulse system.
    /// Provides 10 predefined shake types and supports custom shake configurations.
    /// Multiple shakes can run simultaneously and will blend naturally.
    /// </summary>
    public class ScreenShakeManager : MonoBehaviour
    {
        [Header("Shake Configurations")]
        [SerializeField]
        [Tooltip("Hardcoded defaults can be tweaked here")]
        private ShakeConfig[] _shakeConfigs = new ShakeConfig[10];

        [Header("Global Settings")]
        [SerializeField]
        [Tooltip("Enable/disable all screen shake")]
        private bool _shakeEnabled = true;

        [SerializeField]
        [Range(0f, 2f)]
        [Tooltip("Global intensity multiplier (accessibility)")]
        private float _globalIntensityMultiplier = 1f;

        [Header("Debug")]
        [SerializeField]
        private bool _verboseLogging = false;

        // Cinemachine impulse source
        private CinemachineImpulseSource _impulseSource;

        // Singleton pattern
        public static ScreenShakeManager Instance { get; private set; }

        /// <summary>
        /// Gets or sets whether screen shake is enabled globally
        /// </summary>
        public bool ShakeEnabled
        {
            get => _shakeEnabled;
            set => _shakeEnabled = value;
        }

        /// <summary>
        /// Gets or sets the global intensity multiplier for all shakes (0-2, default 1)
        /// </summary>
        public float GlobalIntensityMultiplier
        {
            get => _globalIntensityMultiplier;
            set => _globalIntensityMultiplier = Mathf.Clamp(value, 0f, 2f);
        }

        private void Awake()
        {
            Instance = this;
            InitializeImpulseSource();
            EnsureCameraListeners();
        }

        /// <summary>
        /// Initialize default shake configurations when component is added
        /// </summary>
        private void Reset()
        {
            InitializeDefaultConfigs();
        }

        /// <summary>
        /// Initializes the Cinemachine impulse source component
        /// </summary>
        private void InitializeImpulseSource()
        {
            _impulseSource = GetComponent<CinemachineImpulseSource>();
            if (_impulseSource == null)
            {
                _impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
            }

            // Configure impulse source defaults
            _impulseSource.DefaultVelocity = Vector3.one;
        }

        /// <summary>
        /// Ensures all cameras in CameraManager have impulse listeners
        /// </summary>
        private void EnsureCameraListeners()
        {
            if (CameraManager.Instance == null)
            {
                Debug.LogWarning("[ScreenShakeManager] CameraManager not found. Impulse listeners should be added manually to cameras.");
                return;
            }

            // Get all CinemachineCamera components in the scene via CameraManager
            var roomCamera = CameraManager.Instance.GetType().GetField("_roomViewCamera",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(CameraManager.Instance) as CinemachineCamera;
            var rulesCamera = CameraManager.Instance.GetType().GetField("_rulesViewCamera",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(CameraManager.Instance) as CinemachineCamera;
            var monitorCamera = CameraManager.Instance.GetType().GetField("_monitorViewCamera",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(CameraManager.Instance) as CinemachineCamera;

            AddListenerIfMissing(roomCamera, "Room");
            AddListenerIfMissing(rulesCamera, "Rules");
            AddListenerIfMissing(monitorCamera, "Monitor");
        }

        /// <summary>
        /// Adds impulse listener to a camera if it doesn't have one
        /// </summary>
        private void AddListenerIfMissing(CinemachineCamera camera, string cameraName)
        {
            if (camera == null) return;

            var listener = camera.GetComponent<CinemachineImpulseListener>();
            if (listener == null)
            {
                listener = camera.gameObject.AddComponent<CinemachineImpulseListener>();
                listener.ChannelMask = -1; // Listen to all channels
                listener.Gain = 1f;
                listener.Use2DDistance = false;

                if (_verboseLogging)
                {
                    Debug.Log($"[ScreenShakeManager] Added CinemachineImpulseListener to {cameraName} camera");
                }
            }
        }

        /// <summary>
        /// Initializes default shake configurations for all shake types
        /// </summary>
        private void InitializeDefaultConfigs()
        {
            _shakeConfigs = new ShakeConfig[10];

            // LeftRight - Small horizontal shake
            _shakeConfigs[0] = new ShakeConfig(
                duration: 0.3f,
                amplitude: 0.5f,
                frequency: 25f,
                direction: Vector3.right,
                decayTime: 0.2f
            );

            // UpDown - Vertical punch
            _shakeConfigs[1] = new ShakeConfig(
                duration: 0.25f,
                amplitude: 0.6f,
                frequency: 30f,
                direction: Vector3.up,
                decayTime: 0.15f
            );

            // Diagonal - Combined horizontal + vertical
            _shakeConfigs[2] = new ShakeConfig(
                duration: 0.35f,
                amplitude: 0.5f,
                frequency: 20f,
                direction: new Vector3(1f, 1f, 0f).normalized,
                decayTime: 0.25f
            );

            // ExplosionImpact - Strong burst with rotation
            _shakeConfigs[3] = new ShakeConfig(
                duration: 0.8f,
                amplitude: 1.5f,
                frequency: 15f,
                direction: Vector3.one,
                decayTime: 0.5f,
                useRotation: true,
                rotationIntensity: 2f
            );

            // GlitchJitter - Fast micro-jitter
            _shakeConfigs[4] = new ShakeConfig(
                duration: 0.15f,
                amplitude: 0.3f,
                frequency: 60f,
                direction: Vector3.one,
                decayTime: 0.05f
            );

            // HeavyStomp - Slow powerful shake
            _shakeConfigs[5] = new ShakeConfig(
                duration: 0.6f,
                amplitude: 1.2f,
                frequency: 8f,
                direction: Vector3.up,
                attackTime: 0.05f,
                decayTime: 0.4f,
                useRotation: true,
                rotationIntensity: 1f
            );

            // Rumble - Low-frequency vibration
            _shakeConfigs[6] = new ShakeConfig(
                duration: 1.0f,
                amplitude: 0.4f,
                frequency: 5f,
                direction: new Vector3(0f, 1f, 0.3f).normalized,
                attackTime: 0.2f,
                decayTime: 0.6f
            );

            // FlickerShake - Quick tremor
            _shakeConfigs[7] = new ShakeConfig(
                duration: 0.12f,
                amplitude: 0.4f,
                frequency: 50f,
                direction: Vector3.right,
                decayTime: 0.08f
            );

            // Aftershock - Custom sequence (config for base shake)
            _shakeConfigs[8] = new ShakeConfig(
                duration: 1.2f,
                amplitude: 1.0f,
                frequency: 20f,
                direction: Vector3.one,
                decayTime: 0.8f,
                useRotation: true,
                rotationIntensity: 1.5f
            );

            // DreadPulse - Custom sequence (config for base shake)
            _shakeConfigs[9] = new ShakeConfig(
                duration: 2.0f,
                amplitude: 0.6f,
                frequency: 3f,
                direction: Vector3.up,
                attackTime: 0.5f,
                decayTime: 1.0f
            );
        }

        /// <summary>
        /// Gets the configuration for a specific shake type
        /// </summary>
        private ShakeConfig GetConfigForType(ShakeType type)
        {
            int index = (int)type;

            // Ensure configs are initialized
            if (_shakeConfigs == null || _shakeConfigs.Length != 10 || _shakeConfigs[index] == null)
            {
                InitializeDefaultConfigs();
            }

            return _shakeConfigs[index];
        }

        /// <summary>
        /// Triggers a screen shake effect asynchronously
        /// </summary>
        /// <param name="type">The type of shake to perform</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        public async UniTask ShakeAsync(ShakeType type, CancellationToken cancellationToken = default)
        {
            await ShakeAsync(type, 1f, cancellationToken);
        }

        /// <summary>
        /// Triggers a screen shake effect asynchronously with intensity multiplier
        /// </summary>
        /// <param name="type">The type of shake to perform</param>
        /// <param name="intensityMultiplier">Multiplier for shake intensity (default: 1.0)</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        public async UniTask ShakeAsync(ShakeType type, float intensityMultiplier, CancellationToken cancellationToken = default)
        {
            if (!_shakeEnabled)
                return;

            if (_impulseSource == null)
            {
                Debug.LogWarning("[ScreenShakeManager] Impulse source not initialized!");
                return;
            }

            // Handle special sequences
            if (type == ShakeType.Aftershock)
            {
                await ShakeAftershockSequence(intensityMultiplier, cancellationToken);
                return;
            }
            else if (type == ShakeType.DreadPulse)
            {
                await ShakeDreadPulseSequence(intensityMultiplier, cancellationToken);
                return;
            }

            // Standard shake
            ShakeConfig config = GetConfigForType(type);

            try
            {
                // Generate impulse
                GenerateImpulse(config.amplitude * intensityMultiplier, config.direction);

                if (_verboseLogging)
                {
                    Debug.Log($"[ScreenShakeManager] Shake: {type} (intensity: {intensityMultiplier})");
                }

                // Wait for shake duration
                await UniTask.Delay(
                    TimeSpan.FromSeconds(config.duration),
                    cancellationToken: cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                // Shake was cancelled
                if (_verboseLogging)
                {
                    Debug.Log($"[ScreenShakeManager] Shake cancelled: {type}");
                }
            }
        }

        /// <summary>
        /// Triggers a screen shake effect (fire-and-forget)
        /// </summary>
        /// <param name="type">The type of shake to perform</param>
        public void Shake(ShakeType type)
        {
            ShakeAsync(type, 1f, this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// Triggers a screen shake effect with intensity multiplier (fire-and-forget)
        /// </summary>
        /// <param name="type">The type of shake to perform</param>
        /// <param name="intensityMultiplier">Multiplier for shake intensity</param>
        public void Shake(ShakeType type, float intensityMultiplier)
        {
            ShakeAsync(type, intensityMultiplier, this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// Triggers a custom screen shake with specific configuration
        /// </summary>
        /// <param name="config">Custom shake configuration</param>
        public void ShakeCustom(ShakeConfig config)
        {
            if (!_shakeEnabled || _impulseSource == null)
                return;

            GenerateImpulse(config.amplitude, config.direction);
        }

        /// <summary>
        /// Generates an impulse with the specified amplitude and direction
        /// </summary>
        private void GenerateImpulse(float amplitude, Vector3 direction)
        {
            if (_impulseSource == null) return;

            float finalAmplitude = amplitude * _globalIntensityMultiplier;
            Vector3 velocity = direction.normalized * finalAmplitude;

            _impulseSource.GenerateImpulseWithVelocity(velocity);
        }

        /// <summary>
        /// Aftershock sequence: one strong hit followed by fading shakes
        /// </summary>
        private async UniTask ShakeAftershockSequence(float intensityMultiplier, CancellationToken ct)
        {
            try
            {
                // Strong initial impact
                GenerateImpulse(1.0f * intensityMultiplier, Vector3.one);

                if (_verboseLogging)
                    Debug.Log("[ScreenShakeManager] Aftershock: Initial impact");

                await UniTask.Delay(300, cancellationToken: ct);

                // First aftershock (40% strength)
                GenerateImpulse(0.4f * intensityMultiplier, Vector3.one);

                if (_verboseLogging)
                    Debug.Log("[ScreenShakeManager] Aftershock: First aftershock");

                await UniTask.Delay(400, cancellationToken: ct);

                // Second aftershock (20% strength)
                GenerateImpulse(0.2f * intensityMultiplier, Vector3.one);

                if (_verboseLogging)
                    Debug.Log("[ScreenShakeManager] Aftershock: Second aftershock");

                await UniTask.Delay(500, cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                if (_verboseLogging)
                    Debug.Log("[ScreenShakeManager] Aftershock sequence cancelled");
            }
        }

        /// <summary>
        /// DreadPulse sequence: pulsing shake that grows and fades
        /// </summary>
        private async UniTask ShakeDreadPulseSequence(float intensityMultiplier, CancellationToken ct)
        {
            float[] pulseIntensities = { 0.3f, 0.5f, 0.7f, 0.5f, 0.3f };

            try
            {
                foreach (float intensity in pulseIntensities)
                {
                    GenerateImpulse(intensity * intensityMultiplier, Vector3.up);

                    if (_verboseLogging)
                        Debug.Log($"[ScreenShakeManager] DreadPulse: Pulse at {intensity}");

                    await UniTask.Delay(400, cancellationToken: ct);
                }
            }
            catch (OperationCanceledException)
            {
                if (_verboseLogging)
                    Debug.Log("[ScreenShakeManager] DreadPulse sequence cancelled");
            }
        }

        /// <summary>
        /// Stops all active shakes by clearing the impulse
        /// </summary>
        public void StopAllShakes()
        {
            // Cinemachine impulses decay naturally, but we can force clear if needed
            if (_impulseSource != null)
            {
                // Reset impulse source to stop shake immediately
                _impulseSource.GenerateImpulseWithVelocity(Vector3.zero);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Tests all shake types in sequence (Editor only)
        /// </summary>
        [ContextMenu("Test All Shakes")]
        private void TestAllShakes()
        {
            TestShakeSequence().Forget();
        }

        /// <summary>
        /// Tests shake sequence for all types
        /// </summary>
        private async UniTaskVoid TestShakeSequence()
        {
            foreach (ShakeType type in Enum.GetValues(typeof(ShakeType)))
            {
                Debug.Log($"[ScreenShakeManager] Testing: {type}");
                await ShakeAsync(type);
                await UniTask.Delay(1000);
            }
            Debug.Log("[ScreenShakeManager] All shake tests complete!");
        }

        [ContextMenu("Test Explosion")]
        private void TestExplosion()
        {
            Shake(ShakeType.ExplosionImpact);
        }

        [ContextMenu("Test Glitch")]
        private void TestGlitch()
        {
            Shake(ShakeType.GlitchJitter);
        }

        [ContextMenu("Test Heavy Stomp")]
        private void TestHeavyStomp()
        {
            Shake(ShakeType.HeavyStomp);
        }

        [ContextMenu("Test Aftershock")]
        private void TestAftershock()
        {
            Shake(ShakeType.Aftershock);
        }

        [ContextMenu("Test Dread Pulse")]
        private void TestDreadPulse()
        {
            Shake(ShakeType.DreadPulse);
        }
#endif
    }
}
