using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{ 
    [SerializeField] private int SkillUniqueNumber; //스킬고유번호
    [SerializeField] private Image SkillImage; //스킬의 이미지
    public Sprite GetSkillImage { get { return SkillImage.sprite; } }
    [SerializeField] private Image LockImage; //레벨이 0일때 활성화 그이외 비활성화
    [SerializeField] private Text SkillNameText; //스킬의 이름 텍스트
    [SerializeField] private Text SkillLevelText; //스킬의 레벨 텍스트
    [SerializeField] private Button SkillLevelPlusButton; //스킬레벨업버튼
    private ToolTip m_Tooltip; //툴팁
    private DraggedObject Dragobject; //드래그 이동, 교환 기능 이용시 활성화되는 오브젝트 
    private SKILLTYPE m_eSkillType; // 액티브 스킬인가 패시브 스킬인가
    private void Awake()
    {
        if (!DataTableManager.Instance) return;
        
        var SkillData = DataTableManager.Instance.GetSkillData(SkillUniqueNumber);
        m_eSkillType = SkillData.SkillType;
        m_Tooltip = UIManager.Instance.GetTooltip;
        Dragobject = UIManager.Instance.GetDraggedObject;
        SkillImage.enabled = true;
        SkillImage.sprite = Resources.Load<Sprite>("UI/SkillImage/" + SkillData.SKillImageName);

        SkillNameText.text = SkillData.SkillName;
        SkillLevelPlusButton.onClick.AddListener(ClickSkillLevelUpButton);
        SetSKillLevelText();
    }
    private void OnEnable()
    {
        Refresh();
    }
    private void OnDisable()
    {
        if (m_Tooltip.gameObject.activeSelf && m_Tooltip.transform.position == gameObject.transform.position)
            m_Tooltip.gameObject.SetActive(false);
    }
    public void Refresh()
    {
        if(UIManager.Instance)
        {
            if(UIManager.Instance.GetUserInfo.gameObject.activeSelf)
                UIManager.Instance.GetUserInfo.RefreshUserInfo();
        }
        SetSKillLevelText();
    }
    private void SetSKillLevelText()
    {
        if (!DataTableManager.Instance) return;
        if (!GameManager.Instance) return;

        StringBuilder TextMaker = new StringBuilder();
        var SkillData = DataTableManager.Instance.GetSkillData(SkillUniqueNumber);
        int SkillLevel = GameManager.Instance.GetPlayerData.GetPlayerStatusData.GetSkillLevel((SKILLNAME)SkillUniqueNumber);
        TextMaker.Append(SkillLevel.ToString());
        TextMaker.Append("/");
        TextMaker.Append(SkillData.SkillMaxLevel);

        SkillLevelText.text = TextMaker.ToString();
        if (SkillLevel > 0)
            LockImage.gameObject.SetActive(false);
        
    }

    private void ClickSkillLevelUpButton()
    {
        if (!GameManager.Instance) return;
        GameManager.Instance.GetPlayerData.GetPlayerStatusData.SkillLevelUp((SKILLNAME)SkillUniqueNumber);
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("MouseClick");
        Refresh();
    }
    public void OnPointerEnter(PointerEventData eventData) //마우스가 포인터(슬롯)에 들어왔을때 툴팁표시 그리고 드래그중인 오브젝트가 들어왔을떄 정보전달
    {
        if (Dragobject.IsDrag)
            return;
        var Position = gameObject.transform.position;
        m_Tooltip.SetToolTipText(DataTableManager.Instance.MakeSkillTooltipText(SkillUniqueNumber, GameManager.Instance.GetPlayerData.GetPlayerStatusData.GetSkillLevel((SKILLNAME)SkillUniqueNumber)));

        m_Tooltip.SetToolTipPosition(Position.x, Position.y);
        m_Tooltip.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) //마우스가 포인터(슬롯)에서 멀어졌을떄 툴팁 비활성화
    {
        m_Tooltip.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData) //마우스가 포인터(슬롯)을 눌럿을떄
    {
        if (!GameManager.Instance) return;
        if (!DataTableManager.Instance) return;
        if (m_eSkillType == SKILLTYPE.PASSIVE) return; // 패시브 스킬의 경우 바로종료
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            int SkillLevel = GameManager.Instance.GetPlayerData.GetPlayerStatusData.GetSkillLevel((SKILLNAME)SkillUniqueNumber);
            if (SkillLevel == 0) return;
            Dragobject.SetStartSlotData(DATATYPE.SKILL, SkillUniqueNumber, SkillImage.sprite);
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
