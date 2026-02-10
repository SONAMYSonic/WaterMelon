using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "WatermelonGame/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    [Tooltip("레벨 순서대로 배치 (index 0 = level 0, 가장 작은 캐릭터)")]
    public CharacterData[] characters;

    public int MaxLevel => characters.Length - 1;

    public CharacterData GetCharacter(int level)
    {
        if (level < 0 || level >= characters.Length) return null;
        return characters[level];
    }

    public CharacterData GetNextCharacter(int currentLevel)
    {
        return GetCharacter(currentLevel + 1);
    }

    /// <summary>
    /// 드랍 가능한 최대 레벨 (보통 0~4 중 랜덤)
    /// </summary>
    public int MaxDropLevel => Mathf.Min(4, MaxLevel);
}
