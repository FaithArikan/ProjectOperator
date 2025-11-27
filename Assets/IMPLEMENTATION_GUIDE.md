# Neural Wave Bureau - Implementation Guide

This guide explains how to use the AI systems that have been implemented according to ARCHITECTURE.md.

## Overview

The AI system for Neural Wave Bureau has been fully implemented with the following components:

1. **Data Models** (ScriptableObjects for tuning)
   - `NeuralProfile` - Defines citizen wave targets and behavior
   - `AISettings` - Global configuration for AI systems

2. **Core AI Systems**
   - `WaveEvaluator` - Computes similarity between player output and target waves
   - `EmotionStateMachine` - FSM managing citizen emotional states
   - `CitizenController` - Per-citizen behavior controller
   - `AIManager` - Central coordinator (singleton)
   - `FeedbackManager` - Audio/visual feedback system

3. **Debug Tools**
   - `WaveVisualizer` - On-screen wave visualization
   - `WaveInputSimulator` - Keyboard-based wave input for testing

## Quick Start

### Step 1: Create Sample Profiles

1. In Unity Editor, go to menu: **Neural Wave Bureau → Create Sample Profiles**
2. This will create:
   - `Assets/Data/Profiles/Ordinary.asset`
   - `Assets/Data/Profiles/Artist.asset`
   - `Assets/Data/Profiles/Rebel.asset`
   - `Assets/Data/DefaultAISettings.asset`

### Step 2: Set Up the Scene

1. Create an empty GameObject named "AIManager"
2. Add these components:
   - `AIManager` script
   - `FeedbackManager` script
   - `WaveVisualizer` script
   - `WaveInputSimulator` script (for testing)
3. Assign the `DefaultAISettings` asset to the AIManager's settings field

### Step 3: Create a Citizen

1. Create a GameObject named "Citizen"
2. Add an `Animator` component (required)
3. Add the `CitizenController` script
4. Assign a `NeuralProfile` asset (e.g., Ordinary)
5. Set the Citizen ID (e.g., "citizen_01")

### Step 4: Configure FeedbackManager

On the AIManager GameObject:
1. Create a Light GameObject and assign it to FeedbackManager's "Ambient Light"
2. Add AudioSource components for tone and ambient sounds
3. (Optional) Add ParticleSystem GameObjects for visual effects

### Step 5: Test the System

1. Enter Play mode
2. Use keyboard controls to simulate wave input:
   - **1-5**: Select brain wave band (Delta, Theta, Alpha, Beta, Gamma)
   - **↑/↓**: Increase/decrease selected band value
   - **Q**: Load "Ordinary" preset
   - **W**: Load "Artist" preset
   - **E**: Load "Rebel" preset
   - **R**: Random values

3. In the AIManager inspector, click "Start Stimulation" on a citizen (or call via code)

4. Watch the WaveVisualizer to see:
   - Green bars = Target wave values
   - Cyan bars = Current wave values
   - White lines = Tolerance range
   - Evaluation score
   - Instability meter
   - Current state

## System Architecture

### Data Flow

```
Player Input → WaveInputSimulator → AIManager.SetWaveSample()
                                          ↓
                            CitizenController.UpdateWaveSample()
                                          ↓
                            WaveEvaluator.Evaluate() ← NeuralProfile
                                          ↓
                            EmotionStateMachine.Update()
                                          ↓
                      ┌─────────────────────────────────┐
                      ↓                                 ↓
              Animator Updates                  FeedbackManager
              (agitation, composure)            (lights, audio, VFX)
```

### State Machine

Citizens transition through these states:

- **Idle** → BeingStimulated (when wave input starts)
- **BeingStimulated** → Stabilized (score ≥ successThreshold for minStimTime)
- **BeingStimulated** → Agitated (score ≤ overloadThreshold)
- **Agitated** → CriticalFailure (instability ≥ failThreshold)
- **Agitated** → Recovering (score improves)
- **Recovering** → Idle or Stabilized (after recoveryTime)

### Wave Evaluation Algorithm

