using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class UIManager : MonoBehaviour
{
    private static UIManager m_instance; // �̱����� �Ҵ�� ����
    // �̱��� ���ٿ� ������Ƽ
    public static UIManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<UIManager>();
                DontDestroyOnLoad(m_instance.gameObject);
            }
            return m_instance;
        }
    }

    //�޽����ڽ�
    [SerializeField] private Text m_SystemLog; //�ý��۷α�â
    [SerializeField] private ScrollRect m_ScrollRect; //�ý��۷α� ��ũ��
    private readonly int m_FontSize = 24; //�ý��۷α� ��Ʈ������
    private readonly int m_FontOffset = 10; //�ý��۷α� ��Ʈ������
    //�ǰ�����Ʈ
    [SerializeField] private PlayerHitEffect m_PlayerHitEffect; //�÷��̾ �ǰݴ��ϸ� ȭ���� �ӰԺ��ϴ� ����Ʈ
    //������ ������ ǥ��â��
    [SerializeField] private UI_UserData_1 m_UserDataWindow;//������ ü��,����,����ġ,�̸�,���� ����
    [SerializeField] private UI_Inventory m_InventoryWindow; //�κ��丮 â
    [SerializeField] private UI_UserInfo m_UserInformationWindow; //�������â+������ â
    [SerializeField] private UI_SkillWindow m_SkillWindow; //������ ��ų����â
    [SerializeField] private QuickSlotManager m_QuickSlots; //�����Ե�
    public UI_Inventory GetInventoryWindow { get { return m_InventoryWindow; } }
    public UI_UserData_1 GetUserDataUI { get { return m_UserDataWindow; } }
    public UI_UserInfo GetUserInfo { get { return m_UserInformationWindow; } }
    public UI_SkillWindow GetSkillWindow { get { return m_SkillWindow; } }
    public QuickSlotManager GetQuickSlots { get { return m_QuickSlots; } }

    [SerializeField] private Text m_AmmoText; //�Ѿ˰��� �ؽ�Ʈ
    //�������Ʈ
    [SerializeField] private UI_DeadFade m_DeadFade; //����� ���̵�����Ʈ
    //������ �ؽ�Ʈ
    [SerializeField] private FloatingDamageText m_DamageTexts_Prefab; //�������ؽ�Ʈ������
    private Queue<FloatingDamageText> m_DamageTextsPooling = new Queue<FloatingDamageText>(); //�������ؽ�ƮǮ��
    private List<FloatingDamageText> m_ActivatedFloatingDamageText = new List<FloatingDamageText>(); //���� Ȱ��ȭ ���ִ� �������ؽ�Ʈ��
    [SerializeField] private Transform m_DamageTextPoolingLocation; // Ǯ���� �������ؽ�Ʈ ���ӻ� ��ġ(����)
    private readonly int m_MaxNumberOfPooling = 100; //�̸� Ǯ���ص� ��
    private int m_iCurAssignedDamageUI; //���� �Ҵ�� ������UI�� ���� 
    //����
    [SerializeField] private ToolTip m_ToolTip;
    public ToolTip GetTooltip { get { return m_ToolTip; } }
    //�巡�� ������Ʈ
    [SerializeField] private DraggedObject m_DraggedObject;
    public DraggedObject GetDraggedObject { get { return m_DraggedObject; } }
    //���̸� 
    [SerializeField] private Text m_MapName;
    private readonly float MapNameLifeTime = 1.0f;
    //�޴�
    [SerializeField] private Menu m_Menu;
    private void Awake()
    {
        // ���� �̱��� ������Ʈ�� �� �ٸ� GameManager ������Ʈ�� �ִٸ�
        if (Instance != this)
        {
            // �ڽ��� �ı�
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        m_iCurAssignedDamageUI = 0;
        MakeDamageTextPoolingList();
    }
    //Fade ȿ��
    [SerializeField] private FadeEffect m_FadeEffect; //���̵� ȿ�� 

    public void SetSystemLog(List<string> Logs) //�ý��۷α��� �ؽ�Ʈ ��ġ �ڵ��̵�
    {
        var text = new System.Text.StringBuilder();
        m_SystemLog.rectTransform.sizeDelta = Vector2.up * (Logs.Count * m_FontSize + m_FontOffset);
        for (int i = 0; Logs.Count > i; i++)
        {
            text.AppendLine(Logs[i]);
        }
        m_SystemLog.text = text.ToString();

        m_ScrollRect.verticalNormalizedPosition = 0;
    }

    public void SetAmmoText(int CurAmmo, int MaxAmmo) //�Ѿ� ���� ����
    {
        StringBuilder TextMaker = new StringBuilder();
        TextMaker.Append("Ammo ");
        TextMaker.Append(CurAmmo);
        TextMaker.Append("/");
        TextMaker.Append(MaxAmmo);
        m_AmmoText.text = TextMaker.ToString();
    }

    // ������Ʈ ��ġ�� ȭ��� ��ǥ�� ��ȯ�Ͽ� UI�� ǥ��
    public void MakeDamageTextToPoolingList() //Ǯ������Ʈ�� �ϳ� �߰�
    {
        FloatingDamageText NewDamageText = Instantiate(m_DamageTexts_Prefab, m_DamageTextPoolingLocation);
        m_DamageTextsPooling.Enqueue(NewDamageText);
        NewDamageText.gameObject.SetActive(false);
    }

    private void MakeDamageTextPoolingList() //Ǯ������Ʈ ����
    {
        for (int i = 0; i < m_MaxNumberOfPooling; i++)
        {
            MakeDamageTextToPoolingList();
        }
    }

    public void CreateDamageUI(int Damage, bool IsCritical, Vector2 Position) //ȭ�鿡 �������ؽ�Ʈ ����
    {
        if (m_iCurAssignedDamageUI < m_MaxNumberOfPooling)
        {
            if (m_DamageTextsPooling.Count <= 0) MakeDamageTextToPoolingList();

            FloatingDamageText NewDamageText = m_DamageTextsPooling.Dequeue();
            m_ActivatedFloatingDamageText.Add(NewDamageText);

            if (NewDamageText)
            {
                NewDamageText.Init(Damage, IsCritical, Position);
                m_iCurAssignedDamageUI++;

                NewDamageText.DamageTextDisable += () => m_ActivatedFloatingDamageText.Remove(NewDamageText);
                NewDamageText.DamageTextDisable += () => m_DamageTextsPooling.Enqueue(NewDamageText);
                NewDamageText.DamageTextDisable += () => m_iCurAssignedDamageUI--;
            }
        }
    }
    public void SetInventoryGold(int NewGold)
    {
        m_InventoryWindow.SetGoldText(NewGold);
    }
    public void OnOffInventoryWindow()
    {
        m_InventoryWindow.gameObject.SetActive(!m_InventoryWindow.gameObject.activeSelf);
        CheckUIActivated();
    }
    public void OnOffUserinfoWindow()
    {
        m_UserInformationWindow.gameObject.SetActive(!m_UserInformationWindow.gameObject.activeSelf);
        CheckUIActivated();
    }
    public void OnOffSkillWindow()
    {
        m_SkillWindow.gameObject.SetActive(!m_SkillWindow.gameObject.activeSelf);
        CheckUIActivated();
    }
    public void OnOffMenu()
    {
        m_Menu.gameObject.SetActive(!m_Menu.gameObject.activeSelf);
        CheckUIActivated();
    }
    public void RefreshSkillWindow()
    {
        if (!m_SkillWindow.gameObject.activeSelf) return;
        m_SkillWindow.SetSkillPointText();
        m_SkillWindow.RefreshAllSkillSlot();
    }
    public void ActivePlayerHitEffect()
    {
        m_PlayerHitEffect.ActivePlayerHitEffect();
    }
    public void ActiveDeadFade()
    {
        m_DeadFade.ActiveDeadFade();
    }
    public void ActiveFadeInEffect()
    {
        m_FadeEffect.Fadein();
    }
    public void ActiveFadeOutEffect()
    {
        m_FadeEffect.FadeOut();
    }
    public void ShowMapName(string NewMapName)
    {
        if (m_MapName.gameObject.activeSelf)
        {
            StopCoroutine(StartShowMapName());
        }
        m_MapName.text = NewMapName;
        StartCoroutine(StartShowMapName());
    }
    IEnumerator StartShowMapName()
    {
        if (!m_MapName.gameObject.activeSelf)
            m_MapName.gameObject.SetActive(true);
        yield return new WaitForSeconds(MapNameLifeTime);
        if (m_MapName.gameObject.activeSelf)
            m_MapName.gameObject.SetActive(false);
    }

    //���â, ��ųâ, �κ��丮 �� Ȱ��ȭ �Ǿ������� �÷��̾ �������̰� �ϰ� ���콺Ŀ���� Ȱ��ȭ ��Ű�� ���
    private void CheckUIActivated()
    {
        bool IsUiActivated = false;

        if (m_UserInformationWindow.gameObject.activeSelf)
            IsUiActivated = true;
        if(m_InventoryWindow.gameObject.activeSelf)
            IsUiActivated = true;
        if(m_SkillWindow.gameObject.activeSelf)
            IsUiActivated = true;
        if(m_Menu.gameObject.activeSelf)
            IsUiActivated = true;

        if (IsUiActivated)
            GameManager.Instance.UnlockMouse();
        else
            GameManager.Instance.LockMouse();
    }
}
