using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCRotateToPlayer : MonoBehaviour
{
    private Transform m_PlayerTr; //플레이어의 위치
    private Quaternion m_OriginRotate; 
    [SerializeField] private float m_fPlayerSearchRange = 7.0f; //플레이어 탐색범위(해당 범위안으로 플레이어가 들어오면 플레이어를 바라봄)
    private float m_fDistanceToPlayer; //현재 플레이어와의 거리
    private bool m_bIsPlayerInRange { get { return m_fDistanceToPlayer <= m_fPlayerSearchRange; } }
    private bool m_bIsPlayerInScene; //플레이어가 맵안에있는지
    private void Start()
    {
        m_OriginRotate = gameObject.transform.rotation;
        m_bIsPlayerInScene = false;
        StartCoroutine(Act());
    }

    IEnumerator Act()
    {
        while (gameObject.activeSelf) //게임오브젝트가 활성화되있을때
        {
            if (!m_bIsPlayerInScene)
            {
                FindPlayer();
            }
            else
            {
                m_fDistanceToPlayer = Vector3.Distance(m_PlayerTr.position, transform.position);
             
                if (m_bIsPlayerInRange)
                {
                    Quaternion lookAt = Quaternion.identity;    // Querternion 함수 선언
                    Vector3 lookatVec = (m_PlayerTr.position - transform.position).normalized; //타겟 위치 - 자신 위치 -> 노멀라이즈
                    lookAt.SetLookRotation(lookatVec);  // 쿼터니언의 SetLookRotaion 함수 적용
                    transform.rotation = lookAt;   //최종적으로 Quternion  적용
                }
                else
                {
                    gameObject.transform.rotation = m_OriginRotate;
                }
            }

            yield return null;
        }
    }

    private void FindPlayer()
    {
        if (!GameManager.Instance) return;
        m_PlayerTr = GameManager.Instance.GetPlayerTransform;
        m_bIsPlayerInScene = true;
    }

}
