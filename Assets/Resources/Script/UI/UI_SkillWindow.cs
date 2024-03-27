using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillWindow : MonoBehaviour
{
    [SerializeField] private SkillSlot[] SkillSlots;
    [SerializeField] private Text SkillPointText;

    private void OnEnable()
    {
        RefreshAllSkillSlot();
    }
    public void RefreshAllSkillSlot()
    {
        SetSkillPointText();
        foreach (SkillSlot Slot in SkillSlots)
        {
            Slot.Refresh();
        }
    }
    public void SetSkillPointText()
    {
        if (!GameManager.Instance) return;
        SkillPointText.text = GameManager.Instance.GetPlayerData.GetPlayerStatusData.SkillPoint.ToString();
    }

    public Sprite GetSkillImage(int SlotNumber)
    {
        if (SlotNumber >= SkillSlots.Length || SlotNumber < 0) return null;
        return SkillSlots[SlotNumber].GetSkillImage;
    }
}
