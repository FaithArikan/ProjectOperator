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

## Performance Comparison

| Setup | Draw Calls | Performance | Visual Quality |
|-------|-----------|-------------|----------------|
| No material (default) | 1 per waveform | ⚡ Fastest | ⭐⭐⭐⭐⭐ Clean |
| UI/Default material | 1 per waveform | ⚡ Fast | ⭐⭐⭐⭐ Clean |

All options are performant - it's about aesthetics!

## Testing Different Looks

To quickly test if materials improve your visuals:

1. **Run your scene** with default (no material)
2. **Take a screenshot**
4. **Take another screenshot**
5. **Compare** - which looks better?

Most likely, the default will look cleaner and more readable.

## When to Use Materials

**Use NO material when**:
- ✅ You want maximum readability
- ✅ You want crisp, clean lines
- ✅ You're on a tight deadline (game jam!)

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
