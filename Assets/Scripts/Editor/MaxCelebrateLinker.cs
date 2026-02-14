using UnityEditor;
using UnityEngine;

public class MaxCelebrateLinker
{
    [MenuItem("Tools/Link MaxCelebrate to UIManager")]
    public static void Link()
    {
        var uiMgr = Object.FindFirstObjectByType<UIManager>();
        if (uiMgr == null)
        {
            return;
        }

        var maxCelebrate = GameObject.Find("MaxCelebrate");
        if (maxCelebrate == null)
        {
            // 비활성 오브젝트는 Find로 못 찾으므로 Canvas 하위 탐색
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                var t = canvas.transform.Find("MaxCelebrate");
                if (t != null)
                {
                    maxCelebrate = t.gameObject;
                    break;
                }
            }
        }

        if (maxCelebrate == null)
        {
            return;
        }

        var so = new SerializedObject(uiMgr);
        var prop = so.FindProperty("maxCelebrateObject");
        if (prop != null)
        {
            prop.objectReferenceValue = maxCelebrate;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(uiMgr);
        }
    }
}
