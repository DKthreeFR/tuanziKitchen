using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestLobbyUI : MonoBehaviour
{
    [SerializeField] private Button createGameBtn;
    [SerializeField]private Button joinGameBtn;

    private void Awake()
    {
        createGameBtn.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.StartHost();
            //网络场景管理器 加载角色选择场景
            LoadNetworkScene("CharacterSelectScene");

        });
        joinGameBtn.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.StartClient();
        });
    }
    public void LoadNetworkScene(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    } 

}
