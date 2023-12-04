using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

public static class GameProgress
{
    public static MahjongOwnType CurMahjongProgress;

    public static object ChoiceResult;

    public static bool ChoiceOver;
    
    public static IEnumerator ContinueProgress()
    {
        while (MahjongGameManager.Instance.CurProgressType == MahjongProgressType.Start)
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
            var mahjongItem =
                MahjongGameManager.Instance.MahjongItemList[MahjongGameManager.Instance.MahjongItemList.Count - 1];
            //移除
            MahjongGameManager.Instance.MahjongItemList.RemoveAt(MahjongGameManager.Instance.MahjongItemList.Count - 1);
            //添加进手牌中 赋值临时位置
            mahjongList.Add(mahjongItem);
            mahjongItem.SetData(CurMahjongProgress, true);
            Debug.Log("摸牌结束");

            //TODO 检测 自摸
            yield return CheckHuSelf(CurMahjongProgress);

            Debug.Log("检测自摸结束");
            //TODO 检测 自身暗杠
            yield return CheckHiddenBar(CurMahjongProgress);

            Debug.Log("检测暗杠结束");
            //TODO 出牌
            yield return new WaitForSeconds(1);
            
            yield return HitMahjong(CurMahjongProgress);

            //打出的麻将
            var hitMahjongItem = (MahjongItem)ChoiceResult;
            //检测点炮
            MahjongOwnType nextOneFarmHouse = ((int)CurMahjongProgress + 1) > 4
                ? MahjongOwnType.Own
                : (MahjongOwnType)((int)CurMahjongProgress + 1);
            MahjongOwnType nextTwoFarmHouse =
                ((int)nextOneFarmHouse + 1) > 4 ? MahjongOwnType.Own : (nextOneFarmHouse + 1);
            MahjongOwnType nextThreeFarmHouse =
                ((int)nextTwoFarmHouse + 1) > 4 ? MahjongOwnType.Own : (nextTwoFarmHouse + 1);
            yield return CheckHu((MahjongOwnType)nextOneFarmHouse, hitMahjongItem);
            yield return CheckHu((MahjongOwnType)nextTwoFarmHouse, hitMahjongItem);
            yield return CheckHu((MahjongOwnType)nextThreeFarmHouse, hitMahjongItem);

            //检测所有方 碰 杠
            yield return CheckBePair((MahjongOwnType)nextOneFarmHouse, hitMahjongItem);
            yield return CheckBePair((MahjongOwnType)nextTwoFarmHouse, hitMahjongItem);
            yield return CheckBePair((MahjongOwnType)nextThreeFarmHouse, hitMahjongItem);

            yield return CheckBeBar((MahjongOwnType)nextOneFarmHouse, hitMahjongItem);
            yield return CheckBeBar((MahjongOwnType)nextTwoFarmHouse, hitMahjongItem);
            yield return CheckBeBar((MahjongOwnType)nextThreeFarmHouse, hitMahjongItem);

            //检测 下家吃
            yield return CheckBeOrder((MahjongOwnType)nextOneFarmHouse, hitMahjongItem);

            //出牌完毕 切换
            CurMahjongProgress = (MahjongOwnType)nextOneFarmHouse;
        }
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

    public static bool CheckMahjongListHu(List<MahjongItem> ItemList)
    {
        return false;
    }

    #region 等待UI操作

    private static void ClearData()
    {
        ChoiceOver = false;
        ChoiceResult = null;
    }

    public static IEnumerator WaitUIChoiceHu()
    {
        //显示UI
        UIUtility.LoadUIView<HuUIView>(UIType.HuUI, null);

        ClearData();

        while (!ChoiceOver)
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

        while (!ChoiceOver)
        {
            yield return null;
        }
        
        //隐藏UI
        UIUtility.CloseUIView(UIType.HiddenBar);
    }

    #endregion

    #region 检测暗杠
    


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

