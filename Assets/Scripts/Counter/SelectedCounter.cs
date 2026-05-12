using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCounter : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    //视觉对象 即选中后会高亮的 表示高亮的哪个对象 clearCounter预制体里的第二个子对象
    [SerializeField] private GameObject[] visualGameObject;
    // Start is called before the first frame update
    void Start()
    {
        //先检测玩家实例是否存在 如果存在直接赋值 不存在就监听事件
        if (Player.LocalInstance != null) {
            Player.LocalInstance.onSelectedCounterChanged += Player_onSelectedCounterChanged;
        }
        else
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }
            foreach (GameObject item in visualGameObject)
            {
                item.SetActive(false);
            }
    }

    private void Player_OnAnyPlayerSpawned(object sender, System.EventArgs e)
    {
        //检测玩家实例是否存在 如果存在直接赋值
        if (Player.LocalInstance != null)
        {
            //如果第一个玩家是本地实例可能产生多个监听器 导致大量重复逻辑 所以先取消再定于
            Player.LocalInstance.onSelectedCounterChanged -= Player_onSelectedCounterChanged;
            Player.LocalInstance.onSelectedCounterChanged += Player_onSelectedCounterChanged;
        }
    }

    private void Player_onSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        //如果当前选中的对象是本对象
        if (e.selectedCounter == baseCounter)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }
    private void Show()
    {
        foreach (GameObject item in visualGameObject)
        {
            item.SetActive(true);
        }
    }
    private void Hide()
    {
        foreach (GameObject item in visualGameObject)
        {
            item.SetActive(false);
        }
    }   
}
