using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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

    // Guard fires once even when multiple objects are selected
    [MenuItem("GameObject/Terrorized/Create Int", false, 13)]
    private static void CreateInt(MenuCommand menuCommand)
    {
        if (menuCommand.context != Selection.activeObject) return;
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects == null || selectedObjects.Length == 0) return;
        ShowIntAnimationDialog(selectedObjects, false);
    }

    [MenuItem("GameObject/Terrorized/Create Int with Off", false, 14)]
    private static void CreateIntWithOff(MenuCommand menuCommand)
    {
        if (menuCommand.context != Selection.activeObject) return;
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects == null || selectedObjects.Length == 0) return;
        ShowIntAnimationDialog(selectedObjects, true);
    }

    [MenuItem("GameObject/Terrorized/Create Single DBT Toggles", false, 15)]
    private static void CreateSingleDBTToggles(MenuCommand menuCommand)
    {
        if (menuCommand.context != Selection.activeObject) return;
        GameObject[] selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0) return;
        CreateSingleDBTTogglesWindow.Show(selected);
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
        if (!AssetDatabase.IsValidFolder(ASSET_FOLDER))
        {
            AssetDatabase.CreateFolder("Assets/!Terrorized", "GeneratedAssets");
        }

        string pathOff = Path.Combine(ASSET_FOLDER, baseName + "Off.anim");
        string pathOn = Path.Combine(ASSET_FOLDER, baseName + "On.anim");

        AnimationClip offClip = new AnimationClip();
        offClip.name = baseName + "Off";
        SetupAnimationClip(offClip, targetObject, false, useDissolve);
        AssetDatabase.CreateAsset(offClip, pathOff);

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
        if (!AssetDatabase.IsValidFolder("Assets/!Terrorized"))
            AssetDatabase.CreateFolder("Assets", "!Terrorized");
        if (!AssetDatabase.IsValidFolder(ASSET_FOLDER))
            AssetDatabase.CreateFolder("Assets/!Terrorized", "GeneratedAssets");

        string savePath = Path.Combine(ASSET_FOLDER, baseName + ".anim");
        string targetPath = GetGameObjectPath(targetObject);

        AnimationClip clip = new AnimationClip();
        clip.name = baseName;

        float frame1Time = 1f / 60f;

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0f, 1f));
        curve.AddKey(new Keyframe(frame1Time, 0f));

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
        if (!AssetDatabase.IsValidFolder(ASSET_FOLDER))
        {
            AssetDatabase.CreateFolder("Assets/!Terrorized", "GeneratedAssets");
        }

        for (int i = 0; i < objects.Length; i++)
        {
            string animName = prefix + animNames[i];
            string path = Path.Combine(ASSET_FOLDER, animName + ".anim");

            AnimationClip clip = new AnimationClip();
            clip.name = animName;
            SetupIntAnimationClip(clip, objects, i);
            AssetDatabase.CreateAsset(clip, path);
        }

        if (includeOffAnimation)
        {
            string offAnimName = prefix + "Off";
            string offPath = Path.Combine(ASSET_FOLDER, offAnimName + ".anim");

            AnimationClip offClip = new AnimationClip();
            offClip.name = offAnimName;
            SetupIntAnimationClip(offClip, objects, -1);
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

        AnimationCurve isActiveCurve = AnimationCurve.EaseInOut(0, isOn ? 1 : 0, 0, isOn ? 1 : 0);
        isActiveCurve.keys[0].inTangent = float.PositiveInfinity;
        isActiveCurve.keys[0].outTangent = float.PositiveInfinity;

        EditorCurveBinding isActiveBinding = EditorCurveBinding.FloatCurve(
            targetPath,
            typeof(GameObject),
            "m_IsActive"
        );
        AnimationUtility.SetEditorCurve(clip, isActiveBinding, isActiveCurve);

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

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
    }

    private static void SetupIntAnimationClip(AnimationClip clip, GameObject[] objects, int activeIndex)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            string targetPath = GetGameObjectPath(objects[i]);
            bool isActive = (i == activeIndex);

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

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
    }

    // ─── DBT Toggle helpers ───────────────────────────────────────────────────

    // Strips "Category | " prefix and returns PascalCase with no spaces.
    // e.g. "Tops | Crop Top" -> "CropTop", "Acc | Bunny Ears" -> "BunnyEars"
    public static string CleanDisplayName(string rawName)
    {
        int pipeIdx = rawName.IndexOf(" | ");
        string name = pipeIdx >= 0 ? rawName.Substring(pipeIdx + 3).Trim() : rawName.Trim();
        var words = name.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        return string.Join("", words.Select(w => char.ToUpper(w[0]) + w.Substring(1)));
    }

    public static List<(BlendTree tree, string label)> FindDirectBlendTrees(AnimatorController controller)
    {
        var results = new List<(BlendTree, string)>();
        foreach (var layer in controller.layers)
            FindDirectBlendTreesInSM(layer.stateMachine, layer.name, results);
        return results;
    }

    private static void FindDirectBlendTreesInSM(AnimatorStateMachine sm, string path, List<(BlendTree, string)> results)
    {
        foreach (var stateInfo in sm.states)
        {
            if (stateInfo.state.motion is BlendTree bt)
                CollectDirectBlendTrees(bt, $"{path}/{stateInfo.state.name}", results);
        }
        foreach (var subSM in sm.stateMachines)
            FindDirectBlendTreesInSM(subSM.stateMachine, $"{path}/{subSM.stateMachine.name}", results);
    }

    private static void CollectDirectBlendTrees(BlendTree tree, string path, List<(BlendTree, string)> results)
    {
        if (tree.blendType == BlendTreeType.Direct)
            results.Add((tree, path));
        foreach (var child in tree.children)
        {
            if (child.motion is BlendTree childTree)
                CollectDirectBlendTrees(childTree, $"{path}/{childTree.name}", results);
        }
    }

    public static void EnsureAnimatorParameter(AnimatorController controller, string name)
    {
        foreach (var param in controller.parameters)
            if (param.name == name) return;
        controller.AddParameter(name, AnimatorControllerParameterType.Float);
    }

    public static void EnsureAnimatorTrigger(AnimatorController controller, string name)
    {
        foreach (var param in controller.parameters)
            if (param.name == name) return;
        controller.AddParameter(name, AnimatorControllerParameterType.Trigger);
    }

    // Searches for blendshapes named "CLIPPING/{displayName}..." on all SMRs under the animator.
    // Also tries without spaces so "Crop Top" matches "CLIPPING/CropTop".
    public static List<(string smrPath, string shapeName)> FindClippingBlendshapes(string displayName, Transform animatorRoot)
    {
        var results = new List<(string, string)>();
        string prefix = "CLIPPING/" + displayName;
        string prefixNoSpaces = "CLIPPING/" + displayName.Replace(" ", "");

        var smrs = animatorRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var smr in smrs)
        {
            var mesh = smr.sharedMesh;
            if (mesh == null) continue;
            string smrPath = GetTransformPath(smr.transform, animatorRoot);
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                string shapeName = mesh.GetBlendShapeName(i);
                bool matches = shapeName.StartsWith(prefix)
                    || (prefixNoSpaces != prefix && shapeName.StartsWith(prefixNoSpaces));
                if (matches)
                    results.Add((smrPath, shapeName));
            }
        }
        return results;
    }

    private static string GetTransformPath(Transform target, Transform root)
    {
        string path = "";
        Transform current = target;
        while (current != root)
        {
            path = path == "" ? current.name : current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }

    private static void EnsureAssetFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/!Terrorized"))
            AssetDatabase.CreateFolder("Assets", "!Terrorized");
        if (!AssetDatabase.IsValidFolder(ASSET_FOLDER))
            AssetDatabase.CreateFolder("Assets/!Terrorized", "GeneratedAssets");
    }

    public static AnimationClip CreateDBTToggleAnimation(
        GameObject obj, string clipName, bool isOn,
        List<(string smrPath, string shapeName)> blendshapes)
    {
        EnsureAssetFolder();
        string savePath = Path.Combine(ASSET_FOLDER, clipName + ".anim");
        string targetPath = GetGameObjectPath(obj);

        AnimationClip clip = new AnimationClip();
        clip.name = clipName;

        AnimationCurve isActiveCurve = new AnimationCurve();
        isActiveCurve.AddKey(new Keyframe(0f, isOn ? 1f : 0f));
        AnimationUtility.SetKeyLeftTangentMode(isActiveCurve, 0, AnimationUtility.TangentMode.Constant);
        AnimationUtility.SetKeyRightTangentMode(isActiveCurve, 0, AnimationUtility.TangentMode.Constant);

        EditorCurveBinding isActiveBinding = EditorCurveBinding.FloatCurve(targetPath, typeof(GameObject), "m_IsActive");
        AnimationUtility.SetEditorCurve(clip, isActiveBinding, isActiveCurve);

        foreach (var (smrPath, shapeName) in blendshapes)
        {
            AnimationCurve bsCurve = new AnimationCurve();
            bsCurve.AddKey(new Keyframe(0f, isOn ? 100f : 0f));
            AnimationUtility.SetKeyLeftTangentMode(bsCurve, 0, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(bsCurve, 0, AnimationUtility.TangentMode.Constant);

            EditorCurveBinding bsBinding = EditorCurveBinding.FloatCurve(smrPath, typeof(SkinnedMeshRenderer), "blendShape." + shapeName);
            AnimationUtility.SetEditorCurve(clip, bsBinding, bsCurve);
        }

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, savePath);
        return clip;
    }

    public static void ExecuteCreateSingleDBTToggles(
        GameObject[] objects,
        string[] displayNames,
        AnimatorController controller,
        BlendTree targetDBT,
        string dbtParameter,
        Dictionary<string, List<(string smrPath, string shapeName)>> selectedBlendshapes)
    {
        EnsureAssetFolder();
        Undo.RecordObject(controller, "Create Single DBT Toggles");

        // Group objects by parent name, preserving insertion order
        var groups = new List<(string emptyName, List<(GameObject obj, string name)> items)>();
        var groupIndex = new Dictionary<string, int>();

        for (int i = 0; i < objects.Length; i++)
        {
            string parentName = objects[i].transform.parent != null ? objects[i].transform.parent.name : "Root";
            if (!groupIndex.TryGetValue(parentName, out int idx))
            {
                idx = groups.Count;
                groupIndex[parentName] = idx;
                groups.Add((parentName, new List<(GameObject, string)>()));
            }
            groups[idx].items.Add((objects[i], displayNames[i]));
        }

        foreach (var (emptyName, items) in groups)
        {
            // Add a visual separator trigger before this group's params
            EnsureAnimatorTrigger(controller, $"-----{emptyName}-----");

            BlendTree groupTree = new BlendTree();
            groupTree.name = emptyName;
            groupTree.blendType = BlendTreeType.Direct;
            groupTree.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(groupTree, controller);
            Undo.RegisterCreatedObjectUndo(groupTree, "Create Single DBT Toggles");

            targetDBT.AddChild(groupTree);
            var targetChildren = targetDBT.children;
            targetChildren[targetChildren.Length - 1].directBlendParameter = dbtParameter;
            targetDBT.children = targetChildren;

            foreach (var (obj, displayName) in items)
            {
                string paramName = $"{emptyName}/{displayName}";
                string animBase = $"Toggles.{emptyName}.{displayName}";

                EnsureAnimatorParameter(controller, paramName);

                string bsKey = obj.GetInstanceID().ToString();
                selectedBlendshapes.TryGetValue(bsKey, out var bsList);
                bsList = bsList ?? new List<(string, string)>();

                AnimationClip offClip = CreateDBTToggleAnimation(obj, animBase + ".Off", false, bsList);
                AnimationClip onClip  = CreateDBTToggleAnimation(obj, animBase + ".On",  true,  bsList);

                BlendTree oneDTree = new BlendTree();
                oneDTree.name = displayName;
                oneDTree.blendType = BlendTreeType.Simple1D;
                oneDTree.blendParameter = paramName;
                oneDTree.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(oneDTree, controller);
                Undo.RegisterCreatedObjectUndo(oneDTree, "Create Single DBT Toggles");

                oneDTree.AddChild(offClip, 0f);
                oneDTree.AddChild(onClip, 1f);

                groupTree.AddChild(oneDTree);
                var groupChildren = groupTree.children;
                groupChildren[groupChildren.Length - 1].directBlendParameter = dbtParameter;
                groupTree.children = groupChildren;
            }
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", "DBT Toggles created successfully!", "OK");
    }

    // ─── Shared ───────────────────────────────────────────────────────────────

    private static string GetGameObjectPath(GameObject obj)
    {
        Transform current = obj.transform;
        Transform animatorTransform = null;

        while (current != null)
        {
            if (current.GetComponent<Animator>() != null)
            {
                animatorTransform = current;
                break;
            }
            current = current.parent;
        }

        if (animatorTransform == null)
        {
            Debug.LogWarning($"No Animator found in hierarchy for {obj.name}. Using object name only.");
            return obj.name;
        }

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

        EditorGUILayout.LabelField("Animation Prefix", EditorStyles.boldLabel);
        GUI.SetNextControlName("PrefixField");
        prefix = EditorGUILayout.TextField("Prefix:", prefix);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Animations to Create ({objects.Length}{(includeOffAnimation ? " + Off" : "")})", EditorStyles.boldLabel);
        EditorGUILayout.Space();

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

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create", GUILayout.Height(30)))
        {
            bool valid = true;

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

public class CreateSingleDBTTogglesWindow : EditorWindow
{
    private GameObject[] objects;
    private string[] displayNames;
    private Transform animatorRoot;
    private AnimatorController detectedController;

    private List<BlendTree> directBlendTrees = new List<BlendTree>();
    private List<string> directBlendTreeLabels = new List<string>();
    private int selectedTreeIndex = 0;

    private string[] floatParamNames = new string[0];
    private int selectedParamIndex = 0;

    // Per-object blendshape results keyed by instanceID string
    private Dictionary<string, List<(string smrPath, string shapeName, bool enabled)>> blendshapeResults
        = new Dictionary<string, List<(string, string, bool)>>();
    private bool blendshapesSearched = false;

    private Vector2 objectsScrollPos;
    private Vector2 blendshapesScrollPos;

    public static void Show(GameObject[] selectedObjects)
    {
        var window = CreateInstance<CreateSingleDBTTogglesWindow>();
        window.Initialize(selectedObjects);
        window.titleContent = new GUIContent("Create DBT Toggles");
        window.minSize = new Vector2(520, 560);
        window.ShowModal();
    }

    private void Initialize(GameObject[] selectedObjects)
    {
        objects = selectedObjects;
        displayNames = new string[selectedObjects.Length];
        for (int i = 0; i < selectedObjects.Length; i++)
            displayNames[i] = AnimationGeneratorTool.CleanDisplayName(selectedObjects[i].name);

        DetectAnimator();
    }

    private void DetectAnimator()
    {
        if (objects == null || objects.Length == 0) return;

        Transform current = objects[0].transform;
        while (current != null)
        {
            var animator = current.GetComponent<Animator>();
            if (animator != null)
            {
                animatorRoot = current;
                detectedController = animator.runtimeAnimatorController as AnimatorController;
                break;
            }
            current = current.parent;
        }

        if (detectedController != null)
            RefreshControllerData();
    }

    private void RefreshControllerData()
    {
        var found = AnimationGeneratorTool.FindDirectBlendTrees(detectedController);
        directBlendTrees = found.Select(x => x.tree).ToList();
        directBlendTreeLabels = found.Select(x => x.label).ToList();

        floatParamNames = detectedController.parameters
            .Where(p => p.type == AnimatorControllerParameterType.Float)
            .Select(p => p.name)
            .ToArray();

        selectedTreeIndex = Mathf.Clamp(selectedTreeIndex, 0, Mathf.Max(0, directBlendTrees.Count - 1));
        selectedParamIndex = Mathf.Clamp(selectedParamIndex, 0, Mathf.Max(0, floatParamNames.Length - 1));
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Create Single DBT Toggles", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // Controller info
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        if (detectedController != null)
            EditorGUILayout.LabelField("Controller: " + detectedController.name);
        else
            EditorGUILayout.HelpBox("No AnimatorController found. Selected objects must be under an Animator.", MessageType.Error);
        EditorGUILayout.EndVertical();

        if (detectedController == null)
        {
            EditorGUILayout.Space(4);
            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
                Close();
            return;
        }

        EditorGUILayout.Space(4);

        // Direct Blend Tree selector
        if (directBlendTrees.Count == 0)
            EditorGUILayout.HelpBox("No Direct Blend Trees found in the controller.", MessageType.Warning);
        else
            selectedTreeIndex = EditorGUILayout.Popup("Direct Blend Tree:", selectedTreeIndex, directBlendTreeLabels.ToArray());

        // DBT Parameter selector
        if (floatParamNames.Length == 0)
            EditorGUILayout.HelpBox("No Float parameters found. Add a Float parameter (e.g. 'OneFloat') to the controller first.", MessageType.Warning);
        else
            selectedParamIndex = EditorGUILayout.Popup("DBT Parameter:", selectedParamIndex, floatParamNames);

        EditorGUILayout.Space(6);

        // Object name list
        EditorGUILayout.LabelField("Object Names:", EditorStyles.boldLabel);
        objectsScrollPos = EditorGUILayout.BeginScrollView(objectsScrollPos, GUILayout.Height(Mathf.Min(objects.Length * 22 + 8, 200)));
        for (int i = 0; i < objects.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            string parentName = objects[i].transform.parent != null ? objects[i].transform.parent.name : "Root";
            EditorGUILayout.LabelField(parentName, GUILayout.Width(90));
            EditorGUILayout.LabelField("|", GUILayout.Width(10));
            EditorGUILayout.LabelField(objects[i].name, EditorStyles.miniLabel, GUILayout.Width(160));
            EditorGUILayout.LabelField("->", GUILayout.Width(20));
            displayNames[i] = EditorGUILayout.TextField(displayNames[i]);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(6);

        // Blendshapes section
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Blendshapes (CLIPPING/ prefix):", EditorStyles.boldLabel);
        if (GUILayout.Button("Search", GUILayout.Width(65)))
            SearchBlendshapes();
        EditorGUILayout.EndHorizontal();

        if (blendshapesSearched)
        {
            blendshapesScrollPos = EditorGUILayout.BeginScrollView(blendshapesScrollPos, GUILayout.Height(140));
            bool anyFound = false;
            for (int i = 0; i < objects.Length; i++)
            {
                string key = objects[i].GetInstanceID().ToString();
                if (!blendshapeResults.TryGetValue(key, out var bsList) || bsList.Count == 0)
                    continue;
                anyFound = true;
                EditorGUILayout.LabelField(displayNames[i], EditorStyles.boldLabel);
                for (int j = 0; j < bsList.Count; j++)
                {
                    var entry = bsList[j];
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(12);
                    bool newEnabled = EditorGUILayout.ToggleLeft($"{entry.shapeName}  ({entry.smrPath})", entry.enabled);
                    if (newEnabled != entry.enabled)
                        bsList[j] = (entry.smrPath, entry.shapeName, newEnabled);
                    EditorGUILayout.EndHorizontal();
                    blendshapeResults[key] = bsList;
                }
            }
            if (!anyFound)
                EditorGUILayout.LabelField("No matching blendshapes found.", EditorStyles.miniLabel);
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.LabelField("Click Search to find CLIPPING/ blendshapes.", EditorStyles.miniLabel);
        }

        GUILayout.FlexibleSpace();

        // Buttons pinned to bottom
        bool canCreate = directBlendTrees.Count > 0 && floatParamNames.Length > 0;
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = canCreate;
        if (GUILayout.Button("Create", GUILayout.Height(30)))
        {
            if (ValidateInputs())
            {
                var selectedBlendshapes = BuildSelectedBlendshapes();
                AnimationGeneratorTool.ExecuteCreateSingleDBTToggles(
                    objects,
                    displayNames,
                    detectedController,
                    directBlendTrees[selectedTreeIndex],
                    floatParamNames[selectedParamIndex],
                    selectedBlendshapes);
                Close();
            }
        }
        GUI.enabled = true;
        if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            Close();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
    }

    private void SearchBlendshapes()
    {
        blendshapeResults.Clear();
        if (animatorRoot == null) return;

        for (int i = 0; i < objects.Length; i++)
        {
            string key = objects[i].GetInstanceID().ToString();
            var raw = AnimationGeneratorTool.FindClippingBlendshapes(displayNames[i], animatorRoot);
            blendshapeResults[key] = raw.Select(x => (x.smrPath, x.shapeName, true)).ToList();
        }
        blendshapesSearched = true;
        Repaint();
    }

    private bool ValidateInputs()
    {
        for (int i = 0; i < displayNames.Length; i++)
        {
            if (string.IsNullOrEmpty(displayNames[i]))
            {
                EditorUtility.DisplayDialog("Error", $"Please enter a name for object {i} ({objects[i].name})", "OK");
                return false;
            }
        }
        return true;
    }

    private Dictionary<string, List<(string smrPath, string shapeName)>> BuildSelectedBlendshapes()
    {
        var result = new Dictionary<string, List<(string, string)>>();
        foreach (var kvp in blendshapeResults)
        {
            result[kvp.Key] = kvp.Value
                .Where(x => x.enabled)
                .Select(x => (x.smrPath, x.shapeName))
                .ToList();
        }
        return result;
    }
}
