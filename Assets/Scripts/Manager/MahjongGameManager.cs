using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class MahjongGameManager : MonoBehaviour
{
    public static MahjongGameManager Instance;
    private Dictionary<string, GameObject> LoadMahjongDictionary;
    /// <summary>
    /// 庄家列表
    /// </summary>
    public List<GameObject> FarmHouseList;
    /// <summary>
    /// 创建跟节点
    /// </summary>
    public List<Transform> CreateRootList;
    /// <summary>
    /// UI视图
    /// </summary>
    public Canvas UICanvas;
    
    //----------------------------
    [HideInInspector]
    public MahjongOwnType CurFarmHouse = MahjongOwnType.Own;
    [HideInInspector]
    public MahjongProgressType CurProgressType = MahjongProgressType.Prepare;
    
    [HideInInspector]
    public List<MahjongItem> MahjongItemList;

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
            UIUtility.Init();
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
    private void SortAndRelocationMahjong(MahjongOwnType ownType)
    {
        var modelList = new List<MahjongItem>();
        switch (ownType)
        {
            case MahjongOwnType.Own:
            {
                modelList = OwnMahjongList;
                break;   
            }
            case MahjongOwnType.Left:
            {
                modelList = LeftMahjongList;
                break;   
            }
            case MahjongOwnType.Oppo:
            {
                modelList = OppoMahjongList;
                break;   
            }
            case MahjongOwnType.Right:
            {
                modelList = RightMahjongList;
                break;   
            }
        }
        
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
            modelList[i].transform.localPosition = GetMahjongVector3(i);
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
    private List<MahjongConfig> SortMahjongConfig(List<MahjongConfig> mahjongConfigList)
    {
        for (int i = mahjongConfigList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, mahjongConfigList.Count);
            (mahjongConfigList[i], mahjongConfigList[j]) = (mahjongConfigList[j], mahjongConfigList[i]);
        }

        return mahjongConfigList;
    }
    
    // Update is called once per frame
    void Update()
    {
        
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
                        Debug.Log("打出" + mahjongItem.m_Config.mahjongType + mahjongItem.m_Config.Id);
                    }
                }
            }
        }
    }

    public void StartProgress()
    {
        GameProgress.CurMahjongProgress = CurFarmHouse;
        
        StartCoroutine(GameProgress.ContinueProgress());
    }
}
