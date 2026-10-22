using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 맵 생성 및 맵 진행 상태 관리 클래스
/// </summary>
public class MapGenerator : MonoBehaviour
{
    public static MapGenerator instance;

    [Header("Map Settings")]
    [SerializeField] private int floorCount = 10;
    [SerializeField] private int minNodesPerFloor = 3;
    [SerializeField] private int maxNodesPerFloor = 5;

    [Header("Random Seed")]
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 12345;

    [Header("Rest Node Settings")]
    [Min(0)]
    [SerializeField] private int minRestNodes = 3;
    [Min(0)]
    [SerializeField] private int maxRestNodes = 5;
    [Min(1)]
    [SerializeField] private int restMinFloor = 5;

    [Header("Elite Node Settings")]
    [Min(0)]
    [SerializeField] private int minEliteNodes = 3;
    [Min(0)]
    [SerializeField] private int maxEliteNodes = 4;
    [Min(1)]
    [SerializeField] private int eliteMinFloor = 5;

    [Header("Shop Node Settings")]
    [Min(0)]
    [SerializeField] private int minShopNodes = 2;
    [Min(0)]
    [SerializeField] private int maxShopNodes = 3;
    [Min(1)]
    [SerializeField] private int shopMinFloor = 3;

    [Header("Remaining Node Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float combatChance = 0.5f;

    [Header("Map Position")]
    [SerializeField] private float horizontalSpacing = 110f;
    [SerializeField] private float verticalSpacing = 170f;
    [SerializeField] private float verticalRandomness = 50f;

    private List<List<MapNode>> floors = new List<List<MapNode>>();

    private int nextNodeId = 0;
    private int currentNodeId = -1;

    public int CurrentSeed { get; private set; }
    public List<List<MapNode>> Floors => floors;
    public bool IsMapGenerated { get; private set; }
    public int CurrentNodeId => currentNodeId;


    // ============================================================
    // SINGLETON / PERSISTENCE
    // ============================================================

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }


    // ============================================================
    // MAP GENERATION
    // ============================================================

    public void GenerateMap()
    {
        IsMapGenerated = false;

        floors.Clear();
        nextNodeId = 0;
        currentNodeId = -1;

        SetSeed();

        GenerateNodes();
        GeneratePositions();
        ConnectNodes();
        AssignNodeTypes();

        DebugPrintMap();

        IsMapGenerated = true;
    }


    // ============================================================
    // SEED
    // ============================================================

    private void SetSeed()
    {
        CurrentSeed = useRandomSeed ? Environment.TickCount : seed;
        UnityEngine.Random.InitState(CurrentSeed);

        Debug.Log($"[MapGenerator] Seed: {CurrentSeed}");
    }


    // ============================================================
    // NODE CREATION
    // ============================================================

    private void GenerateNodes()
    {
        List<MapNode> startFloor = new List<MapNode>();

        MapNode startNode = new MapNode(
            nextNodeId++,
            0,
            MapNodeType.Start
        );

        startFloor.Add(startNode);
        floors.Add(startFloor);

        for (int floor = 1; floor <= floorCount; floor++)
        {
            List<MapNode> currentFloor = new List<MapNode>();

            if (floor == floorCount)
            {
                MapNode bossNode = new MapNode(
                    nextNodeId++,
                    floor,
                    MapNodeType.Boss
                );

                currentFloor.Add(bossNode);
            }
            else
            {
                int nodeCount = UnityEngine.Random.Range(
                    minNodesPerFloor,
                    maxNodesPerFloor + 1
                );

                for (int i = 0; i < nodeCount; i++)
                {
                    MapNode node = new MapNode(
                        nextNodeId++,
                        floor,
                        MapNodeType.Combat
                    );

                    currentFloor.Add(node);
                }
            }

            floors.Add(currentFloor);
        }
    }


    // ============================================================
    // CONNECTION
    // ============================================================

    private void ConnectNodes()
    {
        for (int floor = 0; floor < floors.Count - 1; floor++)
        {
            ConnectFloor(floors[floor], floors[floor + 1]);
        }
    }


    private void ConnectFloor(
        List<MapNode> currentFloor,
        List<MapNode> nextFloor)
    {
        int currentCount = currentFloor.Count;
        int nextCount = nextFloor.Count;

        if (currentCount == 0 || nextCount == 0)
        {
            return;
        }

        // 1. 현재 층의 모든 노드를 다음 층에 연결
        for (int i = 0; i < currentCount; i++)
        {
            int targetIndex;

            if (currentCount == 1)
            {
                targetIndex = Mathf.RoundToInt((nextCount - 1) / 2f);
            }
            else
            {
                targetIndex = Mathf.RoundToInt(
                    i * (nextCount - 1f) / (currentCount - 1f)
                );
            }

            AddConnection(currentFloor[i], nextFloor[targetIndex]);
        }

        // 2. 다음 층의 모든 노드를 이전 층에 연결
        for (int i = 0; i < nextCount; i++)
        {
            int sourceIndex;

            if (nextCount == 1)
            {
                sourceIndex = Mathf.RoundToInt((currentCount - 1) / 2f);
            }
            else
            {
                sourceIndex = Mathf.RoundToInt(
                    i * (currentCount - 1f) / (nextCount - 1f)
                );
            }

            AddConnection(currentFloor[sourceIndex], nextFloor[i]);
        }

        // 3. 추가 갈림길 생성
        for (int i = 0; i < currentCount; i++)
        {
            int baseTargetIndex;

            if (currentCount == 1)
            {
                baseTargetIndex = Mathf.RoundToInt((nextCount - 1) / 2f);
            }
            else
            {
                baseTargetIndex = Mathf.RoundToInt(
                    i * (nextCount - 1f) / (currentCount - 1f)
                );
            }

            List<int> candidates = new List<int>();

            if (baseTargetIndex - 1 >= 0)
            {
                candidates.Add(baseTargetIndex - 1);
            }

            if (baseTargetIndex + 1 < nextCount)
            {
                candidates.Add(baseTargetIndex + 1);
            }

            if (candidates.Count == 0)
            {
                continue;
            }

            if (UnityEngine.Random.value > 0.65f)
            {
                continue;
            }

            int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
            int targetIndex = candidates[randomIndex];

            MapNode fromNode = currentFloor[i];
            MapNode toNode = nextFloor[targetIndex];

            if (!HasConnection(fromNode, toNode) &&
                !WouldCrossExistingConnection(
                    fromNode,
                    toNode,
                    currentFloor,
                    nextFloor))
            {
                AddConnection(fromNode, toNode);
            }
        }
    }


    private bool HasConnection(MapNode fromNode, MapNode toNode)
    {
        return fromNode.nextNodes.Contains(toNode);
    }


    private bool WouldCrossExistingConnection(
        MapNode fromNode,
        MapNode toNode,
        List<MapNode> currentFloor,
        List<MapNode> nextFloor)
    {
        Vector2 newStart = fromNode.position;
        Vector2 newEnd = toNode.position;

        for (int i = 0; i < currentFloor.Count; i++)
        {
            MapNode node = currentFloor[i];

            foreach (MapNode nextNode in node.nextNodes)
            {
                if (node == fromNode && nextNode == toNode)
                {
                    continue;
                }

                if (DoLinesCross(
                    newStart,
                    newEnd,
                    node.position,
                    nextNode.position))
                {
                    return true;
                }
            }
        }

        return false;
    }


    private bool DoLinesCross(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        float abC = Cross(b - a, c - a);
        float abD = Cross(b - a, d - a);
        float cdA = Cross(d - c, a - c);
        float cdB = Cross(d - c, b - c);

        return abC * abD < 0f && cdA * cdB < 0f;
    }


    private float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }


