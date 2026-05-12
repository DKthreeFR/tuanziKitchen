using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeachUI : MonoBehaviour
{
    private void Awake()
    {
       
    }
    private void Start()
    {
        if (KitchenGameManager.Instance.IsWaitingToStartActive())
        {
            Show();
        }
        KitchenGameManager.Instance.OnStateChange += KitchenManager_OnStateChange;
        KitchenGameManager.Instance.OnLocalPlayerReadyChanged += KitchenGameManager_OnLocalPlayerReadyChanged;
    }

    private void KitchenGameManager_OnLocalPlayerReadyChanged(object sender, EventArgs e)
    {
        if(KitchenGameManager.Instance.IsLocalPlayerReady())
        {
            Hide();
        }
    }

    private void KitchenManager_OnStateChange(object sender, System.EventArgs e)
    {
      
        if (KitchenGameManager.Instance.IsCountdownToStartActive())
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
