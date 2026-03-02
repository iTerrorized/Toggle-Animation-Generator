using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class AnimationGeneratorTool
{
    private const string MENU_PATH = "Terrorized/";
    private const string ASSET_FOLDER = "Assets/!Terrorized/GeneratedAssets";

    [MenuItem("GameObject/Terrorized/Generate OnOff", false, 10)]
    private static void GenerateOnOff(MenuCommand menuCommand)
    {
        GameObject targetObject = (GameObject)menuCommand.context;
        if (targetObject == null) return;

        ShowNameDialog((name) => CreateOnOffAnimations(targetObject, name, false));
    }

    [MenuItem("GameObject/Terrorized/Generate OnOff (Dissolve)", false, 11)]
    private static void GenerateOnOffDissolve(MenuCommand menuCommand)
    {
        GameObject targetObject = (GameObject)menuCommand.context;
        if (targetObject == null) return;

        ShowNameDialog((name) => CreateOnOffAnimations(targetObject, name, true));
    }

    [MenuItem("GameObject/Terrorized/Generate OnOff (2 Frame Animation)", false, 12)]
    private static void GenerateOnOff2Frame(MenuCommand menuCommand)
    {
        GameObject targetObject = (GameObject)menuCommand.context;
        if (targetObject == null) return;

        ShowNameDialog((name) => CreateOnOff2FrameAnimation(targetObject, name));
    }

    [MenuItem("GameObject/Terrorized/Create Int", false, 13)]
    private static void CreateInt(MenuCommand menuCommand)
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects == null || selectedObjects.Length == 0) return;

        ShowIntAnimationDialog(selectedObjects, false);
    }

    [MenuItem("GameObject/Terrorized/Create Int with Off", false, 14)]
    private static void CreateIntWithOff(MenuCommand menuCommand)
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects == null || selectedObjects.Length == 0) return;

        ShowIntAnimationDialog(selectedObjects, true);
    }

    private static void ShowNameDialog(System.Action<string> onConfirm)
    {
        AnimationNameWindow.Show(onConfirm);
    }

    private static void ShowIntAnimationDialog(GameObject[] objects, bool includeOffAnimation)
    {
        IntAnimationWindow.Show(objects, includeOffAnimation);
    }

    private static void CreateOnOffAnimations(GameObject targetObject, string baseName, bool useDissolve)
    {
        // Create folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(ASSET_FOLDER))
        {
            AssetDatabase.CreateFolder("Assets/!Terrorized", "GeneratedAssets");
        }

        string pathOff = Path.Combine(ASSET_FOLDER, baseName + "Off.anim");
        string pathOn = Path.Combine(ASSET_FOLDER, baseName + "On.anim");

        // Create Off animation
        AnimationClip offClip = new AnimationClip();
        offClip.name = baseName + "Off";
        SetupAnimationClip(offClip, targetObject, false, useDissolve);
        AssetDatabase.CreateAsset(offClip, pathOff);

        // Create On animation
        AnimationClip onClip = new AnimationClip();
        onClip.name = baseName + "On";
        SetupAnimationClip(onClip, targetObject, true, useDissolve);
        AssetDatabase.CreateAsset(onClip, pathOn);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"Created animations:\n{baseName}Off\n{baseName}On", "OK");
    }

    private static void CreateOnOff2FrameAnimation(GameObject targetObject, string baseName)
    {
        // Create folder structure if needed
        if (!AssetDatabase.IsValidFolder("Assets/!Terrorized"))
            AssetDatabase.CreateFolder("Assets", "!Terrorized");
        if (!AssetDatabase.IsValidFolder(ASSET_FOLDER))
            AssetDatabase.CreateFolder("Assets/!Terrorized", "GeneratedAssets");

        string savePath = Path.Combine(ASSET_FOLDER, baseName + ".anim");
        string targetPath = GetGameObjectPath(targetObject);

        AnimationClip clip = new AnimationClip();
        clip.name = baseName;

        // Two keyframes at 60fps: frame 0 = On (1.0), frame 1 = Off (0.0)
        float frame1Time = 1f / 60f;

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0f, 1f));
        curve.AddKey(new Keyframe(frame1Time, 0f));

        // Set constant/stepped tangents so there is no interpolation between frames
        AnimationUtility.SetKeyLeftTangentMode(curve, 0, AnimationUtility.TangentMode.Constant);
        AnimationUtility.SetKeyRightTangentMode(curve, 0, AnimationUtility.TangentMode.Constant);
        AnimationUtility.SetKeyLeftTangentMode(curve, 1, AnimationUtility.TangentMode.Constant);
        AnimationUtility.SetKeyRightTangentMode(curve, 1, AnimationUtility.TangentMode.Constant);

        EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
            targetPath,
            typeof(GameObject),
            "m_IsActive"
        );
        AnimationUtility.SetEditorCurve(clip, binding, curve);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"Created 2-frame animation:\n{baseName}\n\nFrame 0: On\nFrame 1: Off", "OK");
    }

    public static void CreateIntAnimations(GameObject[] objects, string prefix, string[] animNames, bool includeOffAnimation)
    {
        // Create folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(ASSET_FOLDER))
        {
            AssetDatabase.CreateFolder("Assets/!Terrorized", "GeneratedAssets");
        }

        // Create an animation for each object
        for (int i = 0; i < objects.Length; i++)
        {
            string animName = prefix + animNames[i];
            string path = Path.Combine(ASSET_FOLDER, animName + ".anim");

            AnimationClip clip = new AnimationClip();
            clip.name = animName;
            SetupIntAnimationClip(clip, objects, i);
            AssetDatabase.CreateAsset(clip, path);
        }

        // Create "Off" animation if requested
        if (includeOffAnimation)
        {
            string offAnimName = prefix + "Off";
            string offPath = Path.Combine(ASSET_FOLDER, offAnimName + ".anim");

            AnimationClip offClip = new AnimationClip();
            offClip.name = offAnimName;
            SetupIntAnimationClip(offClip, objects, -1); // -1 means all off
            AssetDatabase.CreateAsset(offClip, offPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string message = includeOffAnimation
            ? $"Created {objects.Length + 1} animations with prefix '{prefix}'"
            : $"Created {objects.Length} animations with prefix '{prefix}'";
        EditorUtility.DisplayDialog("Success", message, "OK");
    }

    private static void SetupAnimationClip(AnimationClip clip, GameObject targetObject, bool isOn, bool useDissolve)
    {
        string targetPath = GetGameObjectPath(targetObject);

        // Set IsActive curve
        AnimationCurve isActiveCurve = AnimationCurve.EaseInOut(0, isOn ? 1 : 0, 0, isOn ? 1 : 0);
        isActiveCurve.keys[0].inTangent = float.PositiveInfinity;
        isActiveCurve.keys[0].outTangent = float.PositiveInfinity;

        EditorCurveBinding isActiveBinding = EditorCurveBinding.FloatCurve(
            targetPath,
            typeof(GameObject),
            "m_IsActive"
        );
        AnimationUtility.SetEditorCurve(clip, isActiveBinding, isActiveCurve);

        // Set DissolveAlpha curve if needed
        if (useDissolve)
        {
            AnimationCurve dissolveCurve = AnimationCurve.Linear(0, isOn ? 0 : 1, 0, isOn ? 0 : 1);
            dissolveCurve.keys[0].inTangent = 0;
            dissolveCurve.keys[0].outTangent = 0;

            EditorCurveBinding dissolveBinding = EditorCurveBinding.FloatCurve(
                targetPath,
                typeof(Renderer),
                "material._DissolveAlpha"
            );
            AnimationUtility.SetEditorCurve(clip, dissolveBinding, dissolveCurve);
        }

        // Set animation settings
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
    }

    private static void SetupIntAnimationClip(AnimationClip clip, GameObject[] objects, int activeIndex)
    {
        // For each object, set its IsActive state
        for (int i = 0; i < objects.Length; i++)
        {
            string targetPath = GetGameObjectPath(objects[i]);
            bool isActive = (i == activeIndex); // activeIndex = -1 means all off

            AnimationCurve isActiveCurve = AnimationCurve.EaseInOut(0, isActive ? 1 : 0, 0, isActive ? 1 : 0);
            isActiveCurve.keys[0].inTangent = float.PositiveInfinity;
            isActiveCurve.keys[0].outTangent = float.PositiveInfinity;

            EditorCurveBinding isActiveBinding = EditorCurveBinding.FloatCurve(
                targetPath,
                typeof(GameObject),
                "m_IsActive"
            );
            AnimationUtility.SetEditorCurve(clip, isActiveBinding, isActiveCurve);
        }

        // Set animation settings
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
    }

    private static string GetGameObjectPath(GameObject obj)
    {
        // Find the nearest Animator component in parent hierarchy
        Transform current = obj.transform;
        Transform animatorTransform = null;

        // Search up the hierarchy for an Animator
        while (current != null)
        {
            if (current.GetComponent<Animator>() != null)
            {
                animatorTransform = current;
                break;
            }
            current = current.parent;
        }

        // If no animator found, return just the object name
        if (animatorTransform == null)
        {
            Debug.LogWarning($"No Animator found in hierarchy for {obj.name}. Using object name only.");
            return obj.name;
        }

        // Build the path from the animator to the target object
        string path = "";
        current = obj.transform;

        while (current != animatorTransform)
        {
            if (path == "")
            {
                path = current.name;
            }
            else
            {
                path = current.name + "/" + path;
            }
            current = current.parent;
        }

        return path;
    }
}

public class AnimationNameWindow : EditorWindow
{
    private string animationName = "";
    private System.Action<string> onConfirm;

    public static void Show(System.Action<string> callback)
    {
        AnimationNameWindow window = CreateInstance<AnimationNameWindow>();
        window.onConfirm = callback;
        window.titleContent = new GUIContent("Animation Name");
        window.minSize = new Vector2(300, 80);
        window.ShowModal();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Enter Animation Name", EditorStyles.boldLabel);

        GUI.SetNextControlName("AnimationNameField");
        animationName = EditorGUILayout.TextField("Name:", animationName);
        EditorGUI.FocusTextInControl("AnimationNameField");

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create", GUILayout.Height(30)))
        {
            if (!string.IsNullOrEmpty(animationName))
            {
                onConfirm?.Invoke(animationName);
                Close();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please enter a name for the animation", "OK");
            }
        }

        if (GUILayout.Button("Cancel", GUILayout.Height(30)))
        {
            Close();
        }
        EditorGUILayout.EndHorizontal();

        // Allow Enter key to confirm
        if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown)
        {
            if (!string.IsNullOrEmpty(animationName))
            {
                onConfirm?.Invoke(animationName);
                Close();
            }
        }
    }
}

