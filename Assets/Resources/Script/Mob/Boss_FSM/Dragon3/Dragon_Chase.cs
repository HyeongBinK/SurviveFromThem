using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragon_Chase : FSM<Mob_Dragon>
{
    private Mob_Dragon m_Owner;
    public Dragon_Chase(Mob_Dragon _owner)
    {
        m_Owner = _owner;
    }
    public override void Begin()
    {
        m_Owner.m_eCurState = DRAGON_ACT.CHASE;
    }

    public override void Exit()
    {
        m_Owner.m_ePrevState = DRAGON_ACT.CHASE;
    }

    public override void Run()
    {
        if (!m_Owner.m_bIsChase)
            m_Owner.ChangeFSM(DRAGON_ACT.SLEEP);

        m_Owner.ChangeToAttack();
        m_Owner.Chase();
    }
}
