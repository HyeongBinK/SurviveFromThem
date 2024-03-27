using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//인벤토리 구현전까지 사용할 임시 무기변경 스크립트
//후에 메인/서브 무기 해서 교체사용으로 활용될시 재사용될가능성 있음
public class WeaponManager : MonoBehaviour
{
    private static WeaponManager m_instance; //싱글톤 할당

    public static WeaponManager Instance
    {
        get
        {
            if (m_instance == null)
                m_instance = FindObjectOfType<WeaponManager>();
            return m_instance;
        }
    }

    [SerializeField] private Weapon[] Weapons; //무기외형+무기코드

    private WeaponData Cur_MainWeapon = new WeaponData(); //현재 장착 중인 메인 장비
    public WeaponData GetMainWeapon { get { return Cur_MainWeapon; }  }
    private WeaponData Cur_SubWeapon = new WeaponData(); //현재 장착 중인 서브 장비
    public WeaponData GetSubWeapon { get { return Cur_SubWeapon; } }

    public bool m_bIsMainWeaponData; //메인장비데이터가 있는가
    public bool m_bIsSubWeaponData; //서브장비데이터가 있는가
    public bool m_bIsMainWeapon; //현재 활성화중인 장비가 메인장비인가
    public bool m_bCanUseWeapon { get; private set; } //무기를 사용할 수 있는 상태인가?
/*    public int m_iMainWeaponRemainAmmo; //메인무기의 남아있는 총알수
    public int m_iSubWeaponRemainAmmo; //서브무기의 남아있는 총알수*/

    private void Awake()
    {
        // 씬에 싱글톤 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        if (Instance != this)
        {
            // 자신을 파괴
            Destroy(gameObject);
        }
        m_bIsMainWeapon = true;
        DefaultSetting();
    }

    private void DefaultSetting()
    {
        if (!DataTableManager.Instance) return;
        Cur_MainWeapon = DataTableManager.Instance.GetWeaponData(5);
        Cur_SubWeapon = DataTableManager.Instance.GetWeaponData(6);
     /*   m_iMainWeaponRemainAmmo = DataTableManager.Instance.GetWeaponData(5).MaxAmmo;
        m_iSubWeaponRemainAmmo = DataTableManager.Instance.GetWeaponData(6).MaxAmmo;*/
        m_bIsMainWeaponData = true;
        m_bIsSubWeaponData = true;
        m_bCanUseWeapon = true;
        if (UIManager.Instance)
            UIManager.Instance.GetUserInfo.RefreshUserInfo();
    }
    public bool SetMainWeaponData(ItemSlotData Data, int ReturnWeaponSlotNumber = -1) //아이템의정보 무기가 되돌아갈경우 되돌아갈 슬롯의 번호
    {
        if (!GameManager.Instance) return false;
        if (!DataTableManager.Instance) return false;
       
        if(!Data.IsData) //장비해체를 할 경우
        {
            if (m_bIsMainWeaponData)
            {
                if (GameManager.Instance.GetPlayerData.GetInventory.AddItem(ITEMTYPE.WEAPON, Cur_MainWeapon.WeaponUniqueNumber, Cur_MainWeapon.Reinforce))
                {
                    m_bIsMainWeaponData = false;
                    Cur_MainWeapon = DataTableManager.Instance.GetWeaponData(0); ;
                    if (UIManager.Instance)
                        UIManager.Instance.GetUserInfo.RefreshUserInfo();
                  //  m_iMainWeaponRemainAmmo = DataTableManager.Instance.GetWeaponData(0).MaxAmmo;
                    ActiveWeapon();
                    return true;
                }
                else //되돌리지못했을때 기존의 데이터를 그대로 놔두고 함수종료
                    return false;
            }
            else
                return false;
        }
        var NewWeaponData = DataTableManager.Instance.GetWeaponData(Data.ItemUniqueNumber);
        if (NewWeaponData.WeaponType == WEAPONTYPE.PISTOL) return false; 

        if (m_bIsMainWeaponData)
        {
           if(!GameManager.Instance.GetPlayerData.GetInventory.AddItemWithSlot(ITEMTYPE.WEAPON, Cur_MainWeapon.WeaponUniqueNumber, Cur_MainWeapon.Reinforce, ReturnWeaponSlotNumber))
                return false; //되돌리지 못하였을때
        }
        m_bIsMainWeaponData = true;
        Cur_MainWeapon = NewWeaponData;
        Cur_MainWeapon.SetReinforce(Data.Value);
     //   m_iMainWeaponRemainAmmo = NewWeaponData.MaxAmmo;
        if (UIManager.Instance)
            UIManager.Instance.GetUserInfo.RefreshUserInfo();
        ActiveWeapon();
        return true;
    }

