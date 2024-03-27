using UnityEngine;

public class MobAttackRange : MonoBehaviour
{
    public int Atk;
    public bool IsDamage = false; //데미지가 한번만 들어가게 제어

    private void OnEnable()
    {
        IsDamage = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsDamage)
        {
            if (!GameManager.Instance) return;
            switch (other.tag)
            {

                case "Player":
                    GameManager.Instance.GetPlayerData.GetDamage(GameManager.Instance.MakeRandomDamage(Atk));
                    break;
                case "Provoke":
                    other.gameObject.SendMessage("GetDamage", GameManager.Instance.MakeRandomDamage(Atk), SendMessageOptions.DontRequireReceiver);
                    break;
            }

            IsDamage = true;
        }
    }
}

