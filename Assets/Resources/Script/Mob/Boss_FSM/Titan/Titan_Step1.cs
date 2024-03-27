using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Titan_Step1 : FSM<Mob_Titan>
{
    private Mob_Titan m_Owner;
    private int PatternNumber; //어떤패턴을 시전할지 

    public Titan_Step1(Mob_Titan _owner)
    {
        m_Owner = _owner;
    }
    public override void Begin()
    {
        m_Owner.m_eCurState = TITAN_ACT.STEP1;
        PatternNumber = Random.Range(1, 3);
    }

    public override void Exit()
    {
        m_Owner.m_ePrevState = TITAN_ACT.STEP1;
    }

    public override void Run()
    {
        if (PatternNumber == 1)
            m_Owner.Attack1();
        else
            m_Owner.Attack2();
    }
}
