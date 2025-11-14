# VR Boat Combat Quest - Complete Setup Guide

This guide will walk you through setting up the VR Boat Combat Quest project from scratch, even if you're a complete beginner to Unity and VR development.

## Table of Contents
1. [System Requirements](#system-requirements)
2. [Installing Unity](#installing-unity)
3. [Opening the Project](#opening-the-project)
4. [Installing Required Packages](#installing-required-packages)
5. [Configuring XR Settings](#configuring-xr-settings)
6. [Configuring Android Build Settings](#configuring-android-build-settings)
7. [Using the Automated Setup Tool](#using-the-automated-setup-tool)
8. [Testing in Unity Editor](#testing-in-unity-editor)
9. [Next Steps](#next-steps)

---

## System Requirements

### Development PC:
- **OS**: Windows 10/11 (64-bit) or macOS
- **Processor**: Intel Core i5 or AMD equivalent
- **RAM**: 16GB minimum (32GB recommended)
- **Graphics**: NVIDIA GTX 1060 or AMD equivalent
- **Storage**: 20GB free space for Unity and project
- **USB**: USB 3.0 port for connecting Quest

### VR Headset:
- Meta Quest 2 or Quest 3
- Latest firmware installed

---

## Installing Unity

### Step 1: Download Unity Hub

1. Go to [https://unity.com/download](https://unity.com/download)
2. Click "Download Unity Hub"
3. Run the installer and follow the installation wizard
4. Launch Unity Hub after installation

### Step 2: Install Unity 2022.3 LTS

1. In Unity Hub, click the **"Installs"** tab on the left
2. Click **"Install Editor"** button (top right)
3. Select **"Unity 2022.3.15f1 LTS"** (or latest 2022.3 LTS version)
4. Click **"Continue"**

### Step 3: Add Android Build Support

⚠️ **IMPORTANT**: You MUST install Android Build Support!

1. In the installation modules, check:
   - ✅ **Android Build Support**
     - ✅ **Android SDK & NDK Tools**
     - ✅ **OpenJDK**
2. Click **"Continue"** and then **"Install"**
3. Wait for installation to complete (10-30 minutes depending on internet speed)

---

## Opening the Project

### Step 1: Locate the Project

1. Find the `VRBoatCombat` folder you downloaded/cloned
2. Verify it contains:
   - `Assets` folder
   - `ProjectSettings` folder
   - `Packages` folder

### Step 2: Add Project to Unity Hub

1. In Unity Hub, click the **"Projects"** tab
2. Click **"Add"** button (top right)
3. Navigate to and select the `VRBoatCombat` folder
4. Click **"Select Folder"**

### Step 3: Open the Project

1. Click on the project in Unity Hub to open it
2. ⏰ **First-time import takes 5-15 minutes** - be patient!
3. You may see warnings about missing packages - this is normal

---

## Installing Required Packages

### Method 1: Using Package Manager (Recommended)

#### Step 1: Open Package Manager

1. In Unity menu: **Window > Package Manager**
2. Wait for package list to load

#### Step 2: Install XR Plugin Management

1. In Package Manager, click the **"+"** button (top left)
2. Select **"Add package by name..."**
3. Enter: `com.unity.xr.management`
4. Click **"Add"**
5. Wait for installation to complete

#### Step 3: Install OpenXR Plugin

1. Click **"+"** again
2. Select **"Add package by name..."**
3. Enter: `com.unity.xr.openxr`
4. Click **"Add"**

#### Step 4: Install XR Interaction Toolkit

1. Click **"+"** again
2. Select **"Add package by name..."**
3. Enter: `com.unity.xr.interaction.toolkit`
4. Click **"Add"**
5. If prompted to install dependencies, click **"Yes"** or **"Install"**

#### Step 5: Install Input System

1. Click **"+"** again
2. Select **"Add package by name..."**
3. Enter: `com.unity.inputsystem`
4. Click **"Add"**
5. If prompted to restart, click **"Yes"**

### Method 2: Using Automated Setup Tool

1. In Unity menu: **Tools > VR Boat Combat > Setup Window**
2. Click **"Configure XR Plugin Management"**
3. Follow the dialog instructions
4. Install packages manually as instructed

---

## Configuring XR Settings

### Step 1: Enable XR Plugin Management

1. In Unity menu: **Edit > Project Settings**
2. Select **"XR Plug-in Management"** in the left sidebar
3. Click the **"Android"** tab (the robot icon)
4. ✅ Check **"OpenXR"**
5. You may see warnings - ignore them for now

### Step 2: Configure OpenXR

1. Still in Project Settings, expand **"XR Plug-in Management"**
2. Click **"OpenXR"** (under Android section)
3. Click the **warning icon** (if present) to fix issues
4. Click **"Fix All"** if a button appears

### Step 3: Add Interaction Profiles

1. In OpenXR settings, scroll to **"Interaction Profiles"**
2. Click the **"+"** button
3. Add **"Oculus Touch Controller Profile"**
4. This enables Quest controller support

### Step 4: Add OpenXR Feature Groups

1. In OpenXR settings, scroll to **"OpenXR Feature Groups"**
2. Enable:
   - ✅ **Meta Quest Support** (if available)
   - ✅ **Hand Tracking** (optional)

---

## Configuring Android Build Settings

### Using Automated Setup (Easiest)

1. In Unity menu: **Tools > VR Boat Combat > Setup Window**
2. Under **"Android Build Settings"**, click:
   - ✅ **"Configure Android Build Settings"**
   - ✅ **"Set IL2CPP Backend"**
   - ✅ **"Configure Graphics API (Vulkan)"**
   - ✅ **"Set Minimum API Level 29"**
3. Click **"OK"** on each success dialog

### Manual Setup (If Automated Fails)

#### Step 1: Switch to Android Platform

1. In Unity menu: **File > Build Settings**
2. Select **"Android"** in platform list
3. Click **"Switch Platform"** (bottom right)
4. ⏰ Wait 5-10 minutes for platform switch

#### Step 2: Configure Player Settings

1. In Build Settings, click **"Player Settings"** (bottom left)
2. This opens Project Settings > Player

#### Step 3: Company and Product Name

1. In **"Company Name"**: Enter your name or company
2. In **"Product Name"**: `VR Boat Combat Quest`

#### Step 4: Package Name

1. Under **"Other Settings"**, find **"Package Name"**
2. Set to: `com.vrboatcombat.quest`
3. Format must be: `com.yourcompany.appname`

#### Step 5: Minimum API Level

1. Under **"Other Settings"**, find **"Minimum API Level"**
2. Set to: **"Android 10.0 (API level 29)"** or higher
3. **Target API Level**: Set to **"Android 12.0 (API level 32)"** or higher

#### Step 6: Scripting Backend

1. Under **"Other Settings"**, find **"Scripting Backend"**
2. Set to: **"IL2CPP"**
3. ⚠️ This is REQUIRED for Quest!

#### Step 7: Target Architectures

1. Under **"Other Settings"**, find **"Target Architectures"**
2. ✅ Check **"ARM64"**
3. ❌ Uncheck other options

#### Step 8: Graphics API

1. Under **"Other Settings"**, find **"Graphics APIs"**
2. If "Auto Graphics API" is checked, uncheck it
3. Remove **"OpenGLES2"** if present (select and click "-")
4. Ensure **"Vulkan"** is at the top of the list
5. If not, add it with "+" button

---

## Using the Automated Setup Tool

The project includes a custom Unity Editor tool to automate most setup tasks.

### Opening the Setup Tool

1. In Unity menu: **Tools > VR Boat Combat > Setup Window**
2. A window will open with multiple sections

### Using Each Section

#### XR Configuration
- Click each button in sequence
- Follow any dialog prompts
- Install packages when requested

#### Android Build Settings
- Click all buttons under this section
- Each configures a specific setting
- Green checkmarks = success

#### Scene Setup
- **"Create Main Game Scene"**: Creates the main gameplay scene
- **"Setup XR Origin in Scene"**: Instructions for adding VR camera
- **"Create Ocean System"**: Adds water and wave manager
- **"Setup Lighting for VR"**: Optimizes lighting for VR

#### Prefab Setup
- Creates template prefabs for:
  - Player boat
  - Enemy boats
  - Projectiles
- These are basic templates - you'll customize them later

#### Optimization Settings
- **Quest 2**: Apply for Quest 2 headset (72Hz)
- **Quest 3**: Apply for Quest 3 headset (90Hz)
- Choose based on your target device

---

## Testing in Unity Editor

### Method 1: Unity XR Device Simulator (No Headset Required)

1. Install **XR Device Simulator** package:
   - **Window > Package Manager**
   - Search for "XR Device Simulator"
   - Click **"Install"**

2. Enable simulator:
   - **GameObject > XR > XR Device Simulator**

3. Press **Play** to test without a headset

### Method 2: Link to Quest (Requires Headset)

1. Enable Developer Mode on Quest:
   - Install Meta Quest mobile app
   - Go to Settings > Developer Mode
   - Enable Developer Mode

2. Connect Quest to PC via USB

3. In Unity, press **Play**
   - Game should run on Quest
   - May require enabling USB debugging on Quest

---

## Next Steps

🎉 **Congratulations! Your project is set up!**

Now proceed to:

1. **[BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md)** - Learn how to build an APK and install on Quest
2. **[NEXT_STEPS.md](NEXT_STEPS.md)** - Customize your game
3. **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)** - Fix common issues

---

## Quick Reference: Installation Checklist

Use this checklist to track your progress:

- [ ] Unity Hub installed
- [ ] Unity 2022.3 LTS installed
- [ ] Android Build Support installed
- [ ] Project opened in Unity
- [ ] XR Plugin Management installed
- [ ] OpenXR Plugin installed
- [ ] XR Interaction Toolkit installed
- [ ] Input System installed
- [ ] XR Plugin Management enabled for Android
- [ ] OpenXR configured
- [ ] Oculus Touch Controller Profile added
- [ ] Android platform selected in Build Settings
- [ ] IL2CPP scripting backend set
- [ ] ARM64 architecture enabled
- [ ] Minimum API Level 29 set
- [ ] Vulkan graphics API configured
- [ ] Package name configured

---

## Common Issues During Setup

### "Missing Project Files" Error
- **Solution**: Ensure you downloaded the complete project with `Assets`, `ProjectSettings`, and `Packages` folders

### "Package Resolution Failed"
- **Solution**: Check internet connection, close and reopen Unity

### "Android NDK Not Found"
- **Solution**: Reinstall Unity with Android Build Support modules

### "IL2CPP Not Found"
- **Solution**: Install "Android SDK & NDK Tools" module in Unity Hub

### Platform Switch Takes Forever
- **Solution**: This is normal for first switch. Can take 10-15 minutes.

---

**Need more help?** Check [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for detailed solutions!