    public bool SetSubWeaponData(ItemSlotData Data, int ReturnWeaponSlotNumber = -1) //아이템의정보 무기가 되돌아갈경우 되돌아갈 슬롯의 번호
    {
        if (!GameManager.Instance) return false;
        if (!DataTableManager.Instance) return false;
        if (!Data.IsData)
        {
            if (m_bIsSubWeaponData)
            {
                if (GameManager.Instance.GetPlayerData.GetInventory.AddItem(ITEMTYPE.WEAPON, Cur_SubWeapon.WeaponUniqueNumber, Cur_SubWeapon.Reinforce))
                {
                    m_bIsSubWeaponData = false;
                    Cur_SubWeapon = DataTableManager.Instance.GetWeaponData(1);
                    if (UIManager.Instance)
                        UIManager.Instance.GetUserInfo.RefreshUserInfo();
               //     m_iSubWeaponRemainAmmo = DataTableManager.Instance.GetWeaponData(1).MaxAmmo;
                    ActiveWeapon();
                    return true;
                }
                else //되돌리지못했을때 기존의 데이터를 그대로 놔두고 함수종료
                    return false;
            }
            else
                return false;
        }
        var NewWeaponData = DataTableManager.Instance.GetWeaponData(Data.ItemUniqueNumber);

        if (NewWeaponData.WeaponType != WEAPONTYPE.PISTOL) return false;

        if (m_bIsSubWeaponData)
        {
            if (!GameManager.Instance.GetPlayerData.GetInventory.AddItemWithSlot(ITEMTYPE.WEAPON, Cur_SubWeapon.WeaponUniqueNumber, Cur_SubWeapon.Reinforce, ReturnWeaponSlotNumber))
                return false; //되돌리지 못하였을때
        }

        m_bIsSubWeaponData = true;
        Cur_SubWeapon = NewWeaponData;
        Cur_SubWeapon.SetReinforce(Data.Value);
     //   m_iSubWeaponRemainAmmo = NewWeaponData.MaxAmmo;
        if (UIManager.Instance)
            UIManager.Instance.GetUserInfo.RefreshUserInfo();
        ActiveWeapon();
        return true;
    }

    private void ActiveWeapon()
    {
        bool IsData = false;
        var WeaponType = GetCurActiveWeapon().WeaponType;
        if (GetCurActiveWeapon().WeaponUniqueNumber == -1)
            return;

        for (int i = 0; i < Weapons.Length; i++)
        {
          /*  if (Weapons[i].gameObject.activeSelf) //활성화 중인 무기의 경우
            {
                if (GetCurActiveWeapon() == Cur_MainWeapon)
                {
                    m_iSubWeaponRemainAmmo = Weapons[i].m_iCurAmmo;
                }
                else
                {
                    m_iMainWeaponRemainAmmo = Weapons[i].m_iCurAmmo;
                }
            }*/

            Weapons[i].gameObject.SetActive(false);
            if (i == (int)WeaponType)
            {
                IsData = true;
            }
        }
        if (IsData)
        {
            Weapons[(int)WeaponType].SetWeaponStatusData(GetCurActiveWeapon());
            Weapons[(int)WeaponType].gameObject.SetActive(true);
        }
    }
    public WeaponData GetCurActiveWeapon()
    {
        if (m_bIsMainWeapon)
            return Cur_MainWeapon;
        else
            return Cur_SubWeapon;
    }
    public void ActiveMainWeapon()
    {
        m_bIsMainWeapon = true;
        ActiveWeapon();
        if (UIManager.Instance)
        {
            if (UIManager.Instance.GetUserInfo.gameObject.activeSelf)
            {
                UIManager.Instance.GetUserInfo.SetUserStatusDataWindow();
                UIManager.Instance.GetUserInfo.WeaponOnDisActiveImage();
            }
        }
    }
    public void ActiveSubWeapon()
    {
        m_bIsMainWeapon = false;
        ActiveWeapon();
        if (UIManager.Instance)
        {
            if (UIManager.Instance.GetUserInfo.gameObject.activeSelf)
            {
                UIManager.Instance.GetUserInfo.SetUserStatusDataWindow();
                UIManager.Instance.GetUserInfo.WeaponOnDisActiveImage();
            }
        }
    }
    public void SetCanUseWeapon(bool NewBoolean)
    {
        m_bCanUseWeapon = NewBoolean;
        if(!m_bCanUseWeapon)
        {
            SetCurActivatedWeaponIsShowCrosshair(false);
        }
        else
        {
            if(GameManager.Instance.m_bIsZoomIn)
                SetCurActivatedWeaponIsShowCrosshair(true);
        }
    }
    private void SetCurActivatedWeaponIsShowCrosshair(bool NewBoolean)
    {
        for (int i = 0; i < Weapons.Length; i++)
        {
            if (Weapons[i].gameObject.activeSelf) //활성화 중인 무기의 경우
            {
                Weapons[i].SetShowCrossHair(NewBoolean);
                return;
            }
        }
    }
  /*  public int GetRemainAmmo()
    {
        if (m_bIsMainWeapon)
            return m_iMainWeaponRemainAmmo;
        else
            return m_iSubWeaponRemainAmmo;
    }*/
}
