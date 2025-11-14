# VR Boat Combat Quest

A native Android VR boat combat game for Meta Quest 2 and 3, built with Unity and C#.

![Unity Version](https://img.shields.io/badge/Unity-2022.3_LTS-blue)
![Platform](https://img.shields.io/badge/Platform-Meta_Quest_2%2F3-green)
![API Level](https://img.shields.io/badge/Android_API-29+-orange)

## 🎮 Game Overview

VR Boat Combat Quest is an immersive VR action game where you command a speedboat, engage in naval combat, and capture enemy vessels using innovative grappling and zipline mechanics.

### Key Features

- **VR Controller Input**: Intuitive steering wheel mechanics with the left controller and weapon aiming with the right
- **Dynamic Boat Physics**: Realistic buoyancy system with wave interaction using Perlin noise ocean simulation
- **Weapon Systems**: Primary cannons with physics-based projectiles and grappling hook with dynamic rope rendering
- **Capture Mechanics**: Grapple and board enemy vessels with zipline traversal system
- **AI Enemy System**: Multiple enemy boat classes with patrol, pursue, attack, and flank behaviors
- **Performance Optimized**: Object pooling, LOD system, and Quest-specific optimizations

## 🛠️ Technical Stack

- **Engine**: Unity 2022.3 LTS
- **VR Framework**: XR Interaction Toolkit with OpenXR
- **Build Target**: Android (IL2CPP backend, API level 29+)
- **Rendering**: Universal Render Pipeline (URP) optimized for mobile
- **Physics**: Custom buoyancy system with Rigidbody-based movement

## 📁 Project Structure

```
VRBoatCombat/
├── Assets/
│   ├── Scenes/              # Game scenes
│   ├── Scripts/
│   │   ├── Core/           # Core game systems (boat physics, VR controller, etc.)
│   │   ├── Weapons/        # Weapon and grapple systems
│   │   ├── AI/             # Enemy AI behaviors
│   │   ├── Managers/       # Game and spawn managers
│   │   ├── Utilities/      # Object pooler, audio manager, etc.
│   │   ├── UI/             # UI management
│   │   └── Editor/         # Unity Editor tools
│   ├── Prefabs/
│   │   ├── Boats/          # Player and enemy boat prefabs
│   │   ├── Weapons/        # Weapon prefabs
│   │   ├── Effects/        # Particle effects
│   │   └── Environment/    # Environment objects
│   ├── Materials/          # Materials and textures
│   ├── Shaders/            # Custom shaders (water, toon)
│   ├── Audio/              # Sound effects and music
│   └── Resources/          # Runtime loaded assets
├── ProjectSettings/        # Unity project settings
└── Packages/              # Package dependencies

```

## 🚀 Quick Start

### Prerequisites

- Unity 2022.3 LTS or newer
- Meta Quest 2 or Quest 3 headset
- SideQuest (for sideloading APK)
- USB cable for connecting Quest to PC

### Installation Steps

1. **Clone/Download** this project to your local machine

2. **Open in Unity**:
   - Launch Unity Hub
   - Click "Open" or "Add"
   - Navigate to the `VRBoatCombat` folder
   - Unity will import all assets (this may take several minutes)

3. **Use Editor Tools** for automated setup:
   - In Unity, go to `Tools > VR Boat Combat > Setup Window`
   - Follow the setup wizard to configure XR, Android build settings, and more

4. **Manual Setup** (if needed):
   - See [SETUP_GUIDE.md](SETUP_GUIDE.md) for detailed step-by-step instructions

5. **Build the Project**:
   - See [BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md) for complete build process

## 📚 Documentation

- **[SETUP_GUIDE.md](SETUP_GUIDE.md)** - Complete setup instructions for beginners
- **[BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md)** - Step-by-step build and deployment guide
- **[NEXT_STEPS.md](NEXT_STEPS.md)** - Post-setup tasks and customization options
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)** - Common issues and solutions

## 🎯 Core Systems Overview

### VR Boat Controller (`VRBoatController.cs`)
Handles VR input for steering and weapons:
- Left controller: Virtual steering wheel with grab mechanics
- Right controller: Weapon aiming and trigger firing
- Haptic feedback for immersion

### Boat Physics (`BoatPhysics.cs`)
Realistic boat movement:
- Rigidbody-based physics
- Multi-point buoyancy system
- Wave interaction via WaveManager
- Momentum preservation

### Weapon System (`WeaponSystem.cs`)
Combat mechanics:
- Projectile spawning with object pooling
- Weak point targeting system
- Recoil physics
- Audio and visual effects

### Grapple Hook (`GrappleHook.cs`)
Innovative traversal:
- Rope physics with dynamic Line Renderer
- Spring joint constraints between boats
- Zipline traversal system

### Enemy AI (`EnemyAI.cs`)
Intelligent opponents:
- State machine: Patrol → Pursue → Attack → Flank
- Difficulty scaling
- Multiple boat classes with unique behaviors

### Game Manager (`GameManager.cs`)
Central game control:
- Score and stat tracking
- Dynamic difficulty adjustment
- Game state management
- Capture event handling

## 🎨 Custom Shaders

### Stylized Water Shader
- Animated waves using vertex displacement
- Fresnel effect for realistic water appearance
- Foam rendering
- Mobile-optimized

### Toon Boat Shader
- Cel-shaded rendering for stylized look
- Outline rendering
- Rim lighting
- Optimized for Quest performance

## 🔧 Editor Tools

Custom Unity Editor menu (`Tools > VR Boat Combat`) provides:
- **One-click Android setup**
- **XR configuration wizard**
- **Scene setup automation**
- **Prefab generation**
- **Quality settings for Quest 2/3**
- **Documentation access**

## 📊 Performance Optimization

### Implemented Optimizations:
- Object pooling for projectiles and effects
- LOD (Level of Detail) system for distant objects
- Occlusion culling
- Fixed foveated rendering support
- Efficient physics calculations
- Mobile-optimized shaders
- Texture atlasing
- Dynamic resolution scaling

### Target Performance:
- **Quest 2**: 72Hz, 1832x1920 per eye
- **Quest 3**: 90Hz, 2064x2208 per eye

## 🎮 Controls

### In VR:
- **Left Grip**: Grab steering wheel
- **Left Stick/Rotation**: Steer boat
- **Left Trigger**: Throttle (push forward)
- **Right Controller Aim**: Aim weapons
- **Right Trigger**: Fire weapons
- **Both Grips (on grapple)**: Initiate zipline traversal

## 🔨 Building for Quest

Quick build process:
1. Open `Tools > VR Boat Combat > Setup Window`
2. Click "Configure Android Build Settings"
3. Click "Set IL2CPP Backend"
4. Go to `File > Build Settings`
5. Click "Build" and save APK
6. Use SideQuest to install APK on Quest

See [BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md) for detailed instructions.

## 🎨 Customization

The project is designed to be easily customizable:

- **Boat models**: Replace primitive cubes in prefabs with your 3D models
- **Weapons**: Add new weapon types by extending `WeaponSystem.cs`
- **Enemies**: Create new AI behaviors by modifying `EnemyAI.cs`
- **Visuals**: Apply custom materials and shaders
- **Game rules**: Modify `GameManager.cs` for different win conditions

## 🐛 Known Issues

- XR packages must be installed manually via Package Manager
- First build may take 10-15 minutes due to IL2CPP compilation
- Oculus/Meta XR SDK recommended for advanced features (hand tracking, etc.)

See [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for solutions.

## 📝 License

This project is provided as-is for educational and development purposes.

## 🤝 Contributing

This is a template project. Feel free to:
- Modify and extend the code
- Add new features
- Optimize performance
- Create your own unique boat combat game!

## 📞 Support

For questions or issues:
1. Check [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
2. Review Unity documentation
3. Check Meta Quest developer documentation

## 🎓 Credits

Built with:
- Unity XR Interaction Toolkit
- OpenXR
- Universal Render Pipeline (URP)
- TextMeshPro

---

**Ready to get started?** Open [SETUP_GUIDE.md](SETUP_GUIDE.md) for step-by-step instructions!
