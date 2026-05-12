using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSingleUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private int characterId;
    //按钮颜色
    [SerializeField] private Image image;
    [SerializeField] private Image tuanziSprite;
    //表示选中状态
    [SerializeField] private GameObject selectedGameObjetct;
    [SerializeField] private Button button;

    private void Awake()
    {
        
        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            //按下按钮改变颜色
            KitchenGameMultiplayer.Instance.ChangePlayerColor(colorId);
            KitchenGameMultiplayer.Instance.ChangePlayerCharacter(characterId);
        });
    }

    private void Start()
    {
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged += KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
        //image.color = KitchenGameMultiplayer.Instance.GetPlayerColor(colorId);
        tuanziSprite.sprite = KitchenGameMultiplayer.Instance.GetPlayerCharacterSprite(characterId);
        UpdateIsSelected();
    }

    private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        //玩家列表改变（比如有玩家加入 那就更新显示）
        UpdateIsSelected();
    }

    private void UpdateIsSelected()
    {
        if(KitchenGameMultiplayer.Instance.GetPlayerData().colorId == colorId)
        {
            //如果玩家当前选择的颜色是这个颜色 那么显示选中
            selectedGameObjetct.SetActive(true);
        }
        else
        {
            selectedGameObjetct.SetActive(false);
        }
    }
    //销毁时取消订阅
    private void OnDestroy()
        {
            KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
    }


}
