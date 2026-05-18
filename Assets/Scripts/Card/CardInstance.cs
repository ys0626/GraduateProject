
using UnityEditor;

/// <summary>
/// 실제 런타임 중 게임에서 사용되는 카드 객체
/// </summary>
[System.Serializable]
public class CardInstance
{
    // =====================================================
    // 카드 인스턴스의 고유 ID
    // =====================================================

    private static int nextID = 0;

    public int instanceID;

    // =====================================================
    // 카드 정보
    // =====================================================

    public CardData data;

    public int currentCost;

    //강화 여부
    public bool upgraded;
    
    //사용 시 소멸되는 지 
    public bool exhaust;

    //휘발성인 지 
    public bool ethereal;

    // =====================================================
    // 생성자
    // =====================================================

    public CardInstance(CardData cardData)
    {
        instanceID = nextID++;  //카드 인스턴스에 고유 ID 발급

        data = cardData;

        currentCost = data.cost;

        upgraded = false;

        exhaust = false;

        ethereal = false;
    }

    /// <summary>
    /// CardInstance의 복사본을 생성
    /// </summary>
    /// <param name="original"></param>
    public CardInstance(CardInstance original)
    {
        instanceID = original.instanceID;
        data = original.data;
        currentCost = original.currentCost;
        upgraded = original.upgraded;
        exhaust = original.exhaust;
        ethereal = original.ethereal;
    }



    // =====================================================
    // MCTS에서 사용
    // =====================================================
    public CardInstance Clone()
    {
        return new CardInstance(this);
    }
}