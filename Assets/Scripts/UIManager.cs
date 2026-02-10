using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private CharacterDatabase characterDB;

    // 자동 생성되는 UI 요소들
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI highScoreText;
    private Image nextCharacterImage;
    private TextMeshProUGUI nextCharacterName;
    private Transform evolutionChartParent;
    private GameObject gameOverPanel;
    private TextMeshProUGUI finalScoreText;

    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        BuildUI();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnHighScoreChanged += UpdateHighScore;
            GameManager.Instance.OnNextCharacterChanged += UpdateNextCharacter;
            GameManager.Instance.OnGameOver += ShowGameOver;
        }
        UpdateScore(0);
        UpdateHighScore(PlayerPrefs.GetInt("HighScore", 0));
        BuildEvolutionChart();
    }

    private void BuildUI()
    {
        // ===== 좌측: 점수 패널 =====
        var leftPanel = MakePanel("LeftScorePanel", canvas.transform,
            TextAnchor.UpperLeft, new Vector2(20, -20), new Vector2(200, 170));

        scoreText = MakeLabel(leftPanel.transform, "ScoreLabel", "SCORE\n0", 28,
            TextAlignmentOptions.TopLeft, new Vector2(15, -15), new Vector2(170, 80));

        highScoreText = MakeLabel(leftPanel.transform, "HighScoreLabel", "BEST\n0", 22,
            TextAlignmentOptions.TopLeft, new Vector2(15, -95), new Vector2(170, 60));

        // ===== 우측 상단: 다음 캐릭터 =====
        var nextPanel = MakePanel("NextCharPanel", canvas.transform,
            TextAnchor.UpperRight, new Vector2(-20, -20), new Vector2(160, 150));

        MakeLabel(nextPanel.transform, "NextTitle", "NEXT", 18,
            TextAlignmentOptions.Center, new Vector2(0, -8), new Vector2(140, 25));

        // 다음 캐릭터 이미지
        var imgGo = new GameObject("NextCharImage", typeof(RectTransform), typeof(Image));
        imgGo.transform.SetParent(nextPanel.transform, false);
        var imgRT = imgGo.GetComponent<RectTransform>();
        SetAnchors(imgRT, 0.5f, 0.5f);
        imgRT.anchoredPosition = new Vector2(0, -5);
        imgRT.sizeDelta = new Vector2(70, 70);
        nextCharacterImage = imgGo.GetComponent<Image>();
        nextCharacterImage.preserveAspect = true;

        nextCharacterName = MakeLabel(nextPanel.transform, "NextName", "", 14,
            TextAlignmentOptions.Center, new Vector2(0, -118), new Vector2(140, 22));
    }

    private void BuildEvolutionChart()
    {
        // ===== 우측: 진화 차트 =====
        var evoPanel = MakePanel("EvolutionPanel", canvas.transform,
            TextAnchor.UpperRight, new Vector2(-20, -180), new Vector2(160, 620));

        MakeLabel(evoPanel.transform, "EvoTitle", "EVOLUTION", 16,
            TextAlignmentOptions.Center, new Vector2(0, -5), new Vector2(140, 22));

        // 스크롤 콘텐츠
        var contentGo = new GameObject("ChartContent", typeof(RectTransform));
        contentGo.transform.SetParent(evoPanel.transform, false);
        var contentRT = contentGo.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 0);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.offsetMin = new Vector2(5, 5);
        contentRT.offsetMax = new Vector2(-5, -30);

        var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        evolutionChartParent = contentGo.transform;

        if (characterDB == null) return;
        for (int i = 0; i <= characterDB.MaxLevel; i++)
        {
            CharacterData data = characterDB.GetCharacter(i);
            if (data == null) continue;
            CreateEvolutionRow(contentGo.transform, data);
        }
    }

    private void CreateEvolutionRow(Transform parent, CharacterData data)
    {
        var row = new GameObject($"Evo_Lv{data.level}", typeof(RectTransform),
            typeof(HorizontalLayoutGroup));
        row.transform.SetParent(parent, false);
        var rowRT = row.GetComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(0, 42);

        var hlg = row.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.padding = new RectOffset(5, 5, 2, 2);
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        // 아이콘
        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(row.transform, false);
        iconGo.GetComponent<RectTransform>().sizeDelta = new Vector2(36, 36);
        var iconImg = iconGo.GetComponent<Image>();
        iconImg.sprite = data.characterSprite;
        iconImg.preserveAspect = true;

        // 레벨 텍스트
        var txtGo = new GameObject("Lvl", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(row.transform, false);
        txtGo.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 36);
        var tmp = txtGo.GetComponent<TextMeshProUGUI>();
        tmp.text = $"Lv.{data.level}";
        tmp.fontSize = 16;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;
    }

    // ===== 게임오버 =====
    private void ShowGameOver()
    {
        if (gameOverPanel != null) { gameOverPanel.SetActive(true); return; }

        gameOverPanel = MakePanel("GameOverPanel", canvas.transform,
            TextAnchor.MiddleCenter, Vector2.zero, new Vector2(460, 280));
        gameOverPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);

        MakeLabel(gameOverPanel.transform, "GOTitle", "GAME OVER", 44,
            TextAlignmentOptions.Center, new Vector2(0, 60), new Vector2(400, 55));

        finalScoreText = MakeLabel(gameOverPanel.transform, "GOScore",
            $"FINAL SCORE: {GameManager.Instance.Score:N0}", 28,
            TextAlignmentOptions.Center, new Vector2(0, 0), new Vector2(400, 40));

        // 재시작 버튼
        var btnGo = new GameObject("RestartBtn", typeof(RectTransform),
            typeof(Image), typeof(Button));
        btnGo.transform.SetParent(gameOverPanel.transform, false);
        var btnRT = btnGo.GetComponent<RectTransform>();
        SetAnchors(btnRT, 0.5f, 0.5f);
        btnRT.anchoredPosition = new Vector2(0, -70);
        btnRT.sizeDelta = new Vector2(180, 48);
        btnGo.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f);
        btnGo.GetComponent<Button>().onClick.AddListener(() =>
            GameManager.Instance.RestartGame());

        MakeLabel(btnGo.transform, "BtnTxt", "RESTART", 22,
            TextAlignmentOptions.Center, Vector2.zero, Vector2.zero, true);
    }

    // ===== 이벤트 핸들러 =====
    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"SCORE\n{score:N0}";
    }

    private void UpdateHighScore(int score)
    {
        if (highScoreText != null)
            highScoreText.text = $"BEST\n{score:N0}";
    }

    private void UpdateNextCharacter(int level)
    {
        CharacterData data = characterDB.GetCharacter(level);
        if (data == null) return;
        if (nextCharacterImage != null)
            nextCharacterImage.sprite = data.characterSprite;
        if (nextCharacterName != null)
            nextCharacterName.text = data.characterName;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnHighScoreChanged -= UpdateHighScore;
            GameManager.Instance.OnNextCharacterChanged -= UpdateNextCharacter;
            GameManager.Instance.OnGameOver -= ShowGameOver;
        }
    }

    // ===== 헬퍼 =====
    private GameObject MakePanel(string name, Transform parent,
        TextAnchor corner, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();

        switch (corner)
        {
            case TextAnchor.UpperLeft:
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
                break;
            case TextAnchor.UpperRight:
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1, 1);
                break;
            case TextAnchor.MiddleCenter:
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                break;
        }

        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.18f, 0.88f);
        return go;
    }

    private TextMeshProUGUI MakeLabel(Transform parent, string name, string text,
        float fontSize, TextAlignmentOptions align, Vector2 pos, Vector2 size,
        bool stretch = false)
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
            SetAnchors(rt, 0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = align;
        return tmp;
    }

    private void SetAnchors(RectTransform rt, float x, float y)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(x, y);
    }
}
