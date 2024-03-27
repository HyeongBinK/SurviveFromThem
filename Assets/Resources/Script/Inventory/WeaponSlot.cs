using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private bool IsMainWeaponSlot; // 메인,서브 무기 2종뿐이므로 불린변수로 True 일시 메인무기 False일시 서브무기
    [SerializeField] private Image m_WeaponImage; //아이템 이미지
    [SerializeField] private Image m_DisActiveImage; //비활성화시 활성화되는 이미지

    [SerializeField] private Sprite DefaultImage; // 데이터가 없을떄 보일 디폴트 이미지
    private ToolTip m_Tooltip; //툴팁
    private string m_TooltipText; //툴팁의 텍스트내용을 미리 받아와둔다
    private DraggedObject Dragobject; //드래그 이동, 교환 기능 이용시 활성화되는 오브젝트
    private bool IsData; //데이터가있는가의 여부
    
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
        if (IsMainWeaponSlot) //메인무기슬롯일 경우
        {
           if(WeaponManager.Instance.m_bIsMainWeaponData)
            {
                SlotWeaponData = WeaponManager.Instance.GetMainWeapon;
                IsWeaponData = true;
            }
        }
        else  //서브무기슬롯일 경우
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
    public void OnPointerEnter(PointerEventData eventData) //마우스가 포인터(슬롯)에 들어왔을때 툴팁표시 그리고 드래그중인 오브젝트가 들어왔을떄 정보전달
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

    public void OnPointerExit(PointerEventData eventData) //마우스가 포인터(슬롯)에서 멀어졌을떄 툴팁 비활성화
    {
        if (IsData)
            m_Tooltip.gameObject.SetActive(false);

    }
    public void OnPointerDown(PointerEventData eventData) //마우스가 포인터(슬롯)을 눌럿을떄
    {
        if (IsData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) //장착해제
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