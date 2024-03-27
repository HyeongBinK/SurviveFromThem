using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingDamageManager_TMP : MonoBehaviour
{
    private static FloatingDamageManager_TMP m_instance; // 싱글톤이 할당될 변수
    // 싱글톤 접근용 프로퍼티
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

    [SerializeField] private FloatingDamage_TMP m_DamageTMP_Prefab; //생성할 데미지 텍스트 오브젝트의 프리팹
    private Queue<FloatingDamage_TMP> m_DamageTMP_PoolingList = new Queue<FloatingDamage_TMP>(); //데미지텍스트들의 풀링 큐
    private List<FloatingDamage_TMP> m_ActivatedDamageTMP_PoolingList = new List<FloatingDamage_TMP>(); //활성화된 데미지택스트들의 리스트
    private Transform m_DamageTextPoolingLocation;
    [SerializeField] private bool m_bIsMakeDamageText; // 데미지 생성여부
    private readonly int MaxFloatingDamageText = 30; //풀링할 데미지텍스트의 갯수
    

    private void Awake()
    {
        // 씬에 싱글톤 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        if (Instance != this)
        {
            // 자신을 파괴
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
    public void MakeDamageTextToPoolingList() //풀링리스트에 하나 추가
    {
        FloatingDamage_TMP NewDamageText = Instantiate(m_DamageTMP_Prefab, m_DamageTextPoolingLocation);
        m_DamageTMP_PoolingList.Enqueue(NewDamageText);
        NewDamageText.gameObject.SetActive(false);
    }

    private void MakeDamageTextPoolingList() //풀링리스트 생성
    {
        for (int i = 0; i < MaxFloatingDamageText; i++)
        {
            MakeDamageTextToPoolingList();
        }
    }

    public void CreateDamageUI(int Damage, bool IsCritical, Transform TR) //데미지텍스트 생성
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