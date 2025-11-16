using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using NeuralWaveBureau.UI;
using NeuralWaveBureau;

/// <summary>
/// Manages the rules/instructions monitor that displays game controls and objectives
/// before the player accesses the main brain activity monitor.
/// </summary>
public class RulesMonitor : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _rulesScreenPanel;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _rulesContentText;
    [SerializeField] private GameObject _continueButton;
    [SerializeField] private CanvasGroup _panelCanvasGroup;

    [Header("Screen Effects")]
    [SerializeField] private CRTScreenEffect _crtEffect;

    [Header("Rules Content")]
    [TextArea(10, 20)]
    [SerializeField] private string _rulesText = @"OPERATION PROTOCOL

OBJECTIVE:
Monitor and control citizen brain activity to maintain social order.

CONTROLS:
- F1: Auto-Setup System
- F2: Power On Monitor
- F3: Start Monitoring
- F4: Stop Monitoring
- F5: Reset System

MONITORING GUIDELINES:
1. Maintain citizen obedience levels
2. Apply stimulation as needed
3. Track instability indicators
4. Prevent cognitive rebellion

REMEMBER: Order through observation.

Click CONTINUE to proceed to your station.";

    [Header("Animation Settings")]
    [SerializeField] private float _typewriterSpeed = 0.03f;
    [SerializeField] private float _powerOnDuration = 1.5f;

    private bool _isPoweredOn = false;
    private Coroutine _typewriterCoroutine;

    private void Awake()
    {
        // Initialize in powered-off state
        if (_rulesScreenPanel != null)
        {
            _rulesScreenPanel.SetActive(false);
        }

        if (_continueButton != null)
        {
            _continueButton.SetActive(false);
        }

        // Set initial text
        if (_rulesContentText != null)
        {
            _rulesContentText.text = "";
        }

        if (_titleText != null)
        {
            _titleText.text = "INSTRUCTIONS";
        }
    }

    /// <summary>
    /// Powers on the rules monitor and displays the instructions.
    /// </summary>
    public void PowerOn()
    {
        if (_isPoweredOn) return;

        Debug.Log("RulesMonitor: Powering on...");
        _isPoweredOn = true;

        // Activate the panel
        if (_rulesScreenPanel != null)
        {
            _rulesScreenPanel.SetActive(true);
        }

        // Play CRT power-on effect
        if (_crtEffect != null)
        {
            _crtEffect.PowerOn();
        }

        // Start displaying rules with typewriter effect
        StartCoroutine(PowerOnSequence());
    }

    /// <summary>
    /// Powers off the rules monitor and returns to room view.
    /// </summary>
    public void PowerOff()
    {
        if (!_isPoweredOn) return;

        Debug.Log("RulesMonitor: Powering off...");

        // Stop any ongoing typewriter effect
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }

        // Play CRT power-off effect
        if (_crtEffect != null)
        {
            _crtEffect.PowerOff();
        }

        // Deactivate panel after a delay
        StartCoroutine(DelayedDeactivate());

        _isPoweredOn = false;
    }

    /// <summary>
    /// Sequence of events when powering on the monitor.
    /// </summary>
    private IEnumerator PowerOnSequence()
    {
        // Wait for camera transition and CRT effect
        yield return new WaitForSeconds(_powerOnDuration);

        // Fade in the panel
        if (_panelCanvasGroup != null)
        {
            _panelCanvasGroup.alpha = 0f;
            _panelCanvasGroup.DOFade(1f, 0.5f);
        }

        // Display title with fade effect
        if (_titleText != null)
        {
            _titleText.alpha = 0f;
            _titleText.DOFade(1f, 0.5f);
        }

        yield return new WaitForSeconds(0.5f);

        // Display rules content with typewriter effect
        if (_rulesContentText != null)
        {
            _typewriterCoroutine = StartCoroutine(TypewriterEffect(_rulesContentText, _rulesText));
        }
    }

    /// <summary>
    /// Displays text character by character for a retro terminal feel.
    /// </summary>
    private IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string fullText)
    {
        textComponent.text = "";

        foreach (char c in fullText)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(_typewriterSpeed);
        }

        // Show continue button after text is fully displayed
        yield return new WaitForSeconds(0.5f);

        if (_continueButton != null)
        {
            _continueButton.SetActive(true);
            CanvasGroup buttonCanvasGroup = _continueButton.GetComponent<CanvasGroup>();
            if (buttonCanvasGroup != null)
            {
                buttonCanvasGroup.alpha = 0f;
                buttonCanvasGroup.DOFade(1f, 0.5f);
            }
        }

        _typewriterCoroutine = null;
    }

    /// <summary>
    /// Deactivates the panel after a short delay (for animations to complete).
    /// </summary>
    private IEnumerator DelayedDeactivate()
    {
        // Fade out
        if (_panelCanvasGroup != null)
        {
            _panelCanvasGroup.DOFade(0f, 0.5f);
        }

        yield return new WaitForSeconds(0.5f);

        if (_rulesScreenPanel != null)
        {
            _rulesScreenPanel.SetActive(false);
        }

        // Reset content
        if (_rulesContentText != null)
        {
            _rulesContentText.text = "";
        }
    }

    /// <summary>
    /// Called when the Continue button is clicked.
    /// </summary>
    public void OnContinueClicked()
    {
        Debug.Log("RulesMonitor: Continue clicked");
        PowerOff();
    }

    /// <summary>
    /// Manually trigger the rules display (for testing).
    /// </summary>
    [ContextMenu("Test Power On")]
    public void TestPowerOn()
    {
        PowerOn();
    }

    /// <summary>
    /// Manually power off (for testing).
    /// </summary>
    [ContextMenu("Test Power Off")]
    public void TestPowerOff()
    {
        PowerOff();
    }

    public bool IsPoweredOn => _isPoweredOn;
}
