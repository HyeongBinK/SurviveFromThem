using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon : MonoBehaviour
{
    //���� ���� ����
    [SerializeField] private int m_iWeaponUniqueNumber; 

    private WeaponData m_WeaponData = new WeaponData(); //���� ����
    public int m_iCurAmmo { get; private set; } //���� ������ �ѿ��� ���� �Ѿ˰���
    private bool m_bCanFire; //���� ��� �ִ��� ������ �� ���� �Ҹ� ����
    private bool m_bIsAutoReload = true; // true : źȯ�� �پ��� �ڵ����� ���ε� false : �������� �ؾߵ� 
    private float m_fFireTimer; //������ݱ����� ���� Ÿ�̸�(��Ÿ��)
    private float m_fActualRateOfFire; //������ �߻�ӵ��� ��������� ���� �߻�ӵ�	
                                          
    [SerializeField] private GameObject m_WeaponModel; //���� ���� Mesh                   
    [SerializeField] private Transform m_RaycastStartSpot; //���������� ���󰡴� �Ѿ� �߻� ��ġ
    [SerializeField] private PROJECTILEPREFAB m_ProjectileType; //źȯ������Ʈ(�̻���, ���ϰ��� ����ü)
    [SerializeField] private Transform m_ProjectileSpawnSpot; //źȯ�� �߻�� ��ġ

    [SerializeField] private bool m_bIsInfiniteAmmo; //źȯ ���� ����(true �Ͻ� ���ε��� �ʿ���� ���Ӿ��� �߻簡��)
    [SerializeField] private bool m_bIsAuto; //true : �ڵ� false : ���ڵ�

    //���ڼ�(CrossHair)
    [SerializeField] private bool m_bIsCrosshair; // ���ڼ��� �ִ��� ������
    [SerializeField] private bool m_bShowCrosshair; // ���ڼ��� ������ ����
    public Texture2D crosshairTexture;// ���ڼ��� ����µ� ���� �ؽ���                  
    public int crosshairLength = 20; //�� ���ڼ��� ����                
    public int crosshairWidth = 4; //�� ���ڼ��� �ʺ�
    public float startingCrosshairSize = 10.0f; //���� ���ڼ� ������ ����(�ȼ� ����)(������ ���߷�)(�ݵ����� ������ ������ źȯ�� �������� ���󰥶�)
    private float currentCrosshairSize; // �ǽð� ���ڼ� ������ ����

    // �ݵ�
    private bool m_bIsRecoil = true; //���� �߻��Ҷ� �ݵ� �߻� ����

    // Burst
    private int m_IBurstCounter = 0; // Counter to keep track of how many shots have been fired per burst
    private float m_fBurstTimer = 0.0f; // Timer to keep track of how long the weapon has paused between bursts

    // ��Ȯ��(Accuracy)              
    private float m_fCurrentAccuracy; //���� ��Ȯ��	

    // �ѱ� �߻� ȿ��
    [SerializeField] private bool m_bIsSpitShells = true; //���� �߻��Ҷ� ����Ǵ� ź�� ��������
    [SerializeField] private Transform m_ShellSpitPosition; //ź�ǰ� ����Ǵ� ��ġ

    private bool m_bMakeMuzzleEffects = true; //���ȿ�� ����Ʈ �߻�����
    [SerializeField] private WEAPONEFFECT MuzzleEffect; //�߻���ų ���ȿ�� 
    [SerializeField] private Transform m_MuzzleEffectsPosition; //���ȿ�� �߻� ��ġ
                                                              
    private bool m_bmakeHitEffects = true; //����ȿ�� ����Ʈ �߻�����			

    // �������۽� 1��Ī(FPS)�� ��ȯ�Ǵ� ȿ��
    private bool m_bIsZoomIn = false;
    private bool m_bIsReloading = false; // ���ε� ���̸� true �ƴҽ� false

    private readonly float m_fAttackRange = 9999.0f; //��ݹ���(���� ������)	
    private readonly KeyCode KeyCodeReload = KeyCode.R; //������Ű
    private readonly KeyCode KeyCodeZoonIn = KeyCode.Mouse1; //����Ű
    private readonly string EnemyTag = "Enemy";
    private void Awake()
    {
        DefaultSetting();
    }

    public void SetWeaponStatusData(WeaponData NewWeaponData)
    {
        if (DataTableManager.Instance)
        {
            WeaponData TempWeaponData = new WeaponData();
            if (m_WeaponData != null)
            {
                TempWeaponData = m_WeaponData;
            }
            m_iWeaponUniqueNumber = NewWeaponData.WeaponUniqueNumber;
            m_WeaponData =  NewWeaponData;
            if(TempWeaponData != NewWeaponData)
                FillAmmoFull();
        }
    }

    private void DefaultSetting()
    {
        if (DataTableManager.Instance)
        {
            m_WeaponData = DataTableManager.Instance.GetWeaponData(m_iWeaponUniqueNumber);
            m_WeaponData.SetReinforce(0);
            FillAmmoFull();
        }
    }

    void OnGUI() //GUI����� ������ �� �� ��������� ����غ��� �; ����߽��ϴ�.
    {
        if (m_WeaponData.FireType == FIRETYPE.PROJECTILE)
        {
            m_fCurrentAccuracy = m_WeaponData.MaxAccuracy;
        }

        if (m_bIsCrosshair)
        {
            if (m_bShowCrosshair)
            {
               
                Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);

                // Left
                Rect leftRect = new Rect(center.x - crosshairLength - currentCrosshairSize, center.y - (crosshairWidth / 2), crosshairLength, crosshairWidth);
                GUI.DrawTexture(leftRect, crosshairTexture, ScaleMode.StretchToFill);
                // Right
                Rect rightRect = new Rect(center.x + currentCrosshairSize, center.y - (crosshairWidth / 2), crosshairLength, crosshairWidth);
                GUI.DrawTexture(rightRect, crosshairTexture, ScaleMode.StretchToFill);
                // Top
                Rect topRect = new Rect(center.x - (crosshairWidth / 2), center.y - crosshairLength - currentCrosshairSize, crosshairWidth, crosshairLength);
                GUI.DrawTexture(topRect, crosshairTexture, ScaleMode.StretchToFill);
                // Bottom
                Rect bottomRect = new Rect(center.x - (crosshairWidth / 2), center.y + currentCrosshairSize, crosshairWidth, crosshairLength);
                GUI.DrawTexture(bottomRect, crosshairTexture, ScaleMode.StretchToFill);
            }    
        }
    }

    private void Start()
    {
        //���ܰ� �߻������ʰԲ�(����ó��)
        if (m_RaycastStartSpot == null)
            m_RaycastStartSpot = gameObject.transform;
        if (m_MuzzleEffectsPosition == null)
            m_MuzzleEffectsPosition = gameObject.transform;
        if (m_ProjectileSpawnSpot == null)
            m_ProjectileSpawnSpot = gameObject.transform;
        if (m_WeaponModel == null)
            m_WeaponModel = gameObject;
        if (crosshairTexture == null)
            crosshairTexture = new Texture2D(0, 0);
    }

    private void OnEnable()
    {
        Init();
        if(GameManager.Instance)
        {
            m_bIsZoomIn = GameManager.Instance.m_bIsZoomIn;
            if (m_bIsZoomIn)
            {
                if(m_bIsCrosshair)
                m_bShowCrosshair = true;
            }
        }
    }

    private void Init()
    {
        currentCrosshairSize = startingCrosshairSize;

        if (m_WeaponData.RateOfFire != 0)
            m_fActualRateOfFire = 1.0f / m_WeaponData.RateOfFire;
        else
            m_fActualRateOfFire = 0.01f;

        // Initialize the current crosshair size variable to the starting value specified by the user
        currentCrosshairSize = startingCrosshairSize;

        // Make sure the fire timer starts at 0
        m_fFireTimer = 0.0f;

        // Start the weapon off with a full magazine
      //  m_iCurAmmo = m_WeaponData.MaxAmmo;
     /*   if (WeaponManager.Instance)
            m_iCurAmmo = WeaponManager.Instance.GetRemainAmmo();*/
        UIManager.Instance.SetAmmoText(m_iCurAmmo, m_WeaponData.MaxAmmo);

        m_bIsReloading = false;
        m_bShowCrosshair = false;
        StartCoroutine(UpdateWeapon());
    }

    private void FillAmmoFull()
    {
        m_iCurAmmo = m_WeaponData.MaxAmmo;
    }
    IEnumerator UpdateWeapon()
    {
        while (gameObject.activeSelf)
        {
            // ��Ȯ�� ���
            m_fCurrentAccuracy = Mathf.Lerp(m_fCurrentAccuracy, m_WeaponData.MaxAccuracy, m_WeaponData.AccuracyRecoverRate * Time.deltaTime);
            // ���ڼ� ������Ʈ
            currentCrosshairSize = startingCrosshairSize + (m_WeaponData.MaxAccuracy - m_fCurrentAccuracy) * 0.8f;

            // Ÿ�̸�
            m_fFireTimer += Time.deltaTime;
            // �÷��̾� Ű�����Է�
            if(WeaponManager.Instance)
            {
                if(WeaponManager.Instance.m_bCanUseWeapon)
                    CheckForUserInput();
            }

            // �ܿ� �Ѿ� ���� Ȯ���ؼ� ������
            if (m_bIsAutoReload && m_iCurAmmo <= 0)
                Reload();

            // �ݵ�ȸ��
            if (m_bIsRecoil)
            {
                m_WeaponModel.transform.position = Vector3.Lerp(m_WeaponModel.transform.position, transform.position, m_WeaponData.RecoilRecoveryRate * Time.deltaTime);
                m_WeaponModel.transform.rotation = Quaternion.Lerp(m_WeaponModel.transform.rotation, transform.rotation, m_WeaponData.RecoilRecoveryRate * Time.deltaTime);
            }

            // Reset the Burst
            if (m_IBurstCounter >= m_WeaponData.m_iBurstRate)
            {
                m_fBurstTimer += Time.deltaTime;
                if (m_fBurstTimer >= m_WeaponData.m_fBurstPause)
                {
                    m_IBurstCounter = 0;
                    m_fBurstTimer = 0.0f;
                }
            }
            yield return null;
        }
    }

    private void CheckAmmo() //źȯ Ȯ��
    {
        if (m_iCurAmmo > 0)
            m_bCanFire = true;
    }
    public void Reload() //������
    {
        if(!m_bIsReloading)
        {
            m_fFireTimer = -m_WeaponData.ReloadTime;
            StartCoroutine(Reloading());
        }
    }

    IEnumerator Reloading()
    {
        m_bIsReloading = true;
        SoundManager.Instance.PlaySFX("Reload");
        yield return new WaitForSeconds(m_WeaponData.ReloadTime);
        m_iCurAmmo = m_WeaponData.MaxAmmo;
        UIManager.Instance.SetAmmoText(m_iCurAmmo, m_WeaponData.MaxAmmo);
        m_bIsReloading = false;
    }

    void Recoil() //�ѱ⸦ �߻��Ҷ� �ݵ�ȿ��
    {
        if (m_bIsRecoil)
        {
            // Calculate random values for the recoil position and rotation
            float kickBack = Random.Range(m_WeaponData.RecoilKickBackMin, m_WeaponData.RecoilKickBackMax);
            float kickRot = Random.Range(m_WeaponData.RecoilRotationMin, m_WeaponData.RecoilKickBackMax);

            // Apply the random values to the weapon's postion and rotation
            m_WeaponModel.transform.Translate(new Vector3(0, 0, -kickBack), Space.Self);
            m_WeaponModel.transform.Rotate(new Vector3(-kickRot, 0, 0), Space.Self);
        }
    }
    void CheckForUserInput() //�÷��̾� �Է�
    {

        // Fire if this is a raycast type weapon and the user presses the fire button
        if (m_fFireTimer >= m_fActualRateOfFire && m_IBurstCounter < m_WeaponData.m_iBurstRate && m_bCanFire)
        {
            if (Input.GetButton("Fire1"))
            {
                switch (m_WeaponData.FireType)
                {
                    case FIRETYPE.RAYCAST:
                        {
                            Fire();
                        }
                        break;
                    case FIRETYPE.PROJECTILE:
                        {
                            Launch();
                        }
                        break;
                    default:
                        Debug.Log("�ѱ�Ÿ�Կ� ���ܹ߻�");
                        break;

                }
            }
        }
        // Reload if the "Reload" button is pressed
        if (Input.GetKeyDown(KeyCodeReload))
            Reload();
        if (Input.GetKeyDown(KeyCodeZoonIn))
            ZoomIn();
        // If the weapon is semi-auto and the user lets up on the button, set canFire to true
        if (Input.GetButtonUp("Fire1"))
            m_bCanFire = true;
    }
    private void ZoomIn()
    {
        if (GameManager.Instance)
        {
            m_bIsZoomIn = GameManager.Instance.m_bIsZoomIn;
            if (m_bIsZoomIn)
            {
                GameManager.Instance.TPSMode();
                m_bShowCrosshair = false;
            }
            else
            {
                GameManager.Instance.FPSMode();
                if (m_bIsCrosshair)
                    m_bShowCrosshair = true;
            }
        }
    }
    public void Fire() //�߻�
    {
        if (!WeaponEffectPoolingManager.Instance) return;
            // Reset the fireTimer to 0 (for ROF)
            m_fFireTimer = 0.0f;

        // Increment the burst counter
        m_IBurstCounter++;

        // If this is a semi-automatic weapon, set canFire to false (this means the weapon can't fire again until the player lets up on the fire button)
        if (!m_bIsAuto)
            m_bCanFire = false;

        // First make sure there is ammo
        if (m_iCurAmmo <= 0)
        {
            DryFire();
            return;
        }

        // Subtract 1 from the current ammo
        if (!m_bIsInfiniteAmmo)
        {
            m_iCurAmmo--;
            UIManager.Instance.SetAmmoText(m_iCurAmmo, m_WeaponData.MaxAmmo);
        }
        // Fire once for each shotPerRound value
        for (int i = 0; i < m_WeaponData.ShotPerRound; i++)
        {
            // Calculate accuracy for this shot
            float accuracyVary = (100 - m_fCurrentAccuracy) / 1000;
            Vector3 direction = m_RaycastStartSpot.forward;

            direction.x += UnityEngine.Random.Range(-accuracyVary, accuracyVary);
            direction.y += UnityEngine.Random.Range(-accuracyVary, accuracyVary);
            direction.z += UnityEngine.Random.Range(-accuracyVary, accuracyVary);
            
            m_fCurrentAccuracy -= m_WeaponData.AccuracyDropPerShot;
            if (m_fCurrentAccuracy <= 0.0f)
                m_fCurrentAccuracy = 0.0f;

            // The ray that will be used for this shot
            Ray ray = new Ray(m_RaycastStartSpot.position, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, m_fAttackRange, 7))
            {
                // Damage
                if(hit.collider.gameObject.tag == EnemyTag)
                hit.collider.gameObject.SendMessageUpwards("GetDamage", ResultDamage(), SendMessageOptions.DontRequireReceiver);

         
                // Hit Effects
                if (m_bmakeHitEffects)
                {
                    WeaponEffectPoolingManager.Instance.CreateEffectPrefab(WEAPONEFFECT.HITEFFECT, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));  
                } 
            }

            // Recoil
            if (m_bIsRecoil)
                Recoil();

            // Muzzle flash effects
            if (m_bMakeMuzzleEffects)
                WeaponEffectPoolingManager.Instance.CreateEffectPrefab(MuzzleEffect, m_MuzzleEffectsPosition.position, m_MuzzleEffectsPosition.rotation);
            

            // Instantiate shell props
            if (m_bIsSpitShells)
                WeaponEffectPoolingManager.Instance.CreateEffectPrefab(WEAPONEFFECT.SPITSHELL, m_MuzzleEffectsPosition.position, m_MuzzleEffectsPosition.rotation);
            

            //�߻�ȿ���� ���
            SoundManager.Instance.PlayFireSound(m_WeaponData.FireSoundName);
        }
    }
    public void Launch() //Projectile Ÿ��(����ü) �߻�
    {
        if (m_ProjectileType == PROJECTILEPREFAB.NONE) return;
        if (!WeaponEffectPoolingManager.Instance) return;
            // Reset the fire timer to 0 (for ROF)
            m_fFireTimer = 0.0f;
        // Increment the burst counter
        m_IBurstCounter++;

        // If this is a semi-automatic weapon, set canFire to false (this means the weapon can't fire again until the player lets up on the fire button)
        if (!m_bIsAuto)
            m_bCanFire = false;

        // First make sure there is ammo
        if (m_iCurAmmo <= 0)
        {
            DryFire();
            return;
        }

        // Subtract 1 from the current ammo
        if (!m_bIsInfiniteAmmo)
        {
            m_iCurAmmo--;
            UIManager.Instance.SetAmmoText(m_iCurAmmo, m_WeaponData.MaxAmmo);
        }

        // Fire once for each shotPerRound value
        for (int i = 0; i < m_WeaponData.ShotPerRound; i++)
            WeaponEffectPoolingManager.Instance.CreateProjectilePrefab(ResultDamage(), m_ProjectileType, m_ProjectileSpawnSpot.position, m_ProjectileSpawnSpot.rotation);
        

        // Recoil
        if (m_bIsRecoil)
            Recoil();

        // Muzzle flash effects
        if (m_bMakeMuzzleEffects)
            WeaponEffectPoolingManager.Instance.CreateEffectPrefab(MuzzleEffect, m_MuzzleEffectsPosition.position, m_MuzzleEffectsPosition.rotation);
        

        if (m_bIsSpitShells)
            WeaponEffectPoolingManager.Instance.CreateEffectPrefab(WEAPONEFFECT.SPITSHELL, m_MuzzleEffectsPosition.position, m_MuzzleEffectsPosition.rotation);
        
        //�߻�ȿ���� ���
        SoundManager.Instance.PlayFireSound(m_WeaponData.FireSoundName);
    }

    private void DryFire() //�Ѿ��� ���µ� �߻��ư�� ������ �� ��Ƽ� ©��©�� �Ÿ��� ȿ���� ���
    {
        SoundManager.Instance.PlaySFX("DryFire");
    }

    private DamageWithIsCritical ResultDamage()
    {
        if (GameManager.Instance)
            return GameManager.Instance.MakePlayerDamage(m_WeaponData.TotalATK);

        return new DamageWithIsCritical();
    }
    public void SetShowCrossHair(bool NewBoolean)
    {
        m_bShowCrosshair = NewBoolean;
    }
}