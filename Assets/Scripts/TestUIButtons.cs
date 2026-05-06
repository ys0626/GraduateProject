using UnityEngine;

public class TestUIButtons : MonoBehaviour
{
    // =====================================================
    // HP
    // =====================================================

    public void IncreaseHP()
    {
        DBTest.instance.CurrentHP += 5;
    }

    public void DecreaseHP()
    {
        DBTest.instance.CurrentHP -= 5;
    }

    // =====================================================
    // Gold
    // =====================================================

    public void IncreaseGold()
    {
        DBTest.instance.Gold += 10;
    }

    public void DecreaseGold()
    {
        DBTest.instance.Gold -= 10;
    }

    // =====================================================
    // Deck Count
    // =====================================================

    public void IncreaseDeckCount()
    {
        DBTest.instance.DeckCount += 1;
    }

    public void DecreaseDeckCount()
    {
        DBTest.instance.DeckCount -= 1;
    }

    // =====================================================
    // Draw Pile
    // =====================================================

    public void IncreaseDrawPile()
    {
        DBTest.instance.DrawPileCount += 1;
    }

    public void DecreaseDrawPile()
    {
        DBTest.instance.DrawPileCount -= 1;
    }

    // =====================================================
    // Discard Pile
    // =====================================================

    public void IncreaseDiscardPile()
    {
        DBTest.instance.DiscardPileCount += 1;
    }

    public void DecreaseDiscardPile()
    {
        DBTest.instance.DiscardPileCount -= 1;
    }

    // =====================================================
    // Exhaust Pile
    // =====================================================

    public void IncreaseExhaustPile()
    {
        DBTest.instance.ExhaustPileCount += 1;
    }

    public void DecreaseExhaustPile()
    {
        DBTest.instance.ExhaustPileCount -= 1;
    }

    // =====================================================
    // Energy
    // =====================================================

    public void IncreaseEnergy()
    {
        DBTest.instance.CurrentEnergy += 1;
    }

    public void DecreaseEnergy()
    {
        DBTest.instance.CurrentEnergy -= 1;
    }

    // =====================================================
    // Max HP
    // =====================================================

    public void IncreaseMaxHP()
    {
        DBTest.instance.MaxHP += 5;
    }

    public void DecreaseMaxHP()
    {
        DBTest.instance.MaxHP -= 5;
    }

    // =====================================================
    // Max Energy
    // =====================================================

    public void IncreaseMaxEnergy()
    {
        DBTest.instance.MaxEnergy += 1;
    }

    public void DecreaseMaxEnergy()
    {
        DBTest.instance.MaxEnergy -= 1;
    }
}