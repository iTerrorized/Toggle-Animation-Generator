using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;

public class AddBlendshapesToTogglesTool
{
    [MenuItem("GameObject/Terrorized/Add Blendshapes to DBT Toggles", false, 16)]
    private static void AddBlendshapesToDBTToggles(MenuCommand menuCommand)
    {
        if (menuCommand.context != Selection.activeObject) return;
        GameObject[] selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0) return;
        AddBlendshapesToTogglesWindow.Show(selected);
    }

    // Searches ALL layers, sub-state machines, and nested blend trees for the
    // 1D tree that was created for this parentName/displayName pair, then returns
    // its Off (threshold=0) and On (threshold=1) animation clips.
    public static (AnimationClip offClip, AnimationClip onClip) FindClipsFromController(
        AnimatorController controller, string parentName, string displayName)
    {
        foreach (var layer in controller.layers)
        {
            var result = SearchStateMachine(layer.stateMachine, parentName, displayName);
            if (result.offClip != null || result.onClip != null) return result;
        }
        return (null, null);
    }

    private static (AnimationClip offClip, AnimationClip onClip) SearchStateMachine(
        AnimatorStateMachine sm, string parentName, string displayName)
    {
        foreach (var stateInfo in sm.states)
        {
            if (stateInfo.state.motion is BlendTree bt)
            {
                var r = SearchInBlendTree(bt, parentName, displayName);
                if (r.offClip != null || r.onClip != null) return r;
            }
        }
        foreach (var sub in sm.stateMachines)
        {
            var r = SearchStateMachine(sub.stateMachine, parentName, displayName);
            if (r.offClip != null || r.onClip != null) return r;
        }
        return (null, null);
    }

    private static (AnimationClip offClip, AnimationClip onClip) SearchInBlendTree(
        BlendTree tree, string parentName, string displayName)
    {
        foreach (var child in tree.children)
        {
            if (!(child.motion is BlendTree childTree)) continue;

            // Match group Direct tree named after the empty parent
            if (childTree.blendType == BlendTreeType.Direct && childTree.name == parentName)
            {
                foreach (var groupChild in childTree.children)
                {
                    if (!(groupChild.motion is BlendTree oneDTree)) continue;
                    if (oneDTree.blendType != BlendTreeType.Simple1D) continue;
                    if (oneDTree.name != displayName) continue;

                    AnimationClip offClip = null, onClip = null;
                    foreach (var m in oneDTree.children)
                    {
                        if (m.motion is AnimationClip clip)
                        {
                            if (m.threshold < 0.5f) offClip = clip;
                            else                    onClip  = clip;
                        }
                    }
                    return (offClip, onClip);
                }
            }

            // Recurse deeper
            var result = SearchInBlendTree(childTree, parentName, displayName);
            if (result.offClip != null || result.onClip != null) return result;
        }
        return (null, null);
    }

    public static void Execute(
        GameObject[] objects,
        string[] displayNames,
        AnimatorController controller,
        Dictionary<string, List<(string smrPath, string shapeName)>> selectedBlendshapes)
    {
        int updated = 0;
        var notFound = new List<string>();

        for (int i = 0; i < objects.Length; i++)
        {
            string parentName = objects[i].transform.parent != null ? objects[i].transform.parent.name : "Root";
            string displayName = displayNames[i];
            string bsKey = objects[i].GetInstanceID().ToString();

            selectedBlendshapes.TryGetValue(bsKey, out var bsList);
            if (bsList == null || bsList.Count == 0) continue;

            var (offClip, onClip) = FindClipsFromController(controller, parentName, displayName);

            if (offClip == null || onClip == null)
            {
                notFound.Add(displayName);
                continue;
            }

            Undo.RecordObject(offClip, "Add Blendshapes to Toggle");
            Undo.RecordObject(onClip,  "Add Blendshapes to Toggle");

            AddCurvesToClip(offClip, bsList, false);
            AddCurvesToClip(onClip,  bsList, true);

            EditorUtility.SetDirty(offClip);
            EditorUtility.SetDirty(onClip);
            updated++;
        }

        AssetDatabase.SaveAssets();

        string msg = $"Added blendshapes to {updated} toggle pair(s).";
        if (notFound.Count > 0)
            msg += $"\n\nNo blend tree entry found for:\n• {string.Join("\n• ", notFound)}\n\nCheck that these objects had DBT toggles created for them.";

        EditorUtility.DisplayDialog(updated > 0 ? "Done" : "Nothing Updated", msg, "OK");
    }

    private static void AddCurvesToClip(AnimationClip clip, List<(string smrPath, string shapeName)> blendshapes, bool isOn)
    {
        foreach (var (smrPath, shapeName) in blendshapes)
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(new Keyframe(0f, isOn ? 100f : 0f));
            AnimationUtility.SetKeyLeftTangentMode(curve, 0, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(curve, 0, AnimationUtility.TangentMode.Constant);

            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(smrPath, typeof(SkinnedMeshRenderer), "blendShape." + shapeName);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }
    }
}

