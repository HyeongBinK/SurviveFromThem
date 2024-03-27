using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private bool IsMainWeaponSlot; // ����,���� ���� 2�����̹Ƿ� �Ҹ������� True �Ͻ� ���ι��� False�Ͻ� ���깫��
    [SerializeField] private Image m_WeaponImage; //������ �̹���
    [SerializeField] private Image m_DisActiveImage; //��Ȱ��ȭ�� Ȱ��ȭ�Ǵ� �̹���

    [SerializeField] private Sprite DefaultImage; // �����Ͱ� ������ ���� ����Ʈ �̹���
    private ToolTip m_Tooltip; //����
    private string m_TooltipText; //������ �ؽ�Ʈ������ �̸� �޾ƿ͵д�
    private DraggedObject Dragobject; //�巡�� �̵�, ��ȯ ��� �̿�� Ȱ��ȭ�Ǵ� ������Ʈ
    private bool IsData; //�����Ͱ��ִ°��� ����
    
    public void Init()
    {
        m_Tooltip = UIManager.Instance.GetTooltip;
        Dragobject = UIManager.Instance.GetDraggedObject;
        IsData = false;
        ClearSlotData();
        SetSlotData();
    }

    private void OnDisable()
    {
        if (m_Tooltip.gameObject.activeSelf && m_Tooltip.transform.position == gameObject.transform.position)
            m_Tooltip.gameObject.SetActive(false);
    }

    public void SetSlotData()
    {
        if (!WeaponManager.Instance) return;
        if (!DataTableManager.Instance) return;

        WeaponData SlotWeaponData = new WeaponData();
        bool IsWeaponData = false;
        if (IsMainWeaponSlot) //���ι��⽽���� ���
        {
           if(WeaponManager.Instance.m_bIsMainWeaponData)
            {
                SlotWeaponData = WeaponManager.Instance.GetMainWeapon;
                IsWeaponData = true;
            }
        }
        else  //���깫�⽽���� ���
        {
            if (WeaponManager.Instance.m_bIsSubWeaponData)
            {
                SlotWeaponData = WeaponManager.Instance.GetSubWeapon;
                IsWeaponData = true;
            }
        }
        if (IsWeaponData)
        {
            var ItemData = DataTableManager.Instance.GetItemData(SlotWeaponData.WeaponUniqueNumber);
            m_TooltipText = DataTableManager.Instance.MakeItemToolTipText(SlotWeaponData.WeaponUniqueNumber, SlotWeaponData.Reinforce);
            m_WeaponImage.sprite = Resources.Load<Sprite>("UI/Item/" + ItemData.ImageName);
            IsData = true;
            return;
        }
        ClearSlotData();
    }
    public void ClearSlotData()
    {
        if (m_Tooltip.gameObject.activeSelf)
            m_Tooltip.gameObject.SetActive(false);
        m_WeaponImage.sprite = DefaultImage;
        m_TooltipText = "";
        m_DisActiveImage.gameObject.SetActive(false);
        IsData = false;
    }
    public void OnDisActiveImage()
    {
        m_DisActiveImage.gameObject.SetActive(true);
    }
    public void OffDisActiveImage()
    {
        m_DisActiveImage.gameObject.SetActive(false);
    }
    public void OnPointerEnter(PointerEventData eventData) //���콺�� ������(����)�� �������� ����ǥ�� �׸��� �巡������ ������Ʈ�� �������� ��������
    {
        // if (Dragobject.IsDrag)

        if (IsData)
        {
            var Position = gameObject.transform.position;
            m_Tooltip.SetToolTipText(m_TooltipText);
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
        if (IsData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) //��������
            {
                if(WeaponManager.Instance)
                {
                    if (IsMainWeaponSlot)
                        WeaponManager.Instance.SetMainWeaponData(new ItemSlotData());
                    else
                        WeaponManager.Instance.SetSubWeaponData(new ItemSlotData());
                }
                SetSlotData();
                if (UIManager.Instance.GetUserInfo.gameObject.activeSelf)
                    UIManager.Instance.GetUserInfo.WeaponOnDisActiveImage();

                if (SoundManager.Instance)
                    SoundManager.Instance.PlaySFX("MouseClick");
            }
        }
    }
}