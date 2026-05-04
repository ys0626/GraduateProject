using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    Attack,
    Skill,
    Power
}

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Scriptable Objects/CardDatabase")]
public class CardDatabase : ScriptableObject
{
    public List<CardEntry> cards;

    [System.Serializable]
    public class CardEntry
    {
        public string cardId;
        public string cardName;
        public int cost;
        public CardType cardType;
        [TextArea] public string description;
    }

    public CardEntry GetCard(string id)
        => cards.Find(c => c.cardId == id);
}