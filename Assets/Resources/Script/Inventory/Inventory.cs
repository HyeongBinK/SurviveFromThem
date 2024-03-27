using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

[System.Serializable]
public class ItemSlotData
{
    public bool IsData = false;
    public ITEMTYPE ItemType; // 아이템의 타입
    public int ItemUniqueNumber = -1; //아이템의 고유번호
    public int Value = -1; // 장비아이템의 경우 강화수치 소모 기타 아이템의 경우 아이템의 양(갯수)
    public int SlotNumber; //담긴슬롯번호
}
public enum INVENTORYDATA
{
    MAXQUANTITY = 99, //최대아이템적재가능량
    MAXGOLD = 99999999, //최대골드적재가능량
    BASESLOT = 5, //기본슬롯의 갯수
    MAXSLOT = 20, //최대 확장가능한슬롯의 갯수
}

[System.Serializable]
public class Inventory
{
    private int DummyWeaponSlotNumber; //장비가 교체될때 장비가 빠져나간 슬롯의 번호
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

    public void DefaultSetting() //게임 새로시작(초기화)시 인벤토리 초기 셋팅
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

    public void GetGold(int NewGold) //골드획득시 인벤토리내 골드상승
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
                GameManager.Instance.AddNewLog("최대보유가능 골드에 도달하여서 더이상 골드를 획득할수없습니다.");
        }
    }

    public bool UseGold(int Price) //골드사용시 인벤토리내 골드감소
    {
        Price = Mathf.Abs(Price);
        if (Gold >= Price)
        {
            GameManager.Instance.AddNewLog(Price.ToString() + "Gold 사용");
            Gold -= Price;
            //인벤토리 UI내 골드수치갱신 기능
            GameManager.Instance.AddNewLog("남은 Gold :" + Gold.ToString() + "Gold");
            return true;
        }
        GameManager.Instance.AddNewLog("골드가 부족합니다");
        return false;
    }

    public bool AddItem(ITEMTYPE Type, int ItemUniqueNumber, int Value) //아이템 습득
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
                        if (!InventorySlotsData[ITEMTYPE.WEAPON][i].IsData) //빈자리를 찾는다
                        {
                            IsEmptySlot = true;
                            NewItemSlotData.SlotNumber = i;
                            NewItemSlotData.Value = Value; //장비아이템의 경우 Value = 강화수치
                            InventorySlotsData[ITEMTYPE.WEAPON][i] = NewItemSlotData;
                            break;
                        }
                    }
                    if (!IsEmptySlot)
                    {
                        GameManager.Instance.AddNewLog("장비아이템슬롯이 가득 차서 아이템을 습득할 수 없습니다.");
                        return false;
                    }
                }
                break;
            case ITEMTYPE.CONSUMPTION:
                {
                    for (int i = 0; i < InventorySlotsData[ITEMTYPE.CONSUMPTION].Count; i++) //같은종류의 아이템이 있는지 찾는다
                    {
                        if (InventorySlotsData[ITEMTYPE.CONSUMPTION][i].ItemUniqueNumber == -1) //슬롯에 데이터가 없으면 넘어간다
                            continue;

                        if (ItemUniqueNumber == InventorySlotsData[ITEMTYPE.CONSUMPTION][i].ItemUniqueNumber) //새로얻은 아이템과 기존슬롯 아이템의 고유번호가 같으면?
                        {
                            if (InventorySlotsData[ITEMTYPE.CONSUMPTION][i].Value >= (int)INVENTORYDATA.MAXQUANTITY) //가득차있는경우 넘어간다
                                continue;

                            int ResultQuantity = InventorySlotsData[ITEMTYPE.CONSUMPTION][i].Value + Value;
                            if (ResultQuantity <= (int)INVENTORYDATA.MAXQUANTITY) //기존의 아이템의 갯수와 새로 더해진 갯수가 최대적재량보다 적거나 같을때
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
                            else // 합계치가 최대적재량을 넘어갈경우
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
                        GameManager.Instance.AddNewLog("소모아이템슬롯이 가득 차서 아이템을 습득할 수 없습니다.");
                        return false;
                    }
                }
                break;
            case ITEMTYPE.ETC:
                {
                    for (int i = 0; i < InventorySlotsData[ITEMTYPE.ETC].Count; i++) //같은종류의 아이템이 있는지 찾는다
                    {
                        if (InventorySlotsData[ITEMTYPE.ETC][i].SlotNumber == -1) continue;

                        if (ItemUniqueNumber == InventorySlotsData[ITEMTYPE.ETC][i].ItemUniqueNumber) //새로얻은 아이템과 기존슬롯 아이템의 고유번호가 같으면?
                        {
                            if (InventorySlotsData[ITEMTYPE.ETC][i].Value >= (int)INVENTORYDATA.MAXQUANTITY) // 가득차있는경우 넘어간다
                                continue;

                            int ResultQuantity = InventorySlotsData[ITEMTYPE.ETC][i].Value + Value;
                            if (ResultQuantity <= (int)INVENTORYDATA.MAXQUANTITY) //기존의 아이템의 갯수와 새로 더해진 갯수가 최대적재량보다 적거나 같을때
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
                            else // 합계치가 최대적재량을 넘어갈경우
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
                        GameManager.Instance.AddNewLog("기타아이템슬롯이 가득 차서 아이템을 습득할 수 없습니다.");
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

    public bool AddItemWithSlot(ITEMTYPE Type, int ItemUniqueNumber, int Value, int SlotNumber) // 슬롯 번호를 지정해서 해당 슬롯번호에 아이템 추가(교환이 아님)
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
            case ITEMTYPE.WEAPON: // 장비아이템일 경우 장비해체, 창고기능(구현예정없음)에 사용
                {
                    if (InventorySlotsData[ITEMTYPE.WEAPON][SlotNumber].IsData) //옮길려는 슬롯에 데이터가 있으면 
                        return false;

                    NewItemSlotData.Value = Value; //장비아이템의 경우 Value = 강화수치
                    InventorySlotsData[ITEMTYPE.WEAPON][SlotNumber] = NewItemSlotData;
                }
                break;
            case ITEMTYPE.CONSUMPTION:
                {
                    if (InventorySlotsData[ITEMTYPE.CONSUMPTION][SlotNumber].IsData) //옮길려는 슬롯에 데이터가 있으면 
                    {
                        if (InventorySlotsData[ITEMTYPE.CONSUMPTION][SlotNumber].ItemUniqueNumber == ItemUniqueNumber) //옮길려는 아이템과 기존위치의 아이템이 같으면
                        {
                            if (InventorySlotsData[ITEMTYPE.CONSUMPTION][SlotNumber].Value >= (int)INVENTORYDATA.MAXQUANTITY) //가득차있는경우 종료
                                return false;

                            int ResultQuantity = InventorySlotsData[ITEMTYPE.CONSUMPTION][SlotNumber].Value + Value;

                            if (ResultQuantity > (int)INVENTORYDATA.MAXQUANTITY) // 합계치가 최대적재량을 넘어갈경우(아예 이러한 경우가 없게끔 처리를 할거임)
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
                    if (InventorySlotsData[ITEMTYPE.ETC][SlotNumber].IsData) //옮길려는 슬롯에 데이터가 있으면 
                    {
                        if (InventorySlotsData[ITEMTYPE.ETC][SlotNumber].ItemUniqueNumber == ItemUniqueNumber) //옮길려는 아이템과 기존위치의 아이템이 같으면
                        {
                            if (InventorySlotsData[ITEMTYPE.ETC][SlotNumber].Value >= (int)INVENTORYDATA.MAXQUANTITY) //가득차있는경우 종료
                                return false;

                            int ResultQuantity = InventorySlotsData[ITEMTYPE.ETC][SlotNumber].Value + Value;

                            if (ResultQuantity > (int)INVENTORYDATA.MAXQUANTITY) // 합계치가 최대적재량을 넘어갈경우(아예 이러한 경우가 없게끔 처리를 할거임)
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
        // UI갱신
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
                GameManager.Instance.AddNewLog("가지고있는 수치보다 더많이 사용/판매 할수 없습니다");
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
    //슬롯이동/교환기능
    public bool SwitchSlotData(ITEMTYPE Type, int Slot1, int Slot2) //Slot1 : 드래그한 슬롯의 슬롯넘버, Slot2 : 대상슬롯넘버, 빈슬롯은 드래그하지못하게 설계
    {
        if (Type == ITEMTYPE.NULL) return false;
        if (Slot1 >= InventorySlotsData[Type].Count || Slot2 >= InventorySlotsData[Type].Count) return false; //아직 열리지않은 슬롯에 접근하면 종료  
        if (!InventorySlotsData[Type][Slot1].IsData) return false; //데이터가 없으면 종료
        
        if(!InventorySlotsData[Type][Slot2].IsData) //대상 슬롯에 데이터가 없는경우
        {
            InventorySlotsData[Type][Slot2] = InventorySlotsData[Type][Slot1]; //데이터 옮기고
            InventorySlotsData[Type][Slot1] = new ItemSlotData(); //기존 슬롯은 클리어
        }
        else //데이터가 있어서 Swap 
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
              Debug.LogError("플레이어 인벤토리 세이브파일을 찾지못했습니다.");
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
