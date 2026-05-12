using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectReady : NetworkBehaviour 
{
    //加载场景
    [SerializeField] private GameObject LoadingPanel;
    //进度条
    [SerializeField] private Slider progressSlider;
    //通知准备的事件
    public event EventHandler OnReadChanged;
    public static CharacterSelectReady Instance { get; private set; }
    //玩家准备字典
    private Dictionary<ulong, bool> playerReadyDictionary;
    private void Awake()
    {
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }
    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }
    //通知玩家加入准备字典 (检查是否准备）
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        //向客户端发送已准备的id 由此所有客户端可以知道哪些id已经准备
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);
        //字典里标记未已准备
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;
        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                //如果任意玩家没准备 
                allClientsReady = false;
                break;

            }
        }
        Debug.Log("玩家准备状态：" + allClientsReady);
        //所有玩家准备就绪 就切换状态
        if (allClientsReady)
        {
           
            //通知每个客户端都执行加载的显示 但只有服务端真正发号施令切换场景
            StartClientLoadingClientRpc("GameScene");


        }
    }
    //通知每个客户端的准备状态
    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId)
    {
        playerReadyDictionary[clientId] = true;
        OnReadChanged?.Invoke(this, EventArgs.Empty);

    }
    [ClientRpc]
    private void StartClientLoadingClientRpc(string sceneName)
    {
        // 每个客户端（包括主机）收到指令后，各自在本地启动协程
        StartCoroutine(FakeLoadingAndSwitch(sceneName));
    }

    // 协程：先播放假进度条，再切换场景
    private IEnumerator FakeLoadingAndSwitch(string sceneName)
    {
        // 1. 显示加载界面
        LoadingPanel.SetActive(true);
        progressSlider.value = 0;

        // 2. 模拟加载进度 (耗时1秒)
        float duration = 1.0f;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // 线性增加进度条
            progressSlider.value = timer / duration;
           
            yield return null;
        }

        // 3. 确保进度条走满
        progressSlider.value = 1;
        if (IsServer)
        {
            //确保只有服务端能够执行场景切换
            LoadNetworkScene(sceneName);

        }

        // 4. 稍微停顿一小下让视觉跟上，然后执行真正的加载
        // 注意：这里会阻塞主线程，但因为界面已经在显示“100%”，所以用户体验尚可
        yield return new WaitForSeconds(0.1f);
        if (IsServer)
        {
            //全部准备后删除大厅 开始进行场景跳转
            KitchenGameLobby.Instance.DeleteLobby();
        }

        // 5. 调用 NetworkManager 的同步加载
        // 这一步会导致画面卡住，直到场景加载完毕
       
    }

    public void LoadNetworkScene(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    //返回对应id玩家的准备状态 
    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionary.ContainsKey(clientId)&&playerReadyDictionary[clientId];
    }

}
