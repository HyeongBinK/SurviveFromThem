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
    //총기관련 효과(폭발, 투사체, 떨어지는 탄피 등)
    //아이템 매니져가 할 일은 아니지만 오브젝트 생성매니져를 새로만들기보단 하는 일이 유사하기에 여기서 같이 병행하게끔 

    [SerializeField] private PoolingObject MuzzleEffect1Prefab; //총구 이펙트1 프리팹(대부분총)
    [SerializeField] private PoolingObject MuzzleEffect2Prefab; //총구 이펙트2 프리팹(레일건) 
    [SerializeField] private PoolingObject HitEffectPrefab; // 피격이펙트 프리팹(돌격소총,권총,샷건)
    [SerializeField] private PoolingObject SpitShellPrefab; // 탄피 프리팹
    [SerializeField] private PoolingObject ExplosionT1Prefab; //폭발1 이펙트 프리팹(로켓런처)
    [SerializeField] private PoolingObject ExplosionT2Prefab; //폭발2 이펙트 프리팹(레일건)
    [SerializeField] private Projectile RocketBombPrefab; // 로켓런쳐 탄환 프리팹
    [SerializeField] private Projectile RailGunBulltPrefab; // 레일건 탄환 프리팹
    private Transform WeaponEffectPoolingLocation; // 프리팹 풀링리스트 목록들을 넣어둘 게임상의 위치(폴더)

    //풀링 목록
    private Dictionary<WEAPONEFFECT, Queue<PoolingObject>> EffectPoolingList = new Dictionary<WEAPONEFFECT, Queue<PoolingObject>>();
    private Dictionary<PROJECTILEPREFAB, Queue<Projectile>> ProjectilePoolingList = new Dictionary<PROJECTILEPREFAB, Queue<Projectile>>();

    //현재 활성화되있는 목록 
    // CA = CurActivated 의 약자(이름이 너무길어지는 거 같아서 약자를 사용)
    private Dictionary<WEAPONEFFECT, List<PoolingObject>> CA_Effect = new Dictionary<WEAPONEFFECT, List<PoolingObject>>();
    private Dictionary<PROJECTILEPREFAB, List<Projectile>> CA_Projectile = new Dictionary<PROJECTILEPREFAB, List<Projectile>>();

    private readonly int MuzzleAndHitEffectMaxPoolingQuantity = 30; // 총구이펙트, 피격이펙트 풀링 생성량
    private readonly int SpitShellMaxPoolingQuantity = 100; // 탄피이펙트 풀링 생성량
    private readonly int ExplosionEffectMaxPoolingQuantity = 50; //폭발이펙트 들의 풀링 생성량
    private readonly int RocketBombMaxPoolingQuantity = 15; //로켓런쳐 발사체 풀링 생성량
    private readonly int RailGunBulltMaxPoolingQuantity = 40; //레일건 발사체 풀링 생성량
    private readonly float m_fShellSpitForce = 1.0f; //탄피가 총에서 배출되는(떨어지는) 힘
    private readonly float m_fShellForceRandom = 0.5f; // 탄피가 총에서 배출되는 힘에 랜덤으로 수치 가산
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
        if (CA_Projectile[type].Count >= GetProjectileMaxQuantity(type)) //생성된 양이 최대생성가능한 양보다 크다면  
            return;
        
        if (ProjectilePoolingList[type].Count <= 0) MakeProjectile(GetProjectilePoolingPrefab(type)); //풀링리스트에 풀링된 프리팹이 없으면 재생산

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
        if (CA_Effect[Effect].Count >= GetEffectMaxQuantity(Effect)) //생성된 양이 최대생성가능한 양보다 크다면  
            return;

        if (EffectPoolingList[Effect].Count <= 0) MakeEffect(GetEffectPoolingPrefab(Effect)); //풀링리스트에 풀링된 프리팹이 없으면 재생산

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
        //조건문이 3개이므로 switch 문이 if 문보다 별로라고 판단

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
            Debug.Log("발사체 타입이상함");
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
            Debug.Log("발사체 타입이상함");
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
