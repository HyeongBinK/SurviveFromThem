using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
public class DataTableManager : MonoBehaviour
{
    private static DataTableManager m_instance;
    public static DataTableManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<DataTableManager>();
                m_instance.LoadMonsterDataTable();
                m_instance.LoadItemDataTable();
                m_instance.LoadWeaponDataTable();
                m_instance.LoadConsumptionDataTable();
                m_instance.LoadSkillDataTalbe();
                DontDestroyOnLoad(m_instance.gameObject);
            }
            return m_instance;
        }
    }

    private Dictionary<int, MonsterStatus> MonsterDataTable = new Dictionary<int, MonsterStatus>(); //Key : 몬스터고유번호, Value : 몬스터스테이터스
    private Dictionary<int, ItemData> ItemDataTable = new Dictionary<int, ItemData>(); //Key : 아이템고유번호, Value : 아이템데이터
    private Dictionary<int, WeaponData> WeaponDataTable = new Dictionary<int, WeaponData>(); //Key : 총기고유번호, Value : 총기데이터
    private Dictionary<int, ConsumptionData> ConsumptionDataTable = new Dictionary<int, ConsumptionData>(); //Key : 총기고유번호, Value : 총기데이터
    private Dictionary<int, SkillData> SkillDataTable = new Dictionary<int, SkillData>(); //Key : 스킬고유번호, Value : 스킬데이터

    private void Awake()
    {
        // 씬에 싱글톤 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        if (Instance != this)
        {
            // 자신을 파괴
            Destroy(gameObject);
        }
    }

    private void LoadMonsterDataTable() //몬스터 데이터테이블에서 몬스터데이터들 불러오기
    {
        List<MonsterStatus> MonsterDatas = XML<MonsterStatus>.Read("DataTable/MonsterDataTable");
        for (int i = 0; i < MonsterDatas.Count; i++)
        {
            MonsterDataTable.Add(MonsterDatas[i].MobUniqueNumber, MonsterDatas[i]);
        }
    }
    private void LoadItemDataTable()
    {
        List<ItemData> ItemDatas = XML<ItemData>.Read("DataTable/ItemDataTable");
        for (int i = 0; i < ItemDatas.Count; i++)
        {
            ItemDataTable.Add(ItemDatas[i].UniquiNumber, ItemDatas[i]);
        }
    }
    private void LoadWeaponDataTable()
    {
        List<WeaponData> WeaponDatas = XML<WeaponData>.Read("DataTable/WeaponDataTable");
        for (int i = 0; i < WeaponDatas.Count; i++)
        {
            WeaponDataTable.Add(WeaponDatas[i].WeaponUniqueNumber, WeaponDatas[i]);
        }
    }
    private void LoadConsumptionDataTable()
    {
        List<ConsumptionData> ConsumptionDatas = XML<ConsumptionData>.Read("DataTable/ConsumptionDataTable");
        for (int i = 0; i < ConsumptionDatas.Count; i++)
        {
            ConsumptionDataTable.Add(ConsumptionDatas[i].ItemUniqueNumber, ConsumptionDatas[i]);
        }
    }
    private void LoadSkillDataTalbe()
    {
        List<SkillData> SkillDatas = XML<SkillData>.Read("DataTable/SkillDataTable");
        for (int i = 0; i < SkillDatas.Count; i++)
        {
            SkillDataTable.Add(SkillDatas[i].SkillUniqueNumber, SkillDatas[i]);
        }
    }
    public MonsterStatus GetMobData(int MobUniqueNumber)
    {
        if (MonsterDataTable.ContainsKey(MobUniqueNumber))
        {
            return MonsterDataTable[MobUniqueNumber];
        }
        return new MonsterStatus();
    }
    public ItemData GetItemData(int ItemUniqueNumber)
    {
        if (ItemDataTable.ContainsKey(ItemUniqueNumber))
        {
            return ItemDataTable[ItemUniqueNumber];
        }
        return new ItemData();
    }
    public WeaponData GetWeaponData(int WeaponUniqueNumber)
    {
        if (WeaponDataTable.ContainsKey(WeaponUniqueNumber))
        {
            return WeaponDataTable[WeaponUniqueNumber];
        }
        return new WeaponData();
    }
    public ConsumptionData GetConsumptionData(int ItemUniqueNumber)
    {
        if (ConsumptionDataTable.ContainsKey(ItemUniqueNumber))
        {
            return ConsumptionDataTable[ItemUniqueNumber];
        }
        return new ConsumptionData();
    }
    public SkillData GetSkillData(int SkillUniqueNumber)
    {
        if (SkillDataTable.ContainsKey(SkillUniqueNumber))
        {
            return SkillDataTable[SkillUniqueNumber];
        }
        return new SkillData();
    }

    public string MakeItemToolTipText(int ItemUniqueNumber, int Value) //무기의 경우 Value 에 강화수치, 소모,기타아이템의 경우 갯수
    {
        if (ItemUniqueNumber < 0) return null;

        StringBuilder TextMaker = new StringBuilder();
        var ItemDataValue = ItemDataTable[ItemUniqueNumber];

        TextMaker.Append("이름 : ");
        TextMaker.Append(ItemDataValue.Name);

        switch (ItemDataValue.Type)
        {
            case ITEMTYPE.WEAPON:
                {
                    var WeaponDataValue = GetWeaponData(ItemUniqueNumber);
                    WeaponDataValue.SetReinforce(Value);

                    TextMaker.Append("(+");
                    TextMaker.Append(Value.ToString());
                    TextMaker.AppendLine(")");
                    TextMaker.Append("무기타입 : ");
                    TextMaker.AppendLine(WeaponDataValue.WeaponType.ToString());

                    TextMaker.Append("공격력 : ");
                    TextMaker.Append(WeaponDataValue.TotalATK.ToString());
                    TextMaker.Append("(");
                    TextMaker.Append(WeaponDataValue.ATK.ToString());
                    TextMaker.Append("+");
                    TextMaker.Append((WeaponDataValue.TotalATK - WeaponDataValue.ATK).ToString());
                    TextMaker.AppendLine(")");
                    TextMaker.Append("강화수치당 강화비용 증가량 : ");
                    TextMaker.AppendLine(WeaponDataValue.ReinforcePrice.ToString());
                    TextMaker.Append("강화수치당 공격력 증가량 : ");
                    TextMaker.AppendLine(WeaponDataValue.ReinforcePerATK.ToString());
                    TextMaker.Append("가격 : ");
                    TextMaker.AppendLine(ItemDataValue.Price.ToString());
                    TextMaker.Append("설명 : ");
                    TextMaker.AppendLine(ItemDataValue.Discription);
                }
                break;
            case ITEMTYPE.CONSUMPTION:
                {
                    TextMaker.AppendLine();
                    var ConsumptionItemValue = GetConsumptionData(ItemUniqueNumber);
                    TextMaker.Append("효과 : ");
                    switch (ConsumptionItemValue.ItemType)
                    {
                        case CONSUMPTIONTYPE.HP:
                            TextMaker.Append("HP + ");
                            break;
                        case CONSUMPTIONTYPE.MP:
                            TextMaker.Append("MP + ");
                            break;
                        case CONSUMPTIONTYPE.ELIXIR:
                            TextMaker.Append("HP & MP + ");
                            break;
                        case CONSUMPTIONTYPE.EXP:
                            TextMaker.Append("EXP + ");
                            break;
                        case CONSUMPTIONTYPE.WARPCAPSULE:
                            TextMaker.Append("지정된 장소로 이동할 수 있다");
                            break;
                        default:
                            Debug.Log("소모 아이템에 타입값이 없음");
                            return null;
                    }

                    TextMaker.Append(ConsumptionItemValue.Value.ToString());
                    if(ConsumptionItemValue.ItemType == CONSUMPTIONTYPE.ELIXIR)
                        TextMaker.AppendLine("%");
                    else
                        TextMaker.AppendLine();
                    TextMaker.Append("가격 : ");
                    TextMaker.AppendLine(ItemDataValue.Price.ToString());
                    TextMaker.Append("설명 : ");
                    TextMaker.AppendLine(ItemDataValue.Discription.ToString());
                }
                break;
            case ITEMTYPE.ETC:
                {
                    TextMaker.AppendLine();
                    TextMaker.Append("가격 : ");
                    TextMaker.AppendLine(ItemDataValue.Price.ToString());
                    TextMaker.Append("설명 : ");
                    TextMaker.AppendLine(ItemDataValue.Discription.ToString());
                }
                break;
            default :
                Debug.Log("아이템에 타입값이 없음");
                return null;
        }
        return TextMaker.ToString();
    }

    public string MakeSkillTooltipText(int SkillUniqueNumber, int SkillLevel)
    {
        StringBuilder TextMaker = new StringBuilder();
        var SkillData = SkillDataTable[SkillUniqueNumber];
        TextMaker.Append("스킬이름 : ");
        TextMaker.AppendLine(SkillData.SkillName);
        TextMaker.Append("스킬타입 : ");
        TextMaker.AppendLine(SkillData.SkillType.ToString());
        TextMaker.Append("스킬레벨 : ");
        TextMaker.Append(SkillLevel.ToString());
        TextMaker.Append("/");
        TextMaker.AppendLine(SkillData.SkillMaxLevel.ToString());

        switch (SkillData.SkillType)
        {
            case SKILLTYPE.ACTIVE:
                {
                    TextMaker.Append("코스트 : ");
                    TextMaker.AppendLine(SkillData.SkillCost.ToString());
                    TextMaker.Append("쿨타임 : ");
                    TextMaker.AppendLine(SkillData.SkillCoolTime.ToString());
                    TextMaker.Append("지속시간 : ");
                    TextMaker.AppendLine(SkillData.SkillDurationTime.ToString());
                    TextMaker.Append("위력 : ");
                    TextMaker.AppendLine(SkillData.GetSkillTotalValue(SkillLevel).ToString());
                    TextMaker.Append("레벨당 위력상승량 : ");
                    TextMaker.AppendLine(SkillData.SkillLevelPerValue.ToString());
                }
                break;
            case SKILLTYPE.PASSIVE:
                {
                    TextMaker.Append("총상승량 : ");
                    TextMaker.AppendLine(SkillData.GetSkillTotalValue(SkillLevel).ToString());
                    TextMaker.Append("레벨당 스탯 상승량 : ");
                    TextMaker.AppendLine(SkillData.SkillLevelPerValue.ToString());
                }
                break;
            default:
                break;
        }

        TextMaker.Append("설명 : ");
        TextMaker.Append(SkillData.SkillDiscription);
        return TextMaker.ToString();
    }



/*    public void MakeXMLFile() //XML File 만들기
    {
        string name = "CraftingDataTable";
        List<CraftingItem> list = new List<CraftingItem>();
        CraftingItem NewData = new CraftingItem();


        list.Add(NewData);
        list.Add(NewData);

        XML<CraftingItem>.Write(name, list);
    }*/

}
