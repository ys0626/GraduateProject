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

    [System.NonSerialized] private MapNode currentNode;

    private void Start()
    {
        if (!mapGenerator.IsMapGenerated)
        {
            mapGenerator.GenerateMap();
        }

        CreateMapUI();

        SetStartNode();
    }

    private void CreateMapUI()
    {
        ClearMapUI();

        nodeUIs.Clear();

        foreach (var floor in mapGenerator.Floors)
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
        GameObject nodeObject =
            Instantiate(mapNodePrefab, mapContent);

        RectTransform rectTransform =
            nodeObject.GetComponent<RectTransform>();

        rectTransform.anchoredPosition =
            node.position;

        MapNodeUI nodeUI =
            nodeObject.GetComponent<MapNodeUI>();

        nodeUI.Setup(node);

        nodeUIs.Add(node, nodeUI);

        Button button =
            nodeObject.GetComponent<Button>();

        button.onClick.AddListener(
            () => OnNodeClicked(node)
        );
    }

    private void CreateConnections()
    {
        foreach (var floor in mapGenerator.Floors)
        {
            foreach (MapNode node in floor)
            {
                foreach (MapNode nextNode in node.nextNodes)
                {
                    mapConnectionUI.CreateConnection(
                        node,
                        nextNode
                    );
                }
            }
        }
    }

    private void SetStartNode()
    {
        currentNode =
            mapGenerator.Floors[0][0];

        UpdateAvailableNodes();
    }

    private void OnNodeClicked(MapNode selectedNode)
    {
        currentNode = selectedNode;

        UpdateAvailableNodes();

        switch (selectedNode.type)
        {
            case MapNodeType.Combat:
                SceneChanger.instance.GoToBattleScene();
                break;

            case MapNodeType.Elite:
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

            case MapNodeType.Boss:
                SceneChanger.instance.GoToBattleScene();
                break;
        }
    }

    private void UpdateAvailableNodes()
    {
        foreach (MapNodeUI nodeUI in nodeUIs.Values)
        {
            nodeUI.SetInteractable(false);
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