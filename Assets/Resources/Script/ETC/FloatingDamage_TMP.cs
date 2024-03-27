using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class FloatingDamage_TMP : MonoBehaviour
{
    [SerializeField] private TextMeshPro m_TextMesh;
    private readonly float m_fFloatingSpeed = 1.0f; //�������� ���� �������� �ӵ�
    private readonly float m_fAlphaSpeed = 8.0f; //�������ؽ�Ʈ�� ����ȭ�Ǵ� �ӵ�
    private readonly float m_fDestroyTime = 1.0f; //�������ؽ�Ʈ�� ������µ� �ɸ��� �ð�
    private Color m_Alpha;
    private float m_fDestroyTimer = 0.0f;
    private readonly float NormalDamageTextFontSize = 2.5f; //�Ϲݵ������� ��Ʈ������
    private readonly float CriticalDamageTextFontSize = 3.0f; //ũ��Ƽ�õ������� ��Ʈ������
    public event Action DamageTextDisable;

    private void Awake()
    {
        m_Alpha = m_TextMesh.color;
    }

    IEnumerator UpdateFloatingText()
    {
        while (m_fDestroyTimer <= m_fDestroyTime)
        {
            m_fDestroyTimer += Time.deltaTime;
            transform.Translate(new Vector3(0, m_fFloatingSpeed * Time.deltaTime, 0)); // �ؽ�Ʈ ��ġ

            m_Alpha.a = Mathf.Lerp(m_Alpha.a, 0, Time.deltaTime * m_fAlphaSpeed); // �ؽ�Ʈ ���İ�
            m_TextMesh.color = m_Alpha;
            yield return null;
        }
        OnDisableEvent();
    }
    public void Init(int Damage, bool IsCritical, Transform TR)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        gameObject.transform.position = TR.position;
        gameObject.transform.rotation = TR.rotation;

        if (IsCritical)
        {
            m_Alpha.g = 0;
            m_TextMesh.fontSize = CriticalDamageTextFontSize;
        }
        else
        {
            m_Alpha.g = 170;
            m_TextMesh.fontSize = NormalDamageTextFontSize;
        }
        m_Alpha.a = 255;
        m_TextMesh.text = Damage.ToString();
        m_fDestroyTimer = 0.0f;
        StartCoroutine(UpdateFloatingText());
    }

    public void OnDisableEvent()
    {
        if (DamageTextDisable != null)
        {
            DamageTextDisable();
            DamageTextDisable = null;
        }
        gameObject.SetActive(false);
    }
}