    private void AddConnection(MapNode source, MapNode target)
    {
        if (source == null || target == null)
        {
            return;
        }

        if (source.nextNodes.Contains(target))
        {
            return;
        }

        source.nextNodes.Add(target);
        target.previousNodes.Add(source);
    }


    // ============================================================
    // CONNECTIVITY CHECK
    // 기존 코드 유지. 기존 GenerateMap에서는 호출하지 않음.
    // ============================================================

    private void EnsureAllNodesConnected()
    {
        for (int floor = 0; floor < floors.Count - 1; floor++)
        {
            List<MapNode> currentFloor = floors[floor];
            List<MapNode> nextFloor = floors[floor + 1];

            if (currentFloor.Count == 0 || nextFloor.Count == 0)
            {
                continue;
            }

            foreach (MapNode currentNode in currentFloor)
            {
                if (currentNode.nextNodes.Count == 0)
                {
                    MapNode target = nextFloor[
                        UnityEngine.Random.Range(0, nextFloor.Count)
                    ];

                    AddConnection(currentNode, target);
                }
            }

            foreach (MapNode nextNode in nextFloor)
            {
                if (nextNode.previousNodes.Count == 0)
                {
                    MapNode source = currentFloor[
                        UnityEngine.Random.Range(0, currentFloor.Count)
                    ];

                    AddConnection(source, nextNode);
                }
            }
        }
    }


