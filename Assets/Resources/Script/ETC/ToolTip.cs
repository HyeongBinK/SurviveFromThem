using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    [SerializeField] private Text tooltip; //ÅøÆÁ

    public void SetToolTipPosition(float NewX, float NewY)
    {
        gameObject.transform.position = new Vector2(NewX, NewY);
    }

    public void SetToolTipText(string NewText)
    {
        tooltip.text = NewText;
    }

    public string GetText()
    {
        return tooltip.text;
    }
}