For each brain wave band:
1. Compute normalized difference: `d[i] = 1 - abs(sample[i] - target[i]) / tolerance[i]`
2. Apply weights: `score = Sum(w[i] * d[i]) / Sum(w[i])`
3. Smooth with exponential moving average: `ema = alpha * score + (1-alpha) * ema`

### Instability System

- Increases when score ≤ overloadThreshold
- Decreases when score ≥ successThreshold
- Triggers CriticalFailure when ≥ instabilityFailThreshold

## Tuning Guide

### NeuralProfile Parameters

- **bandTargets**: Target values for each band (0..1). Match these for success.
- **bandTolerance**: Allowed variance. Lower = harder.
- **bandWeights**: Relative importance of each band. Default: all equal.
- **baselineInstability**: Starting instability (0..1)
- **instabilityRate**: How fast instability builds (0..2)
- **minStimulationTime**: Required time for stabilization (seconds)
- **recoveryTime**: Time to recover from agitation (seconds)

### AISettings Parameters

- **successThreshold**: Score needed for success (default: 0.75)
- **overloadThreshold**: Score that triggers instability (default: 0.25)
- **instabilityFailThreshold**: Instability level for critical failure (default: 0.8)
- **sampleRate**: Evaluation frequency in Hz (default: 30)
- **smoothingTau**: Time constant for smoothing (default: 0.3s)

## Example Profiles

### Ordinary Citizen
```
Band Targets: [0.1, 0.2, 0.6, 0.6, 0.2]
Band Tolerance: [0.15, 0.15, 0.15, 0.15, 0.15]
Description: Balanced, moderate difficulty
```

### Artist
```
Band Targets: [0.05, 0.4, 0.7, 0.5, 0.3]
Description: High alpha/theta (creative states)
```

### Rebel
```
Band Targets: [0.05, 0.15, 0.3, 0.8, 0.9]
Description: High beta/gamma (alert, resistant)
```

## API Reference

### AIManager

```csharp
// Start stimulation on a citizen
AIManager.Instance.StartStimulation("citizen_01");

// Update wave sample
AIManager.Instance.SetWaveSample(delta, theta, alpha, beta, gamma);

// Stop stimulation
AIManager.Instance.StopStimulation();
```

### CitizenController

```csharp
// Access current state
CitizenState state = citizen.CurrentState;

// Get evaluation score
float score = citizen.EvaluationScore;

// Get instability
float instability = citizen.Instability;

// Events
citizen.OnStabilized += (c) => Debug.Log("Success!");
citizen.OnCriticalFailure += (c) => Debug.Log("Failure!");
```

### FeedbackManager

```csharp
// Set ambient intensity (0..1)
feedbackManager.SetAmbientPulse(0.8f);

// Play tone
feedbackManager.PlayTone(440f, 0.5f);

// Trigger alarm
feedbackManager.TriggerAlarm();
```

## Animator Setup

The system expects these animator parameters:

- **agitation** (float): 0..1, drives agitation animations
- **composure** (float): 0..1, drives calm animations
- **critical** (trigger): Triggers critical failure animation

Example animation setup:
1. Create a blend tree with two clips:
   - Idle animation (composure = 1)
   - Agitated animation (agitation = 1)
2. Add a critical failure animation triggered by "critical" parameter

## Performance Notes

- Target: 60 FPS
- Wave evaluation runs at 30 Hz (configurable)
- Use object pooling for particles and audio
- Limit active citizens to 1-2 for game jam

## Debugging

### Enable Verbose Logging
Set `enableVerboseLogging = true` in AISettings to see detailed logs.

### Wave Visualizer
- Shows per-band comparison
- Displays evaluation score
- Shows instability meter
- Indicates current state

### Common Issues

**Problem**: Citizen not responding
- Check AIManager has AISettings assigned
- Verify citizen has valid NeuralProfile
- Call `AIManager.Instance.StartStimulation(citizen)` to activate

**Problem**: Score always 0
- Ensure wave sample is being updated (check WaveInputSimulator)
- Verify bandTargets and bandTolerance are valid
- Check for NaN values in wave input

**Problem**: Immediate critical failure
- Reduce instabilityRate in NeuralProfile
- Increase overloadThreshold in AISettings
- Increase bandTolerance in NeuralProfile

