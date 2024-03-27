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
    public string Name; //�̸�
    public int Level; //���� ����
    public int MaxHP { get { return 900 + (100 * Level); } } //�ִ� ü��
    public int CurHP; //���� ü��
    public int MaxMP { get { return 90 + (10 * Level); } } //�ִ� ����
    public int CurMP; //���� ����
    public int MaxEXP { get { return (10 * Level) + (int)Mathf.Pow(Level, 2); } } //�������� �ʿ��� ����ġ ��ġ
    public int CurEXP; //���� ����ġ ��ġ
    public int SkillPoint; //��ų����Ʈ
    public int MakeTurretSkillLevel; //�ͷ� ��ȯ ��ų ����
    public int MakeScareCrowSkillLevel; //����ƺ� ��ȯ ��ų ����
    public int UseStimPackSkillLevel; //������ ��ų ����
    public int SpeedUpSkillLevel; //�⺻�̵��ӵ� ������ų(��ų ���� 1�� 0.2�� ����, ������ 10)
    public int AttackUpSkillLevel; //���ݷ� ������ų //(�����ǵ����� * (1 + (�÷��̾���� * 0.01)) 
    public int CriticalRateSkillLevel; //ġ��ŸȮ��������ų (�ִ� 100����(��ų ���� 1�� 1�� ����))

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
    public bool LevelUpCheck() //����ġ ����ø��� �ҷ���
    {
        if (CurEXP >= MaxEXP)
        {
            LevelChange(); //��������
            return true;
        }
        return false;
    }
    private void LevelChange() //�����
    {
        if (CurEXP >= MaxEXP)
        {
            CurEXP -= MaxEXP;
            LevelUP(); // ������
            LevelChange(); //�߰� �������� �Ҽ��ִ°�?
        }
    }
    private void LevelUP() //������� ����� ���ʴɷ������� ����,��ų����Ʈ ���� 
    {
        Level++; //���� 1����
        SkillPoint++; //��ų����Ʈ 1����
        GameManager.Instance.AddNewLog(Name + "�Բ��� ��������Ͽ� " + Level.ToString() + "Level�� �Ǽ̽��ϴ�."); //�ý��۹ڽ��� ����ǥ��
        PercentHeal(100);
        RefreshUI();
        //������ ����Ʈ �߻�
    }

    public bool IsSkillPoint()
    {
        if (SkillPoint > 0)
            return true;
        
        return false;
    }
    public void PercentHeal(int Value) //Value��ġ�� �ۼ�Ƽ����ŭȸ��
    {
        CurHP = Mathf.Clamp(CurHP + (int)((MaxHP * Value) * 0.01), 0, MaxHP);
        CurMP = Mathf.Clamp(CurMP + (int)((MaxMP * Value) * 0.01), 0, MaxMP);

    }
    public void RefreshUI() // Level Up, ��� �г�Ƽ�� UI��ü ����
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
