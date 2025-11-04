# AI Architecture — Neural Wave Bureau (3D, heavily 2D)

**Purpose:**
This architecture file defines the AI systems for the 3D-but-heavily-2D game where the player manipulates brain waves to alter citizens. It documents components, data flows, interfaces, algorithms, performance constraints, and Unity-specific implementation guidance for a solo developer building a game jam MVP.

---

## 1. High-level goals

* **Playable in 1 month**: systems must be simple, testable, and easy to iterate.
* **Expressive behavior**: citizens should display readable emotional states and clear reactions to wave input.
* **Deterministic outcome**: player actions should have predictable consequences for tuning and fairness.
* **Low runtime cost**: single scene, locked camera. Keep CPU/GPU budget small.

---

## 2. Scope & Assumptions

* Single-player, single-scene, no multiplayer networking.
* Citizens are non-combat NPCs that react to wave stimuli and produce simple animations/dialogue.
* Wave evaluation logic runs at UI/framerate (60 FPS target) but evaluation can be sampled at lower rate (e.g., 30 Hz) if needed.
* Project uses Unity (URP recommended). Implementation uses MonoBehaviour + ScriptableObjects; ECS not required for jam.

---

## 3. System Overview (modules)

1. **AI Manager** (central coordinator)
2. **Citizen Controller** (per-citizen behavior & animation bridge)
3. **Neural Profile** (data model describing citizen waveform targets)
4. **Wave Evaluator** (computes similarity between player output and target)
5. **Emotion & State Machine** (maps evaluation -> emotional states -> animations)
6. **Feedback Manager** (lights, VFX, audio hooks)
7. **Persistence & Tuning** (ScriptableObjects for data-driven tuning)
8. **Debug & Telemetry** (visualizers for jam testing)

## 4. Data Models

Use `ScriptableObject` assets for all tunable data so you can tweak without recompiling.

### 4.1 `NeuralProfile` (ScriptableObject)

* `string profileId`
* `string displayName`
* `float[] bandTargets` (order: delta, theta, alpha, beta, gamma) — normalized 0..1
* `float[] bandTolerance` (same length) — allowed variance for success
* `float baselineInstability` — base chance of interference
* `BehaviorTree or StatePreset` reference — which state-machine to use
* `AudioClip[] reactionClips` (optional)

### 4.2 `WaveSample` (runtime struct)

* `float timestamp`
* `float[] bandValues` — current measured band energies

---

## 5. Core Algorithms

### 5.1 Wave similarity / evaluation

Compute a similarity score between player's output band energies and the `NeuralProfile.bandTargets`.

**Proposed approach (lightweight & stable):**

1. For each band, compute normalized difference: `d[i] = 1 - abs(sample[i] - target[i]) / tolerance[i]` (clamped 0..1).
2. Weighted average: `score = Sum( w[i] * d[i] ) / Sum(w[i])` where `w[i]` are optional band weights.
3. Smooth score over a short window using exponential moving average to avoid frame jitter.

**Alternative:** Pearson correlation across vectors (more sensitive to shape), but heavier and less intuitive for tuning.

**Thresholds:**

* `successThreshold` (e.g., 0.75)
* `overloadThreshold` (e.g., 0.25 to detect chaotic/incorrect waves)

### 5.2 Side-effects / instability

* Maintain a per-citizen `instability` meter (0..1).
* When player score < `overloadThreshold`, increase instability by `delta = (overloadThreshold - score) * instabilityRate * dt`.
* If instability > `instabilityFail` trigger a critical reaction (spasm, alarm, task failure).
* Successful stable application reduces instability slowly.

### 5.3 Temporal smoothing & sample rates

* Sample wave bands at `sampleRate` (default 30 Hz). Keep evaluator and smoothing on a fixed-timestep coroutine to avoid coupling to variable frame rates.
* Use EMA smoothing: `ema = alpha * currentScore + (1-alpha) * ema` where `alpha` = `1 - exp(-dt / tau)`.

---

## 6. Behavior Representation

Keep citizen behavior simple and deterministic. Two options:

### Option A — Finite State Machine (recommended for jam)

States: `Idle` -> `BeingStimulated` -> `Stabilized` | `Agitated` -> `CriticalFailure` | `Recovered`
Transitions driven by: evaluation score, instability meter, timers.

State table example:

