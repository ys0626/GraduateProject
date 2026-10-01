using UnityEngine;

[CreateAssetMenu(
    fileName = "NewEnemyData",
    menuName = "Enemy/Enemy Data"
)]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;

    [Header("Visual")]
    public Sprite sprite;
    public RuntimeAnimatorController animatorController;

    [Header("Stats")]
    public int maxHP;
    public int maxEnergy;

    [Header("Deck")]
    public CardData[] starterDeck;
}