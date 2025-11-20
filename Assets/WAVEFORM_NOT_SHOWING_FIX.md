# Waveforms Not Showing - Quick Fix Guide

## Problem

When you press keyboard keys (↑↓ or Q/W/E/R), the waveforms don't display anything.

## Root Cause

The **BrainActivityMonitor** needs THREE things to work:
1. ❌ **Powered On** - Monitor must be powered on
2. ❌ **Active Citizen** - A citizen must be connected
3. ❌ **Monitoring Started** - Monitoring must be active

Without ALL THREE, the buffer doesn't receive data and waveforms stay blank.

## Quick Fix - 5 Steps

### Step 1: Add Diagnostic Tool

1. Create an empty GameObject named **"Diagnostics"**
2. Add the **BrainMonitorDiagnostics** script to it
3. Press Play

You'll see a diagnostic panel showing what's missing!

### Step 2: Check the Console

Look for these messages:
- ✅ `✓ AIManager found` - Good!
- ✅ `✓ AISettings assigned` - Good!
- ❌ `❌ No active citizen set` - **THIS IS THE PROBLEM**

### Step 3: Create a Citizen (if missing)

If you see "No citizens found":

1. Create GameObject → Name it **"TestCitizen"**
2. Add Component → **Animator** (required!)
3. Add Component → **CitizenController**
4. In Inspector:
   - **Citizen ID**: "citizen_01"
   - **Profile**: Drag a NeuralProfile asset here (create one if needed)

**To create a NeuralProfile**:
- Menu: Neural Wave Bureau → Create Sample Profiles
- Or: Right-click Project → Create → Neural Wave Bureau → Neural Profile

### Step 4: Auto-Setup the Monitor

1. Find your **BrainActivityMonitor** GameObject
2. Add Component → **BrainMonitorTestSetup**
3. Press Play
4. Press **F1** key

Watch the console for:
```
[BrainMonitorTestSetup] Setting up monitor...
[BrainMonitorTestSetup] Active citizen set: citizen_01
[BrainMonitorTestSetup] Monitor powered on
[BrainMonitorTestSetup] Monitoring started!
```

### Step 5: Test Keyboard Input

Now press:
- **Q** - Ordinary preset
- **W** - Artist preset
- **E** - Rebel preset
- **R** - Random values

Watch the console for:
```
[WaveInputSimulator] Preset loaded: Artist - Bands: [0.05, 0.40, 0.70, 0.50, 0.30]
[WaveformDisplay] Band 0 data: [0.05, 0.05, 0.05...0.05] (length: 120)
```

If you see these messages, the waveforms should be visible!

## Common Issues

### Issue 1: "No buffer manager assigned"

**Console shows**: `[WaveformDisplay] Band 0: No buffer manager assigned!`

**Fix**: The BrainActivityMonitor didn't initialize the waveforms.

1. Check that each WaveformDisplay GameObject is assigned in the Inspector
2. Make sure BrainActivityMonitor.Start() runs before trying to display

### Issue 2: "Buffer has insufficient data"

**Console shows**: `[WaveformDisplay] Band 0: Buffer has insufficient data (length: 0)`

**Fix**: Monitoring hasn't started yet.

1. Press F1 to trigger auto-setup
2. Or manually call: `monitor.PowerOn()` then `monitor.StartMonitoring()`

### Issue 3: Wave values are all zero

**Console shows**: `Wave: [0.00, 0.00, 0.00, 0.00, 0.00]`

**Fix**: WaveInputSimulator isn't sending data.

1. Check that WaveInputSimulator is active
2. Try pressing Q/W/E/R to load presets
3. Values should change immediately

### Issue 4: "Active Citizen: NONE"

**Console shows**: `❌ Active Citizen: NONE`

**Fix**: No citizen is set as active.

```csharp
// Manual fix in code:
var monitor = FindObjectOfType<BrainActivityMonitor>();
var citizen = FindObjectOfType<CitizenController>();
monitor.SetActiveCitizen(citizen);
monitor.PowerOn();
monitor.StartMonitoring();
```

## Debug Checklist

Run through this checklist:

- [ ] **Scene has AIManager GameObject**
  - [ ] AIManager script attached
  - [ ] AISettings asset assigned

- [ ] **Scene has at least one Citizen**
  - [ ] CitizenController script attached
  - [ ] Animator component present
  - [ ] NeuralProfile assigned

- [ ] **Scene has BrainActivityMonitor**
  - [ ] All 5 WaveformDisplay components assigned
  - [ ] ObedienceController assigned
  - [ ] ParameterPanel assigned

- [ ] **Scene has WaveInputSimulator**
  - [ ] Attached to AIManager GameObject
  - [ ] Script is enabled

