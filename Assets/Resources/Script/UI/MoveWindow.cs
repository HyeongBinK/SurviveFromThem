using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveWindow : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private Vector2 m_Transform; //화면상 위치
    private Vector2 m_DefaultPosition; //디폴트위치
    private void Awake()
    {
        m_DefaultPosition = gameObject.transform.position;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (UIManager.Instance.GetDraggedObject.IsDrag) return;
        m_Transform = gameObject.transform.position;
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("DragStart");
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (UIManager.Instance.GetDraggedObject.IsDrag) return;
        gameObject.transform.position = m_Transform;
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("DragEnd");
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (UIManager.Instance.GetDraggedObject.IsDrag) return;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Vector2 CurrentPosition = eventData.position;
            gameObject.transform.position = CurrentPosition;
            m_Transform = CurrentPosition;
        }
    }
}
