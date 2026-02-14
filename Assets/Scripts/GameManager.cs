using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public CharacterDatabase characterDB;
    public Transform dropPoint;
    public Transform containerParent;
    public GameObject characterPrefab;
    public GameObject mergeEffectPrefab;

    [Header("Game Settings")]
    public float dropCooldown = 0.5f;
    public float gameOverLineY = 4.0f;
    public float gameOverCheckDelay = 2.0f;

    [Header("Drop Area")]
    public float dropMinX = -2.2f;
    public float dropMaxX = 2.2f;
    public float dropY = 4.5f;

    // 상태
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }
    public bool HasDroppedAny { get; private set; }

    private Character currentCharacter;
    private int nextCharacterLevel;
    private bool canDrop = true;

    // Input System
    private Mouse mouse;
    private Touchscreen touchscreen;

    // 이벤트
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnHighScoreChanged;
    public System.Action<int> OnNextCharacterChanged;
    public System.Action OnGameOver;
    public System.Action<Vector3> OnMaxMerge;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        HighScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    // 이벤트: 캐릭터 선택 완료 시 (UIManager가 구독)
    public System.Action OnCharactersSelected;

    private void Start()
    {
        mouse = Mouse.current;
        touchscreen = Touchscreen.current;

        // 28명 중 랜덤 11명 선택
        characterDB.SelectRandomCharacters();
        OnCharactersSelected?.Invoke();

        OnHighScoreChanged?.Invoke(HighScore);
        PrepareNextDrop();
        SpawnCurrentCharacter();
    }

    private void Update()
    {
        if (IsGameOver) return;
        if (IsPaused) return;
        HandleInput();
        UpdateCurrentCharacterPosition();
    }

    private Vector2 GetPointerScreenPosition()
    {
        if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
            return touchscreen.primaryTouch.position.ReadValue();
        if (mouse != null)
            return mouse.position.ReadValue();
        return Vector2.zero;
    }

    private bool WasPointerPressedThisFrame()
    {
        if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            return true;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            return true;
        return false;
    }

    private void HandleInput()
    {
        if (currentCharacter == null || !canDrop) return;

        // UI 위에 포인터가 있으면 게임 입력 무시
        if (IsPointerOverUI()) return;

        Vector2 screenPos = GetPointerScreenPosition();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, 0f));
        float clampedX = Mathf.Clamp(worldPos.x, dropMinX, dropMaxX);
        dropPoint.position = new Vector3(clampedX, dropY, 0f);

        if (WasPointerPressedThisFrame())
        {
            DropCharacter();
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // 터치 입력
        if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
            return EventSystem.current.IsPointerOverGameObject(
                touchscreen.primaryTouch.touchId.ReadValue());

        // 마우스 입력
        return EventSystem.current.IsPointerOverGameObject(-1);
    }

    private void UpdateCurrentCharacterPosition()
    {
        if (currentCharacter != null && !currentCharacter.IsDropped)
        {
            currentCharacter.transform.position = dropPoint.position;
        }
    }

    private void PrepareNextDrop()
    {
        nextCharacterLevel = Random.Range(0, Mathf.Min(5, characterDB.MaxDropLevel + 1));
        OnNextCharacterChanged?.Invoke(nextCharacterLevel);
    }

    private void SpawnCurrentCharacter()
    {
        int spawnLevel = nextCharacterLevel;
        CharacterData data = characterDB.GetCharacter(spawnLevel);
        if (data == null) return;

        GameObject go = Instantiate(characterPrefab, dropPoint.position,
            Quaternion.identity, containerParent);
        currentCharacter = go.GetComponent<Character>();
        currentCharacter.Initialize(data, spawnLevel, characterDB.GetRadius(spawnLevel));
        currentCharacter.SetKinematic(true);
        go.layer = LayerMask.NameToLayer("Ignore Raycast");

        PrepareNextDrop();
    }

    private void DropCharacter()
    {
        if (currentCharacter == null) return;

        HasDroppedAny = true;
        canDrop = false;
        currentCharacter.gameObject.layer = LayerMask.NameToLayer("Default");
        currentCharacter.Drop();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDrop();

        currentCharacter = null;
        Invoke(nameof(SpawnAfterDelay), dropCooldown);
    }

    private void SpawnAfterDelay()
    {
        canDrop = true;
        SpawnCurrentCharacter();
    }

    public void MergeCharacters(Character a, Character b)
    {
        if (a.HasMerged || b.HasMerged) return;
        if (a.Level != b.Level) return;
        if (a.Level >= characterDB.MaxLevel)
        {
            // MAX 합체 축하 이벤트
            a.HasMerged = true;
            b.HasMerged = true;
            Vector3 maxPos = (a.transform.position + b.transform.position) / 2f;

            AddScore(characterDB.maxMergeBonus);
            SpawnMergeEffect(maxPos, characterDB.GetCharacter(a.Level), a.Level, true);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCelebration();

            Destroy(a.gameObject);
            Destroy(b.gameObject);

            OnMaxMerge?.Invoke(maxPos);
            return;
        }

        a.HasMerged = true;
        b.HasMerged = true;

        int newLevel = a.Level + 1;
        CharacterData newData = characterDB.GetCharacter(newLevel);
        CharacterData oldData = characterDB.GetCharacter(a.Level);
        Vector3 mergePos = (a.transform.position + b.transform.position) / 2f;

        AddScore(characterDB.GetMergeScore(newLevel));
        SpawnMergeEffect(mergePos, newData, newLevel);

        if (AudioManager.Instance != null)
        {
            AudioClip sfx = newData.mergeSound != null
                ? newData.mergeSound : oldData.mergeSound;
            AudioManager.Instance.PlayMerge(sfx, newData.voiceClip);
        }

        Destroy(a.gameObject);
        Destroy(b.gameObject);

        GameObject go = Instantiate(characterPrefab, mergePos,
            Quaternion.identity, containerParent);
        Character newChar = go.GetComponent<Character>();
        newChar.Initialize(newData, newLevel, characterDB.GetRadius(newLevel));
        newChar.IsDropped = true;
        newChar.gameObject.layer = LayerMask.NameToLayer("Default");

        // 합체로 생성된 캐릭터의 초기 속도를 0으로 (튕김 방지)
        var newRb = go.GetComponent<Rigidbody2D>();
        if (newRb != null)
        {
            newRb.linearVelocity = Vector2.zero;
            newRb.angularVelocity = 0f;
        }
    }

    private void SpawnMergeEffect(Vector3 position, CharacterData data, int level, bool isBig = false)
    {
        if (mergeEffectPrefab == null) return;
        GameObject fx = Instantiate(mergeEffectPrefab, position, Quaternion.identity);
        MergeEffect effect = fx.GetComponent<MergeEffect>();
        if (effect != null)
        {
            float size = characterDB.GetRadius(level) * 2f;
            if (isBig)
                effect.PlayBig(position, data.mergeEffectColor, size);
            else
                effect.Play(position, data.mergeEffectColor, size);
        }
    }

    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt("HighScore", HighScore);
            OnHighScoreChanged?.Invoke(HighScore);
        }
    }

    public void TriggerGameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        IsPaused = false;
        Time.timeScale = 1f;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameOver();
        OnGameOver?.Invoke();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SetPaused(bool paused)
    {
        if (IsGameOver) return;
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
    }

    /// <summary>
    /// 캐릭터만 리롤 (과일이 아직 드롭되지 않았을 때)
    /// </summary>
    public void RerollCharacters()
    {
        // 현재 들고 있는 캐릭터 파괴
        if (currentCharacter != null)
        {
            Destroy(currentCharacter.gameObject);
            currentCharacter = null;
        }

        // 리롤
        characterDB.SelectRandomCharacters();
        OnCharactersSelected?.Invoke();

        // 다시 준비
        PrepareNextDrop();
        SpawnCurrentCharacter();
    }

    /// <summary>
    /// 소프트 리스타트: 모든 과일 파괴 + 점수 리셋 + 캐릭터 리롤 (씬 리로드 없음)
    /// </summary>
    public void SoftRestart()
    {
        // 대기 중인 스폰 취소
        CancelInvoke(nameof(SpawnAfterDelay));

        // 현재 들고 있는 캐릭터 파괴
        if (currentCharacter != null)
        {
            Destroy(currentCharacter.gameObject);
            currentCharacter = null;
        }

        // 컨테이너 안의 모든 캐릭터 파괴
        if (containerParent != null)
        {
            for (int i = containerParent.childCount - 1; i >= 0; i--)
            {
                Transform child = containerParent.GetChild(i);
                if (child.GetComponent<Character>() != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        // 점수 리셋
        Score = 0;
        OnScoreChanged?.Invoke(Score);

        // 상태 리셋
        HasDroppedAny = false;
        canDrop = true;
        IsGameOver = false;
        IsPaused = false;
        Time.timeScale = 1f;

        // 리롤 + 리스폰
        characterDB.SelectRandomCharacters();
        OnCharactersSelected?.Invoke();
        PrepareNextDrop();
        SpawnCurrentCharacter();
    }

    /// <summary>
    /// 디버그용: 캐릭터 없이 MAX 합체 축하 이벤트 발동
    /// </summary>
    public void TriggerMaxMergeCelebration()
    {
        Vector3 pos = Vector3.zero;
        CharacterData data = characterDB.GetCharacter(characterDB.MaxLevel);

        AddScore(characterDB.maxMergeBonus);
        SpawnMergeEffect(pos, data, characterDB.MaxLevel, true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCelebration();

        OnMaxMerge?.Invoke(pos);
    }

    private void OnDrawGizmos()
    {
        // 드롭 가능 영역
        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        float centerX = (dropMinX + dropMaxX) / 2f;
        float rangeX = dropMaxX - dropMinX;
        Gizmos.DrawCube(new Vector3(centerX, dropY, 0), new Vector3(rangeX, 0.1f, 0));
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(centerX, dropY, 0), new Vector3(rangeX, 0.1f, 0));
    }
}
