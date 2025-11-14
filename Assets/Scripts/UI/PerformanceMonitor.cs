using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.InputSystem;
using System.Text;

public class PerformanceMonitor : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private bool showInBuild = true;
    [SerializeField] private bool showInEditor = true;
    [SerializeField] private Key toggleKey = Key.F3;

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.5f;

    [Header("UI Settings")]
    [SerializeField] private int fontSize = 14;
    [SerializeField] private Color goodColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color badColor = Color.red;
    [SerializeField] private Vector2 position = new Vector2(10, 10);

    private bool isVisible = true;
    private float deltaTime = 0.0f;
    private float updateTimer = 0.0f;

    // Performance metrics
    private float currentFPS = 0.0f;
    private float avgFPS = 0.0f;
    private float minFPS = float.MaxValue;
    private float maxFPS = 0.0f;

    // Memory metrics
    private long totalMemory = 0;
    private long usedMemory = 0;
    private long monoMemory = 0;

    // Graphics metrics
    private long totalVRAM = 0;
    private long usedVRAM = 0;

    private GUIStyle textStyle;
    private StringBuilder sb = new StringBuilder();
    private int frameCount = 0;
    private float fpsAccumulator = 0.0f;

    private void Start()
    {
        // Check if we should show the monitor
        #if UNITY_EDITOR
            isVisible = showInEditor;
        #else
            isVisible = showInBuild;
        #endif

        // Initialize metrics
        UpdateMetrics();
    }

    private void Update()
    {
        // Toggle visibility
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            isVisible = !isVisible;
        }

        if (!isVisible) return;

        // Calculate delta time
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // Update timer
        updateTimer += Time.unscaledDeltaTime;

        // Calculate FPS
        float fps = 1.0f / deltaTime;
        currentFPS = fps;

        // Track min/max FPS
        if (fps < minFPS) minFPS = fps;
        if (fps > maxFPS) maxFPS = fps;

        // Calculate average FPS
        frameCount++;
        fpsAccumulator += fps;

        // Update metrics at interval
        if (updateTimer >= updateInterval)
        {
            UpdateMetrics();
            updateTimer = 0.0f;
        }
    }

    private void UpdateMetrics()
    {
        // Memory metrics
        totalMemory = Profiler.GetTotalReservedMemoryLong() / (1024 * 1024); // Convert to MB
        usedMemory = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024); // Convert to MB
        monoMemory = Profiler.GetMonoUsedSizeLong() / (1024 * 1024); // Convert to MB

        // Graphics memory
        totalVRAM = Profiler.GetAllocatedMemoryForGraphicsDriver() / (1024 * 1024); // Convert to MB

        // Calculate average FPS
        if (frameCount > 0)
        {
            avgFPS = fpsAccumulator / frameCount;
        }
    }

    private void OnGUI()
    {
        if (!isVisible) return;

        // Initialize text style
        if (textStyle == null)
        {
            textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = fontSize;
            textStyle.normal.textColor = Color.white;
            textStyle.alignment = TextAnchor.UpperLeft;
        }

        // Build display string
        sb.Clear();
        sb.AppendLine("=== PERFORMANCE MONITOR ===");
        sb.AppendLine();

        // FPS Section
        sb.AppendLine("--- FRAME RATE ---");
        sb.Append("FPS: ");
        sb.Append(GetColoredText(currentFPS.ToString("F1"), GetFPSColor(currentFPS)));
        sb.AppendLine();
        sb.AppendLine($"Avg FPS: {avgFPS:F1}");
        sb.AppendLine($"Min FPS: {minFPS:F1}");
        sb.AppendLine($"Max FPS: {maxFPS:F1}");
        sb.AppendLine($"Frame Time: {(deltaTime * 1000.0f):F2} ms");
        sb.AppendLine();

        // Memory Section
        sb.AppendLine("--- MEMORY ---");
        sb.AppendLine($"Total Reserved: {totalMemory} MB");
        sb.AppendLine($"Total Used: {usedMemory} MB");
        sb.AppendLine($"Mono Memory: {monoMemory} MB");
        sb.AppendLine();

        // Graphics Section
        sb.AppendLine("--- GRAPHICS ---");
        sb.AppendLine($"Graphics Driver: {totalVRAM} MB");
        sb.AppendLine($"GPU: {SystemInfo.graphicsDeviceName}");
        sb.AppendLine($"VRAM: {SystemInfo.graphicsMemorySize} MB");
        sb.AppendLine($"Shader Level: {SystemInfo.graphicsShaderLevel}");
        sb.AppendLine();

        // System Info
        sb.AppendLine("--- SYSTEM ---");
        sb.AppendLine($"CPU: {SystemInfo.processorType}");
        sb.AppendLine($"CPU Cores: {SystemInfo.processorCount}");
        sb.AppendLine($"System RAM: {SystemInfo.systemMemorySize} MB");
        sb.AppendLine();

        // Rendering Stats
        sb.AppendLine("--- RENDERING ---");
        sb.AppendLine($"Screen: {Screen.width}x{Screen.height} @ {Screen.currentResolution.refreshRate}Hz");
        sb.AppendLine($"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        sb.AppendLine($"VSync: {QualitySettings.vSyncCount}");
        sb.AppendLine();

        sb.AppendLine($"Press {toggleKey} to toggle");

        // Calculate background size
        GUIContent content = new GUIContent(sb.ToString());
        Vector2 size = textStyle.CalcSize(content);

        // Draw background
        Rect bgRect = new Rect(position.x - 5, position.y - 5, size.x + 10, size.y + 10);
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(bgRect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Draw text
        GUI.Label(new Rect(position.x, position.y, size.x, size.y), sb.ToString(), textStyle);
    }

    private Color GetFPSColor(float fps)
    {
        if (fps >= 60) return goodColor;
        if (fps >= 30) return warningColor;
        return badColor;
    }

    private string GetColoredText(string text, Color color)
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(color);
        return $"<color=#{hexColor}>{text}</color>";
    }

    public void ResetStats()
    {
        minFPS = float.MaxValue;
        maxFPS = 0.0f;
        frameCount = 0;
        fpsAccumulator = 0.0f;
    }

    // Public methods to control visibility
    public void Show() => isVisible = true;
    public void Hide() => isVisible = false;
    public void Toggle() => isVisible = !isVisible;

    // Public getters for other scripts
    public float GetCurrentFPS() => currentFPS;
    public float GetAverageFPS() => avgFPS;
    public long GetUsedMemory() => usedMemory;
    public long GetUsedVRAM() => totalVRAM;
}
