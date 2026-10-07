using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

/// <summary>
/// One-click configuration of this project for standalone Meta Quest 3 / 3S (OpenXR, Android, Vulkan, URP).
/// Run via Tools > Quest > Configure Project. Safe to run repeatedly.
/// </summary>
public static class QuestProjectSetup
{
    static readonly string[] EnabledFeatureTypes =
    {
        "MetaQuestFeature",
        "OculusTouchControllerProfile",
        "MetaQuestTouchProControllerProfile",
        "MetaQuestTouchPlusControllerProfile",
        "HandInteractionProfile",
        "HandTracking",
        "FoveatedRenderingFeature",
    };

    [MenuItem("Tools/Quest/Configure Project")]
    public static void Configure()
    {
        ConfigureXR();
        ConfigureAndroidPlayer();
        ConfigureGraphics();
        ConfigureTime();
        AssetDatabase.SaveAssets();
        Debug.Log("[QuestSetup] Project configured for Meta Quest 3/3S.");
    }

    static void ConfigureXR()
    {
        // GetOrCreate is internal to XR Management; reach it via reflection so settings assets exist without opening the UI.
        var getOrCreate = typeof(XRGeneralSettingsPerBuildTarget).GetMethod(
            "GetOrCreate", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var container = (XRGeneralSettingsPerBuildTarget)getOrCreate.Invoke(null, null);
        const BuildTargetGroup android = BuildTargetGroup.Android;
        if (!container.HasSettingsForBuildTarget(android)) container.CreateDefaultSettingsForBuildTarget(android);
        if (!container.HasManagerSettingsForBuildTarget(android)) container.CreateDefaultManagerSettingsForBuildTarget(android);

        var general = container.SettingsForBuildTarget(android);
        general.InitManagerOnStart = true;
        var manager = container.ManagerSettingsForBuildTarget(android);
        XRPackageMetadataStore.AssignLoader(manager, "UnityEngine.XR.OpenXR.OpenXRLoader", android);
        EditorUtility.SetDirty(container);
        EditorUtility.SetDirty(general);
        EditorUtility.SetDirty(manager);

        var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(android);
        settings.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
        settings.depthSubmissionMode = OpenXRSettings.DepthSubmissionMode.None;
        EditorUtility.SetDirty(settings);

        foreach (var feature in settings.GetFeatures<OpenXRFeature>())
        {
            string typeName = feature.GetType().Name;
            if (EnabledFeatureTypes.Contains(typeName))
            {
                feature.enabled = true;
                EditorUtility.SetDirty(feature);
            }
            if (typeName == "MetaQuestFeature") SetQuestTargetDevices(feature);
        }
    }

    // Only Quest 3 ("eureka") and Quest 3S ("quest3s") are targeted.
    static void SetQuestTargetDevices(OpenXRFeature feature)
    {
        var so = new SerializedObject(feature);
        var devices = so.FindProperty("targetDevices");
        if (devices == null) return;
        for (int i = 0; i < devices.arraySize; i++)
        {
            var d = devices.GetArrayElementAtIndex(i);
            string manifest = d.FindPropertyRelative("manifestName").stringValue;
            d.FindPropertyRelative("enabled").boolValue = manifest == "eureka" || manifest == "quest3s";
        }
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(feature);
    }

    static void ConfigureAndroidPlayer()
    {
        var android = NamedBuildTarget.Android;
        PlayerSettings.companyName = "MRE Club";
        PlayerSettings.productName = "Space Spark";
        PlayerSettings.SetApplicationIdentifier(android, "com.mreclub.spacespark");

        PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.Activity;
        PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.Medium);

        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.gpuSkinning = true;
        PlayerSettings.graphicsJobs = true;
        PlayerSettings.SetMobileMTRendering(android, true);
        PlayerSettings.Android.optimizedFramePacing = false; // OpenXR runtime owns frame pacing
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = false; // APK for sideloading / Meta Quest Developer Hub
    }

    static void ConfigureGraphics()
    {
        var rp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
        if (rp != null)
        {
            rp.renderScale = 1f;
            rp.msaaSampleCount = 4;
            rp.supportsHDR = false;
            rp.supportsCameraDepthTexture = false;
            rp.supportsCameraOpaqueTexture = false;
            rp.useSRPBatcher = true;
            rp.mainLightShadowmapResolution = 1024;
            rp.shadowDistance = 15f;
            rp.shadowCascadeCount = 1;
            // These three have internal setters in URP, so write the serialized fields directly.
            var rpSo = new SerializedObject(rp);
            SetBool(rpSo, "m_MainLightShadowsSupported", true);
            SetBool(rpSo, "m_SoftShadowsSupported", false);
            var lightMode = rpSo.FindProperty("m_AdditionalLightsRenderingMode");
            if (lightMode != null) lightMode.intValue = (int)LightRenderingMode.PerVertex;
            rpSo.ApplyModifiedPropertiesWithoutUndo();
            rp.maxAdditionalLightsCount = 2;
            rp.colorGradingMode = ColorGradingMode.LowDynamicRange;
            EditorUtility.SetDirty(rp);

            GraphicsSettings.defaultRenderPipeline = rp;
            QualitySettings.renderPipeline = rp;
        }

        QualitySettings.SetQualityLevel(0, true);
        QualitySettings.vSyncCount = 0;
        QualitySettings.skinWeights = SkinWeights.TwoBones;
        QualitySettings.globalTextureMipmapLimit = 0;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.softParticles = false;
        QualitySettings.billboardsFaceCameraPosition = false;

        // Make "Mobile" (index 0) the Android default quality level.
        var qAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset").FirstOrDefault();
        if (qAsset != null)
        {
            var so = new SerializedObject(qAsset);
            var perPlatform = so.FindProperty("m_PerPlatformDefaultQuality");
            if (perPlatform != null)
            {
                for (int i = 0; i < perPlatform.arraySize; i++)
                {
                    var e = perPlatform.GetArrayElementAtIndex(i);
                    if (e.FindPropertyRelative("first").stringValue == "Android")
                        e.FindPropertyRelative("second").intValue = 0;
                }
                so.ApplyModifiedProperties();
            }
        }
    }

    static void SetBool(SerializedObject so, string name, bool value)
    {
        var p = so.FindProperty(name);
        if (p != null) p.boolValue = value;
    }

    static void ConfigureTime()
    {
        // Match Quest 3 default 72 Hz so physics steps line up with rendered frames.
        Time.fixedDeltaTime = 1f / 72f;
        var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TimeManager.asset").FirstOrDefault();
        if (asset == null) return;
        var so = new SerializedObject(asset);
        var p = so.FindProperty("Fixed Timestep");
        if (p != null)
        {
            p.floatValue = 1f / 72f;
            so.ApplyModifiedProperties();
        }
    }
}
