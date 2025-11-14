# Unity 6 Upgrade Guide

## VR Boat Combat Quest - Unity 6000.2.2f1 Migration

This document outlines the complete upgrade from **Unity 2022.3 LTS** to **Unity 6000.2.2f1 (Unity 6)** for the VR Boat Combat Quest project.

---

## Table of Contents

1. [Overview](#overview)
2. [Breaking Changes](#breaking-changes)
3. [Package Updates](#package-updates)
4. [Code Changes](#code-changes)
5. [Shader Updates](#shader-updates)
6. [XR Setup Changes](#xr-setup-changes)
7. [Android Build Configuration](#android-build-configuration)
8. [Migration Steps](#migration-steps)
9. [Known Issues](#known-issues)
10. [Resources](#resources)

---

## Overview

### Why Upgrade to Unity 6?

Unity 6 (formerly Unity 2023 LTS) provides:
- **Improved XR Performance**: Better optimization for Meta Quest 2/3
- **Enhanced Rendering**: URP 17 with improved post-processing and rendering features
- **Better Android Support**: Up to API 36 (Android 16) support
- **Stability**: Long-term support with regular updates
- **New Features**: GPU Resident Drawer, automatic LOD generation, and more

### Version Summary

| Component | Old Version (2022.3 LTS) | New Version (Unity 6) |
|-----------|-------------------------|----------------------|
| Unity Editor | 2022.3.15f1 | 6000.2.2f1 |
| XR Interaction Toolkit | 2.5.2 | 3.2.1 |
| OpenXR | 1.9.1 | 1.14.3 |
| Meta OpenXR Extension | N/A | 2.1.0 (NEW!) |
| Universal RP | 14.0.9 | 17.0.3 |
| Input System | 1.7.0 | 1.11.2 |
| TextMeshPro | 3.0.6 | 4.0.0 |
| Cinemachine | 2.9.7 | 3.1.2 |
| AI Navigation | 1.1.5 | 2.0.4 |

---

## Breaking Changes

### 1. Unity 6 Core API Changes

#### Object Finding Methods (CRITICAL)
```csharp
// ❌ DEPRECATED - Will cause compile errors
GameObject obj = FindObjectOfType<GameObject>();
GameObject[] objs = FindObjectsOfType<GameObject>();

// ✅ NEW API - Use these instead
GameObject obj = FindAnyObjectByType<GameObject>();
GameObject[] objs = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
// Or for sorted results:
GameObject[] objs = FindObjectsByType<GameObject>(FindObjectsSortMode.InstanceID);
```

**All 12 usages in the project have been updated.**

#### Graphics Format Changes
```csharp
// ❌ OBSOLETE - Will cause compile errors
GraphicsFormat.DepthAuto
GraphicsFormat.ShadowAuto
GraphicsFormat.VideoAuto

// ✅ Use GraphicsFormat.None or specific formats instead
```

### 2. XR Interaction Toolkit 3.0+ Breaking Changes

#### Deprecated Controller Classes
```csharp
// ❌ DEPRECATED in XRI 3.0
[SerializeField] private XRController leftController;
[SerializeField] private ActionBasedController rightController;

// ✅ NEW APPROACH - Use Transform references with TrackedPoseDriver
[SerializeField] private Transform leftControllerTransform;
[SerializeField] private Transform rightControllerTransform;
```

**Changes Made:**
- `VRBoatController.cs`: Updated to use Transform references instead of XRController
- Added namespace: `using UnityEngine.XR.Interaction.Toolkit.Interactables;`
- Controller transforms now driven by TrackedPoseDriver (Input System) component

#### LocomotionSystem Deprecated
```csharp
// ❌ DEPRECATED
LocomotionSystem locomotionSystem;

// ✅ NEW
LocomotionMediator locomotionMediator;
```

**Note:** This project doesn't use locomotion components, so no changes were needed.

### 3. URP 17 Shader Changes

#### Surface Shaders Not Supported
URP does not support CG surface shaders. All custom shaders have been converted to HLSL.

```hlsl
// ❌ OLD (Surface Shader with CG)
CGPROGRAM
#pragma surface surf Standard
// ...
ENDCG

// ✅ NEW (HLSL with URP)
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
// ...
ENDHLSL
```

#### Required Shader Changes
1. `CGPROGRAM` → `HLSLPROGRAM`
2. `ENDCG` → `ENDHLSL`
3. Add `"RenderPipeline"="UniversalPipeline"` tag
4. Wrap properties in `CBUFFER_START(UnityPerMaterial)` for SRP Batcher compatibility
5. Update includes to URP shader libraries

**Shaders Updated:**
- `ToonBoat.shader`: Converted from surface shader to URP forward pass
- `StylizedWater.shader`: Converted to URP with vertex displacement

### 4. Android Build Changes

#### Gradle and Tools Updates
```
Old (2022.3 LTS):
- Gradle: 7.x
- AGP: 7.x
- NDK: r23c
- JDK: 11

New (Unity 6):
- Gradle: 8.11
- AGP: 8.7.2
- NDK: r27c
- JDK: 17
```

#### Android API Support
```
Old: API 29-32 (Android 10-12)
New: API 29-36 (Android 10-16)
```

---

## Package Updates

### Core XR Packages

```json
{
  "dependencies": {
    // XR Packages
    "com.unity.xr.openxr": "1.14.3",           // Was: 1.9.1
    "com.unity.xr.meta-openxr": "2.1.0",       // NEW! Meta-specific features
    "com.unity.xr.interaction.toolkit": "3.2.1", // Was: 2.5.2 (BREAKING)
    "com.unity.xr.management": "4.5.0",        // Was: 4.4.0

    // Rendering
    "com.unity.render-pipelines.universal": "17.0.3", // Was: 14.0.9
    "com.unity.render-pipelines.core": "17.0.3",      // Was: 14.0.9
    "com.unity.shadergraph": "17.0.3",               // Was: 14.0.9

    // Input & UI
    "com.unity.inputsystem": "1.11.2",        // Was: 1.7.0
    "com.unity.textmeshpro": "4.0.0",         // Was: 3.0.6

    // Tools
    "com.unity.cinemachine": "3.1.2",         // Was: 2.9.7
    "com.unity.probuilder": "6.0.3",          // Was: 5.2.2
    "com.unity.ai.navigation": "2.0.4",       // Was: 1.1.5
    "com.unity.visualscripting": "1.9.4"      // Was: 1.9.1
  }
}
```

### Removed Packages

```json
// ❌ REMOVED - URP 17 has built-in post-processing
"com.unity.postprocessing": "3.2.2"
```

---

## Code Changes

### Files Modified

#### 1. Core Scripts
- **VRBoatController.cs** (Lines 1-322)
  - Replaced `XRController` with `Transform` references
  - Added XRI 3.0 namespace
  - Updated documentation comments

#### 2. Managers & Utilities (12 files)
- **CaptureSystem.cs**: `FindObjectOfType` → `FindAnyObjectByType`
- **AudioManager.cs**: `FindObjectOfType` → `FindAnyObjectByType`
- **EnemyAI.cs**: `FindObjectOfType` → `FindAnyObjectByType`
- **WeaponSystem.cs**: `FindObjectOfType` → `FindAnyObjectByType`
- **EnemySpawner.cs**: `FindObjectOfType` → `FindAnyObjectByType`
- **Projectile.cs**: `FindObjectOfType` → `FindAnyObjectByType` (2 occurrences)
- **BoatPhysics.cs**: `FindObjectOfType` → `FindAnyObjectByType`
- **GameManager.cs**: `FindObjectOfType` → `FindAnyObjectByType` (2 occurrences)

#### 3. Editor Tools
- **VRBoatCombatEditorTools.cs** (Lines 1-600)
  - Updated documentation with Unity 6 requirements
  - Updated dialog messages for XR setup
  - Updated Android API level targets
  - Added notes about XRI 3.0 breaking changes

### Example Migration Pattern

```csharp
// BEFORE (Unity 2022.3 LTS)
using UnityEngine.XR.Interaction.Toolkit;

public class VRBoatController : MonoBehaviour
{
    [SerializeField] private XRController leftController;
    [SerializeField] private XRController rightController;

    private void Update()
    {
        if (rightController == null) return;
        Quaternion controllerRotation = rightController.transform.rotation;
    }
}

// AFTER (Unity 6)
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRBoatController : MonoBehaviour
{
    [Tooltip("Transform of left controller (use XR Origin > Camera Offset > Left Controller)")]
    [SerializeField] private Transform leftControllerTransform;

    [Tooltip("Transform of right controller (use XR Origin > Camera Offset > Right Controller)")]
    [SerializeField] private Transform rightControllerTransform;

    private void Update()
    {
        if (rightControllerTransform == null) return;
        Quaternion controllerRotation = rightControllerTransform.rotation;
    }
}
```

---

## Shader Updates

### ToonBoat.shader

**Changes:**
1. Converted from CG surface shader to URP forward pass
2. Split into two passes: Outline + Toon Shading
3. Added `"RenderPipeline"="UniversalPipeline"` tag
4. Updated all includes to URP shader library
5. Wrapped properties in CBUFFER for SRP Batcher
6. Added fog support using `MixFog()`

**Before:** 142 lines of CG surface shader
**After:** 192 lines of HLSL URP shader

### StylizedWater.shader

**Changes:**
1. Converted from CG surface shader to URP forward pass
2. Added `"RenderPipeline"="UniversalPipeline"` tag
3. Updated water wave displacement to use URP functions
4. Added proper lighting integration with `GetMainLight()`
5. Implemented specular highlights manually
6. Added fog support

**Before:** 140 lines of CG surface shader
**After:** 179 lines of HLSL URP shader

---

## XR Setup Changes

### Old Setup (Unity 2022.3 LTS + XRI 2.5)

1. Install OpenXR Plugin (1.9.1)
2. Install XR Interaction Toolkit (2.5.2)
3. Enable Oculus Touch Controller Profile
4. Use XRController and ActionBasedController components

### New Setup (Unity 6 + XRI 3.2)

1. **Install Packages:**
   - OpenXR Plugin (1.14.3+)
   - **Unity OpenXR: Meta (2.1.0+)** ← NEW!
   - XR Interaction Toolkit (3.2.1+)

2. **Configure OpenXR:**
   - Enable "Meta Quest Support" feature group
   - Add "Meta Quest Touch Pro Controller Profile"
   - Enable "Meta Quest: Meta" extension features

3. **XR Origin Setup:**
   - Create XR Origin (VR) from hierarchy
   - Add TrackedPoseDriver (Input System) to controllers
   - Assign controller Transforms (not XRController components)

4. **Meta XR SDK:**
   - Unity 6 requires Meta XR SDK v74 or later
   - OpenXR is the only supported backend for new features

### Required Scene Changes

```
XR Origin (VR)
├── Camera Offset
│   ├── Main Camera (XR Origin Camera)
│   ├── Left Controller
│   │   └── TrackedPoseDriver (Input System) ← Add this
│   └── Right Controller
│       └── TrackedPoseDriver (Input System) ← Add this
├── Locomotion
│   └── LocomotionMediator ← Use this instead of LocomotionSystem
```

**Important:** Reassign controller Transform references in VRBoatController.cs after updating XR Origin!

---

## Android Build Configuration

### Player Settings

```csharp
// Minimum & Target API
PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29; // Android 10
PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34; // Android 14

// Scripting Backend
PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

// Graphics API
PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] {
    UnityEngine.Rendering.GraphicsDeviceType.Vulkan,
    UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3
});
```

### Gradle Configuration

Unity 6 automatically handles:
- Gradle 8.11
- Android Gradle Plugin (AGP) 8.7.2
- NDK r27c
- JDK 17

**No manual gradle.properties or build.gradle edits needed for basic projects.**

---

## Migration Steps

### Step 1: Backup Your Project

```bash
# Create a backup before upgrading
cp -r VRBoatCombat VRBoatCombat_Unity2022_Backup
```

### Step 2: Upgrade Unity Editor

1. Install Unity 6000.2.2f1 from Unity Hub
2. Add Android Build Support module
3. Open the project with Unity 6
   - Unity will automatically upgrade project files
   - This may take 5-10 minutes

### Step 3: Update Packages

1. Unity will prompt to update packages automatically
2. **Or manually via Package Manager:**
   - Open `Packages/manifest.json`
   - Replace with updated versions (see Package Updates section)
   - Unity will resolve dependencies

3. **Verify XR Packages:**
   - Window > Package Manager
   - Confirm all XR packages installed
   - Install "Unity OpenXR: Meta" if missing

### Step 4: Update Scripts

**All scripts have already been updated in this branch!**

If starting fresh:
1. Search for `FindObjectOfType` → Replace with `FindAnyObjectByType`
2. Search for `FindObjectsOfType` → Replace with `FindObjectsByType`
3. Update XRController references to Transform references
4. Add new XRI 3.0 namespaces where needed

### Step 5: Update Shaders

**All shaders have already been converted in this branch!**

If you have custom shaders:
1. Open each `.shader` file
2. Add `"RenderPipeline"="UniversalPipeline"` tag
3. Convert CGPROGRAM to HLSLPROGRAM
4. Update includes to URP shader libraries
5. Wrap properties in CBUFFER blocks

### Step 6: Update XR Origin in Scene

1. Open your main scene
2. **If you have an old XR Rig:**
   - Delete existing XR Rig
   - Right-click Hierarchy > XR > XR Origin (VR)
   - This creates Unity 6-compatible XR Origin

3. **Add TrackedPoseDriver:**
   - Select Left Controller game object
   - Add Component > Tracked Pose Driver (Input System)
   - Set Tracking Type: "Rotation and Position"
   - Set Update Type: "Update And Before Render"
   - Repeat for Right Controller

4. **Update VRBoatController references:**
   - Select your boat controller object
   - Drag Left Controller transform to `leftControllerTransform`
   - Drag Right Controller transform to `rightControllerTransform`

### Step 7: Configure XR Plugin Management

1. Edit > Project Settings > XR Plug-in Management
2. **Android Tab:**
   - Enable "OpenXR"
3. **OpenXR Settings (Android):**
   - Interaction Profiles: Add "Meta Quest Touch Pro Controller Profile"
   - OpenXR Feature Groups: Enable "Meta Quest Support"
   - Enable "Meta Quest: Meta" extension features

### Step 8: Test Build

1. File > Build Settings
2. Platform: Android
3. Run Device: Meta Quest 2 or 3
4. Build And Run

**Expected Compile Time:** First build may take 15-30 minutes with IL2CPP

---

## Known Issues

### Issue 1: XRController Assignment Errors

**Symptom:**
```
NullReferenceException: Object reference not set to an instance of an object
VRBoatController.ProcessWeaponAiming()
```

**Solution:**
Reassign controller Transform references in Inspector after upgrading XR Origin.

### Issue 2: Shader Compile Errors

**Symptom:**
```
Shader error in 'VRBoatCombat/ToonBoat': invalid subscript 'rgb'
```

**Solution:**
Ensure all shaders use HLSL syntax. Check that `.shader` files have been updated to URP 17.

### Issue 3: Missing Meta OpenXR Package

**Symptom:**
```
Meta Quest features not working in Unity 6
```

**Solution:**
Install "Unity OpenXR: Meta" package via Package Manager:
```
com.unity.xr.meta-openxr@2.1.0
```

### Issue 4: Input System Warnings

**Symptom:**
```
InputSystem: Device command 'QueryCanRunInBackground' not supported for device 'Quest'
```

**Solution:**
This is a known Unity warning and can be safely ignored. It doesn't affect functionality.

---

## Resources

### Official Documentation

- [Unity 6 Upgrade Guide](https://docs.unity3d.com/6000.0/Documentation/Manual/UpgradeGuideUnity6.html)
- [XR Interaction Toolkit 3.0](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.0/manual/index.html)
- [XRI 3.0 Upgrade Guide](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.0/manual/upgrade-guide-3.0.html)
- [URP 17 Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/manual/index.html)
- [Meta Quest Unity Development](https://developers.meta.com/horizon/documentation/unity/unity-xr-plugin/)

### Key Changes Documentation

- [What's New in Unity 6](https://docs.unity3d.com/6000.0/Documentation/Manual/WhatsNewUnity6.html)
- [Unity 6.2 Release Notes](https://docs.unity3d.com/6000.2/Documentation/Manual/WhatsNewUnity62.html)
- [OpenXR Meta Documentation](https://docs.unity3d.com/Packages/com.unity.xr.meta-openxr@1.0/manual/index.html)

### Mindport XRI 3.0 Migration Guide

- [XRI 3.0 Migration Guide by Mindport](https://www.mindport.co/blog-articles/xri-3-0-migration-guide)

---

## Summary of Changes

### Files Modified: 19

**Project Files:**
- `ProjectSettings/ProjectVersion.txt`: Updated to Unity 6000.2.2f1
- `Packages/manifest.json`: Updated all package versions

**C# Scripts (13 files):**
- `VRBoatController.cs`: XRI 3.0 compatibility
- `CaptureSystem.cs`: API updates
- `AudioManager.cs`: API updates
- `EnemyAI.cs`: API updates
- `WeaponSystem.cs`: API updates
- `EnemySpawner.cs`: API updates
- `Projectile.cs`: API updates
- `BoatPhysics.cs`: API updates
- `GameManager.cs`: API updates
- `VRBoatCombatEditorTools.cs`: Unity 6 compatibility

**Shaders (2 files):**
- `ToonBoat.shader`: Converted to URP 17 HLSL
- `StylizedWater.shader`: Converted to URP 17 HLSL

**Documentation:**
- `UNITY6_UPGRADE_GUIDE.md`: This file (NEW)
- `README.md`: Will be updated

### Lines of Code Changed: ~450

- Scripts: ~50 lines (mostly API updates)
- Shaders: ~350 lines (complete rewrites)
- Editor Tools: ~50 lines (documentation and dialogs)

---

## Support

For issues specific to this project, please:
1. Check the [Known Issues](#known-issues) section
2. Review the [TROUBLESHOOTING.md](TROUBLESHOOTING.md) guide
3. Verify all package versions match this guide
4. Check Unity Console for specific error messages

For Unity 6 or XR Interaction Toolkit issues:
- [Unity Forums](https://forum.unity.com/)
- [Unity Discussions](https://discussions.unity.com/)
- [Meta Quest Developer Forums](https://communityforums.atmeta.com/)

---

**Last Updated:** 2025-11-14
**Unity Version:** 6000.2.2f1
**Project Version:** 1.0.0 (Unity 6)