    // ============================================================
    // NODE TYPE ASSIGNMENT
    // ============================================================

    private void AssignNodeTypes()
    {
        floors[0][0].type = MapNodeType.Start;
        floors[floorCount][0].type = MapNodeType.Boss;

        // 모든 일반 노드는 기본적으로 Combat
        for (int floor = 1; floor < floorCount; floor++)
        {
            foreach (MapNode node in floors[floor])
            {
                node.type = MapNodeType.Combat;
            }
        }

        // 1. 보스 직전 층은 전부 휴식 노드
        FixBossPredecessorRest();

        // 2. 특수 노드를 순서대로 배정
        bool restAssigned = AssignRequiredNodeType(
            MapNodeType.Rest,
            minRestNodes,
            maxRestNodes,
            restMinFloor
        );

        bool eliteAssigned = AssignRequiredNodeType(
            MapNodeType.Elite,
            minEliteNodes,
            maxEliteNodes,
            eliteMinFloor
        );

        bool shopAssigned = AssignRequiredNodeType(
            MapNodeType.Shop,
            minShopNodes,
            maxShopNodes,
            shopMinFloor
        );

        // 3. 남은 Combat 노드를 Combat 또는 Event로 배정
        AssignRemainingNodeTypes();

        // 4. 설정 충족 여부 보고
        CheckNodeCount(MapNodeType.Rest, minRestNodes, maxRestNodes);
        CheckNodeCount(MapNodeType.Elite, minEliteNodes, maxEliteNodes);
        CheckNodeCount(MapNodeType.Shop, minShopNodes, maxShopNodes);

        if (!restAssigned || !eliteAssigned || !shopAssigned)
        {
            Debug.LogWarning(
                "[MapGenerator] 일부 특수 노드 개수 범위를 만족하지 못했습니다. " +
                "Inspector 설정과 맵 구조를 확인하세요."
            );
        }
    }


    /// <summary>
    /// 지정된 타입을 최소 개수 이상 배정하고,
    /// 최소~최대 범위 내에서 추가 노드를 무작위로 배정한다.
    /// </summary>

    private bool AssignRequiredNodeType(
        MapNodeType type,
        int minCount,
        int maxCount,
        int minFloor)
    {
        minCount = Mathf.Max(0, minCount);
        maxCount = Mathf.Max(minCount, maxCount);
        minFloor = Mathf.Max(1, minFloor);

        int currentCount = CountNodesForSettings(type);

        if (currentCount > maxCount)
        {
            Debug.LogWarning(
                $"[MapGenerator] {type} 노드가 최대 개수 " +
                $"{maxCount}개를 초과했습니다. 현재: {currentCount}개"
            );

            return false;
        }

        if (currentCount < minCount)
        {
            int needed = minCount - currentCount;

            if (!TryAssignAdditionalNodes(type, needed, minFloor))
            {
                Debug.LogWarning(
                    $"[MapGenerator] {type} 노드 최소 {minCount}개를 " +
                    $"배정하지 못했습니다. 현재: {CountNodesForSettings(type)}개"
                );

                return false;
            }
        }

        currentCount = CountNodesForSettings(type);

        if (currentCount >= maxCount)
        {
            return true;
        }

        int targetCount = UnityEngine.Random.Range(
            Mathf.Max(minCount, currentCount),
            maxCount + 1
        );

        int extraNeeded = targetCount - currentCount;

        if (extraNeeded > 0)
        {
            for (int requested = extraNeeded; requested > 0; requested--)
            {
                if (TryAssignAdditionalNodes(type, requested, minFloor))
                {
                    return true;
                }
            }
        }

        return CountNodesForSettings(type) >= minCount;
    }



    private int CountNodes(MapNodeType type)
    {
        int count = 0;

        foreach (List<MapNode> floor in floors)
        {
            foreach (MapNode node in floor)
            {
                if (node.type == type)
                {
                    count++;
                }
            }
        }

        return count;
    }




    private int CountNodesForSettings(MapNodeType type)
    {
        int count = 0;
        int bossPredecessorFloor = floorCount - 1;

        foreach (List<MapNode> floor in floors)
        {
            foreach (MapNode node in floor)
            {
                if (node.type != type)
                {
                    continue;
                }

                // 보스 직전 층의 강제 휴식 노드는 설정 개수에서 제외
                if (type == MapNodeType.Rest &&
                    node.floor == bossPredecessorFloor)
                {
                    continue;
                }

                count++;
            }
        }

        return count;
    }



