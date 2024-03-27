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

    public event Action DropItemDisalbe; // �������� �԰ų� ���氡�ɽð��� ������ ������� ȣ��
    private readonly float PositionYOffset = 1.0f;
    private void Awake()
    {
        DropItemDisalbe = null;
    }
    private void Start()
    {
        ActiveAppearance();
    }
    private void OnTriggerEnter(Collider other) //�÷��̾ �ε����� �����۽���
    {
        if (other.tag == "Player")
        {
            if (GameManager.Instance)
            {
                //���ӸŴ����� ���� �÷��̾�� �κ��丮�� �ش�����۵����͸� �߰��ϴ� �Լ�
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
                        SB.Append("�� ȹ��!");
                    }
                    else
                    {
                        SB.Append("��");
                        SB.Append(" �����");
                    }
                    GameManager.Instance.AddNewLog(SB.ToString());
                    OnDisableEvent();
                }
            }
        }
    }
    public void ItemDataInit(int NewItemUniqueNumber, int Quantity, Transform Tr) //������ ��ӽ� �����۵����� �Է¹���
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

    IEnumerator DisappeardByTime() //�����ð��� ������ �ڵ����� �������� �����
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
