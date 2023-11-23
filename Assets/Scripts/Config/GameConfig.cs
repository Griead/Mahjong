using System.Collections.Generic;

public class GameConfig
{
    public List<MahjongConfig> MahjongConfigList;
}

/// <summary>
/// 麻将配置
/// </summary>
public class MahjongConfig
{
    /// <summary> 麻将类型 </summary>
    public EnumMahjongType mahjongType;

    /// <summary> 麻将ID </summary>
    public int Id;

    /// <summary> 加载路径 </summary>
    public string LoadPath;
}
