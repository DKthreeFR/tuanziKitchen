using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrashCounter : BaseCounter
{
    //因为也可能有多个垃圾桶 所以我们使用静态
    public static event EventHandler OnAnyObjectTrashed;
    //重置静态监听
  new    public static void ResetStaticData()
    {
        OnAnyObjectTrashed = null;
    }
    public override void Intertact(Player player)
    {
        //如果玩家身上有物品 就删除物品
        if (player.HasKitchenObject()){
        KitchenObjects.DestoryKitchenObject(player.GetKitchenObjects());
            InteractLogicClientRpc();
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

        //触发丢垃圾的声音
        OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty);
    }
}
