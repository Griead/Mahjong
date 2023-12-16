using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using TMPro;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class MahjongGameManager : MonoBehaviour
{
    public static MahjongGameManager Instance;
    private Dictionary<string, GameObject> LoadMahjongDictionary;
    /// <summary>
    /// 玩家回合
    /// </summary>
    public List<GameObject> PlayOwnList;
    /// <summary>
    /// 庄家列表
    /// </summary>
    public List<GameObject> FarmHouseList;
    /// <summary>
    /// 创建跟节点
    /// </summary>
    public List<Transform> CreateRootList;
    /// <summary>
    /// 弃牌生成父级
    /// </summary>
    public List<Transform> DiscardRootList;
    /// <summary>
    /// 锁定节点列表
    /// </summary>
    public List<Transform> LockRootList;
    /// <summary>
    /// UI视图
    /// </summary>
    public Canvas UICanvas;

    public TextMeshPro MahjongRemainText;
    
    //----------------------------
    [HideInInspector]
    public MahjongOwnType CurFarmHouse = MahjongOwnType.Own;
    [HideInInspector]
    public MahjongProgressType CurProgressType = MahjongProgressType.Prepare;
    
    [HideInInspector]
    public List<MahjongItem> MahjongItemList;

    [HideInInspector] public List<MahjongItem> OwnMahjongList;

    [HideInInspector] public List<MahjongItem> LeftMahjongList;

    [HideInInspector] public List<MahjongItem> OppoMahjongList;

    [HideInInspector] public List<MahjongItem> RightMahjongList;

    /// <summary>
    /// 弃牌字典
    /// </summary>
    private Dictionary<MahjongOwnType, List<MahjongItem>> CacheDiscardDict;

    /// <summary>
    /// 锁定数据
    /// </summary>
    [HideInInspector] public Dictionary<MahjongOwnType, List<MahjongLockData>> CacheLockDict;
    
    public void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
            //初始化配置
            ConfigUtility.CreateConfig();
            UIUtility.Init();
        }
        else
        {
            Destroy(Instance);
        }
    }

    private void Start()
    {
        OwnMahjongList = new List<MahjongItem>();
        LeftMahjongList = new List<MahjongItem>();
        OppoMahjongList = new List<MahjongItem>();
        RightMahjongList = new List<MahjongItem>();
        LoadMahjongDictionary = new Dictionary<string, GameObject>();
        CacheDiscardDict = new Dictionary<MahjongOwnType, List<MahjongItem>>();
        CacheLockDict = new Dictionary<MahjongOwnType, List<MahjongLockData>>();
        
        GamePrepare(MahjongRule.ShenYang);
    }
    
    /// <summary>
    /// 游戏开始
    /// </summary>
    private void GamePrepare(MahjongRule mahjongRule)
    {
        CurProgressType = MahjongProgressType.Prepare;

        for (int i = 0; i < MahjongItemList.Count; i++)
            MahjongItemList[i].ReleaseAsset();
        MahjongItemList.Clear();
        
        var MahjongConfigList = new List<MahjongConfig>();
        switch (mahjongRule)
        {
            case MahjongRule.ShenYang:
            {
                MahjongConfigList = new List<MahjongConfig>();
                int count = ConfigUtility.MahjongConfigList.Count;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < count; j++)
                    {
                        MahjongConfigList.Add(ConfigUtility.MahjongConfigList[j]);
                    }
                }

                //打乱所有麻将
                MahjongConfigList = SortMahjongConfig(MahjongConfigList);
                for (int i = 0; i < MahjongConfigList.Count; i++)
                {
                    GameObject model = null;
                    if (LoadMahjongDictionary.ContainsKey(MahjongConfigList[i].LoadPath))
                    {
                        model = LoadMahjongDictionary[MahjongConfigList[i].LoadPath];
                    }
                    else
                    {
                        model = Resources.Load<GameObject>(MahjongConfigList[i].LoadPath);
                        LoadMahjongDictionary.Add(MahjongConfigList[i].LoadPath, model);
                    }

                    GameObject go = Instantiate(model);
                    MahjongItem item = go.AddComponent<MahjongItem>();
                    item.m_Data = new MahjongData();
                    item.m_Config = MahjongConfigList[i];
                    MahjongItemList.Add(item);
                }

                //准备发牌
                break;
            }
        }
        
        //开始页面
        UIUtility.LoadUIView<StartUIView>(UIType.StartUI, null);
    }

    public void ReStart()
    {
        UIUtility.CloseUIView(UIType.StartUI);
        
        ClearGameAll();
        
        GameStart();

        StartProgress();
    }

    /// <summary>
    /// 清理游戏所有麻将条目
    /// </summary>
    private void ClearGameAll()
    {
        for (int i = 0; i < OwnMahjongList.Count; i++)
        {
            OwnMahjongList[i].ReleaseAsset();
        }
        OwnMahjongList.Clear();
        
        for (int i = 0; i < LeftMahjongList.Count; i++)
        {
            LeftMahjongList[i].ReleaseAsset();
        }
        LeftMahjongList.Clear();
        
        for (int i = 0; i < OppoMahjongList.Count; i++)
        {
            OppoMahjongList[i].ReleaseAsset();
        }
        OppoMahjongList.Clear();
        
        for (int i = 0; i < RightMahjongList.Count; i++)
        {
            RightMahjongList[i].ReleaseAsset();
        }
        RightMahjongList.Clear();
        
        foreach (var mahjongOwnType in CacheDiscardDict.Keys)
        {
            for (int i = 0; i < CacheDiscardDict[mahjongOwnType].Count; i++)
            {
                CacheDiscardDict[mahjongOwnType][i].ReleaseAsset();
            }
            CacheDiscardDict[mahjongOwnType].Clear();
        }
        CacheDiscardDict.Clear();
        
        foreach (var mahjongOwnType in CacheLockDict.Keys)
        {
            for (int i = 0; i < CacheLockDict[mahjongOwnType].Count; i++)
            {
                CacheLockDict[mahjongOwnType][i].ReleaseAsset();
            }
            CacheLockDict[mahjongOwnType].Clear();
        }
        CacheLockDict.Clear();
        
    }
    

    /// <summary>
    /// 改变庄家
    /// </summary>
    /// <param name="ownType"></param>
    public void ChangeFarmHouse(MahjongOwnType ownType)
    {
        CurFarmHouse = ownType;
        FarmHouseList[0].SetActive(CurFarmHouse == MahjongOwnType.Own);
        FarmHouseList[1].SetActive(CurFarmHouse == MahjongOwnType.Left);
        FarmHouseList[2].SetActive(CurFarmHouse == MahjongOwnType.Oppo);
        FarmHouseList[3].SetActive(CurFarmHouse == MahjongOwnType.Right);
    }

    /// <summary>
    /// 游戏流程
    /// </summary>
    public void GameStart()
    {
        if(CurProgressType != MahjongProgressType.Prepare)
            return;
        CurProgressType = MahjongProgressType.Start;

        ChangeFarmHouse(CurFarmHouse);
        int farmHouseCount = (int)CurFarmHouse;
        //发牌
        for (int i = 0; i < 3; i++)
        {
            //每人发牌三次 每次四张
            for (int j = 0; j < 4; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    int endIndex = MahjongItemList.Count - 1;
                    var tempItem = MahjongItemList[endIndex];
                    MahjongItemList.RemoveAt(endIndex);
                    MahjongOwnType _curOwnType = (j + farmHouseCount) > 4
                        ? (MahjongOwnType)((j + farmHouseCount) % 4 + 1)
                        : (MahjongOwnType)(j + farmHouseCount);
                    tempItem.SetData(_curOwnType, false);
                    DivideAddMahjongList(tempItem);
                }
            }
        }
        
        //每人再发一张
        for (int j = 0; j < 4; j++)
        {
            int endIndex = MahjongItemList.Count - 1;
            var tempItem = MahjongItemList[endIndex];
            MahjongItemList.RemoveAt(endIndex);
            MahjongOwnType _curOwnType = (j + farmHouseCount) > 4
                ? (MahjongOwnType)((j + farmHouseCount) % 4 + 1)
                : (MahjongOwnType)(j + farmHouseCount);
            tempItem.SetData(_curOwnType, false);
            DivideAddMahjongList(tempItem);
        }
        
        //排序和重新定位每个角色的牌
        SortAndRelocationEveryOneMahjong();
        
        CurProgressType = MahjongProgressType.Start;
    }

    private void SortAndRelocationEveryOneMahjong()
    {
        SortAndRelocationMahjong(MahjongOwnType.Own);
        SortAndRelocationMahjong(MahjongOwnType.Left);
        SortAndRelocationMahjong(MahjongOwnType.Oppo);
        SortAndRelocationMahjong(MahjongOwnType.Right);
    }

    /// <summary>
    /// 排序麻将
    /// </summary>
    /// <param name="ownType"></param>
    /// <param name="newItem"></param>
    public void SortAndRelocationMahjong(MahjongOwnType ownType, bool otherIsOver = false)
    {
        var modelList = GameProgress.GetCurTurnMahjongList(ownType);
        
        modelList.Sort((a,b) =>
        {
            //类型权重
            int A_MahjongType = (int)a.m_Config.mahjongType;
            int B_MahjongType = (int)b.m_Config.mahjongType;
            int typeWeight =B_MahjongType.CompareTo(A_MahjongType);

            //数量权重
            int countWeight = b.m_Config.Id.CompareTo(a.m_Config.Id);

            return typeWeight * 10 + countWeight;
        });

        for (int i = 0; i < modelList.Count; i++)
        {
            Transform root = CreateRootList[(int)ownType - 1];
            
            modelList[i].transform.SetParent(root);
            modelList[i].transform.localPosition = GetMahjongVector3(i);
            
            if(otherIsOver)
                modelList[i].transform.localEulerAngles = new Vector3(-90, 0, 0);
        }
        
        switch (ownType)
        {
            case MahjongOwnType.Own:
            {
                OwnMahjongList = modelList;
                break;   
            }
            case MahjongOwnType.Left:
            {
                LeftMahjongList = modelList;
                break;   
            }
            case MahjongOwnType.Oppo:
            {
                OppoMahjongList = modelList;
                break;   
            }
            case MahjongOwnType.Right:
            {
                RightMahjongList = modelList;
                break;   
            }
        }
    }

    public Vector3 GetDivideMahjongVector3(MahjongOwnType ownType)
    {
        switch (ownType)
        {
            case MahjongOwnType.Own:
            {
                return GetMahjongVector3(OwnMahjongList.Count);
            }
            case MahjongOwnType.Left:
            {
                return GetMahjongVector3(LeftMahjongList.Count);
                break;
            }
            case MahjongOwnType.Oppo:
            {
                return GetMahjongVector3(OppoMahjongList.Count);
                break;
            }
            case MahjongOwnType.Right:
            {
                return GetMahjongVector3(RightMahjongList.Count);
                break;
            }
        }

        return Vector3.zero;
    }

    public Transform GetMahjongRoot(int index)
    {
        return CreateRootList[index];
    }

    public Vector3 GetMahjongVector3(int index)
    {
       return new Vector3(-index * GameDefine.MahjongInterval, 0, 0);
    }
    
    /// <summary>
    /// 获取临时麻将位置
    /// </summary>
    /// <returns></returns>
    public Vector3 GetMahjongTempVector3()
    {
        return new Vector3(12, 0, 0);
    }

    private void DivideAddMahjongList(MahjongItem tempItem)
    {
        switch (tempItem.m_Data.Own)
        {
            case MahjongOwnType.Own:
            {
                OwnMahjongList.Add(tempItem);
                break;
            }
            case MahjongOwnType.Left:
            {
                LeftMahjongList.Add(tempItem);
                break;
            }
            case MahjongOwnType.Oppo:
            {
                OppoMahjongList.Add(tempItem);
                break;
            }
            case MahjongOwnType.Right:
            {
                RightMahjongList.Add(tempItem);
                break;
            }
        }
    }

    //---------------------排序
    /// <summary>
    /// 重新排序
    /// </summary>
    /// <param name="mahjongConfigList"></param>
    /// <returns></returns>
    public List<MahjongConfig> SortMahjongConfig(List<MahjongConfig> mahjongConfigList)
    {
        for (int i = mahjongConfigList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, mahjongConfigList.Count);
            (mahjongConfigList[i], mahjongConfigList[j]) = (mahjongConfigList[j], mahjongConfigList[i]);
        }

        return mahjongConfigList;
    }
    
    /// <summary>
    /// 从弃牌堆中移除麻将
    /// </summary>
    public void RemoveDiscardMahjongItem(MahjongOwnType mahjongOwnType, MahjongItem mahjongItem)
    {
        if(mahjongItem is null)
            return;
        
        if (CacheDiscardDict.ContainsKey(mahjongOwnType))
        {
            CacheDiscardDict[mahjongOwnType].Remove(mahjongItem);
        }
    }
    
    /// <summary>
    /// 弃牌麻将
    /// </summary>
    /// <param name="mahjongOwnType"></param>
    /// <param name="mahjongItem"></param>
    public void DiscardMahjongItem(MahjongOwnType mahjongOwnType, MahjongItem mahjongItem)
    {
        CacheDiscardDict.TryGetValue(mahjongOwnType, out var discardList);

        int discardListCount = discardList?.Count ?? 0;

        if (discardList == null)
            CacheDiscardDict.Add(mahjongOwnType, new List<MahjongItem>());
        CacheDiscardDict[mahjongOwnType].Add(mahjongItem);
        
        var root = GetDiscardMahjongVector3(mahjongOwnType, discardListCount); 
        
        mahjongItem.transform.SetParent(root);
        mahjongItem.transform.localPosition = Vector3.zero;
        mahjongItem.transform.localRotation = Quaternion.identity;
    }

    public Transform GetDiscardMahjongVector3(MahjongOwnType mahjongOwnType, int Index)
    {
       return DiscardRootList[(int)mahjongOwnType - 1].GetChild(Index);
    }
    
    /// <summary>
    /// 修改麻将锁定数据 碰改为杠
    /// </summary>
    public void ChangeLockMahjongDataPairToBar(MahjongOwnType mahjongOwnType, List<MahjongItem> curMahjongList, MahjongLockData oldLockData, int Index)
    {
        oldLockData.LockType = MahjongLockType.Bar;
        oldLockData.MahjongList = curMahjongList;
        for (int i = 0; i < oldLockData.MahjongList.Count; i++)
        {
            oldLockData.MahjongList[i].m_Data.Lock = MahjongLockType.Bar;
        }
        var mahjongLockList = GetCurLockMahjongData(mahjongOwnType);
        mahjongLockList[Index] = oldLockData;
        
        ShowLockData(oldLockData, Index);
    }
    /// <summary>
    /// 锁定麻将列表
    /// </summary>
    /// <param name="mahjongOwnType"></param>
    /// <param name="mahjongList"></param>
    public void AddLockMahjongList(MahjongOwnType mahjongOwnType, List<MahjongItem> mahjongList, MahjongLockType lockType)
    {
        //音效
        PlayerOperateAudio(mahjongOwnType, lockType);
        
        List<MahjongItem> tempMahjongList = null;
        switch (mahjongOwnType)
        {
            case MahjongOwnType.Own:
            {
                tempMahjongList = OwnMahjongList;
                break;
            }
            case MahjongOwnType.Left:
            {
                tempMahjongList = LeftMahjongList;
                break;
            }
            case MahjongOwnType.Oppo:
            {
                tempMahjongList = OppoMahjongList;
                break;
            }
            case MahjongOwnType.Right:
            {
                tempMahjongList = RightMahjongList;
                break;
            }
        }

        for (int i = 0; i < mahjongList.Count; i++)
        {
            if(tempMahjongList != null && tempMahjongList.Contains(mahjongList[i]))
                tempMahjongList.Remove(mahjongList[i]);
        }

        MahjongLockData lockData = new MahjongLockData()
            { MahjongList = mahjongList, OwnType = mahjongOwnType, LockType = lockType };
        for (int i = 0; i < lockData.MahjongList.Count; i++)
        {
            lockData.MahjongList[i].m_Data.Lock = lockType;
            lockData.MahjongList[i].m_Data.Own = mahjongOwnType;
        }
        
        int Index = 0;
        if (CacheLockDict.ContainsKey(mahjongOwnType))
        {
            Index = CacheLockDict[mahjongOwnType].Count;
            CacheLockDict[mahjongOwnType].Add(lockData);
        }
        else
        {
            Index = 0;
            CacheLockDict.Add(mahjongOwnType, new List<MahjongLockData>(){lockData});
        }

        ShowLockData(lockData, Index);
        
        //重新排列手中的排
        SortAndRelocationMahjong(mahjongOwnType);
    }

    /// <summary>
    /// 展示锁定数据
    /// </summary>
    public void ShowLockData(MahjongLockData lockData, int Index)
    {
        Transform root = LockRootList[(int)lockData.OwnType - 1].GetChild(Index);
        switch (lockData.LockType)
        {
            case MahjongLockType.Pair:
            {
                lockData.MahjongList[0].transform.SetParent(root);
                lockData.MahjongList[0].transform.localPosition = new Vector3(-7, 0, 0);
                lockData.MahjongList[0].transform.localEulerAngles = Vector3.zero;
                lockData.MahjongList[0].transform.localScale = Vector3.one; 
                lockData.MahjongList[1].transform.SetParent(root);
                lockData.MahjongList[1].transform.localPosition = new Vector3(0, 0, 0);
                lockData.MahjongList[1].transform.localEulerAngles = Vector3.zero;
                lockData.MahjongList[1].transform.localScale = Vector3.one; 
                lockData.MahjongList[2].transform.SetParent(root);
                lockData.MahjongList[2].transform.localPosition = new Vector3(7, 0, 0);
                lockData.MahjongList[2].transform.localEulerAngles = Vector3.zero;
                lockData.MahjongList[2].transform.localScale = Vector3.one; 
                break;
            }
            case MahjongLockType.Order:
            {
                lockData.MahjongList[0].transform.SetParent(root);
                lockData.MahjongList[0].transform.localPosition = new Vector3(-7, 0, 0);
                lockData.MahjongList[0].transform.localEulerAngles = Vector3.zero;
                lockData.MahjongList[0].transform.localScale = Vector3.one; 
                lockData.MahjongList[1].transform.SetParent(root);
                lockData.MahjongList[1].transform.localPosition = new Vector3(0, 0, 0);
                lockData.MahjongList[1].transform.localEulerAngles = Vector3.zero;
                lockData.MahjongList[1].transform.localScale = Vector3.one; 
                lockData.MahjongList[2].transform.SetParent(root);
                lockData.MahjongList[2].transform.localPosition = new Vector3(7, 0, 0);
                lockData.MahjongList[2].transform.localEulerAngles = Vector3.zero;
                lockData.MahjongList[2].transform.localScale = Vector3.one; 
                break;
            }
            case MahjongLockType.Bar:
            {
                lockData.MahjongList[0].transform.SetParent(root);
                lockData.MahjongList[0].transform.localPosition = new Vector3(-7, 0, 0);
                lockData.MahjongList[0].transform.localEulerAngles = Vector3.zero;
                lockData.MahjongList[0].transform.localScale = Vector3.one; 
                lockData.MahjongList[1].transform.SetParent(root);
                lockData.MahjongList[1].transform.localPosition = new Vector3(0, 0, 0);
                lockData.MahjongList[1].transform.localEulerAngles = Vector3.zero;
                lockData.MahjongList[1].transform.localScale = Vector3.one; 
                lockData.MahjongList[2].transform.SetParent(root);
                lockData.MahjongList[2].transform.localPosition = new Vector3(7, 0, 0);
                lockData.MahjongList[2].transform.localEulerAngles = Vector3.zero;
                lockData.MahjongList[2].transform.localScale = Vector3.one; 
                lockData.MahjongList[3].transform.SetParent(root);
                lockData.MahjongList[3].transform.localPosition = new Vector3(0, 5, 0);
                lockData.MahjongList[3].transform.localEulerAngles = Vector3.zero;
                lockData.MahjongList[3].transform.localScale = Vector3.one; 
                break;
            }
            case MahjongLockType.HiddenBar:
            {
                lockData.MahjongList[0].transform.SetParent(root);
                lockData.MahjongList[0].transform.localPosition = new Vector3(-7, 0, 0);
                lockData.MahjongList[0].transform.localEulerAngles = Vector3.zero;
                lockData.MahjongList[0].transform.localScale = Vector3.one; 
                lockData.MahjongList[1].transform.SetParent(root);
                lockData.MahjongList[1].transform.localPosition = new Vector3(0, 0, 0);
                lockData.MahjongList[1].transform.localEulerAngles = Vector3.zero;
                lockData.MahjongList[1].transform.localScale = Vector3.one; 
                lockData.MahjongList[2].transform.SetParent(root);
                lockData.MahjongList[2].transform.localPosition = new Vector3(7, 0, 0);
                lockData.MahjongList[2].transform.localEulerAngles = Vector3.zero;
                lockData.MahjongList[2].transform.localScale = Vector3.one; 
                lockData.MahjongList[3].transform.SetParent(root);
                lockData.MahjongList[3].transform.localPosition = new Vector3(0, 5, 0);
                lockData.MahjongList[3].transform.localEulerAngles = Vector3.zero;
                lockData.MahjongList[3].transform.localScale = Vector3.one; 
                break;
            }
        }
    }
    
    public List<MahjongLockData> GetCurLockMahjongData(MahjongOwnType belongType)
    {
        MahjongGameManager.Instance.CacheLockDict.TryGetValue(belongType, out var resultList);
        return resultList;
    }
    
    // Update is called once per frame
    void Update()
    {
        if(CurProgressType != MahjongProgressType.Start)
            return;
        
        MahjongRemainText.text = MahjongItemList.Count.ToString();
        GameProgress.HitMahjongCheck();
        PlayOwnList[0].SetActive(GameProgress.CurMahjongProgress == MahjongOwnType.Own);
        PlayOwnList[1].SetActive(GameProgress.CurMahjongProgress == MahjongOwnType.Left);
        PlayOwnList[2].SetActive(GameProgress.CurMahjongProgress == MahjongOwnType.Oppo);
        PlayOwnList[3].SetActive(GameProgress.CurMahjongProgress == MahjongOwnType.Right);
    }
    /// <summary>
    /// 开始流程
    /// </summary>
    public void StartProgress()
    {
        GameProgress.CurMahjongProgress = CurFarmHouse;
        
        StartCoroutine(GameProgress.ContinueProgress());
    }
    /// <summary>
    /// 结束流程
    /// </summary>
    /// <param name="belongType"></param>
    /// <param name="mahjongItem"></param>
    public void OverProgress(MahjongOwnType belongType, MahjongItem mahjongItem)
    {
        if (belongType != MahjongOwnType.None)
        {
            PlayWinAudio(belongType);
            
            //进入手牌并且重新排序
            var mahjongList = GameProgress.GetCurTurnMahjongList(belongType);
        
            //自摸则不会有mahjongItem数据
            if (mahjongItem != null)
            {
                
                mahjongList.Add(mahjongItem);
            }
            
            SortAndRelocationMahjong(belongType, belongType != MahjongOwnType.Own);
        }
            
        //游戏结束
        CurProgressType = MahjongProgressType.End;

        GamePrepare(MahjongRule.ShenYang);
        
        UIUtility.LoadUIView<StartUIView>(UIType.StartUI, null);
    }

    #region 音效相关

    public MahjongAudioType GetMahjongAudioType(MahjongOwnType ownType)
    {
        switch (ownType)
        {
            case MahjongOwnType.Own:
                return MahjongAudioType.Woman;
            case MahjongOwnType.Left:
                return MahjongAudioType.Man;
            case MahjongOwnType.Oppo:
                return MahjongAudioType.Woman;
            case MahjongOwnType.Right:
                return MahjongAudioType.Man;
        }

        return MahjongAudioType.Man;
    }
    
    /// <summary>
    /// 播放麻将音效
    /// </summary>
    /// <param name="ownType"></param>
    /// <param name="config"></param>
    public void PlayMahjongAudio(MahjongOwnType ownType, MahjongConfig config)
    {
        var soundPath = ConfigUtility.SubstringAudioLoadPath(GetMahjongAudioType(ownType), config.mahjongType, config.Id);
        AudioManager.Instance.PlaySound(soundPath);
        AudioManager.Instance.PlayHitSound();
    }
    
    /// <summary>
    /// 播放操作音效
    /// </summary>
    public void PlayerOperateAudio(MahjongOwnType ownType, MahjongLockType lockType)
    {
        string _path = "Audio";
        switch (GetMahjongAudioType(ownType))
        {
            case MahjongAudioType.Woman:
                _path += "/Woman";
                break;
            case MahjongAudioType.Man:
                _path += "/Man";
                break;
        }

        switch (lockType)
        {
            case MahjongLockType.Pair:
                _path += "/Pair";
                break;
            case MahjongLockType.Order:
                _path += "/Order";
                break;
            case MahjongLockType.Bar:
                _path += "/Bar";
                break;
            case MahjongLockType.HiddenBar:
                _path += "/Bar";
                break;
        }
        
        AudioManager.Instance.PlaySound(_path);
    }

    public void PlayWinAudio(MahjongOwnType ownType)
    {
        string _path = "Audio";
        switch (GetMahjongAudioType(ownType))
        {
            case MahjongAudioType.Woman:
                _path += "/Woman/Win";
                break;
            case MahjongAudioType.Man:
                _path += "/Man/Win";
                break;
        }
    
        
        AudioManager.Instance.PlaySound(_path);
    }
    

    #endregion

    public void OnCallOppoWin()
    {
        var mahjongList = CacheDiscardDict[MahjongOwnType.Own];
        
        OverProgress(MahjongOwnType.Oppo, mahjongList[0]);
    }
    
    public void OnCallBar()
    {
        var mahjongList = CacheDiscardDict[MahjongOwnType.Own];
        var ownmahjongList = GameProgress.GetCurTurnMahjongList(MahjongOwnType.Oppo);
        
        MahjongLockData lockData = new MahjongLockData();
        lockData.LockType = MahjongLockType.Bar;
        lockData.OwnType = MahjongOwnType.Oppo;
        lockData.MahjongList = new List<MahjongItem>();
        lockData.MahjongList.Add(ownmahjongList[0]);
        lockData.MahjongList.Add(ownmahjongList[1]);
        lockData.MahjongList.Add(ownmahjongList[2]);
        lockData.MahjongList.Add(mahjongList[0]);
        
        ShowLockData(lockData,0);
    }
    
}
