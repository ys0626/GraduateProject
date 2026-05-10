using System;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

/// <summary>
/// 런 타임 중 게임의 값들을 저장하는 class
/// </summary>
public class GameData : MonoBehaviour
{
    public static GameData instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
    }

    /// <summary>
    /// GameManger에서 세팅한 테스트값들을 제외한 기본적인 값들 초기화
    /// </summary>
    public void Init()
    {
        player.Gold = 0;
        player.Block = 0;
        enemy.Block = 0;
        player.DeckCount = player.deck.Count;
        player.DrawPileCount = player.drawPile.Count;
        player.DiscardPileCount = player.discardPile.Count;
        player.ExhaustPileCount = player.exhaustPile.Count;
        

        player.CurrentHP = player.MaxHP;
        player.CurrentEnergy = player.MaxEnergy;

        enemy.CurrentHP = enemy.MaxHP;
        enemy.CurrentEnergy = enemy.MaxEnergy;
    }


    // =====================================================
    // 플레이어와 적의 정보
    // =====================================================
    public Entity player = new Entity();
    public Entity enemy = new Entity();


}