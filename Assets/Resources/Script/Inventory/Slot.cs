using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image m_ItemImage; //아이템 이미지
    public Sprite GetSlotImage { get { return m_ItemImage.sprite; } }
    [SerializeField] private Image m_LockImage; //잠긴슬롯일때 활성화될 이미지
    [SerializeField] private Text m_QuantityText; //갯수 
    private ToolTip m_Tooltip; //툴팁
    private DraggedObject Dragobject; //드래그 이동, 교환 기능 이용시 활성화되는 오브젝트
    [SerializeField] private int m_SlotNumber; //슬롯번호  
    private ITEMTYPE m_Type; //슬롯의 아이템 타입
    public bool IsData { get; private set; } //데이터가있는가의 여부
    private bool IsActivate; //활성화된 슬롯인가?

    private void Awake()
    {
        m_Tooltip = UIManager.Instance.GetTooltip;
        Dragobject = UIManager.Instance.GetDraggedObject;
    }
    private void OnDisable()
    {
        if (m_Tooltip.gameObject.activeSelf && m_Tooltip.transform.position == gameObject.transform.position)
            m_Tooltip.gameObject.SetActive(false);
    }
    public void Init(ITEMTYPE Type, int NewSlotNumber) //슬롯생성시에 슬롯자체의 고유번호 셋팅
    {
        m_Tooltip = UIManager.Instance.GetTooltip;
        Dragobject = UIManager.Instance.GetDraggedObject;
        m_Type = Type;
        m_SlotNumber = NewSlotNumber;
        IsData = false;
        IsActivate = false;
        m_LockImage.gameObject.SetActive(true);
        m_ItemImage.enabled = false;
        SetSlotData();
    }
    public void SetSlotData() //슬롯넘버를 기반으로 슬롯데이터셋팅(처음, 갱신등에 사용)
    {
        if (GameManager.Instance)
        {
            IsActivate = false;
            int ItemNumber = -1;
            int Quantity = -1;
            if (m_SlotNumber < GameManager.Instance.GetPlayerData.GetInventory.GetActivatedInventorySlotCount(m_Type))
                IsActivate = true;
            switch (m_Type) //아이템의 종류에따라 갯수표기여부와 장비아이템이 아니면 갯수셋팅
            {
                case ITEMTYPE.WEAPON:
                    {
                        var Data = GameManager.Instance.GetPlayerData.GetInventorySlotData(ITEMTYPE.WEAPON, m_SlotNumber);
                        if (!Data.IsData)
                        {
                            ClearSlotData();
                            break;
                        }
                        ItemNumber = Data.ItemUniqueNumber;
                    }
                    break;
                case ITEMTYPE.CONSUMPTION:
                    {
                        var Data = GameManager.Instance.GetPlayerData.GetInventorySlotData(ITEMTYPE.CONSUMPTION, m_SlotNumber);
                        if (!Data.IsData)
                        {
                            ClearSlotData();
                            break;
                        }
                        ItemNumber = Data.ItemUniqueNumber;
                        Quantity = Data.Value;
                    }
                    break;
                case ITEMTYPE.ETC:
                    {
                        var Data = GameManager.Instance.GetPlayerData.GetInventorySlotData(ITEMTYPE.ETC, m_SlotNumber);
                        if (!Data.IsData)
                        {
                            ClearSlotData();
                            break;
                        }
                        ItemNumber = Data.ItemUniqueNumber;
                        Quantity = Data.Value;
                    }
                    break;
            }
            if (ItemNumber != -1)
            {
                IsData = true;
                if (DataTableManager.Instance)
                {
                    var ItemData = DataTableManager.Instance.GetItemData(ItemNumber);
                    m_ItemImage.enabled = true;
                    m_ItemImage.sprite = Resources.Load<Sprite>("UI/Item/" + ItemData.ImageName);
                }
            }
            else
                ClearSlotData();

            OnOffQuantityText(Quantity);
            m_LockImage.gameObject.SetActive(!IsActivate);
        }
    }
    public void ClearSlotData()
    {
        if (m_Tooltip.gameObject.activeSelf)
            m_Tooltip.gameObject.SetActive(false);
        m_ItemImage.enabled = false;
        m_QuantityText.gameObject.SetActive(false); ;
        IsData = false;
    }
    private void OnOffQuantityText(int Value)
    {
        if (m_Type == ITEMTYPE.WEAPON)
            m_QuantityText.gameObject.SetActive(false);
        else
        {
            if (Value <= 0)
                m_QuantityText.gameObject.SetActive(false);
            else
            {
                m_QuantityText.gameObject.SetActive(true);
                m_QuantityText.text = Value.ToString();
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData) //마우스가 포인터(슬롯)에 들어왔을때 툴팁표시 그리고 드래그중인 오브젝트가 들어왔을떄 정보전달
    {
        if (Dragobject.IsDrag)
        {
            Dragobject.SetCurSlotData((DATATYPE)m_Type, m_SlotNumber);
        }

        if (IsData)
        {
            var Position = gameObject.transform.position;
            var Data = GameManager.Instance.GetPlayerData.GetInventorySlotData(m_Type, m_SlotNumber);
            m_Tooltip.SetToolTipText(DataTableManager.Instance.MakeItemToolTipText(Data.ItemUniqueNumber, Data.Value));

            m_Tooltip.SetToolTipPosition(Position.x, Position.y);
            m_Tooltip.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData) //마우스가 포인터(슬롯)에서 멀어졌을떄 툴팁 비활성화
    {
        if (IsData)
            m_Tooltip.gameObject.SetActive(false);

    }

    public void OnPointerDown(PointerEventData eventData) //마우스가 포인터(슬롯)을 눌럿을떄
    {
        if (!GameManager.Instance) return;
        if (!DataTableManager.Instance) return;
        if (!IsData) return;
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left: //마우스왼쪽클릭(아이템이동(드래그생성)), 상점UI오픈시엔 판매버튼생성
                {
                    Dragobject.SetStartSlotData((DATATYPE)m_Type, m_SlotNumber, m_ItemImage.sprite);
                }
                break;
            case PointerEventData.InputButton.Right:
                {

                    switch (m_Type)
                    {
                        case ITEMTYPE.WEAPON:
                            {
                                if (WeaponManager.Instance)
                                {
                                    ItemSlotData DummyWeaponData = GameManager.Instance.GetPlayerData.GetInventorySlotData(ITEMTYPE.WEAPON, m_SlotNumber);
                                    if (!DummyWeaponData.IsData) return;
                                    if (!GameManager.Instance.GetPlayerData.GetInventory.DecreaseItemQuantity(m_Type, m_SlotNumber, 1))
                                        GameManager.Instance.AddNewLog("아이템수량 감소작업에서 에러");

                                    if (DataTableManager.Instance.GetWeaponData(DummyWeaponData.ItemUniqueNumber).WeaponType == WEAPONTYPE.PISTOL)
                                        WeaponManager.Instance.SetSubWeaponData(DummyWeaponData, m_SlotNumber);
                                    else
                                        WeaponManager.Instance.SetMainWeaponData(DummyWeaponData, m_SlotNumber);
                                }
                            }
                            break;
                        case ITEMTYPE.CONSUMPTION:
                            GameManager.Instance.ItemUse(m_SlotNumber, 1);
                            break;
                    }
                    if (SoundManager.Instance)
                        SoundManager.Instance.PlaySFX("MouseClick");
                }
                break;

        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (Dragobject.IsDrag)
        {
            Dragobject.EndDrag();
        }
    }
}
