using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button ExitToMenuBtn;
    [SerializeField] private Button optionBtn;

    public event EventHandler OnOption;

    private void Awake()
    {
      
        continueBtn.onClick.AddListener(() =>
        {
            KitchenGameManager.Instance.TogglePauseGame();
        });
        ExitToMenuBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MainMenuScene");
        });
        //通知设置界面显示
        optionBtn.onClick.AddListener(() => {
            Hide();
            OnOption?.Invoke(this, EventArgs.Empty);
        });

    }

    private void Start()
    {
        //监听暂停开启或关闭
        KitchenGameManager.Instance.OnLocalPauseUIAction += KitchenGameManager_OnLocalPauseAction;
        KitchenGameManager.Instance.OnLocalUnPauseUIAction += KitchenGameManager_OnLocalUnPauseAction;
        //一定要先监听了事件才把对象设置为false
        gameObject.SetActive(false);

    }
  
  

    private void KitchenGameManager_OnLocalPauseAction(object sender, System.EventArgs e)
    {
        Show();
    }
    private void KitchenGameManager_OnLocalUnPauseAction(object sender, System.EventArgs e)
    {
        Hide();
    }

   public void Show()
    {
       this.gameObject.SetActive(true);
        continueBtn.Select();
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
    

}   
