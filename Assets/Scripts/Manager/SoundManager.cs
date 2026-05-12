using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    DeliveryCounter deliveryCounter;
    //拿到所有音频
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;
    [SerializeField] private AudioSource BKMusic;
    //播放音频
    public float soundVolume;
    public float musicVolume;
    private const string GAME_MUSIC_VOLUME = "KitchenGameMusicVolume";
    private const string GAME_SOUND_VOLUME = "KitchenGameSoundVolume";

    private void Awake()
    {
        Instance = this;
        //读取音量 默认为1
        musicVolume =PlayerPrefs.GetFloat( GAME_MUSIC_VOLUME,1f );
        soundVolume = PlayerPrefs.GetFloat( GAME_SOUND_VOLUME,1f );
        BKMusic.volume = musicVolume;

    }
    private void PlaySound(AudioClip audioClip, Vector3 position, float volume = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volume);
    }
    //一个行为有多种音效的情况
    private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
    {
        PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
    }
    private void Start()
    {
        //对送出失败和成功的监听
        DeliverManager.Instance.OnRecipeSuccess += DeliverManager_OnRecipeSuccess;
        DeliverManager.Instance.OnRecipeFailed += DeliverManager_OnRecipeFailed;
        //监听切菜
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        //玩家拾取物品 玩家放下物品（玩家放下物品即柜子得到物品）
        Player.OnAnyPickedSomething += Player_OnPickedSometh;
        BaseCounter.OnAnyObjectPlacedHere += BaseCounter_OnAnyObjectPlacedHere;
        //丢垃圾
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
        
    }

    private void TrashCounter_OnAnyObjectTrashed(object sender, System.EventArgs e)
    {
        TrashCounter trashCounter = sender as TrashCounter;
        PlaySound(audioClipRefsSO.trash, trashCounter.transform.position,soundVolume);
    }

    private void BaseCounter_OnAnyObjectPlacedHere(object sender, System.EventArgs e)
    {
        BaseCounter baseCounter = sender as BaseCounter;
        PlaySound(audioClipRefsSO.objectDrop, baseCounter.transform.position, soundVolume);
    }

    private void Player_OnPickedSometh(object sender, System.EventArgs e) 
    {
        Player player = sender as Player;
        PlaySound(audioClipRefsSO.objectPickup, player.transform.position, soundVolume);
    }

    private void CuttingCounter_OnAnyCut(object sender, System.EventArgs e)
    {
        CuttingCounter cuttingCounter = sender as CuttingCounter;
        PlaySound(audioClipRefsSO.chop, cuttingCounter.transform.position, soundVolume);
    }

    private void DeliverManager_OnRecipeFailed(object sender, System.EventArgs e)
    {
        deliveryCounter = DeliveryCounter.Instance;
        //播放送出成功音效
        PlaySound(audioClipRefsSO.deliveryFail, deliveryCounter.transform.position, soundVolume);
    }

    private void DeliverManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        deliveryCounter = DeliveryCounter.Instance;
        //播放送出失败音效
        PlaySound(audioClipRefsSO.deliverySuccess, deliveryCounter.transform.position, soundVolume);

    }
    //脚步声音
    public void PlayFootStepSound(Vector3 pos,float volume)
    {
        PlaySound(audioClipRefsSO.footstep, pos,soundVolume);
    }
    //播发倒计时音效
    public void PlayCountdownSound()
    {
        PlaySound(audioClipRefsSO.warning, Vector3.zero);
    }
    //提供方法 通过点击按钮循环增加音量 并通过PlayerPref来持久化
    public void ChangeMusicVolume()
    {
        musicVolume = (musicVolume+0.1f)% 1.1f;
        PlayerPrefs.SetFloat(GAME_MUSIC_VOLUME,musicVolume);
        PlayerPrefs.Save();
        BKMusic.volume = musicVolume;


    }
    //快烧焦了警告音效
    public void PlayWarningSound(Vector3 position)
    {
        PlaySound(audioClipRefsSO.warning, position);
    }
    public void ChangeSoundVolume()
    {
        soundVolume = (soundVolume + 0.1f) % 1.1f;
        PlayerPrefs.SetFloat(GAME_SOUND_VOLUME,soundVolume);
        PlayerPrefs.Save();

    }
}
