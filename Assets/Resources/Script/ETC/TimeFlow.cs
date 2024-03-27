using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeFlow : MonoBehaviour
{
    private readonly float RotateSpeed = 2.0f;
    private readonly float NightRotateSpeed = 5.0f;

    [SerializeField] private float RealRotateSpeed; 
    private void Start()
    {
        RealRotateSpeed = 0;
        StartCoroutine(RotateUpdate());
    }

    IEnumerator RotateUpdate()
    {
        while (gameObject.activeSelf)
        {
            if (transform.rotation.eulerAngles.x > 180)
            {
                RealRotateSpeed = NightRotateSpeed;
            }
            else RealRotateSpeed = RotateSpeed;

            transform.Rotate(RealRotateSpeed * Time.deltaTime, 0, 0);
            yield return null;
        }
    }
}
