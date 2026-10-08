using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

// Builds the "WRONG ARRANGEMENT / TRY AGAIN" banner and wires it (+ the buzzer sound) to the circuit.
// Tools > Escape Room > Build Try Again.
public static class BuildTryAgain
{
    static readonly Color Red = new Color(1f, 0.32f, 0.27f, 1f);

    [MenuItem("Tools/Escape Room/Build Try Again")]
    public static void Build()
    {
        // GameObject.Find skips inactive objects, so search everything
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            if (t != null && t.parent == null && t.name == "TryAgainToast") Object.DestroyImmediate(t.gameObject);

        var cm = Object.FindAnyObjectByType<CircuitManager>();
        var ga = Object.FindAnyObjectByType<GameAudio>();

        var root = new GameObject("TryAgainToast");
        var toast = root.AddComponent<TryAgainToast>();

        var canvasGo = new GameObject("Canvas", typeof(RectTransform));
        canvasGo.transform.SetParent(root.transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = Camera.main;
        canvasGo.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
        canvasGo.AddComponent<GraphicRaycaster>();
        canvasGo.AddComponent<TrackedDeviceGraphicRaycaster>();
        var crt = canvasGo.GetComponent<RectTransform>();
        crt.sizeDelta = new Vector2(1100, 340);
        crt.localScale = Vector3.one * 0.001f;

        var panel = BuildMainMenu.Stretch("Panel", canvasGo.transform);
        var img = panel.gameObject.AddComponent<Image>(); img.color = new Color(0.10f, 0.02f, 0.02f, 0.94f); img.raycastTarget = false;
        var ol = panel.gameObject.AddComponent<Outline>(); ol.effectColor = new Color(1f, 0.32f, 0.27f, 0.85f); ol.effectDistance = new Vector2(3, -3);

        var accent = BuildMainMenu.Rect("Accent", panel, new Vector2(0, 0.5f), Vector2.zero, new Vector2(10, 340));
        accent.pivot = new Vector2(0, 0.5f);
        var aImg = accent.gameObject.AddComponent<Image>(); aImg.color = Red; aImg.raycastTarget = false;

        BuildMainMenu.Text("Title", panel, "WRONG ARRANGEMENT", 46, Red, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0, -34), new Vector2(1000, 64)).characterSpacing = 12;
        var big = BuildMainMenu.Text("TryAgain", panel, "TRY AGAIN", 110, Color.white, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0, -8), new Vector2(1000, 140));
        big.fontStyle = FontStyles.Bold; big.characterSpacing = 20;
        BuildMainMenu.Text("Sub", panel, "THE RESISTOR GOES IN THE MIDDLE", 28, new Color(1, 1, 1, 0.55f), TextAlignmentOptions.Center, new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(1000, 40)).characterSpacing = 8;

        toast.root = root;
        // the toast script lives on the same object as its canvas root; the whole object is toggled
        root.SetActive(false);

        if (cm != null)
        {
            Undo.RecordObject(cm, "Wire try again");
            cm.tryAgainToast = toast;
            EditorUtility.SetDirty(cm);
        }
        if (ga != null)
        {
            Undo.RecordObject(ga, "Wire wrong sound");
            ga.wrongClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sound/generated/wrong_buzzer.wav");
            EditorUtility.SetDirty(ga);
        }

        Undo.RegisterCreatedObjectUndo(root, "TryAgainToast");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Try again toast built");
    }
}
