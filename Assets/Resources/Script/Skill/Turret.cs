using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    private int Atk; //공격력
    private float LifeTime; //소환유지시간

    [SerializeField] private Transform m_RaycastStartSpot1; //일직선으로 날라가는 총알 발사 위치1
    [SerializeField] private Transform m_RaycastStartSpot2; //일직선으로 날라가는 총알 발사 위치2
    [SerializeField] private Transform m_MuzzleEffectsPosition1; // 사격효과 발생위치1 
    [SerializeField] private Transform m_MuzzleEffectsPosition2; // 사격효과 발생위치2 
    [SerializeField] private WEAPONEFFECT MuzzleEffect; //발생시킬 사격효과 
    [SerializeField] private GameObject TurretGun; //회전시킬 부분

    private AudioSource m_AudioSource;
    private float m_fDetectionRange = 20.0f; //적탐지범위
    private GameObject[] m_EnemyList; //탐색된 적리스트
    private float m_fSeekRate = 3.0f; //적 탐색 속도
    private bool m_bIsFire = true; 
    private float m_fTargetListUpdateTimer = 0.0f; //타겟리스트 갱신 타이머
    private float m_fLifeTimer = 0.0f; //소환시간 타이머
    private float m_fFireTimer = 0.0f; //공격주기 타이머
    private float FireCoolTimer = 0.0f; //지정시간동안 발사후 일정시간 휴식
    private float FireBreakTimer = 0.0f; //휴식에서 다시 공격으로 돌아갈때 까지의 타이머
    //공격효과 발생여부
    private bool m_bmakeHitEffects = true; //피격효과 발생여부
    private bool m_bMakeMuzzleEffects = true; // 총구 효과 발생여부

    private readonly float AttackRange = 9999.0f; //사격시 총이 날아 갈 수 있는 최대범위
    private readonly float DetectTerm = 0.5f; //공격대상리스트 전체 갱신 주기
    private readonly float FireRate = 0.1f; //공격주기
    private readonly float FireTime = 3.218f; //공격시간
    private readonly float FireTerm = 0.7f; //공격후 쉬는 시간
    private readonly string TurretBreakSound = "TurretBreak"; //터렛의 지속시간이 끝날때 재생할 사운드
    private readonly string DetectTag = "Enemy"; //공격할 대상의 태그
    private void Awake()
    {
        m_AudioSource = gameObject.GetComponent<AudioSource>();
    }
    private void OnEnable()
    {
        UpdateEnemyList();
        StartCoroutine(Act());
        StartCoroutine(SuumonTimeOver());
    }
    public void Init(int NewAtk, float NewLifeTime, Transform Tr)
    {
        Atk = NewAtk;
        LifeTime = NewLifeTime;
        m_bIsFire = true;
        m_fLifeTimer = 0.0f;
        m_fTargetListUpdateTimer = 0.0f;
        m_fFireTimer = 0.0f;
        FireCoolTimer = 0.0f;
        FireBreakTimer = 0.0f;
        gameObject.transform.position = Tr.position;
        gameObject.transform.rotation = Tr.rotation;
        if (!gameObject.activeSelf)
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
            m_fTargetListUpdateTimer += Time.deltaTime; //적리스트갱신타이머
            FireCoolTimer += Time.deltaTime;

            if (m_fTargetListUpdateTimer >= DetectTerm)
                UpdateEnemyList();

            if (FireCoolTimer >= FireTime)
            {
                FireBreakTimer += Time.deltaTime;
                m_bIsFire = false;
              
            }
            if(FireBreakTimer >= FireTerm)
            {
                m_bIsFire = true;
                FireBreakTimer = 0;
                FireCoolTimer = 0;
            }

            if (m_bIsFire)
            {
                m_fFireTimer += Time.deltaTime;
                if (m_EnemyList != null)
                {
                    if (!m_AudioSource.isPlaying)
                        m_AudioSource.Play();
          
                    float greatestDotSoFar = -1.0f;
                    float Distance;
                    Vector3 target = transform.forward * 1000;
                    foreach (GameObject enemy in m_EnemyList)
                    {
                        if (enemy != null)
                        {
                            Distance = Vector3.Distance(enemy.gameObject.transform.position, transform.position);
                            if (Distance > m_fDetectionRange) continue;
                           
                            Vector3 direction = enemy.transform.position - transform.position;
                            float dot = Vector3.Dot(direction.normalized, transform.forward);
                            if (dot > greatestDotSoFar)
                            {
                                target = enemy.transform.position + new Vector3(0, 1, 0);
                                greatestDotSoFar = dot;
                            }
                        }
                    }
                    Quaternion targetRotation = Quaternion.LookRotation(target - transform.position);
                    TurretGun.gameObject.transform.rotation = Quaternion.Lerp(TurretGun.gameObject.transform.rotation, targetRotation, Time.deltaTime * m_fSeekRate);
                    if (m_fFireTimer >= FireRate)
                        Fire();
                }
                else
                {
                    if (m_AudioSource.isPlaying)
                        m_AudioSource.Stop();
                }
            }
            yield return null;
        }
    }
    IEnumerator SuumonTimeOver()
    {
        yield return new WaitForSeconds(LifeTime);
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX(TurretBreakSound);
        gameObject.SetActive(false);
    }

    private void Fire() //발사
    {
        if (!WeaponEffectPoolingManager.Instance) return;

        Vector3 direction = m_RaycastStartSpot1.forward;
        Vector3 direction2 = m_RaycastStartSpot2.forward;

        Ray ray = new Ray(m_RaycastStartSpot1.position, direction);
        Ray ray2 = new Ray(m_RaycastStartSpot2.position, direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, AttackRange))
        {
            if(hit.collider.tag == DetectTag)
            hit.collider.gameObject.SendMessageUpwards("GetDamage", ResultDamage(), SendMessageOptions.DontRequireReceiver);
            if (m_bmakeHitEffects)
                WeaponEffectPoolingManager.Instance.CreateEffectPrefab(WEAPONEFFECT.HITEFFECT, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));

        }
        if (Physics.Raycast(ray2, out hit, AttackRange))
        {
            if (hit.collider.tag == DetectTag)
                hit.collider.gameObject.SendMessageUpwards("GetDamage", ResultDamage(), SendMessageOptions.DontRequireReceiver);
            if (m_bmakeHitEffects)
                WeaponEffectPoolingManager.Instance.CreateEffectPrefab(WEAPONEFFECT.HITEFFECT, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));

        }
        // Muzzle flash effects
        if (m_bMakeMuzzleEffects)
        {
            WeaponEffectPoolingManager.Instance.CreateEffectPrefab(MuzzleEffect, m_MuzzleEffectsPosition1.position, m_MuzzleEffectsPosition1.rotation);
            WeaponEffectPoolingManager.Instance.CreateEffectPrefab(MuzzleEffect, m_MuzzleEffectsPosition2.position, m_MuzzleEffectsPosition2.rotation);
        }
        m_fFireTimer = 0;
    }


    private DamageWithIsCritical ResultDamage()
    {
        if (GameManager.Instance)
            return GameManager.Instance.MakePlayerDamage(Atk);

        return new DamageWithIsCritical();
    }

    private void UpdateEnemyList()
    {
        m_EnemyList = GameObject.FindGameObjectsWithTag(DetectTag);
        m_fTargetListUpdateTimer = 0.0f;
    }
}
