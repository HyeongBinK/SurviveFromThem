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
    [SerializeField] private int m_iLevel; //������ ����
    [SerializeField] private MONSTER_TYPE m_MobType; //������ ����

    [SerializeField] private SetMonsterData m_AutoState; //���뿡 ���� �ڵ����� �ٲ�� �������ͽ�
    [SerializeField] private MobAttackRange m_AttackRange; //���ݹ���
    [SerializeField] private Transform m_DamageTextPosition; //�������ؽ�Ʈ�� �߻��� ��ġ
    public int GetMaxHP { get { return (null != m_AutoState) ? m_AutoState.GetState.MaxHP : 0; } } //�ܺο��� ���� ü�¹ٿ� �ִ�ü�¼�ġ�� ������������ ����

    public Head_Machine<Mob_Titan> m_StateMachine; //���ѻ��±��ӽ�
    private FSM<Mob_Titan>[] m_arrState = new FSM<Mob_Titan>[(int)TITAN_ACT.END];
    public TITAN_ACT m_ePrevState; //��������
    public TITAN_ACT m_eCurState; //�������
    private BOSS_PHASE m_Phase; //������ �ܰ�
    private bool m_bIsDead; //������ true
    private bool m_bInvincibilityFlag; // true �̸� ����
    private bool m_bIsSleep; //��������ΰ�?(���� == ���������ʰ� ���ڸ����� ����ϴ� ����)
    private Transform m_TargetObject; //������ Ÿ��
    private Animator m_Animator; //�ִϸ�����
    private NavMeshAgent m_PathFinder; // ��ΰ�� AI ������Ʈ
    [SerializeField] private Renderer m_Renderer; //�ɷ�ġ��ȭ�� �Ž��� ���򺯰�
    [SerializeField] private Material m_OriginMat; //�⺻ ���͸���
    [SerializeField] private Material m_BerserkMat; //ȭ�������� ���͸���         
    public float m_fDistance { get; private set; } //�÷��̾���� �Ÿ�
    private readonly float SightRange = 20;  //�÷��̾ �þ߿� ������ �Ÿ�
    private readonly float AttackDistance = 3.0f; //���ݻ�Ÿ� 
    public bool m_bIsChase { get { return (SightRange >= m_fDistance); } } //�����Ÿ��ȿ� �÷��̾ �ٰ����� ��Ʋ����
    public bool m_bIsCanAttack { get { return (AttackDistance >= m_fDistance); } } //�����Ÿ��ȿ� �÷��̾ �ٰ����� ��Ʋ����
    private bool m_bIsAttackPlayer; //�÷��̾ �����ߴ°�?(�� �ִϸ��̼ǿ� �ѹ��� Ʈ���Ź߻��� �������)
    private bool IsAttackCollider; //�������� �ݸ����� Ȱ��ȭ�Ǿ��°�?
    private float AttackTermTimer; //���ݰ� ���ݻ����� Ÿ�̸�
    private bool m_bIsFirstAttack; // ù��° ������ �����°�
    private bool m_bIsSecondAttack; // �ι�° ������ �����°�
    private bool m_bIsProvoke; //������ ���ߴ°�
    private readonly float m_Attack1To2Term = 0.5f;
    private readonly float m_Attack2To3Term = 1.0f;
    private readonly float BLOCKTIME = 100; 

    public int m_iCurHP { get; private set; } //���� �����ִ� ü��
    private float m_fPatrolRate; //�����ֱ�
    private float m_fPatrolTimer = 0; //�����ֱ� �޸�Ÿ�̸�
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
            //ü�¹� �����̺�Ʈ
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
    public void Patrol() //�ֺ�����
    {
        m_Animator.SetFloat("Movement", m_PathFinder.velocity.magnitude);
        if (m_fPatrolRate <= (m_fPatrolTimer += Time.deltaTime))
        {
            m_fPatrolTimer = 0;
            m_PathFinder.SetDestination(GetRandomPointOnNavMesh(gameObject.transform.position, 10));
        }
    }
    private Vector3 GetRandomPointOnNavMesh(Vector3 center, float distance) //Idle �����Ͻ� �ֺ���ȸ�� ���� �ڽ��ֺ� �������� Ư����ǥ�� return ���ִ� �Լ�
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
    IEnumerator DeadEvent() //����̺�Ʈ
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
    public void Disable() //���� ������Ʈ ��Ȱ��ȭ
    {
        gameObject.SetActive(false);
    }
    public void StopNav() //���ݸ�� �� �÷��̾� �߰ݱ�ɸ���
    {
        m_PathFinder.isStopped = true;
    }
    public void StartNav() // ���ݸ�� ������ �ٽ� �÷��̾� �߰�
    {
        m_PathFinder.isStopped = false;
    }

    IEnumerator ActiveAttackRange(float DelayTime, float ColliderActiveTime)
    {
        m_AttackRange.Atk = m_AutoState.GetState.AttackPoint; //����� �ǿ� ����ɼ� �����Ƿ� �����ҋ� ���ݷ��� �Ҵ�
        m_bIsAttackPlayer = true;
        StopNav();
        yield return new WaitForSeconds(DelayTime); //�������� ��Ÿ�ֱ̹��� ���
        IsAttackCollider = true;
        m_AttackRange.gameObject.SetActive(true);

        yield return new WaitForSeconds(ColliderActiveTime); //�ݸ����� ��� Ȱ��ȭ
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
    private void DropItem() //�����۵���Լ�
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
