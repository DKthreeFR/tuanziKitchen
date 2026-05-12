using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStartCountDownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    //获取动画控制器
    private Animator animator;
    //记录当前数字是否和之前不同
    private int previouseCountdownNumber;
    private void Start()
    {
        animator = GetComponent<Animator>();
        KitchenGameManager.Instance.OnStateChange += KitchenGameManager_OnStateChange;
        gameObject.SetActive(false);
    }
    public void Update()
    {
        int countdownNumber = Mathf.CeilToInt(KitchenGameManager.Instance.GetCountdownToStartTimer());
        //实时更新数据
        countdownText.text = countdownNumber.ToString();
        if(previouseCountdownNumber!=countdownNumber)
        {
            previouseCountdownNumber = countdownNumber;
            animator.SetTrigger("NumberPopUp");
            //播放倒计时音效
            SoundManager.Instance.PlayCountdownSound();
        }
    }
    private void KitchenGameManager_OnStateChange(object sender, System.EventArgs e) 
    {
        if (KitchenGameManager.Instance.IsCountdownToStartActive())
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
