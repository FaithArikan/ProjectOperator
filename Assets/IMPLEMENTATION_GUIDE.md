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
  Data/
    Profiles/              - Neural profile assets
    DefaultAISettings.asset - Global settings
```

## Credits

Built according to ARCHITECTURE.md specifications for Neural Wave Bureau game jam project.
