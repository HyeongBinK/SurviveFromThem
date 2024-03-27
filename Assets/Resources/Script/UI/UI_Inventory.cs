using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public enum INVENTORYUI_BUTTON
{
    WEAPON,
    CONSUMPTION,
    ETC
}

public class UI_Inventory : MonoBehaviour
{
    [SerializeField] private Button m_WeaponButton; //장비아이템버튼
    [SerializeField] private Button m_ConsumptionButton; //소모아이템버튼
    [SerializeField] private Button m_ETCButton; //기타아이템버튼
    [SerializeField] private Text m_GoldText; //보유하고있는 금액을 보여줄 Text
    [SerializeField] private Slot m_SlotPrefab; //생성될 슬롯의 프리팹
    [SerializeField] private Transform m_WeaponSlotsPosition; //장비아이템 슬롯의 위치
    [SerializeField] private Transform m_ConsumptionSlotsPosition; //소모아이템 슬롯의 위치
    [SerializeField] private Transform m_ETCSlotsPosition; //기타아이템 슬롯의 위치
    [SerializeField] private List<Slot> m_WeaponSlots; //장비아이템 슬롯
    [SerializeField] private List<Slot> m_ConsumptionSlots; //소모아이템 슬롯
    [SerializeField] private List<Slot> m_ETCSlots; //기타아이템슬롯
    public INVENTORYUI_BUTTON OpendSlotsType { get; private set; } 

    private void Awake()
    {
        MakeInventorySlots();
        m_WeaponButton.onClick.AddListener(ActiveWeaponSlots);
        m_ConsumptionButton.onClick.AddListener(ActiveConsumptionSlots);
        m_ETCButton.onClick.AddListener(ActiveETCSlots);
        ActiveWeaponSlots();
    }
    private void OnEnable()
    {
        RefreshSlotDatas();
    }
    public void SetGoldText(int NewGold)
    {
        StringBuilder SB = new StringBuilder();
        SB.Append(NewGold);
        SB.Append(" Gold");
        m_GoldText.text = SB.ToString();
    }

    public void MakeInventorySlots()
    {
        for(int i = 0; i < (int)INVENTORYDATA.MAXSLOT; i++)
        {
            MakeEquipmentSlot(i);
            MakeConsumptionSlot(i);
            MakeETCSlot(i);
        }
    }
    private void MakeEquipmentSlot(int SlotNumber)
    {
        Slot NewSlot = Instantiate(m_SlotPrefab, m_WeaponSlotsPosition);
        NewSlot.name = SlotNumber.ToString();
        NewSlot.Init(ITEMTYPE.WEAPON, SlotNumber);
        m_WeaponSlots.Add(NewSlot);
    }
    private void MakeConsumptionSlot(int SlotNumber)
    {
        Slot NewSlot = Instantiate(m_SlotPrefab, m_ConsumptionSlotsPosition);
        NewSlot.name = SlotNumber.ToString();
        NewSlot.Init(ITEMTYPE.CONSUMPTION, SlotNumber);
        m_ConsumptionSlots.Add(NewSlot);
    }
    private void MakeETCSlot(int SlotNumber)
    {
        Slot NewSlot = Instantiate(m_SlotPrefab, m_ETCSlotsPosition);
        NewSlot.name = SlotNumber.ToString();
        NewSlot.Init(ITEMTYPE.ETC, SlotNumber);
        m_ETCSlots.Add(NewSlot);
    }

    private void ActiveWeaponSlots()
    {
        m_ConsumptionSlotsPosition.gameObject.SetActive(false);
        m_ETCSlotsPosition.gameObject.SetActive(false);
        m_WeaponSlotsPosition.gameObject.SetActive(true);
        OpendSlotsType = INVENTORYUI_BUTTON.WEAPON;
        RefreshWeaponSlotsData();
        SetButtonColor();
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("MouseClick");
    }
    private void ActiveConsumptionSlots()
    {
        m_WeaponSlotsPosition.gameObject.SetActive(false);
        m_ETCSlotsPosition.gameObject.SetActive(false);
        m_ConsumptionSlotsPosition.gameObject.SetActive(true);
        OpendSlotsType = INVENTORYUI_BUTTON.CONSUMPTION;
        RefreshConsumptionSlotsData();
        SetButtonColor();
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("MouseClick");
    }
    private void ActiveETCSlots()
    {
        m_WeaponSlotsPosition.gameObject.SetActive(false);
        m_ConsumptionSlotsPosition.gameObject.SetActive(false);
        m_ETCSlotsPosition.gameObject.SetActive(true);
        OpendSlotsType = INVENTORYUI_BUTTON.ETC;
        RefreshETCSlotsData();
        SetButtonColor();
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("MouseClick");
    }

    public void RefreshWeaponSlotsData()
    {
        foreach(Slot MySlot in m_WeaponSlots)
        {
            MySlot.SetSlotData();
        }
    }
    public void RefreshConsumptionSlotsData()
    {
        foreach (Slot MySlot in m_ConsumptionSlots)
        {
            MySlot.SetSlotData();
        }
    }
    public void RefreshETCSlotsData()
    {
        foreach (Slot MySlot in m_ETCSlots)
        {
            MySlot.SetSlotData();
        }
    }
    public void RefreshSlotDatas()
    {
        switch (OpendSlotsType)
        {
            case INVENTORYUI_BUTTON.WEAPON:
                RefreshWeaponSlotsData();
                break;
            case INVENTORYUI_BUTTON.CONSUMPTION:
                RefreshConsumptionSlotsData();
                break;
            case INVENTORYUI_BUTTON.ETC:
                RefreshETCSlotsData();
                break;
            default:
                Debug.Log("CriticalError - UI_Inventory_RefreshSlotDatas");
                return;
        }
        SetButtonColor();
    }
    private void SetButtonColor() //현재 활성화되있는 슬롯의 타입을 알기위해 색깔에 차별점을둠
    {
        m_WeaponButton.image.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        m_ConsumptionButton.image.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        m_ETCButton.image.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        switch (OpendSlotsType)
        {
            case INVENTORYUI_BUTTON.WEAPON:
                m_WeaponButton.image.color = new Color(1, 1, 1, 0.5f);
                break;
            case INVENTORYUI_BUTTON.CONSUMPTION:
                m_ConsumptionButton.image.color = new Color(1, 1, 1, 0.5f);
                break;
            case INVENTORYUI_BUTTON.ETC:
                m_ETCButton.image.color = new Color(1, 1, 1, 0.5f);
                break;
            default:
                Debug.Log("CriticalError - UI_Inventory_RefreshSlotDatas");
                return;
        }
    }
    public Sprite GetInventoryItemImage(ITEMTYPE Type, int SlotNumber)
    {
        switch (Type)
        {
            case ITEMTYPE.WEAPON:
                {
                    if (m_WeaponSlots.Count <= SlotNumber) return null;
                    if(m_WeaponSlots[SlotNumber].IsData)
                    return m_WeaponSlots[SlotNumber].GetSlotImage;
                }
                break;
            case ITEMTYPE.CONSUMPTION:
                {
                    if (m_ConsumptionSlots.Count <= SlotNumber) return null;
                    if (m_ConsumptionSlots[SlotNumber].IsData)
                        return m_ConsumptionSlots[SlotNumber].GetSlotImage;
                }
                break;
            case ITEMTYPE.ETC:
                {
                    if (m_ETCSlots.Count <= SlotNumber) return null;
                    if (m_ETCSlots[SlotNumber].IsData)
                        return m_ETCSlots[SlotNumber].GetSlotImage;
                }
                break;
            default:
                return null;
        }
        return null;
    }
}
