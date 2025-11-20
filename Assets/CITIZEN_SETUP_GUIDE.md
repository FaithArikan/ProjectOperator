# Citizen Prefab & Monitoring Setup Guide

This guide explains how to set up the citizen spawning and brain wave monitoring system in your scene.

## Overview

The system consists of several components that work together:
- **Citizen Prefab**: The character that walks to the monitor and gets their brain waves monitored
- **MonitoringStation**: Marks where citizens should stand for monitoring
- **CitizenSpawner**: Spawns citizens and sends them to the monitoring station
- **BrainActivityMonitor**: The UI system that displays brain wave data

## Step 1: Create Citizen Prefabs

You can create **multiple different citizen prefabs** for variety! The spawner will randomly select from your prefab list.

### Create a Single Prefab:

1. **Create a base GameObject** for your citizen in the scene:
   - Right-click in Hierarchy → Create Empty
   - Name it "CitizenPrefab_01" (use numbers for multiple prefabs)

2. **Add visual representation**:
   - Add a Capsule as a child: Right-click CitizenPrefab → 3D Object → Capsule
   - Or add your custom character model (e.g., from POLYGON City Characters pack)
   - Optional: Add an Animator component if you have walk animations

3. **Add required components** to the CitizenPrefab GameObject:
   - **CitizenController** (Scripts/AI/CitizenController.cs)
     - Assign a **NeuralProfile** asset
     - Set a unique **Citizen ID** (e.g., "citizen_01")

   - **CitizenMovement** (Scripts/AI/CitizenMovement.cs)
     - Set **Walk Speed** (default: 1.5)
     - Set **Rotation Speed** (default: 180)
     - **Use NavMesh**: Check if using Unity NavMesh, uncheck for simple movement
     - Assign **Animator** if you have walk animations

4. **Optional: Add NavMeshAgent** (if using NavMesh navigation):
   - Add Component → Navigation → NavMesh Agent
   - Make sure "Use NavMesh" is checked in CitizenMovement

5. **Save as Prefab**:
   - Drag the CitizenPrefab from Hierarchy into your Assets/Prefabs folder
   - Delete the instance from the scene

### Create Multiple Prefabs (Recommended):

Repeat the above steps to create 3-5 different citizen prefabs:
- **CitizenPrefab_Man** - Male character model
- **CitizenPrefab_Woman** - Female character model
- **CitizenPrefab_Old** - Elderly character model
- **CitizenPrefab_Young** - Young character model
- etc.

Each prefab can have:
- Different character models
- Different walk speeds
- Different animations
- Same components (CitizenController + CitizenMovement)

## Step 2: Create a NeuralProfile

Citizens need a NeuralProfile that defines their brain wave targets:

1. **Create NeuralProfile asset**:
   - Right-click in Project → Create → Neural Wave Bureau → Neural Profile
   - Name it descriptively (e.g., "NeuralProfile_Relaxed")

2. **Configure the profile**:
   - **Profile ID**: Unique identifier (e.g., "relaxed_001")
   - **Display Name**: Name shown in UI (e.g., "John Doe")
   - **Band Targets**: Set target values for Delta, Theta, Alpha, Beta, Gamma (0-1 range)
   - **Band Tolerance**: How much variance is allowed (e.g., 0.15)
   - **Baseline Instability**: How unstable this citizen is naturally (0-1)
   - **Min Stimulation Time**: How long they need to be stimulated (seconds)

3. **Create multiple profiles** for variety (optional):
   - Create 3-5 different profiles with different target values
   - This makes each citizen unique

## Step 3: Set Up the Scene

### A. Create Monitoring Station

1. **Create an empty GameObject**:
   - Right-click in Hierarchy → Create Empty
   - Name it "MonitoringStation"
   - Position it where you want citizens to stand (e.g., behind your monitor)

2. **Add MonitoringStation component**:
   - Add Component → Scripts → AI → Monitoring Station
   - Configure settings:
     - **Citizen Position Offset**: Offset from this object where citizen stands (e.g., (0, 0, 0))
     - **Facing Direction**: Direction citizen should face (e.g., (0, 0, 1) to face forward)
     - **Auto Start Monitoring**: ✓ Check this to automatically start when citizen arrives
     - **Monitoring Start Delay**: 0.5 seconds

3. **Position the MonitoringStation**:
   - Move it to where citizens should stand for monitoring
   - Use the gizmos in Scene view to visualize:
     - Cyan sphere = citizen position
     - Yellow arrow = facing direction

### B. Create Citizen Spawner

1. **Create spawn point**:
   - Right-click in Hierarchy → Create Empty
   - Name it "CitizenSpawnPoint"
   - Position it where citizens should spawn (e.g., off-screen, at a door)

2. **Create spawner GameObject**:
   - Right-click in Hierarchy → Create Empty
   - Name it "CitizenSpawner"

3. **Add CitizenSpawner component**:
   - Add Component → Scripts → AI → Citizen Spawner
   - Configure settings:

     **Prefab Settings:**
     - **Citizen Prefabs**: Add your citizen prefabs here (click + to add multiple)
       - Drag all your prefab variants (CitizenPrefab_Man, CitizenPrefab_Woman, etc.)
     - **Available Profiles**: Add your NeuralProfile assets (click + to add multiple)
     - **Prefab Selection Mode**:
       - **Random**: Picks a random prefab each spawn (recommended)
       - **Sequential**: Cycles through prefabs in order

     **Spawn Settings:**
     - **Spawn Point**: Drag your CitizenSpawnPoint GameObject here
     - **Target Station**: Drag your MonitoringStation GameObject here
     - **Spawn On Start**: ✓ Check to spawn first citizen automatically
     - **Initial Spawn Delay**: 2 seconds

     **Citizen Management:**
     - **Destroy Previous Citizen**: ✓ Check to remove old citizen when spawning new one

