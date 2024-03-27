using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Text;

public enum MAPNAME
{
    Town = 0,
    City,
    Forest,
    Desert,
    DestroyedCity,
    DutorialWorld,
    END
}

public class GameManager : MonoBehaviour
{
    private static GameManager m_instance; //싱글톤 할당

    public static GameManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<GameManager>();
                DontDestroyOnLoad(m_instance.gameObject);
                SceneManager.sceneLoaded += m_instance.LoadedScene;
            }
            return m_instance;
        }
    }

    [SerializeField] private Player m_Player;
    public Player GetPlayerData { get { return m_Player; } }  //외부에서 Player내의 기능에 접근할떄 사용
    [SerializeField] private PlayerController m_PlayerController;
    [SerializeField] private WarpPoint[] PlayerWarpPoint;
    public Transform GetPlayerTransform { get { return m_Player.gameObject.transform; } }
    public bool m_bIsDead { get { return m_Player.m_bIsDead; } } // 생사상태(죽으면 패널티를 받은 후 부활지점에서 부활)
    public bool m_bIsZoomIn { get; private set; } //줌인(FPS) 상태이면 true, 아니면(TPS)이면 false 
    private Vector3 m_NewPlayerPosition = new Vector3(0, 5, 0); // 포탈을 탈시 이동될 플레이어의 위치
    private Quaternion m_NewPlayerRotation = new Quaternion(); // 포탈을 탈시 이동될 플레이어의 방향
    //게임시스템 적인 부분
    public readonly int MaxDamage = 99999; //최대 데미지

    //데미지에 랜덤성을 주기위한 변수(후에 플레이어가 이수치의 최소,최대값을 바꿀 능력치가 생길시 플레이어쪽으로 이동)
    public readonly float m_fRandomDamageMin = 0.7f; // 데미지수치에 곱할 최소값 
    public readonly float m_fRandomDamageMax = 1.3f; // 데미지수치에 곱할 최대값

    //키보드(후에 키인풋 매니져 따로 만들게 될시 이동)
    private bool m_bIsCanInput = true;
    private readonly KeyCode KeyCodeTestKey = KeyCode.T; // 테스트키
    private readonly KeyCode KeyCodeInteract = KeyCode.F; // 상호 작용키(npc 대화)
    // private readonly KeyCode KeyCodeOnOffMouse = KeyCode.LeftControl; //마우스 온오프기능
    private readonly KeyCode KeyCodeMainWeapon = KeyCode.Alpha1; //메인무기로 변경키
    private readonly KeyCode KeyCodeSubWeapon = KeyCode.Alpha2; //서브무기로 변경키
    //UI부분
    private readonly KeyCode KeyCodeInventory = KeyCode.I; // 인벤토리창 Active/DisActive 키
    private readonly KeyCode KeyCodeStatus = KeyCode.P; // 플레이어 정밀스테이터스창 Active/DisActive 키
    private readonly KeyCode KeyCodeSkill = KeyCode.K; // 스킬창 Active/DisActive 키
    private readonly KeyCode KeyCodeMenu = KeyCode.Escape; // 메뉴(저장,로드,게임종료)창 Active/DisActive 키
    //시스템로그
    private List<string> SystemLogs = new List<string>();//UIManager의 시스템로그출력에 쓰일 로그들
    private readonly int MaxSystemMessageLine = 20; //시스템로그 최대표기수, 넘어가면 맨앞의 로그부터 순차적으로 사라지고 새로그 추가

    private readonly float ShowMapNameDelayTerm = 1.5f; // 맵로드후 맵의 이름을 보여줄 때까지의 텀
    private void Awake()
    {
        // 씬에 싱글톤 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        if (Instance != this)
        {
            // 자신을 파괴
            Destroy(gameObject);
        }
        m_bIsZoomIn = false;
    }
    private void Start()
    {
        m_bIsCanInput = true;
        StartCoroutine(KeyInputEvent());
    }

    public void AddNewLog(string NewText) //시스템 로그 메시지박스에 새 로그 추가
    {
        if (SystemLogs.Count > MaxSystemMessageLine)
        {
            SystemLogs.RemoveAt(0);
        }
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(" ");
        stringBuilder.Append(NewText);
        SystemLogs.Add(stringBuilder.ToString());
        UIManager.Instance.SetSystemLog(SystemLogs);
    }

    public void ClearSystemLogs()
    {
        SystemLogs.Clear();
    }
    public void FPSMode() //FPS 시점으로 변경
    {
        if (CameraManager.Instance)
            CameraManager.Instance.ActiveSubCamera();
        m_PlayerController.HidePlayerBody();
        m_bIsZoomIn = true;
    }
    public void TPSMode() //TPS시점으로 변경
    {
        if (CameraManager.Instance)
            CameraManager.Instance.ActiveMainCamera();
        m_PlayerController.ShowPlayerBody();
        m_bIsZoomIn = false;
    }
    public int MakeRandomDamage(int NewDamage) //몬스터의 데미지계산
    {
        int Damage = UnityEngine.Random.Range((int)(NewDamage * m_fRandomDamageMin), (int)(NewDamage * m_fRandomDamageMax));
        if (Damage > MaxDamage)
            Damage = MaxDamage;

        return Damage;
    }
    public void ItemUse(int Slotnumber, int Quantity)
    {
        if(m_Player.GetInventory.DecreaseItemQuantity(ITEMTYPE.CONSUMPTION, Slotnumber, Quantity)) //아이템감소작업에 성공시
        {
            if (!DataTableManager.Instance) return;
            var Data = DataTableManager.Instance.GetConsumptionData(m_Player.GetInventorySlotData(ITEMTYPE.CONSUMPTION, Slotnumber).ItemUniqueNumber);
            if (Data.ItemType == CONSUMPTIONTYPE.WARPCAPSULE) //워프류 아이템의 경우
            {
                TeleportPlayer(PlayerWarpPoint[Data.Value]);
                return;
            }
            m_Player.UseItem(Data.ItemType, Data.Value);
            if (SoundManager.Instance)
                SoundManager.Instance.PlaySFX("UseConsumption");
        }
    }
    public void GiveExpToPlayer(int NewEXP) //외부에서 플레이어 경험치에 영향을 줄떄
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("+");
        stringBuilder.Append(NewEXP);
        stringBuilder.Append("EXP");

        AddNewLog(stringBuilder.ToString());
        m_Player.GetExp(NewEXP);
    }
    public void SetPlayerSpeed()
    {
        if(DataTableManager.Instance)
            m_PlayerController.SetSpeed(DataTableManager.Instance.GetSkillData((int)SKILLNAME.SPEEDUP).GetSkillTotalValue(m_Player.GetPlayerStatusData.GetSkillLevel(SKILLNAME.SPEEDUP)));
    }
    public DamageWithIsCritical MakePlayerDamage(int WeaponAtk) //플레이어의 데미지 계산
    {
        DamageWithIsCritical NewDWC = new DamageWithIsCritical(); //DWC = Damage With CriticalRate 의 줄임의 줄임말으로서 사용
        NewDWC.IsCritical = false;
        int Damage = MakeRandomDamage((int)((float)WeaponAtk * (float)(1 + (float)m_Player.GetPlayerAtkUpSkillLevel * 0.01)));
        int RandomNum = UnityEngine.Random.Range(0, 100);
        if (RandomNum <= m_Player.m_iCriticalRate)
        {
            Damage = (int)((float)(Damage) * m_Player.m_fCriticalDammage);
            NewDWC.IsCritical = true;
        }

        if (Damage > MaxDamage)
            Damage = MaxDamage;

        NewDWC.Damage = Damage;
        return NewDWC;
    }
    public void LockMouse()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        UnLockPlayerMovement();
    }
    public void UnlockMouse()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        LockPlayerMovement();
    }

    //키입력부분
    IEnumerator KeyInputEvent()
    {
        while (Instance)
        {
            if (m_bIsCanInput)
            {
                if (Input.GetKeyDown(KeyCodeTestKey))
                {
                    TestFunction();
                }
                if (Input.GetKeyDown(KeyCodeMainWeapon))
                {
                    if (WeaponManager.Instance)
                        WeaponManager.Instance.ActiveMainWeapon();
                }
                if (Input.GetKeyDown(KeyCodeSubWeapon))
                {
                    if (WeaponManager.Instance)
                        WeaponManager.Instance.ActiveSubWeapon();
                }
                if (Input.GetKeyDown(KeyCodeInventory))
                {
                    if (UIManager.Instance)
                        UIManager.Instance.OnOffInventoryWindow();
                }
                if (Input.GetKeyDown(KeyCodeStatus))
                {
                    if (UIManager.Instance)
                        UIManager.Instance.OnOffUserinfoWindow();
                }
                if (Input.GetKeyDown(KeyCodeSkill))
                {
                    if (UIManager.Instance)
                        UIManager.Instance.OnOffSkillWindow();
                }
                if(Input.GetKeyDown(KeyCodeMenu))
                {
                    if (UIManager.Instance)
                        UIManager.Instance.OnOffMenu();
                }
                if(UIManager.Instance)
                {
                    UIManager.Instance.GetQuickSlots.CheckQuickSlotInput();
                }
            }
            yield return null;
        }
    }
    private void LockPlayerMovement() //플레이어의 움직임, 총알 발사 등을 막음
    {
        if (WeaponManager.Instance)
            WeaponManager.Instance.SetCanUseWeapon(false);
     /*   if (SkillManager.Instance)
            SkillManager.Instance.SetCanUseSkillState(false);*/
        m_PlayerController.SetIsCanAct(false);
    }
    private void UnLockPlayerMovement() //플레이어의 움직임, 총알 발사 등을 가능하게함
    {
        if (WeaponManager.Instance)
            WeaponManager.Instance.SetCanUseWeapon(true);
        if (SkillManager.Instance)
            SkillManager.Instance.SetCanUseSkillState(true);
        m_PlayerController.SetIsCanAct(true);
    }
    public void LockAllInput()
    {
        LockPlayerMovement();
        m_bIsCanInput = false;
    }
    public void UnlockAllInput()
    {
        UnLockPlayerMovement();
        m_bIsCanInput = true;
    }

    //맵(씬) 이동 부분
    private void LoadedScene(Scene scene, LoadSceneMode mode)
    {
        if (scene.isLoaded)
        {
            if (UIManager.Instance)
                UIManager.Instance.ActiveFadeOutEffect();
            m_PlayerController.SetPlayerTransform(m_NewPlayerPosition, m_NewPlayerRotation);
            string MapName = scene.name;
            switch (MapName)
            {
                case nameof(MAPNAME.Town):
                    SoundManager.Instance.PlayBGM(BGMLIST.TownAndCity.ToString());
                    break;
                case nameof(MAPNAME.City):
                    SoundManager.Instance.PlayBGM(BGMLIST.TownAndCity.ToString());
                    break;
                case nameof(MAPNAME.Forest):
                    SoundManager.Instance.PlayBGM(BGMLIST.Forest.ToString());
                    break;
                case nameof(MAPNAME.Desert):
                    SoundManager.Instance.PlayBGM(BGMLIST.Desert.ToString());
                    break;
                case nameof(MAPNAME.DestroyedCity):
                    SoundManager.Instance.PlayBGM(BGMLIST.DestroyedCity.ToString());
                    break;
                default:
                    break;
            }
            SetScene();
            UnlockAllInput();
            ClearSystemLogs();
            AddNewLog(scene.name + "으로 이동완료!");
            StartCoroutine(DelayShowMapName(MapName));
        }
    }
    public void MoveScene(MAPNAME TargetMap)
    {
        m_Player.SetInvincibility(true);
        LockAllInput();
        StartCoroutine(DelayLoadScene(TargetMap));
    }
    IEnumerator DelayLoadScene(MAPNAME TargetMap)
    {
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("Portal");
        if (UIManager.Instance)
            UIManager.Instance.ActiveFadeInEffect();
        yield return new WaitForSeconds(2.0f); ;
        ClearScene();
        SceneManager.LoadScene(TargetMap.ToString());
    }

    IEnumerator DelayShowMapName(string NewMapName)
    {
        yield return new WaitForSeconds(ShowMapNameDelayTerm);
        if (UIManager.Instance)
            UIManager.Instance.ShowMapName(NewMapName);
    }

    public void TeleportPlayer(WarpPoint WP) //플레이어 순간이동
    {
        if (SceneManager.GetActiveScene().name == WP.TargetScene.ToString())
        {
            m_Player.gameObject.transform.position = WP.TargetPos;
            m_Player.gameObject.transform.rotation = WP.TargetRot;
            return;
        }

        m_NewPlayerPosition = WP.TargetPos;
        m_NewPlayerRotation = WP.TargetRot;
        MoveScene(WP.TargetScene);
    }
    public void PlayerDeadWarp() //플레이어가 죽었을떄 마을로 부활
    {
        TeleportPlayer(PlayerWarpPoint[0]);
        StartCoroutine(DelayPlayPlayerRespawnSound());
    }
    private void ClearScene()
    {
        m_Player.gameObject.transform.position = Vector3.zero;
        if (ItemManager.Instance) ItemManager.Instance.Clear();
        if (WeaponEffectPoolingManager.Instance) WeaponEffectPoolingManager.Instance.Clear();
        if (SkillManager.Instance)
            SkillManager.Instance.DisActiveSummonObject();
    }
    private void SetScene()
    {
        if (ItemManager.Instance) ItemManager.Instance.Init();
        if (WeaponEffectPoolingManager.Instance) WeaponEffectPoolingManager.Instance.Init();
        m_Player.SetInvincibility(false);
    }
    IEnumerator DelayPlayPlayerRespawnSound()
    {
        yield return new WaitForSeconds(2.5f);
               if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("PlayerRebirth");
    }
    private void TestFunction()
    {
        m_Player.GetDamage(1000);
    }
}
