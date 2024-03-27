using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class ItemObjectV2 : MonoBehaviour
{
    [SerializeField] private GameObject m_WeaponItemObject;
    [SerializeField] private GameObject m_ConsumptionItemObject;
    [SerializeField] private GameObject mETCItemObject;
    [SerializeField] private ITEMTYPE m_ItemType;  
    [SerializeField] private int m_ItemNumber = -1;
    [SerializeField] private int m_ItemQuantity;

    public event Action DropItemDisalbe; // 아이템을 먹거나 습득가능시간이 지나서 사라질시 호출
    private readonly float PositionYOffset = 1.0f;
    private void Awake()
    {
        DropItemDisalbe = null;
    }
    private void Start()
    {
        ActiveAppearance();
    }
    private void OnTriggerEnter(Collider other) //플레이어에 부딛히면 아이템습득
    {
        if (other.tag == "Player")
        {
            if (GameManager.Instance)
            {
                //게임매니져를 통해 플레이어내의 인벤토리에 해당아이템데이터를 추가하는 함수
                if (GameManager.Instance.GetPlayerData.AddItem(m_ItemType, m_ItemNumber, m_ItemQuantity))
                {

                    if (SoundManager.Instance)
                        SoundManager.Instance.PlaySFX("PickUpItem");
                    StringBuilder SB = new StringBuilder();
                    SB.Append(DataTableManager.Instance.GetItemData(m_ItemNumber).Name);
                    if (m_ItemType != ITEMTYPE.WEAPON)
                    {
                        SB.Append(" ");
                        SB.Append(m_ItemQuantity.ToString());
                        SB.Append("개 획득!");
                    }
                    else
                    {
                        SB.Append("를");
                        SB.Append(" 얻었다");
                    }
                    GameManager.Instance.AddNewLog(SB.ToString());
                    OnDisableEvent();
                }
            }
        }
    }
    public void ItemDataInit(int NewItemUniqueNumber, int Quantity, Transform Tr) //아이템 드롭시 아이템데이터 입력받음
    {
        if (DataTableManager.Instance.GetItemData(NewItemUniqueNumber) != null)
        {
            m_ItemNumber = NewItemUniqueNumber;
            m_ItemQuantity = Quantity;
            var ItemData = DataTableManager.Instance.GetItemData(m_ItemNumber);
            m_ItemType = ItemData.Type;
            transform.position = new Vector3(Tr.position.x, Tr.position.y + PositionYOffset, Tr.position.z);

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                ActiveAppearance();
                StartCoroutine(DisappeardByTime());
            }
        }
    }
    public void OnDisableEvent()
    {
        if (DropItemDisalbe != null)
        {
            DropItemDisalbe();
            DropItemDisalbe = null;
        }
        gameObject.SetActive(false);
    }

    IEnumerator DisappeardByTime() //일정시간이 지나면 자동으로 아이템이 사라짐
    {
        if (ItemManager.Instance)
        {
            yield return new WaitForSeconds(ItemManager.Instance.ItemHoldingTime);
            OnDisableEvent();
        }
    }

    private void ActiveAppearance()
    {
        DisActiveAllObject();

        switch (m_ItemType)
        {
            case ITEMTYPE.WEAPON:
                m_WeaponItemObject.gameObject.SetActive(true);
                break;
            case ITEMTYPE.CONSUMPTION:
                m_ConsumptionItemObject.gameObject.SetActive(true);
                break;
            case ITEMTYPE.ETC:
                mETCItemObject.gameObject.SetActive(true);
                break;
        }
    }

    private void DisActiveAllObject()
    {
        m_WeaponItemObject.gameObject.SetActive(false);
        m_ConsumptionItemObject.gameObject.SetActive(false);
        mETCItemObject.gameObject.SetActive(false);
    }
}
