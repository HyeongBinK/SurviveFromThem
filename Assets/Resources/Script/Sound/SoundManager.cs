using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BGMLIST
{
    START = 0,
    TownAndCity = 0,
    Forest,
    Desert,
    DestroyedCity,
    Boss_1,
    Boss_2,
    Boss_3,
    END
}

public class SoundManager : MonoBehaviour
{

    public static SoundManager Instance { get; private set; }
    private void Awake()
    {
        if (null == Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        Destroy(gameObject);
    }

    [SerializeField] private AudioSource m_BGMPlayer; //BGM 재생(몬스터가 소리에 무 반응)
    [SerializeField] private AudioSource m_EffectSoundPlayer; //효과음재생(몬스터의 울음소리나 스킬효과음)(몬스터가 소리에 무 반응)
    [SerializeField] private AudioSource m_FootStepSoundPlayer; // 발걸음 소리 재생플레이어(몬스터가 소리에 반응)
    [SerializeField] private AudioSource m_FireSoundPlayer; //플레이어가 총을 쏘는소리 재생 플레이어(몬스터가 소리에 반응)


    [SerializeField] private AudioClip[] m_BGMAudioClips; //BGM 리스트
    [SerializeField] private AudioClip[] m_SFXAudioClips;  //효과음 리스트
    [SerializeField] private AudioClip[] m_FireAudioClips; //총기 효과음 리스트

    private void Start()
    {
        // audioClips = Resources.LoadAll<AudioClip>("Sound" + ""); // 모두다 로드해오긴 다운받은 파일이 너무많음
        if (m_BGMPlayer) m_BGMPlayer.loop = true;
        if (m_EffectSoundPlayer) m_EffectSoundPlayer.loop = false;
        if (m_FootStepSoundPlayer) m_FootStepSoundPlayer.loop = false;
        if (m_FireSoundPlayer) m_FireSoundPlayer.loop = false;
        PlayBGM(BGMLIST.DestroyedCity.ToString());
    }

    public void PlayBGM(string name)
    {
        if (m_BGMPlayer && 0 < m_BGMAudioClips.Length)
        {
            foreach (var clip in m_BGMAudioClips)
            {
                if (clip.name.ToLower().Equals(name.ToLower()))
                {
                    m_BGMPlayer.clip = clip;
                    m_BGMPlayer.Play();
                    break;
                }
            }
        }
    }

    public void StopBGM()
    {
        if (m_BGMPlayer)
        {
            m_BGMPlayer.Stop();
        }
    }

    public void PlaySFX(string name)
    {
        if (m_EffectSoundPlayer && 0 < m_SFXAudioClips.Length)
        {
            foreach (var clip in m_SFXAudioClips)
            {
                if (clip.name.ToLower().Equals(name.ToLower()))
                {
                    m_EffectSoundPlayer.PlayOneShot(clip);
                    break;
                }
            }
        }
    }
    public void PlayFootStepSound(string name)
    {
        if (m_FootStepSoundPlayer && 0 < m_SFXAudioClips.Length)
        {
            foreach (var clip in m_SFXAudioClips)
            {
                if (clip.name.ToLower().Equals(name.ToLower()))
                {
                    m_FootStepSoundPlayer.PlayOneShot(clip);
                    break;
                }
            }
        }
    }
    public bool GetIsFootStepSoundPlaying() { return m_FootStepSoundPlayer.isPlaying; }
    public void SetFootStepSoundLoop()
    {
        m_FootStepSoundPlayer.loop = true;
    }

    public void StopFootStepSound()
    {
        if (m_FootStepSoundPlayer.isPlaying)
            m_FootStepSoundPlayer.Stop();

        m_FootStepSoundPlayer.loop = false;
    }

    public void PlayFireSound(string name)
    {
        if (m_FireSoundPlayer && 0 < m_FireAudioClips.Length)
        {
            foreach (var clip in m_FireAudioClips)
            {
                if (clip.name.ToLower().Equals(name.ToLower()))
                {
                    m_FireSoundPlayer.PlayOneShot(clip);
                    break;
                }
            }
        }
    }
    public bool GetIsFireSoundPlaying() { return m_FireSoundPlayer.isPlaying; }
    public void SetFireSoundLoop()
    {
        m_FireSoundPlayer.loop = true;
    }

    public void StopFireSound()
    {
        if (m_FireSoundPlayer.isPlaying)
            m_FireSoundPlayer.Stop();

        m_FireSoundPlayer.loop = false;
    }


}
