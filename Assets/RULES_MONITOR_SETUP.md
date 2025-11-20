# Rules Monitor Setup Guide

This guide explains how to set up the Rules Monitor feature in the Unity Editor, which displays game instructions before the player accesses the Brain Activity Monitor.

## Overview

The system uses a three-camera sequence:
1. **Room View** - Overview of the room with interactable objects
2. **Rules Monitor View** - Close-up of instructions/rules screen
3. **Brain Monitor View** - Close-up of the brain activity monitor

## Step-by-Step Setup

### 1. Create Rules Monitor Camera

1. In the Hierarchy, find your existing Cinemachine cameras (RoomCinCamera and MonitorCinCamera)
2. Right-click in Hierarchy → **Cinemachine** → **Cinemachine Camera**
3. Rename it to **"RulesMonitorCinCamera"**
4. Position the camera to have a good view of where your rules screen will be displayed
   - Example position: Adjust X, Y, Z to frame the rules screen nicely
   - Set the camera to focus on the rules display area
5. In the Inspector:
   - Set **Priority** to `10` (base priority, matching others)
   - Configure tracking settings as needed

### 2. Create Rules Monitor UI Panel

1. In the Hierarchy, find your Canvas (where the BrainActivityMonitor UI is)
2. Right-click on Canvas → **UI** → **Panel**
3. Rename it to **"RulesMonitorPanel"**
4. Configure the Panel:
   - Set RectTransform to fill the screen or position as desired

#### 2a. Add Title Text

1. Right-click on RulesMonitorPanel → **UI** → **Text - TextMeshPro**
2. Rename to **"TitleText"**
3. Configure:
   - Set text to "INSTRUCTIONS" or similar
   - Font size: 48-72
   - Alignment: Center
   - Color: Green or cyan (terminal aesthetic)
   - Position at top of panel

#### 2b. Add Content Text

1. Right-click on RulesMonitorPanel → **UI** → **Text - TextMeshPro**
2. Rename to **"RulesContentText"**
3. Configure:
   - Leave text empty (will be filled by script)
   - Font size: 24-32
   - Alignment: Top-Left
   - Enable **Rich Text**
   - Set vertical overflow to **Scroll** or **Truncate** as needed
   - Position in center of panel

#### 2c. Add Continue Button

1. Right-click on RulesMonitorPanel → **UI** → **Button - TextMeshPro**
2. Rename to **"ContinueButton"**
3. Configure:
   - Change button text to "CONTINUE" or "CLOSE"
   - Position at bottom of panel
   - Styling: Match your game's aesthetic
   - Set transition to **Fade** or **Color Tint**

#### 2d. Add Canvas Group (for fading)

1. Select **RulesMonitorPanel**
2. In Inspector, click **Add Component**
3. Add **Canvas Group**
4. Leave settings at default (Alpha: 1, all checkboxes enabled)

### 3. Add Rules Monitor Script

1. In the Hierarchy, create a new empty GameObject
2. Rename it to **"RulesMonitorManager"**
3. In Inspector, click **Add Component**
4. Add the **RulesMonitor** script
5. Configure the component:
   - **Rules Screen Panel**: Drag RulesMonitorPanel here
   - **Title Text**: Drag TitleText here
   - **Rules Content Text**: Drag RulesContentText here
   - **Continue Button**: Drag ContinueButton here
   - **Panel Canvas Group**: Drag RulesMonitorPanel (with Canvas Group) here
   - **Rules Text**: Edit the text area to customize your instructions
   - **Typewriter Speed**: Adjust (default 0.03 = 30ms per character)
   - **Power On Duration**: Adjust (default 1.5s for camera transition)

6. Wire up the **Continue Button**:
   - Select the ContinueButton in Hierarchy
   - In Inspector, find the **Button** component
   - Under **On Click()**, click the **+** button
   - Drag the **RulesMonitorManager** GameObject into the object field
   - Select **RulesMonitor** → **OnContinueClicked()**

### 4. Update Camera Manager

1. In Hierarchy, select your **CameraManager** GameObject
2. In Inspector, find the **Camera Manager** script
3. You should now see a new field: **Rules View Camera**
4. Drag **RulesMonitorCinCamera** into this field

### 5. Create Interactable Screens (3D Objects)

You need 3D objects in your scene that players can click to trigger the camera transitions.

#### 5a. Create Rules Screen Object

1. In Hierarchy, create a **3D Object** → **Quad** (or use existing screen model)
2. Rename to **"RulesScreenInteractable"**
3. Position it in your scene where the rules screen should be
4. Scale appropriately
5. Add a **Collider** if not present:
   - Add Component → **Box Collider**
6. Add the **InteractableScreen** script:
   - Add Component → **Interactable Screen**
   - Configure:
     - **Screen Name**: "Rules Monitor"
     - **Interaction Distance**: 10 (or adjust)
     - **Is Interactable**: ✓ checked
     - **Screen Camera**: Drag **RulesMonitorCinCamera** here (camera switches automatically on click!)
     - **Normal Material**: Assign the screen's default material
     - **Highlight Material**: Assign a glowing/highlighted material (optional)
     - **Screen Renderer**: Drag the Renderer component
     - **On Interact (event)**: Click the **+** button
       - Drag **RulesMonitorManager** into the object field
       - Select **RulesMonitor** → **PowerOn()**

