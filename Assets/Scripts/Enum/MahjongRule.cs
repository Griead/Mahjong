public enum MahjongRule
{
    ShenYang,
}

/// <summary>
/// 麻将拥有的类型
/// </summary>
public enum MahjongOwnType
{
    /// <summary> 无 </summary>
    None,
    /// <summary> 自己 </summary>
    Own,
    /// <summary> 左边 </summary>
    Left,
    /// <summary> 对面 </summary>
    Oppo,
    /// <summary> 右边 </summary>
    Right
}

public enum MahjongProgressType
{
    Prepare,
    //Dice,
    Start,
    End,
}
