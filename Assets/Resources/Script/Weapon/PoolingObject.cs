using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PoolingObject : MonoBehaviour
{
    public WEAPONEFFECT EffectType; //타입
    public Action DisableEvent; // 비활성화 이벤트
    public float ActiveTime; //자동 비활성화까지의 시간

    private void OnEnable()
    {
        StartCoroutine(DisappeardByTime());
    }

    public void OnDisableEvent()
    {
        if (DisableEvent != null)
        {
            DisableEvent();
            DisableEvent = null;
        }
        gameObject.SetActive(false);
    }

    IEnumerator DisappeardByTime() //일정시간이 지나면 자동으로 사라짐
    {
        yield return new WaitForSeconds(ActiveTime);
        OnDisableEvent();
    }
    public void SetTransform(Vector3 Pos, Quaternion Rot)
    {
        gameObject.transform.position = Pos;
        gameObject.transform.rotation = Rot;
        gameObject.SetActive(true);
    }
}
