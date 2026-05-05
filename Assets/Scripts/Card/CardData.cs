using UnityEngine;

[CreateAssetMenu(menuName = "Card/CardData")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;

    [TextArea]
    public string description;

    public Sprite artwork;

    [Header("Card Info")]
    public int cost;

    public CardType cardType;

    public CardRarity rarity;

    public TargetType targetType;

    public CardEffectType cardEffectType;

}