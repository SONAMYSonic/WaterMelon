using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public class LBContentFixer : EditorWindow
{
    [MenuItem("Tools/Fix LBContent Layout")]
    public static void Fix()
    {
        // 프리팹 편집 모드에서 나가기
        var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            StageUtility.GoToMainStage();
        }

        // LBContent 찾기 (path로 검색)
        var lbScrollView = GameObject.Find("LBScrollView");
        if (lbScrollView == null) return;

        var viewport = lbScrollView.transform.Find("Viewport");
        if (viewport == null) return;

        var lbContent = viewport.Find("LBContent");
        if (lbContent == null) return;

        var vlg = lbContent.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) return;

        var so = new SerializedObject(vlg);
        var prop = so.FindProperty("m_ChildControlHeight");
        if (prop != null)
        {
            prop.boolValue = true;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(vlg);
        }
    }
}
