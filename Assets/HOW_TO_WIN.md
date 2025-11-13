# Neural Wave Bureau - How to Win

## 🎯 The Goal

You are a **Neural Wave Bureau Operator**. Your job is to **stabilize citizens** by matching their target brain wave patterns.

### ✅ WIN Condition
**Reach "STABILIZED" state** - Successfully match the citizen's target brain waves for long enough

### ❌ LOSE Condition
**Reach "CRITICAL FAILURE" state** - Build up too much instability by providing wrong brain waves

---

## 🧠 Understanding the Game

### The Monitor Shows:
1. **5 Brain Wave Bands** (waveform displays):
   - 🔴 **Delta** - Deep sleep waves
   - 🟠 **Theta** - Meditation/creativity
   - 🟢 **Alpha** - Relaxed awareness
   - 🔵 **Beta** - Active thinking
   - 🟣 **Gamma** - High-level processing

2. **Target Lines** (yellow) - Where each wave SHOULD be
3. **Tolerance Zones** (semi-transparent yellow) - Acceptable range
4. **Current Wave Lines** (colored) - Where your input currently is

### Key Metrics:
- **Evaluation Score** (0.0 - 1.0)
  - **≥ 0.75** = Good! (Success threshold)
  - **≤ 0.25** = Bad! (Overload threshold - builds instability)

- **Instability** (0.0 - 1.0)
  - **< 0.8** = Safe
  - **≥ 0.8** = CRITICAL FAILURE!

- **State** - Current citizen condition

---

## 🎮 How to Play

### Step 1: Start Monitoring
1. Press **F1** (if you have QuickMonitorFix script)
2. Or manually: Power On → Start Monitoring
3. Watch the monitor turn on with CRT effects

### Step 2: Read the Target
Look at the **yellow target lines** on each waveform:
- High yellow line = Citizen needs HIGH value for this band
- Low yellow line = Citizen needs LOW value for this band
- Middle = Moderate value needed

### Step 3: Match the Pattern
**Goal**: Make the COLORED lines match the YELLOW target lines!

**Using Keyboard**:
1. **Press 1-5** to select a brain wave band
2. **Look at the target** - is it high or low?
3. **Press ↑↓ and HOLD** to adjust your value
4. **Watch Evaluation Score** - try to keep it above 0.75!

**Quick Method**:
- **Press Q/W/E** to try different presets
- Find one that matches closest to the targets
- Fine-tune with ↑↓ arrows

### Step 4: Maintain Balance
- Keep your waves **inside the tolerance zones** (yellow bands)
- Keep **Evaluation Score ≥ 0.75** for the required time
- Avoid letting score drop below **0.25** (builds instability!)

---

## 📊 State Progression

### 🟢 SUCCESS PATH
```
Idle
  ↓ (Start monitoring)
BeingStimulated
  ↓ (Score ≥ 0.75 for minStimulationTime)
✅ STABILIZED - YOU WIN!
```

### 🔴 FAILURE PATH
```
Idle
  ↓ (Start monitoring)
BeingStimulated
  ↓ (Score ≤ 0.25)
Agitated (instability building!)
  ↓ (Instability ≥ 0.8)
❌ CRITICAL FAILURE - YOU LOSE!
```

### 🟡 RECOVERY PATH
```
Agitated
  ↓ (Improve score quickly!)
Recovering
  ↓ (Wait recoveryTime)
Idle (can try again)
```

---

## 💡 Strategy Tips

### For Beginners:
1. **Start with Q preset** (Ordinary citizen)
2. **Watch which waveforms are furthest from target**
3. **Adjust those first** using number keys + arrows
4. **Don't panic if Agitated** - just improve your score quickly!

### Advanced Tactics:
1. **Study the citizen's profile BEFORE starting**
   - Look at target values in Inspector
   - Plan which preset is closest

2. **Use the Obedience Slider**
   - Higher obedience = more forgiving (wider tolerance)
   - Lower obedience = harder (stricter tolerance)

3. **Monitor the Instability meter**
   - If rising, IMMEDIATELY improve your match
   - Don't let it reach 0.8!

4. **Learn the profiles**:
   - **Ordinary**: Balanced (Alpha + Beta high)
   - **Artist**: Creative (Theta + Alpha high)
   - **Rebel**: Alert/Resistant (Beta + Gamma high)

---

## 🎯 Different Citizen Types

### Easy: Ordinary Citizen
- **Profile**: Ordinary
- **Targets**: [0.1, 0.2, 0.6, 0.6, 0.2]
- **Strategy**: Press Q, then tweak Alpha and Beta
- **Time to stabilize**: ~10-15 seconds

### Medium: Artist
- **Profile**: Artist
- **Targets**: [0.05, 0.4, 0.7, 0.5, 0.3]
- **Strategy**: Press W, high Alpha is critical
- **Time to stabilize**: ~15-20 seconds

### Hard: Rebel
- **Profile**: Rebel
- **Targets**: [0.05, 0.15, 0.3, 0.8, 0.9]
- **Strategy**: Press E, requires HIGH Beta + Gamma
- **Time to stabilize**: ~20-30 seconds
- **Warning**: Very sensitive, instability builds fast!

---

## 📈 Scoring System

### Evaluation Score Breakdown
Your score is calculated by:
1. How close each band is to its target
2. Whether you're within tolerance zones
3. Weighted by band importance (if configured)

**Perfect Match**: All 5 bands exactly on target = 1.0 score
**Partial Match**: Some bands good, some off = 0.5-0.75 score
**Bad Match**: Most bands way off = 0.0-0.25 score

### Time Requirements
- **minStimulationTime** (default: ~5-10 seconds)
  - You must maintain high score for this long
  - Prevents "lucky" instant wins

