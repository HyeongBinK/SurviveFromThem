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
    [SerializeField] private SetMonsterData m_AutoState; //레밸에 따라 자동으로 바뀌는 스테이터스
    public int GetMaxHP { get { return (null != m_AutoState) ? m_AutoState.GetState.MaxHP : 0; } }
    [SerializeField] private int m_iLevel;
    [SerializeField] private MONSTER_TYPE m_MobType;
    public MONSTER_TYPE GetMonsterType { get { return m_MobType; } }

    public int m_iCurrentHP;
    [SerializeField] private MONSTERACTIONSTATE m_ActState;
    // [SerializeField] private LayerMask m_Target; // 추적 대상 레이어
    private Transform m_TargetObjectTR; //추적할 타겟
    [SerializeField] private float m_fChaseDistance; //추적시작 거리
    [SerializeField] private float m_fAttackDistance; // 플레이어에게 해당거리만큼 접근시 공격계시
    [SerializeField] private MobAttackRange m_AttackRange; //공격범위
    private Animator m_MobAnimator; // 애니메이터 컴포넌트
   // private Renderer MobRenderer; // 렌더러 컴포넌트
    private NavMeshAgent m_PathFinder; // 경로계산 AI 에이전트
    private bool m_bInvincibilityFlag; // true 이면 무적
    private float m_flastAttackTime; // 마지막 공격 시점(공격애니메이션이 끝난시점부터 카운팅)
    [SerializeField] private float m_fDistance; //플레이어와의 거리
    [SerializeField] private Transform m_DamageTextPosition; //데미지텍스트가 발생할 위치
    private bool m_bIsDead; // 죽으면 false 살아있을땐 true 로하여 사망애니메이션 재생중 update에서 죽음상태 반복호출막는용도
    public bool m_bIsAttack { get { return (m_fAttackDistance >= m_fDistance); } } //플레이어에게 공격할수 있는 거리인가?
    public bool m_bIsChase { get { return (m_fChaseDistance >= m_fDistance); } } //플레이어에게 공격할수 있는 거리인가?
    private bool m_bIsAttackPlayer; //플레이어를 공격했는가?(한 애니메이션에 한번만 공격할수있게끔 제어하기위한 불린변수)
    private bool m_bIsProvoke; //도발을 당했는가?(도발 함수가 한번만 동작하게 하기위해 만든 불린 변수)

    private float m_fPatrolTime = 0; //정찰주기
    private float m_fPatrolTimer = 0; //정찰주기 메모타이머
    private float m_fAttackTimer = 0; //공격주기 메모타이머

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
        if (m_ActState == MONSTERACTIONSTATE.DEAD) //죽었는데 해당함수 호출시 종료
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

        if (m_iCurrentHP <= 0) //체력이 0 이하로 떨어지면 사망
        {
            m_ActState = MONSTERACTIONSTATE.DEAD;
            return;
        }
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX(m_AutoState.GetState.GetDamageEffectSoundName);
     
        if (m_ActState != MONSTERACTIONSTATE.ATTACK) //정찰중에 공격을 맞으면 추격
            m_ActState = MONSTERACTIONSTATE.CHASE;
    }
    private Vector3 GetDamagePos()
    {
        GameManager.Instance.AddNewLog(m_DamageTextPosition.position.ToString());
        return m_DamageTextPosition.position;
    }
    private Vector3 GetRandomPointOnNavMesh(Vector3 center, float distance) //Idle 상태일시 주변배회를 위해 자신주변 원형으로 특정좌표를 return 해주는 함수
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
        //콜리더박스 끄는 처리
    }
    IEnumerator ActiveInvincibility()
    {
        m_bInvincibilityFlag = true;
        yield return new WaitForSeconds(0.1f);
        m_bInvincibilityFlag = false;
    }
   IEnumerator ActiveAttackRange(float DelayTime)
    {
        m_AttackRange.Atk = m_AutoState.GetState.AttackPoint; //버프등에 의에 변경될수 있으므로 공격할떄 공격력을 할당
        m_bIsAttackPlayer = true;
         yield return new WaitForSeconds(DelayTime);
        m_AttackRange.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);
        m_AttackRange.gameObject.SetActive(false);
    }
    private void DropItem() //아이템드롭함수
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
    public void ProvocationByDoll(TimeWithTransform TWT) //도발상태이상에 걸림
    {
        if (!m_bIsProvoke)
        {
            if (m_ActState == MONSTERACTIONSTATE.PEACE)
                m_ActState = MONSTERACTIONSTATE.CHASE;
            StartCoroutine(Provoke(TWT.Time, TWT.Tr));
            m_bIsProvoke = true;
        }
    }
    IEnumerator Provoke(float Time, Transform Tr) //도발상태이상에 걸려 일정 시간동안 공격대상 변경
    {
        m_TargetObjectTR = Tr;
        yield return new WaitForSeconds(Time);
        m_TargetObjectTR = GameManager.Instance.GetPlayerTransform;
    }
}
