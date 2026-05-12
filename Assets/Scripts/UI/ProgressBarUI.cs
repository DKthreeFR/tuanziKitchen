using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    //因为接口类型无法显示到编辑器里所以我们需要这么做 不允许拖入接口引用
    [SerializeField] private GameObject hasProgressGameObject;
    [SerializeField] private Image barImage;

    private IHasProgress hasProgress;
    private void Start()
    {
        //获取hasProgress
        hasProgress = hasProgressGameObject.GetComponent<IHasProgress>();
        if(hasProgress == null )
        {
            Debug.LogError("该对象没有实现 带有进度条的对象");
        }
        hasProgress.OnProgressChanged += HasProgress_OnProgressChanged;
        barImage.fillAmount = 0;
        Hide();
    }

    private void HasProgress_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        barImage.fillAmount = e.progressNormalized;
        if(e.progressNormalized == 0 || e.progressNormalized==1)
        {
            //没有进度或者条满时Hide
            Hide();
        }
        else
        {
            //在进度中时 显示
            Show();
        }
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    } 
}
