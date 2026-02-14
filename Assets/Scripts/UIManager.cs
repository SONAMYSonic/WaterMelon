using System.Collections;
using System.Collections.Generic;
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

    [Header("진화 차트")]
    [SerializeField] private Transform evolutionChartContent;

    [Header("진화 차트 행 프리팹 (비워두면 자동 생성)")]
    [SerializeField] private GameObject evolutionRowPrefab;

    [Header("게임오버 UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;

    [Header("게임오버 - 닉네임 입력")]
    [SerializeField] private GameObject nicknameGroup;
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button submitScoreButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private TextMeshProUGUI submittedText;

    [Header("설정 UI")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button settingsCloseButton;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider voiceSlider;

    [Header("리더보드 UI")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button leaderboardCloseButton;
    [SerializeField] private Button leaderboardRefreshButton;
    [SerializeField] private Transform leaderboardContent;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private GameObject leaderboardRowPrefab;

    [Header("인게임 재시작")]
    [SerializeField] private Button inGameRestartButton;
    [SerializeField] private GameObject confirmRestartPanel;
    [SerializeField] private Button confirmRestartYesBtn;
    [SerializeField] private Button confirmRestartNoBtn;

    [Header("캐릭터 새로고침")]
    [SerializeField] private Button refreshCharactersButton;
    [SerializeField] private GameObject confirmRefreshPanel;
    [SerializeField] private Button confirmRefreshYesBtn;
    [SerializeField] private Button confirmRefreshNoBtn;

    [Header("MAX 합체 축하")]
    [SerializeField] private GameObject maxCelebrateObject;

    private Coroutine cooldownCoroutine;

    private void Start()
    {
        // 게임오버 패널 초기 비활성화
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // MAX 합체 축하 오브젝트 초기 비활성화
        if (maxCelebrateObject != null)
            maxCelebrateObject.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnHighScoreChanged += UpdateHighScore;
            GameManager.Instance.OnNextCharacterChanged += UpdateNextCharacter;
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.OnCharactersSelected += BuildEvolutionChart;
            GameManager.Instance.OnMaxMerge += ShowMaxMergeCelebration;
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(() =>
            {
                PlayUIClick();
                GameManager.Instance.RestartGame();
            });

        // 설정 패널 초기화
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (settingsCloseButton != null)
            settingsCloseButton.onClick.AddListener(CloseSettings);

        // 슬라이더 초기값 및 콜백 연결
        if (AudioManager.Instance != null)
        {
            if (bgmSlider != null)
            {
                bgmSlider.value = AudioManager.Instance.BGMVolume;
                bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);
            }
            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.Instance.SFXVolume;
                sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
            }
            if (voiceSlider != null)
            {
                voiceSlider.value = AudioManager.Instance.VoiceVolume;
                voiceSlider.onValueChanged.AddListener(AudioManager.Instance.SetVoiceVolume);
            }
        }

        // 리더보드 패널 초기화
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);

        if (leaderboardButton != null)
            leaderboardButton.onClick.AddListener(OpenLeaderboard);

        if (leaderboardCloseButton != null)
            leaderboardCloseButton.onClick.AddListener(CloseLeaderboard);

        if (leaderboardRefreshButton != null)
            leaderboardRefreshButton.onClick.AddListener(() =>
            {
                PlayUIClick();
                RefreshLeaderboard();
            });

        // 닉네임 관련 버튼 연결
        if (submitScoreButton != null)
            submitScoreButton.onClick.AddListener(SubmitScore);

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipSubmit);

        // 닉네임 입력 초기값 (마지막 사용 닉네임)
        if (nicknameInput != null)
        {
            nicknameInput.characterLimit = 12;
            nicknameInput.text = PlayerPrefs.GetString("LastNickname", "");
        }

        // 인게임 재시작 버튼
        if (confirmRestartPanel != null)
            confirmRestartPanel.SetActive(false);

        if (inGameRestartButton != null)
            inGameRestartButton.onClick.AddListener(ShowRestartConfirm);

        if (confirmRestartYesBtn != null)
            confirmRestartYesBtn.onClick.AddListener(ConfirmRestart);

        if (confirmRestartNoBtn != null)
            confirmRestartNoBtn.onClick.AddListener(CancelRestart);

        // 캐릭터 새로고침 버튼
        if (confirmRefreshPanel != null)
            confirmRefreshPanel.SetActive(false);

        if (refreshCharactersButton != null)
            refreshCharactersButton.onClick.AddListener(OnRefreshClicked);

        if (confirmRefreshYesBtn != null)
            confirmRefreshYesBtn.onClick.AddListener(ConfirmRefresh);

        if (confirmRefreshNoBtn != null)
            confirmRefreshNoBtn.onClick.AddListener(CancelRefresh);

        UpdateScore(0);
        UpdateHighScore(PlayerPrefs.GetInt("HighScore", 0));
    }

    // ===== 진화 차트 =====
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

        float imageSize = 110f;

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

    // ===== UI 효과음 헬퍼 =====
    private void PlayUIClick()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();
    }

    private void PlayUIClose()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClose();
    }

    // ===== 패널 상태 헬퍼 =====
    private bool IsAnyPanelOpen()
    {
        bool settingsOpen = settingsPanel != null && settingsPanel.activeSelf;
        bool leaderboardOpen = leaderboardPanel != null && leaderboardPanel.activeSelf;
        bool restartConfirmOpen = confirmRestartPanel != null && confirmRestartPanel.activeSelf;
        bool refreshConfirmOpen = confirmRefreshPanel != null && confirmRefreshPanel.activeSelf;
        return settingsOpen || leaderboardOpen || restartConfirmOpen || refreshConfirmOpen;
    }

    // ===== 설정 =====
    private void OpenSettings()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        PlayUIClick();

        // 다른 패널이 열려 있으면 먼저 닫기
        if (leaderboardPanel != null && leaderboardPanel.activeSelf)
            CloseLeaderboardPanel();
        if (confirmRestartPanel != null && confirmRestartPanel.activeSelf)
            confirmRestartPanel.SetActive(false);
        if (confirmRefreshPanel != null && confirmRefreshPanel.activeSelf)
            confirmRefreshPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(true);
    }

    private void CloseSettings()
    {
        PlayUIClose();
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        PlayerPrefs.Save();

        // 다른 패널이 열려 있지 않을 때만 게임 재개
        if (!IsAnyPanelOpen() && GameManager.Instance != null)
            GameManager.Instance.SetPaused(false);
    }

    // ===== 리더보드 =====
    private void OpenLeaderboard()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        PlayUIClick();

        // 다른 패널이 열려 있으면 먼저 닫기
        if (settingsPanel != null && settingsPanel.activeSelf)
            CloseSettingsPanel();
        if (confirmRestartPanel != null && confirmRestartPanel.activeSelf)
            confirmRestartPanel.SetActive(false);
        if (confirmRefreshPanel != null && confirmRefreshPanel.activeSelf)
            confirmRefreshPanel.SetActive(false);

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(true);
        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(true);
        RefreshLeaderboard();
    }

    private void CloseLeaderboard()
    {
        PlayUIClose();
        CloseLeaderboardPanel();

        // 다른 패널이 열려 있지 않을 때만 게임 재개
        if (!IsAnyPanelOpen() && GameManager.Instance != null)
            GameManager.Instance.SetPaused(false);
    }

    // 패널 UI만 닫기 (게임 재개 판단 없이)
    private void CloseSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        PlayerPrefs.Save();
    }

    private void CloseLeaderboardPanel()
    {
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);

        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }
    }

    private void RefreshLeaderboard()
    {
        if (LeaderboardManager.Instance == null) return;

        if (!LeaderboardManager.Instance.CanFetch)
        {
            // 쿨다운 중이면 캐시 표시 + 쿨다운 타이머
            PopulateLeaderboardUI(LeaderboardManager.Instance.CachedEntries);
            StartCooldownTimer();
            return;
        }

        // 로딩 중 표시
        if (cooldownText != null)
            cooldownText.text = "불러오는 중...";

        LeaderboardManager.Instance.FetchLeaderboard((entries) =>
        {
            PopulateLeaderboardUI(entries);
            StartCooldownTimer();
        });
    }

    private void PopulateLeaderboardUI(List<LeaderboardEntry> entries)
    {
        if (leaderboardContent == null) return;

        // 기존 행 제거
        for (int i = leaderboardContent.childCount - 1; i >= 0; i--)
            Destroy(leaderboardContent.GetChild(i).gameObject);

        if (entries == null || entries.Count == 0)
        {
            CreateLeaderboardRow(leaderboardContent, 0, "데이터 없음", "");
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            string playerName = entries[i].player_name;
            string score = entries[i].score.ToString("N0");
            CreateLeaderboardRow(leaderboardContent, i + 1, playerName, score);
        }
    }

    private void CreateLeaderboardRow(Transform parent, int rank, string playerName, string score)
    {
        if (leaderboardRowPrefab == null) return;

        var rowGo = Instantiate(leaderboardRowPrefab, parent);
        rowGo.name = "LBRow";
        var lbRow = rowGo.GetComponent<LeaderboardRow>();
        if (lbRow != null)
            lbRow.Setup(rank, playerName, score);
    }

    private void StartCooldownTimer()
    {
        if (cooldownCoroutine != null)
            StopCoroutine(cooldownCoroutine);
        cooldownCoroutine = StartCoroutine(CooldownTimerCoroutine());
    }

    private IEnumerator CooldownTimerCoroutine()
    {
        while (LeaderboardManager.Instance != null && !LeaderboardManager.Instance.CanFetch)
        {
            UpdateCooldownDisplay();
            yield return new WaitForSecondsRealtime(0.5f);
        }
        UpdateCooldownDisplay();
        cooldownCoroutine = null;
    }

    private void UpdateCooldownDisplay()
    {
        if (cooldownText == null) return;

        if (LeaderboardManager.Instance == null)
        {
            cooldownText.text = "";
            return;
        }

        float remaining = LeaderboardManager.Instance.CooldownRemaining;
        if (remaining > 0f)
            cooldownText.text = $"새로고침 ({Mathf.CeilToInt(remaining)}초)";
        else
            cooldownText.text = "새로고침 가능";
    }

    // ===== 인게임 재시작 확인 =====
    private void ShowRestartConfirm()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        PlayUIClick();

        // 다른 패널 닫기
        if (settingsPanel != null && settingsPanel.activeSelf)
            CloseSettingsPanel();
        if (leaderboardPanel != null && leaderboardPanel.activeSelf)
            CloseLeaderboardPanel();
        if (confirmRefreshPanel != null && confirmRefreshPanel.activeSelf)
            confirmRefreshPanel.SetActive(false);

        if (confirmRestartPanel != null)
            confirmRestartPanel.SetActive(true);
        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(true);
    }

    private void ConfirmRestart()
    {
        PlayUIClick();
        if (confirmRestartPanel != null)
            confirmRestartPanel.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    private void CancelRestart()
    {
        PlayUIClose();
        if (confirmRestartPanel != null)
            confirmRestartPanel.SetActive(false);

        if (!IsAnyPanelOpen() && GameManager.Instance != null)
            GameManager.Instance.SetPaused(false);
    }

    // ===== 캐릭터 새로고침 =====
    private void OnRefreshClicked()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsGameOver) return;
        PlayUIClick();

        // 과일이 아직 없으면 조용히 리롤
        if (!GameManager.Instance.HasDroppedAny)
        {
            GameManager.Instance.RerollCharacters();
            return;
        }

        // 과일이 있으면 확인창 표시
        // 다른 패널 닫기
        if (settingsPanel != null && settingsPanel.activeSelf)
            CloseSettingsPanel();
        if (leaderboardPanel != null && leaderboardPanel.activeSelf)
            CloseLeaderboardPanel();
        if (confirmRestartPanel != null && confirmRestartPanel.activeSelf)
            confirmRestartPanel.SetActive(false);

        if (confirmRefreshPanel != null)
            confirmRefreshPanel.SetActive(true);
        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(true);
    }

    private void ConfirmRefresh()
    {
        PlayUIClick();
        if (confirmRefreshPanel != null)
            confirmRefreshPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPaused(false);
            GameManager.Instance.SoftRestart();
        }
    }

    private void CancelRefresh()
    {
        PlayUIClose();
        if (confirmRefreshPanel != null)
            confirmRefreshPanel.SetActive(false);

        if (!IsAnyPanelOpen() && GameManager.Instance != null)
            GameManager.Instance.SetPaused(false);
    }

    // ===== 게임오버 & 닉네임 =====
    private void ShowGameOver()
    {
        // 모든 확인창 닫기
        if (confirmRestartPanel != null && confirmRestartPanel.activeSelf)
            confirmRestartPanel.SetActive(false);
        if (confirmRefreshPanel != null && confirmRefreshPanel.activeSelf)
            confirmRefreshPanel.SetActive(false);

        // 설정창이 열려 있으면 닫기
        if (settingsPanel != null && settingsPanel.activeSelf)
            CloseSettings();

        // 리더보드 패널 열려 있으면 닫기
        if (leaderboardPanel != null && leaderboardPanel.activeSelf)
            CloseLeaderboard();

        // 인게임 버튼들 숨기기
        if (settingsButton != null)
            settingsButton.gameObject.SetActive(false);
        if (leaderboardButton != null)
            leaderboardButton.gameObject.SetActive(false);
        if (inGameRestartButton != null)
            inGameRestartButton.gameObject.SetActive(false);
        if (refreshCharactersButton != null)
            refreshCharactersButton.gameObject.SetActive(false);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null)
                finalScoreText.text = $"FINAL SCORE: {GameManager.Instance.Score:N0}";

            // Phase 1: 닉네임 입력 표시
            if (nicknameGroup != null)
                nicknameGroup.SetActive(true);
            if (skipButton != null)
                skipButton.gameObject.SetActive(true);
            if (submittedText != null)
                submittedText.gameObject.SetActive(false);
            if (restartButton != null)
                restartButton.gameObject.SetActive(false);

            // 마지막 사용한 닉네임 복원
            if (nicknameInput != null)
                nicknameInput.text = PlayerPrefs.GetString("LastNickname", "");
        }
    }

    private void SubmitScore()
    {
        PlayUIClick();
        if (LeaderboardManager.Instance == null || GameManager.Instance == null) return;

        string nickname = nicknameInput != null ? nicknameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(nickname))
            nickname = "익명";

        // 닉네임 저장
        PlayerPrefs.SetString("LastNickname", nickname);
        PlayerPrefs.Save();

        // 버튼 비활성화 (중복 클릭 방지)
        if (submitScoreButton != null)
            submitScoreButton.interactable = false;
        if (skipButton != null)
            skipButton.interactable = false;

        int score = GameManager.Instance.Score;
        LeaderboardManager.Instance.SubmitScore(nickname, score, (success) =>
        {
            TransitionToRestartPhase(success);
        });
    }

    private void SkipSubmit()
    {
        PlayUIClick();
        TransitionToRestartPhase(false);
    }

    private void TransitionToRestartPhase(bool wasSubmitted)
    {
        // Phase 1 숨기기
        if (nicknameGroup != null)
            nicknameGroup.SetActive(false);
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        // Phase 2 표시
        if (submittedText != null)
        {
            submittedText.gameObject.SetActive(true);
            submittedText.text = wasSubmitted ? "등록 완료!" : "";
        }
        if (restartButton != null)
            restartButton.gameObject.SetActive(true);
    }

    // ===== 점수 & 캐릭터 =====
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
    }

    // ===== MAX 합체 축하 =====
    private void ShowMaxMergeCelebration(Vector3 worldPos)
    {
        StartCoroutine(MaxMergeCelebrationRoutine());
    }

    private IEnumerator MaxMergeCelebrationRoutine()
    {
        if (maxCelebrateObject == null) yield break;

        maxCelebrateObject.SetActive(true);
        var rt = maxCelebrateObject.GetComponent<RectTransform>();
        var img = maxCelebrateObject.GetComponent<Image>();
        Color originalColor = img != null ? img.color : Color.white;

        rt.localScale = Vector3.zero;
        if (img != null) img.color = originalColor;

        // Phase 1: 튀어나오기 (0 → 1.2 스케일, 0.3초) — ease-out
        float duration1 = 0.3f;
        float elapsed = 0f;
        while (elapsed < duration1)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration1);
            float scale = 1.2f * Mathf.Sin(t * Mathf.PI * 0.5f);
            rt.localScale = Vector3.one * scale;
            yield return null;
        }

        // Phase 2: 바운스 복귀 (1.2 → 1.0, 0.15초)
        float duration2 = 0.15f;
        elapsed = 0f;
        while (elapsed < duration2)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration2);
            float scale = Mathf.Lerp(1.2f, 1f, t);
            rt.localScale = Vector3.one * scale;
            yield return null;
        }
        rt.localScale = Vector3.one;

        // Phase 3: 유지 (1.5초)
        yield return new WaitForSecondsRealtime(1.5f);

        // Phase 4: 페이드아웃 + 축소 (0.4초)
        float duration4 = 0.4f;
        elapsed = 0f;
        while (elapsed < duration4)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration4);
            rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.5f, t);
            if (img != null)
                img.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - t);
            yield return null;
        }

        // 원래 상태 복원 후 비활성화
        rt.localScale = Vector3.one;
        if (img != null) img.color = originalColor;
        maxCelebrateObject.SetActive(false);
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
            GameManager.Instance.OnMaxMerge -= ShowMaxMergeCelebration;
        }
    }
}
