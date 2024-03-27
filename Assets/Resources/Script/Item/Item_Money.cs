using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Item_Money : MonoBehaviour
{
    public MONEYTYPE m_eType;  
    private int m_iMoney;
    private float m_fDisappearTime; //사라지는데 걸리는 시간
    public event Action MoneyObjectDisable;
    private readonly float PositionYOffset = 1.0f;

    private void Awake()
    {
        MoneyObjectDisable = null;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (GameManager.Instance)
            {
                GameManager.Instance.GetPlayerData.GetGold(m_iMoney);
                if (SoundManager.Instance)
                    SoundManager.Instance.PlaySFX("PickUpItem");
                OnDisableEvent();
            }
        }
    }
    public void Init(int Money, Transform Tr)
    {
        m_iMoney = MakeRandomMoney(Money);
        transform.position = new Vector3(Tr.position.x + 1, Tr.position.y + PositionYOffset, Tr.position.z);

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        StartCoroutine(DisappearEvent());
    }

    private int MakeRandomMoney(int Money)
    {
        int RandomMoney = UnityEngine.Random.Range((int)((float)Money * 0.5), Money);
        return RandomMoney;
    }

    IEnumerator DisappearEvent()
    {
        if (ItemManager.Instance)
        {
            yield return new WaitForSeconds(ItemManager.Instance.ItemHoldingTime);
            OnDisableEvent();
        }
    }
    public void OnDisableEvent()
    {
        if (MoneyObjectDisable != null)
        {
            MoneyObjectDisable();
            MoneyObjectDisable = null;
        }
        gameObject.SetActive(false);
    }
}
