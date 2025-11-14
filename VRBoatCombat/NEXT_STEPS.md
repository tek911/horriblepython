# VR Boat Combat Quest - Next Steps

Congratulations on setting up and building the project! This guide outlines what to do next to customize and enhance your VR boat combat game.

## Table of Contents
1. [Immediate Next Steps](#immediate-next-steps)
2. [Adding 3D Models](#adding-3d-models)
3. [Creating Custom Boats](#creating-custom-boats)
4. [Designing Levels and Environments](#designing-levels-and-environments)
5. [Adding Audio](#adding-audio)
6. [Creating Visual Effects](#creating-visual-effects)
7. [Tuning Gameplay](#tuning-gameplay)
8. [Adding New Features](#adding-new-features)
9. [Performance Optimization](#performance-optimization)
10. [Preparing for Release](#preparing-for-release)

---

## Immediate Next Steps

### 1. Create Your First Scene

The project includes scripts but you need to assemble a playable scene:

**Step-by-step**:
1. Open Unity
2. **Tools > VR Boat Combat > Setup Window**
3. Click **"Create Main Game Scene"**
4. Click **"Create Ocean System"**
5. Click **"Setup Lighting for VR"**

This creates a basic scene with water and lighting.

### 2. Add XR Origin (VR Camera)

**Manual Method**:
1. Right-click in Hierarchy
2. **XR > XR Origin (VR)**
3. This creates the VR camera setup

**What you get**:
- XR Origin (root object)
  - Camera Offset
    - Main Camera (VR camera)
    - Left Controller
    - Right Controller

### 3. Create Player Boat

**Using Editor Tool**:
1. **Tools > VR Boat Combat > Setup Window**
2. Click **"Create Player Boat Prefab Template"**
3. Drag prefab from `Assets/Prefabs/Boats/` into scene
4. Position at: (0, 0, 0)

**Manual Method**:
1. Create cube: **GameObject > 3D Object > Cube**
2. Scale to: (2, 1, 4)
3. Add components:
   - **Add Component > VR Boat Combat > Core > Boat Physics**
   - **Add Component > VR Boat Combat > Core > VR Boat Controller**
   - **Add Component > VR Boat Combat > Core > Health System**
   - Add **Rigidbody** (should auto-add)

4. Tag as **"Player"**

### 4. Link VR Controllers to Boat

1. Select Player Boat
2. In **VR Boat Controller** component:
   - **Left Controller**: Drag XR Origin/LeftController here
   - **Right Controller**: Drag XR Origin/RightController here
   - **Steering Wheel Visual**: Create empty GameObject, add here

### 5. Create Steering Wheel Interactable

1. Create new GameObject as child of Player Boat: `SteeringWheel`
2. Add Component: **XR Grab Interactable**
3. Configure:
   - Movement Type: **Kinematic**
   - Enable **Select** and **Activate**
4. Link to VR Boat Controller:
   - Drag SteeringWheel to **Steering Wheel Interactable** field

### 6. Add Weapon to Boat

1. Create empty GameObject as child of Player Boat: `WeaponMount`
2. Position in front: (0, 0.5, 1)
3. Add Component: **VR Boat Combat > Weapons > Weapon System**
4. Configure:
   - **Projectile Prefab**: Use editor tool to create, then assign
   - **Fire Point**: Create child GameObject, position at barrel end
5. Link to VR Boat Controller:
   - Drag WeaponMount to **Weapon Mount** field

### 7. Create Buoyancy Points

1. Create 4 empty GameObjects as children of Player Boat:
   - `BuoyancyPoint_FL` (Front Left): (-0.8, -0.5, 1.5)
   - `BuoyancyPoint_FR` (Front Right): (0.8, -0.5, 1.5)
   - `BuoyancyPoint_RL` (Rear Left): (-0.8, -0.5, -1.5)
   - `BuoyancyPoint_RR` (Rear Right): (0.8, -0.5, -1.5)

2. In **Boat Physics** component:
   - Set **Buoyancy Points** array size to 4
   - Drag all 4 buoyancy point GameObjects into array

### 8. Test Basic Functionality

1. Press **Play** in Unity Editor
2. If using XR Device Simulator:
   - Use mouse to look around
   - WASD to move (simulated head position)
   - Test controllers with simulator controls

---

## Adding 3D Models

The project uses primitive cubes as placeholders. Replace with real 3D models:

### Finding Free 3D Models

**Free Resources**:
- [Sketchfab](https://sketchfab.com) - Free models (check license)
- [TurboSquid Free](https://www.turbosquid.com/Search/3D-Models/free) - Free section
- [Poly Haven](https://polyhaven.com) - CC0 models
- [Quaternius](http://quaternius.com) - Low-poly game assets

**Recommended Search Terms**:
- "low poly boat"
- "cartoon ship"
- "speedboat"
- "stylized boat"

### Importing Models into Unity

1. **Download Model**:
   - Preferred format: `.fbx` or `.obj`
   - Include textures if available

2. **Import to Unity**:
   - Drag model file into `Assets/Models/` folder
   - Wait for import

3. **Configure Import Settings**:
   - Select model in Project window
   - Inspector > Model tab:
     - Scale Factor: Adjust to fit (usually 1 or 0.01)
     - Generate Colliders: ✅ (if needed)
   - Materials tab:
     - Location: **Use Embedded Materials**
   - Click **"Apply"**

4. **Replace Placeholder**:
   - Drag model into scene
   - Position inside boat prefab
   - Delete primitive cube mesh
   - Keep all scripts and components!

### Optimizing Models for Quest

**Polygon Count**:
- Player boat: < 5,000 triangles
- Enemy boats: < 3,000 triangles
- Projectiles: < 100 triangles
- Environment props: < 1,000 triangles

**Textures**:
- Max size: 1024x1024 (2048x2048 for main objects)
- Format: Compressed (ASTC)
- Combine materials where possible

---

## Creating Custom Boats

### Enemy Boat Variants

Create different enemy types for variety:

**Patrol Boat** (Fast, Low Health):
1. Duplicate enemy boat prefab
2. Adjust in Boat Physics:
   - Max Speed: 25
   - Acceleration: 7
3. Adjust in Health System:
   - Max Health: 50
4. Adjust in Enemy AI:
   - Personality: Aggressive
   - Detection Range: 40

**Gunboat** (Medium, Balanced):
- Max Speed: 20
- Max Health: 100
- Multiple weapon mounts
- Personality: Balanced

**Tanker** (Slow, High Health):
- Max Speed: 10
- Max Health: 300
- Large capture target
- Personality: Defensive

### Creating Player Speedboat Variants

Offer players different boat choices:

1. **Assault Boat**: High speed, multiple weapons
2. **Tank Boat**: Slow, heavy armor
3. **Stealth Boat**: Fast, low detection

---

## Designing Levels and Environments

### Creating Ocean Environment

1. **Skybox**:
   - Window > Rendering > Lighting
   - Skybox Material: Create or import skybox
   - Assets Store: Search "skybox" (many free)

2. **Islands/Obstacles**:
   - Create terrain: **GameObject > 3D Object > Terrain**
   - Paint heightmap for islands
   - Add rocks, debris as obstacles

3. **Fog**:
   - Window > Rendering > Lighting > Environment
   - Fog: ✅ Enable
   - Color: Light blue/gray
   - Distance: 100-200 (creates ocean horizon)

### Spawn Points and Patrol Routes

1. **Create Enemy Spawner**:
   - **GameObject > Create Empty**: Name "Enemy Spawner"
   - **Add Component > VR Boat Combat > Enemy Spawner**

2. **Setup Spawn Points**:
   - Create empty GameObjects around map edge
   - Name: `SpawnPoint_1`, `SpawnPoint_2`, etc.
   - Assign to Enemy Spawner's **Spawn Points** array

3. **Assign Enemy Prefabs**:
   - Create enemy boat prefabs first
   - Add to Enemy Spawner's **Enemy Prefabs** array

---

## Adding Audio

### Required Audio Assets

**Sound Effects Needed**:
- Boat engine loop
- Cannon fire
- Explosion
- Water splash
- Grapple hook launch
- Zipline whoosh
- UI clicks
- Capture complete

**Where to Find Free Audio**:
- [Freesound.org](https://freesound.org)
- [OpenGameArt.org](https://opengameart.org)
- [Zapsplat.com](https://www.zapsplat.com)

### Importing Audio

1. Download audio (`.wav`, `.mp3`, or `.ogg`)
2. Drag into `Assets/Audio/SFX/` or `Assets/Audio/Music/`
3. Select audio clip
4. Inspector settings:
   - **Load Type**: Compressed in Memory (music), Decompress on Load (short SFX)
   - **Compression Format**: Vorbis (best for Quest)
   - **Quality**: 70-80% (balance size/quality)

### Setting Up Audio Manager

1. Create **GameObject > Create Empty**: Name "Audio Manager"
2. **Add Component > VR Boat Combat > Audio Manager**
3. Add sounds to **Sounds** list:
   - Click "+" to add entry
   - Name: "CannonFire"
   - Clip: Drag audio file
   - Volume: 1.0
   - Is 3D: ✅ (for spatial sounds)

### Playing Sounds in Scripts

Already implemented! Example usage:
```csharp
AudioManager.Instance.PlaySound("CannonFire", transform.position);
```

---

## Creating Visual Effects

### Particle Effects Needed

1. **Muzzle Flash** (weapon fire)
2. **Explosion** (hits and deaths)
3. **Water Splash** (impacts)
4. **Engine Wake** (boat trail)
5. **Capture Effect** (glowing particles)

### Creating Simple Particle Effect

1. **GameObject > Effects > Particle System**
2. Name it: `ExplosionEffect`
3. Configure:
   - Duration: 1.0
   - Start Lifetime: 0.5-2.0
   - Start Speed: 5-10
   - Start Size: 0.5-2.0
   - Start Color: Orange/Yellow
   - Emission Rate: 50
   - Shape: Sphere
4. Add sub-emitters for smoke, sparks
5. Save as prefab in `Assets/Prefabs/Effects/`

### Using Free Particle Assets

**Asset Store** (Free):
1. Window > Asset Store
2. Search: "particle effects free"
3. Popular: "Cartoon FX Free", "Particle Effect Pack"
4. Download and import

---

## Tuning Gameplay

### Balancing Boat Physics

Test and adjust these values:

**Boat Physics**:
- **Max Speed**: How fast boat can go (15-25)
- **Acceleration**: How quickly reaches max speed (3-7)
- **Turn Speed**: How fast boat turns (20-40)
- **Buoyancy Force**: How high boat floats (10-20)

**Wave Manager**:
- **Wave Height**: Visual and physics impact (0.5-2.0)
- **Wave Speed**: How fast waves move (0.5-1.5)

### Balancing Combat

**Weapon System**:
- **Fire Rate**: Shots per second (0.5-2.0)
- **Projectile Speed**: How fast projectiles fly (30-60)
- **Damage**: Damage per hit (20-50)

**Health System**:
- Player Health: 100-200
- Enemy Health: 50-300 (varies by type)

### Balancing AI Difficulty

**Enemy AI**:
- **Detection Range**: How far enemies spot player (30-60)
- **Attack Range**: Optimal combat distance (20-40)
- **Fire Accuracy**: Hit chance (0.5-0.9)
- **Difficulty Multiplier**: Overall challenge (1.0-3.0)

---

## Adding New Features

### Feature Ideas

1. **Power-ups**:
   - Speed boost
   - Shield
   - Multi-shot
   - Health pack

2. **Different Weapons**:
   - Machine gun
   - Missiles
   - Torpedo
   - Mines

3. **Weather System**:
   - Storms (higher waves)
   - Fog (reduced visibility)
   - Day/night cycle

4. **Missions/Objectives**:
   - Escort missions
   - Time trials
   - Survival mode
   - Boss battles

5. **Multiplayer** (Advanced):
   - Co-op gameplay
   - PvP combat

### Implementing a Simple Power-up

1. Create new script: `Powerup.cs`
2. Add trigger collider
3. On collision:
   - Modify player stats
   - Play collection sound
   - Spawn particle effect
   - Destroy powerup

---

## Performance Optimization

### Monitoring Performance

1. **Enable Stats** in Game View:
   - Click "Stats" button
   - Monitor FPS, batches, tris

2. **Use Unity Profiler**:
   - Window > Analysis > Profiler
   - Build with "Development Build" enabled
   - Connect to Quest
   - Analyze bottlenecks

### Optimization Checklist

**Rendering**:
- [ ] Static objects marked as Static
- [ ] Occlusion culling baked
- [ ] LOD groups on models
- [ ] Textures compressed (ASTC)
- [ ] Materials use mobile shaders
- [ ] Baked lighting where possible

**Physics**:
- [ ] Rigidbody count minimized
- [ ] Simple colliders (box/sphere) preferred
- [ ] Fixed Timestep set appropriately (0.02)

**Scripts**:
- [ ] Update() loops optimized
- [ ] Object pooling for frequent spawns
- [ ] Coroutines instead of Update when possible
- [ ] Cache component references

**Audio**:
- [ ] Compressed format (Vorbis)
- [ ] Max simultaneous sounds limited
- [ ] 3D audio settings optimized

---

## Preparing for Release

### Polish Checklist

**Game Feel**:
- [ ] Smooth controls
- [ ] Responsive feedback
- [ ] Satisfying sound effects
- [ ] Juicy visual effects
- [ ] Balanced difficulty

**User Experience**:
- [ ] Tutorial/instructions
- [ ] Clear UI
- [ ] Comfort settings for VR
- [ ] Save/load system
- [ ] Options menu

**Quality Assurance**:
- [ ] No crashes
- [ ] No game-breaking bugs
- [ ] Tested on Quest 2 and 3
- [ ] Performance meets targets (72/90 FPS)
- [ ] Proper error handling

### Creating App Store Assets

**Required Assets**:
1. **Icon** (512x512)
2. **Screenshots** (multiple)
3. **Trailer Video** (30-90 seconds)
4. **Description** (compelling copy)
5. **Privacy Policy** (if collecting data)
6. **Age Rating** information

### Final Build

1. Disable all debug features
2. Remove "Development Build" flag
3. Set version number properly
4. Build signed release APK
5. Test thoroughly one final time
6. Submit to Meta Quest Store!

---

## Learning Resources

### Unity VR Development
- [Unity Learn - VR Development](https://learn.unity.com/course/vr-development)
- [Oculus Developer Documentation](https://developer.oculus.com/documentation/unity/)

### C# Programming
- [Microsoft C# Guide](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [Unity Scripting Reference](https://docs.unity3d.com/ScriptReference/)

### Game Design
- ["The Art of Game Design" by Jesse Schell](https://www.schellgames.com/art-of-game-design)
- [Game Developer Magazine](https://www.gamedeveloper.com/)

---

## Community and Support

### Unity Communities
- [Unity Forums](https://forum.unity.com/)
- [r/Unity3D](https://www.reddit.com/r/Unity3D/)
- [Unity Discord](https://discord.com/invite/unity)

### Quest Development
- [Oculus Developer Forums](https://forums.oculusvr.com/)
- [r/OculusQuest](https://www.reddit.com/r/OculusQuest/)

---

## Final Tips

1. **Start Simple**: Get core gameplay working before adding features
2. **Iterate Quickly**: Build often, test on Quest frequently
3. **Get Feedback**: Share with friends, gather playtesting data
4. **Stay Organized**: Keep scenes, prefabs, and scripts organized
5. **Have Fun**: This is your creative project - enjoy the process!

---

**Ready to build something amazing? Get started with your first customization!**

Need help? Check [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for solutions to common issues.
