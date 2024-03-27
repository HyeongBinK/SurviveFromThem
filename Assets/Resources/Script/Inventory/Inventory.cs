using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

[System.Serializable]
public class ItemSlotData
{
    public bool IsData = false;
    public ITEMTYPE ItemType; // �������� Ÿ��
    public int ItemUniqueNumber = -1; //�������� ������ȣ
    public int Value = -1; // ���������� ��� ��ȭ��ġ �Ҹ� ��Ÿ �������� ��� �������� ��(����)
    public int SlotNumber; //��佽�Թ�ȣ
}
public enum INVENTORYDATA
{
    MAXQUANTITY = 99, //�ִ���������簡�ɷ�
    MAXGOLD = 99999999, //�ִ������簡�ɷ�
    BASESLOT = 5, //�⺻������ ����
    MAXSLOT = 20, //�ִ� Ȯ�尡���ѽ����� ����
}

[System.Serializable]
public class Inventory
{
    private int DummyWeaponSlotNumber; //��� ��ü�ɶ� ��� �������� ������ ��ȣ
    private Dictionary<ITEMTYPE, List<ItemSlotData>> InventorySlotsData = new Dictionary<ITEMTYPE, List<ItemSlotData>>();
    public ItemSlotData GetInventorySlotData(ITEMTYPE Type, int SlotNumber) 
    {
        if (Type == ITEMTYPE.NULL) return new ItemSlotData();
        if (SlotNumber < 0 || SlotNumber >= InventorySlotsData[Type].Count) return new ItemSlotData();
        if (InventorySlotsData[Type].Count <= 0) return new ItemSlotData();

        if(InventorySlotsData[Type][SlotNumber].IsData)
        return InventorySlotsData[Type][SlotNumber];

        return new ItemSlotData();
    }
    public int GetActivatedInventorySlotCount(ITEMTYPE Type)
    {
        if (InventorySlotsData.ContainsKey(Type))
            return InventorySlotsData[Type].Count;
        else return 0;
    }

    public int Gold { get; private set; }

    public void DefaultSetting() //���� ���ν���(�ʱ�ȭ)�� �κ��丮 �ʱ� ����
    {
        Gold = 0;
        UIManager.Instance.SetInventoryGold(Gold);
        OpenSlot(ITEMTYPE.WEAPON, (int)INVENTORYDATA.BASESLOT);
        OpenSlot(ITEMTYPE.CONSUMPTION, (int)INVENTORYDATA.BASESLOT);
        OpenSlot(ITEMTYPE.ETC, (int)INVENTORYDATA.BASESLOT);
    }

    public void OpenSlot(ITEMTYPE Type, int NewSlot)
    {
        if (Type == ITEMTYPE.NULL) return;

        if (!InventorySlotsData.ContainsKey(Type))
            InventorySlotsData.Add(Type, new List<ItemSlotData>());

        if (InventorySlotsData[Type].Count >= (int)INVENTORYDATA.MAXSLOT) return;
        int NumberOfSlot = Mathf.Clamp(InventorySlotsData[Type].Count + NewSlot, 0, (int)INVENTORYDATA.MAXSLOT);
        if(NumberOfSlot > InventorySlotsData[Type].Count)
        {
            int NewNumberOfSlot = NumberOfSlot - InventorySlotsData[Type].Count;

            for (int i = 0; i < NewNumberOfSlot; i++)
            {
                InventorySlotsData[Type].Add(new ItemSlotData());
            }
        }
    }

    public void GetGold(int NewGold) //���ȹ��� �κ��丮�� �����
    {
        if (GameManager.Instance)
        {
            bool IsMaxGold = false;
            StringBuilder SB = new StringBuilder();
            int result = (int)INVENTORYDATA.MAXGOLD - Gold;
            if (NewGold > result)
            {
                IsMaxGold = true;
                NewGold = result;
            }
            Gold = Mathf.Clamp(Gold + NewGold, 0, (int)INVENTORYDATA.MAXGOLD);
  
            SB.Append("+");
            SB.Append(NewGold.ToString());
            SB.Append("Gold");
            GameManager.Instance.AddNewLog(SB.ToString());
            UIManager.Instance.SetInventoryGold(Gold);

            if(IsMaxGold)
                GameManager.Instance.AddNewLog("�ִ뺸������ ��忡 �����Ͽ��� ���̻� ��带 ȹ���Ҽ������ϴ�.");
        }
    }

