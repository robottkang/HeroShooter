using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class LobbyUISetup
{
    const string ScenePath   = "Assets/Scenes/Lobby.unity";
    const string PrefabPath  = "Assets/Prefabs/UI/RoomListItem.prefab";

    [MenuItem("Tools/Setup Lobby UI")]
    static void Run()
    {
        EnsureFolders();

        var itemPrefab = CreateRoomListItemPrefab();

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // Remove any existing Canvas to start clean
        var existing = Object.FindFirstObjectByType<Canvas>();
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        BuildUI(itemPrefab);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[LobbyUISetup] Done — open Lobby.unity to see the result.");
    }

    // ──────────────────────────────────────────────────────────
    //  RoomListItem prefab
    // ──────────────────────────────────────────────────────────

    static GameObject CreateRoomListItemPrefab()
    {
        var root = new GameObject("RoomListItem");
        root.AddComponent<RectTransform>();

        var bg = root.AddComponent<Image>();
        bg.color = new Color(0.18f, 0.18f, 0.18f);

        var le = root.AddComponent<LayoutElement>();
        le.preferredHeight = 56f;
        le.flexibleWidth   = 1f;

        var hlg = root.AddComponent<HorizontalLayoutGroup>();
        hlg.padding             = new RectOffset(10, 10, 0, 0);
        hlg.spacing             = 8;
        hlg.childAlignment      = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth   = true;
        hlg.childControlHeight  = true;

        // Room name
        var roomNameTMP = MakeText(root.transform, "RoomNameText", "Room Name", 16,
            TextAlignmentOptions.MidlineLeft, flexWidth: 1f);

        // Host name
        var hostNameTMP = MakeText(root.transform, "HostNameText", "Host Name", 16,
            TextAlignmentOptions.Midline, flexWidth: 1f);
        hostNameTMP.color = new Color(0.75f, 0.75f, 0.75f);

        // Join button
        var joinBtn = MakeButton(root.transform, "JoinButton", "Join", 14, prefWidth: 80f);

        // Wire RoomListItem component
        var item = root.AddComponent<RoomListItem>();
        var so   = new SerializedObject(item);
        so.FindProperty("roomNameText").objectReferenceValue = roomNameTMP;
        so.FindProperty("hostNameText").objectReferenceValue = hostNameTMP;
        so.FindProperty("joinButton").objectReferenceValue   = joinBtn;
        so.ApplyModifiedPropertiesWithoutUndo();

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    // ──────────────────────────────────────────────────────────
    //  Main UI hierarchy
    // ──────────────────────────────────────────────────────────

    static void BuildUI(GameObject itemPrefab)
    {
        // ── Canvas ──────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = new Vector2(1920, 1080);
        scaler.screenMatchMode      = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight   = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        EnsureEventSystem();

        // ── Root Panel ──────────────────────────────────────
        var rootPanel = MakePanel(canvasGO.transform, "RootPanel", new Color(0.08f, 0.08f, 0.08f));
        FillRT(rootPanel);

        var rootVLG = rootPanel.AddComponent<VerticalLayoutGroup>();
        rootVLG.padding              = new RectOffset(24, 24, 24, 24);
        rootVLG.spacing              = 16;
        rootVLG.childForceExpandWidth  = true;
        rootVLG.childForceExpandHeight = false;
        rootVLG.childControlWidth   = true;
        rootVLG.childControlHeight  = true;

        // ── Player Name InputField (top, full width) ─────────
        var playerNameInput = MakeInputField(rootPanel.transform, "PlayerNameInput",
            "Enter player name...", 60);

        // ── Panels Row ───────────────────────────────────────
        var panelsRow = new GameObject("PanelsRow", typeof(RectTransform));
        panelsRow.transform.SetParent(rootPanel.transform, false);
        var rowLE = panelsRow.AddComponent<LayoutElement>();
        rowLE.flexibleHeight = 1;
        rowLE.flexibleWidth  = 1;
        var rowHLG = panelsRow.AddComponent<HorizontalLayoutGroup>();
        rowHLG.spacing             = 16;
        rowHLG.childForceExpandWidth  = true;
        rowHLG.childForceExpandHeight = true;
        rowHLG.childControlWidth   = true;
        rowHLG.childControlHeight  = true;

        // ── Create Room Panel (left) ─────────────────────────
        var createPanel = MakePanel(panelsRow.transform, "CreateRoomPanel", new Color(0.12f, 0.12f, 0.12f));
        createPanel.AddComponent<LayoutElement>().flexibleWidth = 1;
        var createVLG = createPanel.AddComponent<VerticalLayoutGroup>();
        ApplyPanelVLG(createVLG);

        MakeHeaderLabel(createPanel.transform, "Create Room");
        var roomNameInput   = MakeInputField(createPanel.transform, "RoomNameInput", "Enter room name...", 50);
        var mapPreviewImage = MakeMapPreview(createPanel.transform, "MapPreviewImage", 120f);
        var mapDropdown     = MakeDropdown(createPanel.transform, "MapDropdown", 50f);
        var createButton    = MakeButton(createPanel.transform, "CreateRoomButton", "Create Room", 18, prefHeight: 52f);

        // ── Browse Panel (right) ─────────────────────────────
        var browsePanel = MakePanel(panelsRow.transform, "BrowsePanel", new Color(0.12f, 0.12f, 0.12f));
        browsePanel.AddComponent<LayoutElement>().flexibleWidth = 1;
        var browseVLG = browsePanel.AddComponent<VerticalLayoutGroup>();
        ApplyPanelVLG(browseVLG);

        MakeHeaderLabel(browsePanel.transform, "Room List");
        var searchInput     = MakeInputField(browsePanel.transform, "SearchInput", "Search room name...", 50);
        var roomListContent = MakeScrollView(browsePanel.transform);
        var refreshButton   = MakeButton(browsePanel.transform, "RefreshButton", "Refresh", 18, prefHeight: 52f);

        // ── Connecting Overlay ───────────────────────────────
        var connectingPanel = MakePanel(canvasGO.transform, "ConnectingPanel", new Color(0, 0, 0, 0.78f));
        FillRT(connectingPanel);

        var connectTextGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        connectTextGO.transform.SetParent(connectingPanel.transform, false);
        FillRT(connectTextGO.GetComponent<RectTransform>());
        var connectTMP = connectTextGO.GetComponent<TextMeshProUGUI>();
        connectTMP.text      = "Connecting to server...";
        connectTMP.fontSize  = 28;
        connectTMP.color     = Color.white;
        connectTMP.alignment = TextAlignmentOptions.Center;

        // ── LobbyManager ────────────────────────────────────
        var managerGO  = new GameObject("LobbyManager");
        var lobbyMgr   = managerGO.AddComponent<LobbyManager>();
        var mso        = new SerializedObject(lobbyMgr);
        mso.FindProperty("playerNameInput").objectReferenceValue  = playerNameInput;
        mso.FindProperty("roomNameInput").objectReferenceValue    = roomNameInput;
        mso.FindProperty("createRoomButton").objectReferenceValue = createButton;
        mso.FindProperty("searchInput").objectReferenceValue      = searchInput;
        mso.FindProperty("roomListContent").objectReferenceValue  = roomListContent;
        mso.FindProperty("refreshButton").objectReferenceValue    = refreshButton;
        mso.FindProperty("connectingPanel").objectReferenceValue  = connectingPanel;
        mso.FindProperty("mapDropdown").objectReferenceValue      = mapDropdown;
        mso.FindProperty("mapPreviewImage").objectReferenceValue  = mapPreviewImage;
        mso.FindProperty("roomListItemPrefab").objectReferenceValue = itemPrefab;
        mso.ApplyModifiedPropertiesWithoutUndo();
    }

    // ──────────────────────────────────────────────────────────
    //  Helper: UI element factories
    // ──────────────────────────────────────────────────────────

    static TMP_InputField MakeInputField(Transform parent, string goName, string placeholder, float height)
    {
        var go = new GameObject(goName, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        go.transform.SetParent(parent, false);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;

        go.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f);

        // Text Area (viewport with mask)
        var textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        textArea.transform.SetParent(go.transform, false);
        var taRT = textArea.GetComponent<RectTransform>();
        taRT.anchorMin   = Vector2.zero;
        taRT.anchorMax   = Vector2.one;
        taRT.offsetMin   = new Vector2(10, 6);
        taRT.offsetMax   = new Vector2(-10, -6);

        // Placeholder
        var phGO  = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        phGO.transform.SetParent(textArea.transform, false);
        FillRT(phGO.GetComponent<RectTransform>());
        var phTMP = phGO.GetComponent<TextMeshProUGUI>();
        phTMP.text      = placeholder;
        phTMP.color     = new Color(0.45f, 0.45f, 0.45f);
        phTMP.fontSize  = 16;
        phTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Input text
        var txtGO  = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(textArea.transform, false);
        FillRT(txtGO.GetComponent<RectTransform>());
        var txtTMP = txtGO.GetComponent<TextMeshProUGUI>();
        txtTMP.color     = Color.white;
        txtTMP.fontSize  = 16;
        txtTMP.alignment = TextAlignmentOptions.MidlineLeft;

        var tf = go.GetComponent<TMP_InputField>();
        tf.textViewport  = taRT;
        tf.textComponent = txtTMP;
        tf.placeholder   = phTMP;
        tf.lineType      = TMP_InputField.LineType.SingleLine;

        return tf;
    }

    static Button MakeButton(Transform parent, string goName, string label,
        float fontSize, float prefWidth = -1f, float prefHeight = 50f)
    {
        var go = new GameObject(goName, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = prefHeight;
        if (prefWidth > 0) le.preferredWidth = prefWidth;

        go.GetComponent<Image>().color = new Color(0.22f, 0.47f, 1f);

        var txtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(go.transform, false);
        FillRT(txtGO.GetComponent<RectTransform>());
        var tmp = txtGO.GetComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.color     = Color.white;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;

        return go.GetComponent<Button>();
    }

    static Transform MakeScrollView(Transform parent)
    {
        var go = new GameObject("ScrollView", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        go.transform.SetParent(parent, false);

        var le = go.AddComponent<LayoutElement>();
        le.flexibleHeight = 1;
        le.flexibleWidth  = 1;

        go.GetComponent<Image>().color = new Color(0.09f, 0.09f, 0.09f);

        var scroll     = go.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical   = true;
        scroll.scrollSensitivity = 30f;

        // Viewport
        var vp   = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        vp.transform.SetParent(go.transform, false);
        var vpRT = vp.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero;
        vpRT.offsetMax = Vector2.zero;

        // Content
        var content   = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(vp.transform, false);
        var cRT = content.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0, 1);
        cRT.anchorMax = new Vector2(1, 1);
        cRT.pivot     = new Vector2(0.5f, 1);
        cRT.sizeDelta = Vector2.zero;

        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing             = 4;
        vlg.padding             = new RectOffset(4, 4, 4, 4);
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth   = true;
        vlg.childControlHeight  = true;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        scroll.viewport = vpRT;
        scroll.content  = cRT;

        return content.transform;
    }

    static TextMeshProUGUI MakeText(Transform parent, string goName, string text,
        float fontSize, TextAlignmentOptions alignment, float flexWidth = 0f, float prefWidth = -1f)
    {
        var go  = new GameObject(goName, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var le = go.AddComponent<LayoutElement>();
        if (flexWidth > 0)  le.flexibleWidth  = flexWidth;
        if (prefWidth > 0)  le.preferredWidth = prefWidth;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = Color.white;
        tmp.alignment = alignment;
        return tmp;
    }

    static Image MakeMapPreview(Transform parent, string goName, float height)
    {
        var go = new GameObject(goName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        le.flexibleHeight  = 1;

        var img = go.GetComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f);
        img.preserveAspect = true;
        return img;
    }

    static TMP_Dropdown MakeDropdown(Transform parent, string goName, float height)
    {
        var go = new GameObject(goName, typeof(RectTransform), typeof(Image), typeof(TMP_Dropdown));
        go.transform.SetParent(parent, false);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;

        go.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f);

        // Caption label
        var labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(go.transform, false);
        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = new Vector2(10, 2);
        labelRT.offsetMax = new Vector2(-30, -2);
        var labelTMP = labelGO.GetComponent<TextMeshProUGUI>();
        labelTMP.color     = Color.white;
        labelTMP.fontSize  = 16;
        labelTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Arrow indicator
        var arrowGO = new GameObject("Arrow", typeof(RectTransform), typeof(TextMeshProUGUI));
        arrowGO.transform.SetParent(go.transform, false);
        var arrowRT = arrowGO.GetComponent<RectTransform>();
        arrowRT.anchorMin       = new Vector2(1, 0.5f);
        arrowRT.anchorMax       = new Vector2(1, 0.5f);
        arrowRT.pivot           = new Vector2(1, 0.5f);
        arrowRT.sizeDelta       = new Vector2(30, 30);
        arrowRT.anchoredPosition = Vector2.zero;
        var arrowTMP = arrowGO.GetComponent<TextMeshProUGUI>();
        arrowTMP.text      = "▼";
        arrowTMP.color     = Color.white;
        arrowTMP.fontSize  = 12;
        arrowTMP.alignment = TextAlignmentOptions.Center;

        // Template (disabled — shown when dropdown opens)
        var templateGO = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        templateGO.transform.SetParent(go.transform, false);
        templateGO.SetActive(false);
        var templateRT = templateGO.GetComponent<RectTransform>();
        templateRT.anchorMin        = new Vector2(0, 0);
        templateRT.anchorMax        = new Vector2(1, 0);
        templateRT.pivot            = new Vector2(0.5f, 1);
        templateRT.anchoredPosition = Vector2.zero;
        templateRT.sizeDelta        = new Vector2(0, 160);
        templateGO.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f);
        var scrollRect = templateGO.GetComponent<ScrollRect>();
        scrollRect.horizontal        = false;
        scrollRect.movementType      = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 30f;

        // Viewport
        var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGO.transform.SetParent(templateGO.transform, false);
        var vpRT = vpGO.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero;
        vpRT.offsetMax = Vector2.zero;
        vpGO.GetComponent<Mask>().showMaskGraphic = false;

        // Content
        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(vpGO.transform, false);
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin        = new Vector2(0, 1);
        contentRT.anchorMax        = new Vector2(1, 1);
        contentRT.pivot            = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta        = Vector2.zero;
        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Item template
        var itemGO = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
        itemGO.transform.SetParent(contentGO.transform, false);
        var itemRT = itemGO.GetComponent<RectTransform>();
        itemRT.anchorMin  = new Vector2(0, 0.5f);
        itemRT.anchorMax  = new Vector2(1, 0.5f);
        itemRT.sizeDelta  = new Vector2(0, 40);
        var itemLE = itemGO.AddComponent<LayoutElement>();
        itemLE.minHeight       = 40f;
        itemLE.preferredHeight = 40f;

        var itemBgGO = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
        itemBgGO.transform.SetParent(itemGO.transform, false);
        FillRT(itemBgGO.GetComponent<RectTransform>());
        itemBgGO.GetComponent<Image>().color = new Color(0.22f, 0.22f, 0.22f);

        var checkGO = new GameObject("Item Checkmark", typeof(RectTransform), typeof(Image));
        checkGO.transform.SetParent(itemGO.transform, false);
        var checkRT = checkGO.GetComponent<RectTransform>();
        checkRT.anchorMin        = new Vector2(0, 0.5f);
        checkRT.anchorMax        = new Vector2(0, 0.5f);
        checkRT.pivot            = new Vector2(0.5f, 0.5f);
        checkRT.sizeDelta        = new Vector2(20, 20);
        checkRT.anchoredPosition = new Vector2(14, 0);
        checkGO.GetComponent<Image>().color = new Color(0.22f, 0.47f, 1f);

        var itemLabelGO = new GameObject("Item Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        itemLabelGO.transform.SetParent(itemGO.transform, false);
        var itemLabelRT = itemLabelGO.GetComponent<RectTransform>();
        itemLabelRT.anchorMin = Vector2.zero;
        itemLabelRT.anchorMax = Vector2.one;
        itemLabelRT.offsetMin = new Vector2(30, 0);
        itemLabelRT.offsetMax = new Vector2(-5, 0);
        var itemLabelTMP = itemLabelGO.GetComponent<TextMeshProUGUI>();
        itemLabelTMP.color     = Color.white;
        itemLabelTMP.fontSize  = 16;
        itemLabelTMP.alignment = TextAlignmentOptions.MidlineLeft;

        var toggle = itemGO.GetComponent<Toggle>();
        toggle.targetGraphic = itemBgGO.GetComponent<Image>();
        toggle.graphic       = checkGO.GetComponent<Image>();

        scrollRect.viewport = vpRT;
        scrollRect.content  = contentRT;

        var dropdown = go.GetComponent<TMP_Dropdown>();
        dropdown.targetGraphic = go.GetComponent<Image>();
        dropdown.captionText   = labelTMP;
        dropdown.itemText      = itemLabelTMP;
        dropdown.template      = templateRT;

        return dropdown;
    }

    static GameObject MakePanel(Transform parent, string goName, Color color)
    {
        var go  = new GameObject(goName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    static void MakeHeaderLabel(Transform parent, string text)
    {
        var go  = new GameObject("TitleLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 40f;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    static void ApplyPanelVLG(VerticalLayoutGroup vlg)
    {
        vlg.padding              = new RectOffset(16, 16, 16, 16);
        vlg.spacing              = 12;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth   = true;
        vlg.childControlHeight  = true;
    }

    // ──────────────────────────────────────────────────────────
    //  Utilities
    // ──────────────────────────────────────────────────────────

    static void FillRT(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void FillRT(GameObject go) => FillRT(go.GetComponent<RectTransform>());

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
    }
}
