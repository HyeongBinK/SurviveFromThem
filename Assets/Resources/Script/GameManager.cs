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
    private static GameManager m_instance; //�̱��� �Ҵ�

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
    public Player GetPlayerData { get { return m_Player; } }  //�ܺο��� Player���� ��ɿ� �����ҋ� ���
    [SerializeField] private PlayerController m_PlayerController;
    [SerializeField] private WarpPoint[] PlayerWarpPoint;
    public Transform GetPlayerTransform { get { return m_Player.gameObject.transform; } }
    public bool m_bIsDead { get { return m_Player.m_bIsDead; } } // �������(������ �г�Ƽ�� ���� �� ��Ȱ�������� ��Ȱ)
    public bool m_bIsZoomIn { get; private set; } //����(FPS) �����̸� true, �ƴϸ�(TPS)�̸� false 
    private Vector3 m_NewPlayerPosition = new Vector3(0, 5, 0); // ��Ż�� Ż�� �̵��� �÷��̾��� ��ġ
    private Quaternion m_NewPlayerRotation = new Quaternion(); // ��Ż�� Ż�� �̵��� �÷��̾��� ����
    //���ӽý��� ���� �κ�
    public readonly int MaxDamage = 99999; //�ִ� ������

    //�������� �������� �ֱ����� ����(�Ŀ� �÷��̾ �̼�ġ�� �ּ�,�ִ밪�� �ٲ� �ɷ�ġ�� ����� �÷��̾������� �̵�)
    public readonly float m_fRandomDamageMin = 0.7f; // ��������ġ�� ���� �ּҰ� 
    public readonly float m_fRandomDamageMax = 1.3f; // ��������ġ�� ���� �ִ밪

    //Ű����(�Ŀ� Ű��ǲ �Ŵ��� ���� ����� �ɽ� �̵�)
    private bool m_bIsCanInput = true;
    private readonly KeyCode KeyCodeTestKey = KeyCode.T; // �׽�ƮŰ
    private readonly KeyCode KeyCodeInteract = KeyCode.F; // ��ȣ �ۿ�Ű(npc ��ȭ)
    // private readonly KeyCode KeyCodeOnOffMouse = KeyCode.LeftControl; //���콺 �¿������
    private readonly KeyCode KeyCodeMainWeapon = KeyCode.Alpha1; //���ι���� ����Ű
    private readonly KeyCode KeyCodeSubWeapon = KeyCode.Alpha2; //���깫��� ����Ű
    //UI�κ�
    private readonly KeyCode KeyCodeInventory = KeyCode.I; // �κ��丮â Active/DisActive Ű
    private readonly KeyCode KeyCodeStatus = KeyCode.P; // �÷��̾� ���н������ͽ�â Active/DisActive Ű
    private readonly KeyCode KeyCodeSkill = KeyCode.K; // ��ųâ Active/DisActive Ű
    private readonly KeyCode KeyCodeMenu = KeyCode.Escape; // �޴�(����,�ε�,��������)â Active/DisActive Ű
    //�ý��۷α�
    private List<string> SystemLogs = new List<string>();//UIManager�� �ý��۷α���¿� ���� �α׵�
    private readonly int MaxSystemMessageLine = 20; //�ý��۷α� �ִ�ǥ���, �Ѿ�� �Ǿ��� �α׺��� ���������� ������� ���α� �߰�

    private readonly float ShowMapNameDelayTerm = 1.5f; // �ʷε��� ���� �̸��� ������ �������� ��
    private void Awake()
    {
        // ���� �̱��� ������Ʈ�� �� �ٸ� GameManager ������Ʈ�� �ִٸ�
        if (Instance != this)
        {
            // �ڽ��� �ı�
            Destroy(gameObject);
        }
        m_bIsZoomIn = false;
    }
    private void Start()
    {
        m_bIsCanInput = true;
        StartCoroutine(KeyInputEvent());
    }

    public void AddNewLog(string NewText) //�ý��� �α� �޽����ڽ��� �� �α� �߰�
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
    public void FPSMode() //FPS �������� ����
    {
        if (CameraManager.Instance)
            CameraManager.Instance.ActiveSubCamera();
        m_PlayerController.HidePlayerBody();
        m_bIsZoomIn = true;
    }
    public void TPSMode() //TPS�������� ����
    {
        if (CameraManager.Instance)
            CameraManager.Instance.ActiveMainCamera();
        m_PlayerController.ShowPlayerBody();
        m_bIsZoomIn = false;
    }
    public int MakeRandomDamage(int NewDamage) //������ ���������
    {
        int Damage = UnityEngine.Random.Range((int)(NewDamage * m_fRandomDamageMin), (int)(NewDamage * m_fRandomDamageMax));
        if (Damage > MaxDamage)
            Damage = MaxDamage;

        return Damage;
    }
    public void ItemUse(int Slotnumber, int Quantity)
    {
        if(m_Player.GetInventory.DecreaseItemQuantity(ITEMTYPE.CONSUMPTION, Slotnumber, Quantity)) //�����۰����۾��� ������
        {
            if (!DataTableManager.Instance) return;
            var Data = DataTableManager.Instance.GetConsumptionData(m_Player.GetInventorySlotData(ITEMTYPE.CONSUMPTION, Slotnumber).ItemUniqueNumber);
            if (Data.ItemType == CONSUMPTIONTYPE.WARPCAPSULE) //������ �������� ���
            {
                TeleportPlayer(PlayerWarpPoint[Data.Value]);
                return;
            }
            m_Player.UseItem(Data.ItemType, Data.Value);
            if (SoundManager.Instance)
                SoundManager.Instance.PlaySFX("UseConsumption");
        }
    }
    public void GiveExpToPlayer(int NewEXP) //�ܺο��� �÷��̾� ����ġ�� ������ �ً�
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
    public DamageWithIsCritical MakePlayerDamage(int WeaponAtk) //�÷��̾��� ������ ���
    {
        DamageWithIsCritical NewDWC = new DamageWithIsCritical(); //DWC = Damage With CriticalRate �� ������ ���Ӹ����μ� ���
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

    //Ű�Էºκ�
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
    private void LockPlayerMovement() //�÷��̾��� ������, �Ѿ� �߻� ���� ����
    {
        if (WeaponManager.Instance)
            WeaponManager.Instance.SetCanUseWeapon(false);
     /*   if (SkillManager.Instance)
            SkillManager.Instance.SetCanUseSkillState(false);*/
        m_PlayerController.SetIsCanAct(false);
    }
    private void UnLockPlayerMovement() //�÷��̾��� ������, �Ѿ� �߻� ���� �����ϰ���
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

    //��(��) �̵� �κ�
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
            AddNewLog(scene.name + "���� �̵��Ϸ�!");
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

    public void TeleportPlayer(WarpPoint WP) //�÷��̾� �����̵�
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
    public void PlayerDeadWarp() //�÷��̾ �׾����� ������ ��Ȱ
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
