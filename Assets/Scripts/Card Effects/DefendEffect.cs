using UnityEngine;

public class DefendEffect : ICardEffect
{
    private CardInstance card;

    public DefendEffect(CardInstance card)
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
        int block = 5;
        Debug.Log($"Gain {block} block");
    }
}