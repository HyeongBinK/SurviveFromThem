using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;


public enum MONSTERACTIONSTATE
{
    PEACE = 0,
    CHASE,
    ATTACK,
    DEAD
}

public class NormalMonster : MonoBehaviour
{
    [SerializeField] private SetMonsterData m_AutoState; //���뿡 ���� �ڵ����� �ٲ�� �������ͽ�
    public int GetMaxHP { get { return (null != m_AutoState) ? m_AutoState.GetState.MaxHP : 0; } }
    [SerializeField] private int m_iLevel;
    [SerializeField] private MONSTER_TYPE m_MobType;
    public MONSTER_TYPE GetMonsterType { get { return m_MobType; } }

    public int m_iCurrentHP;
    [SerializeField] private MONSTERACTIONSTATE m_ActState;
    // [SerializeField] private LayerMask m_Target; // ���� ��� ���̾�
    private Transform m_TargetObjectTR; //������ Ÿ��
    [SerializeField] private float m_fChaseDistance; //�������� �Ÿ�
    [SerializeField] private float m_fAttackDistance; // �÷��̾�� �ش�Ÿ���ŭ ���ٽ� ���ݰ��
    [SerializeField] private MobAttackRange m_AttackRange; //���ݹ���
    private Animator m_MobAnimator; // �ִϸ����� ������Ʈ
   // private Renderer MobRenderer; // ������ ������Ʈ
    private NavMeshAgent m_PathFinder; // ��ΰ�� AI ������Ʈ
    private bool m_bInvincibilityFlag; // true �̸� ����
    private float m_flastAttackTime; // ������ ���� ����(���ݾִϸ��̼��� ������������ ī����)
    [SerializeField] private float m_fDistance; //�÷��̾���� �Ÿ�
    [SerializeField] private Transform m_DamageTextPosition; //�������ؽ�Ʈ�� �߻��� ��ġ
    private bool m_bIsDead; // ������ false ��������� true ���Ͽ� ����ִϸ��̼� ����� update���� �������� �ݺ�ȣ�⸷�¿뵵
    public bool m_bIsAttack { get { return (m_fAttackDistance >= m_fDistance); } } //�÷��̾�� �����Ҽ� �ִ� �Ÿ��ΰ�?
    public bool m_bIsChase { get { return (m_fChaseDistance >= m_fDistance); } } //�÷��̾�� �����Ҽ� �ִ� �Ÿ��ΰ�?
    private bool m_bIsAttackPlayer; //�÷��̾ �����ߴ°�?(�� �ִϸ��̼ǿ� �ѹ��� �����Ҽ��ְԲ� �����ϱ����� �Ҹ�����)
    private bool m_bIsProvoke; //������ ���ߴ°�?(���� �Լ��� �ѹ��� �����ϰ� �ϱ����� ���� �Ҹ� ����)

    private float m_fPatrolTime = 0; //�����ֱ�
    private float m_fPatrolTimer = 0; //�����ֱ� �޸�Ÿ�̸�
    private float m_fAttackTimer = 0; //�����ֱ� �޸�Ÿ�̸�

    public event Action OnDeath;
    public void Init(int Level, Transform tr)
    {
        this.m_iLevel = Level;
        transform.position = tr.position;
        transform.rotation = tr.rotation;
        ResetState();
    }

    private void Awake()
    {
        m_PathFinder = GetComponent<NavMeshAgent>();
        m_MobAnimator = GetComponent<Animator>();
    }

    public void ResetState()
    {
        gameObject.tag = "Enemy";
        m_AutoState = new SetMonsterData(m_iLevel, m_MobType);
        m_iCurrentHP = m_AutoState.GetState.MaxHP;
        m_ActState = MONSTERACTIONSTATE.PEACE;
        m_bIsDead = false;
        float patroltime = m_AutoState.GetState.PatrolTime;
        m_fPatrolTime = UnityEngine.Random.Range(patroltime - 1.0f, patroltime + 1.0f);
        m_PathFinder.speed = m_AutoState.GetState.Speed;
        m_TargetObjectTR = GameManager.Instance.GetPlayerTransform;
        m_bInvincibilityFlag = false;
        m_bIsAttackPlayer = false;
        m_bIsProvoke = false;
    }
    public void ClearDeadEvent()
    {
        OnDeath = null;
    }

    private void OnEnable()
    {
        StartCoroutine(Act());
    } 

