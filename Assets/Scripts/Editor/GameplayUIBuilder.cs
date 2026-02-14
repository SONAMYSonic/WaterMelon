using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class GameplayUIBuilder : EditorWindow
{
    [MenuItem("Tools/Build Gameplay UI (Restart + Refresh + Size Fix)")]
    public static void Build()
    {
        BuildUI();
        ApplyAssetChanges();
        Debug.Log("[GameplayUIBuilder] === 모든 작업 완료! ===");
    }

    static void BuildUI()
    {
        // === 1. GameCanvas 찾기 ===
        var canvas = GameObject.Find("GameCanvas");
        if (canvas == null)
        {
            Debug.LogError("[GameplayUIBuilder] GameCanvas를 찾을 수 없습니다!");
            return;
        }
        var canvasT = canvas.transform;

        // === 2. InGameRestartButton 생성 (좌하단 버튼줄 맨 왼쪽) ===
        // 기존: SettingsButton pos(224.5,200), LeaderboardButton pos(383.5,200)
        // 새: InGameRestartButton pos(65,200), RefreshButton pos(542.5,200)

        // 기존 버튼이 있으면 삭제
        var existRestart = canvasT.Find("InGameRestartButton");
        if (existRestart != null) Object.DestroyImmediate(existRestart.gameObject);
        var existRefresh = canvasT.Find("RefreshButton");
        if (existRefresh != null) Object.DestroyImmediate(existRefresh.gameObject);
        var existConfirmRestart = canvasT.Find("ConfirmRestartPanel");
        if (existConfirmRestart != null) Object.DestroyImmediate(existConfirmRestart.gameObject);
        var existConfirmRefresh = canvasT.Find("ConfirmRefreshPanel");
        if (existConfirmRefresh != null) Object.DestroyImmediate(existConfirmRefresh.gameObject);

        // InGameRestartButton
        var restartBtn = CreateButton(canvasT, "InGameRestartButton", "재시작");
        var restartBtnRT = restartBtn.GetComponent<RectTransform>();
        restartBtnRT.anchorMin = new Vector2(0, 0);
        restartBtnRT.anchorMax = new Vector2(0, 0);
        restartBtnRT.pivot = new Vector2(0.5f, 0.5f);
        restartBtnRT.sizeDelta = new Vector2(149, 62);
        restartBtnRT.anchoredPosition = new Vector2(65f, 200f);

        var restartBtnImg = restartBtn.GetComponent<Image>();
        restartBtnImg.color = new Color(0.8f, 0.3f, 0.3f, 1f); // 빨간색
        var restartBtnText = restartBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (restartBtnText != null)
        {
            restartBtnText.fontSize = 26;
            restartBtnText.color = Color.white;
            restartBtnText.fontStyle = FontStyles.Bold;
        }

        // === 3. RefreshButton (새로고침) ===
        var refreshBtn = CreateButton(canvasT, "RefreshButton", "새로고침");
        var refreshBtnRT = refreshBtn.GetComponent<RectTransform>();
        refreshBtnRT.anchorMin = new Vector2(0, 0);
        refreshBtnRT.anchorMax = new Vector2(0, 0);
        refreshBtnRT.pivot = new Vector2(0.5f, 0.5f);
        refreshBtnRT.sizeDelta = new Vector2(149, 62);
        refreshBtnRT.anchoredPosition = new Vector2(542.5f, 200f);

        var refreshBtnImg = refreshBtn.GetComponent<Image>();
        refreshBtnImg.color = new Color(0.2f, 0.7f, 0.4f, 1f); // 초록색
        var refreshBtnText = refreshBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (refreshBtnText != null)
        {
            refreshBtnText.fontSize = 24;
            refreshBtnText.color = Color.white;
            refreshBtnText.fontStyle = FontStyles.Bold;
        }

        // === 4. ConfirmRestartPanel ===
        var confirmRestartPanel = CreateConfirmPanel(canvasT, "ConfirmRestartPanel",
            "재시작하시겠습니까?", 400, 200);

        // === 5. ConfirmRefreshPanel ===
        var confirmRefreshPanel = CreateConfirmPanel(canvasT, "ConfirmRefreshPanel",
            "캐릭터를 새로고침하면\n게임이 초기화됩니다.\n계속하시겠습니까?", 450, 250);

        // === 6. 렌더 순서: 확인 패널들을 GameOverPanel/LeaderboardPanel 앞에 배치 ===
        // GameOverPanel과 LeaderboardPanel이 가장 위에 와야 함
        var goPanel = canvasT.Find("GameOverPanel");
        var lbPanel = canvasT.Find("LeaderboardPanel");
        if (goPanel != null) goPanel.SetAsLastSibling();
        if (lbPanel != null) lbPanel.SetAsLastSibling();

        // === 7. UIManager에 참조 연결 ===
        var uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            var so = new SerializedObject(uiManager);

            // 인게임 재시작
            SetRef(so, "inGameRestartButton", restartBtn);
            SetRef(so, "confirmRestartPanel", confirmRestartPanel);
            SetRef(so, "confirmRestartYesBtn",
                confirmRestartPanel.transform.Find("ButtonRow/YesBtn")?.gameObject);
            SetRef(so, "confirmRestartNoBtn",
                confirmRestartPanel.transform.Find("ButtonRow/NoBtn")?.gameObject);

            // 캐릭터 새로고침
            SetRef(so, "refreshCharactersButton", refreshBtn);
            SetRef(so, "confirmRefreshPanel", confirmRefreshPanel);
            SetRef(so, "confirmRefreshYesBtn",
                confirmRefreshPanel.transform.Find("ButtonRow/YesBtn")?.gameObject);
            SetRef(so, "confirmRefreshNoBtn",
                confirmRefreshPanel.transform.Find("ButtonRow/NoBtn")?.gameObject);

            so.ApplyModifiedProperties();
            Debug.Log("[GameplayUIBuilder] UIManager 참조 연결 완료");
        }
        else
        {
            Debug.LogWarning("[GameplayUIBuilder] UIManager를 찾을 수 없습니다!");
        }

        Debug.Log("[GameplayUIBuilder] UI 빌드 완료");
    }

    static void ApplyAssetChanges()
    {
        // === A. CharacterDatabase.asset — minRadius, maxRadius ×1.75 ===
        var dbAssets = AssetDatabase.FindAssets("t:CharacterDatabase");
        if (dbAssets.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(dbAssets[0]);
            var db = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(path);
            if (db != null)
            {
                var dbSO = new SerializedObject(db);
                var minProp = dbSO.FindProperty("minRadius");
                var maxProp = dbSO.FindProperty("maxRadius");

                if (minProp != null && maxProp != null)
                {
                    float oldMin = minProp.floatValue;
                    float oldMax = maxProp.floatValue;
                    minProp.floatValue = 0.35f;
                    maxProp.floatValue = 1.96f;
                    dbSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(db);
                    Debug.Log($"[GameplayUIBuilder] CharacterDatabase: minRadius {oldMin}→0.35, maxRadius {oldMax}→1.96");
                }
                else
                {
                    Debug.LogWarning("[GameplayUIBuilder] CharacterDatabase에서 minRadius/maxRadius 프로퍼티를 찾을 수 없습니다.");
                }
            }
        }
        else
        {
            Debug.LogWarning("[GameplayUIBuilder] CharacterDatabase 에셋을 찾을 수 없습니다.");
        }

        // === B. CharacterPhysics.physicsMaterial2D — bounciness 0.1 ===
        var physMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(
            "Assets/Prefabs/CharacterPhysics.physicsMaterial2D");
        if (physMat != null)
        {
            float oldBounce = physMat.bounciness;
            physMat.bounciness = 0.1f;
            EditorUtility.SetDirty(physMat);
            Debug.Log($"[GameplayUIBuilder] PhysicsMaterial2D: bounciness {oldBounce}→0.1");
        }
        else
        {
            Debug.LogWarning("[GameplayUIBuilder] CharacterPhysics.physicsMaterial2D를 찾을 수 없습니다.");
        }

        // === C. GameManager — dropMinX, dropMaxX 조정 ===
        var gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            var gmSO = new SerializedObject(gm);
            var minXProp = gmSO.FindProperty("dropMinX");
            var maxXProp = gmSO.FindProperty("dropMaxX");

            if (minXProp != null && maxXProp != null)
            {
                float oldMinX = minXProp.floatValue;
                float oldMaxX = maxXProp.floatValue;
                minXProp.floatValue = -2.0f;
                maxXProp.floatValue = 2.0f;
                gmSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(gm);
                Debug.Log($"[GameplayUIBuilder] GameManager: dropMinX {oldMinX}→-2.0, dropMaxX {oldMaxX}→2.0");
            }
        }
        else
        {
            Debug.LogWarning("[GameplayUIBuilder] GameManager를 찾을 수 없습니다.");
        }

        AssetDatabase.SaveAssets();
    }

    // ===== 확인 패널 생성 헬퍼 =====
    static GameObject CreateConfirmPanel(Transform parent, string name, string message,
        float width, float height)
    {
        // 반투명 배경 오버레이 + 패널
        var panelGo = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(parent, false);
        var panelRT = panelGo.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(width, height);
        panelRT.anchoredPosition = Vector2.zero;
        panelGo.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        // 메시지 텍스트
        var msgGo = new GameObject("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
        msgGo.transform.SetParent(panelGo.transform, false);
        var msgRT = msgGo.GetComponent<RectTransform>();
        msgRT.anchorMin = new Vector2(0.5f, 1f);
        msgRT.anchorMax = new Vector2(0.5f, 1f);
        msgRT.pivot = new Vector2(0.5f, 1f);
        msgRT.anchoredPosition = new Vector2(0f, -20f);
        msgRT.sizeDelta = new Vector2(width - 40f, height - 90f);
        var msgTMP = msgGo.GetComponent<TextMeshProUGUI>();
        msgTMP.text = message;
        msgTMP.fontSize = 24;
        msgTMP.color = Color.white;
        msgTMP.alignment = TextAlignmentOptions.Center;

        // 버튼 행
        var btnRow = new GameObject("ButtonRow", typeof(RectTransform),
            typeof(HorizontalLayoutGroup));
        btnRow.transform.SetParent(panelGo.transform, false);
        var btnRowRT = btnRow.GetComponent<RectTransform>();
        btnRowRT.anchorMin = new Vector2(0.5f, 0f);
        btnRowRT.anchorMax = new Vector2(0.5f, 0f);
        btnRowRT.pivot = new Vector2(0.5f, 0f);
        btnRowRT.anchoredPosition = new Vector2(0f, 15f);
        btnRowRT.sizeDelta = new Vector2(300f, 55f);

        var hlg = btnRow.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 30;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // 예 버튼
        var yesBtn = CreateButton(btnRow.transform, "YesBtn", "예");
        var yesBtnRT = yesBtn.GetComponent<RectTransform>();
        yesBtnRT.sizeDelta = new Vector2(120, 50);
        var yesBtnImg = yesBtn.GetComponent<Image>();
        yesBtnImg.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        var yesBtnText = yesBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (yesBtnText != null)
        {
            yesBtnText.fontSize = 24;
            yesBtnText.color = Color.white;
            yesBtnText.fontStyle = FontStyles.Bold;
        }

        // 아니오 버튼
        var noBtn = CreateButton(btnRow.transform, "NoBtn", "아니오");
        var noBtnRT = noBtn.GetComponent<RectTransform>();
        noBtnRT.sizeDelta = new Vector2(120, 50);
        var noBtnImg = noBtn.GetComponent<Image>();
        noBtnImg.color = new Color(0.6f, 0.3f, 0.3f, 1f);
        var noBtnText = noBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (noBtnText != null)
        {
            noBtnText.fontSize = 24;
            noBtnText.color = Color.white;
            noBtnText.fontStyle = FontStyles.Bold;
        }

        panelGo.SetActive(false);
        return panelGo;
    }

    // ===== 공통 헬퍼 =====
    static void SetRef(SerializedObject so, string propName, Object target)
    {
        if (target == null)
        {
            Debug.LogWarning($"[GameplayUIBuilder] '{propName}' 대상이 null입니다.");
            return;
        }
        var prop = so.FindProperty(propName);
        if (prop != null)
            prop.objectReferenceValue = target;
        else
            Debug.LogWarning($"[GameplayUIBuilder] SerializedProperty '{propName}'를 찾을 수 없습니다.");
    }

    static GameObject CreateButton(Transform parent, string name, string label)
    {
        var btnGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(parent, false);

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
}
