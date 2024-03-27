using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image m_ItemImage; //������ �̹���
    public Sprite GetSlotImage { get { return m_ItemImage.sprite; } }
    [SerializeField] private Image m_LockImage; //��佽���϶� Ȱ��ȭ�� �̹���
    [SerializeField] private Text m_QuantityText; //���� 
    private ToolTip m_Tooltip; //����
    private DraggedObject Dragobject; //�巡�� �̵�, ��ȯ ��� �̿�� Ȱ��ȭ�Ǵ� ������Ʈ
    [SerializeField] private int m_SlotNumber; //���Թ�ȣ  
    private ITEMTYPE m_Type; //������ ������ Ÿ��
    public bool IsData { get; private set; } //�����Ͱ��ִ°��� ����
    private bool IsActivate; //Ȱ��ȭ�� �����ΰ�?

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
    public void Init(ITEMTYPE Type, int NewSlotNumber) //���Ի����ÿ� ������ü�� ������ȣ ����
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
    public void SetSlotData() //���Գѹ��� ������� ���Ե����ͼ���(ó��, ���ŵ ���)
    {
        if (GameManager.Instance)
        {
            IsActivate = false;
            int ItemNumber = -1;
            int Quantity = -1;
            if (m_SlotNumber < GameManager.Instance.GetPlayerData.GetInventory.GetActivatedInventorySlotCount(m_Type))
                IsActivate = true;
            switch (m_Type) //�������� ���������� ����ǥ�⿩�ο� ���������� �ƴϸ� ��������
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
    public void OnPointerEnter(PointerEventData eventData) //���콺�� ������(����)�� �������� ����ǥ�� �׸��� �巡������ ������Ʈ�� �������� ��������
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

    public void OnPointerExit(PointerEventData eventData) //���콺�� ������(����)���� �־������� ���� ��Ȱ��ȭ
    {
        if (IsData)
            m_Tooltip.gameObject.SetActive(false);

    }

    public void OnPointerDown(PointerEventData eventData) //���콺�� ������(����)�� ��������
    {
        if (!GameManager.Instance) return;
        if (!DataTableManager.Instance) return;
        if (!IsData) return;
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left: //���콺����Ŭ��(�������̵�(�巡�׻���)), ����UI���½ÿ� �ǸŹ�ư����
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
                                        GameManager.Instance.AddNewLog("�����ۼ��� �����۾����� ����");

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
