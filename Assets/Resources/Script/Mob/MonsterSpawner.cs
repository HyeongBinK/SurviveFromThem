using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private NormalMonster m_MonsterPrefab1; // 생성할 적 AI1
    [SerializeField] private NormalMonster m_MonsterPrefab2; // 생성할 적 AI2
    [SerializeField] private Transform[] m_SpawnPoints; // 적 AI를 소환할 위치들
    [SerializeField] private int m_iMaxFieldMonster = 30; //최대 몬스터 젠숫자
    [SerializeField] private int m_iMaxLevel; //랜덤스폰될 몬스터의 최대레밸
    [SerializeField] private int m_iMinLevel; //랜덤스폰될 몬스터의 최소레밸

    [SerializeField] [Range(0, 100)] private int AnotherTypeSpawnRate; //타입이 다른 몬스터 등장 확률(범위 0 ~ 100)

    [SerializeField] private bool m_bIsBeginWithMonster; //맵에 넘어오자마자 몬스터를 최대치까지 생성할지의 여부
    [SerializeField] private bool m_bIsMonsterRegen; //몬스터의 리젠 여부
    [SerializeField] private int m_iCurField; //현재 필드몬스터 숫자

    private List<NormalMonster> m_Monsters = new List<NormalMonster>(); // 필드에 생성된 적들을 담는 리스트
    private Dictionary<MONSTER_TYPE, Queue<NormalMonster>> m_MonsterPooling = new Dictionary<MONSTER_TYPE, Queue<NormalMonster>>(); //풀링된 몬스터들의 리스트
   
    [SerializeField] private float m_fMonsterReGenTime = 5.0f; //몬스터 리젠타임

    private GameObject TempTransformGameObject; //몬스터 스폰지역(정해진 스폰지역에서 x,z좌표 랜덤으로 값더해서 스폰) 랜덤을 위한 임시 위치좌표
    [SerializeField] private Transform MonsterPoolingPostion; //풀링된몬스터들의 게임오브젝트위치상 위치
    private void Awake()
    {
        Init();
        if (m_bIsBeginWithMonster)
        {
            for (int i = 0; i < m_iMaxFieldMonster; i++)
            {
                if (!CreateMonster()) break;
            }
        }
    }

    private void Start()
    {
        if(m_bIsMonsterRegen)
        StartCoroutine(MonsterReGen());
    }
    private void Init() //초기화
    {
        TempTransformGameObject = new GameObject();
        TempTransformGameObject.name = "랜덤위치좌표";
        for (int i = 0; i < m_iMaxFieldMonster; i++)
        {
            MakeMonster(m_MonsterPrefab1);
            MakeMonster(m_MonsterPrefab2);
        }
    }
    private void Clear() //생성되 있는 필드 몬스터리스트와 풀링항목 클리어
    {
        m_Monsters.Clear(); // 생성된 몬스터들 리스트 클리어
        m_MonsterPooling.Clear(); //풀링리스트 클리어
    }

    private Transform RandomPoint() //랜덤 스폰위치 생성
    {
        if (0 >= m_SpawnPoints.Length) return null;

        var index = Random.Range(0, m_SpawnPoints.Length);
        int RandomNumber1 = Random.Range(-5, 5);
        int RandomNumber2 = Random.Range(-5, 5);

        var OriginSpawnZone = m_SpawnPoints[index];
        Vector3 NewSpawnZone = new Vector3(OriginSpawnZone.transform.position.x + RandomNumber1, OriginSpawnZone.transform.position.y, OriginSpawnZone.transform.position.z + RandomNumber2);
        Transform temp = TempTransformGameObject.transform;

        temp.position = NewSpawnZone;
        temp.rotation = OriginSpawnZone.rotation;
        return temp;
    }
    private int RandomLevel() //범위내 랜덤 레벨생성
    {
        return Random.Range(m_iMinLevel, m_iMaxLevel) + 1;
    }
    private void MakeMonster(NormalMonster prefab) //몬스터 1마리 풀링리스트(큐)에 생성
    {
        if (prefab && MONSTER_TYPE.NONE != prefab.GetMonsterType)
        {
            NormalMonster NewMonster = Instantiate(prefab, MonsterPoolingPostion);
            if (!m_MonsterPooling.ContainsKey(prefab.GetMonsterType))
            {
                m_MonsterPooling.Add(prefab.GetMonsterType, new Queue<NormalMonster>());
            }
            m_MonsterPooling[prefab.GetMonsterType].Enqueue(NewMonster);
            NewMonster.gameObject.SetActive(false);
        }
    }
    private NormalMonster GetMonster(MONSTER_TYPE type) //풀링리스트에서 몬스터정보 가져오기
    {
        if (m_MonsterPooling.ContainsKey(type))
        {
            return m_MonsterPooling[type].Dequeue();
        }
        return null;
    }
    private bool CreateMonster() //필드에 몬스터 생성
    {
        if (m_iCurField < m_iMaxFieldMonster)
        {
            NormalMonster monster = null;

            int RandomRate = Random.Range(0, 100);
            if (RandomRate >= AnotherTypeSpawnRate)
            {
                monster = GetMonster(m_MonsterPrefab1.GetMonsterType);
            }
            else
            {
                monster = GetMonster(m_MonsterPrefab2.GetMonsterType);
            }

            if (monster)
            {
                var tr = RandomPoint();
                if (tr)
                {
                    monster.Init(RandomLevel(), tr);
                    monster.gameObject.SetActive(true);
                    m_iCurField++;
                    monster.OnDeath += () => monster.ClearDeadEvent();
                    monster.OnDeath += () => m_MonsterPooling[monster.GetMonsterType].Enqueue(monster);
                    monster.OnDeath += () => m_Monsters.Remove(monster);
                    monster.OnDeath += () => m_iCurField--;
                    m_Monsters.Add(monster);
                }
            }

            return true;
        }

        return false;
    }
    private IEnumerator MonsterReGen()
    {
        while (true)
        {
            if (m_bIsMonsterRegen)
            {
                for (int i = 0; i < m_iMaxFieldMonster; i++)
                {
                    if (!CreateMonster()) break;
                }
            }
            yield return new WaitForSeconds(m_fMonsterReGenTime);
        }
    }
    public void ClearField()
    {
        for (int i = 0; i < m_Monsters.Count; i++)
        {
            m_Monsters[i].gameObject.SetActive(false);
        }

        Clear();
        TempTransformGameObject = null;
    }
}