    public bool UseGold(int Price) //������ �κ��丮�� ��尨��
    {
        Price = Mathf.Abs(Price);
        if (Gold >= Price)
        {
            GameManager.Instance.AddNewLog(Price.ToString() + "Gold ���");
            Gold -= Price;
            //�κ��丮 UI�� ����ġ���� ���
            GameManager.Instance.AddNewLog("���� Gold :" + Gold.ToString() + "Gold");
            return true;
        }
        GameManager.Instance.AddNewLog("��尡 �����մϴ�");
        return false;
    }

    public bool AddItem(ITEMTYPE Type, int ItemUniqueNumber, int Value) //������ ����
    {
        if (Type == ITEMTYPE.NULL) return false;
        if (ItemUniqueNumber < 0) return false;
        if (Value < 0) return false;

        bool IsEmptySlot = false;
        ItemSlotData NewItemSlotData = new ItemSlotData();
        NewItemSlotData.ItemType = Type;
        NewItemSlotData.ItemUniqueNumber = ItemUniqueNumber;
        NewItemSlotData.IsData = true;

        switch (Type)
        {
            case ITEMTYPE.WEAPON:
                {
                    for (int i = 0; i < InventorySlotsData[ITEMTYPE.WEAPON].Count; i++)
                    {
                        if (!InventorySlotsData[ITEMTYPE.WEAPON][i].IsData) //���ڸ��� ã�´�
                        {
                            IsEmptySlot = true;
                            NewItemSlotData.SlotNumber = i;
                            NewItemSlotData.Value = Value; //���������� ��� Value = ��ȭ��ġ
                            InventorySlotsData[ITEMTYPE.WEAPON][i] = NewItemSlotData;
                            break;
                        }
                    }
                    if (!IsEmptySlot)
                    {
                        GameManager.Instance.AddNewLog("�������۽����� ���� ���� �������� ������ �� �����ϴ�.");
                        return false;
                    }
                }
                break;
            case ITEMTYPE.CONSUMPTION:
                {
                    for (int i = 0; i < InventorySlotsData[ITEMTYPE.CONSUMPTION].Count; i++) //���������� �������� �ִ��� ã�´�
                    {
                        if (InventorySlotsData[ITEMTYPE.CONSUMPTION][i].ItemUniqueNumber == -1) //���Կ� �����Ͱ� ������ �Ѿ��
                            continue;

                        if (ItemUniqueNumber == InventorySlotsData[ITEMTYPE.CONSUMPTION][i].ItemUniqueNumber) //���ξ��� �����۰� �������� �������� ������ȣ�� ������?
                        {
                            if (InventorySlotsData[ITEMTYPE.CONSUMPTION][i].Value >= (int)INVENTORYDATA.MAXQUANTITY) //�������ִ°�� �Ѿ��
                                continue;

                            int ResultQuantity = InventorySlotsData[ITEMTYPE.CONSUMPTION][i].Value + Value;
                            if (ResultQuantity <= (int)INVENTORYDATA.MAXQUANTITY) //������ �������� ������ ���� ������ ������ �ִ����緮���� ���ų� ������
                            {
                                InventorySlotsData[ITEMTYPE.CONSUMPTION][i].Value = ResultQuantity;
                                if (UIManager.Instance)
                                {
                                    if (UIManager.Instance.GetInventoryWindow.gameObject.activeSelf)
                                    {
                                        UIManager.Instance.GetInventoryWindow.RefreshSlotDatas();
                                    }
                                }
                                return true;
                            }
                            else // �հ�ġ�� �ִ����緮�� �Ѿ���
                            {
                                int NewQuantity = ResultQuantity - (int)INVENTORYDATA.MAXQUANTITY;
                                InventorySlotsData[ITEMTYPE.CONSUMPTION][i].Value = (int)INVENTORYDATA.MAXQUANTITY;
                                Value = NewQuantity;
                            }
                        }
                    }

                    for (int i = 0; i < InventorySlotsData[ITEMTYPE.CONSUMPTION].Count; i++)
                    {
                        if (!InventorySlotsData[ITEMTYPE.CONSUMPTION][i].IsData)
                        {
                            IsEmptySlot = true;
                            NewItemSlotData.SlotNumber = i;
                            NewItemSlotData.Value = Value;
                            InventorySlotsData[ITEMTYPE.CONSUMPTION][i] = NewItemSlotData;
                            break;
                        }
                    }
                    if (!IsEmptySlot)
                    {
                        GameManager.Instance.AddNewLog("�Ҹ�����۽����� ���� ���� �������� ������ �� �����ϴ�.");
                        return false;
                    }
                }
                break;
            case ITEMTYPE.ETC:
                {
                    for (int i = 0; i < InventorySlotsData[ITEMTYPE.ETC].Count; i++) //���������� �������� �ִ��� ã�´�
                    {
                        if (InventorySlotsData[ITEMTYPE.ETC][i].SlotNumber == -1) continue;

                        if (ItemUniqueNumber == InventorySlotsData[ITEMTYPE.ETC][i].ItemUniqueNumber) //���ξ��� �����۰� �������� �������� ������ȣ�� ������?
                        {
                            if (InventorySlotsData[ITEMTYPE.ETC][i].Value >= (int)INVENTORYDATA.MAXQUANTITY) // �������ִ°�� �Ѿ��
                                continue;

                            int ResultQuantity = InventorySlotsData[ITEMTYPE.ETC][i].Value + Value;
                            if (ResultQuantity <= (int)INVENTORYDATA.MAXQUANTITY) //������ �������� ������ ���� ������ ������ �ִ����緮���� ���ų� ������
                            {
                                InventorySlotsData[ITEMTYPE.ETC][i].Value = ResultQuantity;
                                if (UIManager.Instance)
                                {
                                    if (UIManager.Instance.GetInventoryWindow.gameObject.activeSelf)
                                    {
                                        UIManager.Instance.GetInventoryWindow.RefreshSlotDatas();
                                    }
                                }
                                return true;
                            }
                            else // �հ�ġ�� �ִ����緮�� �Ѿ���
                            {
                                int NewQuantity = ResultQuantity - (int)INVENTORYDATA.MAXQUANTITY;
                                InventorySlotsData[ITEMTYPE.ETC][i].Value = (int)INVENTORYDATA.MAXQUANTITY;
                                Value = NewQuantity;
                            }
                        }
                    }
                    for (int i = 0; i < InventorySlotsData[ITEMTYPE.ETC].Count; i++)
                    {
                        if (!InventorySlotsData[ITEMTYPE.ETC][i].IsData)
                        {
                            IsEmptySlot = true;
                            NewItemSlotData.SlotNumber = i;
                            NewItemSlotData.Value = Value;
                            InventorySlotsData[ITEMTYPE.ETC][i] = NewItemSlotData;
                            break;
                        }
                    }
                    if(!IsEmptySlot)
                    {
                        GameManager.Instance.AddNewLog("��Ÿ�����۽����� ���� ���� �������� ������ �� �����ϴ�.");
                        return false;
                    }
                }
                break;
            default:
                Debug.Log("CriticalError - Inventory-AddItem");
                return false;
        }

        if(UIManager.Instance)
        {
            if(UIManager.Instance.GetInventoryWindow.gameObject.activeSelf)
                UIManager.Instance.GetInventoryWindow.RefreshSlotDatas(); 
        }
        return true;
    }

