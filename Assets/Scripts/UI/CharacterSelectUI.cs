using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuBtn;
    [SerializeField] private Button readyBtn;

    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    private void Awake()
    {
        mainMenuBtn.onClick.AddListener(() =>
        {
            //退出游戏大厅
            KitchenGameLobby.Instance.LeaveLobby();
            //返回主菜单关闭网络连接
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MainMenuScene");
        });
        readyBtn.onClick.AddListener(() =>
        {
            //设置为准备状态
            CharacterSelectReady.Instance.SetPlayerReady();
        });
    }
    private void Start()
    {
        Lobby lobby = KitchenGameLobby.Instance.GetLobby();
        lobbyNameText.text = "Lobby Name: " + lobby.Name;
        lobbyCodeText.text ="LobbyCode: " + lobby.LobbyCode;
    }
}
