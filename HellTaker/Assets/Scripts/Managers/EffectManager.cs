using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EffectType
{
    None = -1,
    Move,
    Kick,
    Damage,
    KeyCollect,
    MonsterDestroy,
}

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [Header("Effect Prefabs")]
    [SerializeField] private GameObject moveEffectPrefab;
    [SerializeField] private GameObject kickEffectPrefab;
    [SerializeField] private GameObject damageEffectPrefab;
    [SerializeField] private GameObject keyCollectEffectPrefab;
    [SerializeField] private GameObject monsterDestroyEffectPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        // 싱글톤 중복 방지
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void PlayEffect(EffectType type, Vector3 worldPosition)
    {
        
        if (!TryGetEffectPrefab(type, out GameObject prefab))
        {
            Debug.LogWarning($"[EffectManager] {type} 타입의 이펙트 프리팹이 할당되지 않았습니다.");
            return;
        }

        Instantiate(prefab, worldPosition, Quaternion.identity);
    }

    public void PlayEffectAtObject(EffectType type, GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning($"[EffectManager] 이펙트를 재생할 오브젝트가 null입니다.");
            return;
        }

        Vector2Int gridPos = GridManager.Instance.WorldToGrid(obj.transform.position);
        PlayEffectAtGrid(type, gridPos);
    }

    public void PlayEffectAtGrid(EffectType type, Vector2Int gridPos)
    {
        Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);
        PlayEffect(type, worldPos);
    }

    public void PlayEffectBetweenGrids(EffectType type, Vector2Int fromGrid, Vector2Int toGrid)
    {
        Vector3 fromWorld = GridManager.Instance.GridToWorld(fromGrid);
        Vector3 toWorld = GridManager.Instance.GridToWorld(toGrid);
        Vector3 midPoint = (fromWorld + toWorld) / 2f;

        PlayEffect(type, midPoint);
    }

    private bool TryGetEffectPrefab(EffectType type, out GameObject prefab)
    {
        switch(type)
        {
            case EffectType.Move:
                prefab = moveEffectPrefab;
                break;
            case EffectType.Kick:
                prefab = kickEffectPrefab;
                break;
            case EffectType.Damage:
                prefab = damageEffectPrefab;
                break;
            case EffectType.KeyCollect:
                prefab = keyCollectEffectPrefab;
                break;
            case EffectType.MonsterDestroy:
                prefab = monsterDestroyEffectPrefab;
                break;
            default:
                prefab = null;
                return false;
        }

        return prefab != null;
    }

}
