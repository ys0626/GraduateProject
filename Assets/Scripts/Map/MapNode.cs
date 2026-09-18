using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapNode
{
    public int id;
    public int floor;
    public MapNodeType type;

    public Vector2 position;

    public List<MapNode> nextNodes = new List<MapNode>();
    public List<MapNode> previousNodes = new List<MapNode>();

    public bool visited;
    public bool available;

    public MapNode(int id, int floor, MapNodeType type)
    {
        this.id = id;
        this.floor = floor;
        this.type = type;

        visited = false;
        available = false;
    }
}