## Brain Activity Monitor UI

The Brain Activity Monitor is a retro computer interface that displays real-time brain wave activity and allows control over citizen obedience parameters.

### Features

- **Real-time Waveform Display**: 5 animated line graphs showing Delta, Theta, Alpha, Beta, and Gamma brain waves
- **Obedience Slider**: Single control that adjusts multiple parameters to make citizens more compliant
- **Advanced Parameter Panel**: Fine-tune individual settings with real-time preview
- **DOTween Animations**: Smooth transitions, button feedback, alerts, and power on/off sequences
- **World Space Canvas**: Place the monitor on 3D objects in your scene

### Setup Instructions

#### 1. Create the Monitor UI Hierarchy

```
BrainMonitorComputer (Empty GameObject)
├── Canvas (Canvas - World Space)
│   ├── MainPanel (Panel)
│   │   ├── HeaderPanel
│   │   │   ├── CitizenNameText (TextMeshProUGUI)
│   │   │   └── StatusIndicator (Image)
│   │   ├── WaveformPanel
│   │   │   ├── DeltaWaveform (WaveformDisplay)
│   │   │   │   ├── ThetaWaveform (WaveformDisplay)
│   │   │   │   ├── AlphaWaveform (WaveformDisplay)
│   │   │   │   ├── BetaWaveform (WaveformDisplay)
│   │   │   │   └── GammaWaveform (WaveformDisplay)
│   │   │   ├── StatusPanel
│   │   │   │   ├── StateText (TextMeshProUGUI)
│   │   │   │   ├── EvaluationScoreText (TextMeshProUGUI)
│   │   │   │   ├── InstabilityText (TextMeshProUGUI)
│   │   │   │   └── InstabilityBar (Slider)
│   │   │   ├── ObediencePanel (ObedienceController)
│   │   │   │   ├── ObedienceLabel (TextMeshProUGUI)
│   │   │   │   ├── ObedienceSlider (Slider)
│   │   │   │   └── PercentageText (TextMeshProUGUI)
│   │   │   ├── ParameterPanel (ParameterPanel)
│   │   │   │   ├── ToggleButton (Button)
│   │   │   │   ├── PanelContainer
│   │   │   │   │   ├── SliderContainer (Vertical Layout Group)
│   │   │   │   │   ├── SaveButton (Button)
│   │   │   │   │   ├── LoadButton (Button)
│   │   │   │   │   └── ResetButton (Button)
│   │   │   └── ControlPanel
│   │   │       ├── PowerButton (Button)
│   │   │       ├── StartButton (Button)
│   │   │       ├── StopButton (Button)
│   │   │       └── ResetButton (Button)
└── BrainActivityMonitor (Script)
```

#### 2. Configure Canvas

1. Set **Render Mode** to "World Space"
2. Set **Canvas Scaler** to "Scale With Screen Size" or fixed pixel size (e.g., 1024x768)
3. Position canvas on 3D computer screen model
4. Adjust **Scale** to fit screen (e.g., 0.001 for realistic size)

#### 4. Setup Waveform Displays

For each brain wave band (Delta, Theta, Alpha, Beta, Gamma):

1. Create a **GameObject** with **RawImage** component
2. Add **WaveformDisplay** script
3. Set **Band Index** (0=Delta, 1=Theta, 2=Alpha, 3=Beta, 4=Gamma)
4. Set **Wave Color**:
   - Delta: Red (1, 0.2, 0.2)
   - Theta: Orange (1, 0.6, 0.2)
   - Alpha: Green (0.2, 1, 0.2)
   - Beta: Blue (0.2, 0.6, 1)
   - Gamma: Purple (0.8, 0.2, 1)
5. Enable **Show Target Line** and **Show Tolerance Zone**

#### 5. Setup Obedience Controller

1. Create a panel with:
   - **Slider** (Obedience slider)
   - **TextMeshProUGUI** labels
   - **Image** for slider fill
2. Add **ObedienceController** script
3. Assign UI references
4. Configure parameter ranges:
   - Tolerance Multiplier: 0.5 to 2.5
   - Instability Rate Multiplier: 2.0 to 0.3
   - Success Threshold Adjust: 0.1 to -0.2

