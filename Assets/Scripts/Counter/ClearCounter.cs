using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    //相关的一些方法写在基类里面了 这里的代码是重构过的
    //选中后的高亮逻辑再SelectedCounterl里实现了
    //本对象 挂载脚本的自己
    [SerializeField] private ClearCounter clearCounter;

    [SerializeField] private KitchenObjectsSO kitchenObjectsSO;
    private void Start()
    {
    }

    //柜台交互
    public override void  Intertact(Player player)
    {
        if (!HasKitchenObject())
        {
            //这个干净柜台上没有物体
            if (player.HasKitchenObject())
            {
                //玩家手上有物体
                //调用获取到玩家手上物体的方法
                player.GetKitchenObjects().SetKitchenObjectsParents(this);
               
            }
            else
            {
                //玩家手上没物体 不做任何事情
            }
        }else
        {
            //这个柜子上有物体
            if (player.HasKitchenObject())
            {
                //如果玩家拿的是盘子
                if(player.GetKitchenObjects().TryGetPlate(out PlateKitchenObjects plateKitchenObjects))
                {
                    //把当前柜子上的菜品放到盘子里
                   if(plateKitchenObjects.TryAddIngredient(GetKitchenObjects().GetKitchenObjectsSO()))
                    {
                        //如果成功添加再销毁
                      
                        KitchenObjects.DestoryKitchenObject(GetKitchenObjects());

                    }
                }//如果玩家拿的不是盘子 而是其他相关东西
                else
                {
                    //如果柜台上是盘子
                    if(GetKitchenObjects().TryGetPlate(out plateKitchenObjects))
                    {
                        if (plateKitchenObjects.TryAddIngredient(player.GetKitchenObjects().GetKitchenObjectsSO()))
                        {
                            //销毁对象
                            KitchenObjects.DestoryKitchenObject(player.GetKitchenObjects());
                        }
                    }
                }

            }
            else
            {
                //玩家手上没物体
                clearCounter.GetKitchenObjects().SetKitchenObjectsParents(player);
            }
        }

    }
   
}
