using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager m_instance; //싱글톤 할당

    public static CameraManager Instance
    {
        get
        {
            // 만약 싱글톤 변수에 아직 오브젝트가 할당되지 않았다면
            if (m_instance == null)
            {
                // 씬에서 GameManager 오브젝트를 찾아 할당
                m_instance = FindObjectOfType<CameraManager>();
            }
            // 싱글톤 오브젝트를 반환
            return m_instance;
        }
    }

    [SerializeField] private Camera m_MainCamera; //이동시 기본 플레이어 추적하는 카메라
    [SerializeField] private Camera m_SubCamera; //공격개시시 1인칭으로 시점을 변경하는 서브 카메라
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
