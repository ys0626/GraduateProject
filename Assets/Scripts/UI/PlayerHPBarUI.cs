using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBarUI : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;

    public void RefreshPlayerHPBar()
    {
        hpSlider.maxValue = GameData.instance.player.MaxHP;
        hpSlider.value = GameData.instance.player.CurrentHP;
    }
}