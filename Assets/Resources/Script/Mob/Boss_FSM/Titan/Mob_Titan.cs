using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public enum TITAN_ACT
{
    START,
    PEACE = 0,
    CHASE,
    STEP1,
    STEP2,
    DEAD,
    END
}
public enum BOSS_PHASE
{
    START,
    PHASE1 = 0,
    PHASE2,
    PHASE3,
    END
}

public class Mob_Titan : MonoBehaviour
{
    [SerializeField] private int m_iLevel; //몬스터의 레벨
    [SerializeField] private MONSTER_TYPE m_MobType; //몬스터의 종류

    [SerializeField] private SetMonsterData m_AutoState; //레밸에 따라 자동으로 바뀌는 스테이터스
    [SerializeField] private MobAttackRange m_AttackRange; //공격범위
    [SerializeField] private Transform m_DamageTextPosition; //데미지텍스트가 발생할 위치
    public int GetMaxHP { get { return (null != m_AutoState) ? m_AutoState.GetState.MaxHP : 0; } } //외부에서 몬스터 체력바에 최대체력수치를 가져오기위해 생성

    public Head_Machine<Mob_Titan> m_StateMachine; //유한상태기계머신
    private FSM<Mob_Titan>[] m_arrState = new FSM<Mob_Titan>[(int)TITAN_ACT.END];
    public TITAN_ACT m_ePrevState; //이전상태
    public TITAN_ACT m_eCurState; //현재상태
    private BOSS_PHASE m_Phase; //보스의 단계
    private bool m_bIsDead; //죽으면 true
    private bool m_bInvincibilityFlag; // true 이면 무적
    private bool m_bIsSleep; //수면상태인가?(수면 == 정찰하지않고 그자리에서 대기하는 상태)
    private Transform m_TargetObject; //추적할 타겟
    private Animator m_Animator; //애니메이터
    private NavMeshAgent m_PathFinder; // 경로계산 AI 에이전트
    [SerializeField] private Renderer m_Renderer; //능력치강화시 매쉬의 색깔변경
    [SerializeField] private Material m_OriginMat; //기본 메터리얼
    [SerializeField] private Material m_BerserkMat; //화난상태의 메터리얼         
    public float m_fDistance { get; private set; } //플레이어와의 거리
    private readonly float SightRange = 20;  //플레이어가 시야에 들어오는 거리
    private readonly float AttackDistance = 3.0f; //공격사거리 
    public bool m_bIsChase { get { return (SightRange >= m_fDistance); } } //일정거리안에 플레이어가 다가오면 배틀시작
    public bool m_bIsCanAttack { get { return (AttackDistance >= m_fDistance); } } //일정거리안에 플레이어가 다가오면 배틀시작
    private bool m_bIsAttackPlayer; //플레이어를 공격했는가?(한 애니메이션에 한번만 트리거발생과 음향재생)
    private bool IsAttackCollider; //공격판정 콜리더가 활성화되었는가?
    private float AttackTermTimer; //공격과 공격사이의 타이머
    private bool m_bIsFirstAttack; // 첫번째 공격을 끝냇는가
    private bool m_bIsSecondAttack; // 두번째 공격을 끝냇는가
    private bool m_bIsProvoke; //도발을 당했는가
    private readonly float m_Attack1To2Term = 0.5f;
    private readonly float m_Attack2To3Term = 1.0f;
    private readonly float BLOCKTIME = 100; 

