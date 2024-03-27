using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon : MonoBehaviour
{
    //무기 스탯 정보
    [SerializeField] private int m_iWeaponUniqueNumber; 

    private WeaponData m_WeaponData = new WeaponData(); //무기 정보
    public int m_iCurAmmo { get; private set; } //현재 장전된 총에서 남은 총알갯수
    private bool m_bCanFire; //총을 쏠수 있는지 없는지 에 대한 불린 변수
    private bool m_bIsAutoReload = true; // true : 탄환을 다쓰면 자동으로 리로드 false : 수동으로 해야됨 
    private float m_fFireTimer; //다음사격까지의 간격 타이머(쿨타임)
    private float m_fActualRateOfFire; //무기의 발사속도를 기반으로한 실제 발사속도	
                                          
    [SerializeField] private GameObject m_WeaponModel; //총의 실제 Mesh                   
    [SerializeField] private Transform m_RaycastStartSpot; //일직선으로 날라가는 총알 발사 위치
    [SerializeField] private PROJECTILEPREFAB m_ProjectileType; //탄환오브젝트(미사일, 로켓같은 투사체)
    [SerializeField] private Transform m_ProjectileSpawnSpot; //탄환이 발사될 위치

    [SerializeField] private bool m_bIsInfiniteAmmo; //탄환 무한 여부(true 일시 리로드할 필요없이 끊임없이 발사가능)
    [SerializeField] private bool m_bIsAuto; //true : 자동 false : 반자동

    //십자선(CrossHair)
    [SerializeField] private bool m_bIsCrosshair; // 십자선이 있는지 없는지
    [SerializeField] private bool m_bShowCrosshair; // 십자선을 보일지 말지
    public Texture2D crosshairTexture;// 십자선을 만드는데 쓰일 텍스쳐                  
    public int crosshairLength = 20; //각 십자선의 길이                
    public int crosshairWidth = 4; //각 십자선의 너비
    public float startingCrosshairSize = 10.0f; //시작 십자선 사이의 간격(픽셀 단위)(무기의 명중률)(반동으로 초점이 흔들려서 탄환이 랜덤으로 날라갈때)
    private float currentCrosshairSize; // 실시간 십자선 사이의 간격

    // 반동
    private bool m_bIsRecoil = true; //총을 발사할때 반동 발생 여부

    // Burst
    private int m_IBurstCounter = 0; // Counter to keep track of how many shots have been fired per burst
    private float m_fBurstTimer = 0.0f; // Timer to keep track of how long the weapon has paused between bursts

    // 정확도(Accuracy)              
    private float m_fCurrentAccuracy; //현재 정확도	

    // 총기 발사 효과
    [SerializeField] private bool m_bIsSpitShells = true; //총을 발사할때 배출되는 탄피 생성여부
    [SerializeField] private Transform m_ShellSpitPosition; //탄피가 배출되는 위치

    private bool m_bMakeMuzzleEffects = true; //사격효과 이펙트 발생여부
    [SerializeField] private WEAPONEFFECT MuzzleEffect; //발생시킬 사격효과 
    [SerializeField] private Transform m_MuzzleEffectsPosition; //사격효과 발생 위치
                                                              
    private bool m_bmakeHitEffects = true; //적중효과 이펙트 발생여부			

    // 전투시작시 1인칭(FPS)로 전환되는 효과
    private bool m_bIsZoomIn = false;
    private bool m_bIsReloading = false; // 리로드 중이면 true 아닐시 false

    private readonly float m_fAttackRange = 9999.0f; //사격범위(소총 같은거)	
    private readonly KeyCode KeyCodeReload = KeyCode.R; //재장전키
    private readonly KeyCode KeyCodeZoonIn = KeyCode.Mouse1; //줌인키
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

    void OnGUI() //GUI기능을 요즘은 잘 안 사용하지만 사용해보고 싶어서 사용했습니다.
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
        //예외가 발생하지않게끔(예외처리)
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
            // 정확도 계산
            m_fCurrentAccuracy = Mathf.Lerp(m_fCurrentAccuracy, m_WeaponData.MaxAccuracy, m_WeaponData.AccuracyRecoverRate * Time.deltaTime);
            // 십자선 업데이트
            currentCrosshairSize = startingCrosshairSize + (m_WeaponData.MaxAccuracy - m_fCurrentAccuracy) * 0.8f;

            // 타이머
            m_fFireTimer += Time.deltaTime;
            // 플레이어 키보드입력
            if(WeaponManager.Instance)
            {
                if(WeaponManager.Instance.m_bCanUseWeapon)
                    CheckForUserInput();
            }

            // 잔여 총알 갯수 확인해서 재장전
            if (m_bIsAutoReload && m_iCurAmmo <= 0)
                Reload();

            // 반동회복
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

    private void CheckAmmo() //탄환 확인
    {
        if (m_iCurAmmo > 0)
            m_bCanFire = true;
    }
    public void Reload() //재장전
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

    void Recoil() //총기를 발사할때 반동효과
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
    void CheckForUserInput() //플레이어 입력
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
                        Debug.Log("총기타입에 예외발생");
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
    public void Fire() //발사
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
            

            //발사효과음 재생
            SoundManager.Instance.PlayFireSound(m_WeaponData.FireSoundName);
        }
    }
    public void Launch() //Projectile 타입(투사체) 발사
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
        
        //발사효과음 재생
        SoundManager.Instance.PlayFireSound(m_WeaponData.FireSoundName);
    }

    private void DryFire() //총알이 없는데 발사버튼을 눌럿을 때 방아쇠 짤깍짤깍 거리는 효과음 재생
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