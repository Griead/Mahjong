using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;

public static class GameProgress
{
    public static MahjongOwnType CurMahjongProgress;

    public static object ChoiceResult;

    public static bool ChoiceOver;

    public static bool SkipTouchMahjong = false;
    
    public static IEnumerator ContinueProgress()
    {
        while (MahjongGameManager.Instance.CurProgressType == MahjongProgressType.Start)
        {
            //检测是否流局
            if (MahjongGameManager.Instance.MahjongItemList.Count <= 0)
            {
                MahjongGameManager.Instance.OverProgress(MahjongOwnType.None, null);
                yield break;
            }

            if (!SkipTouchMahjong)
            {
                //摸牌
                var mahjongList = GetCurTurnMahjongList(CurMahjongProgress);
                var mahjongItem =
                    MahjongGameManager.Instance.MahjongItemList[MahjongGameManager.Instance.MahjongItemList.Count - 1];
                //移除
                MahjongGameManager.Instance.MahjongItemList.RemoveAt(MahjongGameManager.Instance.MahjongItemList.Count - 1);
                //添加进手牌中 赋值临时位置
                mahjongList.Add(mahjongItem);
                mahjongItem.SetData(CurMahjongProgress, true);

                //检测自摸
                yield return CheckHuSelf(CurMahjongProgress);
                if (CheckMahjongOperate(CurMahjongProgress, null, true))
                    continue;

                //检测自身暗杠
                yield return CheckHiddenBar(CurMahjongProgress);
                if (CheckMahjongOperate(CurMahjongProgress, null, false))
                    continue;

                //检测添加杠
                yield return CheckAddBar(CurMahjongProgress, mahjongItem);
                if (CheckMahjongOperate(CurMahjongProgress, null, false))
                    continue;
            }

            if (CurMahjongProgress != MahjongOwnType.Own)
                yield return new WaitForSeconds(1);
            
            SkipTouchMahjong = false;
            //出牌
            yield return HitMahjong(CurMahjongProgress);
            
            //打出的麻将
            var hitMahjongItem = (MahjongItem)ChoiceResult;

            //音效
            MahjongGameManager.Instance.PlayMahjongAudio(CurMahjongProgress, hitMahjongItem.m_Config);
            
            yield return new WaitForSeconds(1);
            
            //检测点炮
            MahjongOwnType nextOneFarmHouse = ((int)CurMahjongProgress + 1) > 4
                ? MahjongOwnType.Own
                : (MahjongOwnType)((int)CurMahjongProgress + 1);
            MahjongOwnType nextTwoFarmHouse =
                ((int)nextOneFarmHouse + 1) > 4 ? MahjongOwnType.Own : (nextOneFarmHouse + 1);
            MahjongOwnType nextThreeFarmHouse =
                ((int)nextTwoFarmHouse + 1) > 4 ? MahjongOwnType.Own : (nextTwoFarmHouse + 1);
            
            //下家
            yield return CheckHu((MahjongOwnType)nextOneFarmHouse, hitMahjongItem);            
            if (CheckMahjongOperate(nextOneFarmHouse, hitMahjongItem, true))
                continue;
            
            //对家
            yield return CheckHu((MahjongOwnType)nextTwoFarmHouse, hitMahjongItem);
            if (CheckMahjongOperate(nextTwoFarmHouse, hitMahjongItem, true))
                continue;
            
            //上家
            yield return CheckHu((MahjongOwnType)nextThreeFarmHouse, hitMahjongItem);
            if (CheckMahjongOperate(nextThreeFarmHouse, hitMahjongItem, true))
                continue;

            //检测所有方 碰 杠
            
            //下家
            yield return CheckBePair((MahjongOwnType)nextOneFarmHouse, hitMahjongItem);
            if (CheckMahjongOperate(nextOneFarmHouse, hitMahjongItem, true))
                continue;
            yield return CheckBeBar((MahjongOwnType)nextOneFarmHouse, hitMahjongItem);
            if (CheckMahjongOperate(nextOneFarmHouse, hitMahjongItem, false))
                continue;
            
            //对家
            yield return CheckBePair((MahjongOwnType)nextTwoFarmHouse, hitMahjongItem);
            if (CheckMahjongOperate(nextTwoFarmHouse, hitMahjongItem, true))
                continue;
            yield return CheckBeBar((MahjongOwnType)nextTwoFarmHouse, hitMahjongItem);
            if (CheckMahjongOperate(nextTwoFarmHouse, hitMahjongItem, false))
                continue;
            
            //上家
            yield return CheckBePair((MahjongOwnType)nextThreeFarmHouse, hitMahjongItem);
            if (CheckMahjongOperate(nextThreeFarmHouse, hitMahjongItem, true))
                continue;
            yield return CheckBeBar((MahjongOwnType)nextThreeFarmHouse, hitMahjongItem);
            if (CheckMahjongOperate(nextThreeFarmHouse, hitMahjongItem, false))
                continue;

            //检测 下家吃
            yield return CheckBeOrder((MahjongOwnType)nextOneFarmHouse, hitMahjongItem);
            if (CheckMahjongOperate(nextOneFarmHouse, hitMahjongItem, true))
                continue;

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

    /// <summary>
    /// 检测麻将操作
    /// </summary>
    /// <returns></returns>
    public static bool CheckMahjongOperate(MahjongOwnType nextOperateType, MahjongItem hitMahjongItem, bool needSkipTouchMahjong)
    {
        if ((bool)ChoiceResult)
        {
            SkipTouchMahjong = needSkipTouchMahjong;
            MahjongGameManager.Instance.RemoveDiscardMahjongItem(CurMahjongProgress, hitMahjongItem);
            CurMahjongProgress = nextOperateType;
            return true;
        }

        return false;
    }

    public static IEnumerator WaitTime()
    {
        if (CurMahjongProgress != MahjongOwnType.Own)
            yield return new WaitForSeconds(1);
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
        UIUtility.LoadUIView<CommonUIView>(UIType.HuUI, null);

        ClearData();

        while (!ChoiceOver)
        {
            yield return null;
        }
        
        //隐藏UI
        UIUtility.CloseUIView(UIType.HuUI);
    }
    


    #endregion

    #region 检测麻将牌
    

    /// <summary>
    /// 检测暗杠
    /// </summary>
    /// <param name="mahjongItemList"></param>
    /// <param name="count"></param>
    /// <returns></returns>
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
        if(ChoiceOver || MahjongGameManager.Instance.CurProgressType != MahjongProgressType.Start)
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

    #region 检测自摸

    /// <summary>
    /// 检测胡牌
    /// </summary>
    /// <param name="belongType"></param>
    /// <returns></returns>
    private static IEnumerator CheckHuSelf(MahjongOwnType belongType)
    {
        yield return CheckHu(belongType,null);
    }

    #endregion
    
    #region 检测点炮
    
    /// <summary>
    /// 检测是否准备胡牌
    /// </summary>
    /// <param name="belongType"></param>
    /// <returns></returns>
    public static bool CheckHuResult(MahjongOwnType belongType, MahjongItem addMahjong)
    {
        List<MahjongLockData> mahjongLockDataList = new List<MahjongLockData>();
        
        var curMahjongList = new List<MahjongItem>(GetCurTurnMahjongList(belongType));
        
        // 是否有新出的牌
        if(addMahjong != null)
            curMahjongList.Add(addMahjong);
        
        mahjongLockDataList = MahjongGameManager.Instance.GetCurLockMahjongData(belongType);

        //有1/9/风/中发白
        bool haveOneOrNineOrWindOrDragon = false;

        //有万
        bool haveCharacters = false;

        //有条
        bool haveBamboo = false;

        //有饼
        bool haveDot = false;

        //有碰
        bool havePair = false;
        
        int lockDataListCount = mahjongLockDataList?.Count ?? 0;
        if (lockDataListCount > 0)//是否开门
        {
            AnalysisMahjongLockData(mahjongLockDataList, ref haveOneOrNineOrWindOrDragon, ref haveCharacters, ref haveBamboo, ref haveDot, ref havePair);
    
            //手牌的临时锁定数据
            List<MahjongLockData> tempLockData = new List<MahjongLockData>();
            //遍历直到没有碰
            bool continuesCheckPair = true;
            while (continuesCheckPair)
            {
               var mahjongList = HasIdentical(curMahjongList, 3);
               if (mahjongList is null)
               {
                   continuesCheckPair = false;
               }
               else
               {
                   tempLockData.Add(new MahjongLockData() { LockType = MahjongLockType.Pair, MahjongList = mahjongList, OwnType = belongType});
                   //移除已经处理过的数据
                   for (int i = 0; i < mahjongList.Count; i++)
                   {
                       curMahjongList.Remove(mahjongList[i]); 
                   }
               }
            }
            
            //遍历直到没有吃
            bool continuesCheckOrder = true;
            while (continuesCheckOrder)
            {
                for (int i = 0; i < curMahjongList.Count; i++)
                {
                    var mahjongList = HasContinue(curMahjongList, curMahjongList[i]);

                    if (mahjongList == null)
                    {
                        continuesCheckOrder = false;
                    }
                    else
                    {
                        tempLockData.Add(new MahjongLockData() {LockType = MahjongLockType.Order, MahjongList = mahjongList, OwnType = belongType});
                        //移除已经处理过的数据
                        for (int j = 0; j < mahjongList.Count; j++)
                        {
                            curMahjongList.Remove(mahjongList[j]); 
                        }
                    }
                }
            }
            
            //检测是否剩余麻将中含有一对的
            var residueMahjongList = HasIdentical(curMahjongList, 2);
            if (residueMahjongList != null)
            {                
                tempLockData.Add(new MahjongLockData() { LockType = MahjongLockType.Pair, MahjongList = residueMahjongList, OwnType = belongType});
                //移除已经处理过的数据
                for (int i = 0; i < residueMahjongList.Count; i++)
                {
                    curMahjongList.Remove(residueMahjongList[i]); 
                }
            }
            
            //处理手牌的临时数据和检测剩余数据是否满足听牌
            AnalysisMahjongLockData(tempLockData, ref haveOneOrNineOrWindOrDragon, ref haveCharacters, ref haveBamboo, ref haveDot, ref havePair);

            if (curMahjongList.Count <= 0)
            {
                return haveOneOrNineOrWindOrDragon && haveCharacters && haveBamboo && haveDot && havePair;
            }
            else
            {
                return false;
            }

        }
        else
        {
            return false;
        }
    }

    private static void AnalysisMahjongLockData(List<MahjongLockData> mahjongLockDataList,
        ref bool haveOneOrNineOrWindOrDragon,
        ref bool haveCharacters,
        ref bool haveBamboo,
        ref bool haveDot,
        ref bool havePair
    )
    {
        for (int i = 0; i < mahjongLockDataList.Count; i++)
        {
            switch (mahjongLockDataList[i].LockType)
            {
                case MahjongLockType.Bar:
                case MahjongLockType.HiddenBar:
                case MahjongLockType.Pair:
                {
                    havePair = true;
                    switch (mahjongLockDataList[i].MahjongList[0].m_Config.mahjongType)
                    {
                        case EnumMahjongType.Characters:
                        {
                            haveCharacters = true;
                            if (mahjongLockDataList[i].MahjongList[0].m_Config.Id == 1 ||
                                mahjongLockDataList[i].MahjongList[0].m_Config.Id == 9)
                            {
                                haveOneOrNineOrWindOrDragon = true;
                            }

                            break;
                        }
                        case EnumMahjongType.Bamboo:
                        {
                            haveBamboo = true;
                            if (mahjongLockDataList[i].MahjongList[0].m_Config.Id == 1 ||
                                mahjongLockDataList[i].MahjongList[0].m_Config.Id == 9)
                            {
                                haveOneOrNineOrWindOrDragon = true;
                            }

                            break;
                        }
                        case EnumMahjongType.Dot:
                        {
                            haveDot = true;
                            if (mahjongLockDataList[i].MahjongList[0].m_Config.Id == 1 ||
                                mahjongLockDataList[i].MahjongList[0].m_Config.Id == 9)
                            {
                                haveOneOrNineOrWindOrDragon = true;
                            }

                            break;
                        }
                        case EnumMahjongType.Dragon:
                        case EnumMahjongType.Wind:
                        {
                            haveOneOrNineOrWindOrDragon = true;
                            break;
                        }
                    }

                    break;
                }
                case MahjongLockType.Order:
                {
                    switch (mahjongLockDataList[i].MahjongList[0].m_Config.mahjongType)
                    {
                        case EnumMahjongType.Characters:
                        {
                            haveCharacters = true;
                            break;
                        }
                        case EnumMahjongType.Bamboo:
                        {
                            haveBamboo = true;
                            break;
                        }
                        case EnumMahjongType.Dot:
                        {
                            haveDot = true;
                            break;
                        }
                    }

                    for (int j = 0; j < mahjongLockDataList[i].MahjongList.Count; j++)
                    {
                        if (mahjongLockDataList[i].MahjongList[j].m_Config.Id == 1 ||
                            mahjongLockDataList[i].MahjongList[j].m_Config.Id == 9)
                        {
                            haveOneOrNineOrWindOrDragon = true;
                        }
                    }

                    break;
                }
            }
        }
    }
    

    /// <summary>
    /// 检测胡牌
    /// </summary>
    /// <param name="belongType"></param>
    /// <param name="mahjongItem"></param>
    /// <returns></returns>
    private static IEnumerator CheckHu(MahjongOwnType belongType, MahjongItem mahjongItem)
    {
        bool result = CheckHuResult(belongType, mahjongItem);
        
        if (result)
        {
            //等待UI操作
            if(belongType == MahjongOwnType.Own)
                yield return WaitUIChoiceHu(belongType, mahjongItem);
            else
            {
                //胡牌逻辑
                MahjongGameManager.Instance.OverProgress(belongType, mahjongItem);
            }
        }
        ChoiceResult = result;
    }
    
    /// <summary>
    /// 等待UI选择胡
    /// </summary>
    /// <param name="belongType"></param>
    /// <param name="mahjongItem"></param>
    /// <returns></returns>
    private static IEnumerator WaitUIChoiceHu(MahjongOwnType belongType, MahjongItem mahjongItem)
    {
        //显示UI
        UIUtility.LoadUIView<CommonUIView>(UIType.HuUI, null);

        ClearData();

        while (!ChoiceOver)
        {
            yield return null;
        }

        if ((bool)ChoiceResult)
        {
            MahjongGameManager.Instance.OverProgress(belongType, mahjongItem);
        }
        
        //隐藏UI
        UIUtility.CloseUIView(UIType.HuUI);
    }

    #endregion
    
    #region 检测加杠
    
    /// <summary>
    /// 检测暗杠
    /// </summary>
    /// <returns></returns>
    private static IEnumerator CheckAddBar(MahjongOwnType belongType, MahjongItem mahjongItem)
    {
        MahjongLockData lockData = null;
        var mahjongLockDataList = MahjongGameManager.Instance.GetCurLockMahjongData(belongType);
        int mahjongLockCount = mahjongLockDataList?.Count ?? 0;
        int Index = 0;
        for (int i = 0; i < mahjongLockCount; i++)
        {
            if (mahjongLockDataList[i].LockType == MahjongLockType.Pair)
            {
                if (mahjongLockDataList[i].MahjongList[0].m_Config == mahjongItem.m_Config)
                {
                    Index = i;
                    lockData = mahjongLockDataList[i];
                    break;
                }
            }
        }

        bool isResult = lockData != null;
        
        if (isResult)
        {
            var result = new List<MahjongItem>();
            result.AddRange(lockData.MahjongList);
            result.Add(mahjongItem);
            //等待UI操作
            if(belongType == MahjongOwnType.Own)
                yield return WaitUIChoiceAddBar(belongType, result, lockData, Index);
            else
            {
                //执行碰转杠
                MahjongGameManager.Instance.ChangeLockMahjongDataPairToBar(belongType, result, lockData, Index);
            }
        }

        ChoiceResult = isResult;
    }
    
    public static IEnumerator WaitUIChoiceAddBar(MahjongOwnType belongType, List<MahjongItem> mahjongList, MahjongLockData lockData, int index)
    {
        //显示UI
        UIUtility.LoadUIView<CommonUIView>(UIType.HiddenBar, null);

        ClearData();

        while (!ChoiceOver)
        {
            yield return null;
        }

        if ((bool)ChoiceResult)
        {
            //执行碰转杠
            MahjongGameManager.Instance.ChangeLockMahjongDataPairToBar(belongType, mahjongList, lockData, index);
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
        bool isResult = result != null;
        
        if (isResult)
        {
            //等待UI操作
            if(belongType == MahjongOwnType.Own)
                yield return WaitUIChoiceHiddenBar(result);
            else
                //执行暗杠
                MahjongGameManager.Instance.AddLockMahjongList(belongType, mahjongList, MahjongLockType.HiddenBar);

        }
        
        ChoiceResult = isResult;
    }
    
    public static IEnumerator WaitUIChoiceHiddenBar(List<MahjongItem> mahjongList)
    {
        //显示UI
        UIUtility.LoadUIView<CommonUIView>(UIType.HiddenBar, null);

        ClearData();

        while (!ChoiceOver)
        {
            yield return null;
        }

        if ((bool)ChoiceResult)
        {
            //执行暗杠
            MahjongGameManager.Instance.AddLockMahjongList(CurMahjongProgress, mahjongList, MahjongLockType.HiddenBar);
        }
        
        //隐藏UI
        UIUtility.CloseUIView(UIType.HiddenBar);
    }

    #endregion

    #region 检测被碰
    
    /// <summary>
    /// 检测碰
    /// </summary>
    /// <param name="belongType"></param>
    /// <param name="mahjongItem"></param>
    /// <returns></returns>
    private static IEnumerator CheckBePair(MahjongOwnType belongType, MahjongItem mahjongItem)
    {
        List<MahjongItem> CurMahjongList = GetCurTurnMahjongList(belongType);
        var result = HasSame(CurMahjongList, mahjongItem, 3);
        if (result != null)
        {
            //有碰
            if(belongType == MahjongOwnType.Own)
                yield return WaitUIBePair(belongType, result);
            else
            {
                ChoiceResult = true;
                MahjongGameManager.Instance.AddLockMahjongList(belongType, result, MahjongLockType.Pair);
            }
                
        }
        yield break;
    }
    
    public static IEnumerator WaitUIBePair(MahjongOwnType belongType, List<MahjongItem> mahjongList)
    {
        //显示UI
        UIUtility.LoadUIView<CommonUIView>(UIType.Pair, null);

        ClearData();

        while (!ChoiceOver)
        {
            yield return null;
        }

        if ((bool)ChoiceResult)
        {
            //执行被碰
            MahjongGameManager.Instance.AddLockMahjongList(belongType, mahjongList, MahjongLockType.Pair);
        }
        
        //隐藏UI
        UIUtility.CloseUIView(UIType.Pair);
    }
    #endregion

    #region 检测杠

    private static IEnumerator CheckBeBar(MahjongOwnType belongType, MahjongItem mahjongItem)
    {
        List<MahjongItem> CurMahjongList = GetCurTurnMahjongList(belongType);
        var result = HasSame(CurMahjongList, mahjongItem, 4);
        if (result != null)
        {
            //有杠
            if(belongType == MahjongOwnType.Own)
                yield return WaitUIBeBar(belongType, result);
            else
            {
                ChoiceResult = true;
                MahjongGameManager.Instance.AddLockMahjongList(belongType, result, MahjongLockType.Bar);
            }
                
        }
        
        yield break;
    }
    
    public static IEnumerator WaitUIBeBar(MahjongOwnType belongType, List<MahjongItem> mahjongList)
    {
        //显示UI
        UIUtility.LoadUIView<CommonUIView>(UIType.HiddenBar, null);

        ClearData();

        while (!ChoiceOver)
        {
            yield return null;
        }

        if ((bool)ChoiceResult)
        {
            //执行被碰
            MahjongGameManager.Instance.AddLockMahjongList(belongType, mahjongList, MahjongLockType.Bar);
        }
        
        //隐藏UI
        UIUtility.CloseUIView(UIType.HiddenBar);
    }
    
    #endregion

    #region 检测被吃

    private static IEnumerator CheckBeOrder(MahjongOwnType belongType, MahjongItem mahjongItem)
    {
        var result = HasContinue(GetCurTurnMahjongList(belongType), mahjongItem);
        if (result != null)
        {
            //有吃
            if(belongType == MahjongOwnType.Own)
                yield return WaitUIBeOrder(belongType, result);
            else
            {
                ChoiceResult = true;
                MahjongGameManager.Instance.AddLockMahjongList(belongType, result, MahjongLockType.Order);
            }
        }
        
        yield break;
    }
    
    public static IEnumerator WaitUIBeOrder(MahjongOwnType belongType, List<MahjongItem> mahjongList)
    {
        //显示UI
        UIUtility.LoadUIView<CommonUIView>(UIType.Order, null);

        ClearData();

        while (!ChoiceOver)
        {
            yield return null;
        }

        if ((bool)ChoiceResult)
        {
            MahjongGameManager.Instance.AddLockMahjongList(belongType, mahjongList, MahjongLockType.Order);
        }
        
        //隐藏UI
        UIUtility.CloseUIView(UIType.Order);
    }

    #endregion
} 
    