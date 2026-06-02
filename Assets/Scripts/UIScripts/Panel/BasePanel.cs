using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ShowMode
{
    SoftPopup,    // 柔和弹出（淡入+上浮）
    DirectPopup   // 直接弹出（强反馈+可选弹性）
}

public abstract class BasePanel : MonoBehaviour
{

    [Header("动画设置（可被每个子面板单独覆盖）")]
    [SerializeField] protected ShowMode showMode = ShowMode.SoftPopup;   // 默认柔和模式
    [SerializeField] protected float fadeDuration = 0.5f;               // 淡入淡出持续时间
    [SerializeField] protected float moveDuration = 0.6f;               // 位移动画时长（Soft模式）
    [SerializeField] private Vector3 startOffset = new Vector3(0, -100, 0); // 起始偏移（Soft模式）
    [SerializeField] protected Ease moveEase = Ease.OutCubic;           // 位移曲线（Soft模式）
    [SerializeField] protected float directDuration = 0.25f;            // 直接弹出模式的动画时长
    [SerializeField] protected Ease directEase = Ease.OutBack;          // 直接弹出模式的缓动曲线（带弹性）



    //整体控制淡入淡出的画布组 组件
    private CanvasGroup canvasGroup;

    //当自己淡出成功时 要执行的委托函数
    private UnityAction hideCallBack;

    protected virtual void Awake()
    {
        //一开始获取面板上 挂载的 组件 如果没有 我们通过代码 为它添加一个
        canvasGroup=this.GetComponent<CanvasGroup>();
        if(canvasGroup == null)
        {
            canvasGroup=this.gameObject.AddComponent<CanvasGroup>();
        }
        // 确保初始状态（防止编辑器预览时可见）
        canvasGroup.alpha = 0;
    }
    // Start is called before the first frame update
    protected virtual  void Start()
    {
        Init();
    }

    /// <summary>
    /// 用于初始化  重写按钮添加监听事件
    /// </summary>
    public abstract void Init();

    /// <summary>
    /// 显示自己时候做的事情
    /// </summary>
    public virtual void ShowMe()
    {
        // 根据模式执行不同动画
        switch (showMode)
        {
            case ShowMode.SoftPopup:
                ShowSoftPopup();
                break;
            case ShowMode.DirectPopup:
                ShowDirectPopup();
                break;
        }
    }

    /// <summary>
    /// 柔和弹出：淡入 + 上浮（适合开始界面、主界面）
    /// </summary>
    protected virtual void ShowSoftPopup()
    {
        canvasGroup.alpha = 0;
        transform.localPosition = startOffset;

        // 同时播放两个动画，且不受 Time.timeScale 影响
        canvasGroup.DOFade(1, fadeDuration).SetUpdate(true);
        transform.DOLocalMove(Vector3.zero, moveDuration)
            .SetEase(moveEase)
            .SetUpdate(true);
    }

    /// <summary>
    /// 直接弹出：带弹性/快速弹出，可选是否淡入（强反馈感）
    /// </summary>
    protected virtual void ShowDirectPopup()
    {
        // 方式1：直接弹入（不淡入，瞬间出现+弹性缩放/位移）
        // 这里示例为：从略微缩小到正常大小 + 弹性，你也可以改成从下方快速弹入
        canvasGroup.alpha = 1;                     // 不淡入，直接显示
        transform.localScale = Vector3.one * 0.8f; // 初始缩小

        transform.DOScale(1, directDuration)
            .SetEase(directEase)
            .SetUpdate(true);

        // 如果你想同时带一点位移（从下方快速弹起），可以加上：
        // transform.localPosition = startOffset * 0.5f;
        // transform.DOLocalMove(Vector3.zero, directDuration).SetEase(directEase);
    }




    /// <summary>
    /// 隐藏自己做的事情
    /// </summary>
    public virtual void HideMe(UnityAction callBack) 
    {

        //记录传入的当淡出成功后执行的函数
        hideCallBack=callBack;
        canvasGroup.DOFade(0, fadeDuration).OnComplete(() =>
        {
            hideCallBack?.Invoke();
        });
    }

    /// <summary>
    /// 立即隐藏（无动画），用于特殊需求
    /// </summary>
    public virtual void HideImmediately()
    {
        canvasGroup.DOKill();      // 停止所有动画
    }


}
