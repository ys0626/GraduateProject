using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] CardData[] starterDeck;
    private void Start()
    {
        GetStarterDeck();
    }

    /// <summary>
    /// 덱에 기본적으로 타격 5장, 수비 5장 추가
    /// </summary>
    private void GetStarterDeck()
    {
        foreach (CardData cardData in starterDeck)
        {
            DeckManager.instance.AddCardToPlayerDeck(cardData);
        }
    }
}