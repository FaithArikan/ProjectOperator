# Brain Activity Monitor - Setup Fix

## Problem

When you press Q/W/E/R to change presets in the Wave Input Simulator, the waveforms don't display on the Brain Activity Monitor.

## Root Cause

The `BrainActivityMonitor` requires **three conditions** to be met before it will display data:

1. **Powered On** - `_isPoweredOn = true`
2. **Monitoring Active** - `_isMonitoring = true`
3. **Has Active Citizen** - `_activeCitizen != null`

Without all three, the `UpdateMonitorData()` method returns early and doesn't add samples to the buffer, so waveforms remain empty.

```csharp
// From BrainActivityMonitor.cs:160-174
private void Update()
{
    if (!_isPoweredOn || !_isMonitoring)  // ← Returns if not powered/monitoring
        return;
    // ...
}

// From BrainActivityMonitor.cs:266-276
private void UpdateMonitorData()
{
    if (_aiManager == null || _activeCitizen == null)  // ← Returns if no citizen
        return;

    WaveSample sample = _aiManager.CurrentWaveSample;
    _bufferManager.AddSample(sample.bandValues);  // ← This never runs!
}
```

## Quick Fix - Automated Setup

### Step 1: Add the Test Setup Script

1. Select your **BrainActivityMonitor** GameObject in the hierarchy
2. In the Inspector, click **Add Component**
3. Search for **Brain Monitor Test Setup**
4. The script will auto-setup on Start (after 0.5 seconds)

### Step 2: Create a Test Citizen (if you don't have one)

1. Create an empty GameObject named **"TestCitizen"**
2. Add an **Animator** component (required by CitizenController)
3. Add the **CitizenController** script
4. In the Inspector:
   - **Citizen ID**: "citizen_01"
   - **Profile**: Assign one of your Neural Profiles (e.g., `Ordinary.asset`)

### Step 3: Run the Scene

1. Press Play
2. Wait for the monitor to power on automatically
3. Press **Q/W/E/R** to change presets
4. **Waveforms should now update in real-time!**

If auto-setup doesn't work, press **F1** to manually trigger setup.

## Manual Setup Alternative

If you prefer to control it yourself, remove the `BrainMonitorTestSetup` script and call these methods in order:

```csharp
// 1. Get references
BrainActivityMonitor monitor = GetComponent<BrainActivityMonitor>();
CitizenController citizen = AIManager.Instance.GetCitizen("citizen_01");

// 2. Set active citizen
monitor.SetActiveCitizen(citizen);

// 3. Power on
monitor.PowerOn();

// 4. Start monitoring (after power-on animation completes)
StartCoroutine(DelayedStart(monitor));

IEnumerator DelayedStart(BrainActivityMonitor monitor)
{
    yield return new WaitForSeconds(2f);
    monitor.StartMonitoring();
}
```

## Fixed Issues

### 1. Keyboard Conflict

**Old behavior:**
- W key = Increase band AND Artist preset (conflict!)
- S key = Decrease band

**New behavior:**
- ↑/↓ Arrow keys = Increase/Decrease band values
- Q/W/E/R = Presets (no conflicts)

### 2. Better Debug Info

The Wave Input Simulator now shows:
- Current band values in real-time
- Selected band indicator
- Monitor connection status
- Clearer keyboard hints

### 3. Enhanced Logging

When you press a preset key, you'll see:
```
[WaveInputSimulator] Preset loaded: Artist - Bands: [0.05, 0.40, 0.70, 0.50, 0.30]
```

## Testing Checklist

- [ ] Scene has AIManager GameObject with AIManager script
- [ ] Scene has a Citizen with CitizenController and NeuralProfile
- [ ] BrainActivityMonitor has all waveform displays assigned
- [ ] Monitor has ObedienceController and ParameterPanel assigned
- [ ] BrainMonitorTestSetup is attached (for auto-setup)
- [ ] Press Play
- [ ] Monitor powers on automatically
- [ ] Press Q - waveforms show "Ordinary" pattern
- [ ] Press W - waveforms change to "Artist" pattern
- [ ] Press E - waveforms change to "Rebel" pattern
- [ ] Press R - waveforms show random values
- [ ] Use ↑/↓ to manually adjust individual bands
- [ ] Use 1-5 to select which band to adjust

## Understanding the Data Flow

```
WaveInputSimulator (Update)
    ↓ (calls SetWaveSample)
AIManager.SetWaveSample()
    ↓ (stores _currentWaveSample)
BrainActivityMonitor.UpdateMonitorData() ← REQUIRES: powered, monitoring, citizen
    ↓ (gets CurrentWaveSample)
DataBufferManager.AddSample()
    ↓
WaveformDisplay.DrawWaveform() ← Gets data from buffer
    ↓
Texture2D pixels updated
    ↓
Waveforms visible on screen! ✓
```

## Troubleshooting

### Problem: "No citizen found" warning

**Solution:** Create a GameObject with:
- CitizenController script
- Animator component
- Assigned NeuralProfile

### Problem: Waveforms still not showing

**Checklist:**
1. Check Console for `[WaveInputSimulator]` messages - are presets loading?
2. Check Console for `[BrainMonitorTestSetup]` messages - did setup complete?
3. Is the monitor powered on?
4. Is monitoring active? (Check status text on monitor)
5. Are waveform GameObjects active in hierarchy?

### Problem: Presets load but waveforms don't change

**Possible causes:**
- Monitor not monitoring (call `StartMonitoring()`)
- No active citizen set
- Buffer not receiving data (check `UpdateMonitorData` is running)

### Problem: F1 doesn't work

Make sure the `BrainMonitorTestSetup` component is active and enabled in the Inspector.

## Performance Notes

- Wave input updates at ~60 FPS (every Update)
- Monitor updates at 30 Hz (configurable via `_updateRate`)
- Waveform textures redraw every frame (512x256 pixels)
- Buffer holds 120 samples by default (~2 seconds at 60 FPS)

## Next Steps

Once waveforms are working:
1. Test obedience slider (should affect difficulty)
2. Test parameter panel (advanced tweaking)
3. Try to stabilize a citizen by matching target values
4. Observe state transitions (Idle → BeingStimulated → Stabilized/Agitated)
5. Replace WaveInputSimulator with real input (EEG, controller, etc.)

## Files Modified

- `Assets/Scripts/AI/WaveInputSimulator.cs` - Fixed keyboard conflicts, added debug info
- `Assets/Scripts/UI/BrainMonitorTestSetup.cs` - NEW: Automated setup helper

## Files Referenced

- `Assets/Scripts/UI/BrainActivityMonitor.cs` - Main monitor controller
- `Assets/Scripts/AI/AIManager.cs` - Central AI coordinator
- `Assets/IMPLEMENTATION_GUIDE.md` - Complete system documentation