    public int m_iCurHP { get; private set; } //현재 남아있는 체력
    private float m_fPatrolRate; //정찰주기
    private float m_fPatrolTimer = 0; //정찰주기 메모타이머
    private float m_fAttackTimer = 0;
    public event Action OnDeath;
    public event Action<int> SetHPBar;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, SightRange);
    }

    public void Init()
    {
        gameObject.tag = "Enemy";
        m_AutoState = new SetMonsterData(m_iLevel, m_MobType);
        m_Animator = gameObject.GetComponent<Animator>();
        m_PathFinder = gameObject.GetComponent<NavMeshAgent>();
        m_iCurHP = m_AutoState.GetState.MaxHP;
        float patroltime = m_AutoState.GetState.PatrolTime;
        m_fPatrolRate = UnityEngine.Random.Range(patroltime - 1.0f, patroltime + 1.0f);
        m_PathFinder.speed = m_AutoState.GetState.Speed;
        m_Phase = BOSS_PHASE.PHASE1;
        m_TargetObject = GameManager.Instance.GetPlayerTransform;
        m_fAttackTimer = 0;
        AttackTermTimer = BLOCKTIME;
        m_bIsAttackPlayer = false;
        IsAttackCollider = false;
        m_bIsFirstAttack = false;
        m_bIsSecondAttack = false;
        m_bIsDead = false;
        m_bIsProvoke = false;
        m_Renderer.material = m_OriginMat;
        StartNav();
        m_StateMachine = new Head_Machine<Mob_Titan>();
        m_arrState[(int)TITAN_ACT.PEACE] = new Titan_Peace(this);
        m_arrState[(int)TITAN_ACT.CHASE] = new Titan_Chase(this);
        m_arrState[(int)TITAN_ACT.STEP1] = new Titan_Step1(this);
        m_arrState[(int)TITAN_ACT.STEP2] = new Titan_Step2(this);
        m_arrState[(int)TITAN_ACT.DEAD] = new Titan_Dead(this);
        m_StateMachine.SetState(m_arrState[(int)TITAN_ACT.PEACE], this);
    }

    public void ChangeFSM(TITAN_ACT ps)
    {
        for (int i = (int)TITAN_ACT.START; i < (int)TITAN_ACT.END; i++)
        {
            if (i == (int)ps)
                m_StateMachine.Change(m_arrState[(int)ps]);
        }
    }

    public void Begin()
    {
        m_StateMachine.Begin();
    }

    public void Run()
    {
        m_StateMachine.Run();
    }

    public void Exit()
    {
        m_StateMachine.Exit();
    }

    private void Awake()
    {
        Init();
    }
    private void Start()
    {
        Begin();
    }
    private void Update()
    {
        m_fDistance = Vector3.Distance(m_TargetObject.position, transform.position);
        Run();
    }
    public void GetDamage(DamageWithIsCritical DWC)
    {
        if (m_eCurState != TITAN_ACT.DEAD)
        {
            m_iCurHP -= DWC.Damage;

            m_iCurHP = Mathf.Clamp(m_iCurHP, 0, GetMaxHP);

            if (UIManager.Instance)
            {
                Vector2 DamageTextPosition = Camera.main.WorldToScreenPoint(m_DamageTextPosition.position);
                UIManager.Instance.CreateDamageUI(DWC.Damage, DWC.IsCritical, DamageTextPosition);
            }

            if (m_iCurHP <= 0)
            {
                m_bIsDead = true;
                ChangeFSM(TITAN_ACT.DEAD);
                return;
            }

            ChangePhase();

            if (DWC.Damage >= (int)(GetMaxHP * 0.1))
                m_Animator.SetTrigger("GetDamage");

            if(SoundManager.Instance)
                SoundManager.Instance.PlaySFX(m_AutoState.GetState.GetDamageEffectSoundName);
            //체력바 갱신이벤트
        }
    }

    public void OnInvincibility()
    {
        m_bInvincibilityFlag = true;
    }
    public void OffInvincibility()
    {
        m_bInvincibilityFlag = false;
    }
    public void GotoSleep()
    {
        m_bIsSleep = true;

    }
    public void WakeUp()
    {
        m_bIsSleep = false;
    }
    public void Patrol() //주변정찰
    {
        m_Animator.SetFloat("Movement", m_PathFinder.velocity.magnitude);
        if (m_fPatrolRate <= (m_fPatrolTimer += Time.deltaTime))
        {
            m_fPatrolTimer = 0;
            m_PathFinder.SetDestination(GetRandomPointOnNavMesh(gameObject.transform.position, 10));
        }
    }
    private Vector3 GetRandomPointOnNavMesh(Vector3 center, float distance) //Idle 상태일시 주변배회를 위해 자신주변 원형으로 특정좌표를 return 해주는 함수
    {
        Vector3 randomPos = UnityEngine.Random.insideUnitSphere * distance + center;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, distance, NavMesh.AllAreas);
        return hit.position;
    }
    public void StartDeadEvent()
    {
        StopAllCoroutines();
        StartCoroutine(DeadEvent());
    }
    IEnumerator DeadEvent() //사망이벤트
    {
        StopNav();
        gameObject.tag = "Death";
        m_Animator.SetTrigger("Dead");
        GameManager.Instance.GiveExpToPlayer(m_AutoState.GetState.EXP);

        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX(m_AutoState.GetState.DeadEffectSoundName);
        yield return new WaitForSeconds(m_AutoState.GetState.DeadAnimTime);
        
        DropItem();

        if (OnDeath != null)
            OnDeath();

        if (m_AttackRange.gameObject.activeSelf)
            m_AttackRange.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
    public void ClearDeadEvent()
    {
        OnDeath = null;
        SetHPBar = null;
    }
    public void Chase()
    {
        if (m_bIsDead) return;
        m_Animator.SetFloat("Movement", m_PathFinder.velocity.magnitude);
        m_PathFinder.SetDestination(m_TargetObject.position);
    }
    public void OnTrigger(string TriggerName)
    {
        m_Animator.SetTrigger(TriggerName);
    }
    private void ChangePhase()
    {
        if (m_bIsDead) return;
        if (m_Phase == BOSS_PHASE.PHASE1 && m_iCurHP <= (GetMaxHP * 0.5f))
        {
            m_Phase = BOSS_PHASE.PHASE2;
            StartCoroutine(Berserk());
        }
    }
    public void Disable() //게임 오브젝트 비활성화
    {
        gameObject.SetActive(false);
    }
    public void StopNav() //공격모션 중 플레이어 추격기능멈춤
    {
        m_PathFinder.isStopped = true;
    }
    public void StartNav() // 공격모션 종료후 다시 플레이어 추격
    {
        m_PathFinder.isStopped = false;
    }

    IEnumerator ActiveAttackRange(float DelayTime, float ColliderActiveTime)
    {
        m_AttackRange.Atk = m_AutoState.GetState.AttackPoint; //버프등에 의에 변경될수 있으므로 공격할떄 공격력을 할당
        m_bIsAttackPlayer = true;
        StopNav();
        yield return new WaitForSeconds(DelayTime); //데미지가 들어갈타이밍까지 대기
        IsAttackCollider = true;
        m_AttackRange.gameObject.SetActive(true);

        yield return new WaitForSeconds(ColliderActiveTime); //콜리더를 잠시 활성화
        m_AttackRange.gameObject.SetActive(false);
        IsAttackCollider = false;
        AttackTermTimer = m_fAttackTimer;
    }

    public void Attack1()
    {
        if (m_bIsDead) return;
        if (!m_bIsAttackPlayer)
        {
            m_Animator.SetTrigger("Attack1");
            if (SoundManager.Instance)
                SoundManager.Instance.PlaySFX(m_AutoState.GetState.Attack1EffectSoundName);
            StartCoroutine(ActiveAttackRange(m_AutoState.GetState.Attack1AnimTime * 0.2f, 0.05f));
        }

        if (m_AutoState.GetState.Attack1AnimTime <= (m_fAttackTimer += Time.deltaTime))
            AttackEnd();
    }
    public void Attack2()
    {
        if (m_bIsDead) return;
        if (!m_bIsAttackPlayer)
        {
            m_Animator.SetTrigger("Attack2");
            if (SoundManager.Instance)
                SoundManager.Instance.PlaySFX(m_AutoState.GetState.Attack2EffectSoundName);
            StartCoroutine(ActiveAttackRange(m_AutoState.GetState.Attack2AnimTime * 0.2f, 0.1f));
            m_bIsFirstAttack = true;
        }
        if(m_fAttackTimer >= AttackTermTimer + m_Attack1To2Term && !IsAttackCollider && m_bIsFirstAttack)
        {
            StartCoroutine(ActiveAttackRange(0, 0.1f));
            m_bIsFirstAttack = false;
        }

        if (m_AutoState.GetState.Attack2AnimTime <= (m_fAttackTimer += Time.deltaTime))
            AttackEnd();
    }
    public void Attack3()
    {
        if (m_bIsDead) return;
        if (!m_bIsAttackPlayer)
        {
            m_Animator.SetTrigger("Attack3");
            if (SoundManager.Instance)
                SoundManager.Instance.PlaySFX(m_AutoState.GetState.Attack1EffectSoundName);
            StartCoroutine(ActiveAttackRange(m_AutoState.GetState.Attack3AnimTime * 0.15f, 0.3f));
            m_bIsFirstAttack = true;
        }
        if (m_fAttackTimer >= AttackTermTimer + m_Attack1To2Term && !IsAttackCollider && m_bIsFirstAttack)
        {
            StartCoroutine(ActiveAttackRange(0, 0.1f));
            m_bIsFirstAttack = false;
            m_bIsSecondAttack = true;
            if (SoundManager.Instance)
                SoundManager.Instance.PlaySFX(m_AutoState.GetState.Attack3EffectSoundName);
        }
        if (m_fAttackTimer >= AttackTermTimer + m_Attack2To3Term && !IsAttackCollider && m_bIsSecondAttack)
        {
            StartCoroutine(ActiveAttackRange(0, 0.1f));
            m_bIsSecondAttack = false;
        }

        if (m_AutoState.GetState.Attack3AnimTime <= (m_fAttackTimer += Time.deltaTime))
            AttackEnd();

    }
    private void AttackEnd()
    {
        m_fAttackTimer = 0;
        AttackTermTimer = BLOCKTIME;
        StartNav();
        m_bIsAttackPlayer = false;
        m_bIsFirstAttack = false;
        m_bIsSecondAttack = false;
        ChangeFSM(TITAN_ACT.CHASE);
    }
    public void ChangeToAttack()
    {
        if (m_bIsDead) return;
        if (m_bIsCanAttack)
        {
            if(m_Phase == BOSS_PHASE.PHASE1)
            {
                ChangeFSM(TITAN_ACT.STEP1);
            }
            else if(m_Phase == BOSS_PHASE.PHASE2)
            {
                ChangeFSM(TITAN_ACT.STEP2);
            }
        }
    }
    IEnumerator Berserk()
    {
        m_Animator.SetTrigger("Burf");
        StopNav();
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX(m_AutoState.GetState.BurfEffectSoundName);

        yield return new WaitForSeconds(m_AutoState.GetState.BurfAnimTime);
        m_Renderer.material = m_BerserkMat;
        m_AutoState.ATKBurf(1.5f);
        m_AutoState.SPDBurf(1.5f);
        StartNav();
    }
    private void DropItem() //아이템드롭함수
    {
        if (ItemManager.Instance)
        {
            var Postion = gameObject.transform;
            ItemManager.Instance.CreateMoney(m_AutoState.GetState.Gold, Postion);
            int DropResult = UnityEngine.Random.Range(0, 100);
            if (DropResult <= m_AutoState.GetState.DropRate)
                ItemManager.Instance.CreateItemObject(m_AutoState.GetState.DropItemUniqueNumber, 1, Postion);
        }
    }
    public void ProvocationByDoll(TimeWithTransform TWT)
    {
        if (!m_bIsProvoke)
        {
            if(m_eCurState == TITAN_ACT.PEACE)
            ChangeFSM(TITAN_ACT.CHASE);
            StartCoroutine(Provoke(TWT.Time, TWT.Tr));
            m_bIsProvoke = true;
        }
    }
    IEnumerator Provoke(float Time, Transform Tr)
    {
        m_TargetObject = Tr;
        yield return new WaitForSeconds(Time);
        m_TargetObject = GameManager.Instance.GetPlayerTransform;
    }
}
