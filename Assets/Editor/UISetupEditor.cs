using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using TMPro;

public class UISetupEditor : MonoBehaviour
{
    [MenuItem("Tools/Setup Game UI")]
    static void SetupGameUI()
    {
        var canvas = GameObject.Find("GameCanvas");
        if (canvas == null)
        {
            Debug.LogError("GameCanvas not found!");
            return;
        }

        var canvasTransform = canvas.transform;

        // GameCanvas 기존 자식 제거
        for (int i = canvasTransform.childCount - 1; i >= 0; i--)
            DestroyImmediate(canvasTransform.GetChild(i).gameObject);

        // ===== 배경 전용 Canvas 생성 (Screen Space - Camera, 가장 뒤) =====
        // 기존 BackgroundCanvas 제거
        var oldBgCanvas = GameObject.Find("BackgroundCanvas");
        if (oldBgCanvas != null) DestroyImmediate(oldBgCanvas);

        // 배경 전용 카메라 생성
        var oldBgCam = GameObject.Find("BackgroundCamera");
        if (oldBgCam != null) DestroyImmediate(oldBgCam);

        var bgCamGo = new GameObject("BackgroundCamera");
        var bgCam = bgCamGo.AddComponent<Camera>();
        bgCam.clearFlags = CameraClearFlags.SolidColor;
        bgCam.backgroundColor = Color.black;
        bgCam.cullingMask = 0; // 아무것도 렌더링하지 않음
        bgCam.depth = -100; // 메인 카메라보다 먼저 렌더링
        bgCam.orthographic = true;
        // URP Additional Camera Data 설정
        var bgCamData = bgCamGo.AddComponent<UniversalAdditionalCameraData>();
        bgCamData.renderType = CameraRenderType.Base;

        var bgCanvasGo = new GameObject("BackgroundCanvas");
        var bgCanvas = bgCanvasGo.AddComponent<Canvas>();
        bgCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        bgCanvas.worldCamera = bgCam;
        bgCanvas.sortingOrder = -100;

        var bgScaler = bgCanvasGo.AddComponent<CanvasScaler>();
        bgScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        bgScaler.referenceResolution = new Vector2(1920, 1080);
        bgScaler.matchWidthOrHeight = 0.5f;

        // 배경 이미지
        var bgGo = new GameObject("BackgroundImage", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(bgCanvasGo.transform, false);
        var bgRT = bgGo.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        var bgImage = bgGo.GetComponent<Image>();
        bgImage.color = new Color(0.98f, 0.95f, 0.88f); // 기본 베이지색
        bgImage.raycastTarget = false;

        // 메인 카메라의 배경을 투명하게 설정
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.Depth;
        }

        // ===== 좌측: 점수 패널 =====
        var leftPanel = CreatePanel("LeftScorePanel", canvasTransform,
            new Vector2(0, 1), new Vector2(20, -20), new Vector2(200, 170),
            new Color(0.12f, 0.12f, 0.18f, 0.88f));

        var scoreText = CreateTMP("ScoreLabel", leftPanel.transform,
            new Vector2(0.5f, 1f), new Vector2(15, -15), new Vector2(170, 80),
            "SCORE\n0", 28, TextAlignmentOptions.TopLeft);

        var highScoreText = CreateTMP("HighScoreLabel", leftPanel.transform,
            new Vector2(0.5f, 1f), new Vector2(15, -95), new Vector2(170, 60),
            "BEST\n0", 22, TextAlignmentOptions.TopLeft);

        // ===== 우측 상단: 다음 캐릭터 =====
        var nextPanel = CreatePanel("NextCharPanel", canvasTransform,
            new Vector2(1, 1), new Vector2(-20, -20), new Vector2(160, 150),
            new Color(0.12f, 0.12f, 0.18f, 0.88f));

        CreateTMP("NextTitle", nextPanel.transform,
            new Vector2(0.5f, 1f), new Vector2(0, -8), new Vector2(140, 25),
            "NEXT", 18, TextAlignmentOptions.Center);

        var nextCharImage = CreateImage("NextCharImage", nextPanel.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0, -5), new Vector2(70, 70));
        nextCharImage.GetComponent<Image>().preserveAspect = true;

