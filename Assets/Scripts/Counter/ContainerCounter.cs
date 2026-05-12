using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContainerCounter : BaseCounter
{
    [SerializeField] private KitchenObjectsSO kitchenObjectsSO;

    //玩家打开柜子时触发事件 比如用于ContainerCounterViusal里的显示动画的逻辑
    public event EventHandler OnPlayerGrabbedObject;

    public override void Intertact(Player player)
    {
        Debug.Log("交互");
        //已有物体避免重复创建
        if (!HasKitchenObject())
        {
            //玩家没物体在手上时才允许拿起
            if (!player.HasKitchenObject())
            {
                //生成已同步
                KitchenObjects.SpawnKitchenObjects(kitchenObjectsSO, player);
                InteractLogicServerRpc();


            }
           

        }
    


    }
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }
    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
    }

}
