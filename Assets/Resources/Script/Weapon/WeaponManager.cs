using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�κ��丮 ���������� ����� �ӽ� ���⺯�� ��ũ��Ʈ
//�Ŀ� ����/���� ���� �ؼ� ��ü������� Ȱ��ɽ� ����ɰ��ɼ� ����
public class WeaponManager : MonoBehaviour
{
    private static WeaponManager m_instance; //�̱��� �Ҵ�

    public static WeaponManager Instance
    {
        get
        {
            if (m_instance == null)
                m_instance = FindObjectOfType<WeaponManager>();
            return m_instance;
        }
    }

    [SerializeField] private Weapon[] Weapons; //�������+�����ڵ�

    private WeaponData Cur_MainWeapon = new WeaponData(); //���� ���� ���� ���� ���
    public WeaponData GetMainWeapon { get { return Cur_MainWeapon; }  }
    private WeaponData Cur_SubWeapon = new WeaponData(); //���� ���� ���� ���� ���
    public WeaponData GetSubWeapon { get { return Cur_SubWeapon; } }

    public bool m_bIsMainWeaponData; //����������Ͱ� �ִ°�
    public bool m_bIsSubWeaponData; //����������Ͱ� �ִ°�
    public bool m_bIsMainWeapon; //���� Ȱ��ȭ���� ��� ��������ΰ�
    public bool m_bCanUseWeapon { get; private set; } //���⸦ ����� �� �ִ� �����ΰ�?
/*    public int m_iMainWeaponRemainAmmo; //���ι����� �����ִ� �Ѿ˼�
    public int m_iSubWeaponRemainAmmo; //���깫���� �����ִ� �Ѿ˼�*/

    private void Awake()
    {
        // ���� �̱��� ������Ʈ�� �� �ٸ� GameManager ������Ʈ�� �ִٸ�
        if (Instance != this)
        {
            // �ڽ��� �ı�
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
    public bool SetMainWeaponData(ItemSlotData Data, int ReturnWeaponSlotNumber = -1) //������������ ���Ⱑ �ǵ��ư���� �ǵ��ư� ������ ��ȣ
    {
        if (!GameManager.Instance) return false;
        if (!DataTableManager.Instance) return false;
       
        if(!Data.IsData) //�����ü�� �� ���
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
                else //�ǵ������������� ������ �����͸� �״�� ���ΰ� �Լ�����
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
                return false; //�ǵ����� ���Ͽ�����
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

    public bool SetSubWeaponData(ItemSlotData Data, int ReturnWeaponSlotNumber = -1) //������������ ���Ⱑ �ǵ��ư���� �ǵ��ư� ������ ��ȣ
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
                else //�ǵ������������� ������ �����͸� �״�� ���ΰ� �Լ�����
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
                return false; //�ǵ����� ���Ͽ�����
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
          /*  if (Weapons[i].gameObject.activeSelf) //Ȱ��ȭ ���� ������ ���
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
            if (Weapons[i].gameObject.activeSelf) //Ȱ��ȭ ���� ������ ���
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