* `Idle`: on wave start -> `BeingStimulated`
* `BeingStimulated`: if `emaScore >= successThreshold && time >= minStimTime` -> `Stabilized`
* `BeingStimulated`: if `emaScore <= overloadThreshold` -> increase instability -> if instability > limit -> `Agitated`
* `Agitated`: timer + recovery checks -> either recover to `Idle` or escalate to `CriticalFailure`

### Option B — Tiny Behavior Tree (if you prefer parametrized logic)

Nodes: `CheckWaveScore` -> `IfHigh -> PlayRecovery` | `IfLow -> IncreaseInstability -> PlayAgitation`.

---

## 7. Animation & Presentation Bridge

* Citizens use simple animation layers: `baseIdle`, `smallReact`, `largeReact`, `collapse`.
* Blendtrees controlled by two normalized parameters: `agitation` (0..1) and `composure` (0..1).
* Animator parameters: `float agitation`, `float composure`, `trigger critical`.
* For jam: prefer baked clips (Mixamo idle + 2 reaction clips) and blend via `Animator.SetFloat`.

---

## 8. Audio & VFX Integration

* **FeedbackManager** exposes API:

  * `PlayTone(float freq, float amp)` — generate or pitch-shift a short tone
  * `SetAmbientPulse(float intensity)` — drive light emission and post-process
  * `TriggerAlarm()` / `PlayReactionClip(AudioClip)`
* Use a pool of simple ParticleSystems attached to the chair for sparks, dust, and visual noise.
* Light emission intensity should map to evaluation score (green = good, red = bad).

---

## 9. Unity Implementation Details

### 9.1 Folder structure (suggested)

```
Assets/
  Scripts/
    AI/
      AIManager.cs
      CitizenController.cs
      WaveEvaluator.cs
      EmotionStateMachine.cs
    Data/
      NeuralProfile.cs (ScriptableObject)
  Prefabs/
    Citizen/
      citizen.prefab
    Console/
      console.prefab
  Scenes/
    Main.unity
  Art/
  Audio/
```

### 9.2 Class skeletons & API design

Follow your style guide: private fields start with `_`, public fields lowercase, comments above variables.

**AIManager (singleton-like)**

### 9.3 ScriptableObjects for tuning

* Create `NeuralProfile` assets for each citizen type and place them in `Assets/Data/Profiles`.
* Create `AISettings` ScriptableObject to hold global thresholds like `successThreshold`, `instabilityRate`, and `sampleRate`.

---

## 10. Performance

* Target 60 FPS. Keep per-frame CPU low.
* WaveEvaluator sampling on coroutine at fixed rate reduces per-frame spikes.
* Avoid per-vertex heavy shaders. Use simple ShaderGraph for wave plane.
* Pool particle systems and audio sources.
* Limit number of active citizens to 1—2 (for your scene it's generally 1).

---

## 11. Debugging & Tools

* **WaveVisualizer**: on-screen graph (debug-only) to show player wave vs target bands.
* **Instability meter HUD**: visualize per-citizen instability for tuning.
* **Logging toggles** in `AISettings` to enable verbose logs for development builds only.

---

## 12. Tuning & Playtest Checklist

* Adjust `bandTolerance` per profile until success feels achievable but non-trivial.
* Playtest three patterns: correct, noisy, wildly wrong — ensure readable reactions.
* Tune `smoothingTau` to balance responsiveness vs jitter.
* Ensure audio-visual feedback maps consistently to `score` and `instability`.

---

## 13. Example data: Minimal Profiles

* **Ordinary**: bandTargets = [0.1, 0.2, 0.6, 0.6, 0.2]; bandTolerance = [0.15,...]
* **Artist**: bandTargets = [0.05,0.4,0.7,0.5,0.3]
* **Rebel**: bandTargets = [0.05,0.15,0.3,0.8,0.9]

---

## 14. Edge cases & safety

* If wave input is out-of-range (NaN/Inf), clamp and treat as `no-signal` (score=0).
* If the player disconnects or pauses, persist current instability so resuming is consistent.

---

## 15. Next steps (deliverables I can produce for you)

* Full C# scripts for: `WaveEvaluator`, `aiManager`, `citizenController`, `NeuralProfile` (ScriptableObject), and a simple `FeedbackManager` wired to lights & audio.
* A minimal scene with one citizen and one console prefab pre-wired.
* A tuning JSON or ScriptableObject set with 3 profiles and AI settings.

---

*End of architecture file.*
