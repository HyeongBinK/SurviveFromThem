using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Titan_Dead : FSM<Mob_Titan>
{
    private Mob_Titan m_Owner;
    public Titan_Dead(Mob_Titan _owner)
    {
        m_Owner = _owner;
    }
    public override void Begin()
    {
        m_Owner.m_eCurState = TITAN_ACT.DEAD;
        //사망이벤트 시작
        m_Owner.StartDeadEvent();
    }

    public override void Exit()
    {
        m_Owner.m_ePrevState = TITAN_ACT.DEAD;
    }

    public override void Run() //없음
    {
    }
}
