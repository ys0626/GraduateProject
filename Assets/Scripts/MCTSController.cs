using UnityEngine;

using System;

public class MCTSController : MonoBehaviour
{
    public void GetBestAction(GameState state, Action<CardDatabase.CardEntry> callback)
    {
        callback?.Invoke(null);  // 일단 null 반환
    }
}