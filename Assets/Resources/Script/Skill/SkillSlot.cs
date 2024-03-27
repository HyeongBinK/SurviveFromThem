using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{ 
    [SerializeField] private int SkillUniqueNumber; //��ų������ȣ
    [SerializeField] private Image SkillImage; //��ų�� �̹���
    public Sprite GetSkillImage { get { return SkillImage.sprite; } }
    [SerializeField] private Image LockImage; //������ 0�϶� Ȱ��ȭ ���̿� ��Ȱ��ȭ
    [SerializeField] private Text SkillNameText; //��ų�� �̸� �ؽ�Ʈ
    [SerializeField] private Text SkillLevelText; //��ų�� ���� �ؽ�Ʈ
    [SerializeField] private Button SkillLevelPlusButton; //��ų��������ư
    private ToolTip m_Tooltip; //����
    private DraggedObject Dragobject; //�巡�� �̵�, ��ȯ ��� �̿�� Ȱ��ȭ�Ǵ� ������Ʈ 
    private SKILLTYPE m_eSkillType; // ��Ƽ�� ��ų�ΰ� �нú� ��ų�ΰ�
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
    public void OnPointerEnter(PointerEventData eventData) //���콺�� ������(����)�� �������� ����ǥ�� �׸��� �巡������ ������Ʈ�� �������� ��������
    {
        if (Dragobject.IsDrag)
            return;
        var Position = gameObject.transform.position;
        m_Tooltip.SetToolTipText(DataTableManager.Instance.MakeSkillTooltipText(SkillUniqueNumber, GameManager.Instance.GetPlayerData.GetPlayerStatusData.GetSkillLevel((SKILLNAME)SkillUniqueNumber)));

        m_Tooltip.SetToolTipPosition(Position.x, Position.y);
        m_Tooltip.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) //���콺�� ������(����)���� �־������� ���� ��Ȱ��ȭ
    {
        m_Tooltip.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData) //���콺�� ������(����)�� ��������
    {
        if (!GameManager.Instance) return;
        if (!DataTableManager.Instance) return;
        if (m_eSkillType == SKILLTYPE.PASSIVE) return; // �нú� ��ų�� ��� �ٷ�����
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