#### 6. Setup Parameter Panel

1. Create collapsible panel structure
2. Create a **Slider Prefab** with:
   - Slider component
   - TextMeshProUGUI label
3. Add **ParameterPanel** script
4. Assign references:
   - Panel Container
   - Toggle/Save/Load/Reset buttons
   - Slider Container (Vertical Layout Group)
   - Slider Prefab

#### 7. Setup Brain Activity Monitor

1. Add **BrainActivityMonitor** script to root GameObject
2. Assign all component references:
   - Obedience Controller
   - Parameter Panel
   - All 5 Waveform Displays
   - Status text fields
   - Control buttons
3. Configure settings:
   - History Buffer Size: 120 (2 seconds at 60 FPS)
   - Update Rate: 30 Hz
   - Enable Auto Start: true

### Usage

#### Connecting to a Citizen

```csharp
// Get the monitor
BrainActivityMonitor monitor = FindObjectOfType<BrainActivityMonitor>();

// Get a citizen
CitizenController citizen = AIManager.Instance.GetCitizen("citizen_01");

// Connect monitor to citizen
monitor.SetActiveCitizen(citizen);
```

#### Power Control

```csharp
// Power on the monitor
monitor.PowerOn();

// Power off
monitor.PowerOff();

// Toggle
monitor.TogglePower();
```

#### Monitoring Control

```csharp
// Start monitoring (begins stimulation)
monitor.StartMonitoring();

// Stop monitoring
monitor.StopMonitoring();

// Reset (clears data, resets citizen)
monitor.ResetMonitor();
```

#### Obedience Control

```csharp
// Set obedience level (0-100%)
obedienceController.SetObedience(75f, animate: true);

// Get current obedience
float obedience = obedienceController.CurrentObedience;

// Reset to default (50%)
obedienceController.ResetToDefault();

// Restore original parameters
obedienceController.RestoreOriginalParameters();
```

#### Parameter Tweaking

```csharp
// Set active profile for tweaking
parameterPanel.SetActiveProfile(neuralProfile);

// Expand/collapse panel
parameterPanel.TogglePanel();
parameterPanel.ExpandPanel();
parameterPanel.CollapsePanel();
```

### Obedience System

The obedience slider affects three key parameters simultaneously:

#### At 0% Obedience (Rebellious)
- Band Tolerance: ×0.5 (50% of base)
- Instability Rate: ×2.0 (builds twice as fast)
- Success Threshold: +0.1 (harder to succeed)

#### At 50% Obedience (Neutral)
- Band Tolerance: ×1.25 (125% of base)
- Instability Rate: ×1.15 (slightly faster)
- Success Threshold: -0.05 (slightly easier)

#### At 100% Obedience (Compliant)
- Band Tolerance: ×2.5 (250% of base, very forgiving)
- Instability Rate: ×0.3 (30% of base, very slow buildup)
- Success Threshold: -0.2 (much easier to succeed)

**Obedience Labels:**
- 90-100%: COMPLIANT
- 75-89%: COOPERATIVE
- 60-74%: STABLE
- 40-59%: NEUTRAL
- 25-39%: RESISTANT
- 10-24%: DEFIANT
- 0-9%: REBELLIOUS

### DOTween Animations

The UI uses DOTween for smooth animations:

#### Panel Animations
```csharp
// Slide in from direction
UITweenAnimations.SlideIn(rectTransform, SlideDirection.Left, duration: 0.5f);

// Slide out
UITweenAnimations.SlideOut(rectTransform, SlideDirection.Right);

// Fade in/out
UITweenAnimations.FadeIn(canvasGroup);
UITweenAnimations.FadeOut(canvasGroup);
```

#### Button Feedback
```csharp
// Button press
UITweenAnimations.ButtonPress(button.transform);

// Pulse effect
UITweenAnimations.Pulse(transform, strength: 0.1f);

// Shake for errors
UITweenAnimations.Shake(transform, strength: 20f);
```
### Customization

