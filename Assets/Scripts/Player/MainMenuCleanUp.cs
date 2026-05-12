using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanUp : MonoBehaviour
{
    private void Awake()
    {
        //如果回到主界面他们还在就执行清理操作
        if(NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if(KitchenGameMultiplayer.Instance != null)
        {
            Destroy(KitchenGameMultiplayer.Instance.gameObject);
        }
        if(KitchenGameLobby.Instance != null)
            {
               Destroy(KitchenGameLobby.Instance.gameObject);
        }
    }
}
