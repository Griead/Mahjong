using System;
using UnityEngine;

public class MahjongItem : MonoBehaviour
{
    public MahjongConfig m_Config;
    public MahjongData m_Data;
    
    public void Awake()
    {
        
    }

    public void Start()
    {
        
    }
    
    /// <summary>
    /// 设置数据
    /// </summary>
    public void SetData(MahjongOwnType ownType, bool IsTempPos)
    {
        m_Data.Own = ownType;
        transform.SetParent( MahjongGameManager.Instance.GetMahjongRoot((int)ownType - 1));
        transform.localPosition = IsTempPos ? MahjongGameManager.Instance.GetMahjongTempVector3() : MahjongGameManager.Instance.GetDivideMahjongVector3(ownType);
        transform.localRotation = Quaternion.identity;
    }
}