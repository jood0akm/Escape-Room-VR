using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

// Builds the "YOU ESCAPED THIS ROOM" screen and wires it to doorbg. Tools > Escape Room > Build Escape Screen.
public static class BuildEscapeScreen
{
    static readonly Color Cyan = new Color(0f, 0.898f, 1f, 1f);
    static readonly Color Dim = new Color(1, 1, 1, 0.4f);

    [MenuItem("Tools/Escape Room/Build Escape Screen")]
    public static void Build()
    {
        var old = GameObject.Find("EscapeScreen");
        if (old != null) Object.DestroyImmediate(old);
        var door = GameObject.Find("doorbg");
        var circuit = Object.FindAnyObjectByType<CircuitManager>();
        if (door == null) { Debug.LogError("doorbg not found"); return; }

        var bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/MenuBackground.jpg");

        var root = new GameObject("EscapeScreen");
        var canvasGo = new GameObject("Canvas", typeof(RectTransform));
        canvasGo.transform.SetParent(root.transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvasGo.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
        canvasGo.AddComponent<GraphicRaycaster>();
        canvasGo.AddComponent<TrackedDeviceGraphicRaycaster>();
        var crt = canvasGo.GetComponent<RectTransform>();
        crt.sizeDelta = new Vector2(1920, 1080);
        // 1.1mm/pixel at 1m = ~2.1m x 1.2m panel (~90 deg wide): fills the view, text stays fully readable
        crt.localScale = Vector3.one * 0.0011f;

        // opaque backdrop far larger than the view so the room is fully covered behind the screen
        var back = BuildMainMenu.Rect("Backdrop", canvasGo.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(9000, 6000));
        back.pivot = new Vector2(0.5f, 0.5f);
        var backImg = back.gameObject.AddComponent<Image>();
        backImg.color = new Color(0.02f, 0.03f, 0.035f, 1f); backImg.raycastTarget = false;

        var content = BuildMainMenu.Stretch("Content", canvasGo.transform);
        content.gameObject.AddComponent<RectMask2D>();

        // background (lower part of the poster: the lit workbench) + dark overlay
        var bgRt = BuildMainMenu.Rect("Background", content, new Vector2(0.5f, 1), new Vector2(0, 630), new Vector2(1920, 2880));
        bgRt.pivot = new Vector2(0.5f, 1);
        var bgImg = bgRt.gameObject.AddComponent<Image>(); bgImg.sprite = bg; bgImg.raycastTarget = false;
        var dim = BuildMainMenu.Stretch("Dim", content).gameObject.AddComponent<Image>();
        dim.color = new Color(0, 0.03f, 0.04f, 0.72f); dim.raycastTarget = false;

        BuildMainMenu.Text("SystemStatus", content, "SYSTEM: CIRCUIT COMPLETE", 26, Cyan, TextAlignmentOptions.TopLeft, new Vector2(0, 1), new Vector2(60, -40), new Vector2(800, 40));
        BuildMainMenu.Text("Version", content, "VER 0.9.4_BETA", 26, new Color(1, 1, 1, 0.5f), TextAlignmentOptions.TopRight, new Vector2(1, 1), new Vector2(-60, -40), new Vector2(700, 40));

        var t1 = BuildMainMenu.Text("Title", content, "YOU ESCAPED", 140, new Color(0.91f, 0.93f, 0.95f, 1f), TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(1700, 180));
        t1.fontStyle = FontStyles.Bold; t1.characterSpacing = 16;
        var t2 = BuildMainMenu.Text("Title2", content, "THIS ROOM", 90, Cyan, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0, -320), new Vector2(1700, 120));
        t2.fontStyle = FontStyles.Bold; t2.characterSpacing = 28;
        BuildMainMenu.Text("Tagline", content, "CIRCUIT COMPLETE // DOOR UNLOCKED", 30, Dim, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0, -455), new Vector2(1200, 44)).characterSpacing = 12;

        var esc = door.GetComponent<EscapeScreen>();
        if (esc == null) esc = door.AddComponent<EscapeScreen>();

        var replay = BuildMainMenu.MakeButton(content, "Btn_REPLAY", "REPLAY", "RESTART SEQUENCE", "01", true, new Vector2(0, -560), new Vector2(760, 120));
        var exit = BuildMainMenu.MakeButton(content, "Btn_EXIT", "EXIT", "TERMINATE", "02", false, new Vector2(0, -702), new Vector2(760, 96));
        BuildMainMenu.UnityEventTools(replay, esc.Replay);
        BuildMainMenu.UnityEventTools(exit, esc.ExitGame);

        BuildMainMenu.Text("Footer", content, "COMPLETE THE CIRCUIT. OPEN THE DOOR.", 28, new Color(1, 1, 1, 0.6f), TextAlignmentOptions.Bottom, new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(1200, 44)).characterSpacing = 8;

        // wire doorbg
        var si = door.GetComponent<XRSimpleInteractable>();
        if (si == null) si = door.AddComponent<XRSimpleInteractable>();
        esc.circuit = circuit;
        esc.distance = 1.0f;
        esc.screenRoot = root;
        EditorUtility.SetDirty(esc);

        root.SetActive(false);
        Undo.RegisterCreatedObjectUndo(root, "Build Escape Screen");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Escape screen built and wired to doorbg");
    }
}
