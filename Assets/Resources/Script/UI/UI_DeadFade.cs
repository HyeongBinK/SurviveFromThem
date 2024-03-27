using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DeadFade : MonoBehaviour
{
    [SerializeField] private Image m_BackgroundImage; //�����̹����� ��ο���¿��� ���� ��������ҿ���
    [SerializeField] private Text RebirthTimeText; //��Ȱ(������)���� ���� �ð�Ÿ�̸�
    private Color m_Alpha; //�����̹�����A(����)�� ����
    private readonly float m_fAlphaSpeed = 0.1f; //����ȭ���� ����ȭ(����ȭ)�Ǵ� �ӵ�
    private float m_fDestroyTime = 5.0f; //���̵尡 ������� �ӵ�
    private float m_fDestroyTimer = 0.0f;
    private bool m_bIsActive = false; //�ڷ�ƾ �ߺ�ȣ���� ��������

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
    IEnumerator DeadFadeEvent() //����ȭ���� �����ϰ�
    {
        m_bIsActive = true;
        while (m_fDestroyTimer <= m_fDestroyTime)
        {
            //����ȭ���� �������� ����Ʈ
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
