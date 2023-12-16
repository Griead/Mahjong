using System.Collections.Generic;

public class GameData
{
    public MahjongOwnType SuccessPlayer;
}

/// <summary>
/// 麻将数据
/// </summary>
public class MahjongData
{
    public MahjongData()
    {
        Own = MahjongOwnType.None;
    }
    
    public MahjongOwnType Own;
    
    public MahjongLockType Lock;
}

/// <summary>
/// 麻将锁定数据
/// </summary>
public class MahjongLockData
{
    public List<MahjongItem> MahjongList;

    public MahjongLockType LockType;

    public MahjongOwnType OwnType;

    public void ReleaseAsset()
    {
        for (int i = 0; i < MahjongList.Count; i++)
        {
            MahjongList[i].ReleaseAsset();
        }
        
        MahjongList.Clear();
    }
}