using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("배경")]
    [Tooltip("전체 화면 배경 이미지 (Inspector에서 스프라이트를 넣으면 배경으로 사용)")]
    [SerializeField] private Image backgroundImage;

    [Header("Database")]
    [SerializeField] private CharacterDatabase characterDB;

    [Header("점수 UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    [Header("다음 캐릭터 UI")]
    [SerializeField] private Image nextCharacterImage;
    [SerializeField] private TextMeshProUGUI nextCharacterName;

    [Header("진화 차트")]
    [SerializeField] private Transform evolutionChartContent;

    [Header("진화 차트 행 프리팹 (비워두면 자동 생성)")]
    [SerializeField] private GameObject evolutionRowPrefab;

    [Header("게임오버 UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;

    private void Start()
    {
        // 게임오버 패널 초기 비활성화
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnHighScoreChanged += UpdateHighScore;
            GameManager.Instance.OnNextCharacterChanged += UpdateNextCharacter;
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.OnCharactersSelected += BuildEvolutionChart;
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(() => GameManager.Instance.RestartGame());

        UpdateScore(0);
        UpdateHighScore(PlayerPrefs.GetInt("HighScore", 0));
    }

    private void BuildEvolutionChart()
    {
        if (evolutionChartContent == null || characterDB == null) return;

        // 기존 차트 행 제거 (재시작 시 중복 방지)
        for (int i = evolutionChartContent.childCount - 1; i >= 0; i--)
            Destroy(evolutionChartContent.GetChild(i).gameObject);

        for (int i = 0; i <= characterDB.MaxLevel; i++)
        {
            CharacterData data = characterDB.GetCharacter(i);
            if (data == null) continue;
            CreateEvolutionRow(evolutionChartContent, data, i);
        }
    }

    private void CreateEvolutionRow(Transform parent, CharacterData data, int level)
    {
        if (evolutionRowPrefab != null)
        {
            var row = Instantiate(evolutionRowPrefab, parent);
            row.name = $"Evo_Lv{level}";
            var icon = row.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
            {
                icon.sprite = data.characterSprite;
                icon.preserveAspect = true;
            }
            var lvl = row.transform.Find("Lvl")?.GetComponent<TextMeshProUGUI>();
            if (lvl != null)
                lvl.text = $"Lv.{level}";
            return;
        }

        // 프리팹이 없으면 기존 방식으로 코드 생성
        var rowGo = new GameObject($"Evo_Lv{level}", typeof(RectTransform),
            typeof(HorizontalLayoutGroup));
        rowGo.transform.SetParent(parent, false);
        rowGo.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 42);

        var hlg = rowGo.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.padding = new RectOffset(5, 5, 2, 2);
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(rowGo.transform, false);
        iconGo.GetComponent<RectTransform>().sizeDelta = new Vector2(36, 36);
        var iconImg = iconGo.GetComponent<Image>();
        iconImg.sprite = data.characterSprite;
        iconImg.preserveAspect = true;

        var txtGo = new GameObject("Lvl", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(rowGo.transform, false);
        txtGo.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 36);
        var tmp = txtGo.GetComponent<TextMeshProUGUI>();
        tmp.text = $"Lv.{level}";
        tmp.fontSize = 16;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null)
                finalScoreText.text = $"FINAL SCORE: {GameManager.Instance.Score:N0}";
            return;
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"{score:N0}";
    }

    private void UpdateHighScore(int score)
    {
        if (highScoreText != null)
            highScoreText.text = $"{score:N0}";
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
            GameManager.Instance.OnCharactersSelected -= BuildEvolutionChart;
        }
    }
}
