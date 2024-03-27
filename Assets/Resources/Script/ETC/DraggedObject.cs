using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DATATYPE
{
    NULL = -1, //데이터없음
    SKILL = 0, //스킬
    WEAPON, //무기아이템
    CONSUMPTION, //소모아이템
    ETC, //기타아이템
    QUICKSLOT //퀵슬롯
}
public class DraggedObject : MonoBehaviour //아이템,스킬,퀵슬롯의 이동 등에 사용되는 드래그중인 오브젝트  
{ 
    public bool IsDrag { get; private set; } //드래그되는중인가?
    [SerializeField] private Image m_Image; //드래그되는 오브젝트의 이미지
    public DATATYPE m_eDraggingType { get; private set; } //드래그되고 있는 정보의 타입
    public int m_iSlotNumber { get; private set; } // 드래그가 시작된 슬롯의 번호
    public DATATYPE m_eCurType { get; private set; } //현재 드래그가 위치하고 있는 곳의 정보타입
    public int m_iCurSlotNumber { get; private set; } // 현재 드래그가 위치하고 있는 곳의 슬롯번호

    private void Start()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
        Clear();
    }

    private void Update()
    {
        if (!IsDrag || !gameObject.activeSelf) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
            gameObject.transform.position = Camera.main.WorldToScreenPoint(raycastHit.point);
    }
    private void Clear()
    {
        IsDrag = false;
        m_eDraggingType = DATATYPE.NULL;
        m_eCurType = DATATYPE.NULL;
        m_iSlotNumber = -1;
        m_iCurSlotNumber = -1;
    }
    public void SetStartSlotData(DATATYPE type, int SlotNumber, Sprite image)
    {
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("DragStart");

        gameObject.SetActive(true);
        m_eDraggingType = type;
        m_iSlotNumber = SlotNumber;
        m_Image.sprite = image;
        IsDrag = true;
    }
    public void SetCurSlotData(DATATYPE type, int SlotNumber)
    {
        m_eCurType = type;
        m_iCurSlotNumber = SlotNumber;
    }

    public void EndDrag()
    {
        if (!UIManager.Instance) return;
        if (!GameManager.Instance) return;

        if (m_eDraggingType == DATATYPE.NULL) return;
        switch (m_eCurType)
        {
            case DATATYPE.QUICKSLOT:
                {
                    UIManager.Instance.GetQuickSlots.SetQuickSlotData(m_iCurSlotNumber, m_eDraggingType, m_iSlotNumber);
                }
                break;
            case DATATYPE.WEAPON:
                {
                    if (m_eDraggingType != DATATYPE.WEAPON) break;
                    GameManager.Instance.GetPlayerData.GetInventory.SwitchSlotData((ITEMTYPE)m_eDraggingType, m_iSlotNumber, m_iCurSlotNumber);
                }
                break;
            case DATATYPE.CONSUMPTION:
                {
                    if (m_eDraggingType != DATATYPE.CONSUMPTION) break;
                    GameManager.Instance.GetPlayerData.GetInventory.SwitchSlotData((ITEMTYPE)m_eDraggingType, m_iSlotNumber, m_iCurSlotNumber);
                }
                break;
            case DATATYPE.ETC:
                {
                    if (m_eDraggingType != DATATYPE.ETC) break;
                    GameManager.Instance.GetPlayerData.GetInventory.SwitchSlotData((ITEMTYPE)m_eDraggingType, m_iSlotNumber, m_iCurSlotNumber);
                }
                break;
            case DATATYPE.NULL:
                {
                    switch (m_eDraggingType)
                    {
                        case DATATYPE.QUICKSLOT:
                            UIManager.Instance.GetQuickSlots.ClearQuickSlotData(m_iSlotNumber);
                            break;
                    /*    case DATATYPE.WEAPON:
                            GameManager.Instance.GetPlayerData.GetInventory.DecreaseItemQuantity(ITEMTYPE.WEAPON, m_iSlotNumber, 1);
                            break;
                        case DATATYPE.CONSUMPTION:
                            {
                                int TrashNum = 1;
                                //버릴갯수 입력받는 기능 추가
                                GameManager.Instance.GetPlayerData.GetInventory.DecreaseItemQuantity(ITEMTYPE.CONSUMPTION, m_iSlotNumber, TrashNum);
                            }
                            break;
                        case DATATYPE.ETC:
                            {
                                int TrashNum = 1;
                                //버릴갯수 입력받는 기능 추가
                                GameManager.Instance.GetPlayerData.GetInventory.DecreaseItemQuantity(ITEMTYPE.ETC, m_iSlotNumber, TrashNum);
                            }
                            break;*/
                    }
                }
                break;
        }
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("DragEnd");
        Clear();
        gameObject.SetActive(false);
    }
}
