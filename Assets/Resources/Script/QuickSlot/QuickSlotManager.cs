using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickSlotManager : MonoBehaviour
{
    [SerializeField] private QuickSlot[] m_QuickSlots;
    public QuickSlot GetQuickSlotData(int SlotNumber)
    {
        if (m_QuickSlots.Length <= SlotNumber || SlotNumber < 0)
            return new QuickSlot();

        return m_QuickSlots[SlotNumber];
    }
    public void ClearQuickSlotData(int SlotNumber)
    {
        if (m_QuickSlots.Length <= SlotNumber || SlotNumber < 0) return;
        m_QuickSlots[SlotNumber].ClearSlotData();
    }

    public void SetQuickSlotData(int SlotNumber, DATATYPE Type, int OriginSlotNumber)
    {
        if (m_QuickSlots.Length <= SlotNumber || SlotNumber < 0) return;

        if (Type == DATATYPE.QUICKSLOT) //�������ϰ��(�����Գ����� swap)
        {
            if (m_QuickSlots[SlotNumber].m_bIsData) //�����Ͱ� �������
            {
                if (m_QuickSlots[SlotNumber].GetSlotNumber == OriginSlotNumber) return; //�ڱ��ڽ��̸� ����
                Sprite TempImage = m_QuickSlots[SlotNumber].GetQuickSlotImage;
                DATATYPE TempType = m_QuickSlots[SlotNumber].m_DataType;
                int TempOriginNumber = m_QuickSlots[SlotNumber].m_iOriginSlotNumber;
                m_QuickSlots[SlotNumber].SetSlotData(m_QuickSlots[OriginSlotNumber].m_DataType, m_QuickSlots[OriginSlotNumber].m_iOriginSlotNumber);
                m_QuickSlots[OriginSlotNumber].SetSlotData(TempType, TempOriginNumber);
                return;
            }
            else
            {
                m_QuickSlots[SlotNumber].SetSlotData(m_QuickSlots[OriginSlotNumber].m_DataType, m_QuickSlots[OriginSlotNumber].m_iOriginSlotNumber);
                m_QuickSlots[OriginSlotNumber].ClearSlotData();
                return;
            }
        }

        //�׿�(���ο� ������ ��� or �����)
        foreach (QuickSlot QS in m_QuickSlots) //���� �Ȱ��� �����Ͱ� �����Կ� �����Ұ�� 
        {
            if (QS.m_DataType == Type && QS.m_iOriginSlotNumber == OriginSlotNumber)
            {
                QS.ClearSlotData();
                break;
            }
        }
        m_QuickSlots[SlotNumber].SetSlotData(Type, OriginSlotNumber);
    }
    public void CheckQuickSlotInput()
    {
        foreach (QuickSlot QS in m_QuickSlots)
        {
            if (Input.GetKeyDown(QS.GetQuickSlotKeyCode))
            {
                QS.UseQuickSlot();
                return;
            }
        }
    }
    public void RefreshQuickSlot()
    {
        foreach (QuickSlot QS in m_QuickSlots)
        {
            QS.RefreshQuickSlot();
        }
    }

    public void ChangeOriginQuickSlotNumber(DATATYPE Type, int OriginSlotNumber, int NewOriginSlotNumber)
    {
        foreach(QuickSlot QS in m_QuickSlots)
        {
            if(QS.m_DataType == Type && QS.m_iOriginSlotItemUniqueNumber == GameManager.Instance.GetPlayerData.GetInventorySlotData((ITEMTYPE)QS.m_DataType, NewOriginSlotNumber).ItemUniqueNumber)
            {
                QS.SetOriginSlotNumber(NewOriginSlotNumber);
            }
        }
    }
  /*  public void SetQuickSlotSkillTimeFlow(int QuickSlotNumber)
    {
        if (m_QuickSlots.Length <= QuickSlotNumber) return;
        m_QuickSlots[QuickSlotNumber].Reset_CoolTime();
    }
    public KeyCode GetKeyQuickslotKeycode(int SlotNumber)
    {
        if (m_QuickSlots.Length <= SlotNumber) return KeyCode.None;
        else
            return m_QuickSlots[SlotNumber].GetQuickSlotKeyCode;
    }
    public void UseQuick(int SlotNumber)
    {
        if (Input.GetKeyDown(GetKeyQuickslotKeycode(SlotNumber)))
            m_QuickSlots[SlotNumber].UseQuickSlot();
    }*/
}
