using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveBurnWarningUI : MonoBehaviour
{
    //关联炉灶柜台
    [SerializeField] private StoveCounter stoveCounter;
    private void Start()
    {
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
        //开始时处于隐藏状态
        Hide();
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmout = .5f;
        //当炉灶进度超过50%且处于煎炸状态时 我们就要显示警告
        bool show = stoveCounter.IsFried()&&e.progressNormalized>=burnShowProgressAmout;
        if (show)
        {
            Show();

        }
        else
        {
            Hide();
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
