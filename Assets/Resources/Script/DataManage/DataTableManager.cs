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

    private Dictionary<int, MonsterStatus> MonsterDataTable = new Dictionary<int, MonsterStatus>(); //Key : ���Ͱ�����ȣ, Value : ���ͽ������ͽ�
    private Dictionary<int, ItemData> ItemDataTable = new Dictionary<int, ItemData>(); //Key : �����۰�����ȣ, Value : �����۵�����
    private Dictionary<int, WeaponData> WeaponDataTable = new Dictionary<int, WeaponData>(); //Key : �ѱ������ȣ, Value : �ѱⵥ����
    private Dictionary<int, ConsumptionData> ConsumptionDataTable = new Dictionary<int, ConsumptionData>(); //Key : �ѱ������ȣ, Value : �ѱⵥ����
    private Dictionary<int, SkillData> SkillDataTable = new Dictionary<int, SkillData>(); //Key : ��ų������ȣ, Value : ��ų������

    private void Awake()
    {
        // ���� �̱��� ������Ʈ�� �� �ٸ� GameManager ������Ʈ�� �ִٸ�
        if (Instance != this)
        {
            // �ڽ��� �ı�
            Destroy(gameObject);
        }
    }

    private void LoadMonsterDataTable() //���� ���������̺��� ���͵����͵� �ҷ�����
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

    public string MakeItemToolTipText(int ItemUniqueNumber, int Value) //������ ��� Value �� ��ȭ��ġ, �Ҹ�,��Ÿ�������� ��� ����
    {
        if (ItemUniqueNumber < 0) return null;

        StringBuilder TextMaker = new StringBuilder();
        var ItemDataValue = ItemDataTable[ItemUniqueNumber];

        TextMaker.Append("�̸� : ");
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
                    TextMaker.Append("����Ÿ�� : ");
                    TextMaker.AppendLine(WeaponDataValue.WeaponType.ToString());

                    TextMaker.Append("���ݷ� : ");
                    TextMaker.Append(WeaponDataValue.TotalATK.ToString());
                    TextMaker.Append("(");
                    TextMaker.Append(WeaponDataValue.ATK.ToString());
                    TextMaker.Append("+");
                    TextMaker.Append((WeaponDataValue.TotalATK - WeaponDataValue.ATK).ToString());
                    TextMaker.AppendLine(")");
                    TextMaker.Append("��ȭ��ġ�� ��ȭ��� ������ : ");
                    TextMaker.AppendLine(WeaponDataValue.ReinforcePrice.ToString());
                    TextMaker.Append("��ȭ��ġ�� ���ݷ� ������ : ");
                    TextMaker.AppendLine(WeaponDataValue.ReinforcePerATK.ToString());
                    TextMaker.Append("���� : ");
                    TextMaker.AppendLine(ItemDataValue.Price.ToString());
                    TextMaker.Append("���� : ");
                    TextMaker.AppendLine(ItemDataValue.Discription);
                }
                break;
            case ITEMTYPE.CONSUMPTION:
                {
                    TextMaker.AppendLine();
                    var ConsumptionItemValue = GetConsumptionData(ItemUniqueNumber);
                    TextMaker.Append("ȿ�� : ");
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
                            TextMaker.Append("������ ��ҷ� �̵��� �� �ִ�");
                            break;
                        default:
                            Debug.Log("�Ҹ� �����ۿ� Ÿ�԰��� ����");
                            return null;
                    }

                    TextMaker.Append(ConsumptionItemValue.Value.ToString());
                    if(ConsumptionItemValue.ItemType == CONSUMPTIONTYPE.ELIXIR)
                        TextMaker.AppendLine("%");
                    else
                        TextMaker.AppendLine();
                    TextMaker.Append("���� : ");
                    TextMaker.AppendLine(ItemDataValue.Price.ToString());
                    TextMaker.Append("���� : ");
                    TextMaker.AppendLine(ItemDataValue.Discription.ToString());
                }
                break;
            case ITEMTYPE.ETC:
                {
                    TextMaker.AppendLine();
                    TextMaker.Append("���� : ");
                    TextMaker.AppendLine(ItemDataValue.Price.ToString());
                    TextMaker.Append("���� : ");
                    TextMaker.AppendLine(ItemDataValue.Discription.ToString());
                }
                break;
            default :
                Debug.Log("�����ۿ� Ÿ�԰��� ����");
                return null;
        }
        return TextMaker.ToString();
    }

    public string MakeSkillTooltipText(int SkillUniqueNumber, int SkillLevel)
    {
        StringBuilder TextMaker = new StringBuilder();
        var SkillData = SkillDataTable[SkillUniqueNumber];
        TextMaker.Append("��ų�̸� : ");
        TextMaker.AppendLine(SkillData.SkillName);
        TextMaker.Append("��ųŸ�� : ");
        TextMaker.AppendLine(SkillData.SkillType.ToString());
        TextMaker.Append("��ų���� : ");
        TextMaker.Append(SkillLevel.ToString());
        TextMaker.Append("/");
        TextMaker.AppendLine(SkillData.SkillMaxLevel.ToString());

        switch (SkillData.SkillType)
        {
            case SKILLTYPE.ACTIVE:
                {
                    TextMaker.Append("�ڽ�Ʈ : ");
                    TextMaker.AppendLine(SkillData.SkillCost.ToString());
                    TextMaker.Append("��Ÿ�� : ");
                    TextMaker.AppendLine(SkillData.SkillCoolTime.ToString());
                    TextMaker.Append("���ӽð� : ");
                    TextMaker.AppendLine(SkillData.SkillDurationTime.ToString());
                    TextMaker.Append("���� : ");
                    TextMaker.AppendLine(SkillData.GetSkillTotalValue(SkillLevel).ToString());
                    TextMaker.Append("������ ���»�·� : ");
                    TextMaker.AppendLine(SkillData.SkillLevelPerValue.ToString());
                }
                break;
            case SKILLTYPE.PASSIVE:
                {
                    TextMaker.Append("�ѻ�·� : ");
                    TextMaker.AppendLine(SkillData.GetSkillTotalValue(SkillLevel).ToString());
                    TextMaker.Append("������ ���� ��·� : ");
                    TextMaker.AppendLine(SkillData.SkillLevelPerValue.ToString());
                }
                break;
            default:
                break;
        }

        TextMaker.Append("���� : ");
        TextMaker.Append(SkillData.SkillDiscription);
        return TextMaker.ToString();
    }



/*    public void MakeXMLFile() //XML File �����
    {
        string name = "CraftingDataTable";
        List<CraftingItem> list = new List<CraftingItem>();
        CraftingItem NewData = new CraftingItem();


        list.Add(NewData);
        list.Add(NewData);

        XML<CraftingItem>.Write(name, list);
    }*/

}
