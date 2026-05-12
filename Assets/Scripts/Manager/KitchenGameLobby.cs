using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class KitchenGameLobby : MonoBehaviour
{
    public static KitchenGameLobby Instance { get; private set; }
    //创建大厅（用于通知显示UI) 失败
    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinStarted;
    public event EventHandler OnJoinFaied;
    public event EventHandler OnQuickJoinFailed;
    //大厅列表相关的事件
    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }
    private Lobby joinedLobby; 
    //心跳计时器
    private float heartbeatTimer;
    //定期刷新lobbylist的计时器
    private float listLobbiesTimer;
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeUnityAuthentication();

    }
    //初始化unity服务
    private async void InitializeUnityAuthentication()
    {
        //检擦初始化状态如果没初始化就初始化 已经初始化就不允许初始化 避免重复初始化
        if(UnityServices.State != ServicesInitializationState.Initialized)
        {
            //每次使用不同的配置文件初始化 以便在同一台设备上测试多个玩家
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

            //初始化
            await UnityServices.InitializeAsync(initializationOptions);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        
        
    }
    private void Update()
    {
        HandleHeartbeat();
        HandlePeriodicListLobbies();


    }
    //定期刷新大厅列表的方法 每5秒刷新一次
    private void HandlePeriodicListLobbies()
    {
        //玩家尚未加入大厅才刷新大厅列表 只检查登录状态即可 确保只再大厅列表界面刷新
        if (joinedLobby == null&&AuthenticationService.Instance.IsSignedIn&&SceneManager.GetActiveScene().name == "LobbyScene")
        {
            listLobbiesTimer -= Time.deltaTime;
            if (listLobbiesTimer <= 0f)
            {
                float listLobbiesTimerMax = 5f;
                listLobbiesTimer = listLobbiesTimerMax;
                ListLobbies();
            }
        }
        
    }

    private void HandleHeartbeat()
    {
        if (IsLobbyHost())
        {
            //倒计时
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                //发送心跳
                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }
    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;

    }
    //列出所有大厅的方法
    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
            {//只显示还有空位大于0的大厅
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            //返回的结果是一个列表
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = queryResponse.Results });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);

        }
    }
    //分离中继Relay的方法
    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            //连接数要减去去主机所以-1 会返回一个中继对象
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(KitchenGameMultiplayer.MAX_PLAYER_AMOUNT - 1);
            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }
    //获得加入中继
    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }
    //通过代码加入Relay的功能
    private async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }
    //创建大厅的方法 大厅名字和是否私密
    public async void CreateLobby(string lobbyName,bool isPrivate)
    {
        //创建成功
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
        //因为不可用 一下先注释
        try
        {
            //第一个参数 大厅名字 第二个参数 大厅最大人数 第三个参数 大厅选项
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4, new CreateLobbyOptions
            {
                IsPrivate = isPrivate
            });
            //    //分配中继
            //    Allocation allocation=await AllocateRelay();
            //    string relayJoinCode = await GetRelayJoinCode(allocation);
            //    //更新大厅数据 将中继的加入代码存储在大厅数据中 以便其他玩家加入时可以获取
            //    await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            //    {
            //        Data = new Dictionary<string, DataObject>
            //        {
            //            { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
            //        }
            //    });
            //    NetworkManager.Singleton.GetComponent<UnityTransport>()
            //                            .SetRelayServerData(new RelayServerData(allocation, "dtls"));



            //开启主机
            KitchenGameMultiplayer.Instance.StartHost();
            //网络场景管理器 加载角色选择场景 
            LoadNetworkScene("CharacterSelectScene");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            //创建失败
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
     
    }
    //加入大厅的方法
    public async void QuickJoin()
    {
        //触发加入的Ui显示
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            //不可用 先注释
            ////加入Relay
            //string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value; 
            //JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            //NetworkManager.Singleton.GetComponent<UnityTransport>()
            //                       .SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            //开启客户端
            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            //加入失败
            OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }
    public void LoadNetworkScene(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    //通过代码加入大厅
    public async void JoinWithCode(string lobbyCode)
    {
        //触发加入的Ui显示
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            ////加入Relay
            //string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;
            //JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            //NetworkManager.Singleton.GetComponent<UnityTransport>()
            //                       .SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            KitchenGameMultiplayer.Instance.StartClient();

        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
            OnJoinFaied?.Invoke(this, EventArgs.Empty);
        }
    }
    //通过Id加入大厅
    public async void JoinWithId(string lobbyId)
    {
        //触发加入的Ui显示
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            ////加入Relay
            //string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;
            //JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            //NetworkManager.Singleton.GetComponent<UnityTransport>()
            //                       .SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            KitchenGameMultiplayer.Instance.StartClient();

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnJoinFaied?.Invoke(this, EventArgs.Empty);
        }
    }
    //返回加入大厅的对象
    public Lobby GetLobby()
    {
        return joinedLobby;
    }
    
    //删除大厅的方法
    public async void DeleteLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
    //确保玩家离开大厅的方法
    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                joinedLobby = null;

            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
       
    }
    //踢出玩家的方法
    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);

            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

    }
}
 