        var nextCharName = CreateTMP("NextCharName", nextPanel.transform,
            new Vector2(0.5f, 1f), new Vector2(0, -118), new Vector2(140, 22),
            "", 14, TextAlignmentOptions.Center);

        // ===== 우측: 진화 차트 =====
        var evoPanel = CreatePanel("EvolutionPanel", canvasTransform,
            new Vector2(1, 1), new Vector2(-20, -180), new Vector2(160, 620),
            new Color(0.12f, 0.12f, 0.18f, 0.88f));

        CreateTMP("EvoTitle", evoPanel.transform,
            new Vector2(0.5f, 1f), new Vector2(0, -5), new Vector2(140, 22),
            "EVOLUTION", 16, TextAlignmentOptions.Center);

        var chartContent = new GameObject("ChartContent", typeof(RectTransform));
        chartContent.transform.SetParent(evoPanel.transform, false);
        var contentRT = chartContent.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 0);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.offsetMin = new Vector2(5, 5);
        contentRT.offsetMax = new Vector2(-5, -30);

        var vlg = chartContent.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // ===== 게임오버 패널 =====
        var goPanel = CreatePanel("GameOverPanel", canvasTransform,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460, 280),
            new Color(0, 0, 0, 0.75f));

        CreateTMP("GOTitle", goPanel.transform,
            new Vector2(0.5f, 1f), new Vector2(0, -45), new Vector2(400, 55),
            "GAME OVER", 44, TextAlignmentOptions.Center);

        var finalScore = CreateTMP("GOScore", goPanel.transform,
            new Vector2(0.5f, 1f), new Vector2(0, -115), new Vector2(400, 40),
            "FINAL SCORE: 0", 28, TextAlignmentOptions.Center);

        var btnGo = new GameObject("RestartBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(goPanel.transform, false);
        var btnRT = btnGo.GetComponent<RectTransform>();
        btnRT.anchorMin = btnRT.anchorMax = btnRT.pivot = new Vector2(0.5f, 0.5f);
        btnRT.anchoredPosition = new Vector2(0, -70);
        btnRT.sizeDelta = new Vector2(180, 48);
        btnGo.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f);

        CreateTMP("BtnTxt", btnGo.transform,
            Vector2.zero, Vector2.zero, Vector2.zero,
            "RESTART", 22, TextAlignmentOptions.Center, true);

        goPanel.SetActive(false);

        // ===== UIManager에 참조 연결 =====
        var uiManager = canvas.GetComponent<UIManager>();
        if (uiManager != null)
        {
            var so = new SerializedObject(uiManager);
            so.FindProperty("backgroundImage").objectReferenceValue = bgImage;
            so.FindProperty("scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("highScoreText").objectReferenceValue = highScoreText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("nextCharacterImage").objectReferenceValue = nextCharImage.GetComponent<Image>();
            so.FindProperty("nextCharacterName").objectReferenceValue = nextCharName.GetComponent<TextMeshProUGUI>();
            so.FindProperty("evolutionChartContent").objectReferenceValue = chartContent.transform;
            so.FindProperty("gameOverPanel").objectReferenceValue = goPanel;
            so.FindProperty("finalScoreText").objectReferenceValue = finalScore.GetComponent<TextMeshProUGUI>();
            so.FindProperty("restartButton").objectReferenceValue = btnGo.GetComponent<Button>();
            so.ApplyModifiedProperties();
            Debug.Log("UIManager references connected successfully!");
        }

        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(bgCanvasGo);
        EditorUtility.SetDirty(bgCamGo);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("Game UI setup complete! Background canvas + UI panels created.");
    }

    static GameObject CreatePanel(string name, Transform parent, Vector2 anchor,
        Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        return go;
    }

    static GameObject CreateTMP(string name, Transform parent, Vector2 anchor,
        Vector2 pos, Vector2 size, string text, float fontSize,
        TextAlignmentOptions align, bool stretch = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();

        if (stretch)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
        else
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = align;
        return go;
    }

    static GameObject CreateImage(string name, Transform parent, Vector2 anchor,
        Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return go;
    }
}