    /// <summary>
    /// 检测连续
    /// </summary>
    /// <returns></returns>
    public static List<MahjongItem> HasContinue(List<MahjongItem> mahjongItemList, MahjongItem mahjongItem)
    {
        if (mahjongItem.m_Config.mahjongType == EnumMahjongType.Dragon ||
            mahjongItem.m_Config.mahjongType == EnumMahjongType.Wind)
            return null;
        
        MahjongItem[] frontArray = new MahjongItem[] { mahjongItem, null, null };
        MahjongItem[] middleArray = new MahjongItem[] { null, mahjongItem, null };
        MahjongItem[] afterArray = new MahjongItem[] { null, null, mahjongItem };
        
        for (int i = 0; i < mahjongItemList.Count; i++)
        {
            if (mahjongItemList[i].m_Config.mahjongType == mahjongItem.m_Config.mahjongType)
            {
                if (mahjongItemList[i].m_Config.Id == mahjongItem.m_Config.Id - 2)
                {
                    afterArray[0] = mahjongItemList[i];
                }
                
                if (mahjongItemList[i].m_Config.Id == mahjongItem.m_Config.Id - 1)
                {
                    middleArray[0] = mahjongItemList[i];
                    afterArray[1] = mahjongItemList[i];
                }
                
                if (mahjongItemList[i].m_Config.Id == mahjongItem.m_Config.Id + 2)
                {
                    frontArray[2] = mahjongItemList[i];
                }
                
                if (mahjongItemList[i].m_Config.Id == mahjongItem.m_Config.Id + 1)
                {
                    middleArray[2] = mahjongItemList[i];
                    frontArray[1] = mahjongItemList[i];
                }
            }
        }

        if (frontArray[0] != null && frontArray[1] != null && frontArray[2] != null)
        {
            return new List<MahjongItem>(frontArray);
        }
        
        if (middleArray[0] != null && middleArray[1] != null && middleArray[2] != null)
        {
            return new List<MahjongItem>(middleArray);
        }
        
        if (afterArray[0] != null && afterArray[1] != null && afterArray[2] != null)
        {
            return new List<MahjongItem>(afterArray);
        }

        return null;
    }

    /// <summary>
    /// 检测是否有相同
    /// </summary>
    /// <returns></returns>
    public static List<MahjongItem> HasSame(List<MahjongItem> mahjongList, MahjongItem modelItem, int count)
    {
        List<MahjongItem> resultList = new List<MahjongItem>() { modelItem };
        
        for (int i = 0; i < mahjongList.Count; i++)
        {
            if (mahjongList[i].m_Config.mahjongType == modelItem.m_Config.mahjongType &&
                mahjongList[i].m_Config.Id == modelItem.m_Config.Id)
            {
                resultList.Add(mahjongList[i]);
            }
        }

        return resultList.Count < count ? null : resultList;
    }

    #endregion

    #region 检测胡牌

    /// <summary>
    /// 检测胡牌
    /// </summary>
    /// <param name="belongType"></param>
    /// <returns></returns>
    private static IEnumerator CheckHuSelf(MahjongOwnType belongType)
    {
        yield break;
        
        yield return WaitUIChoiceHu();
    }

    #endregion

    #region 等待出牌

