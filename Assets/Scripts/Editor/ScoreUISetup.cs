using UnityEngine;
using UnityEditor;
using TMPro;

public static class ScoreUISetup
{
    [MenuItem("Watermelon/Setup Score UI (4 objects)")]
    public static void SetupScoreUI()
    {
        // LeftScorePanel 찾기
        var panel = GameObject.Find("GameCanvas/LeftScorePanel");
        if (panel == null) { Debug.LogError("LeftScorePanel not found!"); return; }

        var panelRT = panel.GetComponent<RectTransform>();

        // 기존 ScoreLabel, HighScoreLabel 찾아서 이름 변경 + 텍스트 고정
        var scoreLabel = panel.transform.Find("ScoreLabel");
        if (scoreLabel != null)
        {
            scoreLabel.name = "ScoreTxt";
            var tmp = scoreLabel.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = "SCORE";
        }

        var highLabel = panel.transform.Find("HighScoreLabel");
        if (highLabel != null)
        {
            highLabel.name = "BestTxt";
            var tmp = highLabel.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = "BEST";
        }

        // ScoreValue 생성 (없으면)
        if (panel.transform.Find("ScoreValue") == null)
        {
            var sv = CreateTMPObject("ScoreValue", panel.transform);
            var svRT = sv.GetComponent<RectTransform>();
            // ScoreTxt 바로 아래에 배치
            svRT.anchorMin = svRT.anchorMax = svRT.pivot = new Vector2(0.5f, 1f);
            svRT.anchoredPosition = new Vector2(0, -50);
            svRT.sizeDelta = new Vector2(170, 40);
            var svTMP = sv.GetComponent<TextMeshProUGUI>();
            svTMP.text = "0";
            svTMP.fontSize = 32;
            svTMP.alignment = TextAlignmentOptions.Center;
        }

        // BestValue 생성 (없으면)
        if (panel.transform.Find("BestValue") == null)
        {
            var bv = CreateTMPObject("BestValue", panel.transform);
            var bvRT = bv.GetComponent<RectTransform>();
            bvRT.anchorMin = bvRT.anchorMax = bvRT.pivot = new Vector2(0.5f, 1f);
            bvRT.anchoredPosition = new Vector2(0, -130);
            bvRT.sizeDelta = new Vector2(170, 40);
            var bvTMP = bv.GetComponent<TextMeshProUGUI>();
            bvTMP.text = "0";
            bvTMP.fontSize = 26;
            bvTMP.alignment = TextAlignmentOptions.Center;
        }

        // UIManager에 참조 연결
        var canvas = GameObject.Find("GameCanvas");
        if (canvas != null)
        {
            var uiMgr = canvas.GetComponent<UIManager>();
            if (uiMgr != null)
            {
                var so = new SerializedObject(uiMgr);
                
                var scoreProp = so.FindProperty("scoreText");
                var scoreValObj = panel.transform.Find("ScoreValue");
                if (scoreValObj != null)
                    scoreProp.objectReferenceValue = scoreValObj.GetComponent<TextMeshProUGUI>();
                
                var highProp = so.FindProperty("highScoreText");
                var bestValObj = panel.transform.Find("BestValue");
                if (bestValObj != null)
                    highProp.objectReferenceValue = bestValObj.GetComponent<TextMeshProUGUI>();
                
                so.ApplyModifiedProperties();
                Debug.Log("UIManager score references updated.");
            }
        }

        EditorUtility.SetDirty(panel);
        Debug.Log("Score UI setup complete: ScoreTxt, ScoreValue, BestTxt, BestValue");
    }

    private static GameObject CreateTMPObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.color = Color.white;
        return go;
    }
}