- [ ] **Auto-setup script present**
  - [ ] BrainMonitorTestSetup on monitor
  - [ ] Press F1 to activate

- [ ] **Diagnostics running**
  - [ ] BrainMonitorDiagnostics on any GameObject
  - [ ] Check on-screen display

## Expected Console Output

When everything is working, you should see:

```
=== Brain Monitor Diagnostics Started ===
=== Initial System Check ===
✓ AIManager found
  ✓ AISettings assigned: DefaultAISettings
  ⚠ No active citizen set
✓ BrainActivityMonitor found on: BrainMonitorComputer
✓ WaveInputSimulator found
✓ Found 1 citizen(s):
  ✓ citizen_01: Ordinary - State: Idle
=== End Initial Check ===

[BrainMonitorTestSetup] Setting up monitor...
[BrainMonitorTestSetup] Active citizen set: citizen_01
[BrainMonitorTestSetup] Monitor powered on
[BrainMonitorTestSetup] Monitoring started!
[BrainMonitorTestSetup] You can now use Q/W/E/R to change presets and see waveforms update!

[WaveInputSimulator] Preset loaded: Artist - Bands: [0.05, 0.40, 0.70, 0.50, 0.30]
[WaveformDisplay] Initialized - Band 0, Color: RGBA(1.0, 0.2, 0.2, 1.0)
[WaveformDisplay] Initialized - Band 1, Color: RGBA(1.0, 0.6, 0.2, 1.0)
[WaveformDisplay] Initialized - Band 2, Color: RGBA(0.2, 1.0, 0.2, 1.0)
[WaveformDisplay] Initialized - Band 3, Color: RGBA(0.2, 0.6, 1.0, 1.0)
[WaveformDisplay] Initialized - Band 4, Color: RGBA(0.8, 0.2, 1.0, 1.0)

=== System Status ===
Current Wave Sample: [0.05, 0.40, 0.70, 0.50, 0.30]
Active Citizen: citizen_01
  State: BeingStimulated
  Evaluation Score: 0.756
  Instability: 0.12
  Is Active: True
Monitor found on: BrainMonitorComputer
===================
```

## Visual Indicators

When working correctly:

1. **On-screen diagnostic panel** (top-left corner):
   - ✓ AIManager: OK (green)
   - ✓ Active Citizen: citizen_01 (green)
   - State: BeingStimulated
   - Wave values changing when you press keys

2. **Waveform displays**:
   - Colored horizontal bands visible (red, orange, green, blue, purple)
   - Lines moving/changing when you press Q/W/E/R
   - Target lines visible (yellow)
   - Tolerance zones visible (semi-transparent)

3. **Console messages**:
   - Preset loaded messages
   - Band data messages every 2 seconds
   - No errors or warnings

## Still Not Working?

If waveforms still don't show after following all steps:

1. **Check GameObject hierarchy**:
   ```
   BrainMonitorComputer
   └── Canvas
       └── MainPanel
           └── WaveformPanel
               ├── DeltaWaveform (RawImage + WaveformDisplay)
               ├── ThetaWaveform (RawImage + WaveformDisplay)
               ├── AlphaWaveform (RawImage + WaveformDisplay)
               ├── BetaWaveform (RawImage + WaveformDisplay)
               └── GammaWaveform (RawImage + WaveformDisplay)
   ```

2. **Check each WaveformDisplay**:
   - Has RawImage component
   - Has WaveformDisplay script
   - Band Index is set (0, 1, 2, 3, 4)
   - Wave Color is set
   - GameObject is ACTIVE (checkbox checked)

3. **Check Canvas**:
   - Render Mode: World Space or Screen Space
   - If World Space: Position in front of camera
   - Canvas Scaler configured

4. **Take a screenshot and check**:
   - Are the GameObjects visible in Scene view?
   - Are they visible in Game view?
   - Are they the right size and position?

## Performance Check

If everything is set up but waveforms are slow:

- Buffer update rate too high
- Too many waveforms
- Texture size too large

**Solution**: In BrainActivityMonitor Inspector:
- History Buffer Size: 120 (default)
- Update Rate: 30 Hz (default)

## Files to Check

1. **Scripts/UI/BrainActivityMonitor.cs** - Main controller
2. **Scripts/UI/WaveformDisplay.cs** - Individual waveform renderer (now with debug logs)
3. **Scripts/UI/BrainMonitorTestSetup.cs** - Auto-setup helper
4. **Scripts/UI/BrainMonitorDiagnostics.cs** - Debug tool (NEW!)
5. **Scripts/AI/WaveInputSimulator.cs** - Keyboard input

All scripts should be compiled without errors!
