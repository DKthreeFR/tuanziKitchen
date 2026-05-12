using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliverManager : NetworkBehaviour
{
    //订单生成和订单交付 用来通知UI进行更新
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;

    //通知交付成功或是失败
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;
    public static DeliverManager Instance { get;private set;  }
    //获取到所有菜品配方
    [SerializeField] private RecipeListSO recipeListSO;
    //在等待中的菜品LIst
    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer=4f;
    private float spawnRecipeTimerMax = 4f;
    //最大订单数量
    private int waittingRecipeMax = 4;
    //记录成功交付数量
    private int successfulRecipesAmout;

    private void Awake()
    {
       Instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
    }
    private void Update()
    {
        //只允许服务器生成 如果不是服务器直接返回
        if (!IsServer)
        {
            return;
        }
        spawnRecipeTimer -= Time.deltaTime;
        if(spawnRecipeTimer <= 0)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;
            //当订单数小于这个最大数时才把他加入List 
            if (KitchenGameManager.Instance.IsGamePlaying()&&waitingRecipeSOList.Count <= waittingRecipeMax)
            {
                int waitingRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
                //客户端远程调用
               SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);
            }
          

        }
        
    }
    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex)
    {
        //时间一到随机选择一种菜品加入等待List模拟顾客提出要求
        RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[waitingRecipeSOIndex];
        Debug.Log(waitingRecipeSO.recipeName);
        waitingRecipeSOList.Add(waitingRecipeSO);
        //订单生成时触发事件 
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
    }
    //提交的菜品与正在等待中的订单是否匹配
    public void DeliverRecipe (PlateKitchenObjects plateKitchenObjects)
    {
        for(int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO  = waitingRecipeSOList[i];
            bool plateContentMatchesRecipe = true;
            //如果菜品配置和盘子里的配置一致（比如沙拉 订单里时卷心菜和番茄切片 盘子里也是卷心菜和番茄切片）
            if(waitingRecipeSO.kitchenObjectsSOList.Count == plateKitchenObjects.GetKitchenObjectsSOList().Count)
            {
                //遍历等待列表里的菜品配置
                foreach (KitchenObjectsSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectsSOList) { 
                bool ingredientFound = false;
                    //遍历盘子里的菜品配置
                    foreach(KitchenObjectsSO plateKitchenObjectSO in plateKitchenObjects.GetKitchenObjectsSOList())
                    {
                        if(plateKitchenObjectSO == recipeKitchenObjectSO)
                        {
                            //如果匹配则能正常交付
                            ingredientFound = true;
                            break;
                        }

                    }
                    if (!ingredientFound)
                    {
                        //没找到
                        plateContentMatchesRecipe = false;
                    }
                }
                if(plateContentMatchesRecipe)
                {
                    DeliverCorrectRecipeServerRpc(i);
                    return;
                }

            }
        }
        //交付错误
        DeliverInCorrectRecipeServerRpc();



    }

    //客户端通知服务端交付正确时事件
    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOListIndex)
    {
      DeliverCorrectRecipeClientRpc(waitingRecipeSOListIndex);
    }
    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOListIndex)
    {
        //玩家交互正确食谱
        Debug.Log("玩家正确交互食谱");
        //删除等待订单中的 因为已经交付了
        waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);
        //交付成功 成功数量加1
        successfulRecipesAmout++;
        //交付完成 通知UI进行更新
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        //播放音效
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }
    //通知交付错误
    [ServerRpc(RequireOwnership = false)]
    private void DeliverInCorrectRecipeServerRpc()
    {
        DeliverInCorrectRecipeClientRpc();
    }
    [ClientRpc]
    private void DeliverInCorrectRecipeClientRpc()
    {
        //玩家送错配方
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }
    public List<RecipeSO> GetWaitingRecipeSOList() {
        return waitingRecipeSOList;
    }
    //提供给外部成功交付的数量
    public int GetSuccessfulRecipesAmout()
    {
        return successfulRecipesAmout;
    }
}
