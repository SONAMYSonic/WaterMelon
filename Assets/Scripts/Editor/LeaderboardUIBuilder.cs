using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class LeaderboardUIBuilder : EditorWindow
{
    [MenuItem("Tools/Build Leaderboard UI")]
    public static void Build()
    {
        // === 1. LeaderboardManager 오브젝트 생성 ===
        var existingLM = Object.FindFirstObjectByType<LeaderboardManager>();
        if (existingLM == null)
        {
            var lmGo = new GameObject("LeaderboardManager");
            lmGo.AddComponent<LeaderboardManager>();
            Undo.RegisterCreatedObjectUndo(lmGo, "Create LeaderboardManager");
        }

        // === 2. GameCanvas 찾기 ===
        var canvas = GameObject.Find("GameCanvas");
        if (canvas == null)
        {
            return;
        }
        var canvasTransform = canvas.transform;

        // === 3. LeaderboardButton 생성 (설정 버튼 옆) ===
        var settingsBtn = canvasTransform.Find("SettingsButton");
        var lbButton = CreateButton(canvasTransform, "LeaderboardButton", "랭킹");
        var lbBtnRT = lbButton.GetComponent<RectTransform>();
        lbBtnRT.anchorMin = new Vector2(0, 0);
        lbBtnRT.anchorMax = new Vector2(0, 0);
        lbBtnRT.pivot = new Vector2(0.5f, 0.5f);
        lbBtnRT.sizeDelta = new Vector2(149, 62);
        lbBtnRT.anchoredPosition = new Vector2(383.5f, 200f);

        // 버튼 배경 색상
        var lbBtnImg = lbButton.GetComponent<Image>();
        lbBtnImg.color = new Color(0.2f, 0.5f, 0.8f, 1f); // 파란색

        // 버튼 텍스트 스타일
        var lbBtnText = lbButton.GetComponentInChildren<TextMeshProUGUI>();
        if (lbBtnText != null)
        {
            lbBtnText.fontSize = 26;
            lbBtnText.color = Color.white;
            lbBtnText.fontStyle = FontStyles.Bold;
        }

        // === 4. LeaderboardPanel 생성 ===
        var lbPanel = CreatePanel(canvasTransform, "LeaderboardPanel", 500, 600,
            new Color(0.1f, 0.1f, 0.15f, 0.95f));
        lbPanel.SetActive(false);

        var lbPanelRT = lbPanel.GetComponent<RectTransform>();
        // VerticalLayoutGroup
        var panelVLG = lbPanel.AddComponent<VerticalLayoutGroup>();
        panelVLG.spacing = 8;
        panelVLG.childAlignment = TextAnchor.UpperCenter;
        panelVLG.padding = new RectOffset(15, 15, 15, 15);
        panelVLG.childControlWidth = true;
        panelVLG.childControlHeight = false;
        panelVLG.childForceExpandWidth = true;
        panelVLG.childForceExpandHeight = false;

        // 4-1. 타이틀
        var lbTitle = CreateTMPText(lbPanel.transform, "LBTitle", "랭킹", 32,
            Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        AddLayoutElement(lbTitle, -1, 50);

        // 4-2. 헤더 행
        var headerRow = new GameObject("LBHeader", typeof(RectTransform),
            typeof(HorizontalLayoutGroup));
        headerRow.transform.SetParent(lbPanel.transform, false);
        var headerHLG = headerRow.GetComponent<HorizontalLayoutGroup>();
        headerHLG.spacing = 10;
        headerHLG.childAlignment = TextAnchor.MiddleCenter;
        headerHLG.padding = new RectOffset(10, 10, 2, 2);
        headerHLG.childControlWidth = true;
        headerHLG.childControlHeight = true;
        headerHLG.childForceExpandWidth = false;
        headerHLG.childForceExpandHeight = false;
        AddLayoutElement(headerRow, -1, 36);

        // 헤더 셀
        CreateHeaderCell(headerRow.transform, "순위", 60);
        CreateHeaderCell(headerRow.transform, "이름", 220);
        CreateHeaderCell(headerRow.transform, "점수", 140);

        // 4-3. 구분선
        var divider = new GameObject("Divider", typeof(RectTransform), typeof(Image));
        divider.transform.SetParent(lbPanel.transform, false);
        divider.GetComponent<Image>().color = new Color(1, 1, 1, 0.3f);
        AddLayoutElement(divider, -1, 2);

        // 4-4. ScrollView
        var scrollView = CreateScrollView(lbPanel.transform, "LBScrollView", 420);

        // 4-5. 푸터 (새로고침 버튼 + 쿨다운 텍스트 + 닫기 버튼)
        var footer = new GameObject("LBFooter", typeof(RectTransform),
            typeof(HorizontalLayoutGroup));
        footer.transform.SetParent(lbPanel.transform, false);
        var footerHLG = footer.GetComponent<HorizontalLayoutGroup>();
        footerHLG.spacing = 10;
        footerHLG.childAlignment = TextAnchor.MiddleCenter;
        footerHLG.padding = new RectOffset(5, 5, 5, 5);
        footerHLG.childControlWidth = false;
        footerHLG.childControlHeight = false;
        footerHLG.childForceExpandWidth = false;
        footerHLG.childForceExpandHeight = false;
        AddLayoutElement(footer, -1, 50);

        // 새로고침 버튼
        var refreshBtn = CreateButton(footer.transform, "LBRefreshBtn", "새로고침");
        var refreshRT = refreshBtn.GetComponent<RectTransform>();
        refreshRT.sizeDelta = new Vector2(130, 40);
        var refreshBtnImg = refreshBtn.GetComponent<Image>();
        refreshBtnImg.color = new Color(0.3f, 0.6f, 0.3f, 1f);
        var refreshText = refreshBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (refreshText != null)
        {
            refreshText.fontSize = 18;
            refreshText.color = Color.white;
        }

        // 쿨다운 텍스트
        var cdText = CreateTMPText(footer.transform, "CooldownText", "", 14,
            new Color(0.7f, 0.7f, 0.7f), TextAlignmentOptions.Center, FontStyles.Normal);
        var cdRT = cdText.GetComponent<RectTransform>();
        cdRT.sizeDelta = new Vector2(180, 40);

        // 닫기 버튼
        var closeBtn = CreateButton(footer.transform, "LBCloseBtn", "닫기");
        var closeRT = closeBtn.GetComponent<RectTransform>();
        closeRT.sizeDelta = new Vector2(100, 40);
        var closeBtnImg = closeBtn.GetComponent<Image>();
        closeBtnImg.color = new Color(0.6f, 0.3f, 0.3f, 1f);
        var closeText = closeBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (closeText != null)
        {
            closeText.fontSize = 18;
            closeText.color = Color.white;
        }

        // === 5. GameOverPanel 수정 (닉네임 입력 추가) ===
        var goPanel = canvasTransform.Find("GameOverPanel");
        if (goPanel != null)
        {
            // 패널 크기 확대
            var goPanelRT = goPanel.GetComponent<RectTransform>();
            goPanelRT.sizeDelta = new Vector2(460, 420);

            // NicknameGroup 생성 (TMP_InputField + 등록 버튼)
            var nickGroup = new GameObject("NicknameGroup", typeof(RectTransform),
                typeof(HorizontalLayoutGroup));
            nickGroup.transform.SetParent(goPanel, false);
            var nickGroupRT = nickGroup.GetComponent<RectTransform>();
            nickGroupRT.anchorMin = new Vector2(0.5f, 1f);
            nickGroupRT.anchorMax = new Vector2(0.5f, 1f);
            nickGroupRT.pivot = new Vector2(0.5f, 1f);
            nickGroupRT.anchoredPosition = new Vector2(0f, -170f);
            nickGroupRT.sizeDelta = new Vector2(380, 50);

            var nickHLG = nickGroup.GetComponent<HorizontalLayoutGroup>();
            nickHLG.spacing = 10;
            nickHLG.childAlignment = TextAnchor.MiddleCenter;
            nickHLG.childControlWidth = false;
            nickHLG.childControlHeight = false;
            nickHLG.childForceExpandWidth = false;
            nickHLG.childForceExpandHeight = false;

            // TMP_InputField
            var inputGo = CreateTMPInputField(nickGroup.transform, "NicknameInput", 260, 45);

            // 등록 버튼
            var submitBtn = CreateButton(nickGroup.transform, "SubmitBtn", "등록");
            var submitRT = submitBtn.GetComponent<RectTransform>();
            submitRT.sizeDelta = new Vector2(100, 45);
            var submitImg = submitBtn.GetComponent<Image>();
            submitImg.color = new Color(0.2f, 0.6f, 0.2f, 1f);
            var submitText = submitBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (submitText != null)
            {
                submitText.fontSize = 22;
                submitText.color = Color.white;
                submitText.fontStyle = FontStyles.Bold;
            }

            // 건너뛰기 버튼
            var skipBtn = CreateButton(goPanel, "SkipBtn", "건너뛰기");
            var skipRT = skipBtn.GetComponent<RectTransform>();
            skipRT.anchorMin = new Vector2(0.5f, 1f);
            skipRT.anchorMax = new Vector2(0.5f, 1f);
            skipRT.pivot = new Vector2(0.5f, 1f);
            skipRT.anchoredPosition = new Vector2(0f, -230f);
            skipRT.sizeDelta = new Vector2(200, 40);
            var skipImg = skipBtn.GetComponent<Image>();
            skipImg.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            var skipText = skipBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (skipText != null)
            {
                skipText.fontSize = 20;
                skipText.color = new Color(0.8f, 0.8f, 0.8f);
            }

            // 등록 완료 텍스트
            var submittedText = CreateTMPText(goPanel, "SubmittedText", "등록 완료!", 24,
                new Color(0.3f, 1f, 0.3f), TextAlignmentOptions.Center, FontStyles.Bold);
            var submittedRT = submittedText.GetComponent<RectTransform>();
            submittedRT.anchorMin = new Vector2(0.5f, 1f);
            submittedRT.anchorMax = new Vector2(0.5f, 1f);
            submittedRT.pivot = new Vector2(0.5f, 1f);
            submittedRT.anchoredPosition = new Vector2(0f, -200f);
            submittedRT.sizeDelta = new Vector2(300, 40);
            submittedText.SetActive(false);

            // RestartBtn 위치 조정
            var restartBtnT = goPanel.Find("RestartBtn");
            if (restartBtnT != null)
            {
                var restartRT = restartBtnT.GetComponent<RectTransform>();
                restartRT.anchorMin = new Vector2(0.5f, 1f);
                restartRT.anchorMax = new Vector2(0.5f, 1f);
                restartRT.pivot = new Vector2(0.5f, 1f);
                restartRT.anchoredPosition = new Vector2(0f, -320f);
            }

        }

        // === 6. UIManager에 참조 연결 ===
        var uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            var so = new SerializedObject(uiManager);

            // 리더보드 패널 참조
            SetRef(so, "leaderboardPanel", lbPanel);
            SetRef(so, "leaderboardButton", lbButton);
            SetRef(so, "leaderboardCloseButton",
                lbPanel.transform.Find("LBFooter/LBCloseBtn")?.gameObject);
            SetRef(so, "leaderboardRefreshButton",
                lbPanel.transform.Find("LBFooter/LBRefreshBtn")?.gameObject);
            SetRef(so, "leaderboardContent",
                lbPanel.transform.Find("LBScrollView/Viewport/LBContent")?.gameObject);
            SetRef(so, "cooldownText",
                lbPanel.transform.Find("LBFooter/CooldownText")?.gameObject);

            // 닉네임 관련 참조
            if (goPanel != null)
            {
                SetRef(so, "nicknameGroup", goPanel.Find("NicknameGroup")?.gameObject);
                SetRef(so, "nicknameInput",
                    goPanel.Find("NicknameGroup/NicknameInput")?.gameObject);
                SetRef(so, "submitScoreButton",
                    goPanel.Find("NicknameGroup/SubmitBtn")?.gameObject);
                SetRef(so, "skipButton", goPanel.Find("SkipBtn")?.gameObject);
                SetRef(so, "submittedText", goPanel.Find("SubmittedText")?.gameObject);
            }

            so.ApplyModifiedProperties();
        }

        // === 7. 패널 렌더 순서 조정 ===
        // LeaderboardPanel과 GameOverPanel이 다른 패널 위에 오도록
        if (goPanel != null) goPanel.SetAsLastSibling();
        lbPanel.transform.SetAsLastSibling();

    }

    // ===== 헬퍼 메서드 =====

    static void SetRef(SerializedObject so, string propName, Object target)
    {
        if (target == null) return;
        var prop = so.FindProperty(propName);
        if (prop != null)
            prop.objectReferenceValue = target;
    }

    static GameObject CreateButton(Transform parent, string name, string label)
    {
        var btnGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(parent, false);

        // 텍스트 자식
        var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        var txtRT = txtGo.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        var tmp = txtGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btnGo;
    }

    static GameObject CreatePanel(Transform parent, string name, float width, float height, Color bgColor)
    {
        var panelGo = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(parent, false);
        var rt = panelGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = Vector2.zero;

        panelGo.GetComponent<Image>().color = bgColor;

        return panelGo;
    }

    static GameObject CreateTMPText(Transform parent, string name, string text, float fontSize,
        Color color, TextAlignmentOptions align, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.fontStyle = style;
        return go;
    }

    static void AddLayoutElement(GameObject go, float prefWidth, float prefHeight)
    {
        var le = go.AddComponent<LayoutElement>();
        if (prefWidth > 0) le.preferredWidth = prefWidth;
        if (prefHeight > 0) le.preferredHeight = prefHeight;
    }

    static void CreateHeaderCell(Transform parent, string text, float width)
    {
        var cellGo = new GameObject("Header_" + text, typeof(RectTransform),
            typeof(TextMeshProUGUI));
        cellGo.transform.SetParent(parent, false);

        var le = cellGo.AddComponent<LayoutElement>();
        le.preferredWidth = width;
        le.flexibleWidth = 0;

        var tmp = cellGo.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 18;
        tmp.color = new Color(0.8f, 0.8f, 0.5f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
    }

    static GameObject CreateScrollView(Transform parent, string name, float height)
    {
        // ScrollView 루트
        var svGo = new GameObject(name, typeof(RectTransform), typeof(ScrollRect));
        svGo.transform.SetParent(parent, false);
        var svLE = svGo.AddComponent<LayoutElement>();
        svLE.flexibleHeight = 1;
        svLE.preferredHeight = height;

        var svRT = svGo.GetComponent<RectTransform>();
        svRT.anchorMin = Vector2.zero;
        svRT.anchorMax = Vector2.one;

        // Viewport
        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image),
            typeof(Mask));
        viewport.transform.SetParent(svGo.transform, false);
        var vpRT = viewport.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero;
        vpRT.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.02f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        // Content
        var content = new GameObject("LBContent", typeof(RectTransform),
            typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.offsetMin = new Vector2(0, 0);
        contentRT.offsetMax = new Vector2(0, 0);

        var contentVLG = content.GetComponent<VerticalLayoutGroup>();
        contentVLG.spacing = 2;
        contentVLG.childAlignment = TextAnchor.UpperCenter;
        contentVLG.padding = new RectOffset(5, 5, 5, 5);
        contentVLG.childControlWidth = true;
        contentVLG.childControlHeight = false;
        contentVLG.childForceExpandWidth = true;
        contentVLG.childForceExpandHeight = false;

        var csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ScrollRect 설정
        var scrollRect = svGo.GetComponent<ScrollRect>();
        scrollRect.viewport = vpRT;
        scrollRect.content = contentRT;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 20f;

        return svGo;
    }

    static GameObject CreateTMPInputField(Transform parent, string name, float width, float height)
    {
        // TMP_InputField 생성
        var inputGo = new GameObject(name, typeof(RectTransform), typeof(Image),
            typeof(TMP_InputField));
        inputGo.transform.SetParent(parent, false);
        var inputRT = inputGo.GetComponent<RectTransform>();
        inputRT.sizeDelta = new Vector2(width, height);

        var inputImg = inputGo.GetComponent<Image>();
        inputImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        // TextArea (입력 영역)
        var textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        textArea.transform.SetParent(inputGo.transform, false);
        var taRT = textArea.GetComponent<RectTransform>();
        taRT.anchorMin = Vector2.zero;
        taRT.anchorMax = Vector2.one;
        taRT.offsetMin = new Vector2(10, 5);
        taRT.offsetMax = new Vector2(-10, -5);

        // Placeholder
        var placeholder = new GameObject("Placeholder", typeof(RectTransform),
            typeof(TextMeshProUGUI));
        placeholder.transform.SetParent(textArea.transform, false);
        var phRT = placeholder.GetComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero;
        phRT.anchorMax = Vector2.one;
        phRT.offsetMin = Vector2.zero;
        phRT.offsetMax = Vector2.zero;
        var phTMP = placeholder.GetComponent<TextMeshProUGUI>();
        phTMP.text = "닉네임 입력...";
        phTMP.fontSize = 20;
        phTMP.color = new Color(0.5f, 0.5f, 0.5f);
        phTMP.alignment = TextAlignmentOptions.MidlineLeft;
        phTMP.fontStyle = FontStyles.Italic;

        // Text (입력 텍스트)
        var inputText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        inputText.transform.SetParent(textArea.transform, false);
        var itRT = inputText.GetComponent<RectTransform>();
        itRT.anchorMin = Vector2.zero;
        itRT.anchorMax = Vector2.one;
        itRT.offsetMin = Vector2.zero;
        itRT.offsetMax = Vector2.zero;
        var itTMP = inputText.GetComponent<TextMeshProUGUI>();
        itTMP.fontSize = 22;
        itTMP.color = Color.white;
        itTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // TMP_InputField 컴포넌트 연결
        var inputField = inputGo.GetComponent<TMP_InputField>();
        inputField.textViewport = taRT;
        inputField.textComponent = itTMP;
        inputField.placeholder = phTMP;
        inputField.characterLimit = 12;
        inputField.contentType = TMP_InputField.ContentType.Standard;
        inputField.lineType = TMP_InputField.LineType.SingleLine;

        return inputGo;
    }
}
