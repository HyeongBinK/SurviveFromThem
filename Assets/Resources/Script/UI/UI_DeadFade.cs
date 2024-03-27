using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DeadFade : MonoBehaviour
{
    [SerializeField] private Image m_BackgroundImage; //바탕이미지를 어두운상태에서 점점 밝아지게할예정
    [SerializeField] private Text RebirthTimeText; //부활(리스폰)까지 남은 시간타이머
    private Color m_Alpha; //바탕이미지의A(투명도)값 조절
    private readonly float m_fAlphaSpeed = 0.1f; //검은화면이 투명화(정상화)되는 속도
    private float m_fDestroyTime = 5.0f; //페이드가 사라지는 속도
    private float m_fDestroyTimer = 0.0f;
    private bool m_bIsActive = false; //코루틴 중복호출을 막기위해

    private void Awake()
    {
        m_Alpha = m_BackgroundImage.color;
        m_fDestroyTime = 5.0f;
    }
    public void ActiveDeadFade()
    {
        gameObject.SetActive(true);
        m_Alpha.a = 1.0f;
        m_fDestroyTimer = 0.0f;
        RebirthTimeText.text = m_fDestroyTime.ToString();

        if (!m_bIsActive)
        {
            StartCoroutine(DeadFadeEvent());
        }
    }
    IEnumerator DeadFadeEvent() //바탕화면을 투명하게
    {
        m_bIsActive = true;
        while (m_fDestroyTimer <= m_fDestroyTime)
        {
            //바탕화면이 맑아지는 이펙트
            m_fDestroyTimer += Time.deltaTime;
            m_Alpha.a = Mathf.Lerp(m_Alpha.a, 0, Time.deltaTime * m_fAlphaSpeed);
            m_BackgroundImage.color = m_Alpha;

            RebirthTimeText.text = ((int)(m_fDestroyTime - m_fDestroyTimer)).ToString();
            yield return null;
        }
        m_bIsActive = false;
        gameObject.SetActive(false);
    }
}
