using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingWindow : MonoBehaviour
{
    [SerializeField] private Button m_CraftButton; //���������۹�ư
    [SerializeField] private Button m_CloseWindowButton; //������ ����â �ݱ� ��ư
    [SerializeField] private Button m_ShowRifleListButton; //������ ���۸���Ʈ Ȱ��/��Ȱ�� ��ư
    [SerializeField] private Button m_ShowPistolListButton; //���� ���۸���Ʈ Ȱ��/��Ȱ�� ��ư
    [SerializeField] private Button m_ShowShotgunListButton; //���� ���۸���Ʈ Ȱ��/��Ȱ�� ��ư
    [SerializeField] private Button m_ShowLazergunListButton; //�������� ���۸���Ʈ Ȱ��/��Ȱ�� ��ư
    [SerializeField] private Button m_ShowConsumptionListButton; //�Ҹ�ǰ ���۸���Ʈ Ȱ��/��Ȱ��ȭ ��ư
    [SerializeField] private Image m_ResultItemImage; //���۰�� ������ �̹���
    [SerializeField] private NeedItemSlot[] m_NeedMaterials; //���ۿ� �ʿ��� �����۸���Ʈ
    [SerializeField] private Text[] m_CurInventoryItemQuantity; //���ۿ� �ʿ��� �����۸���Ʈ�� ���� �κ��丮�� ����
    private int CurSelectedItemUniquNumber; //���� ���õ� �������� ������ȣ

    private void Awake()
    {
        m_CloseWindowButton.onClick.AddListener(DisActiveWindow);

    }




    private void DisActiveWindow() //â ��Ȱ��ȭ
    {
        gameObject.SetActive(false);
    }


}
