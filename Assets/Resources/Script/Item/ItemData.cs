using UnityEngine;
public enum ITEMTYPE
{
    NULL = 0, //데이터없음
    WEAPON = 1, //장착아이템(무기)류
    CONSUMPTION, //소모아이템류
    ETC //재료아이템류
}

[System.Serializable]
public class ItemData
{
    public int UniquiNumber; //아이템고유번호
    public string Name; //아이템이름
    public ITEMTYPE Type; //아이템의 타입
    public int Price; //아이템가격
    public string ImageName; //아이템UI이미지의 이름
    public string Discription; //아이템설명
}
