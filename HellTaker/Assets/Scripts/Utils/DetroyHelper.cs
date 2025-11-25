using UnityEngine;

/** 확장 메서드 생성 (static 필수, this 키워드 활용)*/
public static class DestroyHelper
{
    /** Transform의 모든 자식 안전하게 삭제 */
    public static void DestroyAllChildren(this Transform parent)
    {
        if (parent == null) return;

        // foreach 대신 역순 순회 (삭제 시 인덱스 꼬임 방지, 안정성 향상)
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child != null)
            {
                Object.Destroy(child.gameObject);
            }
        }
    }

    /** 여러 부모의 자식 안전하게 삭제 (오버로드) */
    public static void DestroyAllChildren(params Transform[] parents)
    {
        foreach (Transform parent in parents)
        {
            DestroyAllChildren(parent);
        }
    }

    /** 특정 태그의 모든 오브젝트 삭제 */
    public static void DestroyAllWithTagImmediate(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }

    /** 여러 태그의 모든 오브젝트 삭제*/
    public static void DestroyAllWithTagImmediate(params string[] tags)
    {
        foreach (string tag in tags)
        {
            DestroyAllWithTagImmediate(tag);
        }
    }
}
