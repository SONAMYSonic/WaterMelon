using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "WatermelonGame/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("기본 정보")]
    public string characterName;
    public Sprite characterSprite;

    [Header("오디오")]
    public AudioClip mergeSound;    // 합체 효과음
    public AudioClip voiceClip;     // 캐릭터 보이스

    [Header("이펙트")]
    public Color mergeEffectColor = Color.white; // 합체 이펙트 색상
}
