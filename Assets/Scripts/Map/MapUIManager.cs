using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private GameObject mapNodePrefab;
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private MapConnectionUI mapConnectionUI;

    private Dictionary<MapNode, MapNodeUI> nodeUIs =
        new Dictionary<MapNode, MapNodeUI>();

    private MapNode currentNode;

    private void Start()
    {
        mapGenerator = MapGenerator.instance;

        if (mapGenerator == null)
        {
            Debug.LogError("MapGenerator 인스턴스를 찾을 수 없습니다.");
            return;
        }

        if (!mapGenerator.IsMapGenerated)
        {
            mapGenerator.GenerateMap();
        }

        CreateMapUI();
        RestoreCurrentNode();
    }

    private void CreateMapUI()
    {
        ClearMapUI();
        nodeUIs.Clear();

        foreach (List<MapNode> floor in mapGenerator.Floors)
        {
            foreach (MapNode node in floor)
            {
                CreateNodeUI(node);
            }
        }

        CreateConnections();
    }

    private void CreateNodeUI(MapNode node)
    {
        GameObject nodeObject = Instantiate(
            mapNodePrefab,
            mapContent
        );

        RectTransform rectTransform =
            nodeObject.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = node.position;

        MapNodeUI nodeUI =
            nodeObject.GetComponent<MapNodeUI>();

        nodeUI.Setup(node);
        nodeUIs.Add(node, nodeUI);

        Button button = nodeObject.GetComponent<Button>();

        button.onClick.AddListener(
            () => OnNodeClicked(node)
        );
    }

    private void CreateConnections()
    {
        foreach (List<MapNode> floor in mapGenerator.Floors)
        {
            foreach (MapNode node in floor)
            {
                foreach (MapNode nextNode in node.nextNodes)
                {
                    mapConnectionUI.CreateConnection(node, nextNode);
                }
            }
        }
    }

    private void RestoreCurrentNode()
    {
        currentNode = mapGenerator.GetNodeById(
            mapGenerator.CurrentNodeId
        );

        if (currentNode == null)
        {
            currentNode = mapGenerator.GetStartNode();

            if (currentNode == null)
            {
                Debug.LogError("시작 노드를 찾을 수 없습니다.");
                return;
            }

            mapGenerator.SetCurrentNode(currentNode);
        }

        UpdateAvailableNodes();
    }

    private void OnNodeClicked(MapNode selectedNode)
    {
        if (selectedNode == null || currentNode == null)
            return;

        if (!currentNode.nextNodes.Contains(selectedNode))
            return;

        // 현재 노드와 진행 상태 저장
        currentNode = selectedNode;
        selectedNode.visited = true;
        mapGenerator.SetCurrentNode(selectedNode);

        // 여기서는 UpdateAvailableNodes()를 호출하지 않음
        // 바로 해당 노드의 씬으로 이동
        switch (selectedNode.type)
        {
            case MapNodeType.Combat:
                GameData.instance.CurrentBattleType = BattleType.Normal;
                SceneChanger.instance.GoToBattleScene();
                break;

            case MapNodeType.Elite:
                GameData.instance.CurrentBattleType = BattleType.Elite;
                SceneChanger.instance.GoToBattleScene();
                break;

            case MapNodeType.Boss:
                GameData.instance.CurrentBattleType = BattleType.Boss;
                SceneChanger.instance.GoToBattleScene();
                break;

            case MapNodeType.Event:
                SceneChanger.instance.GoToEventScene();
                break;

            case MapNodeType.Shop:
                SceneChanger.instance.GoToShopScene();
                break;

            case MapNodeType.Rest:
                SceneChanger.instance.GoToRestScene();
                break;
        }
    }

    private void UpdateAvailableNodes()
    {
        foreach (MapNodeUI nodeUI in nodeUIs.Values)
        {
            nodeUI.SetInteractable(false);
        }

        if (currentNode == null)
        {
            return;
        }

        foreach (MapNode nextNode in currentNode.nextNodes)
        {
            if (nodeUIs.ContainsKey(nextNode))
            {
                nodeUIs[nextNode].SetInteractable(true);
            }
        }
    }

    private void ClearMapUI()
    {
        for (int i = mapContent.childCount - 1; i >= 0; i--)
        {
            Transform child = mapContent.GetChild(i);

            if (child.name == "ConnectionLayer")
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }
}