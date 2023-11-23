using System;
using System.Collections.Generic;

public static class ConfigUtility
{
    /// <summary>
    /// 麻将配置列表
    /// </summary>
    public static List<MahjongConfig> MahjongConfigList; 
    
    /// <summary>
    /// 创建所有麻将配置
    /// </summary>
    public static void CreateConfig()
    {
        MahjongConfigList = new List<MahjongConfig>();
        
        //首先创建条
        //一共九个
        for (int i = 1; i < 10; i++)
        {
            var mahjongConfig = new MahjongConfig() { mahjongType = EnumMahjongType.Bamboo, Id = 1 };
            mahjongConfig.LoadPath = SubstringLoadPath(mahjongConfig.mahjongType, mahjongConfig.Id);
            MahjongConfigList.Add(mahjongConfig);
        }
    }
    
    /// <summary>
    /// 拼接加载路径
    /// </summary>
    /// <returns></returns>
    public static string SubstringLoadPath(EnumMahjongType type, int Id)
    {
        string path = "";
        switch (type)
        {
            case EnumMahjongType.Characters:
            {
                path = $"Craks/Crak_{Id}";
                break;
            }
            case EnumMahjongType.Bamboo:
            {
                path += $"Bams/Bam_{Id}";
                break;
            }
            case EnumMahjongType.Dot:
            {
                path += $"Dots/Dot_{Id}";
                break;
            }
            case EnumMahjongType.Wind:
            {
                path += "Winds/Wind";
                break;
            }
            case EnumMahjongType.Dragon:
            {
                path += "Dragons/Dragon";
                break;
            }
        }

        return path;
    }
}