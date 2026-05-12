using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    //通知视觉脚本 盘子生成
    public event EventHandler OnPlateSpawned;
    //通知视觉脚本 盘子被拿走
    public event EventHandler OnPlateRemoved;

    [SerializeField] private KitchenObjectsSO plateKitchenObjectsSO;
    //每隔一段时间生成一个盘子
    private float spawnPlateTimer;
    private float spawnPlateTimerMax=4f;
    private int plateSpawnedAmount;
    private int plateSpawnedAmounMax=5;

    public void Update()
    {
        //将生成盘子的逻辑交给服务端 不是服务端直接返回
        if (!IsServer)
        {
            return;
        }
        spawnPlateTimer += Time.deltaTime;
        if (spawnPlateTimer > spawnPlateTimerMax)
        {
            spawnPlateTimer = 0;
            SpawnPlateServerRpc();
            //时间到生成盘子
            if (KitchenGameManager.Instance.IsGamePlaying() && plateSpawnedAmount < plateSpawnedAmounMax)
            { 
                //服务端调用通知每个客户端进行触发
                SpawnPlateServerRpc();
            }
        }
      

    }
    [ServerRpc]
    private void SpawnPlateServerRpc()
    {
        SpawnPlateClientRpc();
    }
    [ClientRpc]
    private void SpawnPlateClientRpc()
    {
        plateSpawnedAmount++;
        //通知视觉脚本创建盘子
        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }
    public override void Intertact(Player player)
    {
        //如果玩家手上没有盘子
        if (!player.HasKitchenObject()&&plateSpawnedAmount>0)
        {
            //生成已经同步 所以不用放进去
            KitchenObjects.SpawnKitchenObjects(plateKitchenObjectsSO, player);
            InteractLogicServerRpc();
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

        plateSpawnedAmount--;
        //玩家手上拿到盘子
     
        //通知视觉效果盘子被拿走
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
}
