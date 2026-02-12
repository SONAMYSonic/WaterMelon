using UnityEngine;
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
        if (a.Level >= characterDB.MaxLevel) return;

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
    }

    private void SpawnMergeEffect(Vector3 position, CharacterData data, int level)
    {
        if (mergeEffectPrefab == null) return;
        GameObject fx = Instantiate(mergeEffectPrefab, position, Quaternion.identity);
        MergeEffect effect = fx.GetComponent<MergeEffect>();
        if (effect != null)
            effect.Play(position, data.mergeEffectColor, characterDB.GetRadius(level) * 2f);
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
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameOver();
        OnGameOver?.Invoke();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
