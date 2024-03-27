using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillData 
{
    public int SkillUniqueNumber = -1; //��ų�� ������ȣ
    public string SkillName = ""; //��ų�� �̸�
    public string SKillImageName = "";
    public SKILLTYPE SkillType = SKILLTYPE.NONE; //��ų�� ����
    public int SkillMaxLevel = -1; //��ų�� �ִ뷹��
    public int SkillCost = -1; //��ų�� �ڽ�Ʈ 
    public float SkillCoolTime = -1; //��ų�� ������ð�
    public float SkillDurationTime = -1; //��ų�� ���ӽð�
    public float SkillValue = -1; //��ų�� �⺻ ����
    public float SkillLevelPerValue = -1; //��ų�� ������ ���� ������
    public string SkillDiscription = ""; //��ų�� ���� 
    public float GetSkillTotalValue(int CurSkillLevel)
    {
        return (SkillValue + SkillLevelPerValue* CurSkillLevel);
    }
}