4. **Optional: Enable auto-spawn**:
   - **Auto Spawn Enabled**: ✓ Check to spawn citizens at intervals
   - **Spawn Interval**: 30 seconds between spawns
   - **Max Citizens**: 0 for unlimited, or set a limit

### C. Verify BrainActivityMonitor Setup

Make sure your BrainActivityMonitor is properly configured:

1. Find your BrainActivityMonitor GameObject in the scene
2. Check that it has:
   - **Main Screen Panel**: Assigned
   - **CRT Effect**: Assigned
   - **Waveform Displays**: List of 5 WaveformDisplay components
   - **Status texts**: Citizen name, state, evaluation score, instability
   - **Power Button**: Assigned

## Step 4: Test the System

### Basic Test Flow:

1. **Enter Play Mode**
2. **Citizen spawns** at spawn point after initial delay
3. **Citizen walks** to monitoring station
4. **Citizen arrives** and stops at monitoring station
5. **Monitor powers on** automatically (if Auto Start Monitoring is enabled)
6. **Waveforms appear** on screen
7. **Start manipulating brain waves** using the ObedienceController

### Manual Testing:

If auto-start is disabled:
1. Spawn citizen: Select CitizenSpawner → Right-click → Spawn Citizen Now
2. Wait for citizen to arrive
3. Click the **Power Button** on the monitor to power on
4. Click **Start Monitoring** (if you have that button)

## Step 5: Control Brain Waves

Once monitoring is active:

1. **Use ObedienceController sliders** to adjust brain wave bands (Delta, Theta, Alpha, Beta, Gamma)
2. **Watch the waveforms** update in real-time
3. **Match the target values** shown as yellow lines
4. **Stay within tolerance zones** (yellow shaded areas)
5. **Monitor the evaluation score** - get it above the success threshold
6. **Watch for states**:
   - **Idle**: Gray - Not being monitored
   - **Being Stimulated**: Cyan - Monitoring active
   - **Stabilized**: Green - Success! Brain waves matched
   - **Agitated**: Yellow - Brain waves too far from target
   - **Critical Failure**: Red - Citizen overloaded

## Step 6: Spawn Multiple Citizens

To test with multiple citizens:

1. **Enable Auto Spawn** on CitizenSpawner
2. Each new citizen will walk to the station
3. Previous citizen is destroyed (if Destroy Previous Citizen is enabled)
4. Each citizen can have:
   - A different **character model** (random prefab from your list)
   - A different **NeuralProfile** (random profile from your list)
   - This creates lots of variety!

### Variety Example:
With 5 prefabs and 5 profiles, you get **25 possible combinations**:
- Old Man with Relaxed profile
- Young Woman with Anxious profile
- Business Man with Focused profile
- etc.

## Troubleshooting

### Citizen doesn't spawn
- Check that at least one Citizen Prefab is assigned in CitizenSpawner's **Citizen Prefabs** list
- Check that all prefabs have CitizenController and CitizenMovement components
- Look for errors in Console

### Citizen doesn't move
- Check that Target Station is assigned in CitizenSpawner
- If using NavMesh: Make sure NavMesh is baked and NavMeshAgent is on prefab
- If not using NavMesh: Uncheck "Use NavMesh" in CitizenMovement

### Monitor doesn't power on
- Check that MonitoringStation has BrainActivityMonitor reference
- Make sure "Auto Start Monitoring" is checked
- Check that monitor's Main Screen Panel is assigned

### Waveforms don't show
- Make sure Waveform Displays are assigned in BrainActivityMonitor
- Check that each WaveformDisplay has proper initialization
- Make sure monitoring actually started (check Console for "Monitoring STARTED" log)

### Citizen walks through objects
- If using NavMesh: Make sure obstacles are marked as NavMesh Obstacle or Static
- If using simple movement: Add colliders and consider implementing obstacle avoidance

## Advanced: Custom Animations

To add walk animations:

1. Add an **Animator** component to your citizen prefab
2. Create an Animator Controller with:
   - **Bool parameter**: "isWalking"
   - **Float parameter**: "walkSpeed"
3. Set up animation states:
   - Idle animation (when isWalking = false)
   - Walk animation (when isWalking = true)
4. Assign Animator to CitizenMovement component
5. Set animation parameter names in CitizenMovement

## System Architecture

```
CitizenSpawner
    ↓ spawns
CitizenPrefab (CitizenController + CitizenMovement)
    ↓ moves to
MonitoringStation
    ↓ notifies on arrival
BrainActivityMonitor
    ↓ displays data from
AIManager → WaveEvaluator → EmotionStateMachine
    ↑ receives input from
ObedienceController (player input)
```

## Next Steps

- Create multiple NeuralProfiles for variety
- Design custom citizen character models
- Add particle effects when citizens arrive/leave
- Add sound effects for footsteps and arrivals
- Create UI buttons to manually spawn citizens
- Implement a queue system for multiple citizens waiting
