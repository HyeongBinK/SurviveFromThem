using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class FloatingDamageText : MonoBehaviour
{
    private readonly float m_fFloatingSpeed = 100.0f; //�������� ���� �������� �ӵ�
    private readonly float m_fAlphaSpeed = 10.0f; //�������ؽ�Ʈ�� ����ȭ�Ǵ� �ӵ�
    private readonly float m_fDestroyTime = 1.0f; //�������ؽ�Ʈ�� ������µ� �ɸ��� �ð�
    [SerializeField] private Text m_Text;
    private Color m_Alpha;
    public int damage;
    private float m_fDestroyTimer = 0.0f;
    private readonly int NormalDamageTextFontSize = 24; //�Ϲݵ������� ��Ʈ������
    private readonly int CriticalDamageTextFontSize = 30; //ũ��Ƽ�õ������� ��Ʈ������
    public event Action DamageTextDisable;

    // Start is called before the first frame update
    private void Awake()
    {
        m_Text = GetComponent<Text>();
        m_Alpha = m_Text.color;
    }
    IEnumerator UpdateFloatingText()
    {
        while(m_fDestroyTimer <= m_fDestroyTime)
        {
            m_fDestroyTimer += Time.deltaTime;
            transform.Translate(new Vector2(0, m_fFloatingSpeed * Time.deltaTime)); // �ؽ�Ʈ ��ġ

            m_Alpha.a = Mathf.Lerp(m_Alpha.a, 0, Time.deltaTime * m_fAlphaSpeed); // �ؽ�Ʈ ���İ�
            m_Text.color = m_Alpha;
            yield return null;
        }
        OnDisableEvent();
    }
    public void Init(int Damage, bool IsCritical, Vector2 Position)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        gameObject.transform.position = Position;
 
        if(IsCritical)
        {
            m_Alpha.g = 0;
            m_Text.fontSize = CriticalDamageTextFontSize;
        }
        else
        {
            m_Alpha.g = 170;
            m_Text.fontSize = NormalDamageTextFontSize;
        }
        m_Alpha.a = 255;
        m_Text.text = Damage.ToString();
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
