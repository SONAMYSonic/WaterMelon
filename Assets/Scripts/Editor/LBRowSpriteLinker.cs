using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class LBRowSpriteLinker : EditorWindow
{
    [MenuItem("Tools/Link LBRow Sprites")]
    public static void Link()
    {
        string prefabPath = "Assets/Prefabs/UI/LBRow.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return;

        var lbRow = prefab.GetComponent<LeaderboardRow>();
        if (lbRow == null) return;

        var so = new SerializedObject(lbRow);

        // Front Image 연결
        Transform front = prefab.transform.Find("Front");
        if (front != null)
        {
            var frontImg = front.GetComponent<Image>();
            so.FindProperty("frontImage").objectReferenceValue = frontImg;
        }

        // 서브 스프라이트 로드
        string sheetPath = "Assets/Sprites/fes_match_ranking_parts.png";
        Object[] allSprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath);

        foreach (var obj in allSprites)
        {
            if (obj is Sprite sprite)
            {
                switch (sprite.name)
                {
                    case "fes_match_ranking_parts_3":
                        so.FindProperty("rank1Sprite").objectReferenceValue = sprite;
                        break;
                    case "fes_match_ranking_parts_5":
                        so.FindProperty("rank2Sprite").objectReferenceValue = sprite;
                        break;
                    case "fes_match_ranking_parts_7":
                        so.FindProperty("rank3Sprite").objectReferenceValue = sprite;
                        break;
                    case "fes_match_ranking_parts_12":
                        so.FindProperty("defaultFrontSprite").objectReferenceValue = sprite;
                        break;
                }
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();
    }
}
