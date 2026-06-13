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

    [MenuItem("GameObject/Terrorized/Create DBT Material Swaps", false, 16)]
    private static void CreateDBTMaterialSwaps(MenuCommand menuCommand)
    {
        if (menuCommand.context != Selection.activeObject) return;
        GameObject[] selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0) return;
        CreateDBTMaterialSwapsWindow.Show(selected);
    }

    [MenuItem("GameObject/Terrorized/Create DBT Group Material Swap", false, 17)]
    private static void CreateDBTGroupMaterialSwap(MenuCommand menuCommand)
    {
        if (menuCommand.context != Selection.activeObject) return;
        GameObject[] selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0) return;
        CreateDBTGroupMaterialSwapWindow.Show(selected);
    }

    [MenuItem("GameObject/Terrorized/Create DBT Decal Reveal", false, 18)]
    private static void CreateDBTDecalReveal(MenuCommand menuCommand)
    {
        if (menuCommand.context != Selection.activeObject) return;
        GameObject[] selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0) return;
        CreateDBTDecalRevealWindow.Show(selected);
    }

    [MenuItem("GameObject/Terrorized/Create DBT Group Decal Reveal", false, 19)]
    private static void CreateDBTGroupDecalReveal(MenuCommand menuCommand)
    {
        if (menuCommand.context != Selection.activeObject) return;
        GameObject[] selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0) return;
        CreateDBTGroupDecalRevealWindow.Show(selected);
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

    // ─── DBT Material Swap helpers ────────────────────────────────────────────

    public static AnimationClip CreateMaterialSwapAnimation(
        GameObject obj, string clipName, int materialSlot, Material material)
    {
        EnsureAssetFolder();
        string savePath = Path.Combine(ASSET_FOLDER, clipName + ".anim");
        string targetPath = GetGameObjectPath(obj);

        AnimationClip clip = new AnimationClip();
        clip.name = clipName;

        EditorCurveBinding binding = new EditorCurveBinding
        {
            path = targetPath,
            type = typeof(SkinnedMeshRenderer),
            propertyName = "m_Materials.Array.data[" + materialSlot + "]"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[]
        {
            new ObjectReferenceKeyframe { time = 0f, value = material }
        };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, savePath);
        return clip;
    }

    public static void ExecuteCreateDBTMaterialSwaps(
        GameObject[] objects,
        string[] assetNames,
        int[] materialSlots,
        List<Material>[] extraMaterials,
        AnimatorController controller,
        BlendTree targetDBT,
        string dbtParameter)
    {
        EnsureAssetFolder();
        Undo.RecordObject(controller, "Create DBT Material Swaps");
        Undo.RecordObject(targetDBT, "Create DBT Material Swaps");

        // Group object indices by parent name (category), preserving insertion order
        var groups = new List<(string category, List<int> itemIndices)>();
        var groupIndex = new Dictionary<string, int>();

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null) continue;
            if (objects[i].GetComponent<SkinnedMeshRenderer>() == null) continue;

            string parentName = objects[i].transform.parent != null ? objects[i].transform.parent.name : "Root";
            if (!groupIndex.TryGetValue(parentName, out int idx))
            {
                idx = groups.Count;
                groupIndex[parentName] = idx;
                groups.Add((parentName, new List<int>()));
            }
            groups[idx].itemIndices.Add(i);
        }

        int totalAssets = 0;
        int totalClips = 0;

        foreach (var (category, itemIndices) in groups)
        {
            // Visual separator trigger (matches DBT Toggles convention; no-op if it already exists)
            EnsureAnimatorTrigger(controller, $"-----{category}-----");

            BlendTree groupTree = new BlendTree();
            groupTree.name = category;
            groupTree.blendType = BlendTreeType.Direct;
            groupTree.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(groupTree, controller);
            Undo.RegisterCreatedObjectUndo(groupTree, "Create DBT Material Swaps");

            targetDBT.AddChild(groupTree);
            var targetChildren = targetDBT.children;
            targetChildren[targetChildren.Length - 1].directBlendParameter = dbtParameter;
            targetDBT.children = targetChildren;

            foreach (int i in itemIndices)
            {
                var smr = objects[i].GetComponent<SkinnedMeshRenderer>();
                var sharedMats = smr.sharedMaterials;
                int slot = Mathf.Clamp(materialSlots[i], 0, Mathf.Max(0, sharedMats.Length - 1));
                Material defaultMat = sharedMats.Length > 0 ? sharedMats[slot] : null;
                var extras = extraMaterials[i] ?? new List<Material>();

                string assetName = assetNames[i];
                string paramName = $"Materials/{category}/{assetName}";
                string animBase = $"Materials.{category}.{assetName}";

                EnsureAnimatorParameter(controller, paramName);

                // Build clips: index 0 = default, 1..N = extras
                var clips = new List<AnimationClip>();
                clips.Add(CreateMaterialSwapAnimation(objects[i], animBase + ".0", slot, defaultMat));
                for (int m = 0; m < extras.Count; m++)
                {
                    clips.Add(CreateMaterialSwapAnimation(objects[i], animBase + "." + (m + 1), slot, extras[m]));
                }
                totalClips += clips.Count;

                // Simple1D tree driven by the per-asset float parameter
                BlendTree oneDTree = new BlendTree();
                oneDTree.name = assetName;
                oneDTree.blendType = BlendTreeType.Simple1D;
                oneDTree.blendParameter = paramName;
                oneDTree.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(oneDTree, controller);
                Undo.RegisterCreatedObjectUndo(oneDTree, "Create DBT Material Swaps");

                for (int m = 0; m < clips.Count; m++)
                    oneDTree.AddChild(clips[m], (float)m);

                groupTree.AddChild(oneDTree);
                var groupChildren = groupTree.children;
                groupChildren[groupChildren.Length - 1].directBlendParameter = dbtParameter;
                groupTree.children = groupChildren;

                totalAssets++;
            }
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Success",
            $"Created {totalAssets} material swap{(totalAssets == 1 ? "" : "s")} ({totalClips} clip{(totalClips == 1 ? "" : "s")}).",
            "OK");
    }

    // ─── DBT Group Material Swap helpers ──────────────────────────────────────

    public struct MaterialTarget
    {
        public string smrPath;
        public int slot;
        public Material material;  // null = clear; for default state usually the slot's existing material
    }

    // Creates one clip that drives multiple (smrPath, slot) bindings to specific materials.
    public static AnimationClip CreateGroupMaterialSwapAnimation(string clipName, List<MaterialTarget> targets)
    {
        EnsureAssetFolder();
        string savePath = Path.Combine(ASSET_FOLDER, clipName + ".anim");

        AnimationClip clip = new AnimationClip();
        clip.name = clipName;

        foreach (var t in targets)
        {
            EditorCurveBinding binding = new EditorCurveBinding
            {
                path = t.smrPath,
                type = typeof(SkinnedMeshRenderer),
                propertyName = "m_Materials.Array.data[" + t.slot + "]"
            };
            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[]
            {
                new ObjectReferenceKeyframe { time = 0f, value = t.material }
            };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        }

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, savePath);
        return clip;
    }

    // For a given material reference, returns (smrPath, slotIndex) for every slot in every SMR that uses it.
    public static List<(string smrPath, int slot)> FindMaterialSlots(GameObject[] objects, Transform animatorRoot, Material material)
    {
        var results = new List<(string, int)>();
        if (material == null) return results;

        foreach (var go in objects)
        {
            if (go == null) continue;
            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (smr == null) continue;

            string smrPath = GetGameObjectPath(go);
            var mats = smr.sharedMaterials;
            for (int s = 0; s < mats.Length; s++)
            {
                if (mats[s] == material)
                    results.Add((smrPath, s));
            }
        }
        return results;
    }

    // Returns materials that appear at least once in EVERY SMR among `objects`.
    public static List<Material> FindSharedMaterials(GameObject[] objects)
    {
        var validSmrs = objects
            .Where(o => o != null && o.GetComponent<SkinnedMeshRenderer>() != null)
            .Select(o => o.GetComponent<SkinnedMeshRenderer>())
            .ToList();

        if (validSmrs.Count == 0) return new List<Material>();

        // Start with the unique set from the first SMR (preserve order)
        HashSet<Material> seen = new HashSet<Material>();
        List<Material> candidates = new List<Material>();
        foreach (var m in validSmrs[0].sharedMaterials)
        {
            if (m != null && seen.Add(m))
                candidates.Add(m);
        }

        // Intersect against each subsequent SMR
        for (int i = 1; i < validSmrs.Count; i++)
        {
            var these = new HashSet<Material>(validSmrs[i].sharedMaterials.Where(m => m != null));
            candidates = candidates.Where(m => these.Contains(m)).ToList();
            if (candidates.Count == 0) break;
        }

        return candidates;
    }

    public static void ExecuteCreateDBTGroupMaterialSwap(
        GameObject[] objects,
        string category,
        string assetName,
        Material sharedMaterial,
        List<Material> extraMaterials,
        AnimatorController controller,
        BlendTree targetDBT,
        string dbtParameter)
    {
        EnsureAssetFolder();
        Undo.RecordObject(controller, "Create DBT Group Material Swap");
        Undo.RecordObject(targetDBT, "Create DBT Group Material Swap");

        // Resolve every (smr, slot) currently using the shared material
        var slotMap = FindMaterialSlots(objects, null, sharedMaterial);
        if (slotMap.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "Selected shared material isn't on any of the selected meshes anymore.", "OK");
            return;
        }

        // Visual separator + category group blendtree (matches DBT Toggles convention)
        EnsureAnimatorTrigger(controller, $"-----{category}-----");

        BlendTree groupTree = new BlendTree();
        groupTree.name = category;
        groupTree.blendType = BlendTreeType.Direct;
        groupTree.hideFlags = HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(groupTree, controller);
        Undo.RegisterCreatedObjectUndo(groupTree, "Create DBT Group Material Swap");

        targetDBT.AddChild(groupTree);
        var targetChildren = targetDBT.children;
        targetChildren[targetChildren.Length - 1].directBlendParameter = dbtParameter;
        targetDBT.children = targetChildren;

        string paramName = $"Materials/{category}/{assetName}";
        string animBase = $"Materials.{category}.{assetName}";

        EnsureAnimatorParameter(controller, paramName);

        // Build the default clip (state 0): every found slot pinned back to the shared material
        var defaultTargets = slotMap.Select(p => new MaterialTarget
        {
            smrPath = p.smrPath,
            slot = p.slot,
            material = sharedMaterial
        }).ToList();

        var clips = new List<AnimationClip>();
        clips.Add(CreateGroupMaterialSwapAnimation(animBase + ".0", defaultTargets));

        // Build a clip per extra material: every found slot drives to that material
        for (int e = 0; e < extraMaterials.Count; e++)
        {
            var mat = extraMaterials[e];
            var targets = slotMap.Select(p => new MaterialTarget
            {
                smrPath = p.smrPath,
                slot = p.slot,
                material = mat
            }).ToList();
            clips.Add(CreateGroupMaterialSwapAnimation(animBase + "." + (e + 1), targets));
        }

        // Simple1D tree driven by the new float parameter
        BlendTree oneDTree = new BlendTree();
        oneDTree.name = assetName;
        oneDTree.blendType = BlendTreeType.Simple1D;
        oneDTree.blendParameter = paramName;
        oneDTree.hideFlags = HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(oneDTree, controller);
        Undo.RegisterCreatedObjectUndo(oneDTree, "Create DBT Group Material Swap");

        for (int m = 0; m < clips.Count; m++)
            oneDTree.AddChild(clips[m], (float)m);

        groupTree.AddChild(oneDTree);
        var groupChildren = groupTree.children;
        groupChildren[groupChildren.Length - 1].directBlendParameter = dbtParameter;
        groupTree.children = groupChildren;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Success",
            $"Created group material swap '{category}/{assetName}' ({clips.Count} clips across {slotMap.Count} slot binding{(slotMap.Count == 1 ? "" : "s")}).",
            "OK");
    }

    // ─── DBT Decal Reveal helpers ─────────────────────────────────────────────

    // Returns the BASE names (e.g. "_DecalBlendAlpha", "_DecalHueShift1") of a Poiyomi material's
    // decal properties marked animated, ordered by (decal index, alpha-before-hue, name).
    //
    // Poiyomi/Thry stores the "animated" flag as a material string TAG "<Base>Animated" = "1"
    // ("2" = animated AND renamed when locked) in stringTagMap — keyed to the ORIGINAL name.
    // On an unlocked material the shader property still uses that base name. On a LOCKED material
    // Poiyomi renames the property to "<Base>_<suffix>" (e.g. "_DecalBlendAlpha_Eyes") while the tag
    // stays keyed to the base — so we strip the rename suffix before checking the tag. Use
    // ResolveBoundDecalProp to turn a base name into the actual property name to animate.
    public static List<string> FindAnimatedDecalBaseProps(Material mat)
    {
        var results = new List<string>();
        if (mat == null || mat.shader == null) return results;

        var shader = mat.shader;
        int count = ShaderUtil.GetPropertyCount(shader);
        var seen = new HashSet<string>();

        for (int i = 0; i < count; i++)
        {
            var ptype = ShaderUtil.GetPropertyType(shader, i);
            // Only float-like props make sense for a 0..1 reveal (alpha, hue shift, etc.)
            if (ptype != ShaderUtil.ShaderPropertyType.Float && ptype != ShaderUtil.ShaderPropertyType.Range)
                continue;

            string propName = ShaderUtil.GetPropertyName(shader, i);
            if (propName.IndexOf("Decal", System.StringComparison.OrdinalIgnoreCase) < 0) continue;

            // Try the property name directly (unlocked), then with the lock-rename suffix stripped.
            string baseName = propName;
            if (!IsAnimatedTag(mat, baseName))
            {
                int li = propName.LastIndexOf('_');   // index 0 is the leading "_"; >0 means a suffix
                if (li <= 0) continue;
                baseName = propName.Substring(0, li);
                if (!IsAnimatedTag(mat, baseName)) continue;
            }

            if (seen.Add(baseName))
                results.Add(baseName);
        }

        results.Sort((a, b) =>
        {
            int ia = DecalIndexOf(a), ib = DecalIndexOf(b);
            if (ia != ib) return ia.CompareTo(ib);
            int ta = DecalTypeRank(a), tb = DecalTypeRank(b);
            if (ta != tb) return ta.CompareTo(tb);
            return string.Compare(a, b, System.StringComparison.Ordinal);
        });

        return results;
    }

    // True if the material flags this base property as animated. "1" = animated,
    // "2" = animated + renamed when locked (RA).
    private static bool IsAnimatedTag(Material mat, string baseProp)
    {
        string tag = mat.GetTag(baseProp + "Animated", false, "");
        return tag == "1" || tag == "2";
    }

    // True if the base property is tagged "2" (renamed-when-locked / RA).
    public static bool IsRenamedAnimated(Material mat, string baseProp)
    {
        return mat != null && mat.GetTag(baseProp + "Animated", false, "") == "2";
    }

    // Turns a base property name into the actual shader property to animate on this material.
    // Unlocked: the base name itself. Locked + renamed (RA): the "<Base>_<suffix>" property that
    // Poiyomi generated. Falls back to the base name if nothing better is found.
    public static string ResolveBoundDecalProp(Material mat, string baseProp)
    {
        if (mat == null) return baseProp;
        if (mat.HasProperty(baseProp)) return baseProp;   // unlocked: original name is valid

        var shader = mat.shader;
        if (shader != null)
        {
            int count = ShaderUtil.GetPropertyCount(shader);
            string prefix = baseProp + "_";
            for (int i = 0; i < count; i++)
            {
                var ptype = ShaderUtil.GetPropertyType(shader, i);
                if (ptype != ShaderUtil.ShaderPropertyType.Float && ptype != ShaderUtil.ShaderPropertyType.Range)
                    continue;
                string name = ShaderUtil.GetPropertyName(shader, i);
                if (name.StartsWith(prefix, System.StringComparison.Ordinal))
                    return name;
            }
        }
        return baseProp;
    }

    // Trailing digits of a property name → decal index (no digits ⇒ 0). "_DecalHueShift1" → 1.
    private static int DecalIndexOf(string propName)
    {
        int end = propName.Length;
        int start = end;
        while (start > 0 && char.IsDigit(propName[start - 1])) start--;
        if (start == end) return 0;
        return int.TryParse(propName.Substring(start, end - start), out int idx) ? idx : 0;
    }

    // alpha (0) before hue (1) before anything else (2).
    private static int DecalTypeRank(string propName)
    {
        if (propName.IndexOf("Alpha", System.StringComparison.OrdinalIgnoreCase) >= 0) return 0;
        if (propName.IndexOf("Hue", System.StringComparison.OrdinalIgnoreCase) >= 0) return 1;
        return 2;
    }

    // One reveal clip on a single renderer: the first `onCount` properties are driven to 1, the
    // rest to 0 (constant). `boundProps` must be the resolved shader property names for `obj`.
    public static AnimationClip CreateDecalRevealAnimation(
        GameObject obj, string clipName, List<string> boundProps, int onCount)
    {
        EnsureAssetFolder();
        string savePath = Path.Combine(ASSET_FOLDER, clipName + ".anim");
        string targetPath = GetGameObjectPath(obj);

        AnimationClip clip = new AnimationClip();
        clip.name = clipName;

        for (int j = 0; j < boundProps.Count; j++)
            AddRevealCurve(clip, targetPath, boundProps[j], j < onCount ? 1f : 0f);

        FinalizeRevealClip(clip, savePath);
        return clip;
    }

    // One reveal clip across MANY renderers (the group variant). `targets` is one entry per renderer
    // holding that renderer's path and its resolved bound property names (same order as the base set).
    public static AnimationClip CreateGroupDecalRevealAnimation(
        string clipName, List<(string smrPath, List<string> boundProps)> targets, int onCount)
    {
        EnsureAssetFolder();
        string savePath = Path.Combine(ASSET_FOLDER, clipName + ".anim");

        AnimationClip clip = new AnimationClip();
        clip.name = clipName;

        foreach (var (smrPath, boundProps) in targets)
            for (int j = 0; j < boundProps.Count; j++)
                AddRevealCurve(clip, smrPath, boundProps[j], j < onCount ? 1f : 0f);

        FinalizeRevealClip(clip, savePath);
        return clip;
    }

    private static void AddRevealCurve(AnimationClip clip, string targetPath, string boundProp, float value)
    {
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0f, value));
        AnimationUtility.SetKeyLeftTangentMode(curve, 0, AnimationUtility.TangentMode.Constant);
        AnimationUtility.SetKeyRightTangentMode(curve, 0, AnimationUtility.TangentMode.Constant);

        EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
            targetPath, typeof(Renderer), "material." + boundProp);
        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }

    private static void FinalizeRevealClip(AnimationClip clip, string savePath)
    {
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AssetDatabase.CreateAsset(clip, savePath);
    }

    // Base animated decal props read from a specific material slot on the mesh.
    public static List<string> GetMeshAnimatedDecalBaseProps(GameObject obj, int slot)
    {
        var mat = GetSlotMaterial(obj, slot);
        return mat != null ? FindAnimatedDecalBaseProps(mat) : new List<string>();
    }

    public static Material GetSlotMaterial(GameObject obj, int slot)
    {
        if (obj == null) return null;
        var smr = obj.GetComponent<SkinnedMeshRenderer>();
        if (smr == null) return null;
        var mats = smr.sharedMaterials;
        if (mats == null || mats.Length == 0) return null;
        slot = Mathf.Clamp(slot, 0, mats.Length - 1);
        return mats[slot];
    }

    // First material slot whose material has animated decal props, or 0 if none qualify.
    public static int AutoDetectDecalSlot(GameObject obj)
    {
        if (obj == null) return 0;
        var smr = obj.GetComponent<SkinnedMeshRenderer>();
        if (smr == null) return 0;
        var mats = smr.sharedMaterials;
        for (int s = 0; s < mats.Length; s++)
            if (FindAnimatedDecalBaseProps(mats[s]).Count > 0) return s;
        return 0;
    }

    public static void ExecuteCreateDBTDecalReveal(
        GameObject[] objects,
        string[] assetNames,
        int[] materialSlots,
        AnimatorController controller,
        BlendTree targetDBT,
        string dbtParameter)
    {
        EnsureAssetFolder();
        Undo.RecordObject(controller, "Create DBT Decal Reveal");
        Undo.RecordObject(targetDBT, "Create DBT Decal Reveal");

        // Group object indices by parent name (category), preserving insertion order
        var groups = new List<(string category, List<int> itemIndices)>();
        var groupIndex = new Dictionary<string, int>();

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null) continue;
            if (objects[i].GetComponent<SkinnedMeshRenderer>() == null) continue;
            if (GetMeshAnimatedDecalBaseProps(objects[i], materialSlots[i]).Count == 0) continue;

            string parentName = objects[i].transform.parent != null ? objects[i].transform.parent.name : "Root";
            if (!groupIndex.TryGetValue(parentName, out int idx))
            {
                idx = groups.Count;
                groupIndex[parentName] = idx;
                groups.Add((parentName, new List<int>()));
            }
            groups[idx].itemIndices.Add(i);
        }

        int totalAssets = 0;
        int totalClips = 0;

        foreach (var (category, itemIndices) in groups)
        {
            // Visual separator trigger (matches the other DBT features; no-op if it already exists)
            EnsureAnimatorTrigger(controller, $"-----{category}-----");

            BlendTree groupTree = new BlendTree();
            groupTree.name = category;
            groupTree.blendType = BlendTreeType.Direct;
            groupTree.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(groupTree, controller);
            Undo.RegisterCreatedObjectUndo(groupTree, "Create DBT Decal Reveal");

            targetDBT.AddChild(groupTree);
            var targetChildren = targetDBT.children;
            targetChildren[targetChildren.Length - 1].directBlendParameter = dbtParameter;
            targetDBT.children = targetChildren;

            foreach (int i in itemIndices)
            {
                var slotMat = GetSlotMaterial(objects[i], materialSlots[i]);
                var baseProps = FindAnimatedDecalBaseProps(slotMat);
                // Resolve each base prop to the actual property to animate on this material
                // (handles Poiyomi's lock-rename for RA props).
                var boundProps = baseProps.Select(b => ResolveBoundDecalProp(slotMat, b)).ToList();

                string assetName = assetNames[i];
                string paramName = $"Decals/{category}/{assetName}";
                string animBase = $"Decals.{category}.{assetName}";

                EnsureAnimatorParameter(controller, paramName);

                // Cumulative reveal: clip k turns on the first k properties (k = 0..N)
                var clips = new List<AnimationClip>();
                for (int k = 0; k <= boundProps.Count; k++)
                    clips.Add(CreateDecalRevealAnimation(objects[i], animBase + "." + k, boundProps, k));
                totalClips += clips.Count;

                // Simple1D tree driven by the per-asset float parameter, thresholds 0..N
                BlendTree oneDTree = new BlendTree();
                oneDTree.name = assetName;
                oneDTree.blendType = BlendTreeType.Simple1D;
                oneDTree.blendParameter = paramName;
                oneDTree.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(oneDTree, controller);
                Undo.RegisterCreatedObjectUndo(oneDTree, "Create DBT Decal Reveal");

                for (int m = 0; m < clips.Count; m++)
                    oneDTree.AddChild(clips[m], (float)m);

                groupTree.AddChild(oneDTree);
                var groupChildren = groupTree.children;
                groupChildren[groupChildren.Length - 1].directBlendParameter = dbtParameter;
                groupTree.children = groupChildren;

                totalAssets++;
            }
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (totalAssets == 0)
        {
            EditorUtility.DisplayDialog(
                "Nothing Created",
                "None of the selected meshes have any decal properties marked as animated.",
                "OK");
            return;
        }

        EditorUtility.DisplayDialog(
            "Success",
            $"Created {totalAssets} decal reveal{(totalAssets == 1 ? "" : "s")} ({totalClips} clip{(totalClips == 1 ? "" : "s")}).",
            "OK");
    }

    // ─── DBT Group Decal Reveal ───────────────────────────────────────────────

    // One control (Simple1D + float param) drives the same cumulative decal reveal across every
    // mesh that uses `sharedMaterial`. clip k turns on the first k animated decal props on all of
    // them at once. Bound property names are resolved from the shared material (handles RA rename).
    public static void ExecuteCreateDBTGroupDecalReveal(
        GameObject[] objects,
        string category,
        string assetName,
        Material sharedMaterial,
        AnimatorController controller,
        BlendTree targetDBT,
        string dbtParameter)
    {
        EnsureAssetFolder();
        Undo.RecordObject(controller, "Create DBT Group Decal Reveal");
        Undo.RecordObject(targetDBT, "Create DBT Group Decal Reveal");

        var baseProps = FindAnimatedDecalBaseProps(sharedMaterial);
        if (baseProps.Count == 0)
        {
            EditorUtility.DisplayDialog("Nothing Created",
                "The selected material has no decal properties marked as animated.", "OK");
            return;
        }

        // Every renderer (path) that uses the shared material gets the same reveal.
        var slotMap = FindMaterialSlots(objects, null, sharedMaterial);
        var smrPaths = slotMap.Select(p => p.smrPath).Distinct().ToList();
        if (smrPaths.Count == 0)
        {
            EditorUtility.DisplayDialog("Error",
                "The selected material isn't on any of the selected meshes anymore.", "OK");
            return;
        }

        var boundProps = baseProps.Select(b => ResolveBoundDecalProp(sharedMaterial, b)).ToList();
        var targets = smrPaths.Select(p => (p, boundProps)).ToList();

        EnsureAnimatorTrigger(controller, $"-----{category}-----");

        BlendTree groupTree = new BlendTree();
        groupTree.name = category;
        groupTree.blendType = BlendTreeType.Direct;
        groupTree.hideFlags = HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(groupTree, controller);
        Undo.RegisterCreatedObjectUndo(groupTree, "Create DBT Group Decal Reveal");

        targetDBT.AddChild(groupTree);
        var targetChildren = targetDBT.children;
        targetChildren[targetChildren.Length - 1].directBlendParameter = dbtParameter;
        targetDBT.children = targetChildren;

        string paramName = $"Decals/{category}/{assetName}";
        string animBase = $"Decals.{category}.{assetName}";
        EnsureAnimatorParameter(controller, paramName);

        var clips = new List<AnimationClip>();
        for (int k = 0; k <= boundProps.Count; k++)
            clips.Add(CreateGroupDecalRevealAnimation(animBase + "." + k, targets, k));

        BlendTree oneDTree = new BlendTree();
        oneDTree.name = assetName;
        oneDTree.blendType = BlendTreeType.Simple1D;
        oneDTree.blendParameter = paramName;
        oneDTree.hideFlags = HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(oneDTree, controller);
        Undo.RegisterCreatedObjectUndo(oneDTree, "Create DBT Group Decal Reveal");

        for (int m = 0; m < clips.Count; m++)
            oneDTree.AddChild(clips[m], (float)m);

        groupTree.AddChild(oneDTree);
        var groupChildren = groupTree.children;
        groupChildren[groupChildren.Length - 1].directBlendParameter = dbtParameter;
        groupTree.children = groupChildren;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Success",
            $"Created group decal reveal '{category}/{assetName}' ({clips.Count} clips across {smrPaths.Count} mesh{(smrPaths.Count == 1 ? "" : "es")}).",
            "OK");
    }

    // ─── Shared ───────────────────────────────────────────────────────────────

    // Accepts material drops into the given rect; returns true if anything was added.
    // Call this AFTER drawing a section, using GUILayoutUtility.GetLastRect() for the area.
    public static bool HandleMaterialDragAndDrop(Rect dropArea, List<Material> target)
    {
        Event evt = Event.current;
        if (evt == null) return false;
        if (!dropArea.Contains(evt.mousePosition)) return false;

        if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            return false;

        bool hasMaterial = false;
        foreach (var obj in DragAndDrop.objectReferences)
        {
            if (obj is Material) { hasMaterial = true; break; }
        }
        if (!hasMaterial) return false;

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is Material mat)
                    target.Add(mat);
            }
            evt.Use();
            return true;
        }
        return false;
    }

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

