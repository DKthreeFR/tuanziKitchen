using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliverResultUI : MonoBehaviour
{
    private const string POPUP = "Popup";
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI messageText;

    //关联对应的成功和失败的颜色和图片以便下面监听到对应事件后进行切换显示
    [SerializeField] private Color successColor;
    [SerializeField] private Color failedColor;

    [SerializeField] private Sprite iconSuccess;
    [SerializeField] private Sprite iconFailed;

    //获取动画
    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        DeliverManager.Instance.OnRecipeSuccess += DeliverManager_OnRecipeSuccess;
        DeliverManager.Instance.OnRecipeFailed += DeliverManager_OnRecipeFailed;
        //开始时先隐藏
        gameObject.SetActive(false);
    }

    private void DeliverManager_OnRecipeFailed(object sender, System.EventArgs e)
    {
        //触发动画
        gameObject.SetActive(true);
        backgroundImage.color = failedColor;
        animator.SetTrigger(POPUP);
       
        iconImage.sprite = iconFailed;
        messageText.text = "DELIVERED\nWRONG";
    }

    private void DeliverManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        gameObject.SetActive(true);

        backgroundImage.color = successColor;
        animator.SetTrigger(POPUP);

        iconImage.sprite = iconSuccess;
        messageText.text = "DELIVERED\nRIGHT";
    }
}
