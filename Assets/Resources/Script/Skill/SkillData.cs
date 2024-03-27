using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillData 
{
    public int SkillUniqueNumber = -1; //스킬의 고유번호
    public string SkillName = ""; //스킬의 이름
    public string SKillImageName = "";
    public SKILLTYPE SkillType = SKILLTYPE.NONE; //스킬의 종류
    public int SkillMaxLevel = -1; //스킬의 최대레벨
    public int SkillCost = -1; //스킬의 코스트 
    public float SkillCoolTime = -1; //스킬의 재사용대기시간
    public float SkillDurationTime = -1; //스킬의 지속시간
    public float SkillValue = -1; //스킬의 기본 위력
    public float SkillLevelPerValue = -1; //스킬의 레벨당 위력 증가량
    public string SkillDiscription = ""; //스킬의 설명 
    public float GetSkillTotalValue(int CurSkillLevel)
    {
        return (SkillValue + SkillLevelPerValue* CurSkillLevel);
    }
}
