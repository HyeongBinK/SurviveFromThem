using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragon_Dead : FSM<Mob_Dragon>
{
    private Mob_Dragon m_Owner;
    public Dragon_Dead(Mob_Dragon _owner)
    {
        m_Owner = _owner;
    }
    public override void Begin()
    {
        m_Owner.m_eCurState = DRAGON_ACT.DEAD;
    }

    public override void Exit()
    {
        m_Owner.m_ePrevState = DRAGON_ACT.DEAD;
    }

    public override void Run()
    {
    }
}
