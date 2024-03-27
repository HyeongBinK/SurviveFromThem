using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Titan_Step2 : FSM<Mob_Titan>
{
    private Mob_Titan m_Owner;
    private int PatternNumber;

    public Titan_Step2(Mob_Titan _owner)
    {
        m_Owner = _owner;
    }
    public override void Begin()
    {
        m_Owner.m_eCurState = TITAN_ACT.STEP2;
        PatternNumber = Random.Range(1, 3);
    }

    public override void Exit()
    {
        m_Owner.m_ePrevState = TITAN_ACT.STEP2;
    }

    public override void Run()
    {
        if (PatternNumber == 1)
            m_Owner.Attack2();
        else
            m_Owner.Attack3();
    }
}