    public bool AddItemWithSlot(ITEMTYPE Type, int ItemUniqueNumber, int Value, int SlotNumber) // ���� ��ȣ�� �����ؼ� �ش� ���Թ�ȣ�� ������ �߰�(��ȯ�� �ƴ�)
    {
        if (Type == ITEMTYPE.NULL) return false;
        if (ItemUniqueNumber < 0) return false;
        if (Value < 0) return false;

        ItemSlotData NewItemSlotData = new ItemSlotData();
        NewItemSlotData.ItemType = Type;
        NewItemSlotData.ItemUniqueNumber = ItemUniqueNumber;
        NewItemSlotData.IsData = true;
        NewItemSlotData.SlotNumber = SlotNumber;

        switch (Type)
        {
            case ITEMTYPE.WEAPON: // ���������� ��� �����ü, â����(������������)�� ���
                {
                    if (InventorySlotsData[ITEMTYPE.WEAPON][SlotNumber].IsData) //�ű���� ���Կ� �����Ͱ� ������ 
                        return false;

                    NewItemSlotData.Value = Value; //���������� ��� Value = ��ȭ��ġ
                    InventorySlotsData[ITEMTYPE.WEAPON][SlotNumber] = NewItemSlotData;
                }
                break;
            case ITEMTYPE.CONSUMPTION:
                {
                    if (InventorySlotsData[ITEMTYPE.CONSUMPTION][SlotNumber].IsData) //�ű���� ���Կ� �����Ͱ� ������ 
                    {
                        if (InventorySlotsData[ITEMTYPE.CONSUMPTION][SlotNumber].ItemUniqueNumber == ItemUniqueNumber) //�ű���� �����۰� ������ġ�� �������� ������
                        {
                            if (InventorySlotsData[ITEMTYPE.CONSUMPTION][SlotNumber].Value >= (int)INVENTORYDATA.MAXQUANTITY) //�������ִ°�� ����
                                return false;

                            int ResultQuantity = InventorySlotsData[ITEMTYPE.CONSUMPTION][SlotNumber].Value + Value;

                            if (ResultQuantity > (int)INVENTORYDATA.MAXQUANTITY) // �հ�ġ�� �ִ����緮�� �Ѿ���(�ƿ� �̷��� ��찡 ���Բ� ó���� �Ұ���)
                                return false;

                            InventorySlotsData[ITEMTYPE.CONSUMPTION][SlotNumber].Value = ResultQuantity; 
                        }
                    }
                    else
                    {
                        NewItemSlotData.Value = Value;
                        InventorySlotsData[ITEMTYPE.CONSUMPTION][SlotNumber] = NewItemSlotData;
                    }
                }
                break;
            case ITEMTYPE.ETC:
                {
                    if (InventorySlotsData[ITEMTYPE.ETC][SlotNumber].IsData) //�ű���� ���Կ� �����Ͱ� ������ 
                    {
                        if (InventorySlotsData[ITEMTYPE.ETC][SlotNumber].ItemUniqueNumber == ItemUniqueNumber) //�ű���� �����۰� ������ġ�� �������� ������
                        {
                            if (InventorySlotsData[ITEMTYPE.ETC][SlotNumber].Value >= (int)INVENTORYDATA.MAXQUANTITY) //�������ִ°�� ����
                                return false;

                            int ResultQuantity = InventorySlotsData[ITEMTYPE.ETC][SlotNumber].Value + Value;

                            if (ResultQuantity > (int)INVENTORYDATA.MAXQUANTITY) // �հ�ġ�� �ִ����緮�� �Ѿ���(�ƿ� �̷��� ��찡 ���Բ� ó���� �Ұ���)
                                return false;

                            InventorySlotsData[ITEMTYPE.ETC][SlotNumber].Value = ResultQuantity;
                        }
                    }
                    else
                    {
                        NewItemSlotData.Value = Value;
                        InventorySlotsData[ITEMTYPE.ETC][SlotNumber] = NewItemSlotData;
                    }
                }
                break;
            default:
                Debug.Log("CriticalError - Inventory-AddItem");
                return false;
        }

        if (UIManager.Instance)
        {
            if (UIManager.Instance.GetInventoryWindow.gameObject.activeSelf)
                UIManager.Instance.GetInventoryWindow.RefreshSlotDatas();
        }
        return true;
    }
    public void ClearSlotData(ITEMTYPE Type, int SlotNumber)
    {
        if (Type == ITEMTYPE.NULL) return;
        if (SlotNumber < 0) return;

        InventorySlotsData[Type][SlotNumber] = new ItemSlotData();
        // UI����
        if (UIManager.Instance)
        {
            UIManager.Instance.GetInventoryWindow.RefreshSlotDatas();
            UIManager.Instance.GetQuickSlots.RefreshQuickSlot();
        }
    }

