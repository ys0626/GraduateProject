using System.Collections.Generic;
using UnityEngine;

public class EntityStatusUI : MonoBehaviour
{
    public enum StatusOwner
    {
        Player,
        Enemy
    }

    [Header("Status Owner")]
    [SerializeField] private StatusOwner statusOwner;

    private Entity targetEntity;

    [Header("UI")]
    [SerializeField] private Transform iconParent;

    [SerializeField] private GameObject statusIconPrefab;

    private List<GameObject> spawnedIcons =
        new List<GameObject>();

    private void Start()
    {
        // Entity 연결
        if (statusOwner == StatusOwner.Player)
        {
            targetEntity =
                GameData.instance.player;
        }

        else
        {
            targetEntity =
                GameData.instance.enemy;
        }

        // 이벤트 등록
        targetEntity.OnStatusesChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (targetEntity != null)
        {
            targetEntity.OnStatusesChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        ClearIcons();

        // UI 표시용 정렬 리스트
        List<Status> sortedStatuses =
            new List<Status>(targetEntity.statuses);

        // 버프 우선 정렬
        sortedStatuses.Sort((a, b) =>
        {
            // 둘 다 같은 종류면 순서 유지
            if (a.IsDebuff == b.IsDebuff)
            {
                return 0;
            }

            // 버프(false)가 디버프(true)보다 앞
            return a.IsDebuff.CompareTo(b.IsDebuff);
        });

        foreach (Status status in sortedStatuses)
        {
            GameObject obj =
                Instantiate(
                    statusIconPrefab,
                    iconParent);

            StatusIconUI iconUI =
                obj.GetComponent<StatusIconUI>();

            iconUI.Setup(status);

            spawnedIcons.Add(obj);
        }
    }

    private void ClearIcons()
    {
        foreach (GameObject obj in spawnedIcons)
        {
            Destroy(obj);
        }

        spawnedIcons.Clear();
    }
}