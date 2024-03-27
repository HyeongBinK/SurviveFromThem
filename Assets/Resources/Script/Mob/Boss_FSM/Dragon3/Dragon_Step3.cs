using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragon_Step3 : FSM<Mob_Dragon>
{
    private Mob_Dragon m_Owner;
    public Dragon_Step3(Mob_Dragon _owner)
    {
        m_Owner = _owner;
    }
    public override void Begin()
    {
        m_Owner.m_eCurState = DRAGON_ACT.STEP3;
    }

    public override void Exit()
    {
        m_Owner.m_ePrevState = DRAGON_ACT.STEP3;
    }

    public override void Run()
    {
    }
}
