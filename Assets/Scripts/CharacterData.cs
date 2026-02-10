using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "WatermelonGame/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("기본 정보")]
    public string characterName;
    public int level; // 0~10 (0이 가장 작은 캐릭터, 10이 최종 진화)
    public Sprite characterSprite;

    [Header("크기")]
    public float radius = 0.3f; // 물리 원형 콜라이더 반지름

    [Header("점수")]
    public int mergeScore = 10; // 합체 시 획득 점수

    [Header("오디오")]
    public AudioClip mergeSound;    // 합체 효과음
    public AudioClip voiceClip;     // 캐릭터 보이스

    [Header("이펙트")]
    public Color mergeEffectColor = Color.white; // 합체 이펙트 색상
}
