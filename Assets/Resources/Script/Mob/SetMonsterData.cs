using UnityEngine;
public enum MONSTER_TYPE
{
    NONE = -1,
    ZOMBIE =0,
    FASTZOMBIE,
    TITAN,
    GOLEM,
    DRAGON1,
    DRAGON2,
    DRAGON3,
    END
}
public class SetMonsterData
{
    public int Level;
    public MONSTER_TYPE MobNumber;
    protected MonsterStatus m_State = new MonsterStatus();
    public MonsterStatus GetState { get { return m_State; } }

    public SetMonsterData(int level, MONSTER_TYPE MobNumber)
    {
        this.Level = level;
        this.MobNumber = MobNumber;
        SetState();
    }
    public virtual void SetState() //받은 레벨을 기반으로 최종 스테이터스 세팅
    {
        if (!DataTableManager.Instance) return;
        //m_State = DataTableManager.Instance.GetMobData((int)MobNumber);
        m_State.SetStatus(DataTableManager.Instance.GetMobData((int)MobNumber));

        m_State.MaxHP += (m_State.LevelPerMaxHP * Level);
        m_State.AttackPoint += (m_State.LevelPerAttackPoint * Level);
        m_State.EXP += (m_State.LevelPerEXP * Level);
        m_State.Gold += (m_State.LevelPerGold * Level);
    }

    public void ATKBurf(float BurfVelocity) // 공격력강화 버프, 버프배율
    {
        m_State.AttackPoint = (int)(m_State.AttackPoint * BurfVelocity);
    }

    public void SPDBurf(float BurfVelocity) // 속도업 버프, 버프배율
    {
        m_State.Speed = m_State.Speed * BurfVelocity;
    }
}