# VR Boat Combat Quest - Build Instructions

Complete guide for building and deploying your game to Meta Quest 2/3.

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Preparing Your Quest Headset](#preparing-your-quest-headset)
3. [Building the APK in Unity](#building-the-apk-in-unity)
4. [Installing SideQuest](#installing-sidequest)
5. [Installing APK on Quest](#installing-apk-on-quest)
6. [Testing on Quest](#testing-on-quest)
7. [Build Optimization Tips](#build-optimization-tips)
8. [Troubleshooting Build Issues](#troubleshooting-build-issues)

---

## Prerequisites

Before building, ensure you have completed:

- ✅ Unity project setup (see [SETUP_GUIDE.md](SETUP_GUIDE.md))
- ✅ Android Build Support installed
- ✅ IL2CPP backend configured
- ✅ Meta Quest 2 or 3 headset
- ✅ USB-C cable (Quest to PC)
- ✅ Meta account (for Developer Mode)

---

## Preparing Your Quest Headset

### Step 1: Enable Developer Mode

1. **Install Meta Quest Mobile App**:
   - Download from App Store (iOS) or Google Play (Android)
   - Log in with your Meta account

2. **Enable Developer Mode**:
   - Open the Meta Quest app on your phone
   - Tap **Menu** (bottom right)
   - Tap **Devices**
   - Select your Quest headset
   - Tap **Headset Settings**
   - Tap **Developer Mode**
   - Toggle **Developer Mode** to **ON**

3. **Create Developer Organization** (if prompted):
   - Go to [developer.oculus.com](https://developer.oculus.com)
   - Click **Create New Organization**
   - Fill in organization name (can be your name)
   - Accept terms and create

4. **Restart Your Quest**:
   - Power off Quest completely
   - Power back on
   - Developer Mode should now be active

### Step 2: Enable USB Debugging

1. Put on your Quest headset
2. Connect Quest to your PC via USB-C cable
3. A popup will appear: **"Allow USB debugging?"**
4. Check **"Always allow from this computer"**
5. Click **"OK"**

---

## Building the APK in Unity

### Quick Build Method

1. In Unity menu: **Tools > VR Boat Combat > Setup Window**
2. Ensure all Android settings are configured (green checkmarks)
3. Close the setup window
4. Go to **File > Build Settings**
5. Click **"Build"**
6. Choose save location (e.g., `Builds` folder)
7. Name your APK: `VRBoatCombat.apk`
8. Click **"Save"**
9. ⏰ **Wait 10-20 minutes** for first build (IL2CPP compilation is slow)

### Detailed Build Method

#### Step 1: Open Build Settings

1. In Unity: **File > Build Settings**
2. Verify **"Android"** is selected
3. Verify scene is in **"Scenes In Build"** list:
   - If empty, click **"Add Open Scenes"**

#### Step 2: Configure Build Settings

1. **Texture Compression**: Set to **"ASTC"** (best quality for Quest)
2. **Development Build**:
   - ✅ Check for testing (enables profiler)
   - ❌ Uncheck for final release
3. **Autoconnect Profiler**: ✅ Check if Development Build is checked

#### Step 3: Verify Player Settings

1. Click **"Player Settings"** in Build Settings window
2. Verify these settings:

**Other Settings**:
- Package Name: `com.vrboatcombat.quest`
- Version: `1.0.0`
- Minimum API Level: `Android 10.0 (API 29)`
- Target API Level: `Android 12.0 (API 32)` or higher
- Scripting Backend: `IL2CPP`
- Target Architectures: `ARM64` only

**Publishing Settings**:
- Create new Keystore (for release builds):
  - Click **"Keystore Manager"**
  - **"Keystore..." > "Create New"**
  - Choose location and password
  - Fill in key details
  - Click **"Add Key"**

#### Step 4: Build

1. Close Player Settings
2. Back in Build Settings, click **"Build"**
3. Create a `Builds` folder if it doesn't exist
4. Save as: `VRBoatCombat.apk`
5. Click **"Save"**

#### Step 5: Monitor Build Progress

- Progress bar shows in Unity bottom-right
- Console shows build steps
- **First build**: 10-20 minutes
- **Subsequent builds**: 5-10 minutes

⚠️ **Common Build Times**:
- IL2CPP compilation: 5-10 minutes
- Asset bundling: 2-3 minutes
- APK creation: 1-2 minutes

---

## Installing SideQuest

SideQuest is the easiest way to install APKs on Quest.

### Step 1: Download SideQuest

1. Go to [sidequestvr.com](https://sidequestvr.com)
2. Click **"Download SideQuest"**
3. Choose your OS:
   - **Windows**: Download `.exe`
   - **Mac**: Download `.dmg`
   - **Linux**: Download `.AppImage`

### Step 2: Install SideQuest

**Windows**:
1. Run `SideQuest-Setup.exe`
2. Follow installation wizard
3. Launch SideQuest

**Mac**:
1. Open `SideQuest.dmg`
2. Drag SideQuest to Applications
3. Launch from Applications
4. If blocked: System Preferences > Security > Allow

**Linux**:
1. Make AppImage executable: `chmod +x SideQuest.AppImage`
2. Run: `./SideQuest.AppImage`

### Step 3: Install ADB Drivers (Windows Only)

1. Connect Quest to PC
2. SideQuest should detect Quest
3. If not detected:
   - Click **"Setup"** in SideQuest
   - Click **"Install Mobile"**
   - Install ADB drivers

---

## Installing APK on Quest

### Method 1: Using SideQuest (Recommended)

#### Step 1: Connect Quest

1. Put on Quest headset
2. Connect Quest to PC via USB
3. Allow USB debugging popup in Quest
4. In SideQuest:
   - Green dot (top left) = Connected ✅
   - Red dot = Not connected ❌

#### Step 2: Install APK

1. In SideQuest, click the **folder icon** (top menu bar)
2. Navigate to your `Builds` folder
3. Select `VRBoatCombat.apk`
4. Click **"Open"**
5. Wait for installation (30-60 seconds)
6. Success message: **"App successfully installed"**

### Method 2: Using ADB Command Line

1. Open Command Prompt (Windows) or Terminal (Mac/Linux)
2. Navigate to your Builds folder:
   ```bash
   cd path/to/VRBoatCombat/Builds
   ```
3. Install APK:
   ```bash
   adb install VRBoatCombat.apk
   ```
4. Wait for: **"Success"** message

### Method 3: Unity Direct Build and Run

1. In Unity Build Settings
2. Quest must be connected
3. Click **"Build And Run"** instead of "Build"
4. Unity builds and auto-installs on Quest

---

## Testing on Quest

### Step 1: Find Your App

1. Put on Quest headset
2. Click **Library** (bottom menu)
3. Click dropdown (top right): **"All"**
4. Select **"Unknown Sources"**
5. Find **"VR Boat Combat Quest"**

### Step 2: Launch Game

1. Click on **"VR Boat Combat Quest"**
2. Click **"Launch"**
3. Game should start!

### Step 3: Test Core Features

Test these features in order:

1. **VR Tracking**:
   - Move your head - camera should follow
   - Move controllers - hands should move

2. **Boat Steering**:
   - Grab steering wheel with left controller
   - Test steering input

3. **Weapons**:
   - Aim with right controller
   - Pull trigger to fire

4. **Physics**:
   - Boat should float on water
   - Check wave interaction

5. **Performance**:
   - Smooth 72Hz (Quest 2) or 90Hz (Quest 3)
   - No stuttering or lag

---

## Build Optimization Tips

### For Smaller APK Size

1. **Texture Compression**:
   - Project Settings > Player > Android
   - Texture Compression: **ASTC**

2. **Asset Optimization**:
   - Compress textures (RGB Compressed DXT1/5)
   - Use low-poly models
   - Optimize audio files (Vorbis compression)

3. **Strip Engine Code**:
   - Player Settings > Other Settings
   - ✅ **Strip Engine Code**
   - **Managed Stripping Level**: **High**

### For Better Performance

1. **Quality Settings**:
   - Use automated tool: **Tools > VR Boat Combat > Setup Window**
   - Click "Apply Quest 2 Quality Settings" or "Quest 3"

2. **Occlusion Culling**:
   - Window > Rendering > Occlusion Culling
   - Mark static objects
   - Bake occlusion data

3. **Object Pooling**:
   - Already implemented in scripts
   - Ensure ObjectPooler is in scene

4. **LOD Groups**:
   - Add LOD components to boat models
   - Set multiple detail levels

### For Faster Builds

1. **Incremental IL2CPP**:
   - Edit > Preferences > External Tools
   - ✅ **IL2CPP Incremental Build Support**

2. **Cache Server** (Team Development):
   - Edit > Preferences > Cache Server
   - Enable Unity Accelerator

---

## Troubleshooting Build Issues

### "Unable to list target platforms"
**Solution**:
- Reinstall Android Build Support
- Unity Hub > Installs > Add Modules

### "IL2CPP error: Failed to compile"
**Solution**:
- Ensure Android NDK is installed
- Unity Hub > Installs > Add Modules > Android NDK

### "Minimum API level too low"
**Solution**:
- Player Settings > Other Settings
- Minimum API Level: At least 29

### "Build failed with errors"
**Solution**:
1. Check Console for specific errors
2. Common fixes:
   - Clear Library folder (close Unity first)
   - Reimport all assets
   - Restart Unity

### "Installation failed: INSTALL_FAILED_UPDATE_INCOMPATIBLE"
**Solution**:
1. Uninstall old version from Quest
2. In Quest: Settings > Apps > Unknown Sources
3. Find app > Uninstall
4. Reinstall new APK

### "Quest not detected in SideQuest"
**Solution**:
1. Enable Developer Mode on Quest
2. Allow USB debugging
3. Restart Quest
4. Try different USB cable
5. Install ADB drivers (Windows)

### Build is very slow (> 30 minutes)
**Solution**:
1. Enable IL2CPP incremental builds
2. Close other applications
3. Exclude Unity/Project folders from antivirus
4. Use SSD for project location

---

## Publishing to Meta Quest Store

Once your game is complete and polished:

1. **Create Developer Account**:
   - [developer.oculus.com](https://developer.oculus.com)

2. **Prepare Submission**:
   - Create signed release build
   - Prepare store assets (screenshots, videos)
   - Write description and metadata

3. **Submit for Review**:
   - Upload APK
   - Fill in store information
   - Submit for approval

4. **Review Process**:
   - Meta reviews app (1-2 weeks)
   - May request changes
   - Approval = published to store!

---

## Quick Reference: Build Checklist

Use this checklist before each build:

**Pre-Build**:
- [ ] All scenes saved
- [ ] No compilation errors
- [ ] Quest connected (for Build & Run)
- [ ] Developer Mode enabled on Quest

**Build Settings**:
- [ ] Android platform selected
- [ ] Scenes added to build
- [ ] Texture compression set (ASTC)
- [ ] IL2CPP backend
- [ ] ARM64 architecture

**After Build**:
- [ ] APK file created successfully
- [ ] APK installed on Quest
- [ ] App appears in Unknown Sources
- [ ] Game launches without errors
- [ ] All features working
- [ ] Performance is smooth

---

**Having issues?** Check [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for solutions!

**Ready to customize?** See [NEXT_STEPS.md](NEXT_STEPS.md) for what to do next!
