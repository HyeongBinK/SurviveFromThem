using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ProvokeDoll : MonoBehaviour
{
    private AudioSource m_AudioSource;
    [SerializeField] private AudioClip HitSound;
    private GameObject[] m_EnemyList; //탐색된 적리스트
    private float m_fProvokeListUpdateTimer = 0.0f; //도발리스트 갱신 타이머
    private float m_fActiveTime = 0.0f; //현재 활성화 되어있는 시간 타이머

    private int MaxHP;
    private int CurHP;
    private float SummonTime;

    private readonly float ProvokeRange = 20.0f; //적도발범위
    private readonly float DetectTerm = 1.0f; //도발대상리스트 전체 갱신 주기
    private readonly string ProvokeTag = "Enemy"; //공격할 대상의 태그
    private readonly string DeadSound = "ProvokeDollDie";
    private void Awake()
    {
        m_AudioSource = gameObject.GetComponent<AudioSource>();
    }

    public void OnEnable()
    {
        StartCoroutine(Act());
    }
    public void Init(int NewHP, float NewLifeTime, Transform Tr)
    {
        m_fProvokeListUpdateTimer = 0.0f;
        m_fActiveTime = 0.0f;
        MaxHP = NewHP;
        CurHP = MaxHP;
        SummonTime = NewLifeTime;
        gameObject.transform.position = Tr.position;
        gameObject.transform.rotation = Tr.rotation;
        if(!gameObject.activeSelf)
            gameObject.SetActive(true);
    }
    public void SetTransform(Transform Tr)
    {
        gameObject.transform.position = Tr.position;
        gameObject.transform.rotation = Tr.rotation;
    }
    IEnumerator Act()
    {
        while (gameObject.activeSelf)
        {
            m_fProvokeListUpdateTimer += Time.deltaTime; 
            m_fActiveTime += Time.deltaTime;

            if(m_fActiveTime >= SummonTime)
                DeadEvent();
            if (m_fProvokeListUpdateTimer >= DetectTerm)
                UpdateEnemyList();

            if (m_EnemyList != null)
            {
                float Distance;
                Vector3 target = transform.forward * 1000;
                foreach (GameObject enemy in m_EnemyList)
                {
                    if (enemy != null)
                    {
                        Distance = Vector3.Distance(enemy.gameObject.transform.position, transform.position);
                        if (Distance > ProvokeRange) continue;

                        enemy.gameObject.SendMessage("ProvocationByDoll", MakeTWT(), SendMessageOptions.DontRequireReceiver);
                    }
                }
             
            }
            yield return null;
        }
       
    }
    public void GetDamage(int NewDamage)
    {
        if (CurHP <= 0) return;

        CurHP = Mathf.Clamp(CurHP - NewDamage, 0, MaxHP);

        if (CurHP <= 0)
        {
            DeadEvent();
            return;
        }
        m_AudioSource.PlayOneShot(HitSound);
    }

    private void DeadEvent()
    {
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX(DeadSound);
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
    private void UpdateEnemyList()
    {
        m_EnemyList = GameObject.FindGameObjectsWithTag(ProvokeTag);
        m_fProvokeListUpdateTimer = 0.0f;
    }

    private TimeWithTransform MakeTWT()
    {
        TimeWithTransform TWT = new TimeWithTransform();
        TWT.Tr = gameObject.transform;
        TWT.Time = SummonTime - m_fActiveTime;
        return TWT;
    }
}
