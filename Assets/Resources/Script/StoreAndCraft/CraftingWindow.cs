using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingWindow : MonoBehaviour
{
    [SerializeField] private Button m_CraftButton; //아이템제작버튼
    [SerializeField] private Button m_CloseWindowButton; //아이템 제작창 닫기 버튼
    [SerializeField] private Button m_ShowRifleListButton; //라이플 제작리스트 활성/비활성 버튼
    [SerializeField] private Button m_ShowPistolListButton; //권총 제작리스트 활성/비활성 버튼
    [SerializeField] private Button m_ShowShotgunListButton; //샷건 제작리스트 활성/비활성 버튼
    [SerializeField] private Button m_ShowLazergunListButton; //레이져건 제작리스트 활성/비활성 버튼
    [SerializeField] private Button m_ShowConsumptionListButton; //소모품 제작리스트 활성/비활성화 버튼
    [SerializeField] private Image m_ResultItemImage; //제작결과 아이템 이미지
    [SerializeField] private NeedItemSlot[] m_NeedMaterials; //제작에 필요한 아이템리스트
    [SerializeField] private Text[] m_CurInventoryItemQuantity; //제작에 필요한 아이템리스트의 현재 인벤토리내 숫자
    private int CurSelectedItemUniquNumber; //현재 선택된 아이템의 고유번호

    private void Awake()
    {
        m_CloseWindowButton.onClick.AddListener(DisActiveWindow);

    }




    private void DisActiveWindow() //창 비활성화
    {
        gameObject.SetActive(false);
    }


}