    /// <summary>
    /// 해당 타입을 지정 개수만큼 배정할 수 있는 연결 충돌 없는 조합을 탐색한다.
    /// 선택한 노드들은 같은 타입의 이웃을 만들지 않아야 한다.
    /// </summary>
    private bool TryAssignAdditionalNodes(
        MapNodeType type,
        int needed,
        int minFloor)
    {
        if (needed <= 0)
        {
            return true;
        }

        List<MapNode> candidates = new List<MapNode>();

        for (int floor = Mathf.Max(1, minFloor);
             floor < floorCount;
             floor++)
        {
            foreach (MapNode node in floors[floor])
            {
                if (node.type == MapNodeType.Combat &&
                    CanAssignNodeType(node, type))
                {
                    candidates.Add(node);
                }
            }
        }

        if (candidates.Count < needed)
        {
            return false;
        }

        // 후보 순서를 무작위화하되, 백트래킹으로 가능한 조합을 탐색한다.
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            MapNode temp = candidates[i];
            candidates[i] = candidates[j];
            candidates[j] = temp;
        }

        List<MapNode> selected = new List<MapNode>();

        if (!TrySelectNodesRecursive(
            candidates,
            type,
            needed,
            0,
            selected))
        {
            return false;
        }

        foreach (MapNode node in selected)
        {
            node.type = type;
        }

