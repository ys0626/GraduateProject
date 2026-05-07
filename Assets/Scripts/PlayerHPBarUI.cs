using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBarUI : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;

    private void Start()
    {
        hpSlider.maxValue = DBTest.instance.MaxHP;
        hpSlider.value = DBTest.instance.CurrentHP;
    }

    public void RefreshPlayerHPBar()
    {
        hpSlider.value = DBTest.instance.CurrentHP;
    }
}