    public void GetDamage(DamageWithIsCritical DWC)
    {
        if (m_ActState == MONSTERACTIONSTATE.DEAD) //�׾��µ� �ش��Լ� ȣ��� ����
            return;
        if (DWC.Damage <= 0) return;
    
        if (UIManager.Instance)
        {
            Vector2 DamageTextPosition;
           /* if (GetDamagePos() != m_DamageTextPosition.position)
            {
                DamageTextPosition = Camera.main.WorldToScreenPoint(GetDamagePos());
                UIManager.Instance.CreateDamageUI(DWC.Damage, DWC.IsCritical, DamageTextPosition);
            }
            else*/
            {
                DamageTextPosition = Camera.main.WorldToScreenPoint(m_DamageTextPosition.position);
                UIManager.Instance.CreateDamageUI(DWC.Damage, DWC.IsCritical, DamageTextPosition);
            }
        }
        //FloatingDamageManager_TMP.Instance.CreateDamageUI(DWC.Damage, DWC.IsCritical, m_DamageTextPosition);
        m_iCurrentHP = Mathf.Clamp(m_iCurrentHP - DWC.Damage, 0, GetMaxHP);

        if (m_iCurrentHP <= 0) //ü���� 0 ���Ϸ� �������� ���
        {
            m_ActState = MONSTERACTIONSTATE.DEAD;
            return;
        }
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX(m_AutoState.GetState.GetDamageEffectSoundName);
     
        if (m_ActState != MONSTERACTIONSTATE.ATTACK) //�����߿� ������ ������ �߰�
            m_ActState = MONSTERACTIONSTATE.CHASE;
    }
    private Vector3 GetDamagePos()
    {
        GameManager.Instance.AddNewLog(m_DamageTextPosition.position.ToString());
        return m_DamageTextPosition.position;
    }
    private Vector3 GetRandomPointOnNavMesh(Vector3 center, float distance) //Idle �����Ͻ� �ֺ���ȸ�� ���� �ڽ��ֺ� �������� Ư����ǥ�� return ���ִ� �Լ�
    {
        Vector3 randomPos = UnityEngine.Random.insideUnitSphere * distance + center;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, distance, NavMesh.AllAreas);
        return hit.position;
    }

    private IEnumerator Act()
    {
        if (!m_TargetObjectTR) yield break;

        while (!m_bIsDead)
        {
            m_fDistance = Vector3.Distance(m_TargetObjectTR.position, transform.position);
            
            switch (m_ActState)
            {
                case MONSTERACTIONSTATE.PEACE:
                    {
                        m_MobAnimator.SetFloat("Movement", m_PathFinder.velocity.magnitude);
                        if (m_fPatrolTime <= (m_fPatrolTimer += Time.deltaTime))
                        {
                            m_fPatrolTimer = 0;
                            m_PathFinder.SetDestination(GetRandomPointOnNavMesh(gameObject.transform.position, 10));
                        }

                        if (m_bIsChase)
                            m_ActState = MONSTERACTIONSTATE.CHASE;
                    }
                    break;
                case MONSTERACTIONSTATE.CHASE:
                    {
                        m_MobAnimator.SetFloat("Movement", m_PathFinder.velocity.magnitude);
                        m_PathFinder.SetDestination(m_TargetObjectTR.position);
                       
                        if(m_bIsAttack)
                        {
                            m_PathFinder.isStopped = true;
                            m_MobAnimator.SetTrigger("Attack");
                            m_ActState = MONSTERACTIONSTATE.ATTACK;
                        }

                        if (!m_bIsChase)
                            m_ActState = MONSTERACTIONSTATE.PEACE;
                        
                        //OnTriggerStay(TargetCollider);
                    }
                    break;
                case MONSTERACTIONSTATE.ATTACK:
                    {
                        if(!m_bIsAttackPlayer)
                        StartCoroutine(ActiveAttackRange(0.1f));

                        if (m_AutoState.GetState.Attack1AnimTime <= (m_fAttackTimer += Time.deltaTime))
                        {
                            m_fAttackTimer = 0;
                            m_PathFinder.isStopped = false;
                            m_bIsAttackPlayer = false;
                            m_ActState = MONSTERACTIONSTATE.CHASE;
                        }
                    }
                    break;
                case MONSTERACTIONSTATE.DEAD:
                    {
                        m_PathFinder.isStopped = true;
                        //m_PathFinder.enabled = false;
                        m_MobAnimator.SetTrigger("Dead");
                        if(GameManager.Instance)
                        GameManager.Instance.GiveExpToPlayer(m_AutoState.GetState.EXP);
                        if(SoundManager.Instance)
                        SoundManager.Instance.PlaySFX(m_AutoState.GetState.DeadEffectSoundName);
                        m_bIsDead = true;
                        StartCoroutine(DeadEvent());
                        yield break;
                    }
            }
            yield return null;
        }
    }
    IEnumerator DeadEvent()
    {
        gameObject.tag = "Death";
        yield return new WaitForSeconds(m_AutoState.GetState.DeadAnimTime);
        DropItem();

        if (OnDeath != null)
            OnDeath();
        
        gameObject.SetActive(false);
        //�ݸ����ڽ� ���� ó��
    }
    IEnumerator ActiveInvincibility()
    {
        m_bInvincibilityFlag = true;
        yield return new WaitForSeconds(0.1f);
        m_bInvincibilityFlag = false;
    }
   IEnumerator ActiveAttackRange(float DelayTime)
    {
        m_AttackRange.Atk = m_AutoState.GetState.AttackPoint; //����� �ǿ� ����ɼ� �����Ƿ� �����ҋ� ���ݷ��� �Ҵ�
        m_bIsAttackPlayer = true;
         yield return new WaitForSeconds(DelayTime);
        m_AttackRange.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);
        m_AttackRange.gameObject.SetActive(false);
    }
    private void DropItem() //�����۵���Լ�
    {
        if (ItemManager.Instance)
        {
            var Postion = gameObject.transform;
            ItemManager.Instance.CreateMoney(m_AutoState.GetState.Gold, Postion);
            int DropResult = UnityEngine.Random.Range(0, 100);
            if(DropResult <= m_AutoState.GetState.DropRate)
            ItemManager.Instance.CreateItemObject(m_AutoState.GetState.DropItemUniqueNumber, 1, Postion);
        }
    }
    public void ProvocationByDoll(TimeWithTransform TWT) //���߻����̻� �ɸ�
    {
        if (!m_bIsProvoke)
        {
            if (m_ActState == MONSTERACTIONSTATE.PEACE)
                m_ActState = MONSTERACTIONSTATE.CHASE;
            StartCoroutine(Provoke(TWT.Time, TWT.Tr));
            m_bIsProvoke = true;
        }
    }
    IEnumerator Provoke(float Time, Transform Tr) //���߻����̻� �ɷ� ���� �ð����� ���ݴ�� ����
    {
        m_TargetObjectTR = Tr;
        yield return new WaitForSeconds(Time);
        m_TargetObjectTR = GameManager.Instance.GetPlayerTransform;
    }
}
