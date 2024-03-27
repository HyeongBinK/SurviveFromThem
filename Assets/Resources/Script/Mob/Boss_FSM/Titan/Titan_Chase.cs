using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Titan_Chase : FSM<Mob_Titan>
{
    private Mob_Titan m_Owner;
    public Titan_Chase(Mob_Titan _owner)
    {
        m_Owner = _owner;
    }
    public override void Begin()
    {
        m_Owner.m_eCurState = TITAN_ACT.CHASE;
    }

    public override void Exit()
    {
        m_Owner.m_ePrevState = TITAN_ACT.CHASE;
    }

    public override void Run()
    {
        if (!m_Owner.m_bIsChase)
            m_Owner.ChangeFSM(TITAN_ACT.PEACE);
        
        m_Owner.ChangeToAttack();
        m_Owner.Chase();
    }
}
