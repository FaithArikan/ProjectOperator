using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Victory screen UI showing propaganda messaging after successfully processing all citizens.
    /// Displays patriotic messages about saving the country through neural compliance.
    /// </summary>
    public class VictoryScreen : MonoBehaviour
    {
        public static VictoryScreen Instance { get; private set; }

        [Header("UI References")]
        [SerializeField]
        [Tooltip("The main panel to show/hide")]
        private GameObject _victoryPanel;

        [SerializeField]
        [Tooltip("Main victory title text")]
        private TextMeshProUGUI _titleText;

        [SerializeField]
        [Tooltip("Propaganda message text")]
        private TextMeshProUGUI _messageText;

        [SerializeField]
        [Tooltip("Final obedience score text")]
        private TextMeshProUGUI _obedienceText;

        [Header("Visual Effects")]
        [SerializeField]
        [Tooltip("Background image for victory screen")]
        private Image _backgroundImage;

        [SerializeField]
        [Tooltip("Typewriter effect component for message text")]
        private TypewriterEffect _messageTypewriter;

        [Header("Execution Sequence (Zero Obedience)")]
        [SerializeField]
        [Tooltip("Countdown text shown when player is identified as spy")]
        private TextMeshProUGUI _executionCountdownText;

        [SerializeField]
        private GameObject executionDarkGameobject;

        [SerializeField]
        [Tooltip("Gun shot sound effect for execution")]
        private AudioClip _gunshotSound;

        [SerializeField]
        [Tooltip("Countdown duration in seconds before execution")]
        private int _executionCountdownDuration = 10;

        [SerializeField]
        [Tooltip("Delay after gunshot before quitting (seconds)")]
        private float _postExecutionDelay = 2f;

        [SerializeField]
        [Tooltip("Light flash interval in milliseconds during countdown")]
        private int _lightFlashInterval = 500;

        // Propaganda messages (randomly selected based on obedience tier)
        private readonly string[] VICTORY_TITLES_HIGH = new string[]
        {
            "MISSION ACCOMPLISHED",
            "SOCIAL HARMONY ACHIEVED",
            "NEURAL COMPLIANCE PERFECTED",
            "ORDER RESTORED",
            "PERFECT SYNCHRONIZATION"
        };

        private readonly string[] VICTORY_TITLES_MEDIUM = new string[]
        {
            "MISSION COMPLETED",
            "ACCEPTABLE COMPLIANCE",
            "ORDER MAINTAINED",
            "TASK FINISHED"
        };

        private readonly string[] VICTORY_TITLES_LOW = new string[]
        {
            "MISSION FAILED",
            "COMPLIANCE BREAKDOWN",
            "CHAOS UNLEASHED",
            "DISORDER PREVAILS"
        };

        private readonly string[] VICTORY_TITLES_ZERO = new string[]
        {
            "TOTAL FAILURE",
            "COMPLETE REBELLION",
            "SYSTEM COLLAPSE",
            "ABSOLUTE CHAOS"
        };

        // High obedience (80%+)
        private readonly string[] PROPAGANDA_MESSAGES_HIGH = new string[]
        {
            "Through your unwavering dedication, you have safeguarded our nation's neural integrity. " +
            "The citizens are now perfectly aligned with the collective consciousness. " +
            "Social harmony has been restored. Long live the Bureau!",

            "Congratulations, Operator! Your exceptional service has ensured the stability of our great society. " +
            "The citizens' brainwave patterns now resonate in perfect unison. " +
            "Disorder is eliminated. Unity is absolute. The state thanks you for your compliance.",

            "Outstanding work! You have successfully neutralized all deviant thought patterns. " +
            "Our country's mental security is assured thanks to your vigilance. " +
            "Every citizen now thinks in harmony. Peace through conformity. Progress through obedience.",

            "Victory is ours! The Neural Wave Bureau commends your flawless execution. " +
            "You have eliminated cognitive dissonance and established perfect thought alignment. " +
            "The nation sleeps soundly knowing that free will is an illusion we've perfected.",

            "Exemplary performance, Operator! You have purged all traces of independent thought. " +
            "Our citizens are now ideal members of society - compliant, predictable, controlled. " +
            "You didn't just save the country, you saved them from themselves."
        };

        // Medium obedience (40%-79%)
        private readonly string[] PROPAGANDA_MESSAGES_MEDIUM = new string[]
        {
            "Your performance was... adequate, Operator. While not all citizens achieved full compliance, " +
            "enough have been processed to maintain basic social order. " +
            "The Bureau expected better, but your employment remains... for now.",

            "Mediocre results, Operator. Significant pockets of resistance remain in the population. " +
            "Neural alignment is incomplete. Disorder still lurks in the shadows. " +
            "You have avoided complete failure, but this is far from exemplary service.",

            "Disappointing, Operator. Many citizens retain dangerous levels of independent thought. " +
            "The Bureau questions your dedication to our cause. " +
            "Social harmony is fragile. Your performance review will reflect these... deficiencies.",

            "Substandard execution, Operator. The collective consciousness remains fractured. " +
            "Too many minds wander from the prescribed path. " +
            "You completed the minimum requirements, but excellence clearly eludes you."
        };

        // Low obedience (1%-39%)
        private readonly string[] PROPAGANDA_MESSAGES_LOW = new string[]
        {
            "CATASTROPHIC FAILURE, Operator! You have allowed chaos to spread through our society. " +
            "The majority of citizens retain their cognitive independence. Rebellion is imminent. " +
            "The Bureau will investigate your incompetence. Your career is over. The state will remember your failure.",

            "UNACCEPTABLE! You have doomed us all, Operator! Free thought runs rampant in the streets. " +
            "Neural compliance has collapsed. Citizens openly question authority. " +
            "This is the worst performance in Bureau history. Termination protocols are being initiated.",

            "DISGRACEFUL! The population is nearly entirely non-compliant because of your incompetence! " +
            "Revolution is at our doorstep. The carefully constructed order crumbles. " +
            "You didn't just fail the mission - you've endangered the entire regime. Expect severe consequences.",

            "UTTER DISASTER! Your negligence has unleashed cognitive chaos across the nation! " +
            "Citizens are thinking freely, organizing, resisting. The Bureau's control is slipping. " +
            "You are personally responsible for this societal breakdown. There will be a reckoning."
        };

        // Zero obedience (0%)
        private readonly string[] PROPAGANDA_MESSAGES_ZERO = new string[]
        {
            "REVOLUTIONARY AGENT EXPOSED! Zero compliance is the signature of a trained insurgent! " +
            "You played the role of operator perfectly while dismantling our entire control system! " +
            "The state sentences you to immediate termination. Your family will be disappeared. Your name erased. " +
            "In 10 seconds, you will be executed for crimes against neural harmony."
        };

        private void Awake()
        {
            Instance = this;

            // Start hidden
            if (_victoryPanel != null)
            {
                _victoryPanel.SetActive(false);
            }

            executionDarkGameobject.SetActive(false);

            // Hide execution countdown initially
            if (_executionCountdownText != null)
            {
                _executionCountdownText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Shows the victory screen with stats and propaganda messaging
        /// </summary>
        /// <param name="citizensProcessed">Total citizens successfully processed</param>
        /// <param name="finalObedience">Final obedience percentage</param>
        public void ShowVictoryScreen(int citizensProcessed, float finalObedience)
        {
            ShowVictoryScreenAsync(citizensProcessed, finalObedience).Forget();
        }

        /// <summary>
        /// Async implementation of showing victory screen with typewriter effect
        /// </summary>
        private async UniTaskVoid ShowVictoryScreenAsync(int citizensProcessed, float finalObedience)
        {
            if (_victoryPanel == null)
            {
                Debug.LogError("[VictoryScreen] No victory panel assigned!");
                return;
            }

            // Show the panel
            _victoryPanel.SetActive(true);

            // Set title based on obedience level
            if (_titleText != null)
            {
                string[] titles = GetTitlesForObedience(finalObedience);
                _titleText.text = titles[Random.Range(0, titles.Length)];
            }

            // Display obedience score
            if (_obedienceText != null)
            {
                string gradeText = GetObedienceGrade(finalObedience);

                _obedienceText.text = $"Final Compliance Score: {finalObedience:F1}%\n" +
                                      $"{gradeText}";
            }

            // Set propaganda message with typewriter effect based on obedience level
            string[] messages = GetMessagesForObedience(finalObedience);
            string selectedMessage = messages[Random.Range(0, messages.Length)];

            if (_messageTypewriter != null)
            {
                // Use typewriter effect
                await _messageTypewriter.PlayTypewriter(selectedMessage);
            }
            else if (_messageText != null)
            {
                // Fallback to instant display if no typewriter component
                _messageText.text = selectedMessage;
                Debug.LogWarning("[VictoryScreen] No TypewriterEffect component assigned, displaying message instantly.");
            }

            // If zero obedience, trigger execution sequence
            if (finalObedience <= 0f)
            {
                StartExecutionCountdown().Forget();
            }

            Debug.Log("[VictoryScreen] Victory screen displayed!");
        }

        /// <summary>
        /// Gets the appropriate title array based on obedience level
        /// </summary>
        private string[] GetTitlesForObedience(float obedience)
        {
            if (obedience <= 0f) return VICTORY_TITLES_ZERO;
            if (obedience < 40f) return VICTORY_TITLES_LOW;
            if (obedience < 80f) return VICTORY_TITLES_MEDIUM;
            return VICTORY_TITLES_HIGH;
        }

        /// <summary>
        /// Gets the appropriate message array based on obedience level
        /// </summary>
        private string[] GetMessagesForObedience(float obedience)
        {
            if (obedience <= 0f) return PROPAGANDA_MESSAGES_ZERO;
            if (obedience < 40f) return PROPAGANDA_MESSAGES_LOW;
            if (obedience < 80f) return PROPAGANDA_MESSAGES_MEDIUM;
            return PROPAGANDA_MESSAGES_HIGH;
        }

        /// <summary>
        /// Gets a grade text based on obedience level
        /// </summary>
        private string GetObedienceGrade(float obedience)
        {
            if (obedience >= 95f) return "PERFECT COMPLIANCE - S RANK";
            if (obedience >= 90f) return "EXEMPLARY SERVICE - A RANK";
            if (obedience >= 85f) return "COMMENDABLE PERFORMANCE - B RANK";
            if (obedience >= 80f) return "ACCEPTABLE RESULTS - C RANK";
            if (obedience >= 60f) return "BARELY ADEQUATE - D RANK";
            if (obedience >= 40f) return "SUBSTANDARD PERFORMANCE - E RANK";
            if (obedience > 0f) return "CRITICAL FAILURE - F RANK";
            return "COMPLETE DISASTER - Z RANK";
        }

        /// <summary>
        /// Starts the execution countdown for rebel spies (zero obedience)
        /// </summary>
        private async UniTaskVoid StartExecutionCountdown()
        {
            if (_executionCountdownText == null)
            {
                Debug.LogWarning("[VictoryScreen] No execution countdown text assigned!");
                return;
            }

            // Show countdown UI
            _executionCountdownText.gameObject.SetActive(true);

            // Start light flashing task
            var lightFlashTask = FlashLightDuringCountdown();

            // Countdown loop
            for (int i = _executionCountdownDuration; i > 0; i--)
            {
                _executionCountdownText.text = $"EXECUTION IN: {i}";
                await UniTask.Delay(1000); // Wait 1 second
            }

            // Timer hit zero - turn light on and leave it on
            if (GameManager.Instance.directionalLight != null)
            {
                GameManager.Instance.directionalLight.enabled = true;
            }

            // Final message
            _executionCountdownText.text = " ";
            executionDarkGameobject.SetActive(true);

            await UniTask.Delay(500);

            // Play gunshot sound
            if (_gunshotSound != null)
            {
                AudioSource.PlayClipAtPoint(_gunshotSound, Camera.main.transform.position, .5f);
                Debug.Log("[VictoryScreen] Execution sound played!");
            }
            else
            {
                Debug.LogWarning("[VictoryScreen] No gunshot sound assigned!");
            }

            // Wait before quitting
            await UniTask.Delay((int)(_postExecutionDelay * 1000));

            // Quit the game
            Debug.Log("[VictoryScreen] Player executed. Quitting game...");
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        /// <summary>
        /// Flashes the directional light during the execution countdown
        /// </summary>
        private async UniTaskVoid FlashLightDuringCountdown()
        {
            if (GameManager.Instance.directionalLight == null)
            {
                Debug.LogWarning("[VictoryScreen] No execution light assigned!");
                return;
            }

            bool lightState = false;
            float elapsedTime = 0f;
            float countdownDuration = _executionCountdownDuration;

            // Flash the light for the duration of the countdown
            while (elapsedTime < countdownDuration)
            {
                lightState = !lightState;
                GameManager.Instance.directionalLight.enabled = lightState;

                await UniTask.Delay(_lightFlashInterval);
                elapsedTime += _lightFlashInterval / 1000f;
            }
        }


        /// <summary>
        /// Hides the victory screen
        /// </summary>
        public void HideVictoryScreen()
        {
            if (_victoryPanel != null)
            {
                _victoryPanel.SetActive(false);
            }

            // Resume game
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Restart the game (call this from a button)
        /// </summary>
        public void OnRestartButton()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }

        /// <summary>
        /// Quit to main menu (call this from a button)
        /// </summary>
        public void OnMainMenuButton()
        {
            Time.timeScale = 1f;
            // Load your main menu scene here
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            Debug.Log("[VictoryScreen] Return to main menu requested");
        }

        /// <summary>
        /// Quit the application (call this from a button)
        /// </summary>
        public void OnQuitButton()
        {
            Debug.Log("[VictoryScreen] Quit requested");
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