public class CreateDBTMaterialSwapsWindow : EditorWindow
{
    private GameObject[] objects;
    private string[] assetNames;
    private int[] materialSlotIndices;
    private List<Material>[] extraMaterials;
    private bool[] foldoutStates;

    private Transform animatorRoot;
    private AnimatorController detectedController;

    private List<BlendTree> directBlendTrees = new List<BlendTree>();
    private List<string> directBlendTreeLabels = new List<string>();
    private int selectedTreeIndex = 0;

    private string[] floatParamNames = new string[0];
    private int selectedParamIndex = 0;

    private Vector2 mainScrollPos;

    public static void Show(GameObject[] selectedObjects)
    {
        var window = CreateInstance<CreateDBTMaterialSwapsWindow>();
        window.Initialize(selectedObjects);
        window.titleContent = new GUIContent("Create DBT Material Swaps");
        window.minSize = new Vector2(560, 600);
        window.ShowUtility();
    }

    private void Initialize(GameObject[] selectedObjects)
    {
        // Keep only objects that actually have a SkinnedMeshRenderer
        var valid = selectedObjects.Where(o => o != null && o.GetComponent<SkinnedMeshRenderer>() != null).ToArray();
        objects = valid;
        assetNames = new string[valid.Length];
        materialSlotIndices = new int[valid.Length];
        extraMaterials = new List<Material>[valid.Length];
        foldoutStates = new bool[valid.Length];

        for (int i = 0; i < valid.Length; i++)
        {
            assetNames[i] = AnimationGeneratorTool.CleanDisplayName(valid[i].name);
            materialSlotIndices[i] = 0;
            extraMaterials[i] = new List<Material>();
            foldoutStates[i] = true;
        }

        DetectAnimator(selectedObjects);
    }

