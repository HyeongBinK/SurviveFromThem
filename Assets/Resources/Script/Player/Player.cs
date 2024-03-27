using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class Player : MonoBehaviour
{
    [SerializeField] private PlayerStatus m_State; //플레이어 스테이터스
    private Inventory m_Inventory = new Inventory(); //인벤토리
    public Inventory GetInventory {get { return m_Inventory; } } //인벤토리내부의 함수를 쓸경우 접근
    public PlayerStatus GetPlayerStatusData { get { return m_State; } }
    public int GetPlayerAtkUpSkillLevel { get { return m_State.AttackUpSkillLevel; } }
    private int BaseCriticalRate = 10; //기본크리티컬 확률
    public int m_iCriticalRate { get { return Mathf.Clamp(BaseCriticalRate + m_State.CriticalRateSkillLevel, 0, 100); } }
    public float m_fCriticalDammage = 1.5f; //크리티컬발생시 데미지배율 //이후에 크리티컬데미지 증가스킬 생길시 수정
    private float m_fRebirthTime; //부활에 필요한시간 
    private bool m_bIsInvincible; //무적상태인가 true : 무적상태 
    public bool m_bIsDead { get; private set; } //죽은상태인가 

    private readonly int StartCriticalRate = 10;
    private readonly float StartCriticalDammage = 1.5f;
    public readonly float RebirthCoolTIme = 5.0f; //플레이어가 사망후 부활하는데 걸리는 시간
    private readonly string PlayerSaveDataName = "PlayerStateSaveData"; //후에 시간에 여유있을시 슬롯형태로 변경예정(캐릭터 선택창 같은거 구현할시)
    private readonly string PlayerDefaultDataName = "PlayerDefaultData"; //디폴트 데이터 파일네임
    private readonly string PlayerCheatDataName = "PlayerCheatStateData"; //테스트를 용이하게 해줄 치트파일네임
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
    public void SaveStateData() //세이브파일
    {
        string json = JsonUtility.ToJson(m_State);
        File.WriteAllText(DataSaveAndLoad.Instance.MakeFilePath(PlayerSaveDataName, "/SaveData/PlayerStateData/"), json);
    }

    public bool LoadStateData(string FileName) //로드파일
    {
        string FilePath = DataSaveAndLoad.Instance.MakeFilePath(FileName, "/SaveData/PlayerStateData/");
        if (!File.Exists(FilePath))
        {
            Debug.LogError("플레이어 스테이터스 세이브파일을 찾지못했습니다.");
            return false;
        }
        string SaveFile = File.ReadAllText(FilePath);
        PlayerStatus PlayerStateData = JsonUtility.FromJson<PlayerStatus>(SaveFile);

        m_State.SetStatus(PlayerStateData.Name, PlayerStateData.Level, PlayerStateData.CurHP, PlayerStateData.CurMP, PlayerStateData.CurEXP,
            PlayerStateData.SkillPoint, PlayerStateData.MakeTurretSkillLevel, PlayerStateData.MakeScareCrowSkillLevel, PlayerStateData.UseStimPackSkillLevel,
            PlayerStateData.SpeedUpSkillLevel, PlayerStateData.AttackUpSkillLevel, PlayerStateData.CriticalRateSkillLevel);

        GameManager.Instance.AddNewLog("플레이어 데이터 로드완료");
        return true;
    }
    public void GetExp(int NewExp)
    {
        m_State.CurEXP += NewExp;
        if (m_State.LevelUpCheck())
        {
            //레벨업이펙트발생
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

    IEnumerator PlayerDeadEvent() //플레이어의 사망 이벤트
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
    public void UseItem(CONSUMPTIONTYPE Type, int RecoveryRate) //아이템 사용시 플레이어에게의 영향 //아이템의 타입과 회복양
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
                Debug.Log("소모아이템타입에 이상한값있음");
                return;
        }
        m_State.RefreshUI();
    }

    //인벤토리 관련부분
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

    //스킬 관련부분
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
    IEnumerator StimPack(float LifeTime, float Value) //플레이어의 능력치에 영향을 주는 스킬이기에 기능이 이곳에 있음
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
