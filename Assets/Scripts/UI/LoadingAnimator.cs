using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// Animates loading text with animated dots using DOTween
/// </summary>
public class LoadingAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Animation Settings")]
    [SerializeField] private string baseText = "loading";
    [SerializeField] private int maxDots = 3;
    [SerializeField] private float dotInterval = 0.5f;
    [SerializeField] private bool animateOnEnable = true;

    private Sequence dotSequence;
    private int currentDotCount = 0;

    private void OnEnable()
    {
        if (animateOnEnable)
        {
            StartAnimation();
        }
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    /// <summary>
    /// Starts the loading dot animation
    /// </summary>
    public void StartAnimation()
    {
        // Stop any existing animation
        StopAnimation();

        // Ensure we have a text reference
        if (loadingText == null)
        {
            loadingText = GetComponent<TextMeshProUGUI>();
            if (loadingText == null)
            {
                Debug.LogError("LoadingAnimator: No TextMeshProUGUI component found!");
                return;
            }
        }

        // Initialize with base text
        currentDotCount = 0;
        UpdateLoadingText();

        // Create a sequence that loops forever
        dotSequence = DOTween.Sequence()
            .AppendCallback(() => IncrementDots())
            .AppendInterval(dotInterval)
            .AppendCallback(() => IncrementDots())
            .AppendInterval(dotInterval)
            .AppendCallback(() => IncrementDots())
            .AppendInterval(dotInterval)
            .AppendCallback(() => ResetDots())
            .SetLoops(-1); // Loop infinitely
    }

    /// <summary>
    /// Stops the loading dot animation
    /// </summary>
    public void StopAnimation()
    {
        if (dotSequence != null && dotSequence.IsActive())
        {
            dotSequence.Kill();
        }
    }

    /// <summary>
    /// Sets the base loading text (without dots)
    /// </summary>
    public void SetBaseText(string text)
    {
        baseText = text;
        UpdateLoadingText();
    }

    /// <summary>
    /// Increments the dot count and updates the text
    /// </summary>
    private void IncrementDots()
    {
        currentDotCount++;
        if (currentDotCount > maxDots)
        {
            currentDotCount = maxDots;
        }
        UpdateLoadingText();
    }

    /// <summary>
    /// Resets the dots to zero
    /// </summary>
    private void ResetDots()
    {
        currentDotCount = 0;
        UpdateLoadingText();
    }

    /// <summary>
    /// Updates the loading text with the current dot count
    /// </summary>
    private void UpdateLoadingText()
    {
        if (loadingText == null) return;

        string dots = new string('.', currentDotCount);
        loadingText.text = baseText + dots;
    }

    private void OnDestroy()
    {
        StopAnimation();
    }
}
