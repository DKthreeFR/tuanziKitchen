using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    private const string IS_WALKING = "IsWalking";
    private Animator animator;
    //获得到Player脚本里是否处于行走状态 私有 但加上特性可以再窗口里显示出来 进行关联
    [SerializeField]
    private Player player;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //如果不是本地玩家 直接返回
        if (!IsOwner)
        {
            return;
        }
        animator.SetBool(IS_WALKING, player.IsWalking());
    }
}
