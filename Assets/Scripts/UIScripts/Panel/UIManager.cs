using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//单例模式
public class UIManager
{
    private static UIManager instance=new UIManager();
    public static UIManager Instance => instance;

    //存储面板的容器
    private Dictionary<string,BasePanel> panelDic=new Dictionary<string,BasePanel>();

    //应该一开始就得到我们的Canvas对象
    private UnityEngine.Transform canvasTrans;
    private UIManager()
    {
        canvasTrans = GameObject.Find("Canvas").transform;
        //让Canvas过场景不移除
        //通过动态创建和动态删除来显示隐藏面板 所以不删除它影响不大
        GameObject.DontDestroyOnLoad(canvasTrans.gameObject);
    }

    //显示面板  T为泛型面板名字，基类的子类
    public T ShowPanel<T> () where T : BasePanel
    {
        string panelName = typeof(T).Name;

        if (panelDic.TryGetValue(panelName, out BasePanel cachedPanel))
        {
            if (cachedPanel != null && cachedPanel.gameObject != null)
            {
                cachedPanel.ShowMe();
                return cachedPanel as T;
            }
            else
            {
                panelDic.Remove(panelName);
            }
        }

        // 加载预制体
        string path = "UI/" + panelName;
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            return null;
        }

        GameObject panelObj = GameObject.Instantiate(prefab, canvasTrans, false);
        T panel = panelObj.GetComponent<T>();
        panelDic.Add(panelName, panel);
        panel.ShowMe();
        return panel;
    }
    //隐藏面板
    //参数一：如果希望 淡出 就默认传true 如果希望直接隐藏(删除)面板 那就传false
    public void HidePanel<T>(bool isFade = true) where T : BasePanel
    {
    string panelName = typeof(T).Name;
    if (panelDic.TryGetValue(panelName, out BasePanel panel))
    {
        // 立即从字典中移除，防止二次获取到正在销毁的面板
        panelDic.Remove(panelName);

        if (isFade)
        {
            // 淡出动画结束后销毁面板
            panel.HideMe(() =>
            {
                GameObject.Destroy(panel.gameObject);
            });
        }
        else
        {
            // 直接销毁
            GameObject.Destroy(panel.gameObject);
        }
    }
        
    }
    //获取面板
    public T GetPanel<T>() where T : BasePanel
    {
        string panelName=typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            return panelDic[panelName] as T;
        }
        else
        {
            return null;
        }
    }
}
