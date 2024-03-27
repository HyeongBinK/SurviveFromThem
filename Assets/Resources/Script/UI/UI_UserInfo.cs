using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class UI_UserInfo : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    //스테이터스창에서 장비창의 기능을 겸하기위해 던전앤파이터의 스테이터스창 양식을 참고함

    [SerializeField] private WeaponSlot m_MainWeaponSlot; //메인장비슬롯
    [SerializeField] private WeaponSlot m_SubWeaponSlot; //서브장비슬롯
    [SerializeField] private Image m_UserImage; //가운데의 유저 전신 이미지
    [SerializeField] private Text m_UserStatusInformation; //유저의 스테이터스 정보
    private Vector2 m_Transform; //화면상 위치
    private Vector2 m_DefaultPosition; //디폴트위치

    private void Awake()
    {
        m_DefaultPosition = gameObject.transform.position;
        m_Transform = m_DefaultPosition;
        m_MainWeaponSlot.Init();
        m_SubWeaponSlot.Init();
        SubWeaponDisActiveImage();
    }
    private void OnEnable()
    {
        SetUserStatusDataWindow();
        RefreshUserInfo();
    }
    private string MakeStatusData()
    {
        if (!WeaponManager.Instance) return null;
        if (!GameManager.Instance) return null;
        var PlayerData = GameManager.Instance.GetPlayerData.GetPlayerStatusData;
        int WeaponTotalAtk = WeaponManager.Instance.GetCurActiveWeapon().TotalATK;
        int PlayerATKUPPassiveSkillLevel = GameManager.Instance.GetPlayerData.GetPlayerAtkUpSkillLevel;
        float MinAtk = (WeaponTotalAtk * GameManager.Instance.m_fRandomDamageMin) * (1.0f + PlayerATKUPPassiveSkillLevel * 0.01f);
        float MaxAtk = (WeaponTotalAtk * GameManager.Instance.m_fRandomDamageMax) * (1.0f + PlayerATKUPPassiveSkillLevel * 0.01f);

        StringBuilder TextMaker = new StringBuilder();
        //Name
        TextMaker.Append(" Name : ");
        TextMaker.AppendLine(PlayerData.Name);
        //Level
        TextMaker.Append(" Level : ");
        TextMaker.AppendLine(PlayerData.Level.ToString());
        //HP
        TextMaker.Append(" HP : ");
        TextMaker.Append(PlayerData.CurHP.ToString());
        TextMaker.Append("/");
        TextMaker.AppendLine(PlayerData.MaxHP.ToString());
        //MP
        TextMaker.Append(" MP : ");
        TextMaker.Append(PlayerData.CurMP.ToString());
        TextMaker.Append("/");
        TextMaker.AppendLine(PlayerData.MaxMP.ToString());
        //EXP
        TextMaker.Append(" EXP : ");
        TextMaker.Append(PlayerData.CurEXP.ToString());
        TextMaker.Append("/");
        TextMaker.AppendLine(PlayerData.MaxEXP.ToString());
        //ATK
        TextMaker.Append(" ATK : ");
        TextMaker.Append(((int)MinAtk).ToString());
        TextMaker.Append(" ~ ");
        TextMaker.AppendLine(((int)MaxAtk).ToString());
        //CriticalRate
        TextMaker.Append(" CriticalRate : ");
        TextMaker.Append(GameManager.Instance.GetPlayerData.m_iCriticalRate.ToString());
        TextMaker.Append("%");
        return TextMaker.ToString();
    }

    public void SetUserStatusDataWindow()
    {
        m_UserStatusInformation.text = MakeStatusData();
    }

    public void RefreshUserInfo()
    {
        m_MainWeaponSlot.SetSlotData();
        m_SubWeaponSlot.SetSlotData();
        WeaponOnDisActiveImage();
        SetUserStatusDataWindow();
    }
    private void MainWeaponDisActiveImage()
    {
        m_MainWeaponSlot.OnDisActiveImage();
        m_SubWeaponSlot.OffDisActiveImage();
    }
    private void SubWeaponDisActiveImage()
    {
        m_MainWeaponSlot.OffDisActiveImage();
        m_SubWeaponSlot.OnDisActiveImage();
    }
    public void WeaponOnDisActiveImage() //메인 서브장비중 현재 활성화중이지 않은 장비의 이미지를 어둡게
    {
        if (WeaponManager.Instance)
        {
            if (WeaponManager.Instance.m_bIsMainWeapon)
                SubWeaponDisActiveImage();
            else
                MainWeaponDisActiveImage();
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        m_Transform = gameObject.transform.position;
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("DragStart");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.transform.position = m_Transform;
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("DragEnd");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Vector2 CurrentPosition = eventData.position;
            this.transform.position = CurrentPosition;
            m_Transform = CurrentPosition;
        }
    }
}
