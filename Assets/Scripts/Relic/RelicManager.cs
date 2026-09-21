using System.Collections.Generic;
using UnityEngine;

// 이번 런의 유물 보유 목록과 획득·효과 호출을 관리
// 상점과 이벤트는 이 클래스를 통해서만 유물을 지급하도록 연결
// 새 런 또는 게임오버 처리 시 ClearRunRelics를 호출하는 구조를 사용

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance { get; private set; }

    private readonly List<RelicInstance> relics = new List<RelicInstance>();
    public IReadOnlyList<RelicInstance> Relics => relics;

    // 싱글턴과 런 간 유지 정책을 초기화할 예정
    private void Awake()
    {
        // TODO: 싱글턴 중복 방지와 DontDestroyOnLoad 정책을 구현
    }

    // 상점 또는 이벤트 보상으로 유물을 획득
    public RelicInstance Acquire(RelicData relicData, RelicSource source)
    {
        // TODO: 인스턴스 생성, 목록 추가, OnAcquired 호출, 획득 연출을 구현.
        return null;
    }

    // 해당 유물을 현재 런에서 보유했는지 확인
    public bool HasRelic(string relicId)
    {
        // TODO: relicId로 보유 목록을 검색
        return false;
    }

    // 게임오버 또는 새 런 시작 시 모든 보유 유물을 제거
    public void ClearRunRelics()
    {
        // TODO: 효과 종료 처리 후 보유 목록을 비운다.
    }

    // BattleManager가 전투 시작 시 호출할 유물
    public void NotifyBattleStarted(Entity owner, Entity opponent)
    {
        // TODO: 모든 보유 유물 효과의 OnBattleStarted를 호출
    }

    // BattleManager가 카드 사용 성공 후 호출할 유물 
    public void NotifyCardPlayed(Entity owner, CardInstance card, Entity target)
    {
        // TODO: 모든 보유 유물 효과의 OnCardPlayed를 호출
    }

    // BattleManager가 전투 종료 시 호출할 유물
    public void NotifyBattleEnded(bool playerWon)
    {
        // TODO: 모든 보유 유물 효과의 OnBattleEnded를 호출
    }
}
