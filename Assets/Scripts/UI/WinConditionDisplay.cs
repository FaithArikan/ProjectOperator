using UnityEngine;
using NeuralWaveBureau.AI;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Displays win condition, progress, and helpful tips during gameplay
    /// Shows you exactly what you need to do to win
    /// </summary>
    public class WinConditionDisplay : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private bool _showDisplay = true;

        [SerializeField]
        private bool _showDetailedInfo = true;

        private BrainActivityMonitor _monitor;
        private AIManager _aiManager;
        private float _stimulationStartTime = 0f;
        private bool _hasWon = false;
        private bool _hasLost = false;

        private void Start()
        {
            _monitor = FindFirstObjectByType<BrainActivityMonitor>();
            _aiManager = AIManager.Instance;
        }

        private void OnGUI()
        {
            if (!_showDisplay || _aiManager == null)
                return;

            var citizen = _aiManager.ActiveCitizen;
            if (citizen == null)
                return;

            // Panel setup
            float panelX = Screen.width - 420;
            float panelY = 10;
            float panelWidth = 410;
            float panelHeight = _showDetailedInfo ? 280 : 160;

            // Background
            GUI.Box(new Rect(panelX - 5, panelY - 5, panelWidth + 10, panelHeight + 10), "");

            // Styles
            GUIStyle headerStyle = new GUIStyle();
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.yellow;

            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontSize = 13;
            labelStyle.normal.textColor = Color.white;

            GUIStyle valueStyle = new GUIStyle(labelStyle);
            valueStyle.fontStyle = FontStyle.Bold;

            float y = panelY;

            // Header
            GUI.Label(new Rect(panelX, y, panelWidth, 22), "=== MISSION OBJECTIVE ===", headerStyle);
            y += 28;

            // Win condition
            GUIStyle winStyle = new GUIStyle(labelStyle);
            winStyle.fontSize = 14;
            winStyle.normal.textColor = Color.green;
            GUI.Label(new Rect(panelX, y, panelWidth, 20), "🎯 GOAL: Reach STABILIZED state", winStyle);
            y += 25;

            // Current state
            var state = citizen.CurrentState;
            Color stateColor = GetStateColor(state);
            GUIStyle stateStyle = new GUIStyle(valueStyle);
            stateStyle.fontSize = 15;
            stateStyle.normal.textColor = stateColor;

            GUI.Label(new Rect(panelX, y, 150, 20), "Current State:", labelStyle);
            GUI.Label(new Rect(panelX + 150, y, 250, 20), state.ToString().ToUpper(), stateStyle);
            y += 25;

            // Check win/lose
            if (state == CitizenState.Stabilized && !_hasWon)
            {
                _hasWon = true;
                Debug.Log("=== 🎉 YOU WIN! CITIZEN STABILIZED! 🎉 ===");
            }
            else if (state == CitizenState.Critical_Failure && !_hasLost)
            {
                _hasLost = true;
                Debug.Log("=== 💥 GAME OVER! CRITICAL FAILURE! 💥 ===");
            }

            // Progress metrics
            DrawProgressBar(panelX, y, panelWidth, "Evaluation Score", citizen.EvaluationScore, _aiManager.Settings.successThreshold, Color.cyan, Color.green);
            y += 30;

            DrawProgressBar(panelX, y, panelWidth, "Instability", citizen.Instability, _aiManager.Settings.instabilityFailThreshold, Color.yellow, Color.red);
            y += 30;

            // Status message
            y += 5;
            GUIStyle statusStyle = new GUIStyle(labelStyle);
            statusStyle.fontSize = 12;
            statusStyle.fontStyle = FontStyle.Italic;

            string statusMessage = GetStatusMessage(citizen);
            Color statusColor = GetStatusColor(citizen);
            statusStyle.normal.textColor = statusColor;

            GUI.Label(new Rect(panelX, y, panelWidth, 40), statusMessage, statusStyle);
            y += 45;

            // Detailed info
            if (_showDetailedInfo)
            {
                GUIStyle infoStyle = new GUIStyle(labelStyle);
                infoStyle.fontSize = 11;
                infoStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

                GUI.Label(new Rect(panelX, y, panelWidth, 16), $"Win Threshold: ≥ {_aiManager.Settings.successThreshold:F2}", infoStyle);
                y += 16;
                GUI.Label(new Rect(panelX, y, panelWidth, 16), $"Fail Threshold: ≥ {_aiManager.Settings.instabilityFailThreshold:F2} instability", infoStyle);
                y += 16;
                GUI.Label(new Rect(panelX, y, panelWidth, 16), $"Citizen: {citizen.Profile.displayName}", infoStyle);
            }

            // Win/Lose overlay
            if (_hasWon)
            {
                DrawWinScreen();
            }
            else if (_hasLost)
            {
                DrawLoseScreen();
            }
        }

        private void DrawProgressBar(float x, float y, float width, string label, float value, float threshold, Color normalColor, Color thresholdColor)
        {
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontSize = 12;
            labelStyle.normal.textColor = Color.white;

            // Label
            GUI.Label(new Rect(x, y, 150, 20), label + ":", labelStyle);

            // Bar background
            float barX = x + 150;
            float barWidth = width - 200;
            float barHeight = 16;
            GUI.Box(new Rect(barX, y + 2, barWidth, barHeight), "");

            // Bar fill
            float fillWidth = Mathf.Clamp01(value) * barWidth;
            Color barColor = value >= threshold ? thresholdColor : normalColor;

            Texture2D barTexture = new Texture2D(1, 1);
            barTexture.SetPixel(0, 0, barColor);
            barTexture.Apply();

            GUI.DrawTexture(new Rect(barX + 1, y + 3, fillWidth - 2, barHeight - 2), barTexture);

            // Threshold line
            float thresholdX = barX + (threshold * barWidth);
            Texture2D lineTexture = new Texture2D(1, 1);
            lineTexture.SetPixel(0, 0, Color.yellow);
            lineTexture.Apply();
            GUI.DrawTexture(new Rect(thresholdX, y + 2, 2, barHeight), lineTexture);

            // Value text
            GUIStyle valueStyle = new GUIStyle(labelStyle);
            valueStyle.alignment = TextAnchor.MiddleRight;
            valueStyle.fontStyle = FontStyle.Bold;
            valueStyle.normal.textColor = barColor;
            GUI.Label(new Rect(barX + barWidth + 5, y, 40, 20), $"{value:F2}", valueStyle);
        }

        private string GetStatusMessage(CitizenController citizen)
        {
            var state = citizen.CurrentState;
            float score = citizen.EvaluationScore;
            float instability = citizen.Instability;
            float threshold = _aiManager.Settings.successThreshold;

            switch (state)
            {
                case CitizenState.Idle:
                    return "Press F1 or Start Monitoring to begin";

                case CitizenState.Stimulated:
                    if (score >= threshold)
                        return "✓ Good! Maintain this score to win!";
                    else if (score >= 0.5f)
                        return "↑ Getting closer! Keep adjusting!";
                    else if (score >= 0.25f)
                        return "⚠ Low score - adjust waves to match targets!";
                    else
                        return "⚠⚠ DANGER! Score too low, instability rising!";

                case CitizenState.Stabilized:
                    return "🎉 SUCCESS! Citizen stabilized!";

                case CitizenState.Agitated:
                    return $"⚠⚠ AGITATED! Improve score NOW! ({instability:F2}/0.8 instability)";

                case CitizenState.Critical_Failure:
                    return "💥 CRITICAL FAILURE! Try again?";

                case CitizenState.Recovering:
                    return "↻ Recovering... Give them time to stabilize";

                default:
                    return "";
            }
        }

        private Color GetStatusColor(CitizenController citizen)
        {
            var state = citizen.CurrentState;
            float score = citizen.EvaluationScore;

            switch (state)
            {
                case CitizenState.Idle:
                    return Color.gray;
                case CitizenState.Stimulated:
                    return score >= _aiManager.Settings.successThreshold ? Color.green : (score >= 0.5f ? Color.yellow : Color.red);
                case CitizenState.Stabilized:
                    return Color.green;
                case CitizenState.Agitated:
                    return Color.red;
                case CitizenState.Critical_Failure:
                    return Color.red;
                case CitizenState.Recovering:
                    return Color.cyan;
                default:
                    return Color.white;
            }
        }

        private Color GetStateColor(CitizenState state)
        {
            switch (state)
            {
                case CitizenState.Idle: return Color.gray;
                case CitizenState.Stimulated: return Color.cyan;
                case CitizenState.Stabilized: return Color.green;
                case CitizenState.Agitated: return Color.yellow;
                case CitizenState.Critical_Failure: return Color.red;
                case CitizenState.Recovering: return new Color(0.5f, 1f, 1f);
                default: return Color.white;
            }
        }

        private void DrawWinScreen()
        {
            float screenCenterX = Screen.width / 2;
            float screenCenterY = Screen.height / 2;
            float boxWidth = 500;
            float boxHeight = 200;

            // Semi-transparent overlay
            Texture2D overlayTexture = new Texture2D(1, 1);
            overlayTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
            overlayTexture.Apply();
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture);

            // Win box
            GUI.Box(new Rect(screenCenterX - boxWidth / 2, screenCenterY - boxHeight / 2, boxWidth, boxHeight), "");

            GUIStyle winStyle = new GUIStyle();
            winStyle.fontSize = 32;
            winStyle.fontStyle = FontStyle.Bold;
            winStyle.normal.textColor = Color.green;
            winStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(screenCenterX - boxWidth / 2, screenCenterY - 60, boxWidth, 50), "🎉 MISSION SUCCESS! 🎉", winStyle);

            GUIStyle subStyle = new GUIStyle(winStyle);
            subStyle.fontSize = 18;
            subStyle.normal.textColor = Color.white;

            GUI.Label(new Rect(screenCenterX - boxWidth / 2, screenCenterY, boxWidth, 40), "Citizen Successfully Stabilized", subStyle);

            GUIStyle tipStyle = new GUIStyle(winStyle);
            tipStyle.fontSize = 14;
            tipStyle.normal.textColor = Color.gray;
            tipStyle.fontStyle = FontStyle.Italic;

            GUI.Label(new Rect(screenCenterX - boxWidth / 2, screenCenterY + 50, boxWidth, 30), "Press F5 to reset and try again", tipStyle);
        }

        private void DrawLoseScreen()
        {
            float screenCenterX = Screen.width / 2;
            float screenCenterY = Screen.height / 2;
            float boxWidth = 500;
            float boxHeight = 220;

            // Semi-transparent overlay
            Texture2D overlayTexture = new Texture2D(1, 1);
            overlayTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
            overlayTexture.Apply();
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture);

            // Lose box
            GUI.Box(new Rect(screenCenterX - boxWidth / 2, screenCenterY - boxHeight / 2, boxWidth, boxHeight), "");

            GUIStyle loseStyle = new GUIStyle();
            loseStyle.fontSize = 32;
            loseStyle.fontStyle = FontStyle.Bold;
            loseStyle.normal.textColor = Color.red;
            loseStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(screenCenterX - boxWidth / 2, screenCenterY - 70, boxWidth, 50), "💥 CRITICAL FAILURE 💥", loseStyle);

            GUIStyle subStyle = new GUIStyle(loseStyle);
            subStyle.fontSize = 18;
            subStyle.normal.textColor = Color.white;

            GUI.Label(new Rect(screenCenterX - boxWidth / 2, screenCenterY - 10, boxWidth, 40), "Instability Threshold Exceeded", subStyle);

            GUIStyle tipStyle = new GUIStyle(loseStyle);
            tipStyle.fontSize = 14;
            tipStyle.normal.textColor = Color.gray;
            tipStyle.fontStyle = FontStyle.Italic;

            GUI.Label(new Rect(screenCenterX - boxWidth / 2, screenCenterY + 40, boxWidth, 30), "TIP: Keep Evaluation Score above 0.75", tipStyle);
            GUI.Label(new Rect(screenCenterX - boxWidth / 2, screenCenterY + 65, boxWidth, 30), "Press F5 to reset and try again", tipStyle);
        }
    }
}
