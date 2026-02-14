using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class LBRowPrefabBuilder : EditorWindow
{
    [MenuItem("Tools/Build LBRow Prefab")]
    public static void Build()
    {
        string prefabPath = "Assets/Prefabs/UI/LBRow.prefab";

        // 폴더 확인
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

        // 기존 프리팹이 있으면 삭제 후 재생성
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            AssetDatabase.DeleteAsset(prefabPath);

        // === Row 루트 ===
        var rowGo = new GameObject("LBRow", typeof(RectTransform), typeof(Image),
            typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(LeaderboardRow));

        var rowRT = rowGo.GetComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(0, 40);

        // 배경 이미지 (투명 기본, Inspector에서 스프라이트 변경 가능)
        var bgImg = rowGo.GetComponent<Image>();
        bgImg.color = new Color(1f, 1f, 1f, 0.05f);

        // 레이아웃
        var hlg = rowGo.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.padding = new RectOffset(10, 10, 2, 2);
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        var rowLE = rowGo.GetComponent<LayoutElement>();
        rowLE.preferredHeight = 40;

        // === 순위 텍스트 ===
        var rankGo = CreateCell(rowGo.transform, "RankText", "#", 55,
            TextAlignmentOptions.Center, 20, FontStyles.Bold);

        // === 이름 텍스트 ===
        var nameGo = CreateCell(rowGo.transform, "NameText", "Player", 220,
            TextAlignmentOptions.Left, 19, FontStyles.Normal);

        // === 점수 텍스트 ===
        var scoreGo = CreateCell(rowGo.transform, "ScoreText", "0", 140,
            TextAlignmentOptions.Right, 19, FontStyles.Normal);

        // === LeaderboardRow 컴포넌트에 참조 연결 ===
        var lbRow = rowGo.GetComponent<LeaderboardRow>();
        var lbRowSO = new SerializedObject(lbRow);
        lbRowSO.FindProperty("rankText").objectReferenceValue =
            rankGo.GetComponent<TextMeshProUGUI>();
        lbRowSO.FindProperty("nameText").objectReferenceValue =
            nameGo.GetComponent<TextMeshProUGUI>();
        lbRowSO.FindProperty("scoreText").objectReferenceValue =
            scoreGo.GetComponent<TextMeshProUGUI>();
        lbRowSO.FindProperty("backgroundImage").objectReferenceValue = bgImg;
        lbRowSO.ApplyModifiedProperties();

        // === 프리팹 저장 ===
        PrefabUtility.SaveAsPrefabAsset(rowGo, prefabPath);
        Object.DestroyImmediate(rowGo);

        // === UIManager에 프리팹 연결 ===
        var uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            var so = new SerializedObject(uiManager);
            var prop = so.FindProperty("leaderboardRowPrefab");
            if (prop != null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                prop.objectReferenceValue = prefab;
                so.ApplyModifiedProperties();
            }
        }
    }

    static GameObject CreateCell(Transform parent, string name, string text,
        float width, TextAlignmentOptions align, float fontSize, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = width;
        le.flexibleWidth = 0;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = align;
        tmp.fontStyle = style;

        return go;
    }
}
