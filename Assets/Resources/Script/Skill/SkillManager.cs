using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum SKILLTYPE
{
    NONE = 0,
    ACTIVE,
    PASSIVE,
    END
}
public class SkillManager : MonoBehaviour
{
    private static SkillManager m_instance; //싱글톤 할당

    public static SkillManager Instance
    {
        get
        {
            // 만약 싱글톤 변수에 아직 오브젝트가 할당되지 않았다면
            if (m_instance == null)
            {
                // 씬에서 GameManager 오브젝트를 찾아 할당
                m_instance = FindObjectOfType<SkillManager>();
            }
            // 싱글톤 오브젝트를 반환
            return m_instance;
        }
    }

    [SerializeField] private Turret m_MyTurret; //터렛소환 스킬 사용시 소환할 터렛 오브젝트
    [SerializeField] private ProvokeDoll m_MyProvokeDoll; //도발인형 스킬 사용시 소환할 도발인형 오브젝트
    private float m_fMakeTurretSkillCoolTimer = 0.0f; //터렛소환스킬 쿨타임 타이머
    private float m_fMakeProvokeDollSkillCoolTimer = 0.0f; //도발인형소환스킬 쿨타임 타이머
    private float m_fStimPackSkillCoolTimer = 0.0f; //스팀팩스킬 쿨타임 타이머
    private bool m_bIsMakeTurretSkillCoolTime; //터렛소환 스킬이 쿨타임인가?
    private bool m_bIsMakeProvokeDollSkillCoolTime; //도발인형 소환 스킬이 쿨타임인가?
    private bool m_bIsStimPackSkillCoolTime; //스팀팩 스킬이 쿨타임인가?
    private bool m_bCanUseSkill; //true : 스킬사용 가능 false : 스킬 사용불가능

    private void Awake()

    {  // 씬에 싱글톤 오브젝트가 된 다른 SkillManager 오브젝트가 있다면
        if (Instance != this)
        {
            // 자신을 파괴
            Destroy(gameObject);
        }
        m_fMakeTurretSkillCoolTimer = 0.0f;
        m_fMakeProvokeDollSkillCoolTimer = 0.0f;
        m_fStimPackSkillCoolTimer = 0.0f;
        m_bIsMakeTurretSkillCoolTime = false;
        m_bIsMakeProvokeDollSkillCoolTime = false;
        m_bIsStimPackSkillCoolTime = false;
        m_bCanUseSkill = true;
    }

    public void SetCanUseSkillState(bool NewBoolean)
    {
        m_bCanUseSkill = NewBoolean;
    }

    public float GetSkillCoolTimer(SKILLNAME SkillName)
    {
        switch (SkillName)
        {
            case SKILLNAME.MAKETURRET:
                return m_fMakeTurretSkillCoolTimer;
            case SKILLNAME.MAKEPROVOKEDOLL:
                return m_fMakeProvokeDollSkillCoolTimer;
            case SKILLNAME.USESTIMPACK:
                return m_fStimPackSkillCoolTimer;
            default:
                Debug.Log("스킬 쿨타임정보를 가져오는데에서 에러 발생");
                return 0;
        }
    }
    public bool GetIsSkillCoolTime(SKILLNAME SkillName)
    {
        switch (SkillName)
        {
            case SKILLNAME.MAKETURRET:
                return m_bIsMakeTurretSkillCoolTime;
            case SKILLNAME.MAKEPROVOKEDOLL:
                return m_bIsMakeProvokeDollSkillCoolTime;
            case SKILLNAME.USESTIMPACK:
                return m_bIsStimPackSkillCoolTime;
            default:
                Debug.Log("스킬 쿨타임정보를 가져오는데에서 에러 발생");
                return false;
        }
    }

    public void DisActiveSummonObject() //맵이동을 하게되어서 소환물들을 강제로 비활성화 해야할떄
    {
        if (m_MyTurret.gameObject.activeSelf)
            m_MyTurret.gameObject.SetActive(false);
        if (m_MyProvokeDoll.gameObject.activeSelf)
            m_MyProvokeDoll.gameObject.SetActive(false);
    }

