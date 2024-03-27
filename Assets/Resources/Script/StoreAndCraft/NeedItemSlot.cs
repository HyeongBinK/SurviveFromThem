using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeedItemSlot : MonoBehaviour
{
    [SerializeField] private Image ItemImage;
    [SerializeField] private Text Quantity;
    public int m_iItemUniqueNumber { get; private set; } //아이템고유번호 
    public int m_iQuantity { get; private set; } // 필요아이템의 갯수
    public void SetSlotData(int ItemUniqueNumber, int ItemQuantity)
    {
        m_iItemUniqueNumber = ItemUniqueNumber;
        m_iQuantity = ItemQuantity;
        Quantity.text = m_iQuantity.ToString();
        if (DataTableManager.Instance)
        {
            var ItemData = DataTableManager.Instance.GetItemData(m_iItemUniqueNumber);
            ItemImage.enabled = true;
            ItemImage.sprite = Resources.Load<Sprite>("UI/Item/" + ItemData.ImageName);
        }
    }

    public void ClearSlotData()
    {
        ItemImage.enabled = false;
        m_iItemUniqueNumber = -1;
        m_iQuantity = -1;
    }
}
