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
    private static SkillManager m_instance; //�̱��� �Ҵ�

    public static SkillManager Instance
    {
        get
        {
            // ���� �̱��� ������ ���� ������Ʈ�� �Ҵ���� �ʾҴٸ�
            if (m_instance == null)
            {
                // ������ GameManager ������Ʈ�� ã�� �Ҵ�
                m_instance = FindObjectOfType<SkillManager>();
            }
            // �̱��� ������Ʈ�� ��ȯ
            return m_instance;
        }
    }

    [SerializeField] private Turret m_MyTurret; //�ͷ���ȯ ��ų ���� ��ȯ�� �ͷ� ������Ʈ
    [SerializeField] private ProvokeDoll m_MyProvokeDoll; //�������� ��ų ���� ��ȯ�� �������� ������Ʈ
    private float m_fMakeTurretSkillCoolTimer = 0.0f; //�ͷ���ȯ��ų ��Ÿ�� Ÿ�̸�
    private float m_fMakeProvokeDollSkillCoolTimer = 0.0f; //����������ȯ��ų ��Ÿ�� Ÿ�̸�
    private float m_fStimPackSkillCoolTimer = 0.0f; //�����ѽ�ų ��Ÿ�� Ÿ�̸�
    private bool m_bIsMakeTurretSkillCoolTime; //�ͷ���ȯ ��ų�� ��Ÿ���ΰ�?
    private bool m_bIsMakeProvokeDollSkillCoolTime; //�������� ��ȯ ��ų�� ��Ÿ���ΰ�?
    private bool m_bIsStimPackSkillCoolTime; //������ ��ų�� ��Ÿ���ΰ�?
    private bool m_bCanUseSkill; //true : ��ų��� ���� false : ��ų ���Ұ���

    private void Awake()

    {  // ���� �̱��� ������Ʈ�� �� �ٸ� SkillManager ������Ʈ�� �ִٸ�
        if (Instance != this)
        {
            // �ڽ��� �ı�
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
                Debug.Log("��ų ��Ÿ�������� �������µ����� ���� �߻�");
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
                Debug.Log("��ų ��Ÿ�������� �������µ����� ���� �߻�");
                return false;
        }
    }

    public void DisActiveSummonObject() //���̵��� �ϰԵǾ ��ȯ������ ������ ��Ȱ��ȭ �ؾ��ҋ�
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
                        GameManager.Instance.AddNewLog("�ͷ���ġ ��ų�� ���� ���ð����Դϴ�");
                        return false;
                    }
                    if (!GameManager.Instance.GetPlayerData.IsSkillMPCost(SkillData.SkillCost))
                    {
                        GameManager.Instance.AddNewLog("�ͷ���ġ ��ų�� ����ϱ� ���� MP�� �����մϴ�");
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
                        GameManager.Instance.AddNewLog("����������ȯ! ��ų�� ���� ���ð����Դϴ�");
                        return false;
                    }
                    if (!GameManager.Instance.GetPlayerData.IsSkillMPCost(SkillData.SkillCost))
                    {
                        GameManager.Instance.AddNewLog("����������ȯ! ��ų�� ����ϱ� ���� MP�� �����մϴ�");
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
                        GameManager.Instance.AddNewLog("������ ��ų�� ���� ���ð����Դϴ�");
                        return false;
                    }
                    if (!GameManager.Instance.GetPlayerData.IsSkillHPCost(SkillData.SkillCost))
                    {
                        GameManager.Instance.AddNewLog("���� ü���� �ִ�ü���� 10���� ���Ͽ��� ������ ��ų�� ��� �� �� �����ϴ�.");
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
