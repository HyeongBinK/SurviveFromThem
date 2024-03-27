using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class UI_UserData_1 : MonoBehaviour
{
    [SerializeField] private Image m_UserIcon; 
    [SerializeField] private Slider m_HPBar;
    [SerializeField] private Slider m_MPBar;
    [SerializeField] private Slider m_EXPBar;
    [SerializeField] private Text m_HPText;
    [SerializeField] private Text m_MPText;
    [SerializeField] private Text m_EXPText;
    [SerializeField] private Text m_NameText;
    [SerializeField] private Text m_LevelText;

    public void SetUserIcon(Image NewImage)
    {
        m_UserIcon = NewImage;
    }
    public void SetHPSlider(int CurHP, int MaxHP)
    {
        m_HPText.text = MakeSliderText(CurHP, MaxHP);
        m_HPBar.value = (float)CurHP / (float)MaxHP;
    }
    public void SetMPSlider(int CurMP, int MaxMP)
    {
        m_MPText.text = MakeSliderText(CurMP, MaxMP);
        m_MPBar.value = (float)CurMP / (float)MaxMP;
    }
    public void SetEXPSlider(int CurEXP, int MaxEXP)
    {
        m_EXPText.text = MakeSliderText(CurEXP, MaxEXP);
        m_EXPBar.value = (float)CurEXP / (float)MaxEXP;
    }
    public void SetLevelText(int Level)
    {
        m_LevelText.text = Level.ToString();
    }
    public void SetNameText(string Name)
    {
        m_NameText.text = Name;
    }
    private string MakeSliderText(int Cur, int Max)
    {
        StringBuilder TextMaker = new StringBuilder();
        TextMaker.Append(Cur);
        TextMaker.Append("/");
        TextMaker.Append(Max);
        return TextMaker.ToString();
    }
}
