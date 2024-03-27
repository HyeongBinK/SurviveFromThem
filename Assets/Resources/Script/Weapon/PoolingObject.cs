using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PoolingObject : MonoBehaviour
{
    public WEAPONEFFECT EffectType; //Ÿ��
    public Action DisableEvent; // ��Ȱ��ȭ �̺�Ʈ
    public float ActiveTime; //�ڵ� ��Ȱ��ȭ������ �ð�

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

    IEnumerator DisappeardByTime() //�����ð��� ������ �ڵ����� �����
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
