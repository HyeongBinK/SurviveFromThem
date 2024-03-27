using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SKILLNAME
{
    MAKETURRET = 0,
    MAKEPROVOKEDOLL,
    USESTIMPACK,
    ATTACKUP,
    CRITICALRATEUP,
    SPEEDUP
}


[System.Serializable]
public class PlayerStatus 
{
    public string Name; //이름
    public int Level; //현재 레벨
    public int MaxHP { get { return 900 + (100 * Level); } } //최대 체력
    public int CurHP; //현재 체력
    public int MaxMP { get { return 90 + (10 * Level); } } //최대 마력
    public int CurMP; //현재 마력
    public int MaxEXP { get { return (10 * Level) + (int)Mathf.Pow(Level, 2); } } //레벨업에 필요한 경험치 수치
    public int CurEXP; //현재 경험치 수치
    public int SkillPoint; //스킬포인트
    public int MakeTurretSkillLevel; //터렛 소환 스킬 레벨
    public int MakeScareCrowSkillLevel; //허수아비 소환 스킬 레벨
    public int UseStimPackSkillLevel; //스팀팩 스킬 레벨
    public int SpeedUpSkillLevel; //기본이동속도 증가스킬(스킬 레벨 1당 0.2씩 증가, 만랩은 10)
    public int AttackUpSkillLevel; //공격력 증가스킬 //(무기의데미지 * (1 + (플레이어데미지 * 0.01)) 
    public int CriticalRateSkillLevel; //치명타확률증가스킬 (최대 100프로(스킬 레벨 1당 1씩 증가))

    public float m_fPlayerSpeed { get { return (5 + (SpeedUpSkillLevel * 0.1f)); } }

    public void SetStatus(string NewName, int NewLevel, int NewCurHP, int NewCurMP, int NewCurEXP, int NewSKillPoint,
        int NewMakeTurretSkillLevel,int NewMakeScareCrowSkillLevel, int NewSteamPackSkillLevel, int NewSpeedUpSkillLevel, int NewAttackUpSkillLevel, int NewCriticalRateSkillLevel)
    {
        Name = NewName;
        Level = NewLevel;
        CurHP = NewCurHP;
        CurMP = NewCurMP;
        CurEXP = NewCurEXP;
        SkillPoint = NewSKillPoint;
        MakeTurretSkillLevel = NewMakeTurretSkillLevel;
        MakeScareCrowSkillLevel = NewMakeScareCrowSkillLevel;
        UseStimPackSkillLevel = NewSteamPackSkillLevel;
        SpeedUpSkillLevel = NewSpeedUpSkillLevel;
        AttackUpSkillLevel = NewAttackUpSkillLevel;
        CriticalRateSkillLevel = NewCriticalRateSkillLevel;
    }
    public void GetDeadPenalty()
    {
        CurHP = (int)(MaxHP * 0.5);
        CurMP = (int)(MaxMP * 0.5);
        CurEXP = (int)(CurEXP * 0.8);
        RefreshUI();
    }
    public bool LevelUpCheck() //경험치 습득시마다 불려짐
    {
        if (CurEXP >= MaxEXP)
        {
            LevelChange(); //레벨변경
            return true;
        }
        return false;
    }
    private void LevelChange() //레밸업
    {
        if (CurEXP >= MaxEXP)
        {
            CurEXP -= MaxEXP;
            LevelUP(); // 레벨업
            LevelChange(); //추가 레벨업을 할수있는가?
        }
    }
    private void LevelUP() //레밸업시 레밸및 기초능력증가와 스탯,스킬포인트 지급 
    {
        Level++; //레벨 1증가
        SkillPoint++; //스킬포인트 1증가
        GameManager.Instance.AddNewLog(Name + "님께서 레밸업을하여 " + Level.ToString() + "Level이 되셨습니다."); //시스템박스에 정보표시
        PercentHeal(100);
        RefreshUI();
        //레벨업 이팩트 발생
    }

