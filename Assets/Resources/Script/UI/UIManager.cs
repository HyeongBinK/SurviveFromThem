using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class UIManager : MonoBehaviour
{
    private static UIManager m_instance; // 싱글톤이 할당될 변수
    // 싱글톤 접근용 프로퍼티
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

    //메시지박스
    [SerializeField] private Text m_SystemLog; //시스템로그창
    [SerializeField] private ScrollRect m_ScrollRect; //시스템로그 스크롤
    private readonly int m_FontSize = 24; //시스템로그 폰트사이즈
    private readonly int m_FontOffset = 10; //시스템로그 폰트오프셋
    //피격이펙트
    [SerializeField] private PlayerHitEffect m_PlayerHitEffect; //플레이어가 피격당하면 화면이 붉게변하는 이펙트
    //유저의 데이터 표시창들
    [SerializeField] private UI_UserData_1 m_UserDataWindow;//유저의 체력,마력,경험치,이름,레벨 정보
    [SerializeField] private UI_Inventory m_InventoryWindow; //인벤토리 창
    [SerializeField] private UI_UserInfo m_UserInformationWindow; //유저장비창+상세정보 창
    [SerializeField] private UI_SkillWindow m_SkillWindow; //유저의 스킬정보창
    [SerializeField] private QuickSlotManager m_QuickSlots; //퀵슬롯들
    public UI_Inventory GetInventoryWindow { get { return m_InventoryWindow; } }
    public UI_UserData_1 GetUserDataUI { get { return m_UserDataWindow; } }
    public UI_UserInfo GetUserInfo { get { return m_UserInformationWindow; } }
    public UI_SkillWindow GetSkillWindow { get { return m_SkillWindow; } }
    public QuickSlotManager GetQuickSlots { get { return m_QuickSlots; } }

    [SerializeField] private Text m_AmmoText; //총알갯수 텍스트
    //사망이펙트
    [SerializeField] private UI_DeadFade m_DeadFade; //사망시 페이드이펙트
    //데미지 텍스트
    [SerializeField] private FloatingDamageText m_DamageTexts_Prefab; //데미지텍스트프리팹
    private Queue<FloatingDamageText> m_DamageTextsPooling = new Queue<FloatingDamageText>(); //데미지텍스트풀링
    private List<FloatingDamageText> m_ActivatedFloatingDamageText = new List<FloatingDamageText>(); //현재 활성화 되있는 데미지텍스트들
    [SerializeField] private Transform m_DamageTextPoolingLocation; // 풀링된 데미지텍스트 게임상 위치(폴더)
    private readonly int m_MaxNumberOfPooling = 100; //미리 풀링해둘 양
    private int m_iCurAssignedDamageUI; //현재 할당된 데미지UI의 갯수 
    //툴팁
    [SerializeField] private ToolTip m_ToolTip;
    public ToolTip GetTooltip { get { return m_ToolTip; } }
    //드래그 오브젝트
    [SerializeField] private DraggedObject m_DraggedObject;
    public DraggedObject GetDraggedObject { get { return m_DraggedObject; } }
    //맵이름 
    [SerializeField] private Text m_MapName;
    private readonly float MapNameLifeTime = 1.0f;
    //메뉴
    [SerializeField] private Menu m_Menu;
    private void Awake()
    {
        // 씬에 싱글톤 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        if (Instance != this)
        {
            // 자신을 파괴
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        m_iCurAssignedDamageUI = 0;
        MakeDamageTextPoolingList();
    }
    //Fade 효과
    [SerializeField] private FadeEffect m_FadeEffect; //페이드 효과 

    public void SetSystemLog(List<string> Logs) //시스템로그의 텍스트 위치 자동이동
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

    public void SetAmmoText(int CurAmmo, int MaxAmmo) //총알 갯수 셋팅
    {
        StringBuilder TextMaker = new StringBuilder();
        TextMaker.Append("Ammo ");
        TextMaker.Append(CurAmmo);
        TextMaker.Append("/");
        TextMaker.Append(MaxAmmo);
        m_AmmoText.text = TextMaker.ToString();
    }

    // 오브젝트 위치를 화면상 좌표로 변환하여 UI에 표시
    public void MakeDamageTextToPoolingList() //풀링리스트에 하나 추가
    {
        FloatingDamageText NewDamageText = Instantiate(m_DamageTexts_Prefab, m_DamageTextPoolingLocation);
        m_DamageTextsPooling.Enqueue(NewDamageText);
        NewDamageText.gameObject.SetActive(false);
    }

    private void MakeDamageTextPoolingList() //풀링리스트 생성
    {
        for (int i = 0; i < m_MaxNumberOfPooling; i++)
        {
            MakeDamageTextToPoolingList();
        }
    }

    public void CreateDamageUI(int Damage, bool IsCritical, Vector2 Position) //화면에 데미지텍스트 생성
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

    //장비창, 스킬창, 인벤토리 가 활성화 되어있을때 플레이어를 못움직이게 하고 마우스커서를 활성화 시키는 기능
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
