using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class Player : MonoBehaviour
{
    [SerializeField] private PlayerStatus m_State; //�÷��̾� �������ͽ�
    private Inventory m_Inventory = new Inventory(); //�κ��丮
    public Inventory GetInventory {get { return m_Inventory; } } //�κ��丮������ �Լ��� ����� ����
    public PlayerStatus GetPlayerStatusData { get { return m_State; } }
    public int GetPlayerAtkUpSkillLevel { get { return m_State.AttackUpSkillLevel; } }
    private int BaseCriticalRate = 10; //�⺻ũ��Ƽ�� Ȯ��
    public int m_iCriticalRate { get { return Mathf.Clamp(BaseCriticalRate + m_State.CriticalRateSkillLevel, 0, 100); } }
    public float m_fCriticalDammage = 1.5f; //ũ��Ƽ�ù߻��� ���������� //���Ŀ� ũ��Ƽ�õ����� ������ų ����� ����
    private float m_fRebirthTime; //��Ȱ�� �ʿ��ѽð� 
    private bool m_bIsInvincible; //���������ΰ� true : �������� 
    public bool m_bIsDead { get; private set; } //���������ΰ� 

    private readonly int StartCriticalRate = 10;
    private readonly float StartCriticalDammage = 1.5f;
    public readonly float RebirthCoolTIme = 5.0f; //�÷��̾ ����� ��Ȱ�ϴµ� �ɸ��� �ð�
    private readonly string PlayerSaveDataName = "PlayerStateSaveData"; //�Ŀ� �ð��� ���������� �������·� ���濹��(ĳ���� ����â ������ �����ҽ�)
    private readonly string PlayerDefaultDataName = "PlayerDefaultData"; //����Ʈ ������ ���ϳ���
    private readonly string PlayerCheatDataName = "PlayerCheatStateData"; //�׽�Ʈ�� �����ϰ� ���� ġƮ���ϳ���
    private void Awake()
    {
        DefaultSetting();
        m_State.RefreshUI();
        m_bIsDead = false;
        m_bIsInvincible = false;
        m_fRebirthTime = RebirthCoolTIme;
    }
    private void DefaultSetting()
    {
        //LoadStateData(PlayerDefaultDataName);
        UseCheat();
        m_Inventory.DefaultSetting();
        BaseCriticalRate = StartCriticalRate;
        m_fCriticalDammage = StartCriticalDammage;
    }
    public void UseCheat()
    {
        LoadStateData(PlayerCheatDataName);
    }
    public void SaveStateData() //���̺�����
    {
        string json = JsonUtility.ToJson(m_State);
        File.WriteAllText(DataSaveAndLoad.Instance.MakeFilePath(PlayerSaveDataName, "/SaveData/PlayerStateData/"), json);
    }

    public bool LoadStateData(string FileName) //�ε�����
    {
        string FilePath = DataSaveAndLoad.Instance.MakeFilePath(FileName, "/SaveData/PlayerStateData/");
        if (!File.Exists(FilePath))
        {
            Debug.LogError("�÷��̾� �������ͽ� ���̺������� ã�����߽��ϴ�.");
            return false;
        }
        string SaveFile = File.ReadAllText(FilePath);
        PlayerStatus PlayerStateData = JsonUtility.FromJson<PlayerStatus>(SaveFile);

        m_State.SetStatus(PlayerStateData.Name, PlayerStateData.Level, PlayerStateData.CurHP, PlayerStateData.CurMP, PlayerStateData.CurEXP,
            PlayerStateData.SkillPoint, PlayerStateData.MakeTurretSkillLevel, PlayerStateData.MakeScareCrowSkillLevel, PlayerStateData.UseStimPackSkillLevel,
            PlayerStateData.SpeedUpSkillLevel, PlayerStateData.AttackUpSkillLevel, PlayerStateData.CriticalRateSkillLevel);

        GameManager.Instance.AddNewLog("�÷��̾� ������ �ε�Ϸ�");
        return true;
    }
    public void GetExp(int NewExp)
    {
        m_State.CurEXP += NewExp;
        if (m_State.LevelUpCheck())
        {
            //����������Ʈ�߻�
            if (SoundManager.Instance)
                SoundManager.Instance.PlaySFX("LevelUp");
        }

        if (UIManager.Instance)
        {
            UIManager.Instance.GetUserDataUI.SetEXPSlider(m_State.CurEXP, m_State.MaxEXP);
            if (UIManager.Instance.GetUserInfo.gameObject.activeSelf)
                UIManager.Instance.GetUserInfo.SetUserStatusDataWindow();
        }
    }
    public void GetDamage(int NewDamage)
    {
        if (NewDamage == 0 || m_bIsInvincible || m_bIsDead)
            return;

        m_State.CurHP = Mathf.Clamp(m_State.CurHP - NewDamage, 0, m_State.MaxHP);
        if (UIManager.Instance)
        {
            UIManager.Instance.GetUserDataUI.SetHPSlider(m_State.CurHP, m_State.MaxHP);
            if(UIManager.Instance.GetUserInfo.gameObject.activeSelf)
            UIManager.Instance.GetUserInfo.RefreshUserInfo();
            UIManager.Instance.ActivePlayerHitEffect();
        }

        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("Player_Damage");

        if (m_State.CurHP <= 0)
        {
            StartCoroutine(PlayerDeadEvent());
            return;
        }        
    }
    public void SetPlayerName(string NewName)
    {
        m_State.Name = NewName;
        if (UIManager.Instance)
        {
            UIManager.Instance.GetUserDataUI.SetNameText(m_State.Name);
            if (UIManager.Instance.GetUserInfo.gameObject.activeSelf)
                UIManager.Instance.GetUserInfo.SetUserStatusDataWindow();
        }
    }
    public void SetInvincibility(bool NewBoolean)
    {
        m_bIsInvincible = NewBoolean;
    }

    IEnumerator PlayerDeadEvent() //�÷��̾��� ��� �̺�Ʈ
    {
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("PlayerDie");
        m_bIsDead = true;
        m_bIsInvincible = true;
        if (UIManager.Instance)
            UIManager.Instance.ActiveDeadFade();
        yield return new WaitForSeconds(m_fRebirthTime);

        m_State.GetDeadPenalty();
        m_bIsDead = false;
        m_bIsInvincible = false;
        GameManager.Instance.PlayerDeadWarp();
    }
    public void UseItem(CONSUMPTIONTYPE Type, int RecoveryRate) //������ ���� �÷��̾���� ���� //�������� Ÿ�԰� ȸ����
    {
        switch (Type)
        {
            case CONSUMPTIONTYPE.HP:
                {
                    m_State.CurHP = Mathf.Clamp(m_State.CurHP + RecoveryRate, 0, m_State.MaxHP);
                }
                break;
            case CONSUMPTIONTYPE.MP:
                {
                    m_State.CurMP = Mathf.Clamp(m_State.CurMP + RecoveryRate, 0, m_State.MaxMP);
                }
                break;
            case CONSUMPTIONTYPE.ELIXIR:
                {
                    m_State.PercentHeal(RecoveryRate);
                }
                break;
            case CONSUMPTIONTYPE.EXP:
                {
                    GetExp(RecoveryRate);
                }
                break;
            default:
                Debug.Log("�Ҹ������Ÿ�Կ� �̻��Ѱ�����");
                return;
        }
        m_State.RefreshUI();
    }

    //�κ��丮 ���úκ�
    public void GetGold(int NewGold)
    {
        m_Inventory.GetGold(NewGold);
    }

    public bool AddItem(ITEMTYPE type, int ItemUniqueNumber, int Quantity)
    {
        return m_Inventory.AddItem(type, ItemUniqueNumber, Quantity);
    }
    public ItemSlotData GetInventorySlotData(ITEMTYPE Type, int SlotNumber)
    {
        return m_Inventory.GetInventorySlotData(Type, SlotNumber);
    }

    //��ų ���úκ�
    public bool IsSkillMPCost(int Value) 
    {
        if (m_State.CurMP >= Value)
        {
            m_State.CurMP -= Value;
            m_State.RefreshUI();
            return true;
        }
        return false;
    }
    public bool IsSkillHPCost(int Value)
    {
        if (m_State.CurHP > m_State.MaxHP * (0.01f * Value)) 
        {
            m_State.CurHP -= (int)((float)m_State.MaxHP * (0.01f * Value));
            m_State.RefreshUI();
            return true;
        }
        return false;
    }
    IEnumerator StimPack(float LifeTime, float Value) //�÷��̾��� �ɷ�ġ�� ������ �ִ� ��ų�̱⿡ ����� �̰��� ����
    {
        m_fCriticalDammage += Value;
        BaseCriticalRate = 100;
        if (UIManager.Instance) UIManager.Instance.GetUserInfo.RefreshUserInfo();

        yield return new WaitForSeconds(LifeTime);

        m_fCriticalDammage = StartCriticalDammage;
        BaseCriticalRate = StartCriticalRate;
        if (UIManager.Instance) UIManager.Instance.GetUserInfo.RefreshUserInfo();
    }
    public void UseStimPack(float LifeTime, float Value)
    {
        StartCoroutine(StimPack(LifeTime, Value));
    }


}
