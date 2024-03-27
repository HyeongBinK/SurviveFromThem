using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingDamageManager_TMP : MonoBehaviour
{
    private static FloatingDamageManager_TMP m_instance; // �̱����� �Ҵ�� ����
    // �̱��� ���ٿ� ������Ƽ
    public static FloatingDamageManager_TMP Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<FloatingDamageManager_TMP>();
                DontDestroyOnLoad(m_instance.gameObject);
            }
            return m_instance;
        }
    }

    [SerializeField] private FloatingDamage_TMP m_DamageTMP_Prefab; //������ ������ �ؽ�Ʈ ������Ʈ�� ������
    private Queue<FloatingDamage_TMP> m_DamageTMP_PoolingList = new Queue<FloatingDamage_TMP>(); //�������ؽ�Ʈ���� Ǯ�� ť
    private List<FloatingDamage_TMP> m_ActivatedDamageTMP_PoolingList = new List<FloatingDamage_TMP>(); //Ȱ��ȭ�� �������ý�Ʈ���� ����Ʈ
    private Transform m_DamageTextPoolingLocation;
    [SerializeField] private bool m_bIsMakeDamageText; // ������ ��������
    private readonly int MaxFloatingDamageText = 30; //Ǯ���� �������ؽ�Ʈ�� ����
    

    private void Awake()
    {
        // ���� �̱��� ������Ʈ�� �� �ٸ� GameManager ������Ʈ�� �ִٸ�
        if (Instance != this)
        {
            // �ڽ��� �ı�
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        Init();
    }
    public void Init()
    {
        GameObject DamageTextPoolingLocation = new GameObject();
        DamageTextPoolingLocation.name = "FloatingDamageTextPoolingLocation";
        m_DamageTextPoolingLocation = DamageTextPoolingLocation.transform;
        MakeDamageTextPoolingList();
    }
    public void MakeDamageTextToPoolingList() //Ǯ������Ʈ�� �ϳ� �߰�
    {
        FloatingDamage_TMP NewDamageText = Instantiate(m_DamageTMP_Prefab, m_DamageTextPoolingLocation);
        m_DamageTMP_PoolingList.Enqueue(NewDamageText);
        NewDamageText.gameObject.SetActive(false);
    }

    private void MakeDamageTextPoolingList() //Ǯ������Ʈ ����
    {
        for (int i = 0; i < MaxFloatingDamageText; i++)
        {
            MakeDamageTextToPoolingList();
        }
    }

    public void CreateDamageUI(int Damage, bool IsCritical, Transform TR) //�������ؽ�Ʈ ����
    {
        if (!m_bIsMakeDamageText) return; 

        if (m_ActivatedDamageTMP_PoolingList.Count < MaxFloatingDamageText)
        {
            if (m_DamageTMP_PoolingList.Count <= 0) MakeDamageTextToPoolingList();

            FloatingDamage_TMP NewDamageText = m_DamageTMP_PoolingList.Dequeue();
            m_ActivatedDamageTMP_PoolingList.Add(NewDamageText);

            if (NewDamageText)
            {
                NewDamageText.Init(Damage, IsCritical, TR);

                NewDamageText.DamageTextDisable += () => m_ActivatedDamageTMP_PoolingList.Remove(NewDamageText);
                NewDamageText.DamageTextDisable += () => m_DamageTMP_PoolingList.Enqueue(NewDamageText);
            }
        }
    }

    public void ClearActivatedDamageText()
    {
        foreach(FloatingDamage_TMP FD in m_ActivatedDamageTMP_PoolingList)
        {
            FD.gameObject.SetActive(false);
            m_ActivatedDamageTMP_PoolingList.Remove(FD);
        }
        m_ActivatedDamageTMP_PoolingList.Clear();
    }


}