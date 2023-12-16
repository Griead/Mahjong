using System.Collections.Generic;
using UnityEngine;

public static class UIUtility
{
    /// <summary>
    /// UI加载路径
    /// </summary>
    private static Dictionary<UIType, string> LoadPathUIDict;
    /// <summary>
    /// 加载模板字典
    /// </summary>
    private static Dictionary<UIType, GameObject> LoadModelDict;
    /// <summary>
    /// 已存在的UI视图
    /// </summary>
    private static Dictionary<UIType, BaseUIView> ViewUIDict;

    public static void Init()
    {
        //预先键入加载路径
        LoadPathUIDict = new Dictionary<UIType, string>();
        LoadPathUIDict.Add(UIType.HuUI, "UI/HuUI");
        LoadPathUIDict.Add(UIType.HiddenBar, "UI/HiddenBarUI");
        LoadPathUIDict.Add(UIType.Pair, "UI/PairUI");
        LoadPathUIDict.Add(UIType.Order, "UI/OrderUI");
        LoadPathUIDict.Add(UIType.StartUI, "UI/StartUI");
        
        LoadModelDict = new Dictionary<UIType, GameObject>();
        ViewUIDict = new Dictionary<UIType, BaseUIView>();
    }

    /// <summary>
    /// 加载UI视图
    /// </summary>
    public static void LoadUIView<T>(UIType type, object[] parameters) where T : BaseUIView
    {
        if (!LoadPathUIDict.ContainsKey(type))
            return;

        if (ViewUIDict.ContainsKey(type))
            return;
        
        GameObject model = null;
        if (LoadModelDict.ContainsKey(type))
            model = LoadModelDict[type];
        else
        {
            model = Resources.Load<GameObject>(LoadPathUIDict[type]);
            LoadModelDict.Add(type, model); 
        }

        GameObject go = Object.Instantiate(model, MahjongGameManager.Instance.UICanvas.transform);
        T uIView = go.AddComponent<T>();
        
        uIView.OnAwake();
        uIView.OnStart();
        
        uIView.Show(parameters);
        
        ViewUIDict.Add(type, uIView);
    }

    /// <summary>
    /// 关闭UI视图
    /// </summary>
    /// <param name="type"></param>
    public static void CloseUIView(UIType type)
    {
        if (!ViewUIDict.ContainsKey(type))
            return;
        
        Object.Destroy(ViewUIDict[type].gameObject);
        ViewUIDict.Remove(type);
    }
}