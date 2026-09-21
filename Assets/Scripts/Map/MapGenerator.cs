using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 맵 생성 로직 담당하는 클래스
/// </summary>
public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int floorCount = 10;

    [SerializeField] private int minNodesPerFloor = 3;
    [SerializeField] private int maxNodesPerFloor = 5;

    [Header("Random Seed")]
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 12345;

    [Header("Node Type Probability")]
    [Range(0f, 1f)]
    [SerializeField] private float eliteChance = 0.15f;

    [Range(0f, 1f)]
    [SerializeField] private float eventChance = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float shopChance = 0.15f;

    //[Range(0f, 1f)]
    //[SerializeField] private float restChance = 0.15f;

    [Header("Map Position")]
    [SerializeField] private float horizontalSpacing = 110f;
    [SerializeField] private float verticalSpacing = 170f;
    [SerializeField] private float verticalRandomness = 50f;

    [Header("Debug")]
    [SerializeField] private bool generateOnStart = true;

    private List<List<MapNode>> floors = new List<List<MapNode>>();

    private int nextNodeId = 0;

    public int CurrentSeed { get; private set; }

    public List<List<MapNode>> Floors => floors;

    public bool IsMapGenerated { get; private set; }

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateMap();
        }
    }


    // ============================================================
    // MAP GENERATION
    // ============================================================

    public void GenerateMap()
    {
        floors.Clear();
        nextNodeId = 0;

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
        if (useRandomSeed)
        {
            CurrentSeed = Environment.TickCount;
        }
        else
        {
            CurrentSeed = seed;
        }

        UnityEngine.Random.InitState(CurrentSeed);

        Debug.Log($"[MapGenerator] Seed: {CurrentSeed}");
    }


    // ============================================================
    // NODE CREATION
    // ============================================================

    private void GenerateNodes()
    {
        // START
        List<MapNode> startFloor = new List<MapNode>();

        MapNode startNode =
            new MapNode(
                nextNodeId++,
                0,
                MapNodeType.Start
            );

        startFloor.Add(startNode);
        floors.Add(startFloor);


        // 일반 층
        for (int floor = 1; floor <= floorCount; floor++)
        {
            List<MapNode> currentFloor = new List<MapNode>();

            // 마지막 층은 Boss 하나만
            if (floor == floorCount)
            {
                MapNode bossNode =
                    new MapNode(
                        nextNodeId++,
                        floor,
                        MapNodeType.Boss
                    );

                currentFloor.Add(bossNode);
            }
            else
            {
                int nodeCount =
                    UnityEngine.Random.Range(
                        minNodesPerFloor,
                        maxNodesPerFloor + 1
                    );

                for (int i = 0; i < nodeCount; i++)
                {
                    MapNode node =
                        new MapNode(
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
            List<MapNode> currentFloor = floors[floor];
            List<MapNode> nextFloor = floors[floor + 1];

            ConnectFloor(currentFloor, nextFloor);
        }
    }


    private void ConnectFloor(
    List<MapNode> currentFloor,
    List<MapNode> nextFloor)
    {
        int currentCount = currentFloor.Count;
        int nextCount = nextFloor.Count;

        if (currentCount == 0 || nextCount == 0)
            return;

        // --------------------------------------------------
        // 1. 현재 층의 모든 노드가 다음 층으로 연결
        // --------------------------------------------------

        for (int i = 0; i < currentCount; i++)
        {
            int targetIndex;

            if (currentCount == 1)
            {
                targetIndex =
                    Mathf.RoundToInt(
                        (nextCount - 1) / 2f
                    );
            }
            else
            {
                targetIndex =
                    Mathf.RoundToInt(
                        i * (nextCount - 1f)
                        / (currentCount - 1f)
                    );
            }

            AddConnection(
                currentFloor[i],
                nextFloor[targetIndex]
            );
        }

        // --------------------------------------------------
        // 2. 다음 층의 모든 노드가 이전 층과 연결
        // --------------------------------------------------

        for (int i = 0; i < nextCount; i++)
        {
            int sourceIndex;

            if (nextCount == 1)
            {
                sourceIndex =
                    Mathf.RoundToInt(
                        (currentCount - 1) / 2f
                    );
            }
            else
            {
                sourceIndex =
                    Mathf.RoundToInt(
                        i * (currentCount - 1f)
                        / (nextCount - 1f)
                    );
            }

            AddConnection(
                currentFloor[sourceIndex],
                nextFloor[i]
            );
        }

        // --------------------------------------------------
        // 3. 추가 갈림길 생성
        // --------------------------------------------------

        for (int i = 0; i < currentCount; i++)
        {
            int baseTargetIndex;

            if (currentCount == 1)
            {
                baseTargetIndex =
                    Mathf.RoundToInt(
                        (nextCount - 1) / 2f
                    );
            }
            else
            {
                baseTargetIndex =
                    Mathf.RoundToInt(
                        i * (nextCount - 1f)
                        / (currentCount - 1f)
                    );
            }

            List<int> candidates =
                new List<int>();

            if (baseTargetIndex - 1 >= 0)
            {
                candidates.Add(
                    baseTargetIndex - 1
                );
            }

            if (baseTargetIndex + 1 < nextCount)
            {
                candidates.Add(
                    baseTargetIndex + 1
                );
            }

            if (candidates.Count == 0)
            {
                continue;
            }

            // 추가 연결 확률
            if (UnityEngine.Random.value > 0.65f)
            {
                continue;
            }

            int randomIndex =
                UnityEngine.Random.Range(
                    0,
                    candidates.Count
                );

            int targetIndex =
                candidates[randomIndex];

            MapNode fromNode =
                currentFloor[i];

            MapNode toNode =
                nextFloor[targetIndex];

            if (!HasConnection(fromNode, toNode))
            {
                if (!WouldCrossExistingConnection(
                    fromNode,
                    toNode,
                    currentFloor,
                    nextFloor))
                {
                    AddConnection(
                        fromNode,
                        toNode
                    );
                }
            }
        }
    }

    private bool HasConnection(
    MapNode fromNode,
    MapNode toNode)
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
                if (node == fromNode &&
                    nextNode == toNode)
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


    private bool DoLinesCross(
    Vector2 a,
    Vector2 b,
    Vector2 c,
    Vector2 d)
    {
        float abC =
            Cross(b - a, c - a);

        float abD =
            Cross(b - a, d - a);

        float cdA =
            Cross(d - c, a - c);

        float cdB =
            Cross(d - c, b - c);

        return
            abC * abD < 0f &&
            cdA * cdB < 0f;
    }


    private float Cross(
    Vector2 a,
    Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }


    private void AddConnection(
        MapNode source,
        MapNode target)
    {
        if (source == null || target == null)
            return;

        if (source.nextNodes.Contains(target))
            return;

        source.nextNodes.Add(target);
        target.previousNodes.Add(source);
    }


    // ============================================================
    // CONNECTIVITY CHECK
    // ============================================================

    private void EnsureAllNodesConnected()
    {
        for (int floor = 0; floor < floors.Count - 1; floor++)
        {
            List<MapNode> currentFloor = floors[floor];
            List<MapNode> nextFloor = floors[floor + 1];

            // 다음 노드가 없는 현재 노드
            foreach (MapNode currentNode in currentFloor)
            {
                if (currentNode.nextNodes.Count == 0)
                {
                    MapNode target =
                        nextFloor[
                            UnityEngine.Random.Range(
                                0,
                                nextFloor.Count
                            )
                        ];

                    AddConnection(currentNode, target);
                }
            }

            // 이전 노드가 없는 다음 노드
            foreach (MapNode nextNode in nextFloor)
            {
                if (nextNode.previousNodes.Count == 0)
                {
                    MapNode source =
                        currentFloor[
                            UnityEngine.Random.Range(
                                0,
                                currentFloor.Count
                            )
                        ];

                    AddConnection(source, nextNode);
                }
            }
        }
    }


    // ============================================================
    // NODE TYPE
    // ============================================================

    private void AssignNodeTypes()
    {
        // START
        floors[0][0].type = MapNodeType.Start;

        // Boss
        floors[floorCount][0].type = MapNodeType.Boss;


        for (int floor = 1; floor < floorCount; floor++)
        {
            foreach (MapNode node in floors[floor])
            {
                node.type = GetNodeType(floor);
            }
        }

        // 너무 많은 Elite가 연속으로 나오는 것을 방지
        FixEliteChains();

        // 시작 직후에는 Elite가 나오지 않도록
        FixEarlyElite();
    }


    private MapNodeType GetNodeType(int floor)
    {
        // 초반
        if (floor <= 3)
        {
            float value = UnityEngine.Random.value;

            if (value < 0.70f)
                return MapNodeType.Combat;

            if (value < 0.90f)
                return MapNodeType.Event;

            return MapNodeType.Shop;
        }


        // 중반
        if (floor <= 10)
        {
            float value = UnityEngine.Random.value;

            if (value < 0.45f)
                return MapNodeType.Combat;

            if (value < 0.45f + eliteChance)
                return MapNodeType.Elite;

            if (value < 0.45f + eliteChance + eventChance)
                return MapNodeType.Event;

            if (value < 0.45f + eliteChance + eventChance + shopChance)
                return MapNodeType.Shop;

            return MapNodeType.Rest;
        }


        // 후반
        {
            float value = UnityEngine.Random.value;

            if (value < 0.35f)
                return MapNodeType.Combat;

            if (value < 0.35f + eliteChance + 0.10f)
                return MapNodeType.Elite;

            if (value < 0.35f + eliteChance + 0.10f + eventChance)
                return MapNodeType.Event;

            if (value < 0.35f + eliteChance + 0.10f + eventChance + shopChance)
                return MapNodeType.Shop;

            return MapNodeType.Rest;
        }
    }


    // ============================================================
    // ELITE CONTROL
    // ============================================================

    private void FixEliteChains()
    {
        for (int floor = 2; floor < floorCount; floor++)
        {
            foreach (MapNode node in floors[floor])
            {
                if (node.type != MapNodeType.Elite)
                    continue;

                bool hasElitePrevious = false;

                foreach (MapNode previous in node.previousNodes)
                {
                    if (previous.type == MapNodeType.Elite)
                    {
                        hasElitePrevious = true;
                        break;
                    }
                }

                if (hasElitePrevious)
                {
                    node.type = MapNodeType.Combat;
                }
            }
        }
    }


    private void FixEarlyElite()
    {
        for (int floor = 1; floor <= 3 && floor < floorCount; floor++)
        {
            foreach (MapNode node in floors[floor])
            {
                if (node.type == MapNodeType.Elite)
                {
                    node.type = MapNodeType.Combat;
                }
            }
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
                currentFloor[0].position =
                    new Vector2(
                        floor * horizontalSpacing,
                        0f
                    );

                continue;
            }

            float[] yPositions =
                new float[nodeCount];

            yPositions[0] = 0f;

            for (int i = 1; i < nodeCount; i++)
            {
                float randomSpacing =
                    UnityEngine.Random.Range(
                        0f,
                        verticalRandomness
                    );

                float spacing =
                    verticalSpacing + randomSpacing;

                yPositions[i] =
                    yPositions[i - 1] - spacing;
            }

            float centerOffset =
                (yPositions[0] + yPositions[nodeCount - 1]) / 2f;

            for (int i = 0; i < nodeCount; i++)
            {
                float y =
                    yPositions[i] - centerOffset;

                currentFloor[i].position =
                    new Vector2(
                        floor * horizontalSpacing,
                        y
                    );
            }
        }

        float totalWidth =
            (floors.Count - 1) * horizontalSpacing;

        float offsetX =
            totalWidth / 2f;

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
                    connections +=
                        $"Node {next.id} ";
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
    // PUBLIC API
    // ============================================================

    public List<MapNode> GetFloor(int floor)
    {
        if (floor < 0 || floor >= floors.Count)
            return null;

        return floors[floor];
    }


    public MapNode GetNode(int floor, int index)
    {
        if (floor < 0 || floor >= floors.Count)
            return null;

        if (index < 0 || index >= floors[floor].Count)
            return null;

        return floors[floor][index];
    }


    public void RegenerateMap()
    {
        GenerateMap();
    }


    public MapNode GetStartNode()
    {
        return floors[0][0];
    }


    public MapNode GetBossNode()
    {
        return floors[floors.Count - 1][0];
    }
}