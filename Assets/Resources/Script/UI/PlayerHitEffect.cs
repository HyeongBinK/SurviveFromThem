using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHitEffect : MonoBehaviour
{
    [SerializeField] private Image m_Image;
    private Color m_Alpha;
    private readonly float m_fAlphaSpeed = 1.5f; //ȭ���� �ӰԺ��ϴ� ����Ʈ�� ����ȭ(����ȭ)�Ǵ� �ӵ�
    private readonly float m_fDestroyTime = 1.0f; //����Ʈ�� ������� �ӵ�
    private float m_fDestroyTimer = 0.0f;
    private bool m_bIsActive = false; //�ڷ�ƾ �ߺ�ȣ���� ��������
    private readonly float MaxRGB_A = 100.0f;
    private void Awake()
    {
        m_Alpha = m_Image.color;
        m_Alpha.a = 50;
    }
    public void ActivePlayerHitEffect()
    {
        gameObject.SetActive(true);
        m_Alpha.a = 0.5f;
        m_fDestroyTimer = 0.0f;

        if (!m_bIsActive)
            StartCoroutine(HitEffectActive());
    }
    IEnumerator HitEffectActive()
    {
        m_bIsActive = true;
        while (m_fDestroyTimer <= m_fDestroyTime)
        {
            m_fDestroyTimer += Time.deltaTime;
            m_Alpha.a = Mathf.Lerp(m_Alpha.a, 0, Time.deltaTime * m_fAlphaSpeed);
            m_Image.color = m_Alpha;
            yield return null;
        }
        m_bIsActive = false;
        gameObject.SetActive(false);
    }

}
