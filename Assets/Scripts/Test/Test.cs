using System.Collections.Generic;

namespace Test
{
    
    public enum EnumMahjongType
    {
        /// <summary> 萬 </summary>
        Characters,
        /// <summary> 条 </summary>
        Bamboo,
        /// <summary> 饼 </summary>
        Dot,
        /// <summary> 东南西北风 </summary>
        Wind,
        /// <summary> 中發白 </summary>
        Dragon,
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
    
    
    
    public class Test
    {
        private List<MahjongConfig> curList;

        public bool CheckSuccess()
        {
            for (int i = 0; i < curList.Count; i++)
            {
                
            }

            return false;
        }
    }
}