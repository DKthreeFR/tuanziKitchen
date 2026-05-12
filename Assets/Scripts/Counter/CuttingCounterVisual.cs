using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounterVisual : MonoBehaviour
{
    private const string CUT = "Cut";
    //得到Containercounter以便向其中的事件添加监听
    [SerializeField] private CuttingCounter cuttingCounter;
    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        //监听
        cuttingCounter.Oncut += ContainerCounter_OnPlayerGrabbedObject;
    }
    //播放动画的方法
    private void ContainerCounter_OnPlayerGrabbedObject(object sender, EventArgs e)
    {
        animator.SetTrigger(CUT);
    }
}
