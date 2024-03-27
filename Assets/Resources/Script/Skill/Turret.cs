using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    private int Atk; //���ݷ�
    private float LifeTime; //��ȯ�����ð�

    [SerializeField] private Transform m_RaycastStartSpot1; //���������� ���󰡴� �Ѿ� �߻� ��ġ1
    [SerializeField] private Transform m_RaycastStartSpot2; //���������� ���󰡴� �Ѿ� �߻� ��ġ2
    [SerializeField] private Transform m_MuzzleEffectsPosition1; // ���ȿ�� �߻���ġ1 
    [SerializeField] private Transform m_MuzzleEffectsPosition2; // ���ȿ�� �߻���ġ2 
    [SerializeField] private WEAPONEFFECT MuzzleEffect; //�߻���ų ���ȿ�� 
    [SerializeField] private GameObject TurretGun; //ȸ����ų �κ�

    private AudioSource m_AudioSource;
    private float m_fDetectionRange = 20.0f; //��Ž������
    private GameObject[] m_EnemyList; //Ž���� ������Ʈ
    private float m_fSeekRate = 3.0f; //�� Ž�� �ӵ�
    private bool m_bIsFire = true; 
    private float m_fTargetListUpdateTimer = 0.0f; //Ÿ�ٸ���Ʈ ���� Ÿ�̸�
    private float m_fLifeTimer = 0.0f; //��ȯ�ð� Ÿ�̸�
    private float m_fFireTimer = 0.0f; //�����ֱ� Ÿ�̸�
    private float FireCoolTimer = 0.0f; //�����ð����� �߻��� �����ð� �޽�
    private float FireBreakTimer = 0.0f; //�޽Ŀ��� �ٽ� �������� ���ư��� ������ Ÿ�̸�
    //����ȿ�� �߻�����
    private bool m_bmakeHitEffects = true; //�ǰ�ȿ�� �߻�����
    private bool m_bMakeMuzzleEffects = true; // �ѱ� ȿ�� �߻�����

    private readonly float AttackRange = 9999.0f; //��ݽ� ���� ���� �� �� �ִ� �ִ����
    private readonly float DetectTerm = 0.5f; //���ݴ�󸮽�Ʈ ��ü ���� �ֱ�
    private readonly float FireRate = 0.1f; //�����ֱ�
    private readonly float FireTime = 3.218f; //���ݽð�
    private readonly float FireTerm = 0.7f; //������ ���� �ð�
    private readonly string TurretBreakSound = "TurretBreak"; //�ͷ��� ���ӽð��� ������ ����� ����
    private readonly string DetectTag = "Enemy"; //������ ����� �±�
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
            m_fTargetListUpdateTimer += Time.deltaTime; //������Ʈ����Ÿ�̸�
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

    private void Fire() //�߻�
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
