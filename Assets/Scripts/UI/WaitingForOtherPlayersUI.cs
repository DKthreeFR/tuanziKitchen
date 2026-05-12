using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForOtherPlayersUI : MonoBehaviour
{
    private void Start()
    {
        KitchenGameManager.Instance.OnLocalPlayerReadyChanged += KitchenGameManager_OnLocalPlayerReadyChanged;
        KitchenGameManager.Instance.OnStateChange += KitchenManager_OnStateChange;

        Hide();
    }

    private void KitchenManager_OnStateChange(object sender, EventArgs e)
    {
        //全部准备进入倒计时开始状态 隐藏等待界面
        if (KitchenGameManager.Instance.IsCountdownToStartActive())
        {
            Hide();
        }        
    }

    private void KitchenGameManager_OnLocalPlayerReadyChanged(object sender, System.EventArgs e)
    {
        //玩家准备好了 其他玩家还没准备好 显示等待界面
        if (KitchenGameManager.Instance.IsLocalPlayerReady())
        {
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
