using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WEAPONEFFECT
{
    START,
    MUZZLEEFFECT1 = 0,
    MUZZLEEFFECT2,
    HITEFFECT,
    SPITSHELL,
    EXPLOSIONT1,
    EXPLOSIONT2,
    END
}

public enum PROJECTILEPREFAB
{
    START,
    NONE = 0,
    ROCKETBOMB = 1,
    RAILGUNBULLET,
    END
}

public class WeaponEffectPoolingManager : MonoBehaviour
{
    private static WeaponEffectPoolingManager m_instance;
    public static WeaponEffectPoolingManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<WeaponEffectPoolingManager>();
                DontDestroyOnLoad(m_instance.gameObject);
            }
            return m_instance;
        }
    }
    //�ѱ���� ȿ��(����, ����ü, �������� ź�� ��)
    //������ �Ŵ����� �� ���� �ƴ����� ������Ʈ �����Ŵ����� ���θ���⺸�� �ϴ� ���� �����ϱ⿡ ���⼭ ���� �����ϰԲ� 

    [SerializeField] private PoolingObject MuzzleEffect1Prefab; //�ѱ� ����Ʈ1 ������(��κ���)
    [SerializeField] private PoolingObject MuzzleEffect2Prefab; //�ѱ� ����Ʈ2 ������(���ϰ�) 
    [SerializeField] private PoolingObject HitEffectPrefab; // �ǰ�����Ʈ ������(���ݼ���,����,����)
    [SerializeField] private PoolingObject SpitShellPrefab; // ź�� ������
    [SerializeField] private PoolingObject ExplosionT1Prefab; //����1 ����Ʈ ������(���Ϸ�ó)
    [SerializeField] private PoolingObject ExplosionT2Prefab; //����2 ����Ʈ ������(���ϰ�)
    [SerializeField] private Projectile RocketBombPrefab; // ���Ϸ��� źȯ ������
    [SerializeField] private Projectile RailGunBulltPrefab; // ���ϰ� źȯ ������
    private Transform WeaponEffectPoolingLocation; // ������ Ǯ������Ʈ ��ϵ��� �־�� ���ӻ��� ��ġ(����)

    //Ǯ�� ���
    private Dictionary<WEAPONEFFECT, Queue<PoolingObject>> EffectPoolingList = new Dictionary<WEAPONEFFECT, Queue<PoolingObject>>();
    private Dictionary<PROJECTILEPREFAB, Queue<Projectile>> ProjectilePoolingList = new Dictionary<PROJECTILEPREFAB, Queue<Projectile>>();

    //���� Ȱ��ȭ���ִ� ��� 
    // CA = CurActivated �� ����(�̸��� �ʹ�������� �� ���Ƽ� ���ڸ� ���)
    private Dictionary<WEAPONEFFECT, List<PoolingObject>> CA_Effect = new Dictionary<WEAPONEFFECT, List<PoolingObject>>();
    private Dictionary<PROJECTILEPREFAB, List<Projectile>> CA_Projectile = new Dictionary<PROJECTILEPREFAB, List<Projectile>>();

    private readonly int MuzzleAndHitEffectMaxPoolingQuantity = 30; // �ѱ�����Ʈ, �ǰ�����Ʈ Ǯ�� ������
    private readonly int SpitShellMaxPoolingQuantity = 100; // ź������Ʈ Ǯ�� ������
    private readonly int ExplosionEffectMaxPoolingQuantity = 50; //��������Ʈ ���� Ǯ�� ������
    private readonly int RocketBombMaxPoolingQuantity = 15; //���Ϸ��� �߻�ü Ǯ�� ������
    private readonly int RailGunBulltMaxPoolingQuantity = 40; //���ϰ� �߻�ü Ǯ�� ������
    private readonly float m_fShellSpitForce = 1.0f; //ź�ǰ� �ѿ��� ����Ǵ�(��������) ��
    private readonly float m_fShellForceRandom = 0.5f; // ź�ǰ� �ѿ��� ����Ǵ� ���� �������� ��ġ ����
    private readonly float m_fShellTorqueRandom = 1.0f; // The variant by which the split torque can change + or - for each shot


    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        Init();
    }
    public void Init()
    {
        GameObject WeaponEffectPrefabLocation = new GameObject();
        WeaponEffectPrefabLocation.name = "WeaponEffectPrefabLocation";
        WeaponEffectPoolingLocation = WeaponEffectPrefabLocation.transform;
        MakePooilingList();
    }
    private void MakeEffect(PoolingObject prefab)
    {
        if (prefab)
        {
            PoolingObject NewEffect = Instantiate(prefab, WeaponEffectPoolingLocation);
            if (!EffectPoolingList.ContainsKey(prefab.EffectType))
            {
                EffectPoolingList.Add(prefab.EffectType, new Queue<PoolingObject>());
                CA_Effect.Add(prefab.EffectType, new List<PoolingObject>());
            }
            EffectPoolingList[prefab.EffectType].Enqueue(NewEffect);
            NewEffect.gameObject.SetActive(false);
        }
    }
    private void MakeProjectile(Projectile prefab)
    {
        if (prefab)
        {
            Projectile NewProjectile = Instantiate(prefab, WeaponEffectPoolingLocation);
            if (!ProjectilePoolingList.ContainsKey(prefab.ProjectileAppearance))
            {
                ProjectilePoolingList.Add(prefab.ProjectileAppearance, new Queue<Projectile>());
                CA_Projectile.Add(prefab.ProjectileAppearance, new List<Projectile>());
            }
            ProjectilePoolingList[prefab.ProjectileAppearance].Enqueue(NewProjectile);
            NewProjectile.gameObject.SetActive(false);
        }
    }

    private void MakePooilingList()
    {
        for(int i = 0; i < MuzzleAndHitEffectMaxPoolingQuantity; i++)
        {
            MakeEffect(MuzzleEffect1Prefab);
            MakeEffect(MuzzleEffect2Prefab);
            MakeEffect(HitEffectPrefab);
        }
        for (int i = 0; i < SpitShellMaxPoolingQuantity; i++)
        {
            MakeEffect(SpitShellPrefab);
        }
        for (int i = 0; i < ExplosionEffectMaxPoolingQuantity; i++)
        {
            MakeEffect(ExplosionT1Prefab);
            MakeEffect(ExplosionT2Prefab);
        }
        for (int i = 0; i < RocketBombMaxPoolingQuantity; i++)
        {
            MakeProjectile(RocketBombPrefab);
        }
        for (int i = 0; i < RailGunBulltMaxPoolingQuantity; i++)
        {
            MakeProjectile(RailGunBulltPrefab);
        }
    }

    public void CreateProjectilePrefab(DamageWithIsCritical DWC, PROJECTILEPREFAB type, Vector3 Position, Quaternion Rotaion)
    {
        if (CA_Projectile[type].Count >= GetProjectileMaxQuantity(type)) //������ ���� �ִ���������� �纸�� ũ�ٸ�  
            return;
        
        if (ProjectilePoolingList[type].Count <= 0) MakeProjectile(GetProjectilePoolingPrefab(type)); //Ǯ������Ʈ�� Ǯ���� �������� ������ �����

        Projectile NewProjectile = ProjectilePoolingList[type].Dequeue();
        CA_Projectile[type].Add(NewProjectile);

        if (NewProjectile)
        {
            NewProjectile.SetDammage(DWC);
            NewProjectile.MultiplyInitialForce(0);
            NewProjectile.SetPosition(Position, Rotaion);
            NewProjectile.DisableEvent += () => CA_Projectile[type].Remove(NewProjectile);
            NewProjectile.DisableEvent += () => ProjectilePoolingList[type].Enqueue(NewProjectile);
        }
    }
    public void CreateEffectPrefab(WEAPONEFFECT Effect, Vector3 Position, Quaternion Rotation)
    {
        if (CA_Effect[Effect].Count >= GetEffectMaxQuantity(Effect)) //������ ���� �ִ���������� �纸�� ũ�ٸ�  
            return;

        if (EffectPoolingList[Effect].Count <= 0) MakeEffect(GetEffectPoolingPrefab(Effect)); //Ǯ������Ʈ�� Ǯ���� �������� ������ �����

        PoolingObject NewEffect = EffectPoolingList[Effect].Dequeue();
        CA_Effect[Effect].Add(NewEffect);

        if (NewEffect)
        {
            NewEffect.SetTransform(Position, Rotation);
            if(Effect == WEAPONEFFECT.SPITSHELL)
            {
                NewEffect.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(m_fShellSpitForce + Random.Range(0, m_fShellForceRandom), 0, 0), ForceMode.Impulse);
                NewEffect.GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(Random.Range(m_fShellTorqueRandom, m_fShellTorqueRandom), Random.Range(m_fShellTorqueRandom, m_fShellTorqueRandom), 0), ForceMode.Impulse);
            }
            NewEffect.DisableEvent += () => CA_Effect[Effect].Remove(NewEffect);
            NewEffect.DisableEvent += () => EffectPoolingList[Effect].Enqueue(NewEffect);
        }
    }
    private int GetEffectMaxQuantity(WEAPONEFFECT type)
    {
        //���ǹ��� 3���̹Ƿ� switch ���� if ������ ���ζ�� �Ǵ�

        if ((int)type < (int)(WEAPONEFFECT.SPITSHELL))
            return MuzzleAndHitEffectMaxPoolingQuantity;
        else if ((int)type == (int)(WEAPONEFFECT.SPITSHELL))
            return SpitShellMaxPoolingQuantity;
        else return ExplosionEffectMaxPoolingQuantity;
    }
    private PoolingObject GetEffectPoolingPrefab(WEAPONEFFECT type)
    {
        switch (type)
        {
            case WEAPONEFFECT.MUZZLEEFFECT1:
                return MuzzleEffect1Prefab;
            case WEAPONEFFECT.MUZZLEEFFECT2:
                return MuzzleEffect2Prefab;
            case WEAPONEFFECT.HITEFFECT:
                return HitEffectPrefab;
            case WEAPONEFFECT.SPITSHELL:
                return SpitShellPrefab;
            case WEAPONEFFECT.EXPLOSIONT1:
                return ExplosionT1Prefab;
            case WEAPONEFFECT.EXPLOSIONT2:
                return ExplosionT2Prefab;
        }

        return null;
    }
    private int GetProjectileMaxQuantity(PROJECTILEPREFAB Type)
    {
        if (Type == PROJECTILEPREFAB.ROCKETBOMB)
            return RocketBombMaxPoolingQuantity;
        else if (Type == PROJECTILEPREFAB.RAILGUNBULLET)
            return RailGunBulltMaxPoolingQuantity;
        else
        {
            Debug.Log("�߻�ü Ÿ���̻���");
            return 0;
        }
    }
    private Projectile GetProjectilePoolingPrefab(PROJECTILEPREFAB Type)
    {
        if (Type == PROJECTILEPREFAB.ROCKETBOMB)
            return RocketBombPrefab;
        else if (Type == PROJECTILEPREFAB.RAILGUNBULLET)
            return RailGunBulltPrefab;
        else
        {
            Debug.Log("�߻�ü Ÿ���̻���");
            return null;
        }
    }

    public void Clear()
    {
        EffectPoolingList.Clear();
        ProjectilePoolingList.Clear();
        CA_Effect.Clear();
        CA_Projectile.Clear();
    }
}
