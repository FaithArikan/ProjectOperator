using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Helper script to automatically arrange waveforms in a vertical layout
    /// Attach to the WaveformPanel GameObject
    /// </summary>
    [ExecuteInEditMode]
    public class WaveformLayoutHelper : MonoBehaviour
    {
        [Header("Layout Settings")]
        [SerializeField]
        [Tooltip("Space between each waveform row")]
        private float _rowSpacing = 10f;

        [SerializeField]
        [Tooltip("Height of each waveform row")]
        private float _rowHeight = 80f;

        [SerializeField]
        [Tooltip("Left/right padding")]
        private float _horizontalPadding = 20f;

        [SerializeField]
        [Tooltip("Top padding")]
        private float _topPadding = 10f;

        [SerializeField]
        [Tooltip("Show band labels?")]
        private bool _showLabels = true;

        [SerializeField]
        [Tooltip("Label width (if showing labels)")]
        private float _labelWidth = 100f;

        [Header("Waveforms (Assign in Order)")]
        [SerializeField]
        private List<WaveformDisplay> _waveforms = new List<WaveformDisplay>();

        [Header("Auto-Arrange")]
        [SerializeField]
        [Tooltip("Automatically arrange on value change")]
        private bool _autoArrange = true;

        private void OnValidate()
        {
            if (_autoArrange)
            {
                ArrangeWaveforms();
            }
        }

        [ContextMenu("Arrange Waveforms Vertically")]
        public void ArrangeWaveforms()
        {
            if (_waveforms == null || _waveforms.Count == 0)
            {
                Debug.LogWarning("[WaveformLayoutHelper] No waveforms assigned!");
                return;
            }

            RectTransform panelRect = GetComponent<RectTransform>();
            if (panelRect == null)
            {
                Debug.LogError("[WaveformLayoutHelper] This component must be on a RectTransform!");
                return;
            }

            string[] bandNames = { "DELTA", "THETA", "ALPHA", "BETA", "GAMMA" };
            Color[] bandColors = new Color[]
            {
                new Color(1f, 0.2f, 0.2f),      // Delta - Red
                new Color(1f, 0.6f, 0.2f),      // Theta - Orange
                new Color(0.2f, 1f, 0.2f),      // Alpha - Green
                new Color(0.2f, 0.6f, 1f),      // Beta - Blue
                new Color(0.8f, 0.2f, 1f)       // Gamma - Purple
            };

            float currentY = -_topPadding; // Start from top

            for (int i = 0; i < _waveforms.Count; i++)
            {
                if (_waveforms[i] == null)
                    continue;

                RectTransform waveRect = _waveforms[i].GetComponent<RectTransform>();
                if (waveRect == null)
                    continue;

                // Position and size
                float xPos = _horizontalPadding + (_showLabels ? _labelWidth : 0);
                float width = panelRect.rect.width - (2 * _horizontalPadding) - (_showLabels ? _labelWidth : 0);

                waveRect.anchorMin = new Vector2(0, 1); // Top-left anchor
                waveRect.anchorMax = new Vector2(0, 1);
                waveRect.pivot = new Vector2(0, 1);

                waveRect.anchoredPosition = new Vector2(xPos, currentY);
                waveRect.sizeDelta = new Vector2(width, _rowHeight);

                // Set band index and color
                _waveforms[i].BandIndex = i;
                _waveforms[i].WaveColor = bandColors[i];

                // Create/update label if enabled
                if (_showLabels)
                {
                    CreateOrUpdateLabel(waveRect, i, bandNames[i], bandColors[i]);
                }

                currentY -= (_rowHeight + _rowSpacing);
            }

            // Update panel size to fit all waveforms
            float totalHeight = _topPadding + (_waveforms.Count * (_rowHeight + _rowSpacing));
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, totalHeight);

            Debug.Log($"[WaveformLayoutHelper] Arranged {_waveforms.Count} waveforms vertically");
        }

        private void CreateOrUpdateLabel(RectTransform waveformRect, int index, string bandName, Color color)
        {
            // Look for existing label
            Transform labelTransform = waveformRect.Find("BandLabel");
            GameObject labelObj;

            if (labelTransform == null)
            {
                // Create new label
                labelObj = new GameObject("BandLabel", typeof(RectTransform));
                labelObj.transform.SetParent(waveformRect.parent, false);
            }
            else
            {
                labelObj = labelTransform.gameObject;
            }

            // Position label to the left of waveform
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(0, 1);
            labelRect.pivot = new Vector2(0, 1);
            labelRect.anchoredPosition = new Vector2(_horizontalPadding, waveformRect.anchoredPosition.y);
            labelRect.sizeDelta = new Vector2(_labelWidth - 10, _rowHeight);

            // Add/update text component
            TMPro.TextMeshProUGUI text = labelObj.GetComponent<TMPro.TextMeshProUGUI>();
            if (text == null)
            {
                text = labelObj.AddComponent<TMPro.TextMeshProUGUI>();
            }

            text.text = $"{index + 1}. {bandName}";
            text.fontSize = 14;
            text.fontStyle = TMPro.FontStyles.Bold;
            text.color = color;
            text.alignment = TMPro.TextAlignmentOptions.Left;
            text.verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;
        }

        [ContextMenu("Auto-Find Waveforms")]
        public void AutoFindWaveforms()
        {
            _waveforms.Clear();
            WaveformDisplay[] found = GetComponentsInChildren<WaveformDisplay>();

            // Sort by band index
            System.Array.Sort(found, (a, b) => a.BandIndex.CompareTo(b.BandIndex));

            _waveforms.AddRange(found);

            Debug.Log($"[WaveformLayoutHelper] Found {_waveforms.Count} waveforms");

            if (_autoArrange)
            {
                ArrangeWaveforms();
            }
        }

        // Quick layout presets
        [ContextMenu("Preset: Compact")]
        public void PresetCompact()
        {
            _rowHeight = 60f;
            _rowSpacing = 5f;
            _horizontalPadding = 10f;
            ArrangeWaveforms();
        }

        [ContextMenu("Preset: Comfortable")]
        public void PresetComfortable()
        {
            _rowHeight = 80f;
            _rowSpacing = 10f;
            _horizontalPadding = 20f;
            ArrangeWaveforms();
        }

        [ContextMenu("Preset: Spacious")]
        public void PresetSpacious()
        {
            _rowHeight = 100f;
            _rowSpacing = 15f;
            _horizontalPadding = 30f;
            ArrangeWaveforms();
        }
    }
}
