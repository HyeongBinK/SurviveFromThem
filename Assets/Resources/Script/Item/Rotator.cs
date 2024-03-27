using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// ���� ������Ʈ�� ���������� ȸ���ϴ� ��ũ��Ʈ
public class Rotator : MonoBehaviour
{
    private const float rotationSpeed = 180f;

    private void OnEnable()
    {
        StartCoroutine(RotateUpdate());
    }
    IEnumerator RotateUpdate()
    {
        while(gameObject.activeSelf)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            yield return null;
        }
    }
}
