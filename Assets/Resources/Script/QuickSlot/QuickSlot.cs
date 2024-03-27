using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class QuickSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private int m_SlotNumber; //���Թ�ȣ 
    public int GetSlotNumber { get { return m_SlotNumber; } }
    [SerializeField] private Image m_QuickSlotImage; //ȭ�鿡 ���̴� ������ �̹���
    public Sprite GetQuickSlotImage { get { return m_QuickSlotImage.sprite; } }
    [SerializeField] private Text CoolTimeText; //��Ÿ�� �ؽ�Ʈ
    [SerializeField] private Image FillImage; //��Ÿ�� �̹���
    [SerializeField] private Text m_QuantityText; //����
    [SerializeField] private Text m_QuickSlotUseKey; //������ ���Ű �ؽ�Ʈ 
    [SerializeField] private KeyCode KeyCodeUseQuickSlot; //������ ���Ű
    public KeyCode GetQuickSlotKeyCode { get { return KeyCodeUseQuickSlot; } }
    private ToolTip m_Tooltip; //����
    private DraggedObject Dragobject; //�巡�� �̵�, ��ȯ ��� �̿�� Ȱ��ȭ�Ǵ� ������Ʈ
    public DATATYPE m_DataType { get; private set; }
    public int m_iOriginSlotNumber { get; private set; } // �����Կ� ��ϵ� ������ ���� ������ ��ȣ
    public int m_iOriginSlotItemUniqueNumber { get; private set; } 
    public bool m_bIsData { get; private set; } //�����Ͱ��ִ°��� ����

    //��ų�������Ͻ� �ʿ��� ����
    private float m_fCoolTime; //��ų�� ��Ÿ��
    private bool m_bIsCoolTimeFlow = false; //�ڷ�ƾ �ߺ�ȣ���� ����
    private void Start()
    {
        FillImage.type = Image.Type.Filled;
        FillImage.fillMethod = Image.FillMethod.Radial360;
        FillImage.fillOrigin = (int)Image.Origin360.Top;
        m_Tooltip = UIManager.Instance.GetTooltip;
        Dragobject = UIManager.Instance.GetDraggedObject;
        m_bIsData = false;
        m_iOriginSlotNumber = -1;
        m_iOriginSlotItemUniqueNumber = -1;
        m_QuickSlotImage.enabled = false;
        FillImage.gameObject.SetActive(false);
        m_bIsCoolTimeFlow = false;
        m_QuickSlotUseKey.text = KeyCodeUseQuickSlot.ToString();
    }
    public void SetOriginSlotNumber(int NewNumber) //�κ��丮��� �������� ������ġ�� �����Ͽ� �������Թ�ȣ�� ����� ���
    {
        m_iOriginSlotNumber = NewNumber;
    }
    public void SetSlotData(DATATYPE Type, int SlotNumber) //���Գѹ��� ������� ���Ե����ͼ���(ó��, ���ŵ ���)
    {
        if (!UIManager.Instance) return;
        if(m_bIsData) //������ �����Ͱ� ������� ���
            ClearSlotData();
        m_DataType = Type;
        m_iOriginSlotNumber = SlotNumber;
        m_bIsData = true;

        if (Type == DATATYPE.SKILL)
        {
            FillImage.gameObject.SetActive(true);
            m_fCoolTime = DataTableManager.Instance.GetSkillData(SlotNumber).SkillCoolTime;

            if (!SkillManager.Instance.GetIsSkillCoolTime((SKILLNAME)m_iOriginSlotNumber))
                Set_FillAmount(0);
            else
                Reset_CoolTime();

            StartCoroutine(CheckSkillCoolTime());
        }
        else
        {
            m_iOriginSlotItemUniqueNumber = GameManager.Instance.GetPlayerData.GetInventorySlotData((ITEMTYPE)m_DataType, m_iOriginSlotNumber).ItemUniqueNumber;
            FillImage.gameObject.SetActive(false);
        }
        RefreshQuickSlot();
    }
    public void ClearSlotData()
    {
        if (m_DataType == DATATYPE.SKILL)
        {
            StopAllCoroutines();
            End_CoolTime();
        }
        if (m_Tooltip.gameObject.activeSelf)
            m_Tooltip.gameObject.SetActive(false);
        m_QuickSlotImage.enabled = false;
        m_QuantityText.gameObject.SetActive(false); ;
        m_bIsData = false;
        m_bIsCoolTimeFlow = false;
        m_iOriginSlotNumber = -1;
        m_iOriginSlotItemUniqueNumber = -1;
        FillImage.gameObject.SetActive(false);
    }
    public void RefreshQuickSlot()
    {
        if (!UIManager.Instance) return;
        if (m_bIsData)
        {
            switch (m_DataType)
            {
                case DATATYPE.SKILL:
                    {
                        m_QuickSlotImage.sprite = UIManager.Instance.GetSkillWindow.GetSkillImage(m_iOriginSlotNumber);
                    }
                    break;
                case DATATYPE.WEAPON:
                    {
                        m_QuickSlotImage.sprite = UIManager.Instance.GetInventoryWindow.GetInventoryItemImage(ITEMTYPE.WEAPON, m_iOriginSlotNumber);
                    }
                    break;
                case DATATYPE.CONSUMPTION:
                    {
                        m_QuickSlotImage.sprite = UIManager.Instance.GetInventoryWindow.GetInventoryItemImage(ITEMTYPE.CONSUMPTION, m_iOriginSlotNumber);
                    }
                    break;
                case DATATYPE.ETC:
                    {
                        m_QuickSlotImage.sprite = UIManager.Instance.GetInventoryWindow.GetInventoryItemImage(ITEMTYPE.ETC, m_iOriginSlotNumber);
                    }
                    break;
                default:
                    break;
            }
            if(m_QuickSlotImage.sprite != null)
                m_QuickSlotImage.enabled = true;
        }
        SetQuantityText();
    }

    private void SetQuantityText()
    {
        bool IsNeedQuantity = false;
        int Value = -1;
        switch (m_DataType)
        {
            case DATATYPE.CONSUMPTION:
                {
                    IsNeedQuantity = true;
                    var ItemData = GameManager.Instance.GetPlayerData.GetInventorySlotData(ITEMTYPE.CONSUMPTION, m_iOriginSlotNumber);
                    Value = ItemData.Value;
                }
                break;
            case DATATYPE.ETC:
                {
                    IsNeedQuantity = true;
                    var ItemData = GameManager.Instance.GetPlayerData.GetInventorySlotData(ITEMTYPE.ETC, m_iOriginSlotNumber);
                    Value = ItemData.Value;
                }
                break;
            default:
                break;
        }

        if (IsNeedQuantity)
        {
            if (Value > 0)
            {
                m_QuantityText.gameObject.SetActive(true);
                m_QuantityText.text = Value.ToString();
            }
            else
            {
                m_QuantityText.gameObject.SetActive(false);
                ClearSlotData();
            }
            return;
        }

        m_QuantityText.gameObject.SetActive(false);
    }

    //��Ÿ�Ӱ���
    IEnumerator CheckSkillCoolTime()
    {
        while (m_DataType == DATATYPE.SKILL)
        {
            Check_CoolTime(SkillManager.Instance.GetSkillCoolTimer((SKILLNAME)m_iOriginSlotNumber));
            yield return null;
        }
    }

    private void Check_CoolTime(float CoolTimer)
    {
        if (!m_bIsCoolTimeFlow) return;


        if (!SkillManager.Instance.GetIsSkillCoolTime((SKILLNAME)m_iOriginSlotNumber))
        {
            End_CoolTime();
            return;
        }
        Set_FillAmount(m_fCoolTime - CoolTimer);
    }

    private void End_CoolTime()
    {
        m_bIsCoolTimeFlow = false;
        Set_FillAmount(0);
        CoolTimeText.gameObject.SetActive(false);
    }

    public void Reset_CoolTime()
    {
        if (!SkillManager.Instance.GetIsSkillCoolTime((SKILLNAME)m_iOriginSlotNumber)) return;
        m_bIsCoolTimeFlow = true;
        CoolTimeText.gameObject.SetActive(true);
        Set_FillAmount(m_fCoolTime);
    }
    private void Set_FillAmount(float _value)
    {
        FillImage.fillAmount = _value / m_fCoolTime;
        string txt = _value.ToString("0.0");
        CoolTimeText.text = txt;
    }

    public void UseQuickSlot()
    {
        if (!m_bIsData) return;

        switch (m_DataType)
        {
            case DATATYPE.SKILL:
                {
                    if (SkillManager.Instance)
                    {
                        SkillManager.Instance.UseSkill((SKILLNAME)m_iOriginSlotNumber);
                        Reset_CoolTime();
                    }
                }
                break;
            case DATATYPE.CONSUMPTION:
                {
                    if (GameManager.Instance)
                        GameManager.Instance.ItemUse(m_iOriginSlotNumber, 1);
                }
                break;
            case DATATYPE.WEAPON:
                {
                    if (WeaponManager.Instance)
                    {
                        ItemSlotData DummyWeaponData = GameManager.Instance.GetPlayerData.GetInventorySlotData(ITEMTYPE.WEAPON, m_SlotNumber);
                        if (!DummyWeaponData.IsData) return;
                        if (!GameManager.Instance.GetPlayerData.GetInventory.DecreaseItemQuantity(ITEMTYPE.WEAPON, m_SlotNumber, 1))
                        {
                            GameManager.Instance.AddNewLog("�����ۼ��� �����۾����� ����");
                            return;
                        }
                        if (DataTableManager.Instance.GetWeaponData(DummyWeaponData.ItemUniqueNumber).WeaponType == WEAPONTYPE.PISTOL)
                            WeaponManager.Instance.SetSubWeaponData(DummyWeaponData, m_SlotNumber);
                        else
                            WeaponManager.Instance.SetMainWeaponData(DummyWeaponData, m_SlotNumber);

                        if (SoundManager.Instance)
                            SoundManager.Instance.PlaySFX("MouseClick");
                        ClearSlotData();
                    }
                }
                break;
            default:
                return;
        }

    }

    public void OnPointerEnter(PointerEventData eventData) //���콺�� ������(����)�� �������� ����ǥ�� �׸��� �巡������ ������Ʈ�� �������� ��������
    { 
        if (Dragobject.IsDrag)
        {
            Dragobject.SetCurSlotData(DATATYPE.QUICKSLOT, m_SlotNumber);
        }

        if (m_bIsData)
        {
            string TooltipText = "";
            switch (m_DataType)
            {
                case DATATYPE.SKILL:
                    {
                        TooltipText = DataTableManager.Instance.MakeSkillTooltipText(m_iOriginSlotNumber, GameManager.Instance.GetPlayerData.GetPlayerStatusData.GetSkillLevel((SKILLNAME)m_iOriginSlotNumber));
                    }
                    break;
                case DATATYPE.WEAPON:
                    {
                        var ItemData = GameManager.Instance.GetPlayerData.GetInventorySlotData(ITEMTYPE.WEAPON, m_iOriginSlotNumber);
                        TooltipText = DataTableManager.Instance.MakeItemToolTipText(ItemData.ItemUniqueNumber, ItemData.Value);
                    }
                    break;
                case DATATYPE.CONSUMPTION:
                    {
                        var ItemData = GameManager.Instance.GetPlayerData.GetInventorySlotData(ITEMTYPE.CONSUMPTION, m_iOriginSlotNumber);
                        TooltipText = DataTableManager.Instance.MakeItemToolTipText(ItemData.ItemUniqueNumber, ItemData.Value);
                    }
                    break;
                case DATATYPE.ETC:
                    {
                        var ItemData = GameManager.Instance.GetPlayerData.GetInventorySlotData(ITEMTYPE.ETC, m_iOriginSlotNumber);
                        TooltipText = DataTableManager.Instance.MakeItemToolTipText(ItemData.ItemUniqueNumber, ItemData.Value);
                    }
                    break;
                default:
                    break;
            
            }
            m_Tooltip.SetToolTipText(TooltipText);
            var Position = gameObject.transform.position;
            m_Tooltip.SetToolTipPosition(Position.x, Position.y);
            m_Tooltip.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData) //���콺�� ������(����)���� �־������� ���� ��Ȱ��ȭ
    {
        if (m_bIsData)
            m_Tooltip.gameObject.SetActive(false);
    }


    public void OnPointerDown(PointerEventData eventData) //���콺�� ������(����)�� ��������
    {
        if (!GameManager.Instance) return;
        if (!DataTableManager.Instance) return;
        if (!m_bIsData) return;
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            Dragobject.SetStartSlotData(DATATYPE.QUICKSLOT, m_SlotNumber, m_QuickSlotImage.sprite);
            if (SoundManager.Instance)
                SoundManager.Instance.PlaySFX("DragStart");
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
