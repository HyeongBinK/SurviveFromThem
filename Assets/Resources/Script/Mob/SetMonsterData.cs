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
    public virtual void SetState() //���� ������ ������� ���� �������ͽ� ����
    {
        if (!DataTableManager.Instance) return;
        //m_State = DataTableManager.Instance.GetMobData((int)MobNumber);
        m_State.SetStatus(DataTableManager.Instance.GetMobData((int)MobNumber));

        m_State.MaxHP += (m_State.LevelPerMaxHP * Level);
        m_State.AttackPoint += (m_State.LevelPerAttackPoint * Level);
        m_State.EXP += (m_State.LevelPerEXP * Level);
        m_State.Gold += (m_State.LevelPerGold * Level);
    }

    public void ATKBurf(float BurfVelocity) // ���ݷ°�ȭ ����, ��������
    {
        m_State.AttackPoint = (int)(m_State.AttackPoint * BurfVelocity);
    }

    public void SPDBurf(float BurfVelocity) // �ӵ��� ����, ��������
    {
        m_State.Speed = m_State.Speed * BurfVelocity;
    }
}