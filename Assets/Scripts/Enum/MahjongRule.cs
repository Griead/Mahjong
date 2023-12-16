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

public enum MahjongLockType
{
    None,
    /// <summary> 碰 </summary>
    Pair,
    /// <summary> 吃 </summary>
    Order,
    /// <summary> 杠 </summary>
    Bar,
    /// <summary> 暗杠 </summary>
    HiddenBar,
}

public enum MahjongProgressType
{
    Prepare,
    Start,
    End,
}

public enum MahjongAudioType
{
    Woman,
    Man
}