    private static IEnumerator HitMahjong(MahjongOwnType belongType)
    {
        if (belongType == MahjongOwnType.Own)
        {
            ClearData();

            while (!ChoiceOver)
            {
                yield return null;
            }
        }
        else
        {
            HitMahjong(GetCurTurnMahjongList(belongType).First(), belongType);
        }

        yield break;
    }
    static Vector3 selectAdd = new Vector3(0, 0, 3);
    public static void HitMahjongCheck()
    {
        if(ChoiceOver)
            return;
        
        // 检测鼠标点击或触摸
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector3 inputPosition = Input.GetMouseButtonDown(0) ? Input.mousePosition : Input.GetTouch(0).position;

            // 发射射线
            Ray ray = Camera.main.ScreenPointToRay(inputPosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                // 如果点击到了一个对象
                if (hit.collider.tag.Equals("Mahjong"))
                {
                    GameObject clickedObject = hit.collider.gameObject;
                    var mahjongItem = clickedObject.GetComponent<MahjongItem>();
                    if (mahjongItem.m_Data.Own == MahjongOwnType.Own && mahjongItem.m_Data.Lock == MahjongLockType.None)
                    {
                        //TODO 出牌
                        if (ChoiceResult != null)
                        {
                            if (mahjongItem == (MahjongItem)ChoiceResult)
                            {
                                //如果之前已经选中的麻将 则打出
                                HitMahjong(mahjongItem, MahjongOwnType.Own);
                            }
                            else
                            {
                                var temp = (MahjongItem)ChoiceResult;
                                temp.transform.localPosition = new Vector3(temp.transform.localPosition.x, 0, 0);
                                mahjongItem.transform.localPosition += selectAdd;
                                ChoiceResult = mahjongItem;
                            }
                        }
                        else
                        {
                            mahjongItem.transform.localPosition += selectAdd;
                            ChoiceResult = mahjongItem;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 打出麻将
    /// </summary>
    /// <param name="mahjongItem"></param>
    /// <param name="ownType"></param>
    private static void HitMahjong(MahjongItem mahjongItem, MahjongOwnType ownType)
    {
        var mahjongList = GetCurTurnMahjongList(ownType);
        mahjongItem.m_Data.Own = MahjongOwnType.None;
        mahjongList.Remove(mahjongItem);
        
        MahjongGameManager.Instance.SortAndRelocationMahjong(ownType);
        MahjongGameManager.Instance.DiscardMahjongItem(ownType, mahjongItem);
        ChoiceResult = mahjongItem;
        ChoiceOver = true;
    }
    

    #endregion

    #region 检测点炮
    

    /// <summary>
    /// 检测胡牌
    /// </summary>
    /// <param name="belongType"></param>
    /// <param name="mahjongItem"></param>
    /// <returns></returns>
    private static IEnumerator CheckHu(MahjongOwnType belongType, MahjongItem mahjongItem)
    {
        var TempMahjongList = new List<MahjongItem>(GetCurTurnMahjongList(belongType));
        TempMahjongList.Add(mahjongItem);
        bool result = CheckMahjongListHu(TempMahjongList);
        ChoiceResult = result;
        yield break;
    }

    #endregion

    #region 检测被碰、被杠
    
    /// <summary>
    /// 检测暗杠
    /// </summary>
    /// <param name="belongType"></param>
    /// <returns></returns>
    private static IEnumerator CheckHiddenBar(MahjongOwnType belongType)
    {
        var mahjongList = GetCurTurnMahjongList(belongType);
        var result = HasIdentical(mahjongList, 4);
        if (result != null)
        {
            //等待UI操作
            yield return WaitUIChoiceHiddenBar();
        }
        
        
        yield break;
    }

    private static IEnumerator CheckBePair(MahjongOwnType belongType, MahjongItem mahjongItem)
    {
        List<MahjongItem> CurMahjongList = GetCurTurnMahjongList(belongType);
        var result = HasSame(CurMahjongList, mahjongItem, 3);
        if (result != null)
        {
            //有碰
            MahjongGameManager.Instance.AddLockMahjongList(belongType, result, MahjongLockType.Pair);
        }
        
        
        yield break;
    }

    private static IEnumerator CheckBeBar(MahjongOwnType belongType, MahjongItem mahjongItem)
    {
        List<MahjongItem> CurMahjongList = GetCurTurnMahjongList(belongType);
        var result = HasSame(CurMahjongList, mahjongItem, 4);
        if (result != null)
        {
            //有杠
            MahjongGameManager.Instance.AddLockMahjongList(belongType, result, MahjongLockType.Bar);
        }
        
        yield break;
    }

    #endregion

    #region 检测被吃

    private static IEnumerator CheckBeOrder(MahjongOwnType belongType, MahjongItem mahjongItem)
    {
        var result = HasContinue(GetCurTurnMahjongList(belongType), mahjongItem);
        if (result != null)
        {
            //有吃
            MahjongGameManager.Instance.AddLockMahjongList(belongType, result, MahjongLockType.Order);
        }
        
        yield break;
    }

    #endregion
} 
    