using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CONSUMPTIONTYPE
{
    START = 0,
    HP = 0,  //ü���� �ش��ġ��ŭ ȸ��
    MP, //������ �ش��ġ��ŭ ȸ��
    ELIXIR, //ü�°� ������ �ش��ġ�� �ۼ�Ƽ����ŭ ȸ��
    EXP,  //����ġ�� �ش��ġ��ŭ ȹ��
    WARPCAPSULE, //�����̵�(��ġ�̵�)������
    END
}
public enum WARPPOINT //���� ���߰��� �߰�����
{
    START = 0,
    TOWN, //����(�����۱��ŵ�  npc���� �ִ� ��������)
    END
}

public class ConsumptionData
{
    public int ItemUniqueNumber; //�����۰�����ȣ
    public string ItemName; //�������� �̸�
    public CONSUMPTIONTYPE ItemType; //�Ҹ�������� ����
    public int Value; //ȸ����ġ, �̵�ĸ���� ��� ��������Ʈ�� ��Ʈ��
}