    public bool DecreaseItemQuantity(ITEMTYPE Type, int SlotNumber, int NumberToDecrease)
    {
        if (Type == ITEMTYPE.NULL) return false;
        if (SlotNumber < 0) return false;
        if (NumberToDecrease <= 0) return false;
        if (!InventorySlotsData[Type][SlotNumber].IsData) return false;

        if (Type == ITEMTYPE.WEAPON)
        {
            ClearSlotData(Type, SlotNumber);
            return true;
        }

        int SlotQuantity = InventorySlotsData[Type][SlotNumber].Value;
        if (SlotQuantity > 0)
        {
            if (SlotQuantity < NumberToDecrease)
            {
                GameManager.Instance.AddNewLog("�������ִ� ��ġ���� ������ ���/�Ǹ� �Ҽ� �����ϴ�");
                return false;
            }

            InventorySlotsData[Type][SlotNumber].Value = Mathf.Clamp(SlotQuantity - NumberToDecrease, 0, (int)INVENTORYDATA.MAXQUANTITY);
            if (UIManager.Instance)
            {
                UIManager.Instance.GetInventoryWindow.RefreshConsumptionSlotsData();
                UIManager.Instance.GetQuickSlots.RefreshQuickSlot();
            }
        }

        if (InventorySlotsData[Type][SlotNumber].Value <= 0)
            ClearSlotData(Type, SlotNumber);
        return true;
    }
    //�����̵�/��ȯ���
    public bool SwitchSlotData(ITEMTYPE Type, int Slot1, int Slot2) //Slot1 : �巡���� ������ ���Գѹ�, Slot2 : ��󽽷Գѹ�, �󽽷��� �巡���������ϰ� ����
    {
        if (Type == ITEMTYPE.NULL) return false;
        if (Slot1 >= InventorySlotsData[Type].Count || Slot2 >= InventorySlotsData[Type].Count) return false; //���� ���������� ���Կ� �����ϸ� ����  
        if (!InventorySlotsData[Type][Slot1].IsData) return false; //�����Ͱ� ������ ����
        
        if(!InventorySlotsData[Type][Slot2].IsData) //��� ���Կ� �����Ͱ� ���°��
        {
            InventorySlotsData[Type][Slot2] = InventorySlotsData[Type][Slot1]; //������ �ű��
            InventorySlotsData[Type][Slot1] = new ItemSlotData(); //���� ������ Ŭ����
        }
        else //�����Ͱ� �־ Swap 
        {
            ItemSlotData TempData = InventorySlotsData[Type][Slot2];
            InventorySlotsData[Type][Slot2] = InventorySlotsData[Type][Slot1];
            InventorySlotsData[Type][Slot1] = TempData;
        }
        if (UIManager.Instance)
        {
            UIManager.Instance.GetQuickSlots.ChangeOriginQuickSlotNumber((DATATYPE)Type, Slot1, Slot2);

            if (UIManager.Instance.GetInventoryWindow.gameObject.activeSelf)
                UIManager.Instance.GetInventoryWindow.RefreshSlotDatas();
        }
        return true;
    }

