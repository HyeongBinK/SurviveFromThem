using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class FloatingDamageText : MonoBehaviour
{
    private readonly float m_fFloatingSpeed = 100.0f; //데미지가 위로 떠오르는 속도
    private readonly float m_fAlphaSpeed = 10.0f; //데미지텍스트가 투명화되는 속도
    private readonly float m_fDestroyTime = 1.0f; //데미지텍스트가 사라지는데 걸리는 시간
    [SerializeField] private Text m_Text;
    private Color m_Alpha;
    public int damage;
    private float m_fDestroyTimer = 0.0f;
    private readonly int NormalDamageTextFontSize = 24; //일반데미지의 폰트사이즈
    private readonly int CriticalDamageTextFontSize = 30; //크리티컬데미지의 폰트사이즈
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
            transform.Translate(new Vector2(0, m_fFloatingSpeed * Time.deltaTime)); // 텍스트 위치

            m_Alpha.a = Mathf.Lerp(m_Alpha.a, 0, Time.deltaTime * m_fAlphaSpeed); // 텍스트 알파값
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
