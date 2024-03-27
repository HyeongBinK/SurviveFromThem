using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCRotateToPlayer : MonoBehaviour
{
    private Transform m_PlayerTr; //�÷��̾��� ��ġ
    private Quaternion m_OriginRotate; 
    [SerializeField] private float m_fPlayerSearchRange = 7.0f; //�÷��̾� Ž������(�ش� ���������� �÷��̾ ������ �÷��̾ �ٶ�)
    private float m_fDistanceToPlayer; //���� �÷��̾���� �Ÿ�
    private bool m_bIsPlayerInRange { get { return m_fDistanceToPlayer <= m_fPlayerSearchRange; } }
    private bool m_bIsPlayerInScene; //�÷��̾ �ʾȿ��ִ���
    private void Start()
    {
        m_OriginRotate = gameObject.transform.rotation;
        m_bIsPlayerInScene = false;
        StartCoroutine(Act());
    }

    IEnumerator Act()
    {
        while (gameObject.activeSelf) //���ӿ�����Ʈ�� Ȱ��ȭ��������
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
                    Quaternion lookAt = Quaternion.identity;    // Querternion �Լ� ����
                    Vector3 lookatVec = (m_PlayerTr.position - transform.position).normalized; //Ÿ�� ��ġ - �ڽ� ��ġ -> ��ֶ�����
                    lookAt.SetLookRotation(lookatVec);  // ���ʹϾ��� SetLookRotaion �Լ� ����
                    transform.rotation = lookAt;   //���������� Quternion  ����
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
