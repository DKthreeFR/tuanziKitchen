using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    public static DeliveryCounter Instance {  get; private set; }
    public void Awake()
    {
        Instance = this;
    }
    public override void Intertact(Player player)
    {
        //检测玩家是否有物品 
        if (player.HasKitchenObject())
        {
            //检测玩家拿着的是不是盘子 如果是盘子才允许销毁（销毁即模拟送出去）
            if(player.GetKitchenObjects().TryGetPlate(out PlateKitchenObjects plateKitchenObjects))
            {
                //调用处理方法
                DeliverManager.Instance.DeliverRecipe(plateKitchenObjects);
                KitchenObjects.DestoryKitchenObject(player.GetKitchenObjects());
               
            }
        }
    }

}
