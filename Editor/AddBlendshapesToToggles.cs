using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class AddBlendshapesToTogglesTool
{
    private const string ASSET_FOLDER = "Assets/!Terrorized/GeneratedAssets";

    [MenuItem("GameObject/Terrorized/Add Blendshapes to DBT Toggles", false, 16)]
    private static void AddBlendshapesToDBTToggles(MenuCommand menuCommand)
    {
        if (menuCommand.context != Selection.activeObject) return;
        GameObject[] selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0) return;
        AddBlendshapesToTogglesWindow.Show(selected);
    }

    public static AnimationClip FindClip(string parentName, string displayName, bool isOn)
    {
        string suffix = isOn ? ".On" : ".Off";
        string path = $"{ASSET_FOLDER}/Toggles.{parentName}.{displayName}{suffix}.anim";
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
    }

    public static void Execute(
        GameObject[] objects,
        string[] displayNames,
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

            AnimationClip offClip = FindClip(parentName, displayName, false);
            AnimationClip onClip  = FindClip(parentName, displayName, true);

            if (offClip == null || onClip == null)
            {
                notFound.Add($"{parentName}/{displayName}");
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

        if (notFound.Count > 0)
            Debug.LogWarning($"[Add Blendshapes] Could not find clips for: {string.Join(", ", notFound)}");

        if (updated > 0)
            EditorUtility.DisplayDialog("Success", $"Added blendshapes to {updated} toggle pair(s).", "OK");
        else
            EditorUtility.DisplayDialog("Nothing Updated",
                "No clips were updated.\nCheck that display names match the original toggle names,\nand that blendshapes are selected.", "OK");
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
            if (current.GetComponent<Animator>() != null)
            {
                animatorRoot = current;
                break;
            }
            current = current.parent;
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Add Blendshapes to DBT Toggles", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // Object list
        EditorGUILayout.LabelField("Object Names (must match existing toggle names):", EditorStyles.boldLabel);
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

            // Live clip found indicator
            bool offFound = AddBlendshapesToTogglesTool.FindClip(parentName, displayNames[i], false) != null;
            bool onFound  = AddBlendshapesToTogglesTool.FindClip(parentName, displayNames[i], true)  != null;
            bool bothFound = offFound && onFound;

            Color prev = GUI.contentColor;
            GUI.contentColor = bothFound ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f);
            EditorGUILayout.LabelField(bothFound ? "✓" : "✗", GUILayout.Width(18));
            GUI.contentColor = prev;

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(6);

        // Blendshapes section
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

        // Buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Blendshapes", GUILayout.Height(30)))
        {
            if (ValidateInputs())
            {
                AddBlendshapesToTogglesTool.Execute(objects, displayNames, BuildSelectedBlendshapes());
                Close();
            }
        }
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
        if (!blendshapesSearched || !blendshapeResults.Values.Any(l => l.Any(e => e.enabled)))
        {
            EditorUtility.DisplayDialog("Nothing Selected",
                "Search for blendshapes first, then check at least one to add.", "OK");
            return false;
        }
        for (int i = 0; i < displayNames.Length; i++)
        {
            if (string.IsNullOrEmpty(displayNames[i]))
            {
                EditorUtility.DisplayDialog("Error", $"Display name for object {i} ({objects[i].name}) is empty.", "OK");
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
