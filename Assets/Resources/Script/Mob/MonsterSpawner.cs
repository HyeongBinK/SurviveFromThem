using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private NormalMonster m_MonsterPrefab1; // ������ �� AI1
    [SerializeField] private NormalMonster m_MonsterPrefab2; // ������ �� AI2
    [SerializeField] private Transform[] m_SpawnPoints; // �� AI�� ��ȯ�� ��ġ��
    [SerializeField] private int m_iMaxFieldMonster = 30; //�ִ� ���� ������
    [SerializeField] private int m_iMaxLevel; //���������� ������ �ִ뷹��
    [SerializeField] private int m_iMinLevel; //���������� ������ �ּҷ���

    [SerializeField] [Range(0, 100)] private int AnotherTypeSpawnRate; //Ÿ���� �ٸ� ���� ���� Ȯ��(���� 0 ~ 100)

    [SerializeField] private bool m_bIsBeginWithMonster; //�ʿ� �Ѿ���ڸ��� ���͸� �ִ�ġ���� ���������� ����
    [SerializeField] private bool m_bIsMonsterRegen; //������ ���� ����
    [SerializeField] private int m_iCurField; //���� �ʵ���� ����

    private List<NormalMonster> m_Monsters = new List<NormalMonster>(); // �ʵ忡 ������ ������ ��� ����Ʈ
    private Dictionary<MONSTER_TYPE, Queue<NormalMonster>> m_MonsterPooling = new Dictionary<MONSTER_TYPE, Queue<NormalMonster>>(); //Ǯ���� ���͵��� ����Ʈ
   
    [SerializeField] private float m_fMonsterReGenTime = 5.0f; //���� ����Ÿ��

    private GameObject TempTransformGameObject; //���� ��������(������ ������������ x,z��ǥ �������� �����ؼ� ����) ������ ���� �ӽ� ��ġ��ǥ
    [SerializeField] private Transform MonsterPoolingPostion; //Ǯ���ȸ��͵��� ���ӿ�����Ʈ��ġ�� ��ġ
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
    private void Init() //�ʱ�ȭ
    {
        TempTransformGameObject = new GameObject();
        TempTransformGameObject.name = "������ġ��ǥ";
        for (int i = 0; i < m_iMaxFieldMonster; i++)
        {
            MakeMonster(m_MonsterPrefab1);
            MakeMonster(m_MonsterPrefab2);
        }
    }
    private void Clear() //������ �ִ� �ʵ� ���͸���Ʈ�� Ǯ���׸� Ŭ����
    {
        m_Monsters.Clear(); // ������ ���͵� ����Ʈ Ŭ����
        m_MonsterPooling.Clear(); //Ǯ������Ʈ Ŭ����
    }

    private Transform RandomPoint() //���� ������ġ ����
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
    private int RandomLevel() //������ ���� ��������
    {
        return Random.Range(m_iMinLevel, m_iMaxLevel) + 1;
    }
    private void MakeMonster(NormalMonster prefab) //���� 1���� Ǯ������Ʈ(ť)�� ����
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
    private NormalMonster GetMonster(MONSTER_TYPE type) //Ǯ������Ʈ���� �������� ��������
    {
        if (m_MonsterPooling.ContainsKey(type))
        {
            return m_MonsterPooling[type].Dequeue();
        }
        return null;
    }
    private bool CreateMonster() //�ʵ忡 ���� ����
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
