using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounterVisual: MonoBehaviour
{
    //关联柜台顶部 和 盘子预设体
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform plateVisualPrefab;
    //获得盘子柜台以拿到事件来添加
    [SerializeField] private PlatesCounter platesCounter;
     
    //记录生成了多少个盘子 然后根据盘子数量确定新创建的盘子要向上偏移多少（模拟盘子能够叠起来）
    private List<GameObject> plateVisualGameObjectList;
    private void Start()
    {
        platesCounter.OnPlateSpawned += PlatesCounter_OnPlateSpawned;
        platesCounter.OnPlateRemoved += PlatesCounter_OnPlateRemoved;
        plateVisualGameObjectList = new List<GameObject>();
    }

    private void PlatesCounter_OnPlateRemoved(object sender, System.EventArgs e)
    {
        //监听到盘子被拿走把最后一个盘子销毁掉
        GameObject plateGameObject = plateVisualGameObjectList[plateVisualGameObjectList.Count - 1];
        plateVisualGameObjectList.Remove(plateGameObject);
        Destroy(plateGameObject);
    } 

    private void PlatesCounter_OnPlateSpawned(object sender, System.EventArgs e)
    {
        Transform plateVisualTransform = Instantiate(plateVisualPrefab,counterTopPoint);
        //根据数量创建偏移
        float plateOffSetY = 0.1f;
        plateVisualTransform.localPosition = new Vector3(0,plateOffSetY*plateVisualGameObjectList.Count,0);
        plateVisualGameObjectList.Add(plateVisualTransform.gameObject);
    }
}
