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

    private const int ColumnsPerRow = 3;

    private void BuildEvolutionChart()
    {
        if (evolutionChartContent == null || characterDB == null) return;

        // 기존 차트 행 제거 (재시작 시 중복 방지)
        for (int i = evolutionChartContent.childCount - 1; i >= 0; i--)
            Destroy(evolutionChartContent.GetChild(i).gameObject);

        int totalLevels = characterDB.MaxLevel + 1; // 0 ~ MaxLevel
        int maxLevel = characterDB.MaxLevel;
        Transform currentRow = null;
        int colIndex = 0;

        for (int i = 0; i < totalLevels; i++)
        {
            // 새 행 시작 (3개마다)
            if (colIndex == 0)
            {
                currentRow = CreateRow(evolutionChartContent, i / ColumnsPerRow);
            }

            CharacterData data = characterDB.GetCharacter(i);
            if (data == null) continue;

            CreateEvolutionCell(currentRow, data, i, maxLevel);

            colIndex++;
            if (colIndex >= ColumnsPerRow)
                colIndex = 0;
        }
    }

    private Transform CreateRow(Transform parent, int rowIndex)
    {
        var rowGo = new GameObject($"Row_{rowIndex}", typeof(RectTransform),
            typeof(HorizontalLayoutGroup));
        rowGo.transform.SetParent(parent, false);

        var hlg = rowGo.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.padding = new RectOffset(5, 5, 5, 5);
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // LayoutElement으로 행 높이 유연하게
        var le = rowGo.AddComponent<LayoutElement>();
        le.minHeight = 80;
        le.flexibleHeight = 1;

        return rowGo.transform;
    }

    private void CreateEvolutionCell(Transform parent, CharacterData data, int level, int maxLevel)
    {
        if (evolutionRowPrefab != null)
        {
            var cell = Instantiate(evolutionRowPrefab, parent);
            cell.name = $"Evo_Lv{level}";
            var entry = cell.GetComponent<EvolutionEntry>();
            if (entry != null)
            {
                entry.Setup(data, level, maxLevel);
            }
            return;
        }

        // 프리팹이 없으면 코드로 생성 (세로 배치: 이미지 위, 텍스트 아래)
        var cellGo = new GameObject($"Evo_Lv{level}", typeof(RectTransform),
            typeof(VerticalLayoutGroup));
        cellGo.transform.SetParent(parent, false);

        var vlg = cellGo.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 2;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.padding = new RectOffset(4, 4, 4, 4);
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        // 레벨에 따른 이미지 크기
        float t = maxLevel > 0 ? (float)level / maxLevel : 0f;
        float imageSize = Mathf.Lerp(50f, 110f, t);

        // 캐릭터 이미지
        var iconGo = new GameObject("CharImage", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(cellGo.transform, false);
        iconGo.GetComponent<RectTransform>().sizeDelta = new Vector2(imageSize, imageSize);
        var iconImg = iconGo.GetComponent<Image>();
        iconImg.sprite = data.characterSprite;
        iconImg.preserveAspect = true;

        // 레벨 텍스트
        var txtGo = new GameObject("LevelText", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(cellGo.transform, false);
        txtGo.GetComponent<RectTransform>().sizeDelta = new Vector2(imageSize, 20);
        var tmp = txtGo.GetComponent<TextMeshProUGUI>();
        tmp.text = $"Lv.{level}";
        tmp.fontSize = 14;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
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