    /*  public bool LoadData()
      {
          string FilePath = DataSaveAndLoad.Instance.MakeFilePath("InventoryData", "/SaveData/InventoryData/");
          if (!File.Exists(FilePath))
          {
              Debug.LogError("�÷��̾� �κ��丮 ���̺������� ã�����߽��ϴ�.");
              return false;
          }
          string SaveFile = File.ReadAllText(FilePath);
          InventorySaveData SaveData = JsonUtility.FromJson<InventorySaveData>(SaveFile);

          if (SlotDatas.Count != 0 || EquipmentDatas.Count != 0)
          {
              for (int i = 0; i < IsSlotDatas.Count; i++)
              {
                  ClearSlotData(i);
              }
          }

          Gold = SaveData.GoldData;

          UpgradeSlotNumber(SaveData.InventoryActiveSlotNumberData - GetInventorySlotNumber);

          if (SaveData.EquipmentsSaveData != null)
          {
              var RegionEquipmentDatas = SaveData.EquipmentsSaveData;
              for (int i = 0; i < RegionEquipmentDatas.Count; i++)
              {
                  EquipmentDatas.Add(RegionEquipmentDatas[i].SlotNumber, RegionEquipmentDatas[i]);
              }
          }

          if (SaveData.SlotItemSaveData != null)
          {
              var RegionSlotItemDatas = SaveData.SlotItemSaveData;
              for (int i = 0; i < RegionSlotItemDatas.Count; i++)
              {
                 // if (RegionSlotItemDatas[i].SlotNumber) ;
                  SlotDatas.Add(RegionSlotItemDatas[i].SlotNumber, RegionSlotItemDatas[i]);
                  IsSlotDatas[RegionSlotItemDatas[i].SlotNumber] = true;
              }
          }

          for (int i = 0; i < IsSlotDatas.Count; i++)
          {
              UpdateSlotUI(i);
          }

          return true;
      }*/
}
