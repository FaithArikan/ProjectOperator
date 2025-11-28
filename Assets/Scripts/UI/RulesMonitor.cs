using UnityEngine;
using TMPro;
using DG.Tweening;
using NeuralWaveBureau.UI;
using Cysharp.Threading.Tasks;
using System;

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

    [Header("Rules Content")]
    [TextArea(10, 20)]
    private string _rulesText = @"OPERATION PROTOCOL

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
    [SerializeField] private TypewriterEffect _typewriterEffect;
    [SerializeField] private float _powerOnDuration = 1.5f;

    private bool _isPoweredOn = false;

    private void Awake()
    {
        // Initialize in powered-off state
        if (_rulesScreenPanel != null)
        {
            _rulesScreenPanel.SetActive(false);
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

        // Start displaying rules with typewriter effect
        PowerOnSequenceAsync().Forget();
    }

    /// <summary>
    /// Powers off the rules monitor and returns to room view.
    /// </summary>
    public void PowerOff()
    {
        if (!_isPoweredOn) return;

        Debug.Log("RulesMonitor: Powering off...");

        // Stop any ongoing typewriter effect
        if (_typewriterEffect != null)
        {
            _typewriterEffect.StopTypewriter();
        }

        // Deactivate panel after a delay
        DelayedDeactivateAsync().Forget();

        _isPoweredOn = false;
    }

    /// <summary>
    /// Sequence of events when powering on the monitor.
    /// </summary>
    private async UniTaskVoid PowerOnSequenceAsync()
    {
        try
        {
            // Wait for camera transition
            await UniTask.Delay(TimeSpan.FromSeconds(_powerOnDuration));

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

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            // Display rules content with typewriter effect
            if (_typewriterEffect != null)
            {
                await _typewriterEffect.PlayTypewriter(_rulesText);
            }
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation if needed
        }
    }

    /// <summary>
    /// Deactivates the panel after a short delay (for animations to complete).
    /// </summary>
    private async UniTaskVoid DelayedDeactivateAsync()
    {
        // Fade out
        if (_panelCanvasGroup != null)
        {
            _panelCanvasGroup.DOFade(0f, 0.5f);
        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

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