public class IntAnimationWindow : EditorWindow
{
    private string prefix = "";
    private string[] animationNames;
    private GameObject[] objects;
    private bool includeOffAnimation;
    private Vector2 scrollPosition;

    public static void Show(GameObject[] selectedObjects, bool includeOff)
    {
        IntAnimationWindow window = CreateInstance<IntAnimationWindow>();
        window.objects = selectedObjects;
        window.includeOffAnimation = includeOff;
        window.animationNames = new string[selectedObjects.Length];

        // Initialize with object names
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            window.animationNames[i] = selectedObjects[i].name;
        }

        window.titleContent = new GUIContent("Create Int Animations");
        window.minSize = new Vector2(400, 300);
        window.ShowModal();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Int Animation Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Prefix field
        EditorGUILayout.LabelField("Animation Prefix", EditorStyles.boldLabel);
        GUI.SetNextControlName("PrefixField");
        prefix = EditorGUILayout.TextField("Prefix:", prefix);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Animations to Create ({objects.Length}{(includeOffAnimation ? " + Off" : "")})", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Scrollable area for object names
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));

        for (int i = 0; i < objects.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{i}:", GUILayout.Width(30));
            EditorGUILayout.LabelField(objects[i].name, GUILayout.Width(150));
            EditorGUILayout.LabelField("->", GUILayout.Width(20));
            animationNames[i] = EditorGUILayout.TextField(animationNames[i]);
            EditorGUILayout.EndHorizontal();
        }

        if (includeOffAnimation)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Off:", GUILayout.Width(30));
            EditorGUILayout.LabelField("(All objects off)", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // Preview
        EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        for (int i = 0; i < objects.Length; i++)
        {
            EditorGUILayout.LabelField($"* {prefix}{animationNames[i]}.anim", EditorStyles.miniLabel);
        }
        if (includeOffAnimation)
        {
            EditorGUILayout.LabelField($"* {prefix}Off.anim", EditorStyles.miniLabel);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create", GUILayout.Height(30)))
        {
            bool valid = true;

            // Validate all names are filled
            for (int i = 0; i < animationNames.Length; i++)
            {
                if (string.IsNullOrEmpty(animationNames[i]))
                {
                    EditorUtility.DisplayDialog("Error", $"Please enter a name for object {i} ({objects[i].name})", "OK");
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                AnimationGeneratorTool.CreateIntAnimations(objects, prefix, animationNames, includeOffAnimation);
                Close();
            }
        }

        if (GUILayout.Button("Cancel", GUILayout.Height(30)))
        {
            Close();
        }
        EditorGUILayout.EndHorizontal();
    }
}