        return true;
    }


    private bool TrySelectNodesRecursive(
        List<MapNode> candidates,
        MapNodeType type,
        int needed,
        int startIndex,
        List<MapNode> selected)
    {
        if (selected.Count == needed)
        {
            return true;
        }

        int stillNeeded = needed - selected.Count;

        if (candidates.Count - startIndex < stillNeeded)
        {
            return false;
        }

        for (int i = startIndex; i < candidates.Count; i++)
        {
            MapNode candidate = candidates[i];

            if (!CanAssignNodeType(candidate, type))
            {
                continue;
            }

            bool conflictsWithSelected = false;

            foreach (MapNode selectedNode in selected)
            {
                if (AreConnected(candidate, selectedNode))
                {
                    conflictsWithSelected = true;
                    break;
                }
            }

            if (conflictsWithSelected)
            {
                continue;
            }

            selected.Add(candidate);

            if (TrySelectNodesRecursive(
                candidates,
                type,
                needed,
                i + 1,
                selected))
            {
                return true;
            }

            selected.RemoveAt(selected.Count - 1);
        }

        return false;
    }

    private bool CanAssignNodeType(MapNode node, MapNodeType type)
    {
        if (node == null || node.type != MapNodeType.Combat)
        {
            return false;
        }

        // 휴식 노드는 보스 직전 층을 제외하고 설정한 최소 층수부터 가능.
        // 보스 직전 층은 이미 강제 휴식으로 지정되어 있다.
        if (type == MapNodeType.Rest && node.floor < restMinFloor)
        {
            return false;
        }

        if (type == MapNodeType.Elite && node.floor < eliteMinFloor)
        {
            return false;
        }

        if (type == MapNodeType.Shop && node.floor < shopMinFloor)
        {
            return false;
        }

        // 보스 직전 층은 이미 휴식 노드로 고정되어야 한다.
        if (node.floor == floorCount - 1)
        {
            return false;
        }

        // 연결된 이전/다음 노드와 같은 특수 타입이 될 수 없다.
        if (HasSameTypeNeighbor(node, type))
        {
            return false;
        }

        return true;
    }


    private bool HasSameTypeNeighbor(MapNode node, MapNodeType type)
    {
        foreach (MapNode previous in node.previousNodes)
        {
            if (previous.type == type)
            {
                return true;
            }
        }

        foreach (MapNode next in node.nextNodes)
        {
            if (next.type == type)
            {
                return true;
            }
        }

        return false;
    }


    private bool AreConnected(MapNode a, MapNode b)
    {
        return a.nextNodes.Contains(b) ||
               a.previousNodes.Contains(b) ||
               b.nextNodes.Contains(a) ||
               b.previousNodes.Contains(a);
    }


    private void FixBossPredecessorRest()
    {
        int bossPredecessorFloor = floorCount - 1;

        if (bossPredecessorFloor < 1 ||
            bossPredecessorFloor >= floors.Count)
        {
            return;
        }

        foreach (MapNode node in floors[bossPredecessorFloor])
        {
            node.type = MapNodeType.Rest;
        }
    }


    private void AssignRemainingNodeTypes()
    {
        float chance = Mathf.Clamp01(combatChance);

        for (int floor = 1; floor < floorCount; floor++)
        {
            foreach (MapNode node in floors[floor])
            {
                if (node.type != MapNodeType.Combat)
                {
                    continue;
                }

                if (UnityEngine.Random.value < chance)
                {
                    node.type = MapNodeType.Combat;
                }
                else
                {
                    node.type = MapNodeType.Event;
                }
            }
        }
    }


    private void CheckNodeCount(
        MapNodeType type,
        int minCount,
        int maxCount)
    {
        minCount = Mathf.Max(0, minCount);
        maxCount = Mathf.Max(minCount, maxCount);

        int count = CountNodesForSettings(type);

        if (count < minCount || count > maxCount)
        {
            Debug.LogWarning(
                $"[MapGenerator] {type} 노드 개수 범위 미충족. " +
                $"현재: {count}개 / 허용 범위: {minCount}~{maxCount}개"
            );
        }
    }


    // ============================================================
    // POSITION
    // ============================================================

    private void GeneratePositions()
    {
        for (int floor = 0; floor < floors.Count; floor++)
        {
            List<MapNode> currentFloor = floors[floor];
            int nodeCount = currentFloor.Count;

            if (nodeCount == 1)
            {
                currentFloor[0].position = new Vector2(
                    floor * horizontalSpacing,
                    0f
                );

                continue;
            }

            float[] yPositions = new float[nodeCount];
            yPositions[0] = 0f;

            for (int i = 1; i < nodeCount; i++)
            {
                float randomSpacing = UnityEngine.Random.Range(
                    0f,
                    verticalRandomness
                );

                float spacing = verticalSpacing + randomSpacing;
                yPositions[i] = yPositions[i - 1] - spacing;
            }

            float centerOffset =
                (yPositions[0] + yPositions[nodeCount - 1]) / 2f;

            for (int i = 0; i < nodeCount; i++)
            {
                float y = yPositions[i] - centerOffset;

                currentFloor[i].position = new Vector2(
                    floor * horizontalSpacing,
                    y
                );
            }
        }

        float totalWidth = (floors.Count - 1) * horizontalSpacing;
        float offsetX = totalWidth / 2f;

        for (int floor = 0; floor < floors.Count; floor++)
        {
            foreach (MapNode node in floors[floor])
            {
                node.position.x -= offsetX;
            }
        }
    }


    // ============================================================
    // DEBUG
    // ============================================================

    private void DebugPrintMap()
    {
        Debug.Log(
            $"========== MAP GENERATED ==========\n" +
            $"Seed: {CurrentSeed}\n" +
            $"Floors: {floors.Count}"
        );

        foreach (List<MapNode> floor in floors)
        {
            foreach (MapNode node in floor)
            {
                string connections = "";

                foreach (MapNode next in node.nextNodes)
                {
                    connections += $"Node {next.id} ";
                }

                Debug.Log(
                    $"Floor {node.floor} | " +
                    $"Node {node.id} | " +
                    $"{node.type} | " +
                    $"Next: {connections}"
                );
            }
        }
    }


    // ============================================================
    // MAP PROGRESS
    // ============================================================

    public void SetCurrentNode(MapNode node)
    {
        if (node == null)
        {
            return;
        }

        currentNodeId = node.id;
    }


    public MapNode GetNodeById(int id)
    {
        foreach (List<MapNode> floor in floors)
        {
            foreach (MapNode node in floor)
            {
                if (node.id == id)
                {
                    return node;
                }
            }
        }

        return null;
    }


    // ============================================================
    // PUBLIC API
    // ============================================================

    public List<MapNode> GetFloor(int floor)
    {
        if (floor < 0 || floor >= floors.Count)
        {
            return null;
        }

        return floors[floor];
    }


    public MapNode GetNode(int floor, int index)
    {
        if (floor < 0 || floor >= floors.Count)
        {
            return null;
        }

        if (index < 0 || index >= floors[floor].Count)
        {
            return null;
        }

        return floors[floor][index];
    }


    public MapNode GetStartNode()
    {
        if (floors.Count == 0 || floors[0].Count == 0)
        {
            return null;
        }

        return floors[0][0];
    }


    public MapNode GetBossNode()
    {
        if (floors.Count == 0)
        {
            return null;
        }

        List<MapNode> lastFloor = floors[floors.Count - 1];

        if (lastFloor.Count == 0)
        {
            return null;
        }

        return lastFloor[0];
    }


    public void RegenerateMap()
    {
        GenerateMap();
    }
}