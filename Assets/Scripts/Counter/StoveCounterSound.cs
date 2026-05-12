using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;
    private AudioSource audioSource;

    //用于每隔一段时间就播发警告
    private float warningSoundTimer;
    bool playWarningSound;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
        //监听进度改变以播放警告声音
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmout = .5f;
        //当炉灶进度超过50%且处于煎炸状态时 我们就要播发
        playWarningSound = stoveCounter.IsFried() && e.progressNormalized >= burnShowProgressAmout;
    }

    //监听处于什么状态
    private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangedEventArgs e)
    {
        bool playSound = e.state == StoveCounter.State.Frying || e.state == StoveCounter.State.Fried;
        if (playSound)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }
    private void Update()
    {
        if (playWarningSound)
        {
            //只有需要播放时才进行开始记时重置
            warningSoundTimer -= Time.deltaTime;
            if (warningSoundTimer <= 0f)
            {
                float waringSoundTimerMax = .2f;
                warningSoundTimer = waringSoundTimerMax;
                //播放声音
                SoundManager.Instance.PlayWarningSound(stoveCounter.transform.position);
            }
        }
     
        
    }
}
