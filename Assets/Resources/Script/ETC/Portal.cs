using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private WarpPoint WP;
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            GameManager.Instance.TeleportPlayer(WP);
        }
    }

}
