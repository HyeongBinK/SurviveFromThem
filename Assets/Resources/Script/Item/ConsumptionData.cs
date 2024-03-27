using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CONSUMPTIONTYPE
{
    START = 0,
    HP = 0,  //체력을 해당수치만큼 회복
    MP, //마력을 해당수치만큼 회복
    ELIXIR, //체력과 마력을 해당수치의 퍼센티지만큼 회복
    EXP,  //경험치를 해당수치만큼 획득
    WARPCAPSULE, //순간이동(위치이동)아이템
    END
}
public enum WARPPOINT //추후 맵추가시 추가예정
{
    START = 0,
    TOWN, //마을(아이템구매등  npc들이 있는 안전지대)
    END
}

public class ConsumptionData
{
    public int ItemUniqueNumber; //아이템고유번호
    public string ItemName; //아이템의 이름
    public CONSUMPTIONTYPE ItemType; //소모아이템의 종류
    public int Value; //회복수치, 이동캡슙일 경우 워프포인트의 인트값
}
