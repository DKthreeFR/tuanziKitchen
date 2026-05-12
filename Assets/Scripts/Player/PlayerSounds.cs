using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private Player player;
    private float footsteoTimer;
    //这个很重要用来控制播放时长
    private float footsteoTimerMax=.2f;
    private void Awake()
    {
        player = GetComponent<Player>();
    }
    private void Update()
    {
        footsteoTimer-=Time.deltaTime;
        if (footsteoTimer < 0f)
        {
            footsteoTimer = footsteoTimerMax;
            //播放声音  
            //玩家在行走时才播放
            if (player.IsWalking())
            {
                float volume = 1f;

                SoundManager.Instance.PlayFootStepSound(player.transform.position, volume);

            }

        }
    }
}