#### Waveform Colors
Edit the `BandColors` array in BrainActivityMonitor.cs:
```csharp
private static readonly Color[] BandColors = new Color[]
{
    new Color(1f, 0.2f, 0.2f),  // Delta - Red
    new Color(1f, 0.6f, 0.2f),  // Theta - Orange
    new Color(0.2f, 1f, 0.2f),  // Alpha - Green
    new Color(0.2f, 0.6f, 1f),  // Beta - Blue
    new Color(0.8f, 0.2f, 1f)   // Gamma - Purple
};
```

#### Buffer Size
Change history window size:
```csharp
// In BrainActivityMonitor Inspector
History Buffer Size: 120  // 2 seconds at 60 FPS
                    : 240  // 4 seconds at 60 FPS
                    : 60   // 1 second at 60 FPS
```

### Troubleshooting

**Problem**: Waveforms not displaying
- Check DataBufferManager is receiving data
- Verify WaveformDisplay components are initialized
- Ensure textures are being created (check for errors in console)

**Problem**: Obedience slider not affecting parameters
- Verify active profile is set: `obedienceController.SetActiveProfile(profile)`
- Check AISettings reference is not null
- Ensure original values are stored before modification

**Problem**: DOTween animations not playing
- Verify DOTween is imported and initialized
- Check for animation conflicts (multiple tweens on same property)
- Ensure gameObject is active when starting animations

**Problem**: UI not visible in world space
- Check Canvas Render Mode is "World Space"
- Verify Canvas scale (should be very small, e.g., 0.001)
- Check camera culling layers
- Ensure Event Camera is assigned if using UI interactions

### Performance Tips

1. **Update Rate**: Lower the update rate (20-30 Hz) for better performance
2. **Buffer Size**: Smaller buffers use less memory and are faster to render
3. **Texture Size**: Reduce waveform texture size if needed (default: 512x256)
4. **Object Pooling**: Pool parameter slider instances for large parameter sets

### Integration Example

Complete setup from scratch:

```csharp
using NeuralWaveBureau.UI;
using NeuralWaveBureau.AI;

public class MonitorSetup : MonoBehaviour
{
    void Start()
    {
        // Get components
        var monitor = GetComponent<BrainActivityMonitor>();
        var citizen = AIManager.Instance.GetCitizen("citizen_01");

        // Setup monitor
        monitor.SetActiveCitizen(citizen);
        monitor.PowerOn();

        // Wait a moment, then start monitoring
        StartCoroutine(DelayedStart(monitor));
    }

    IEnumerator DelayedStart(BrainActivityMonitor monitor)
    {
        yield return new WaitForSeconds(2f);
        monitor.StartMonitoring();
    }
}
```

## Next Steps

1. Create actual wave input system (replace WaveInputSimulator)
2. Add citizen models and animations
3. Design console/chair prefabs
4. Add audio clips for reactions
5. Create particle effects for feedback
6. Build level with citizen placement
7. Implement game objectives and win conditions

## File Structure

```
Assets/
  Scripts/
    AI/
      AIManager.cs           - Central coordinator
      CitizenController.cs   - Per-citizen behavior
      WaveEvaluator.cs      - Wave similarity computation
      EmotionStateMachine.cs - State machine
      FeedbackManager.cs    - Audio/visual feedback
      WaveVisualizer.cs     - Debug visualization
      WaveInputSimulator.cs - Test input
    Data/
      NeuralProfile.cs      - Citizen profile data
      AISettings.cs         - Global settings
      CreateSampleProfiles.cs - Profile generator
    UI/
      BrainActivityMonitor.cs - Main UI controller
      WaveformDisplay.cs     - Animated waveform renderer
   ObedienceController.cs - Obedience slider logic
      ParameterPanel.cs      - Advanced parameter tweaking
      UITweenAnimations.cs   - DOTween animation utilities
      DataBufferManager.cs   - Wave history buffer
  Data/
    Profiles/              - Neural profile assets
    DefaultAISettings.asset - Global settings
  Prefabs/
    UI/
      BrainMonitorComputer.prefab - Complete monitor UI
      ParameterSlider.prefab      - Parameter slider template
```

## Credits

Built according to ARCHITECTURE.md specifications for Neural Wave Bureau game jam project.
