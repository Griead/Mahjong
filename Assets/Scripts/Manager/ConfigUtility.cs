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
            var mahjongConfig = new MahjongConfig() { mahjongType = EnumMahjongType.Bamboo, Id = i };
            mahjongConfig.LoadPath = SubstringLoadPath(mahjongConfig.mahjongType, mahjongConfig.Id);
            MahjongConfigList.Add(mahjongConfig);
        }
        
        //创建万
        for (int i = 1; i < 10; i++)
        {
            var mahjongConfig = new MahjongConfig() { mahjongType = EnumMahjongType.Characters, Id = i };
            mahjongConfig.LoadPath = SubstringLoadPath(mahjongConfig.mahjongType, mahjongConfig.Id);
            MahjongConfigList.Add(mahjongConfig);
        }
        
        //创建饼
        for (int i = 1; i < 10; i++)
        {
            var mahjongConfig = new MahjongConfig() { mahjongType = EnumMahjongType.Dot, Id = i };
            mahjongConfig.LoadPath = SubstringLoadPath(mahjongConfig.mahjongType, mahjongConfig.Id);
            MahjongConfigList.Add(mahjongConfig);
        }
        
        //创建中发白
        for (int i = 1; i < 3; i++)
        {
            var mahjongConfig = new MahjongConfig() { mahjongType = EnumMahjongType.Dragon, Id = i };
            mahjongConfig.LoadPath = SubstringLoadPath(mahjongConfig.mahjongType, mahjongConfig.Id);
            MahjongConfigList.Add(mahjongConfig);
        }
        
        //东南西北风
        for (int i = 1; i < 4; i++)
        {
            var mahjongConfig = new MahjongConfig() { mahjongType = EnumMahjongType.Wind, Id = i };
            mahjongConfig.LoadPath = SubstringLoadPath(mahjongConfig.mahjongType, mahjongConfig.Id);
            MahjongConfigList.Add(mahjongConfig);
        }
    }
    
    /// <summary>
    /// 拼接加载路径
    /// </summary>
    /// <returns></returns>
    public static string SubstringAudioLoadPath(MahjongAudioType audioType, EnumMahjongType type, int Id)
    {
        string path = "";
        switch (audioType)
        {
            case MahjongAudioType.Woman:
            {
                path += $"Audio/Woman";
                break;
            }
            case MahjongAudioType.Man:
            {
                path += $"Audio/Man";
                break;
            }
        }
        
        switch (type)
        {
            case EnumMahjongType.Characters:
            {
                path += $"/Crak_{Id}";
                break;
            }
            case EnumMahjongType.Bamboo:
            {
                path += $"/Bam_{Id}";
                break;
            }
            case EnumMahjongType.Dot:
            {
                path += $"/Dot_{Id}";
                break;
            }
            case EnumMahjongType.Wind:
            {
                path += "/Wind";
                switch (Id)
                {
                    case 0:
                    {
                        path += "_East";
                        break;
                    }
                    case 1:
                    {
                        path += "_South";
                        break;
                    }
                    case 2:
                    {
                        path += "_West";
                        break;
                    }
                    case 3:
                    {
                        path += "_North";
                        break;
                    }
                }
                break;
            }
            case EnumMahjongType.Dragon:
            {
                path += "/Dragon";
                
                switch (Id)
                {
                    case 0:
                    {
                        path += "_Red";
                        break;
                    }
                    case 1:
                    {
                        path += "_Green";
                        break;
                    }
                    case 2:
                    {
                        path += "_White";
                        break;
                    }
                }
                break;
            }
        }

        return path;
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
                path = $"Prefab/Craks/Crak_{Id}";
                break;
            }
            case EnumMahjongType.Bamboo:
            {
                path += $"Prefab/Bams/Bam_{Id}";
                break;
            }
            case EnumMahjongType.Dot:
            {
                path += $"Prefab/Dots/Dot_{Id}";
                break;
            }
            case EnumMahjongType.Wind:
            {
                path += "Prefab/Winds/Wind";
                switch (Id)
                {
                    case 0:
                    {
                        path += "_East";
                        break;
                    }
                    case 1:
                    {
                        path += "_South";
                        break;
                    }
                    case 2:
                    {
                        path += "_West";
                        break;
                    }
                    case 3:
                    {
                        path += "_North";
                        break;
                    }
                }
                break;
            }
            case EnumMahjongType.Dragon:
            {
                path += "Prefab/Dragons/Dragon";
                
                switch (Id)
                {
                    case 0:
                    {
                        path += "_Red";
                        break;
                    }
                    case 1:
                    {
                        path += "_Green";
                        break;
                    }
                    case 2:
                    {
                        path += "_White";
                        break;
                    }
                }
                break;
            }
        }

        return path;
    }
}