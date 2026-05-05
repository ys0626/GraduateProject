using UnityEngine;

public class StrikeEffect : ICardEffect
{
    private CardInstance card;

    public StrikeEffect(CardInstance card)
    {
        this.card = card;
    }

    public void Execute()
    {
        if (DBTest.instance.CurrentEnergy < card.currentCost)
        { 
            Debug.Log("에너지가 부족합니다");
            return;
        }
        DBTest.instance.CurrentEnergy -= card.currentCost;
        int damage = 6;
        Debug.Log($"Deal {damage} damage");
        DBTest.instance.EnemyCurrentHP -= damage;
    }
}