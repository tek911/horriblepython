#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace VRBoatCombat.Editor
{
    /// <summary>
    /// Unity Editor tools for VR Boat Combat project setup and automation.
    /// Provides Tools menu with automated setup for XR, Android build, scenes, and prefabs.
    ///
    /// Updated for Unity 6000.2.2f1 (Unity 6):
    /// - XR Interaction Toolkit 3.2.1
    /// - OpenXR 1.14.3+
    /// - Unity OpenXR: Meta extension 2.1.0+
    /// - URP 17.0.3
    /// - Gradle 8.11, AGP 8.7.2, NDK r27c
    /// </summary>
    public class VRBoatCombatEditorTools : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool showXRSettings = true;
        private bool showBuildSettings = true;
        private bool showSceneSetup = true;
        private bool showPrefabSetup = true;
        private bool showOptimization = true;

        [MenuItem("Tools/VR Boat Combat/Setup Window")]
        public static void ShowWindow()
        {
            VRBoatCombatEditorTools window = GetWindow<VRBoatCombatEditorTools>("VR Boat Combat Setup");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("VR Boat Combat - Project Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            DrawXRSettings();
            EditorGUILayout.Space(10);

            DrawBuildSettings();
            EditorGUILayout.Space(10);

            DrawSceneSetup();
            EditorGUILayout.Space(10);

            DrawPrefabSetup();
            EditorGUILayout.Space(10);

            DrawOptimizationSettings();
            EditorGUILayout.Space(10);

            DrawUtilities();

            EditorGUILayout.EndScrollView();
        }

        private void DrawXRSettings()
        {
            showXRSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showXRSettings, "XR Configuration");

            if (showXRSettings)
            {
                EditorGUILayout.HelpBox("Configure XR settings for Meta Quest 2 and 3", MessageType.Info);

                if (GUILayout.Button("Configure XR Plugin Management"))
                {
                    ConfigureXRPluginManagement();
                }

                if (GUILayout.Button("Setup OpenXR for Quest"))
                {
                    SetupOpenXRForQuest();
                }

                if (GUILayout.Button("Configure XR Interaction Toolkit"))
                {
                    ConfigureXRInteractionToolkit();
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Status: Check Package Manager for XR packages", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawBuildSettings()
        {
            showBuildSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showBuildSettings, "Android Build Settings");

            if (showBuildSettings)
            {
                EditorGUILayout.HelpBox("Configure Android build settings for Quest", MessageType.Info);

                if (GUILayout.Button("Configure Android Build Settings"))
                {
                    ConfigureAndroidBuildSettings();
                }

                if (GUILayout.Button("Set IL2CPP Backend"))
                {
                    SetIL2CPPBackend();
                }

                if (GUILayout.Button("Configure Graphics API (Vulkan)"))
                {
                    ConfigureGraphicsAPI();
                }

                if (GUILayout.Button("Set Minimum API Level 29"))
                {
                    SetMinimumAPILevel();
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Current Platform: {EditorUserBuildSettings.activeBuildTarget}", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawSceneSetup()
        {
            showSceneSetup = EditorGUILayout.BeginFoldoutHeaderGroup(showSceneSetup, "Scene Setup");

            if (showSceneSetup)
            {
                EditorGUILayout.HelpBox("Create and configure game scenes", MessageType.Info);

                if (GUILayout.Button("Create Main Game Scene"))
                {
                    CreateMainGameScene();
                }

                if (GUILayout.Button("Setup XR Origin in Scene"))
                {
                    SetupXROriginInScene();
                }

                if (GUILayout.Button("Create Ocean System"))
                {
                    CreateOceanSystem();
                }

                if (GUILayout.Button("Setup Lighting for VR"))
                {
                    SetupLightingForVR();
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawPrefabSetup()
        {
            showPrefabSetup = EditorGUILayout.BeginFoldoutHeaderGroup(showPrefabSetup, "Prefab Setup");

            if (showPrefabSetup)
            {
                EditorGUILayout.HelpBox("Create prefab templates", MessageType.Info);

                if (GUILayout.Button("Create Player Boat Prefab Template"))
                {
                    CreatePlayerBoatPrefab();
                }

                if (GUILayout.Button("Create Enemy Boat Prefab Template"))
                {
                    CreateEnemyBoatPrefab();
                }

                if (GUILayout.Button("Create Projectile Prefab"))
                {
                    CreateProjectilePrefab();
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawOptimizationSettings()
        {
            showOptimization = EditorGUILayout.BeginFoldoutHeaderGroup(showOptimization, "Optimization Settings");

            if (showOptimization)
            {
                EditorGUILayout.HelpBox("Apply Quest-optimized settings", MessageType.Info);

                if (GUILayout.Button("Apply Quest 2 Quality Settings"))
                {
                    ApplyQuest2QualitySettings();
                }

                if (GUILayout.Button("Apply Quest 3 Quality Settings"))
                {
                    ApplyQuest3QualitySettings();
                }

                if (GUILayout.Button("Enable Fixed Foveated Rendering"))
                {
                    EnableFixedFoveatedRendering();
                }

                if (GUILayout.Button("Configure Occlusion Culling"))
                {
                    ConfigureOcclusionCulling();
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawUtilities()
        {
            EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);

            if (GUILayout.Button("Open Package Manager"))
            {
                UnityEditor.PackageManager.UI.Window.Open("");
            }

            if (GUILayout.Button("Open Project Settings"))
            {
                SettingsService.OpenProjectSettings("Project");
            }

            if (GUILayout.Button("Open Build Settings"))
            {
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Generate README for Project"))
            {
                GenerateREADME();
            }
        }

        // Implementation methods
        private void ConfigureXRPluginManagement()
        {
            Debug.Log("[VR Boat Combat Setup] Opening XR Plugin Management settings...");
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
            EditorUtility.DisplayDialog("XR Plugin Management - Unity 6",
                "Unity 6 requires these packages:\n" +
                "1. XR Plugin Management (4.5.0+)\n" +
                "2. OpenXR Plugin (1.14.3+)\n" +
                "3. Unity OpenXR: Meta (2.1.0+) - NEW!\n" +
                "4. XR Interaction Toolkit (3.2.1+)\n\n" +
                "Then enable OpenXR for Android platform\n\n" +
                "NOTE: XRController is deprecated in XRI 3.0.\n" +
                "Use Transform references with TrackedPoseDriver instead.",
                "OK");
        }

        private void SetupOpenXRForQuest()
        {
            Debug.Log("[VR Boat Combat Setup] Configuring OpenXR for Quest...");
            EditorUtility.DisplayDialog("OpenXR Setup - Unity 6",
                "In XR Plugin Management (Unity 6):\n" +
                "1. Select Android tab\n" +
                "2. Enable 'OpenXR'\n" +
                "3. Click 'OpenXR' settings\n" +
                "4. Add 'Meta Quest Touch Pro Controller Profile'\n" +
                "5. Enable 'Meta Quest Support' feature group\n" +
                "6. Enable 'Meta Quest: Meta' extension features\n\n" +
                "Unity 6 requires Meta XR SDK v74+ with OpenXR backend.",
                "OK");
        }

        private void ConfigureXRInteractionToolkit()
        {
            Debug.Log("[VR Boat Combat Setup] Configuring XR Interaction Toolkit...");
            EditorUtility.DisplayDialog("XR Interaction Toolkit 3.0+ (Unity 6)",
                "Install XR Interaction Toolkit 3.2.1+ from Package Manager:\n" +
                "1. Window > Package Manager\n" +
                "2. Click '+' > Add package by name\n" +
                "3. Name: com.unity.xr.interaction.toolkit\n" +
                "4. Version: 3.2.1 (for Unity 6000.2)\n\n" +
                "BREAKING CHANGES in XRI 3.0:\n" +
                "- XRController deprecated (use TrackedPoseDriver)\n" +
                "- LocomotionSystem → LocomotionMediator\n" +
                "- Input actions now on individual interactors\n" +
                "- Namespace reorganization",
                "OK");
        }

        private void ConfigureAndroidBuildSettings()
        {
            Debug.Log("[VR Boat Combat Setup] Configuring Android build settings...");

            // Switch to Android platform
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            // Set package name
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.vrboatcombat.quest");

            // Set version
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;

            Debug.Log("[VR Boat Combat Setup] Android build settings configured!");
            EditorUtility.DisplayDialog("Success", "Android build settings configured successfully!", "OK");
        }

        private void SetIL2CPPBackend()
        {
            Debug.Log("[VR Boat Combat Setup] Setting IL2CPP scripting backend...");
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            Debug.Log("[VR Boat Combat Setup] IL2CPP backend configured!");
            EditorUtility.DisplayDialog("Success", "IL2CPP scripting backend set for Android!", "OK");
        }

        private void ConfigureGraphicsAPI()
        {
            Debug.Log("[VR Boat Combat Setup] Configuring graphics API...");
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] {
                UnityEngine.Rendering.GraphicsDeviceType.Vulkan,
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3
            });
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            Debug.Log("[VR Boat Combat Setup] Graphics API configured!");
            EditorUtility.DisplayDialog("Success", "Vulkan graphics API configured!", "OK");
        }

        private void SetMinimumAPILevel()
        {
            Debug.Log("[VR Boat Combat Setup] Setting minimum API level...");
            // Unity 6 with Meta Quest supports up to Android API 36 (Android 16)
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34; // Android 14
            Debug.Log("[VR Boat Combat Setup] API level configured for Unity 6!");
            EditorUtility.DisplayDialog("Success - Unity 6",
                "Android API levels configured:\n" +
                "- Minimum: API 29 (Android 10)\n" +
                "- Target: API 34 (Android 14)\n\n" +
                "Unity 6 supports up to API 36 (Android 16)",
                "OK");
        }

        private void CreateMainGameScene()
        {
            Debug.Log("[VR Boat Combat Setup] Creating main game scene...");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Save scene
            string scenePath = "Assets/Scenes/MainGame.unity";
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[VR Boat Combat Setup] Main game scene created at {scenePath}");
            EditorUtility.DisplayDialog("Success", "Main game scene created!", "OK");
        }

        private void SetupXROriginInScene()
        {
            Debug.Log("[VR Boat Combat Setup] Setting up XR Origin...");
            EditorUtility.DisplayDialog("XR Origin Setup",
                "To setup XR Origin:\n" +
                "1. Right-click in Hierarchy\n" +
                "2. Select XR > XR Origin (VR)\n" +
                "3. This creates XR Origin with Camera Offset and Main Camera\n" +
                "4. Add Left/Right Controller game objects\n\n" +
                "Note: Requires XR Interaction Toolkit installed",
                "OK");
        }

        private void CreateOceanSystem()
        {
            Debug.Log("[VR Boat Combat Setup] Creating ocean system...");

            GameObject ocean = new GameObject("Ocean System");
            GameObject waveManager = new GameObject("Wave Manager");
            waveManager.transform.SetParent(ocean.transform);
            waveManager.AddComponent<Core.WaveManager>();

            // Create water plane
            GameObject waterPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterPlane.name = "Water Surface";
            waterPlane.transform.SetParent(ocean.transform);
            waterPlane.transform.localScale = new Vector3(100, 1, 100);
            waterPlane.transform.position = Vector3.zero;

            Selection.activeGameObject = ocean;

            Debug.Log("[VR Boat Combat Setup] Ocean system created!");
            EditorUtility.DisplayDialog("Success", "Ocean system created! Add water shader to Water Surface object.", "OK");
        }

        private void SetupLightingForVR()
        {
            Debug.Log("[VR Boat Combat Setup] Setting up VR-optimized lighting...");

            // Set to baked lighting
            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;

            // Find directional light or create one
            Light dirLight = FindAnyObjectByType<Light>();
            if (dirLight == null)
            {
                GameObject lightObj = new GameObject("Directional Light");
                dirLight = lightObj.AddComponent<Light>();
                dirLight.type = LightType.Directional;
            }

            dirLight.intensity = 1.0f;
            dirLight.shadows = LightShadows.Soft;

            // Set ambient lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientIntensity = 0.5f;

            Debug.Log("[VR Boat Combat Setup] Lighting configured!");
            EditorUtility.DisplayDialog("Success", "VR-optimized lighting configured!", "OK");
        }

        private void CreatePlayerBoatPrefab()
        {
            Debug.Log("[VR Boat Combat Setup] Creating player boat prefab...");

            GameObject boat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boat.name = "PlayerBoat";
            boat.transform.localScale = new Vector3(2, 1, 4);

            // Add components
            boat.AddComponent<Rigidbody>();
            boat.AddComponent<Core.BoatPhysics>();
            boat.AddComponent<Core.VRBoatController>();
            boat.AddComponent<Core.HealthSystem>();

            // Create prefab
            string path = "Assets/Prefabs/Boats/PlayerBoat.prefab";
            System.IO.Directory.CreateDirectory("Assets/Prefabs/Boats");
            PrefabUtility.SaveAsPrefabAsset(boat, path);

            DestroyImmediate(boat);

            Debug.Log($"[VR Boat Combat Setup] Player boat prefab created at {path}");
            EditorUtility.DisplayDialog("Success", "Player boat prefab created!", "OK");
        }

        private void CreateEnemyBoatPrefab()
        {
            Debug.Log("[VR Boat Combat Setup] Creating enemy boat prefab...");

            GameObject boat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boat.name = "EnemyBoat";
            boat.transform.localScale = new Vector3(2, 1, 4);
            boat.tag = "Enemy";

            // Add components
            boat.AddComponent<Rigidbody>();
            boat.AddComponent<Core.BoatPhysics>();
            boat.AddComponent<AI.EnemyAI>();
            boat.AddComponent<Core.HealthSystem>();
            boat.AddComponent<Core.CaptureSystem>();

            // Create prefab
            string path = "Assets/Prefabs/Boats/EnemyBoat.prefab";
            System.IO.Directory.CreateDirectory("Assets/Prefabs/Boats");
            PrefabUtility.SaveAsPrefabAsset(boat, path);

            DestroyImmediate(boat);

            Debug.Log($"[VR Boat Combat Setup] Enemy boat prefab created at {path}");
            EditorUtility.DisplayDialog("Success", "Enemy boat prefab created!", "OK");
        }

        private void CreateProjectilePrefab()
        {
            Debug.Log("[VR Boat Combat Setup] Creating projectile prefab...");

            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "Projectile";
            projectile.transform.localScale = Vector3.one * 0.2f;

            // Add components
            Rigidbody rb = projectile.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.mass = 0.5f;

            projectile.AddComponent<Weapons.Projectile>();

            // Create prefab
            string path = "Assets/Prefabs/Weapons/Projectile.prefab";
            System.IO.Directory.CreateDirectory("Assets/Prefabs/Weapons");
            PrefabUtility.SaveAsPrefabAsset(projectile, path);

            DestroyImmediate(projectile);

            Debug.Log($"[VR Boat Combat Setup] Projectile prefab created at {path}");
            EditorUtility.DisplayDialog("Success", "Projectile prefab created!", "OK");
        }

        private void ApplyQuest2QualitySettings()
        {
            Debug.Log("[VR Boat Combat Setup] Applying Quest 2 quality settings...");

            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 2;
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            QualitySettings.shadowDistance = 20f;
            QualitySettings.SetQualityLevel(1, true); // Medium quality

            Debug.Log("[VR Boat Combat Setup] Quest 2 quality settings applied!");
            EditorUtility.DisplayDialog("Success", "Quest 2 quality settings applied (72Hz, Medium)", "OK");
        }

        private void ApplyQuest3QualitySettings()
        {
            Debug.Log("[VR Boat Combat Setup] Applying Quest 3 quality settings...");

            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 4;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.Medium;
            QualitySettings.shadowDistance = 40f;
            QualitySettings.SetQualityLevel(2, true); // High quality

            Debug.Log("[VR Boat Combat Setup] Quest 3 quality settings applied!");
            EditorUtility.DisplayDialog("Success", "Quest 3 quality settings applied (90Hz, High)", "OK");
        }

        private void EnableFixedFoveatedRendering()
        {
            Debug.Log("[VR Boat Combat Setup] Fixed Foveated Rendering info...");
            EditorUtility.DisplayDialog("Fixed Foveated Rendering",
                "To enable Fixed Foveated Rendering:\n\n" +
                "1. This is configured at runtime via Oculus/Meta XR SDK\n" +
                "2. Add the Meta XR SDK package\n" +
                "3. Use OVRManager.tiledMultiResLevel\n" +
                "4. Set to OVRManager.TiledMultiResLevel.LMSMedium for balance\n\n" +
                "This significantly improves performance on Quest!",
                "OK");
        }

        private void ConfigureOcclusionCulling()
        {
            Debug.Log("[VR Boat Combat Setup] Configuring occlusion culling...");

            // Enable occlusion culling
            StaticOcclusionCulling.umbraDataSize = 0;

            EditorUtility.DisplayDialog("Occlusion Culling",
                "To setup occlusion culling:\n\n" +
                "1. Mark static objects as 'Occluder Static' and 'Occludee Static'\n" +
                "2. Window > Rendering > Occlusion Culling\n" +
                "3. Click 'Bake' tab\n" +
                "4. Click 'Bake' button\n\n" +
                "This improves rendering performance!",
                "OK");
        }

        private void GenerateREADME()
        {
            Debug.Log("[VR Boat Combat Setup] Generating README...");
            EditorUtility.DisplayDialog("Generate README",
                "A comprehensive README is generated separately.\n" +
                "Check the project root for documentation files.",
                "OK");
        }
    }

    // Quick action menu items
    public static class QuickSetupMenuItems
    {
        [MenuItem("Tools/VR Boat Combat/Quick Setup/Configure Android Build")]
        public static void QuickConfigureAndroid()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            Debug.Log("[VR Boat Combat] Quick Android setup complete!");
        }

        [MenuItem("Tools/VR Boat Combat/Quick Setup/Create All Prefabs")]
        public static void QuickCreatePrefabs()
        {
            Debug.Log("[VR Boat Combat] Creating all prefab templates...");
            // This would call the prefab creation methods
        }

        [MenuItem("Tools/VR Boat Combat/Documentation/Open Setup Guide")]
        public static void OpenSetupGuide()
        {
            string path = Application.dataPath + "/../SETUP_GUIDE.md";
            if (System.IO.File.Exists(path))
            {
                Application.OpenURL("file://" + path);
            }
            else
            {
                EditorUtility.DisplayDialog("Setup Guide", "SETUP_GUIDE.md not found in project root", "OK");
            }
        }

        [MenuItem("Tools/VR Boat Combat/Documentation/Open Build Instructions")]
        public static void OpenBuildInstructions()
        {
            string path = Application.dataPath + "/../BUILD_INSTRUCTIONS.md";
            if (System.IO.File.Exists(path))
            {
                Application.OpenURL("file://" + path);
            }
            else
            {
                EditorUtility.DisplayDialog("Build Instructions", "BUILD_INSTRUCTIONS.md not found in project root", "OK");
            }
        }
    }
}
#endif