    IEnumerator MakeTurretSkillCoolTimeFlow()
    {
        m_bIsMakeTurretSkillCoolTime = true;
        while (m_bIsMakeTurretSkillCoolTime)
        {
            m_fMakeTurretSkillCoolTimer += Time.deltaTime;
            if (m_fMakeTurretSkillCoolTimer >= DataTableManager.Instance.GetSkillData((int)SKILLNAME.MAKETURRET).SkillCoolTime)
                m_bIsMakeTurretSkillCoolTime = false;
            yield return null;
        }
        m_fMakeTurretSkillCoolTimer = 0;
    }
    IEnumerator MakeProvokeDollSkillCoolTimeFlow()
    {
        m_bIsMakeProvokeDollSkillCoolTime = true;
        while (m_bIsMakeProvokeDollSkillCoolTime)
        {
            m_fMakeProvokeDollSkillCoolTimer += Time.deltaTime;
            if (m_fMakeProvokeDollSkillCoolTimer >= DataTableManager.Instance.GetSkillData((int)SKILLNAME.MAKEPROVOKEDOLL).SkillCoolTime)
                m_bIsMakeProvokeDollSkillCoolTime = false;
            yield return null;
        }
        m_fMakeProvokeDollSkillCoolTimer = 0;
    }
    IEnumerator StimPackSkillCoolTimeFlow()
    {
        m_bIsStimPackSkillCoolTime = true;
        while (m_bIsStimPackSkillCoolTime)
        {
            m_fStimPackSkillCoolTimer += Time.deltaTime;
            if (m_fStimPackSkillCoolTimer >= DataTableManager.Instance.GetSkillData((int)SKILLNAME.USESTIMPACK).SkillCoolTime)
                m_bIsStimPackSkillCoolTime = false;
            yield return null;
        }
        m_fStimPackSkillCoolTimer = 0;
    }

    public bool UseSkill(SKILLNAME Skill)
    {
        if (!m_bCanUseSkill) return false; 
        if (!DataTableManager.Instance) return false;
        if (!GameManager.Instance) return false;
        var SkillData = DataTableManager.Instance.GetSkillData((int)Skill);
        int SKillLevel = GameManager.Instance.GetPlayerData.GetPlayerStatusData.GetSkillLevel(Skill);
        if (SKillLevel <= 0) return false;
        if (SkillData.SkillType == SKILLTYPE.PASSIVE) return false;

        switch (Skill)
        {
            case SKILLNAME.MAKETURRET:
                {
                    if(m_bIsMakeTurretSkillCoolTime)
                    {
                        GameManager.Instance.AddNewLog("터렛설치 스킬은 재사용 대기시간중입니다");
                        return false;
                    }
                    if (!GameManager.Instance.GetPlayerData.IsSkillMPCost(SkillData.SkillCost))
                    {
                        GameManager.Instance.AddNewLog("터렛설치 스킬을 사용하기 위한 MP가 부족합니다");
                        return false;
                    }

                    m_MyTurret.Init((int)SkillData.GetSkillTotalValue(SKillLevel), SkillData.SkillDurationTime, GameManager.Instance.GetPlayerTransform);
                    StartCoroutine(MakeTurretSkillCoolTimeFlow());
                }
                break;
            case SKILLNAME.MAKEPROVOKEDOLL:
                {
                    if (m_bIsMakeProvokeDollSkillCoolTime)
                    {
                        GameManager.Instance.AddNewLog("도발인형소환! 스킬은 재사용 대기시간중입니다");
                        return false;
                    }
                    if (!GameManager.Instance.GetPlayerData.IsSkillMPCost(SkillData.SkillCost))
                    {
                        GameManager.Instance.AddNewLog("도발인형소환! 스킬을 사용하기 위한 MP가 부족합니다");
                        return false;
                    }
                    m_MyProvokeDoll.Init((int)SkillData.GetSkillTotalValue(SKillLevel), SkillData.SkillDurationTime, GameManager.Instance.GetPlayerTransform);
                    StartCoroutine(MakeProvokeDollSkillCoolTimeFlow());
                }
                break;
            case SKILLNAME.USESTIMPACK:
                {
                    if (m_bIsStimPackSkillCoolTime)
                    {
                        GameManager.Instance.AddNewLog("스팀팩 스킬은 재사용 대기시간중입니다");
                        return false;
                    }
                    if (!GameManager.Instance.GetPlayerData.IsSkillHPCost(SkillData.SkillCost))
                    {
                        GameManager.Instance.AddNewLog("현재 체력이 최대체력의 10프로 이하여서 스팀팩 스킬을 사용 할 수 없습니다.");
                        return false;
                    }
                    GameManager.Instance.GetPlayerData.UseStimPack(SkillData.SkillDurationTime, SkillData.GetSkillTotalValue(SKillLevel));
                    StartCoroutine(StimPackSkillCoolTimeFlow());
                    if (SoundManager.Instance)
                        SoundManager.Instance.PlaySFX("UseConsumption");
                }
                break;
            default: return false;
        }
        return true;
    }
}
