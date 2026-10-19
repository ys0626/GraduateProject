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
        // 이 UI가 단독 테스트 씬에 있어도 예외 없이 동작하도록 한다.
        // GameData는 영속 오브젝트이므로 해당 씬에 없을 수 있다.
        if (GameData.instance == null)
        {
            Debug.LogWarning(
                "[EntityStatusUI] GameData가 없어 상태 UI를 초기화하지 않습니다."
            );

            return;
        }

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
        if (targetEntity == null)
            return;

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
