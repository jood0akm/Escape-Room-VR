using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

// Builds the room audio (clock + ambience + sfx) and the HINT button/popup. Tools > Escape Room > Build Hint And Audio.
public static class BuildHintAndAudio
{
    static readonly Color Cyan = new Color(0f, 0.898f, 1f, 1f);

    static AudioClip Clip(string name)
    {
        var path = "Assets/sound/generated/" + name;
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    static Canvas MakeCanvas(string name, Transform parent, Vector2 size, float scale)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        go.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
        go.AddComponent<GraphicRaycaster>();
        go.AddComponent<TrackedDeviceGraphicRaycaster>();
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.localScale = Vector3.one * scale;
        return canvas;
    }

    [MenuItem("Tools/Escape Room/Build Hint And Audio")]
    public static void Build()
    {
        foreach (var n in new[] { "GameAudio", "HintSystem" })
        {
            var old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }

        // ---- sprite import for hint image
        var hintPath = "Assets/UI/HintCircuit.jpg";
        var ti = (TextureImporter)AssetImporter.GetAtPath(hintPath);
        if (ti != null && ti.textureType != TextureImporterType.Sprite)
        {
            ti.textureType = TextureImporterType.Sprite; ti.maxTextureSize = 2048; ti.SaveAndReimport();
        }
        var hintSprite = AssetDatabase.LoadAssetAtPath<Sprite>(hintPath);

        // ---- audio
        var audioGo = new GameObject("GameAudio");
        var ga = audioGo.AddComponent<GameAudio>();
        ga.tick = AddSource(audioGo, "Tick", Clip("clock_tick.wav"), true, 0.35f);
        ga.ambient = AddSource(audioGo, "Ambient", Clip("scary_ambient.wav"), true, 0.45f);
        ga.sfx = AddSource(audioGo, "Sfx", null, false, 0.9f);
        ga.hintClip = Clip("hint_chime.wav");
        ga.successClip = Clip("escape_success.wav");
        Undo.RegisterCreatedObjectUndo(audioGo, "GameAudio");

        // ---- hint system root
        var root = new GameObject("HintSystem");
        var popup = root.AddComponent<HintPopup>();

        // HINT button floating over the workbench, facing the player's start position
        var btnCanvas = MakeCanvas("HintButtonCanvas", root.transform, new Vector2(420, 150), 0.0026f); // large: it sits ~5m from the start spot
        // Strategic spot: behind the workbench (back wall side), far right end, out of the way of the circuit sockets.
        // Room x[-2.45,2.46] z[-3.04,2.98]; table z up to 2.08, right end x=1.99; sockets are at x -1.27..0.13.
        var hintPos = new Vector3(1.55f, 1.3f, 2.6f);
        btnCanvas.transform.position = hintPos;
        var cam = Camera.main;
        var from = cam != null ? cam.transform.position : new Vector3(0.9f, 1.36f, -2.42f);
        var toward = hintPos - from; toward.y = 0f;   // canvas reads from the side it faces away from: face away from the player
        btnCanvas.transform.rotation = Quaternion.LookRotation(toward.normalized, Vector3.up);
        var hintBtn = BuildMainMenu.MakeSimpleButton(btnCanvas.transform, "HintButton", "HINT", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 130), new Color(0.02f, 0.12f, 0.14f, 0.92f), Color.white);
        var ol = hintBtn.gameObject.AddComponent<Outline>(); ol.effectColor = new Color(0f, 0.898f, 1f, 0.8f); ol.effectDistance = new Vector2(3, -3);
        BuildMainMenu.UnityEventTools(hintBtn, popup.Open);

        // popup with the circuit diagram
        var popRoot = new GameObject("HintPopupRoot");
        popRoot.transform.SetParent(root.transform, false);
        var pc = MakeCanvas("Canvas", popRoot.transform, new Vector2(1700, 1330), 0.001f);
        var panel = BuildMainMenu.Stretch("Panel", pc.transform);
        var pImg = panel.gameObject.AddComponent<Image>(); pImg.color = new Color(0.04f, 0.06f, 0.07f, 0.97f);
        var pOl = panel.gameObject.AddComponent<Outline>(); pOl.effectColor = new Color(0f, 0.898f, 1f, 0.5f); pOl.effectDistance = new Vector2(3, -3);

        BuildMainMenu.Text("Header", panel, "HINT // CORRECT CIRCUIT", 40, Cyan, TextAlignmentOptions.TopLeft, new Vector2(0, 1), new Vector2(60, -40), new Vector2(1000, 60)).characterSpacing = 10;

        var imgRt = BuildMainMenu.Rect("DiagramImage", panel, new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(1500, 986));
        imgRt.pivot = new Vector2(0.5f, 0.5f);
        var img = imgRt.gameObject.AddComponent<Image>(); img.sprite = hintSprite; img.color = Color.white; img.raycastTarget = false;

        var close = BuildMainMenu.MakeSimpleButton(panel, "CloseButton", "X", new Vector2(1, 1), new Vector2(-70, -60), new Vector2(80, 80), new Color(1, 1, 1, 0.1f), Color.white);
        BuildMainMenu.UnityEventTools(close, popup.Close);
        var gotIt = BuildMainMenu.MakeSimpleButton(panel, "GotItButton", "GOT IT", new Vector2(0.5f, 0), new Vector2(0, 75), new Vector2(420, 90), Color.white, Color.black);
        BuildMainMenu.UnityEventTools(gotIt, popup.Close);

        popup.popupRoot = popRoot;
        popRoot.SetActive(false);

        Undo.RegisterCreatedObjectUndo(root, "HintSystem");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Hint + audio built");
    }

    static AudioSource AddSource(GameObject parent, string name, AudioClip clip, bool loop, float vol)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var s = go.AddComponent<AudioSource>();
        s.clip = clip; s.loop = loop; s.volume = vol; s.playOnAwake = false; s.spatialBlend = 0f;
        return s;
    }
}