public class AddBlendshapesToTogglesWindow : EditorWindow
{
    private GameObject[] objects;
    private string[] displayNames;
    private Transform animatorRoot;
    private AnimatorController detectedController;

    private Dictionary<string, List<(string smrPath, string shapeName, bool enabled)>> blendshapeResults
        = new Dictionary<string, List<(string, string, bool)>>();
    private bool blendshapesSearched = false;

    private Vector2 objectsScrollPos;
    private Vector2 blendshapesScrollPos;

    public static void Show(GameObject[] selectedObjects)
    {
        var window = CreateInstance<AddBlendshapesToTogglesWindow>();
        window.Initialize(selectedObjects);
        window.titleContent = new GUIContent("Add Blendshapes to Toggles");
        window.minSize = new Vector2(520, 480);
        window.ShowModal();
    }

    private void Initialize(GameObject[] selectedObjects)
    {
        objects = selectedObjects;
        displayNames = new string[selectedObjects.Length];
        for (int i = 0; i < selectedObjects.Length; i++)
            displayNames[i] = AnimationGeneratorTool.CleanDisplayName(selectedObjects[i].name);

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
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Add Blendshapes to DBT Toggles", EditorStyles.boldLabel);
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

        EditorGUILayout.Space(4);

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

            // Live check: find clips via blend tree traversal
            var (offClip, onClip) = AddBlendshapesToTogglesTool.FindClipsFromController(
                detectedController, parentName, displayNames[i]);
            bool found = offClip != null && onClip != null;

            Color prev = GUI.contentColor;
            GUI.contentColor = found ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f);
            EditorGUILayout.LabelField(found ? "✓" : "✗", GUILayout.Width(18));
            GUI.contentColor = prev;

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(6);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Blendshapes to Add (CLIPPING/ prefix):", EditorStyles.boldLabel);
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
                if (!blendshapeResults.TryGetValue(key, out var bsList) || bsList.Count == 0) continue;
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
            if (!anyFound) EditorGUILayout.LabelField("No matching blendshapes found.", EditorStyles.miniLabel);
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.LabelField("Click Search to find CLIPPING/ blendshapes.", EditorStyles.miniLabel);
        }

        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Blendshapes", GUILayout.Height(30)))
        {
            if (ValidateInputs())
            {
                AddBlendshapesToTogglesTool.Execute(
                    objects, displayNames, detectedController, BuildSelectedBlendshapes());
                Close();
            }
        }
        if (GUILayout.Button("Cancel", GUILayout.Height(30))) Close();
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
        if (!blendshapesSearched || !blendshapeResults.Values.Any(l => l.Any(e => e.enabled)))
        {
            EditorUtility.DisplayDialog("Nothing Selected",
                "Search for blendshapes first, then check at least one to add.", "OK");
            return false;
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
