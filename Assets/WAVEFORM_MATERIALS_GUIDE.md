# Waveform Display Materials Guide

## Quick Answer

**WaveformDisplay components DON'T NEED custom materials to work!**

They use Unity's default UI rendering with dynamically generated textures.

## How WaveformDisplay Works

```
WaveformDisplay.cs
  ↓ Creates
RawImage Component
  ↓ Creates
Texture2D (512x256)
  ↓ Draws to texture using
SetPixel() operations
  ↓ Result
Animated line graphs!
```

No material required - it just works™

## Setup Options

### Option 1: Default (Recommended) ⭐

**Material**: None
**Shader**: Unity's default UI shader
**Performance**: Fastest
**Look**: Clean, crisp waveforms

**Setup**:
1. Nothing to do - WaveformDisplay handles everything
2. Just assign the component to your GameObjects
3. Waveforms will render automatically

### Option 2: Simple Color Tint

**Material**: UI/Default material with color tint
**Use case**: Want to tint all waveforms a certain color

**Setup**:
1. Create → Material → "WaveformBasic"
2. Shader: `UI/Default`
3. Color: White (or desired tint)
4. Assign to each WaveformDisplay's RawImage

### Option 3: CRT Effect on Each Waveform

**Material**: Custom material with CRTMonitor shader
**Use case**: Want scanlines/effects ON the waveforms themselves
**Warning**: Can look busy, use sparingly

**Setup**:

1. **Create the Material**
   - Right-click in `Assets/Materials/`
   - Create → Material
   - Name: "WaveformCRT"

2. **Configure the Material**
   - Shader: `UI/CRTMonitor`
   - Scanline Intensity: 0.1 (subtle)
   - Scanline Count: 300
   - Chromatic Aberration: 0 (don't use)
   - Curvature: 0 (keep flat)
   - Vignette: 0
   - Flicker Intensity: 0.02
   - Brightness: 1.2 (slightly brighter)
   - Contrast: 1.1

3. **Assign to Waveforms**
   - Select each WaveformDisplay GameObject
   - Find the RawImage component
   - Drag WaveformCRT into the Material slot

## Current Project Assets

Your project already has:

✅ **Assets/Shaders/CRTMonitor.shader** - Full CRT effect shader
✅ **Assets/Materials/ScreenMaterial.mat** - Material for main screen (not using CRT shader yet)

## Visual Hierarchy Recommendation

For the best retro look, use CRT effects at different intensities:

```
Main Monitor Screen (CRTScreenEffect)
  ├─ Heavy CRT effect (scanlines, curvature, flicker)
  │
  ├─ Waveform Displays (RawImage with NO material)
  │   └─ Clean, crisp lines that contrast with CRT background
  │
  └─ UI Text (TextMeshPro)
      └─ Also clean, readable against CRT effect
```

This creates **visual depth** - the CRT effect is the "glass" you're looking through, and the waveforms are the "content" displayed on the monitor.

## Performance Comparison

| Setup | Draw Calls | Performance | Visual Quality |
|-------|-----------|-------------|----------------|
| No material (default) | 1 per waveform | ⚡ Fastest | ⭐⭐⭐⭐⭐ Clean |
| UI/Default material | 1 per waveform | ⚡ Fast | ⭐⭐⭐⭐ Clean |
| CRTMonitor material | 1 per waveform | ⚡ Fast | ⭐⭐⭐ Retro (can be busy) |

All options are performant - it's about aesthetics!

## Testing Different Looks

To quickly test if materials improve your visuals:

1. **Run your scene** with default (no material)
2. **Take a screenshot**
3. **Create a CRT material** and assign to ONE waveform
4. **Take another screenshot**
5. **Compare** - which looks better?

Most likely, the default will look cleaner and more readable.

## When to Use Materials

**Use NO material when**:
- ✅ You want maximum readability
- ✅ You want crisp, clean lines
- ✅ The parent screen already has CRT effect
- ✅ You're on a tight deadline (game jam!)

**Use CRT material when**:
- ✅ You want EXTRA retro vibes
- ✅ You don't have a CRT effect on parent screen
- ✅ You have time to tune the effect per waveform
- ✅ Your art direction calls for it specifically

**Use simple color tint when**:
- ✅ You want to color-code different monitor screens
- ✅ You need to distinguish monitor types
- ✅ You want a "broken monitor" effect (red tint)

## Code Integration

If you decide to use materials later, add this to `WaveformDisplay.cs`:

```csharp
[Header("Display Settings")]
[SerializeField]
private Material _waveformMaterial; // Optional material override

private void Awake()
{
    _rectTransform = GetComponent<RectTransform>();

    _displayImage = GetComponent<RawImage>();
    if (_displayImage == null)
    {
        _displayImage = gameObject.AddComponent<RawImage>();
    }

    // Apply custom material if assigned
    if (_waveformMaterial != null)
    {
        _displayImage.material = _waveformMaterial;
    }

    CreateWaveTexture();
}
```

Then you can optionally assign materials per-waveform in the Inspector.

## Existing CRTScreenEffect

Your `BrainActivityMonitor` already uses `CRTScreenEffect.cs` which applies CRT effects to the ENTIRE monitor screen. This is the right approach!

The CRTScreenEffect component should be on:
- The main CRTScreen RawImage (parent of all waveforms)

This gives you the CRT look WITHOUT needing materials on individual waveforms.

## Summary

**TL;DR**:
- **Waveforms need NO material** - they create their own textures
- **CRT effect goes on the parent screen** (CRTScreenEffect component)
- **This separation creates visual depth and readability**
- **Only add materials if you specifically want extra effects**

Your current setup is correct! Just make sure the `CRTScreenEffect` component is configured on the parent RawImage that contains all the waveforms.
