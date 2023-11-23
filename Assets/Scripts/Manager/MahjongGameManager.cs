using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MahjongGameManager : MonoBehaviour
{
    private static MahjongGameManager Instance;
    
    public void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
    }

    private void Start()
    {
        
    }
    
    /// <summary>
    /// 游戏开始
    /// </summary>
    private void GameStart(MahjongRule mahjongRule)
    {
        switch (mahjongRule)
        {
            case MahjongRule.ShenYang:
            {
                
                break;
            }
        }
        
        //TODO 创建所有麻将牌
        for (int i = 0; i < GameDefine.TotalItemCount; i++)
        {
            
        }
    }
}