- **recoveryTime** (default: ~3 seconds)
  - Time to recover from Agitated state
  - Gives you a second chance

---

## 🏆 Win Conditions by Difficulty

### Casual Mode (High Obedience)
- **Obedience**: 80-100%
- **Tolerance**: Very wide (2.5x base)
- **Success Threshold**: 0.55 (easier)
- **Strategy**: Just get close to targets

### Normal Mode (Default)
- **Obedience**: 50%
- **Tolerance**: Normal (1.25x base)
- **Success Threshold**: 0.75
- **Strategy**: Match targets carefully

### Hard Mode (Low Obedience)
- **Obedience**: 0-20%
- **Tolerance**: Very tight (0.5x base)
- **Success Threshold**: 0.95 (nearly perfect)
- **Instability**: Builds 2x faster
- **Strategy**: Pixel-perfect matching required!

---

## ⚠️ Common Mistakes

### ❌ "I'm adjusting but nothing changes!"
- **Problem**: You didn't SELECT a band first
- **Fix**: Press 1-5 FIRST, THEN use ↑↓

### ❌ "Instability keeps rising!"
- **Problem**: Your score is too low
- **Fix**: Look at yellow targets, adjust waves to match

### ❌ "I reached 0.75 but didn't win!"
- **Problem**: Need to maintain it for minStimulationTime
- **Fix**: Hold steady at high score for 5-10 seconds

### ❌ "Too hard to match all 5 bands!"
- **Problem**: Rebel profile or low obedience
- **Fix**:
  1. Increase obedience slider
  2. Use preset closest to target (E for Rebel)
  3. Focus on highest-weight bands first

---

## 🎨 Visual Feedback

### When Winning:
- ✅ Evaluation Score: **GREEN** (≥ 0.75)
- ✅ State shows: **"BeingStimulated"**
- ✅ Waveforms **close to yellow targets**
- ✅ Status Indicator: **CYAN** (good)

### When Losing:
- ❌ Evaluation Score: **RED** (≤ 0.25)
- ❌ Instability rising: **ORANGE → RED**
- ❌ State shows: **"Agitated"**
- ❌ Status Indicator: **YELLOW/RED** (warning)

### When Won:
- 🎉 State: **"STABILIZED"**
- 🎉 Status Indicator: **BRIGHT GREEN**
- 🎉 Waveforms **pulse animation**
- 🎉 Success sound (if FeedbackManager configured)

### When Lost:
- 💥 State: **"CRITICAL FAILURE"**
- 💥 Screen **shake + static effect**
- 💥 Status Indicator: **FLASHING RED**
- 💥 Alarm sound (if FeedbackManager configured)

---

## 🔧 Adjusting Difficulty

### Make It Easier:
1. **Increase Obedience Slider** (in game UI)
2. **Edit NeuralProfile** (Inspector):
   - Increase `bandTolerance` (wider targets)
   - Decrease `instabilityRate` (slower failure)
   - Increase `minStimulationTime` (less time required)

3. **Edit AISettings** (Inspector):
   - Lower `successThreshold` (e.g., 0.5)
   - Raise `overloadThreshold` (e.g., 0.4)
   - Raise `instabilityFailThreshold` (e.g., 0.9)

### Make It Harder:
1. **Decrease Obedience Slider**
2. **Edit NeuralProfile**:
   - Decrease `bandTolerance` (tighter targets)
   - Increase `instabilityRate` (faster failure)
   - Decrease `minStimulationTime` (must be faster)

3. **Edit AISettings**:
   - Raise `successThreshold` (e.g., 0.9)
   - Lower `overloadThreshold` (e.g., 0.15)
   - Lower `instabilityFailThreshold` (e.g., 0.6)

---

## 🎯 Quick Reference Card

```
┌─────────────────────────────────────┐
│  NEURAL WAVE BUREAU - QUICK GUIDE  │
├─────────────────────────────────────┤
│ GOAL: Reach "STABILIZED" state     │
│                                     │
│ HOW:                                │
│  1. Match colored lines to yellow  │
│  2. Keep score ≥ 0.75              │
│  3. Hold for ~10 seconds           │
│                                     │
│ CONTROLS:                           │
│  1-5 = Select band                 │
│  ↑↓  = Adjust (HOLD!)              │
│  Q/W/E = Presets                   │
│                                     │
│ DANGER:                             │
│  Score < 0.25 = Instability!       │
│  Instability ≥ 0.8 = Game Over!    │
│                                     │
│ TIP: Start with Q preset!          │
└─────────────────────────────────────┘
```

---

## 🎮 Practice Challenge

**Beginner Tutorial**:
1. Create Ordinary citizen
2. Set Obedience to 100%
3. Press Q (loads Ordinary preset = perfect match!)
4. Watch automatic win in ~10 seconds
5. Now try manually: Reset, start with random values
6. Adjust each band one by one to match targets

**Intermediate Challenge**:
1. Create Artist citizen
2. Set Obedience to 50%
3. Try to stabilize WITHOUT using presets
4. Use only 1-5 and ↑↓ keys

**Expert Challenge**:
1. Create Rebel citizen
2. Set Obedience to 0%
3. Stabilize in under 15 seconds
4. Without letting instability go above 0.5

---

## 🏅 Achievement Ideas

- **First Contact**: Stabilize any citizen
- **Perfectionist**: Reach 1.0 evaluation score
- **Speed Run**: Stabilize in under 10 seconds
- **No Presets**: Win without using Q/W/E
- **Rebel Tamer**: Stabilize a Rebel citizen
- **Marathon**: Stabilize 3 citizens in a row
- **Zero Instability**: Win without instability rising above 0.1
- **Master Operator**: Stabilize on 0% obedience

Good luck, Operator! The Bureau is counting on you. 🧠✨
