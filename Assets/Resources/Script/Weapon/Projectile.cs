using UnityEngine;
using System.Collections;
using System;
public enum PROJECTILETYPE
{
	START,
	STANDARD = 0,
	SEEKER, //추격(유도)탄
	CLUSTERBBOMB,
	END
}
public enum DamageType
{
	START,
	DIRECT = 0, //맞은적에게만
	EXPLOSION, //광역
	END
}
public class Projectile : MonoBehaviour
{
	public PROJECTILEPREFAB ProjectileAppearance; //외형
	public PROJECTILETYPE m_ProjectileType = PROJECTILETYPE.STANDARD; //표준은 수직으로 날라가고 Seeker 는 유도탄
	public DamageType m_DamageType = DamageType.DIRECT; //Direct 는 맞은 대상만 Explosion 은 폭발범위내 전부 데미지
	private Rigidbody m_Body;
	public DamageWithIsCritical m_DWC; //데미지와 크리티컬 발생여부
	public float m_Sfpeed = 100.0f; //투사체(Projectile) 발사속도
	public float m_fInitialForce = 1000.0f; //투사체가 발사될때 받는 힘

	public float m_fSeekRate = 1.0f; //적 탐색 속도
	public string m_SeekTag = "Enemy"; //유도탄이 추격할 태그
	public WEAPONEFFECT ExplosionEffect; //폭발효과
	public float m_fTargetListUpdateRate = 5.0f; //적리스트 업데이트 주기

	public GameObject m_ClusterBomb; //클러스터폭탄 일때 클러스터폭탄 오브젝트
	public int n_iClusterBombNum = 6; //클러스터 폭탄이 폭발한후 확산되는 작은 폭발물 수
	private float m_fTargetListUpdateTimer = 0.0f;  //적리스트 업데이트 주기 타이머
	private GameObject[] m_EnemyList; //탐색된 적리스트
	[SerializeField] private float m_ActiveTime = 5.0f; //활성화할 시간

	public Action DisableEvent; //풀링을 사용할것이기에 비활성화 이벤트
	public void OnDisableEvent()
	{
		if (DisableEvent != null)
		{
			DisableEvent();
			DisableEvent = null;
		}
		gameObject.SetActive(false);
	}
	private void Awake()
	{
		m_Body = gameObject.GetComponent<Rigidbody>();
	}
	IEnumerator DisappeardByTime() //일정시간이 지나면 자동으로 사라짐
	{
		yield return new WaitForSeconds(m_ActiveTime);
		Explode(transform.position);
	}

	private void OnEnable()
	{
		m_EnemyList = null;
		UpdateEnemyList();
		m_Body.AddRelativeForce(0, 0, m_fInitialForce);
		StartCoroutine(DisappeardByTime());
		StartCoroutine(ProjectUpdate());
	}

	IEnumerator ProjectUpdate()
	{
		while (gameObject.activeSelf)
		{
			if (m_fInitialForce == 0)
				m_Body.velocity = transform.forward * m_Sfpeed;

			if (m_ProjectileType == PROJECTILETYPE.SEEKER) //유도탄일 경우 적을 탐색
			{
				m_fTargetListUpdateTimer += Time.deltaTime; //적리스트갱신타이머 

				if (m_fTargetListUpdateTimer >= m_fTargetListUpdateRate)
					UpdateEnemyList();

                if (m_EnemyList != null)
                {
                    float greatestDotSoFar = -1.0f;
                    Vector3 target = transform.forward * 1000;
                    foreach (GameObject enemy in m_EnemyList)
                    {
                        if (enemy != null)
                        {
                            Vector3 direction = enemy.transform.position - transform.position;
                            float dot = Vector3.Dot(direction.normalized, transform.forward);
                            if (dot > greatestDotSoFar)
                            {
                                target = enemy.transform.position + new Vector3(0,1,0);
                                greatestDotSoFar = dot;
                            }
                        }
                    }

                    Quaternion targetRotation = Quaternion.LookRotation(target - transform.position);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * m_fSeekRate);
                }
			}
			yield return null;
		}
	}

	void UpdateEnemyList()
	{
		if (m_ProjectileType != PROJECTILETYPE.SEEKER) return; //유도탄이 아니라면 종료
		m_EnemyList = GameObject.FindGameObjectsWithTag(m_SeekTag);
		m_fTargetListUpdateTimer = 0.0f;
	}

	void OnCollisionEnter(Collision col)
	{
		Hit(col);
	}

	void Hit(Collision col)
	{
		if (m_DamageType == DamageType.DIRECT)
		{
			if (col.gameObject.tag == "Enemy")
			{
				col.collider.gameObject.SendMessageUpwards("GetDamage", m_DWC, SendMessageOptions.DontRequireReceiver);
			}
		}
		Explode(col.contacts[0].point); //폭발효과생성
	}

	void Explode(Vector3 position) //뭔가에 부딛혀 폭발
	{
		if (WeaponEffectPoolingManager.Instance)
			WeaponEffectPoolingManager.Instance.CreateEffectPrefab(ExplosionEffect, position, Quaternion.identity);

		OnDisableEvent();
	}

	// Modify the damage that this projectile can cause
	public void SetDammage(DamageWithIsCritical DWC)
	{
		m_DWC = DWC;
	}

	// Modify the inital force
	public void MultiplyInitialForce(float amount)
	{
		m_fInitialForce *= amount;
	}
	public void SetPosition(Vector3 Position, Quaternion Rotaion)
	{
		gameObject.transform.position = Position;
		gameObject.transform.rotation = Rotaion;
		gameObject.SetActive(true);
	}
}