    public bool IsSkillPoint()
    {
        if (SkillPoint > 0)
            return true;
        
        return false;
    }
    public void PercentHeal(int Value) //Value수치의 퍼센티지만큼회복
    {
        CurHP = Mathf.Clamp(CurHP + (int)((MaxHP * Value) * 0.01), 0, MaxHP);
        CurMP = Mathf.Clamp(CurMP + (int)((MaxMP * Value) * 0.01), 0, MaxMP);

    }
    public void RefreshUI() // Level Up, 사망 패널티시 UI전체 갱신
    {
        if (UIManager.Instance) 
        {
            UIManager.Instance.GetUserDataUI.SetNameText(Name);
            UIManager.Instance.GetUserDataUI.SetLevelText(Level);
            UIManager.Instance.GetUserDataUI.SetHPSlider(CurHP, MaxHP);
            UIManager.Instance.GetUserDataUI.SetMPSlider(CurMP, MaxMP);
            UIManager.Instance.GetUserDataUI.SetEXPSlider(CurEXP, MaxEXP);
            if (UIManager.Instance.GetUserInfo.gameObject.activeSelf)
                UIManager.Instance.GetUserInfo.RefreshUserInfo();
            UIManager.Instance.RefreshSkillWindow();
        }
    }

    public int GetSkillLevel(SKILLNAME SkillNumber)
    {
        switch (SkillNumber)
        {
            case SKILLNAME.MAKETURRET:
                return MakeTurretSkillLevel;
            case SKILLNAME.MAKEPROVOKEDOLL:
                return MakeScareCrowSkillLevel;
            case SKILLNAME.USESTIMPACK:
                return UseStimPackSkillLevel;
            case SKILLNAME.ATTACKUP:
                return AttackUpSkillLevel;
            case SKILLNAME.CRITICALRATEUP:
                return CriticalRateSkillLevel;
            case SKILLNAME.SPEEDUP:
                return SpeedUpSkillLevel;
        }

        return 0;
    }
    public bool SkillLevelUp(SKILLNAME SkillNumber)
    {
        if (SkillPoint <= 0) return false;
        if (!DataTableManager.Instance) return false;

        var SkillData = DataTableManager.Instance.GetSkillData((int)SkillNumber);
        switch (SkillNumber)
        {
            case SKILLNAME.MAKETURRET:
                {
                    if (SkillData.SkillMaxLevel > MakeTurretSkillLevel)
                        MakeTurretSkillLevel = Mathf.Clamp(MakeTurretSkillLevel + 1, 0, SkillData.SkillMaxLevel);
                    else
                        return false;
                }
                break;
            case SKILLNAME.MAKEPROVOKEDOLL:
                {
                    if (SkillData.SkillMaxLevel > MakeScareCrowSkillLevel)
                        MakeScareCrowSkillLevel = Mathf.Clamp(MakeScareCrowSkillLevel + 1, 0, SkillData.SkillMaxLevel);
                    else
                        return false;
                }
                break;
            case SKILLNAME.USESTIMPACK:
                {
                    if (SkillData.SkillMaxLevel > UseStimPackSkillLevel)
                        UseStimPackSkillLevel = Mathf.Clamp(UseStimPackSkillLevel + 1, 0, SkillData.SkillMaxLevel);
                    else
                        return false;
                }
                break;
            case SKILLNAME.ATTACKUP:
                {
                    if (SkillData.SkillMaxLevel > AttackUpSkillLevel)
                        AttackUpSkillLevel = Mathf.Clamp(AttackUpSkillLevel + 1, 0, SkillData.SkillMaxLevel);
                    else
                        return false;
                }
                break;
            case SKILLNAME.CRITICALRATEUP:
                {
                    if (SkillData.SkillMaxLevel > CriticalRateSkillLevel)
                        CriticalRateSkillLevel = Mathf.Clamp(CriticalRateSkillLevel + 1, 0, SkillData.SkillMaxLevel);
                    else
                        return false;
                }
                break;
            case SKILLNAME.SPEEDUP:
                {
                    if (SkillData.SkillMaxLevel > SpeedUpSkillLevel)
                    {
                        SpeedUpSkillLevel = Mathf.Clamp(SpeedUpSkillLevel + 1, 0, SkillData.SkillMaxLevel);
                        if (GameManager.Instance) GameManager.Instance.SetPlayerSpeed();
                    }
                    else
                        return false;
                }
                break;
        }

        SkillPoint--;
        if(UIManager.Instance)
            UIManager.Instance.RefreshSkillWindow();
        return true;
    }
}
