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

    [SerializeField] private AudioSource m_BGMPlayer; //BGM ���(���Ͱ� �Ҹ��� �� ����)
    [SerializeField] private AudioSource m_EffectSoundPlayer; //ȿ�������(������ �����Ҹ��� ��ųȿ����)(���Ͱ� �Ҹ��� �� ����)
    [SerializeField] private AudioSource m_FootStepSoundPlayer; // �߰��� �Ҹ� ����÷��̾�(���Ͱ� �Ҹ��� ����)
    [SerializeField] private AudioSource m_FireSoundPlayer; //�÷��̾ ���� ��¼Ҹ� ��� �÷��̾�(���Ͱ� �Ҹ��� ����)


    [SerializeField] private AudioClip[] m_BGMAudioClips; //BGM ����Ʈ
    [SerializeField] private AudioClip[] m_SFXAudioClips;  //ȿ���� ����Ʈ
    [SerializeField] private AudioClip[] m_FireAudioClips; //�ѱ� ȿ���� ����Ʈ

    private void Start()
    {
        // audioClips = Resources.LoadAll<AudioClip>("Sound" + ""); // ��δ� �ε��ؿ��� �ٿ���� ������ �ʹ�����
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