#### 5b. Create Brain Monitor Interactable

1. Similarly, find or create your Brain Activity Monitor screen in the scene
2. Add the **InteractableScreen** script
3. Configure:
   - **Screen Name**: "Brain Monitor"
   - **Interaction Distance**: 10
   - **Screen Camera**: Drag **MonitorCinCamera** here (camera switches automatically on click!)
   - **On Interact (event)**: Click the **+** button
     - Drag your **BrainActivityMonitor** GameObject
     - Select **BrainActivityMonitor** → **PowerOn()**

### 6. Configure Brain Activity Monitor

1. Select your **BrainActivityMonitor** GameObject
2. In Inspector, find the **Brain Activity Monitor** script
3. Locate the **Settings** section
4. **IMPORTANT**: Uncheck **Auto Start Monitoring**
   - This ensures the monitor powers on but waits for manual start
   - Prevents automatic monitoring when camera moves to monitor view

### 7. Layer Setup (Optional but Recommended)

For better raycast performance:

1. Create a new Layer called **"Interactable"**
2. Assign RulesScreenInteractable and BrainMonitorInteractable to this layer
3. In your InteractableScreen scripts, you can add layer filtering if needed

## Testing the Setup

### Test Sequence

1. **Start Play Mode**
2. **Camera should be in Room View** (showing both screens)
3. **Click on the Rules Screen**
   - Camera should smoothly transition to Rules View
   - Text should type out character by character
   - Continue button should appear after text finishes
4. **Click Continue button**
   - Camera should return to Room View
   - Rules panel should fade out and deactivate
5. **Click on the Brain Monitor Screen**
   - Camera should transition to Brain Monitor View
   - Brain monitor should power on
   - Monitoring should NOT auto-start (if Auto Start Monitoring is unchecked)
6. **Press F3 or Start Monitoring button**
   - Monitoring begins with waveforms

### Debug Context Menu Options

You can test individual components without Play mode:

**RulesMonitorManager** (Right-click in Inspector):
- Test Power On
- Test Power Off

**CameraManager** (Right-click in Inspector):
- Force Switch to Room View
- Force Switch to Rules View
- Force Switch to Monitor View

## Customization

### Customize Rules Text

Edit the **Rules Text** field in the RulesMonitor component. You can use basic formatting:
- Line breaks for spacing
- Bullet points with dashes or numbers
- ALL CAPS for emphasis

Example:
```
OPERATION PROTOCOL

OBJECTIVE:
Monitor and control citizen brain activity.

CONTROLS:
- F1: Auto-Setup
- F2: Power On
- F3: Start Monitoring
- F4: Stop Monitoring
```

### Adjust Camera Positions

1. Select each Cinemachine camera
2. Use the Scene view gizmos to position them
3. Test transitions in Play mode
4. Adjust **Blend Duration** in CameraManager if transitions are too fast/slow

### Style the UI

Match the rules monitor UI to your game's aesthetic:
- Apply similar fonts and colors
- Add scanlines, static, or glitch effects
- Use DOTween animations for button interactions

## Troubleshooting

### Camera doesn't switch
- Check that the Screen Camera field is assigned in each InteractableScreen
- Check that CameraManager.Instance is not null
- Verify Priority values are set correctly (base = 10)
- Ensure Cinemachine Brain is on Main Camera

### Can't click screens
- Verify colliders are attached to screen objects
- Check that InteractableScreen script is added
- Make sure screen is within Interaction Distance
- Check Event System exists in scene (required for UI)

### Rules text doesn't appear
- Check that RulesContentText is assigned in RulesMonitor
- Verify the Rules Text field is not empty
- Look for errors in Console

### Continue button doesn't work
- Verify the button's OnClick event is wired to OnContinueClicked()
- Check that the button is active and interactable
- Ensure Event System exists in scene

### Monitor auto-starts when it shouldn't
- Select BrainActivityMonitor GameObject
- In Inspector, uncheck **Auto Start Monitoring** in Settings

## File Reference

New files created:
- `Assets/Scripts/InteractableScreen.cs` - Handles clicking 3D objects with automatic camera switching
- `Assets/Scripts/UI/RulesMonitor.cs` - Manages rules display
- `Assets/Scripts/CameraManager.cs` - *Modified* to support three cameras + flexible camera switching

Modified files:
- `Assets/Scripts/UI/BrainActivityMonitor.cs` - Added auto-start toggle

Key Features:
- **InteractableScreen**: Each screen can have its own Cinemachine camera assigned - clicks automatically switch to that camera
- **CameraManager.MoveToCamera()**: New public method for switching to any Cinemachine camera dynamically

## Next Steps

After setup is complete:
1. Test the full flow multiple times
2. Adjust camera angles for best framing
3. Customize the rules text for your game
4. Add visual polish (effects, animations, sounds)
5. Consider adding keyboard shortcuts (ESC to close rules, etc.)

Enjoy your new two-monitor system!
