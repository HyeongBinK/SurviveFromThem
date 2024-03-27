using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class QuickSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private int m_SlotNumber; //슬롯번호 
    public int GetSlotNumber { get { return m_SlotNumber; } }
    [SerializeField] private Image m_QuickSlotImage; //화면에 보이는 퀵슬롯 이미지
    public Sprite GetQuickSlotImage { get { return m_QuickSlotImage.sprite; } }
    [SerializeField] private Text CoolTimeText; //쿨타임 텍스트
    [SerializeField] private Image FillImage; //쿨타임 이미지
    [SerializeField] private Text m_QuantityText; //갯수
    [SerializeField] private Text m_QuickSlotUseKey; //퀵슬롯 사용키 텍스트 
    [SerializeField] private KeyCode KeyCodeUseQuickSlot; //퀵슬롯 사용키
    public KeyCode GetQuickSlotKeyCode { get { return KeyCodeUseQuickSlot; } }
    private ToolTip m_Tooltip; //툴팁
    private DraggedObject Dragobject; //드래그 이동, 교환 기능 이용시 활성화되는 오브젝트
    public DATATYPE m_DataType { get; private set; }
    public int m_iOriginSlotNumber { get; private set; } // 퀵슬롯에 등록된 정보의 원래 슬롯의 번호
    public int m_iOriginSlotItemUniqueNumber { get; private set; } 
    public bool m_bIsData { get; private set; } //데이터가있는가의 여부

    //스킬데이터일시 필요한 변수
    private float m_fCoolTime; //스킬의 쿨타임
    private bool m_bIsCoolTimeFlow = false; //코루틴 중복호출을 제어
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
    public void SetOriginSlotNumber(int NewNumber) //인벤토리등에서 아이템의 슬롯위치를 변경하여 고유슬롯번호가 변경된 경우
    {
        m_iOriginSlotNumber = NewNumber;
    }
    public void SetSlotData(DATATYPE Type, int SlotNumber) //슬롯넘버를 기반으로 슬롯데이터셋팅(처음, 갱신등에 사용)
    {
        if (!UIManager.Instance) return;
        if(m_bIsData) //기존에 데이터가 있을경우 비움
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

    //쿨타임관련
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
                            GameManager.Instance.AddNewLog("아이템수량 감소작업에서 에러");
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

    public void OnPointerEnter(PointerEventData eventData) //마우스가 포인터(슬롯)에 들어왔을때 툴팁표시 그리고 드래그중인 오브젝트가 들어왔을떄 정보전달
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

    public void OnPointerExit(PointerEventData eventData) //마우스가 포인터(슬롯)에서 멀어졌을떄 툴팁 비활성화
    {
        if (m_bIsData)
            m_Tooltip.gameObject.SetActive(false);
    }


    public void OnPointerDown(PointerEventData eventData) //마우스가 포인터(슬롯)을 눌럿을떄
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
