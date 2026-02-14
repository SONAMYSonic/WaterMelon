using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "WatermelonGame/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    [Tooltip("사용 가능한 전체 캐릭터 풀 (28명)")]
    public CharacterData[] allCharacters;

    [Tooltip("한 게임에 사용할 캐릭터 수 (레벨 수)")]
    public int levelCount = 11;

    [Header("크기 설정")]
    [Tooltip("레벨 0 캐릭터의 반지름")]
    public float minRadius = 0.2f;
    [Tooltip("최대 레벨 캐릭터의 반지름")]
    public float maxRadius = 0.8f;

    [Header("점수 설정")]
    [Tooltip("레벨 0 합체 시 점수")]
    public int baseScore = 10;
    [Tooltip("레벨당 점수 증가량")]
    public int scorePerLevel = 10;

    [Tooltip("최종 합체 보너스 점수")]
    public int maxMergeBonus = 500;

    // 런타임에 선택된 캐릭터 (레벨 순)
    private CharacterData[] selectedCharacters;

    public int MaxLevel => levelCount - 1;
    public int MaxDropLevel => Mathf.Min(4, MaxLevel);

    /// <summary>
    /// 게임 시작 시 호출 - 전체 풀에서 랜덤으로 levelCount명 선택
    /// </summary>
    public void SelectRandomCharacters()
    {
        var pool = new List<CharacterData>(allCharacters);

        // Fisher-Yates 셔플
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        int count = Mathf.Min(levelCount, pool.Count);
        selectedCharacters = new CharacterData[count];
        for (int i = 0; i < count; i++)
        {
            selectedCharacters[i] = pool[i];
        }
    }

    /// <summary>
    /// 현재 선택된 캐릭터 배열 (레벨 순서)
    /// </summary>
    public CharacterData[] SelectedCharacters => selectedCharacters;

    public CharacterData GetCharacter(int level)
    {
        if (selectedCharacters == null || level < 0 || level >= selectedCharacters.Length)
            return null;
        return selectedCharacters[level];
    }

    public CharacterData GetNextCharacter(int currentLevel)
    {
        return GetCharacter(currentLevel + 1);
    }

    /// <summary>
    /// 레벨에 따른 반지름 계산
    /// 레벨 4 이상부터 가속적으로 커지는 커브 적용
    /// </summary>
    public float GetRadius(int level)
    {
        if (MaxLevel <= 0) return minRadius;
        float t = (float)level / MaxLevel;
        // t를 제곱하여 후반 레벨이 더 급격히 커지게
        float curved = t * t;
        return Mathf.Lerp(minRadius, maxRadius, curved);
    }

    /// <summary>
    /// 레벨에 따른 점수 계산
    /// </summary>
    public int GetMergeScore(int level)
    {
        return baseScore + scorePerLevel * level;
    }
}
