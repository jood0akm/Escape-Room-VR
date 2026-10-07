using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

// Builds the world-space VR main menu in the open scene. Run via Tools > Escape Room > Build Main Menu.
public static class BuildMainMenu
{
    static readonly Color Cyan = new Color(0f, 0.898f, 1f, 1f);
    static readonly Color Ink = new Color(0.027f, 0.035f, 0.039f, 1f);
    static readonly Color Panel = new Color(0.055f, 0.075f, 0.086f, 0.95f);

    [MenuItem("Tools/Escape Room/Build Main Menu")]
    public static void Build()
    {
        var old = GameObject.Find("MainMenu");
        if (old != null) Object.DestroyImmediate(old);

        var bgPath = "Assets/UI/MenuBackground.jpg";
        var ti = (TextureImporter)AssetImporter.GetAtPath(bgPath);
        if (ti != null && ti.textureType != TextureImporterType.Sprite)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.maxTextureSize = 2048;
            ti.SaveAndReimport();
        }
        var bg = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);

        // ---- root + canvas (world space, 1920x1080 at 1mm/pixel)
        var root = new GameObject("MainMenu");
        var cam = Camera.main;
        Vector3 pos = new Vector3(0.9f, 1.45f, -0.8f);
        if (cam != null)
        {
            var f = cam.transform.forward; f.y = 0; f.Normalize();
            pos = cam.transform.position + f * 1.6f; pos.y = cam.transform.position.y + 0.05f;
            root.transform.rotation = Quaternion.LookRotation(f, Vector3.up);
        }
        root.transform.position = pos;

        var canvasGo = new GameObject("Canvas", typeof(RectTransform));
        canvasGo.transform.SetParent(root.transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;
        canvasGo.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
        canvasGo.AddComponent<GraphicRaycaster>();
        canvasGo.AddComponent<TrackedDeviceGraphicRaycaster>();
        var crt = canvasGo.GetComponent<RectTransform>();
        crt.sizeDelta = new Vector2(1920, 1080);
        crt.localScale = Vector3.one * 0.001f;

        var menu = canvasGo.AddComponent<MainMenu>();

        // menuRoot holds everything so NEW GAME can hide the whole menu
        var menuRoot = Stretch("MenuRoot", canvasGo.transform);
        menu.menuRoot = menuRoot.gameObject;
        menuRoot.gameObject.AddComponent<RectMask2D>();

        // ---- background
        // poster is 2:3 portrait; like the web design, show the band at 35% from the top (hides the baked-in title)
        var bgRt = Rect("Background", menuRoot, new Vector2(0.5f, 1), new Vector2(0, 630), new Vector2(1920, 2880));
        bgRt.pivot = new Vector2(0.5f, 1);
        var bgImg = bgRt.gameObject.AddComponent<Image>();
        bgImg.sprite = bg; bgImg.color = Color.white; bgImg.raycastTarget = false;
        var dim = Stretch("Dim", menuRoot).gameObject.AddComponent<Image>();
        dim.color = new Color(0, 0, 0, 0.55f); dim.raycastTarget = false;

        // ---- top bar
        Text("SystemOnline", menuRoot, "SYSTEM: ONLINE", 26, new Color(1, 1, 1, 0.5f), TextAlignmentOptions.TopLeft, new Vector2(0, 1), new Vector2(60, -40), new Vector2(700, 40));
        Text("Version", menuRoot, "VER 0.9.4_BETA", 26, new Color(1, 1, 1, 0.5f), TextAlignmentOptions.TopRight, new Vector2(1, 1), new Vector2(-60, -40), new Vector2(700, 40));

        // ---- title
        var title = Text("Title", menuRoot, "ESCAPE ROOM", 150, new Color(0.91f, 0.93f, 0.95f, 1f), TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(1500, 190));
        title.fontStyle = FontStyles.Bold; title.characterSpacing = 18;
        Text("Tagline", menuRoot, "ESCAPE SEQUENCE", 30, Cyan, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0, -352), new Vector2(800, 44)).characterSpacing = 14;

        // ---- buttons
        string[] labels = { "NEW GAME", "SETTINGS", "EXIT" };
        string[] subs = { "INITIALIZE SEQUENCE", "SYSTEM CONFIG", "TERMINATE" };
        float y = -440f;
        for (int i = 0; i < 3; i++)
        {
            bool primary = i == 0;
            float h = primary ? 120 : 96;
            var btn = MakeButton(menuRoot, "Btn_" + labels[i], labels[i], subs[i], (i + 1).ToString("00"), primary, new Vector2(0, y), new Vector2(760, h));
            y -= h + 22;
            switch (i)
            {
                case 0: UnityEventTools(btn, menu.NewGame); break;
                case 1: UnityEventTools(btn, menu.OpenSettings); break;
                case 2: UnityEventTools(btn, menu.ExitGame); break;
            }
        }

        // ---- footer
        Text("Footer1", menuRoot, "VR | PUZZLE | ESCAPE | 1-2 PLAYERS", 24, new Color(1, 1, 1, 0.4f), TextAlignmentOptions.BottomLeft, new Vector2(0, 0), new Vector2(60, 40), new Vector2(900, 40));
        Text("Footer2", menuRoot, "COMPLETE THE CIRCUIT. OPEN THE DOOR.", 30, new Color(1, 1, 1, 0.75f), TextAlignmentOptions.Bottom, new Vector2(0.5f, 0), new Vector2(0, 90), new Vector2(1200, 44)).characterSpacing = 8;
        Text("Footer3", menuRoot, "EDITION: LAPTOP // DIRECT DRIVE", 24, new Color(1, 1, 1, 0.4f), TextAlignmentOptions.BottomRight, new Vector2(1, 0), new Vector2(-60, 40), new Vector2(900, 40));

        // ---- loading panel
        var loading = Stretch("LoadingPanel", menuRoot);
        var lImg = loading.gameObject.AddComponent<Image>(); lImg.color = new Color(0.02f, 0.03f, 0.03f, 0.93f);
        Text("LoadingLabel", loading, "INITIALIZING CIRCUIT...", 40, Cyan, TextAlignmentOptions.Left, new Vector2(0.5f, 0.5f), new Vector2(-300, 60), new Vector2(600, 60)).characterSpacing = 10;
        var pct = Text("LoadingPercent", loading, "0%", 40, Color.white, TextAlignmentOptions.Right, new Vector2(0.5f, 0.5f), new Vector2(300, 60), new Vector2(200, 60));
        var barBg = Rect("BarBg", loading, new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(1000, 40));
        barBg.gameObject.AddComponent<Image>().color = new Color(1, 1, 1, 0.08f);
        var fillRt = Stretch("Fill", barBg);
        var fill = fillRt.gameObject.AddComponent<Image>();
        fill.color = Cyan; fill.sprite = Solid(); fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = 0; fill.fillAmount = 0;
        Text("LoadingHint", loading, "TRACE: OK", 26, new Color(1, 1, 1, 0.4f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0, -90), new Vector2(600, 40));
        menu.loadingPanel = loading.gameObject; menu.loadingFill = fill; menu.loadingPercent = pct;

        // ---- settings panel
        var settings = Stretch("SettingsPanel", menuRoot);
        settings.gameObject.AddComponent<Image>().color = new Color(0.02f, 0.03f, 0.03f, 0.85f);
        var card = Rect("Card", settings, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 620));
        card.gameObject.AddComponent<Image>().color = Panel;
        var outline = card.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(1, 1, 1, 0.15f); outline.effectDistance = new Vector2(2, -2);
        var sTitle = Text("SettingsTitle", card, "SYSTEM CONFIG", 44, Color.white, TextAlignmentOptions.TopLeft, new Vector2(0, 1), new Vector2(50, -40), new Vector2(600, 60));
        sTitle.fontStyle = FontStyles.Bold; sTitle.characterSpacing = 10;
        Text("SettingsSub", card, "ADJUST PARAMETERS // IMMEDIATE APPLY", 22, new Color(1, 1, 1, 0.4f), TextAlignmentOptions.TopLeft, new Vector2(0, 1), new Vector2(50, -100), new Vector2(700, 34));

        var close = MakeSimpleButton(card, "CloseButton", "X", new Vector2(1, 1), new Vector2(-60, -60), new Vector2(70, 70), new Color(1, 1, 1, 0.08f), Color.white);
        UnityEventTools(close, menu.CloseSettings);

        Text("VolLabel", card, "MASTER VOLUME", 28, new Color(1, 1, 1, 0.8f), TextAlignmentOptions.TopLeft, new Vector2(0, 1), new Vector2(50, -170), new Vector2(500, 40)).characterSpacing = 6;
        menu.volumeValue = Text("VolValue", card, "74%", 28, Cyan, TextAlignmentOptions.TopRight, new Vector2(1, 1), new Vector2(-50, -170), new Vector2(200, 40));
        menu.volumeSlider = MakeSlider(card, "VolumeSlider", new Vector2(0.5f, 1), new Vector2(0, -250), 800, 74);

        Text("BriLabel", card, "BRIGHTNESS", 28, new Color(1, 1, 1, 0.8f), TextAlignmentOptions.TopLeft, new Vector2(0, 1), new Vector2(50, -300), new Vector2(500, 40)).characterSpacing = 6;
        menu.brightnessValue = Text("BriValue", card, "62%", 28, Cyan, TextAlignmentOptions.TopRight, new Vector2(1, 1), new Vector2(-50, -300), new Vector2(200, 40));
        menu.brightnessSlider = MakeSlider(card, "BrightnessSlider", new Vector2(0.5f, 1), new Vector2(0, -380), 800, 62);

        Text("GfxLabel", card, "GRAPHICS", 28, new Color(1, 1, 1, 0.8f), TextAlignmentOptions.TopLeft, new Vector2(0, 1), new Vector2(50, -440), new Vector2(500, 40)).characterSpacing = 6;
        var high = MakeSimpleButton(card, "HighButton", "HIGH", new Vector2(0.5f, 1), new Vector2(-205, -540), new Vector2(390, 80), new Color(1, 1, 1, 0.06f), Color.white);
        var ultra = MakeSimpleButton(card, "UltraButton", "ULTRA", new Vector2(0.5f, 1), new Vector2(205, -540), new Vector2(390, 80), Color.white, Color.black);
        menu.highButton = high.GetComponent<Image>(); menu.ultraButton = ultra.GetComponent<Image>();
        UnityEventTools(high, menu.SetHigh);
        UnityEventTools(ultra, menu.SetUltra);
        menu.settingsPanel = settings.gameObject;
        settings.gameObject.SetActive(false);
        loading.gameObject.SetActive(false);

        // ---- event system with XR UI input module
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(XRUIInputModule));
            Undo.RegisterCreatedObjectUndo(es, "EventSystem");
        }

        Undo.RegisterCreatedObjectUndo(root, "Build Main Menu");
        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Main menu built at " + pos);
    }

    // ---------------- helpers ----------------
    internal static Sprite _solid;
    internal static Sprite Solid()
    {
        if (_solid == null) _solid = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        return _solid;
    }

    internal static RectTransform Stretch(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return rt;
    }
    internal static RectTransform Stretch(string name, RectTransform parent) { return Stretch(name, (Transform)parent); }

    internal static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = anchor;
        if (anchor.y == 0.5f) rt.pivot = new Vector2(anchor.x, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        return rt;
    }

    internal static TextMeshProUGUI AddText(RectTransform rt, string text, float size, Color color, TextAlignmentOptions align)
    {
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = color; t.alignment = align; t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        return t;
    }

    internal static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color, TextAlignmentOptions align, Vector2 anchor, Vector2 pos, Vector2 sizeDelta)
    {
        var rt = Rect(name, parent, anchor, pos, sizeDelta);
        // pivot follows alignment horizontally so positions behave as expected
        if (align == TextAlignmentOptions.Center || align == TextAlignmentOptions.Bottom) rt.pivot = new Vector2(0.5f, rt.pivot.y);
        if (align == TextAlignmentOptions.Left) rt.pivot = new Vector2(0.5f, 0.5f);
        if (align == TextAlignmentOptions.Right) rt.pivot = new Vector2(0.5f, 0.5f);
        return AddText(rt, text, size, color, align);
    }
    internal static TextMeshProUGUI Text(string name, RectTransform parent, string text, float size, Color color, TextAlignmentOptions align, Vector2 anchor, Vector2 pos, Vector2 sizeDelta)
    { return Text(name, (Transform)parent, text, size, color, align, anchor, pos, sizeDelta); }

    internal static void UnityEventTools(Button b, UnityEngine.Events.UnityAction a)
    {
        UnityEditor.Events.UnityEventTools.AddPersistentListener(b.onClick, a);
    }

    internal static Button MakeButton(Transform parent, string name, string label, string sub, string num, bool primary, Vector2 pos, Vector2 size)
    {
        var rt = Rect(name, parent, new Vector2(0.5f, 1), pos, size);
        rt.pivot = new Vector2(0.5f, 1);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = Color.white; // state colors below carry the actual look
        var outline = rt.gameObject.AddComponent<Outline>();
        outline.effectColor = primary ? new Color(0f, 0.898f, 1f, 0.55f) : new Color(1, 1, 1, 0.15f);
        outline.effectDistance = new Vector2(2, -2);
        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        var cb = b.colors;
        cb.normalColor = primary ? new Color(0.02f, 0.1f, 0.12f, 0.75f) : new Color(0f, 0f, 0f, 0.55f);
        cb.highlightedColor = primary ? new Color(0f, 0.55f, 0.65f, 0.85f) : new Color(0.25f, 0.28f, 0.3f, 0.8f);
        cb.pressedColor = new Color(0f, 0.7f, 0.8f, 0.95f);
        cb.selectedColor = cb.normalColor;
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        b.colors = cb;

        var accent = Rect("Accent", rt, new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(6, size.y * 0.68f));
        accent.pivot = new Vector2(0, 0.5f);
        accent.gameObject.AddComponent<Image>().color = Cyan;
        accent.GetComponent<Image>().raycastTarget = false;

        AddText(Rect("Num", rt, new Vector2(0, 0.5f), new Vector2(34, 0), new Vector2(70, 40)), num, 24, primary ? Cyan : new Color(1, 1, 1, 0.3f), TextAlignmentOptions.Left);
        var l = AddText(Rect("Label", rt, new Vector2(0, 0.5f), new Vector2(110, primary ? 14 : 12), new Vector2(560, 50)), label, primary ? 44 : 38, primary ? Color.white : new Color(1, 1, 1, 0.85f), TextAlignmentOptions.Left);
        l.fontStyle = FontStyles.Bold; l.characterSpacing = 10;
        AddText(Rect("Sub", rt, new Vector2(0, 0.5f), new Vector2(110, primary ? -28 : -24), new Vector2(560, 30)), sub, 20, primary ? new Color(0f, 0.898f, 1f, 0.85f) : new Color(1, 1, 1, 0.35f), TextAlignmentOptions.Left).characterSpacing = 8;
        AddText(Rect("Arrow", rt, new Vector2(1, 0.5f), new Vector2(-60, 0), new Vector2(60, 40)), ">", 34, primary ? Cyan : new Color(1, 1, 1, 0.5f), TextAlignmentOptions.Center);
        // pivots for left-anchored text rects
        foreach (var n in new[] { "Num", "Label", "Sub" }) { var c = rt.Find(n) as RectTransform; c.pivot = new Vector2(0, 0.5f); }
        (rt.Find("Arrow") as RectTransform).pivot = new Vector2(1, 0.5f);
        return b;
    }

    internal static Button MakeSimpleButton(Transform parent, string name, string label, Vector2 anchor, Vector2 pos, Vector2 size, Color bg, Color textColor)
    {
        var rt = Rect(name, parent, anchor, pos, size);
        rt.pivot = new Vector2(0.5f, 0.5f);
        var img = rt.gameObject.AddComponent<Image>(); img.color = bg;
        var b = rt.gameObject.AddComponent<Button>(); b.targetGraphic = img;
        var cb = b.colors; cb.highlightedColor = new Color(0.8f, 1f, 1f, 1f); cb.fadeDuration = 0.08f; b.colors = cb;
        var t = AddText(Stretch("Label", rt), label, 32, textColor, TextAlignmentOptions.Center);
        t.fontStyle = FontStyles.Bold; t.characterSpacing = 8;
        return b;
    }

    internal static Slider MakeSlider(Transform parent, string name, Vector2 anchor, Vector2 pos, float width, float value)
    {
        var rt = Rect(name, parent, anchor, pos, new Vector2(width, 50));
        rt.pivot = new Vector2(0.5f, 1);
        var bgRt = Stretch("Background", rt);
        bgRt.anchorMin = new Vector2(0, 0.5f); bgRt.anchorMax = new Vector2(1, 0.5f); bgRt.sizeDelta = new Vector2(0, 14);
        bgRt.gameObject.AddComponent<Image>().color = new Color(1, 1, 1, 0.1f);

        var fillArea = Stretch("Fill Area", rt);
        fillArea.anchorMin = new Vector2(0, 0.5f); fillArea.anchorMax = new Vector2(1, 0.5f); fillArea.sizeDelta = new Vector2(-30, 14);
        var fillRt = Stretch("Fill", fillArea); fillRt.sizeDelta = new Vector2(0, 0);
        fillRt.gameObject.AddComponent<Image>().color = Cyan;

        var handleArea = Stretch("Handle Slide Area", rt);
        handleArea.offsetMin = new Vector2(15, 0); handleArea.offsetMax = new Vector2(-15, 0);
        var handleRt = Rect("Handle", handleArea, new Vector2(0, 0.5f), Vector2.zero, new Vector2(36, 46));
        handleRt.pivot = new Vector2(0.5f, 0.5f);
        var hImg = handleRt.gameObject.AddComponent<Image>(); hImg.color = Color.white;

        var s = rt.gameObject.AddComponent<Slider>();
        s.fillRect = fillRt; s.handleRect = handleRt; s.targetGraphic = hImg;
        s.minValue = 0; s.maxValue = 100; s.wholeNumbers = true; s.value = value;
        return s;
    }
}
