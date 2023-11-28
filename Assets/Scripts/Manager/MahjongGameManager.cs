using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MahjongGameManager : MonoBehaviour
{
    private static MahjongGameManager Instance;
    private static List<MahjongItem> MahjongItemList;
    private Dictionary<string, GameObject> LoadMahjongDictionary;
    private MahjongOwnType CurFarmHouse = MahjongOwnType.Own;
    private MahjongProgressType CurProgressType = MahjongProgressType.Prepare;
    
    /// <summary>
    /// 庄家列表
    /// </summary>
    public List<GameObject> FarmHouseList;
    
    /// <summary>
    /// 创建跟节点
    /// </summary>
    public List<Transform> CreateRootList;

    [HideInInspector]
    public List<MahjongItem> OwnMahjongList;
    [HideInInspector]
    public List<MahjongItem> LeftMahjongList;
    [HideInInspector]
    public List<MahjongItem> OppoMahjongList;
    [HideInInspector]
    public List<MahjongItem> RightMahjongList;
    
    public void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
            //初始化配置
            ConfigUtility.CreateConfig();
        }
        else
        {
            Destroy(Instance);
        }
    }

    private void Start()
    {
        GamePrepare(MahjongRule.ShenYang);
    }
    
    /// <summary>
    /// 游戏开始
    /// </summary>
    private void GamePrepare(MahjongRule mahjongRule)
    {
        OwnMahjongList = new List<MahjongItem>();
        LeftMahjongList = new List<MahjongItem>();
        OppoMahjongList = new List<MahjongItem>();
        RightMahjongList = new List<MahjongItem>();

        LoadMahjongDictionary = new Dictionary<string, GameObject>();
        MahjongItemList = new List<MahjongItem>();
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

        CurProgressType = MahjongProgressType.Prepare;
    }

    /// <summary>
    /// 改变庄家
    /// </summary>
    /// <param name="ownType"></param>
    public void ChangeFarmHouse(MahjongOwnType ownType)
    {
        CurFarmHouse = ownType;
        for (int i = 0; i < FarmHouseList.Count; i++)
        {
            FarmHouseList[i].SetActive(CurFarmHouse == (MahjongOwnType)i);
        }
    }

    /// <summary>
    /// 游戏流程
    /// </summary>
    public void GameStart()
    {
        if(CurProgressType != MahjongProgressType.Prepare)
            return;
        CurProgressType = MahjongProgressType.Start;
        
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
                    tempItem.m_Data.Own = _curOwnType;
                    tempItem.transform.SetParent(CreateRootList[(int)_curOwnType - 1]);
                    tempItem.transform.localPosition = GetDivideMahjongVector3(_curOwnType);
                    tempItem.transform.localRotation = Quaternion.identity;
                    DivideAddMahjongList(tempItem);
                }
            }
        }
        
        
        
        CurProgressType = MahjongProgressType.Start;
    }

    private Vector3 GetDivideMahjongVector3(MahjongOwnType ownType)
    {
        switch (ownType)
        {
            case MahjongOwnType.Own:
            {
                return  new Vector3(-OwnMahjongList.Count * GameDefine.MahjongInterval, 0, 0);
            }
            case MahjongOwnType.Left:
            {
                return  new Vector3(-LeftMahjongList.Count * GameDefine.MahjongInterval, 0, 0);
                break;
            }
            case MahjongOwnType.Oppo:
            {
                return  new Vector3(-OppoMahjongList.Count * GameDefine.MahjongInterval, 0, 0);
                break;
            }
            case MahjongOwnType.Right:
            {
                return  new Vector3(-RightMahjongList.Count * GameDefine.MahjongInterval, 0, 0);
                break;
            }
        }

        return Vector3.zero;
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

    private void Update()
    {
        
    }


    //---------------------排序
    /// <summary>
    /// 重新排序
    /// </summary>
    /// <param name="mahjongConfigList"></param>
    /// <returns></returns>
    private List<MahjongConfig> SortMahjongConfig(List<MahjongConfig> mahjongConfigList)
    {
        for (int i = mahjongConfigList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, mahjongConfigList.Count + 1);
            (mahjongConfigList[i], mahjongConfigList[j]) = (mahjongConfigList[j], mahjongConfigList[i]);
        }

        return mahjongConfigList;
    }
}
