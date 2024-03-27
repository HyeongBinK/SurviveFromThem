using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Titan_Peace : FSM<Mob_Titan>
{
    private Mob_Titan m_Owner;
    public Titan_Peace(Mob_Titan _owner)
    {
        m_Owner = _owner;
    }
    public override void Begin()
    {
        m_Owner.m_eCurState = TITAN_ACT.PEACE;
    }

    public override void Exit()
    {
        m_Owner.m_ePrevState = TITAN_ACT.PEACE;
    }

    public override void Run()
    {
        m_Owner.Patrol();

        if(m_Owner.m_bIsChase)
        {
            m_Owner.ChangeFSM(TITAN_ACT.CHASE);
        }
    }
}
