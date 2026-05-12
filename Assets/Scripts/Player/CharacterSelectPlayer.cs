using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CharacterSelectPlayer : MonoBehaviour
{
    //玩家索引
    [SerializeField] private int playerIndex;
        //玩家准备文字 我们只需要控制显示隐藏 表示准备状态
        [SerializeField] private GameObject readyText;
    //关联玩家视觉对象
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private TuanZiVisual tuanziVisual;
    //踢出玩家按钮引用
    [SerializeField] private Button kickBtn;
    //关联玩家名字
    [SerializeField] private TextMeshPro playerNameText;

    private void Awake()
    {
        kickBtn.onClick.AddListener(() =>
        {
            //获取id根据id提出对应玩家
            PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            KitchenGameMultiplayer.Instance.KickPlayer(playerData.clientId);
        });
    }
    private void Start()
    {
        //监听当玩家加入游戏 会引起网络列表变化 执行下方函数
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged += KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
        CharacterSelectReady.Instance.OnReadChanged += CharacterSelectReady_OnReadChanged;
        kickBtn.gameObject.SetActive(NetworkManager.Singleton.IsServer);//只有服务器才显示踢出玩家按钮
        UpdatePlayer();
    }

    private void CharacterSelectReady_OnReadChanged(object sender, System.EventArgs e)
    {
        //通知场上有人准备状态更新
        UpdatePlayer();
    }

    private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }
    private void UpdatePlayer()
    {
        if (KitchenGameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
        {
            Show();
            //根据当前玩家索引得到玩家在网络列表里的数据 然后判断是否已经准备如果准备就显示
            PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            readyText.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.clientId));
            playerNameText.text = playerData.playerName.ToString();
            //根据玩家索引值来设置初始颜色
            playerVisual.SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
            //根据玩家索引值来设置初始角色
            tuanziVisual.SetPlayerCharacter(KitchenGameMultiplayer.Instance.GetPlayerCharacterObject(playerData.characterId));
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
