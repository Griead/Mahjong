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