    private void DetectAnimator(GameObject[] sourceObjects)
    {
        if (sourceObjects == null || sourceObjects.Length == 0) return;

        Transform current = sourceObjects[0].transform;
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
        EditorGUILayout.LabelField("Create DBT Material Swaps", EditorStyles.boldLabel);
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

        if (objects == null || objects.Length == 0)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox("None of the selected objects have a SkinnedMeshRenderer.", MessageType.Error);
            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
                Close();
            return;
        }

        EditorGUILayout.Space(4);

        // Tree + parameter selectors
        if (directBlendTrees.Count == 0)
            EditorGUILayout.HelpBox("No Direct Blend Trees found in the controller.", MessageType.Warning);
        else
            selectedTreeIndex = EditorGUILayout.Popup("Direct Blend Tree:", selectedTreeIndex, directBlendTreeLabels.ToArray());

        if (floatParamNames.Length == 0)
            EditorGUILayout.HelpBox("No Float parameters found. Add a Float parameter to the controller first.", MessageType.Warning);
        else
            selectedParamIndex = EditorGUILayout.Popup("DBT Parameter:", selectedParamIndex, floatParamNames);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Material Swaps:", EditorStyles.boldLabel);

        mainScrollPos = EditorGUILayout.BeginScrollView(mainScrollPos);

