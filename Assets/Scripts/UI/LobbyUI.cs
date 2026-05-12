using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuBtn;
    [SerializeField] private Button createLobbyBtn;
    [SerializeField] private Button quickJoinBtn;
    //引用创建界面
    [SerializeField] private LobbyCreateUI lobbyCreateUI;
    [SerializeField] private TMP_InputField playerNameInputField;


    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private Button codeJoinBtn;

    //大厅列表
    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;

    private void Awake()
    {
        mainMenuBtn.onClick.AddListener(() =>
        {
            
            SceneManager.LoadScene("MainMenuScene");
        });
        createLobbyBtn.onClick.AddListener(() =>
        {
            lobbyCreateUI.Show();
        });
        quickJoinBtn.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.QuickJoin();
        });
        codeJoinBtn.onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.JoinWithCode(lobbyCodeInputField.text);
        });
        lobbyTemplate.gameObject.SetActive(false);
    }
    private void Start()
    {
        playerNameInputField.text = KitchenGameMultiplayer.Instance.GetPlayerName();
        //如果玩家名称发生改变 unity提供了一个回调函数
        playerNameInputField.onValueChanged.AddListener((value) =>
        {
            //玩家名称记录修改名称
            KitchenGameMultiplayer.Instance.SetPlayerName(value);
        });
        //监听列表数的变化
        KitchenGameLobby.Instance.OnLobbyListChanged += KitchenGameLobby_OnLobbyListChanged;

        UpdateLobbyList(new List<Lobby>());

    }

    private void KitchenGameLobby_OnLobbyListChanged(object sender, KitchenGameLobby.OnLobbyListChangedEventArgs e)
    {
        //列表数发生变化时 更新大厅列表
        UpdateLobbyList(e.lobbyList);
    }

    //更新大厅列表
    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach(Transform child in lobbyContainer)
        {
            //先销毁除模板外的子游戏对象
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }
        foreach(Lobby lobby in lobbyList)
        {
            //根据模板创建一个新的游戏对象
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            //设置显示的文本
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);

        }
    }
    private void OnDestroy()
    {
        KitchenGameLobby.Instance.OnLobbyListChanged -= KitchenGameLobby_OnLobbyListChanged;
    }
}
 