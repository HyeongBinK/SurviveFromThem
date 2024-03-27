using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// 게임 오브젝트를 지속적으로 회전하는 스크립트
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
