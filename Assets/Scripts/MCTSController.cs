using UnityEngine;
// MCTSController.cs
using System;

public class MCTSController : MonoBehaviour
{
    public void GetBestAction(GameState state, Action<Card> callback)
    {
        callback?.Invoke(null);  // 일단 null 반환
    }
}