        for (int i = 0; i < objects.Length; i++)
        {
            var smr = objects[i].GetComponent<SkinnedMeshRenderer>();
            var mats = smr.sharedMaterials;
            string parentName = objects[i].transform.parent != null ? objects[i].transform.parent.name : "Root";

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            foldoutStates[i] = EditorGUILayout.Foldout(
                foldoutStates[i],
                $"{parentName} | {objects[i].name}",
                true,
                EditorStyles.foldoutHeader);

            if (foldoutStates[i])
            {
                EditorGUI.indentLevel++;

                // Asset name
                assetNames[i] = EditorGUILayout.TextField("Asset Name:", assetNames[i]);

                // Material slot dropdown
                if (mats.Length == 0)
                {
                    EditorGUILayout.HelpBox("This SkinnedMeshRenderer has no materials.", MessageType.Warning);
                }
                else
                {
                    string[] slotOptions = new string[mats.Length];
                    for (int s = 0; s < mats.Length; s++)
                    {
                        string matName = mats[s] != null ? mats[s].name : "(None)";
                        slotOptions[s] = $"[{s}] {matName}";
                    }
                    materialSlotIndices[i] = Mathf.Clamp(materialSlotIndices[i], 0, mats.Length - 1);
                    materialSlotIndices[i] = EditorGUILayout.Popup("Material Slot (default):", materialSlotIndices[i], slotOptions);
                }

                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("Extra Materials (Swap Targets):", EditorStyles.miniBoldLabel);

                var extras = extraMaterials[i];
                for (int e = 0; e < extras.Count; e++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"[{e + 1}]", GUILayout.Width(34));
                    extras[e] = (Material)EditorGUILayout.ObjectField(extras[e], typeof(Material), false);
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        extras.RemoveAt(e);
                        e--;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+ Add Material", GUILayout.Width(130)))
                    extras.Add(null);
                EditorGUILayout.EndHorizontal();

                // Preview
                if (mats.Length > 0 && !string.IsNullOrEmpty(assetNames[i]))
                {
                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField("Preview:", EditorStyles.miniBoldLabel);
                    EditorGUILayout.LabelField($"  Param: Materials/{parentName}/{assetNames[i]}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"  Materials.{parentName}.{assetNames[i]}.0  (default)", EditorStyles.miniLabel);
                    for (int e = 0; e < extras.Count; e++)
                        EditorGUILayout.LabelField($"  Materials.{parentName}.{assetNames[i]}.{e + 1}", EditorStyles.miniLabel);
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            // Drag-and-drop: any Material(s) dropped on this asset's box are appended to its extras list
            Rect boxRect = GUILayoutUtility.GetLastRect();
            if (AnimationGeneratorTool.HandleMaterialDragAndDrop(boxRect, extraMaterials[i]))
                Repaint();

            EditorGUILayout.Space(2);
        }

        EditorGUILayout.EndScrollView();

        GUILayout.FlexibleSpace();

        // Bottom buttons
        bool canCreate = directBlendTrees.Count > 0 && floatParamNames.Length > 0;
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = canCreate;
        if (GUILayout.Button("Create", GUILayout.Height(30)))
        {
            if (ValidateInputs())
            {
                AnimationGeneratorTool.ExecuteCreateDBTMaterialSwaps(
                    objects,
                    assetNames,
                    materialSlotIndices,
                    extraMaterials,
                    detectedController,
                    directBlendTrees[selectedTreeIndex],
                    floatParamNames[selectedParamIndex]);
                Close();
            }
        }
        GUI.enabled = true;
        if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            Close();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
    }

    private bool ValidateInputs()
    {
        bool anyExtras = false;

        for (int i = 0; i < objects.Length; i++)
        {
            if (string.IsNullOrEmpty(assetNames[i]))
            {
                EditorUtility.DisplayDialog("Error", $"Please enter an asset name for {objects[i].name}.", "OK");
                return false;
            }

            var smr = objects[i].GetComponent<SkinnedMeshRenderer>();
            if (smr.sharedMaterials.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", $"{objects[i].name} has no materials on its SkinnedMeshRenderer.", "OK");
                return false;
            }

            for (int e = 0; e < extraMaterials[i].Count; e++)
            {
                if (extraMaterials[i][e] == null)
                {
                    EditorUtility.DisplayDialog(
                        "Error",
                        $"{objects[i].name}: extra material slot [{e + 1}] is empty. Assign or remove it.",
                        "OK");
                    return false;
                }
            }

            if (extraMaterials[i].Count > 0) anyExtras = true;
        }

        if (!anyExtras)
        {
            return EditorUtility.DisplayDialog(
                "No Extra Materials",
                "No extra materials were added to any asset. Only default (.0) clips will be created. Continue?",
                "Continue",
                "Cancel");
        }

        return true;
    }
}

public class CreateDBTGroupMaterialSwapWindow : EditorWindow
{
    private GameObject[] objects;
    private string categoryName = "";
    private string assetName = "";

    private List<Material> sharedMaterials = new List<Material>();
    private int selectedSharedIndex = 0;

    private List<Material> extraMaterials = new List<Material>();

    private Transform animatorRoot;
    private AnimatorController detectedController;

    private List<BlendTree> directBlendTrees = new List<BlendTree>();
    private List<string> directBlendTreeLabels = new List<string>();
    private int selectedTreeIndex = 0;

    private string[] floatParamNames = new string[0];
    private int selectedParamIndex = 0;

    private Vector2 mainScrollPos;

    public static void Show(GameObject[] selectedObjects)
    {
        var window = CreateInstance<CreateDBTGroupMaterialSwapWindow>();
        window.Initialize(selectedObjects);
        window.titleContent = new GUIContent("Create DBT Group Material Swap");
        window.minSize = new Vector2(560, 600);
        window.ShowUtility();
    }

    private void Initialize(GameObject[] selectedObjects)
    {
        var valid = selectedObjects.Where(o => o != null && o.GetComponent<SkinnedMeshRenderer>() != null).ToArray();
        objects = valid;

        // Default category = common parent name if all share one; otherwise blank
        if (valid.Length > 0)
        {
            string firstParent = valid[0].transform.parent != null ? valid[0].transform.parent.name : "";
            bool allSame = true;
            for (int i = 1; i < valid.Length; i++)
            {
                string p = valid[i].transform.parent != null ? valid[i].transform.parent.name : "";
                if (p != firstParent) { allSame = false; break; }
            }
            if (allSame && !string.IsNullOrEmpty(firstParent))
                categoryName = firstParent;
        }

        sharedMaterials = AnimationGeneratorTool.FindSharedMaterials(valid);

        DetectAnimator(selectedObjects);
    }

    private void DetectAnimator(GameObject[] sourceObjects)
    {
        if (sourceObjects == null || sourceObjects.Length == 0) return;

        Transform current = sourceObjects[0].transform;
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
        EditorGUILayout.LabelField("Create DBT Group Material Swap", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        if (detectedController != null)
            EditorGUILayout.LabelField("Controller: " + detectedController.name);
        else
            EditorGUILayout.HelpBox("No AnimatorController found. Selected objects must be under an Animator.", MessageType.Error);
        EditorGUILayout.EndVertical();

        if (detectedController == null)
        {
            EditorGUILayout.Space(4);
            if (GUILayout.Button("Cancel", GUILayout.Height(30))) Close();
            return;
        }

        if (objects == null || objects.Length == 0)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox("None of the selected objects have a SkinnedMeshRenderer.", MessageType.Error);
            if (GUILayout.Button("Cancel", GUILayout.Height(30))) Close();
            return;
        }

        EditorGUILayout.Space(4);

        if (directBlendTrees.Count == 0)
            EditorGUILayout.HelpBox("No Direct Blend Trees found in the controller.", MessageType.Warning);
        else
            selectedTreeIndex = EditorGUILayout.Popup("Direct Blend Tree:", selectedTreeIndex, directBlendTreeLabels.ToArray());

        if (floatParamNames.Length == 0)
            EditorGUILayout.HelpBox("No Float parameters found. Add a Float parameter to the controller first.", MessageType.Warning);
        else
            selectedParamIndex = EditorGUILayout.Popup("DBT Parameter:", selectedParamIndex, floatParamNames);

        EditorGUILayout.Space(6);

        // Naming
        EditorGUILayout.LabelField("Naming:", EditorStyles.boldLabel);
        categoryName = EditorGUILayout.TextField("Category:", categoryName);
        assetName    = EditorGUILayout.TextField("Name:",     assetName);

        EditorGUILayout.Space(6);

        // Selected objects summary
        EditorGUILayout.LabelField($"Targets ({objects.Length}):", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        foreach (var go in objects)
        {
            string parent = go.transform.parent != null ? go.transform.parent.name : "Root";
            EditorGUILayout.LabelField($"{parent} | {go.name}", EditorStyles.miniLabel);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(6);

        // Shared material selector
        EditorGUILayout.LabelField("Shared Material:", EditorStyles.boldLabel);
        if (sharedMaterials.Count == 0)
        {
            EditorGUILayout.HelpBox("No materials are shared across all selected meshes.", MessageType.Warning);
        }
        else
        {
            string[] options = sharedMaterials
                .Select(m => m != null ? m.name : "(None)")
                .ToArray();
            selectedSharedIndex = Mathf.Clamp(selectedSharedIndex, 0, sharedMaterials.Count - 1);
            selectedSharedIndex = EditorGUILayout.Popup("Material:", selectedSharedIndex, options);

            // Show which slot the chosen material occupies on each mesh
            Material chosen = sharedMaterials[selectedSharedIndex];
            EditorGUILayout.LabelField("Will affect:", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (var go in objects)
            {
                var smr = go.GetComponent<SkinnedMeshRenderer>();
                var mats = smr.sharedMaterials;
                var slots = new List<int>();
                for (int s = 0; s < mats.Length; s++)
                    if (mats[s] == chosen) slots.Add(s);

                string slotText = slots.Count == 0
                    ? "no slot (?)"
                    : "slot " + string.Join(", ", slots.Select(s => s.ToString()));
                EditorGUILayout.LabelField($"  {go.name}: {slotText}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(6);

        // Extras list (drag-and-drop + buttons)
        EditorGUILayout.LabelField("Extra Materials (Swap Targets):", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        mainScrollPos = EditorGUILayout.BeginScrollView(mainScrollPos, GUILayout.MinHeight(80));
        for (int e = 0; e < extraMaterials.Count; e++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"[{e + 1}]", GUILayout.Width(34));
            extraMaterials[e] = (Material)EditorGUILayout.ObjectField(extraMaterials[e], typeof(Material), false);
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                extraMaterials.RemoveAt(e);
                e--;
            }
            EditorGUILayout.EndHorizontal();
        }
        if (extraMaterials.Count == 0)
        {
            EditorGUILayout.LabelField("Drop materials here, or use + Add Material below.", EditorStyles.miniLabel);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+ Add Material", GUILayout.Width(130)))
            extraMaterials.Add(null);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        // Drag-and-drop onto the extras box
        Rect extrasRect = GUILayoutUtility.GetLastRect();
        if (AnimationGeneratorTool.HandleMaterialDragAndDrop(extrasRect, extraMaterials))
            Repaint();

        EditorGUILayout.Space(6);

        // Preview
        if (!string.IsNullOrEmpty(categoryName) && !string.IsNullOrEmpty(assetName))
        {
            EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Param: Materials/{categoryName}/{assetName}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Materials.{categoryName}.{assetName}.0  (default)", EditorStyles.miniLabel);
            for (int e = 0; e < extraMaterials.Count; e++)
                EditorGUILayout.LabelField($"Materials.{categoryName}.{assetName}.{e + 1}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        GUILayout.FlexibleSpace();

        // Buttons
        bool canCreate = directBlendTrees.Count > 0
                         && floatParamNames.Length > 0
                         && sharedMaterials.Count > 0;
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = canCreate;
        if (GUILayout.Button("Create", GUILayout.Height(30)))
        {
            if (ValidateInputs())
            {
                AnimationGeneratorTool.ExecuteCreateDBTGroupMaterialSwap(
                    objects,
                    categoryName,
                    assetName,
                    sharedMaterials[selectedSharedIndex],
                    extraMaterials,
                    detectedController,
                    directBlendTrees[selectedTreeIndex],
                    floatParamNames[selectedParamIndex]);
                Close();
            }
        }
        GUI.enabled = true;
        if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            Close();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrEmpty(categoryName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a category name.", "OK");
            return false;
        }
        if (string.IsNullOrEmpty(assetName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter an asset name.", "OK");
            return false;
        }

        for (int e = 0; e < extraMaterials.Count; e++)
        {
            if (extraMaterials[e] == null)
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    $"Extra material slot [{e + 1}] is empty. Assign or remove it.",
                    "OK");
                return false;
            }
        }

        if (extraMaterials.Count == 0)
        {
            return EditorUtility.DisplayDialog(
                "No Extra Materials",
                "No extra materials added. Only the default (.0) clip will be created. Continue?",
                "Continue",
                "Cancel");
        }
        return true;
    }
}

public class CreateDBTDecalRevealWindow : EditorWindow
{
    private GameObject[] objects;
    private string[] assetNames;
    private int[] materialSlots;          // which material slot to read animated decals from
    private List<string>[] decalProps;   // detected animated decal base props per object (reveal order)
    private bool[] foldoutStates;

    private Transform animatorRoot;
    private AnimatorController detectedController;

    private List<BlendTree> directBlendTrees = new List<BlendTree>();
    private List<string> directBlendTreeLabels = new List<string>();
    private int selectedTreeIndex = 0;

    private string[] floatParamNames = new string[0];
    private int selectedParamIndex = 0;

    private Vector2 mainScrollPos;

    public static void Show(GameObject[] selectedObjects)
    {
        var window = CreateInstance<CreateDBTDecalRevealWindow>();
        window.Initialize(selectedObjects);
        window.titleContent = new GUIContent("Create DBT Decal Reveal");
        window.minSize = new Vector2(560, 600);
        window.ShowUtility();
    }

    private void Initialize(GameObject[] selectedObjects)
    {
        // Keep only objects that actually have a SkinnedMeshRenderer
        var valid = selectedObjects.Where(o => o != null && o.GetComponent<SkinnedMeshRenderer>() != null).ToArray();
        objects = valid;
        assetNames = new string[valid.Length];
        materialSlots = new int[valid.Length];
        decalProps = new List<string>[valid.Length];
        foldoutStates = new bool[valid.Length];

        for (int i = 0; i < valid.Length; i++)
        {
            assetNames[i] = AnimationGeneratorTool.CleanDisplayName(valid[i].name);
            materialSlots[i] = AnimationGeneratorTool.AutoDetectDecalSlot(valid[i]);
            decalProps[i] = AnimationGeneratorTool.GetMeshAnimatedDecalBaseProps(valid[i], materialSlots[i]);
            foldoutStates[i] = true;
        }

        DetectAnimator(selectedObjects);
    }

    private void DetectAnimator(GameObject[] sourceObjects)
    {
        if (sourceObjects == null || sourceObjects.Length == 0) return;

        Transform current = sourceObjects[0].transform;
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
        EditorGUILayout.LabelField("Create DBT Decal Reveal", EditorStyles.boldLabel);
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

        if (objects == null || objects.Length == 0)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox("None of the selected objects have a SkinnedMeshRenderer.", MessageType.Error);
            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
                Close();
            return;
        }

        EditorGUILayout.Space(4);

        // Tree + parameter selectors
        if (directBlendTrees.Count == 0)
            EditorGUILayout.HelpBox("No Direct Blend Trees found in the controller.", MessageType.Warning);
        else
            selectedTreeIndex = EditorGUILayout.Popup("Direct Blend Tree:", selectedTreeIndex, directBlendTreeLabels.ToArray());

        if (floatParamNames.Length == 0)
            EditorGUILayout.HelpBox("No Float parameters found. Add a Float parameter to the controller first.", MessageType.Warning);
        else
            selectedParamIndex = EditorGUILayout.Popup("DBT Parameter:", selectedParamIndex, floatParamNames);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Decal Reveals (Poiyomi properties marked animated):", EditorStyles.boldLabel);

        mainScrollPos = EditorGUILayout.BeginScrollView(mainScrollPos);

        for (int i = 0; i < objects.Length; i++)
        {
            string parentName = objects[i].transform.parent != null ? objects[i].transform.parent.name : "Root";
            var smr = objects[i].GetComponent<SkinnedMeshRenderer>();
            var mats = smr.sharedMaterials;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            foldoutStates[i] = EditorGUILayout.Foldout(
                foldoutStates[i],
                $"{parentName} | {objects[i].name}",
                true,
                EditorStyles.foldoutHeader);

            if (foldoutStates[i])
            {
                EditorGUI.indentLevel++;

                // Material slot picker (which slot's material to read animated decals from)
                if (mats.Length > 1)
                {
                    string[] slotOptions = new string[mats.Length];
                    for (int s = 0; s < mats.Length; s++)
                        slotOptions[s] = $"[{s}] {(mats[s] != null ? mats[s].name : "(None)")}";
                    int newSlot = EditorGUILayout.Popup("Material Slot:", materialSlots[i], slotOptions);
                    if (newSlot != materialSlots[i])
                    {
                        materialSlots[i] = newSlot;
                        decalProps[i] = AnimationGeneratorTool.GetMeshAnimatedDecalBaseProps(objects[i], newSlot);
                    }
                }

                var props = decalProps[i];
                if (props.Count == 0)
                {
                    EditorGUILayout.HelpBox("No decal properties are marked animated on the selected material slot. It will be skipped.", MessageType.Warning);
                }
                else
                {
                    assetNames[i] = EditorGUILayout.TextField("Asset Name:", assetNames[i]);

                    var slotMat = AnimationGeneratorTool.GetSlotMaterial(objects[i], materialSlots[i]);
                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField($"Animated decal props ({props.Count}, reveal order):", EditorStyles.miniBoldLabel);
                    for (int p = 0; p < props.Count; p++)
                    {
                        bool ra = AnimationGeneratorTool.IsRenamedAnimated(slotMat, props[p]);
                        EditorGUILayout.LabelField($"  {p + 1}. {props[p]}{(ra ? "   (RA / renamed when locked)" : "")}", EditorStyles.miniLabel);
                    }

                    if (!string.IsNullOrEmpty(assetNames[i]))
                    {
                        EditorGUILayout.Space(2);
                        EditorGUILayout.LabelField("Preview:", EditorStyles.miniBoldLabel);
                        EditorGUILayout.LabelField($"  Param: Decals/{parentName}/{assetNames[i]}", EditorStyles.miniLabel);
                        EditorGUILayout.LabelField($"  {props.Count + 1} clips: Decals.{parentName}.{assetNames[i]}.0 .. .{props.Count}", EditorStyles.miniLabel);
                        EditorGUILayout.LabelField("  (cumulative: each step turns on one more decal property)", EditorStyles.miniLabel);
                    }
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        EditorGUILayout.EndScrollView();

        GUILayout.FlexibleSpace();

        // Bottom buttons
        bool anyWithProps = decalProps.Any(p => p != null && p.Count > 0);
        bool canCreate = directBlendTrees.Count > 0 && floatParamNames.Length > 0 && anyWithProps;
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = canCreate;
        if (GUILayout.Button("Create", GUILayout.Height(30)))
        {
            if (ValidateInputs())
            {
                AnimationGeneratorTool.ExecuteCreateDBTDecalReveal(
                    objects,
                    assetNames,
                    materialSlots,
                    detectedController,
                    directBlendTrees[selectedTreeIndex],
                    floatParamNames[selectedParamIndex]);
                Close();
            }
        }
        GUI.enabled = true;
        if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            Close();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
    }

    private bool ValidateInputs()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (decalProps[i].Count == 0) continue;  // skipped meshes don't need a name
            if (string.IsNullOrEmpty(assetNames[i]))
            {
                EditorUtility.DisplayDialog("Error", $"Please enter an asset name for {objects[i].name}.", "OK");
                return false;
            }
        }
        return true;
    }
}

public class CreateDBTGroupDecalRevealWindow : EditorWindow
{
    private GameObject[] objects;
    private string categoryName = "";
    private string assetName = "";

    private List<Material> sharedMaterials = new List<Material>();
    private int selectedSharedIndex = 0;
    private List<string> decalProps = new List<string>();   // base props of the chosen shared material

    private Transform animatorRoot;
    private AnimatorController detectedController;

    private List<BlendTree> directBlendTrees = new List<BlendTree>();
    private List<string> directBlendTreeLabels = new List<string>();
    private int selectedTreeIndex = 0;

    private string[] floatParamNames = new string[0];
    private int selectedParamIndex = 0;

    private Vector2 mainScrollPos;

    public static void Show(GameObject[] selectedObjects)
    {
        var window = CreateInstance<CreateDBTGroupDecalRevealWindow>();
        window.Initialize(selectedObjects);
        window.titleContent = new GUIContent("Create DBT Group Decal Reveal");
        window.minSize = new Vector2(560, 600);
        window.ShowUtility();
    }

    private void Initialize(GameObject[] selectedObjects)
    {
        var valid = selectedObjects.Where(o => o != null && o.GetComponent<SkinnedMeshRenderer>() != null).ToArray();
        objects = valid;

        // Default category = common parent name if all selected share one
        if (valid.Length > 0)
        {
            string firstParent = valid[0].transform.parent != null ? valid[0].transform.parent.name : "";
            bool allSame = true;
            for (int i = 1; i < valid.Length; i++)
            {
                string p = valid[i].transform.parent != null ? valid[i].transform.parent.name : "";
                if (p != firstParent) { allSame = false; break; }
            }
            if (allSame && !string.IsNullOrEmpty(firstParent))
                categoryName = firstParent;
        }

        sharedMaterials = AnimationGeneratorTool.FindSharedMaterials(valid);
        RefreshDecalProps();

        DetectAnimator(selectedObjects);
    }

    private void RefreshDecalProps()
    {
        if (sharedMaterials.Count == 0) { decalProps = new List<string>(); return; }
        selectedSharedIndex = Mathf.Clamp(selectedSharedIndex, 0, sharedMaterials.Count - 1);
        decalProps = AnimationGeneratorTool.FindAnimatedDecalBaseProps(sharedMaterials[selectedSharedIndex]);
    }

    private void DetectAnimator(GameObject[] sourceObjects)
    {
        if (sourceObjects == null || sourceObjects.Length == 0) return;

        Transform current = sourceObjects[0].transform;
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
        EditorGUILayout.LabelField("Create DBT Group Decal Reveal", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        if (detectedController != null)
            EditorGUILayout.LabelField("Controller: " + detectedController.name);
        else
            EditorGUILayout.HelpBox("No AnimatorController found. Selected objects must be under an Animator.", MessageType.Error);
        EditorGUILayout.EndVertical();

        if (detectedController == null)
        {
            EditorGUILayout.Space(4);
            if (GUILayout.Button("Cancel", GUILayout.Height(30))) Close();
            return;
        }

        if (objects == null || objects.Length == 0)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox("None of the selected objects have a SkinnedMeshRenderer.", MessageType.Error);
            if (GUILayout.Button("Cancel", GUILayout.Height(30))) Close();
            return;
        }

        EditorGUILayout.Space(4);

        if (directBlendTrees.Count == 0)
            EditorGUILayout.HelpBox("No Direct Blend Trees found in the controller.", MessageType.Warning);
        else
            selectedTreeIndex = EditorGUILayout.Popup("Direct Blend Tree:", selectedTreeIndex, directBlendTreeLabels.ToArray());

        if (floatParamNames.Length == 0)
            EditorGUILayout.HelpBox("No Float parameters found. Add a Float parameter to the controller first.", MessageType.Warning);
        else
            selectedParamIndex = EditorGUILayout.Popup("DBT Parameter:", selectedParamIndex, floatParamNames);

        EditorGUILayout.Space(6);

        // Naming
        EditorGUILayout.LabelField("Naming:", EditorStyles.boldLabel);
        categoryName = EditorGUILayout.TextField("Category:", categoryName);
        assetName    = EditorGUILayout.TextField("Name:",     assetName);

        EditorGUILayout.Space(6);

        // Targets
        EditorGUILayout.LabelField($"Targets ({objects.Length}):", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        foreach (var go in objects)
        {
            string parent = go.transform.parent != null ? go.transform.parent.name : "Root";
            EditorGUILayout.LabelField($"{parent} | {go.name}", EditorStyles.miniLabel);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(6);

        // Shared material selector
        EditorGUILayout.LabelField("Shared Material (read decals from):", EditorStyles.boldLabel);
        if (sharedMaterials.Count == 0)
        {
            EditorGUILayout.HelpBox("No materials are shared across all selected meshes.", MessageType.Warning);
        }
        else
        {
            string[] options = sharedMaterials.Select(m => m != null ? m.name : "(None)").ToArray();
            int newIndex = EditorGUILayout.Popup("Material:", selectedSharedIndex, options);
            if (newIndex != selectedSharedIndex)
            {
                selectedSharedIndex = newIndex;
                RefreshDecalProps();
            }
        }

        EditorGUILayout.Space(6);

        // Detected animated decal props on the chosen material
        mainScrollPos = EditorGUILayout.BeginScrollView(mainScrollPos);
        if (sharedMaterials.Count > 0)
        {
            var mat = sharedMaterials[selectedSharedIndex];
            if (decalProps.Count == 0)
            {
                EditorGUILayout.HelpBox("This material has no decal properties marked animated.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField($"Animated decal props ({decalProps.Count}, reveal order):", EditorStyles.miniBoldLabel);
                for (int p = 0; p < decalProps.Count; p++)
                {
                    bool ra = AnimationGeneratorTool.IsRenamedAnimated(mat, decalProps[p]);
                    EditorGUILayout.LabelField($"  {p + 1}. {decalProps[p]}{(ra ? "   (RA / renamed when locked)" : "")}", EditorStyles.miniLabel);
                }

                if (!string.IsNullOrEmpty(categoryName) && !string.IsNullOrEmpty(assetName))
                {
                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField("Preview:", EditorStyles.miniBoldLabel);
                    EditorGUILayout.LabelField($"  Param: Decals/{categoryName}/{assetName}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"  {decalProps.Count + 1} clips driving every mesh that uses this material together.", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField("  (cumulative: each step turns on one more decal property)", EditorStyles.miniLabel);
                }
            }
        }
        EditorGUILayout.EndScrollView();

        GUILayout.FlexibleSpace();

        bool canCreate = directBlendTrees.Count > 0 && floatParamNames.Length > 0
                         && sharedMaterials.Count > 0 && decalProps.Count > 0;
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = canCreate;
        if (GUILayout.Button("Create", GUILayout.Height(30)))
        {
            if (ValidateInputs())
            {
                AnimationGeneratorTool.ExecuteCreateDBTGroupDecalReveal(
                    objects,
                    categoryName,
                    assetName,
                    sharedMaterials[selectedSharedIndex],
                    detectedController,
                    directBlendTrees[selectedTreeIndex],
                    floatParamNames[selectedParamIndex]);
                Close();
            }
        }
        GUI.enabled = true;
        if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            Close();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrEmpty(categoryName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a category name.", "OK");
            return false;
        }
        if (string.IsNullOrEmpty(assetName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter an asset name.", "OK");
            return false;
        }
        if (decalProps.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "The selected material has no decal properties marked animated.", "OK");
            return false;
        }
        return true;
    }
}