using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class FloatingDamage_TMP : MonoBehaviour
{
    [SerializeField] private TextMeshPro m_TextMesh;
    private readonly float m_fFloatingSpeed = 1.0f; //데미지가 위로 떠오르는 속도
    private readonly float m_fAlphaSpeed = 8.0f; //데미지텍스트가 투명화되는 속도
    private readonly float m_fDestroyTime = 1.0f; //데미지텍스트가 사라지는데 걸리는 시간
    private Color m_Alpha;
    private float m_fDestroyTimer = 0.0f;
    private readonly float NormalDamageTextFontSize = 2.5f; //일반데미지의 폰트사이즈
    private readonly float CriticalDamageTextFontSize = 3.0f; //크리티컬데미지의 폰트사이즈
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
            transform.Translate(new Vector3(0, m_fFloatingSpeed * Time.deltaTime, 0)); // 텍스트 위치

            m_Alpha.a = Mathf.Lerp(m_Alpha.a, 0, Time.deltaTime * m_fAlphaSpeed); // 텍스트 알파값
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
