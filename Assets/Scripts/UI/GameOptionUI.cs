using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameOptionUI : MonoBehaviour
{
    [SerializeField] private Button musicBtn;
    [SerializeField] private TextMeshProUGUI musicText;
    [SerializeField] private Button soundBtn;
    [SerializeField] private TextMeshProUGUI soundText;
    
    [SerializeField] private Button closeBtn;
    //关联以监听事件
    [SerializeField] private GamePauseUI GamePauseUI;

    //改建相关
    [SerializeField] private TextMeshProUGUI moveUpText;
    [SerializeField] private TextMeshProUGUI moveDownText;
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI moveRightText;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI interactAlternateText;
    [SerializeField] private TextMeshProUGUI pauseText;
    [SerializeField] private TextMeshProUGUI gamepadPauseText;
    [SerializeField] private TextMeshProUGUI gamepadInteractText;
    [SerializeField] private TextMeshProUGUI gamepadInteractAlternateText;
    [SerializeField] private Button moveUpBtn;
    [SerializeField] private Button moveDownBtn;
    [SerializeField] private Button moveLeftBtn;
    [SerializeField] private Button moveRightBtn;
    [SerializeField] private Button interactBtn;
    [SerializeField] private Button interactAlternateBtn;
    [SerializeField] private Button pauseBtn;
    [SerializeField] private Button gamepadPauseBtn;
    [SerializeField] private Button gamepadInteractBtn;
    [SerializeField] private Button gamepadInteractAlternateBtn;

    //提示按下任意键来绑定的界面
    [SerializeField] private GameObject rebindUI;
    //绑定暂停界面
    [SerializeField] private GamePauseUI gamePauseUI;
    private void Start()
    {
        //先更新记录
        UpdateVisual();
      
        musicBtn.onClick.AddListener(() =>
        {
            SoundManager.Instance.ChangeMusicVolume();
            UpdateVisual();

        });
        soundBtn.onClick.AddListener(() => {

            SoundManager.Instance.ChangeSoundVolume();
            UpdateVisual();


        });
        GamePauseUI.OnOption += GamePauseUI_OnOption;
        closeBtn.onClick.AddListener(() => {
            gamePauseUI.Show(); 
        gameObject.SetActive(false);
        });
        ////重新绑定
        moveUpBtn.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Up); });
        moveDownBtn.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Down); });
        moveLeftBtn.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Left); });
        moveRightBtn.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Right); });
        interactBtn.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Interact); });
        interactAlternateBtn.onClick.AddListener(() => { RebindBinding(GameInput.Binding.InteractAlternate); });
        pauseBtn.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Pause); });
        gamepadPauseBtn.onClick.AddListener(() =>{RebindBinding(GameInput.Binding.Gamepad_Pause);});
        gamepadInteractBtn.onClick.AddListener(() =>{RebindBinding(GameInput.Binding.Gamepad_Interact);});
        gamepadInteractAlternateBtn.onClick.AddListener(() =>{RebindBinding(GameInput.Binding.Gamepad_InteractAlternate);});
        gameObject.SetActive(false);
    }

    private void GamePauseUI_OnOption(object sender, System.EventArgs e)
    {
        gameObject.SetActive(true);
        //优先选中 允许导航
        musicBtn.Select();
    }

    public void UpdateVisual()
    {
        musicText.text = "Music:" + Math.Round(SoundManager.Instance.musicVolume,1).ToString();
        soundText.text = "Sound:" + Math.Round(SoundManager.Instance.soundVolume,1).ToString();

        moveUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Up);
        moveDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Down);
        moveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Left);
        moveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Right);
        interactText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
        interactAlternateText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlternate);
        pauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause);
        gamepadPauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_Pause);
        gamepadInteractText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_Interact);
        gamepadInteractAlternateText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_InteractAlternate);
      

    }
    private void ShowRebindUI()
    {
        rebindUI.SetActive(true);
       
    }
    private void HideRebindUI()
    {   
        rebindUI?.SetActive(false);
    }
    //重绑定方法
    private void RebindBinding(GameInput.Binding binding)
    {
        ShowRebindUI();
       GameInput.Instance.Rebinding(binding, () =>
        {
            //重绑后关闭提示界面 以及更新新的按键显示
            HideRebindUI();
            UpdateVisual();
        });
    }
}
 