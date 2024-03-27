using UnityEngine;
public enum ITEMTYPE
{
    NULL = 0, //�����;���
    WEAPON = 1, //����������(����)��
    CONSUMPTION, //�Ҹ�����۷�
    ETC //�������۷�
}

[System.Serializable]
public class ItemData
{
    public int UniquiNumber; //�����۰�����ȣ
    public string Name; //�������̸�
    public ITEMTYPE Type; //�������� Ÿ��
    public int Price; //�����۰���
    public string ImageName; //������UI�̹����� �̸�
    public string Discription; //�����ۼ���
}
