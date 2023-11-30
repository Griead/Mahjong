using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GameProgress
{
    public static MahjongOwnType CurMahjongProgress;

    public static object UIChoiceResult;

    public static bool UIChoiceOver;
    
    public static IEnumerator ContinueProgress()
    {
        //检测是否流局
        if (MahjongGameManager.Instance.MahjongItemList.Count <= 0)
        {
            MahjongGameManager.Instance.CurProgressType = MahjongProgressType.End;
            yield break;
        }
        
        Debug.Log("检测流局结束");
        
        //摸牌
        var mahjongList = GetCurTurnMahjongList(CurMahjongProgress);
        var mahjongItem = MahjongGameManager.Instance.MahjongItemList[MahjongGameManager.Instance.MahjongItemList.Count - 1];
        //移除
        MahjongGameManager.Instance.MahjongItemList.RemoveAt(MahjongGameManager.Instance.MahjongItemList.Count - 1);
        //添加进手牌中 赋值临时位置
        mahjongList.Add(mahjongItem);
        mahjongItem.SetData(CurMahjongProgress, true);
        Debug.Log("摸牌结束");
        
        //TODO 检测 自摸
        yield return CheckHu(CurMahjongProgress);
        
        Debug.Log("检测自摸结束");
        //TODO 检测 自身暗杠
        yield return CheckHiddenBar(CurMahjongProgress);
        
        Debug.Log("检测暗杠结束");
        //TODO 出牌


        //检测点炮

        //检测所有方 碰 杠

        //检测 下家吃

        //出牌完毕 切换
    }

    /// <summary>
    /// 获取当前轮次麻将列表
    /// </summary>
    /// <returns></returns>
    public static List<MahjongItem> GetCurTurnMahjongList(MahjongOwnType belongType)
    {
        switch (belongType)
        {
            case MahjongOwnType.Own:
               return MahjongGameManager.Instance.OwnMahjongList;
            case MahjongOwnType.Left:
                return MahjongGameManager.Instance.LeftMahjongList;
            case MahjongOwnType.Oppo:
                return MahjongGameManager.Instance.OppoMahjongList;
            case MahjongOwnType.Right:
                return MahjongGameManager.Instance.RightMahjongList;
        }
        return null;
    }

    #region 等待UI操作

    private static void ClearData()
    {
        UIChoiceOver = false;
        UIChoiceResult = null;
    }

    public static IEnumerator WaitUIChoiceHu()
    {
        //显示UI
        UIUtility.LoadUIView<HuUIView>(UIType.HuUI, null);

        ClearData();

        while (!UIChoiceOver)
        {
            yield return null;
        }
        
        //隐藏UI
        UIUtility.CloseUIView(UIType.HuUI);
    }
    
    public static IEnumerator WaitUIChoiceHiddenBar()
    {
        //显示UI
        UIUtility.LoadUIView<HuUIView>(UIType.HiddenBar, null);

        ClearData();

        while (!UIChoiceOver)
        {
            yield return null;
        }
        
        //隐藏UI
        UIUtility.CloseUIView(UIType.HiddenBar);
    }

    #endregion

    #region 检测暗杠
    
    /// <summary>
    /// 检测暗杠
    /// </summary>
    /// <param name="belongType"></param>
    /// <returns></returns>
    private static IEnumerator CheckHiddenBar(MahjongOwnType belongType)
    {
        var mahjongList = GetCurTurnMahjongList(belongType);
        var result = HasIdentical(mahjongList, 4);
        if (result is null)
        {
            yield break;
        }
        else
        {
            //等待UI操作
            yield return WaitUIChoiceHiddenBar();
        }
    }

    public static List<MahjongItem> HasIdentical(List<MahjongItem> mahjongItemList, int count)
    {        
        var identicalGroups = mahjongItemList.GroupBy(m => new { m.m_Config.mahjongType, m.m_Config.Id })
            .Where(g => g.Count() >= count);

        var enumerable = identicalGroups.ToList();
        if (enumerable.Any())
        {
            // 返回第一个符合条件的组的所有元素
            return enumerable.First().ToList();
        }
        else
        {
            // 没有找到符合条件的组，返回空列表
            return null;
        }
        
    }

    #endregion

    #region 检测胡牌

    /// <summary>
    /// 检测胡牌
    /// </summary>
    /// <param name="belongType"></param>
    /// <returns></returns>
    private static IEnumerator CheckHu(MahjongOwnType belongType)
    {
        yield break;
        
        yield return WaitUIChoiceHu();
    }

    #endregion

    #region 等待出牌

    private static IEnumerator HitMahjong()
    {
        //TODO 将每个行为单独拆分为同的脚本
        yield break;
    }
    

    #endregion
} 
    