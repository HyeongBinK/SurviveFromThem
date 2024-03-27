using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager m_instance; //�̱��� �Ҵ�

    public static CameraManager Instance
    {
        get
        {
            // ���� �̱��� ������ ���� ������Ʈ�� �Ҵ���� �ʾҴٸ�
            if (m_instance == null)
            {
                // ������ GameManager ������Ʈ�� ã�� �Ҵ�
                m_instance = FindObjectOfType<CameraManager>();
            }
            // �̱��� ������Ʈ�� ��ȯ
            return m_instance;
        }
    }

    [SerializeField] private Camera m_MainCamera; //�̵��� �⺻ �÷��̾� �����ϴ� ī�޶�
    [SerializeField] private Camera m_SubCamera; //���ݰ��ý� 1��Ī���� ������ �����ϴ� ���� ī�޶�
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
        m_MainCamera.enabled = true;
        m_SubCamera.enabled = false;
    }
    public void ActiveMainCamera()
    {
        if (m_SubCamera.enabled)
            m_SubCamera.enabled = false;

        m_MainCamera.enabled = true;
    }

    public void ActiveSubCamera()
    {
        if (m_MainCamera.enabled)
            m_MainCamera.enabled = false;

        m_SubCamera.enabled = true;
    }
   /* public void ChangeToTPSCam()
    {
        if (FPSCam.gameObject.activeSelf)
            FPSCam.gameObject.SetActive(false);

        TPSCam.gameObject.SetActive(true);
    }
    public void ChangeToFPSCam()
    {
        if (TPSCam.gameObject.activeSelf)
            TPSCam.gameObject.SetActive(false);

        FPSCam.gameObject.SetActive(false);
    }*/
}
