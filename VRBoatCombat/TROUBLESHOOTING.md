# VR Boat Combat Quest - Troubleshooting Guide

Common issues and solutions for Unity setup, building, and Quest deployment.

## Table of Contents
1. [Unity Setup Issues](#unity-setup-issues)
2. [Package Installation Issues](#package-installation-issues)
3. [XR Configuration Issues](#xr-configuration-issues)
4. [Android Build Issues](#android-build-issues)
5. [IL2CPP Compilation Issues](#il2cpp-compilation-issues)
6. [Quest Connection Issues](#quest-connection-issues)
7. [Runtime Errors](#runtime-errors)
8. [Performance Issues](#performance-issues)
9. [VR-Specific Issues](#vr-specific-issues)
10. [General Tips](#general-tips)

---

## Unity Setup Issues

### Issue: "Cannot open project - Unity version mismatch"

**Symptoms**:
- Unity won't open project
- Version warning appears

**Solutions**:
1. **Install correct Unity version**:
   - Check `ProjectVersion.txt` in ProjectSettings folder
   - Install matching version from Unity Hub
   - Use Unity 2022.3 LTS or newer

2. **Upgrade project** (if using newer Unity):
   - Unity Hub > Projects
   - Right-click project > Select Unity version
   - Choose newer version
   - Open project (Unity will upgrade)

### Issue: "Library folder is locked" or "Unable to parse file"

**Symptoms**:
- Project won't open
- Error about locked files

**Solutions**:
1. **Delete Library folder**:
   - Close Unity completely
   - Navigate to project folder
   - Delete `Library` folder
   - Reopen project (Unity regenerates Library)

2. **Check file permissions**:
   - Ensure you have read/write access
   - Disable antivirus temporarily
   - Move project to non-network drive

### Issue: "Assembly compilation errors"

**Symptoms**:
- Red errors in Console
- Scripts won't compile
- Can't enter Play mode

**Solutions**:
1. **Clear and reimport**:
   - Edit > Preferences > External Tools
   - Click "Regenerate project files"
   - Assets > Reimport All

2. **Check script syntax**:
   - Double-click error to open script
   - Fix syntax errors (missing semicolons, brackets, etc.)
   - Save and wait for recompile

---

## Package Installation Issues

### Issue: "Package resolution failed"

**Symptoms**:
- Packages won't install
- Error in Package Manager
- Project stuck loading

**Solutions**:
1. **Check internet connection**
2. **Clear package cache**:
   - Close Unity
   - Delete: `C:\Users\[Username]\AppData\Local\Unity\cache` (Windows)
   - Delete: `~/Library/Unity/cache` (Mac)
   - Reopen Unity

3. **Manual package installation**:
   - Open `Packages/manifest.json`
   - Add package manually:
     ```json
     "com.unity.xr.management": "4.4.0",
     "com.unity.xr.openxr": "1.9.1"
     ```
   - Save and let Unity reimport

### Issue: "Package version conflict"

**Symptoms**:
- Error about incompatible versions
- Packages won't install together

**Solutions**:
1. **Update all packages**:
   - Package Manager > My Assets
   - Click "Update All"

2. **Resolve dependencies manually**:
   - Read error message for conflicting packages
   - Update or remove conflicting package
   - Install required version

### Issue: "XR Interaction Toolkit samples won't import"

**Symptoms**:
- Can't import starter assets
- Import button grayed out

**Solutions**:
1. **Install XR Interaction Toolkit first**
2. **Import samples**:
   - Package Manager > XR Interaction Toolkit
   - Expand "Samples"
   - Click "Import" for needed samples

---

## XR Configuration Issues

### Issue: "OpenXR not listed in XR Plug-in Management"

**Symptoms**:
- Can't enable OpenXR
- Android tab shows "No loaders available"

**Solutions**:
1. **Install OpenXR package**:
   - Window > Package Manager
   - Search "OpenXR"
   - Install "OpenXR Plugin"

2. **Refresh project**:
   - Close Project Settings
   - Assets > Reimport All
   - Reopen Project Settings

### Issue: "OpenXR warnings and errors"

**Symptoms**:
- Yellow warning triangle in XR settings
- "Fix All" button present

**Solutions**:
1. **Click "Fix All"** button
2. **Manual fixes**:
   - Project Settings > XR Plug-in Management > OpenXR
   - Click each warning
   - Follow instructions

Common warnings:
- **Missing Interaction Profile**: Add "Oculus Touch Controller Profile"
- **Missing Feature Groups**: Enable "Meta Quest Support"
- **Render Mode**: Set to "Multi-pass" or "Single Pass Instanced"

### Issue: "VR not working in Play mode"

**Symptoms**:
- Game runs flat, not in VR
- No headset tracking

**Solutions**:
1. **Check XR Origin setup**:
   - Scene must have XR Origin GameObject
   - Camera Offset and Main Camera present
   - Camera tagged as "MainCamera"

2. **Verify XR enabled**:
   - Project Settings > XR Plug-in Management
   - Standalone tab: Enable OpenXR
   - Initialize XR on Startup: ✅

3. **Link cable mode** (Quest):
   - Enable Oculus Link in Quest
   - Connect Quest to PC
   - Allow data access

---

## Android Build Issues

### Issue: "Android SDK not found"

**Symptoms**:
- Can't switch to Android platform
- "Android SDK/NDK not found" error

**Solutions**:
1. **Install Android Build Support**:
   - Unity Hub > Installs
   - Click ⚙️ next to Unity version
   - Add Modules
   - ✅ Android Build Support
   - ✅ Android SDK & NDK Tools
   - ✅ OpenJDK

2. **Set custom SDK path** (if already installed):
   - Edit > Preferences > External Tools
   - Android SDK: Browse to SDK location
   - Android NDK: Browse to NDK location

### Issue: "Unable to list target platforms"

**Symptoms**:
- Build Settings shows error
- Can't build APK

**Solutions**:
1. **Reinstall Android module** (see above)
2. **Update Android tools**:
   - Preferences > External Tools
   - Android SDK location: Click "Download" links
   - Update to latest versions

### Issue: "Gradle build failed"

**Symptoms**:
- Build fails during Gradle phase
- Error mentions Gradle or dependencies

**Solutions**:
1. **Enable Custom Gradle template**:
   - Project Settings > Player > Publishing Settings
   - ✅ Custom Main Gradle Template
   - ✅ Custom Gradle Properties Template

2. **Clear Gradle cache**:
   - Close Unity
   - Delete: `C:\Users\[Username]\.gradle` (Windows)
   - Delete: `~/.gradle` (Mac)
   - Rebuild

3. **Check internet connection**: Gradle downloads dependencies

4. **Increase JVM heap size**:
   - Edit `gradleTemplate.properties`
   - Add: `org.gradle.jvmargs=-Xmx4096M`

### Issue: "Minimum API level error"

**Symptoms**:
- Build fails: "Minimum API level too low"
- XR requires higher API level

**Solutions**:
1. **Set API level**:
   - Project Settings > Player > Android > Other Settings
   - Minimum API Level: **Android 10.0 (API 29)** or higher
   - Target API Level: **Android 12.0 (API 32)** or higher

---

## IL2CPP Compilation Issues

### Issue: "IL2CPP not found"

**Symptoms**:
- Build fails: "IL2CPP not installed"
- Can't build with IL2CPP backend

**Solutions**:
1. **Install IL2CPP module**:
   - Unity Hub > Installs > Add Modules
   - ✅ Android Build Support > IL2CPP

2. **Or use Mono** (not recommended for Quest):
   - Project Settings > Player > Android
   - Scripting Backend: **Mono**
   - ⚠️ Performance will be worse

### Issue: "IL2CPP build takes forever (>30 minutes)"

**Symptoms**:
- Build hangs at "Building with IL2CPP"
- Very slow builds

**Solutions**:
1. **Enable incremental builds**:
   - Edit > Preferences > External Tools
   - ✅ IL2CPP Incremental Build Support

2. **Use faster compilation**:
   - Build Settings
   - ✅ Development Build (faster IL2CPP)
   - ⚠️ Only for testing, not release

3. **Exclude from antivirus**:
   - Add Unity folders to antivirus exclusions
   - Add project folder to exclusions

4. **Use SSD**: Move project to SSD drive

### Issue: "IL2CPP compilation errors"

**Symptoms**:
- Build fails with C++ errors
- "error: Unknown type name"

**Solutions**:
1. **Update Android NDK**:
   - Unity Hub > Installs > Add Modules
   - Update Android NDK to latest

2. **Check script compatibility**:
   - Some C# features not supported by IL2CPP
   - Avoid: Dynamic code generation, runtime compilation
   - Use AOT-compatible code

---

## Quest Connection Issues

### Issue: "Quest not detected by PC"

**Symptoms**:
- ADB can't find device
- SideQuest shows red dot
- Unity can't connect

**Solutions**:
1. **Enable Developer Mode**:
   - See [BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md)
   - Meta Quest app > Devices > Developer Mode: ON

2. **Allow USB debugging**:
   - Connect Quest to PC
   - Put on headset
   - Popup: "Allow USB debugging?" > ✅ Always allow > OK

3. **Try different USB cable**:
   - Use USB 3.0 cable
   - Direct connection (no USB hub)

4. **Install ADB drivers** (Windows):
   - Download: [Oculus ADB Drivers](https://developer.oculus.com/downloads/package/oculus-adb-drivers/)
   - Install and restart PC

5. **Restart devices**:
   - Restart Quest
   - Restart PC
   - Reconnect

### Issue: "App won't install on Quest"

**Symptoms**:
- "Installation failed" error
- APK won't transfer

**Solutions**:
1. **Check storage space**: Free up space on Quest

2. **Uninstall old version**:
   - Quest: Settings > Apps > Unknown Sources
   - Find app > Uninstall
   - Try installing again

3. **Build with different signature**:
   - Delete old keystore
   - Create new keystore
   - Build new APK

### Issue: "App installed but won't appear"

**Symptoms**:
- Installation succeeds
- App not in Library

**Solutions**:
1. **Check "Unknown Sources"**:
   - Quest Library
   - Filter dropdown (top right)
   - Select "Unknown Sources"
   - App should appear here

2. **Check package name**:
   - Ensure unique package name
   - Different from any existing apps

---

## Runtime Errors

### Issue: "NullReferenceException"

**Symptoms**:
- Game crashes
- Console error: "Object reference not set"

**Solutions**:
1. **Check Inspector assignments**:
   - All serialized fields assigned?
   - No "None" or "Missing" references?

2. **Add null checks in code**:
   ```csharp
   if (myObject != null)
   {
       myObject.DoSomething();
   }
   ```

### Issue: "Missing component errors"

**Symptoms**:
- "GetComponent returned null"
- Features not working

**Solutions**:
1. **Verify component exists**:
   - Check GameObject has required component
   - Add component if missing

2. **Check GameObject hierarchy**:
   - `GetComponent` searches current object
   - `GetComponentInParent` searches up
   - `GetComponentInChildren` searches down

### Issue: "Boat not floating / sinking"

**Symptoms**:
- Boat falls through water
- No buoyancy

**Solutions**:
1. **Check Buoyancy Points**:
   - Boat Physics component
   - Buoyancy Points array populated?
   - Points positioned correctly?

2. **Check Water Level**:
   - Wave Manager: Water Level = 0
   - Buoyancy Points below water surface

3. **Check Physics**:
   - Rigidbody attached
   - Mass set appropriately
   - Not kinematic

### Issue: "VR controllers not working"

**Symptoms**:
- Controllers don't respond
- No tracking

**Solutions**:
1. **Check controller assignments**:
   - VR Boat Controller component
   - Left/Right Controller assigned?

2. **Check XR Input**:
   - XR Origin properly set up
   - Controller GameObjects exist
   - XR Controller components attached

3. **Check Input System**:
   - Project Settings > Player
   - Active Input Handling: "Both" or "Input System Package"

---

## Performance Issues

### Issue: "Low FPS on Quest"

**Symptoms**:
- Stuttering
- Below 72Hz (Quest 2) or 90Hz (Quest 3)

**Solutions**:
1. **Use Quality Settings tool**:
   - Tools > VR Boat Combat > Setup Window
   - Apply Quest 2 or Quest 3 settings

2. **Reduce draw calls**:
   - Combine meshes
   - Use texture atlasing
   - Static batching

3. **Optimize physics**:
   - Reduce Rigidbody count
   - Simplify colliders
   - Increase Fixed Timestep to 0.02

4. **Disable shadows**:
   - Quality Settings
   - Shadows: Disable or Hard Only

### Issue: "High memory usage"

**Symptoms**:
- Quest runs out of memory
- App crashes

**Solutions**:
1. **Compress textures**:
   - All textures: ASTC compression
   - Max size: 1024x1024 (2048 for important)

2. **Use object pooling**:
   - Already implemented
   - Ensure ObjectPooler in scene

3. **Unload unused assets**:
   ```csharp
   Resources.UnloadUnusedAssets();
   ```

4. **Optimize models**:
   - Reduce polygon count
   - Remove unnecessary vertices

---

## VR-Specific Issues

### Issue: "Motion sickness / comfort issues"

**Symptoms**:
- Players feel nauseous
- Discomfort during play

**Solutions**:
1. **Maintain framerate**:
   - Always hit 72Hz (Quest 2) or 90Hz (Quest 3)
   - Even one dropped frame causes nausea

2. **Reduce artificial movement**:
   - Smooth camera transitions
   - No sudden accelerations
   - Add comfort vignette

3. **Add VR comfort options**:
   - Snap turning instead of smooth
   - Teleport movement option
   - Comfort mode in CameraShake

### Issue: "IPD / scale issues"

**Symptoms**:
- World feels wrong size
- Objects too big/small

**Solutions**:
1. **Check XR Origin scale**: Should be (1, 1, 1)
2. **Check unit scale**: 1 Unity unit = 1 meter
3. **Test on actual headset**: Simulator can be misleading

### Issue: "Hand/controller offset"

**Symptoms**:
- Hands don't match controller position
- Grabbing feels wrong

**Solutions**:
1. **Adjust attach points**:
   - XR Grab Interactable
   - Attach Transform: Set offset

2. **Calibrate controller offsets**:
   - Test in VR
   - Adjust visual models
   - Match physical controller position

---

## General Tips

### Best Practices

1. **Save often**: Ctrl+S (Cmd+S on Mac)
2. **Use version control**: Git, Plastic SCM, etc.
3. **Test on device frequently**: Don't just use simulator
4. **Keep backups**: Before major changes
5. **Read Console**: All errors and warnings
6. **Check documentation**: Unity docs, Oculus docs

### When All Else Fails

1. **Restart Unity**
2. **Restart PC**
3. **Delete Library folder** (Unity regenerates)
4. **Reimport all assets**
5. **Start fresh scene** and rebuild
6. **Ask for help**:
   - Unity Forums
   - Stack Overflow
   - Reddit r/Unity3D

### Getting Help

**When asking for help, include**:
1. Unity version
2. Platform (Windows/Mac)
3. Quest model
4. Exact error message
5. What you tried
6. Screenshots/video

---

## Error Code Reference

### Common Unity Error Codes

**CS0246**: Type or namespace not found
- **Fix**: Missing using statement or package

**CS1061**: Does not contain a definition for...
- **Fix**: Check method name, check component exists

**InvalidOperationException**: Object not initialized
- **Fix**: Add null check, ensure initialization

### Common Android Error Codes

**INSTALL_FAILED_UPDATE_INCOMPATIBLE**
- **Fix**: Uninstall old version first

**INSTALL_FAILED_INSUFFICIENT_STORAGE**
- **Fix**: Free up Quest storage

**INSTALL_FAILED_NO_MATCHING_ABIS**
- **Fix**: Enable ARM64 architecture

---

**Still having issues?**

Check:
- [Unity Forums](https://forum.unity.com/)
- [Oculus Developer Forums](https://forums.oculusvr.com/)
- [Unity Documentation](https://docs.unity3d.com/)
- [Oculus Documentation](https://developer.oculus.com/documentation/)

**Or review our other guides**:
- [SETUP_GUIDE.md](SETUP_GUIDE.md)
- [BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md)
- [NEXT_STEPS.md](NEXT_STEPS.md)
