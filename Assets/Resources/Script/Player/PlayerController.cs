using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform m_CameraArm; //플레이어를 따라가는 카메라
    [SerializeField] private Transform m_PlayerEyes; //플레이어의 시야
    [SerializeField] private GameObject m_PlayerBody; //플레이어의 몸통 매쉬(시점 전환시 몸통을 비활성화 시킬예정)

    private float m_fMoveSpeed; // 앞뒤 움직임의 속도
    private float m_fBaseSpeed; // 기본이동 속도
    private float m_fRunSpeed { get { return m_fBaseSpeed * 2; } } // 뛰기이동 속도
    private Vector3 m_MoveForce; // 이동 힘(x,z와 y축을 별도로 계산해 실제 이동에 적용)

    private float m_fJumpForce = 8; //점프 힘
    private bool m_bIsGrounded; //땅에 있을때
    private bool m_bIsDead; //체력이 0이하로 떨어져 사망한 상태(일정시간후 부활)
    private bool m_bIsCanAct = true; // true : 조작가능, false : 조작불능
    private Animator m_PlayerAnimator; // 플레이어 캐릭터의 애니메이터
    private CharacterController m_CharacterController; //플레이어 캐릭터 콘트롤러

    private readonly float m_fGravity = -18.6f;
    private readonly float BASESPEED = 5;
    private readonly KeyCode KeyCodeRun = KeyCode.LeftShift; //달리기 키
    private readonly KeyCode KeyCodeJump = KeyCode.Space; //점프 키

    private void Awake()
    {
        // 사용할 컴포넌트들 가져오기
        m_PlayerAnimator = GetComponent<Animator>();
        m_CharacterController = GetComponent<CharacterController>();
        m_fBaseSpeed = BASESPEED;
    }

    private void Start()
    {
        if(GameManager.Instance)
            GameManager.Instance.LockMouse();
        Init();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!m_bIsGrounded)
        {
            if (collision.gameObject.tag == "Ground")
            {
                SoundManager.Instance.PlayFootStepSound("Land");
                m_bIsGrounded = true;
            }
        }
    }
    private void Init()
    {
        m_bIsGrounded = true;
        m_bIsDead = false;
        m_bIsCanAct = true;
        StartCoroutine(ActUpdate());
    }

    IEnumerator ActUpdate() //행동상태 업데이트 코루틴
    {
        while (true)
        {
            if (m_bIsCanAct)
            {
                if (GameManager.Instance)
                    m_bIsDead = GameManager.Instance.m_bIsDead;

                if (!m_bIsDead)
                {
                    if (!m_bIsGrounded || !m_CharacterController.isGrounded)
                    {
                        m_MoveForce.y += m_fGravity * Time.deltaTime;
                    }

                    LookAround();
                    UpdateJump();
                    Move();

                    m_CharacterController.Move(m_MoveForce * Time.deltaTime);
                }
            }
            yield return null;
        }
    }
    private void LookAround()
    {
        Vector2 MouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector3 CamAngle = m_CameraArm.rotation.eulerAngles;
      
        m_CameraArm.rotation = Quaternion.Euler(CamAngle.x - MouseDelta.y, CamAngle.y + MouseDelta.x, CamAngle.z);
        m_PlayerEyes.rotation = m_CameraArm.rotation;
    }
    private void Move()
    {
        float X = Input.GetAxisRaw("Horizontal");
        float Z = Input.GetAxisRaw("Vertical");

        Vector2 MoveInput = new Vector2(X, Z);
        bool IsMove = MoveInput.magnitude != 0;
        if (IsMove)
        {
            bool IsRun = false;
            string SFXSoundName;
            float value;

            //옆이나 뒤로 이동할 때는 뛰기 막기
            if (Z > 0) IsRun = Input.GetKey(KeyCodeRun);
            value = IsRun == true ? m_fRunSpeed : m_fBaseSpeed;
            SFXSoundName = IsRun == true ? "Run" : "Walk";
            m_fMoveSpeed = Mathf.Max(0, value);

            if (m_bIsGrounded)
            {
                if (!SoundManager.Instance.GetIsFootStepSoundPlaying())
                {
                    SoundManager.Instance.SetFootStepSoundLoop();
                    SoundManager.Instance.PlayFootStepSound(SFXSoundName);
                }
            }
            else
            {
                SoundManager.Instance.StopFootStepSound();
            }
        }
        else
            SoundManager.Instance.StopFootStepSound();

        Vector3 LookForward = new Vector3(m_CameraArm.forward.x, 0f, m_CameraArm.forward.z).normalized;
        Vector3 LookRight = new Vector3(m_CameraArm.right.x, 0f, m_CameraArm.right.z).normalized;
        Vector3 MoveDir = LookForward * MoveInput.y + LookRight * MoveInput.x;

        m_CharacterController.transform.forward = LookForward;
        transform.position += MoveDir * Time.deltaTime * m_fMoveSpeed;

        m_PlayerAnimator.SetFloat("Movement", MoveInput.magnitude);
    }
    private void UpdateJump()
    {
        if (Input.GetKeyDown(KeyCodeJump))
        {
            if (m_CharacterController.isGrounded)
            {
                m_MoveForce.y = m_fJumpForce;
                SoundManager.Instance.PlaySFX("Jump");
                // m_PlayerAnimator.SetTrigger("Jump"); //플레이어 점프애니없음
                m_bIsGrounded = false;
            }
        }
    }
    public void HidePlayerBody()
    {
        m_PlayerBody.SetActive(false);
    }
    public void ShowPlayerBody()
    {
        m_PlayerBody.SetActive(true);
    }
    public void SetIsCanAct(bool NewBoolean)
    {
        m_bIsCanAct = NewBoolean;
        if(!m_bIsCanAct)
            m_PlayerAnimator.SetFloat("Movement", 0);
    }

    public void SetSpeed(float NewValue)
    {
        m_fBaseSpeed = BASESPEED + NewValue;
    }

    public void SetPlayerTransform(Vector3 NewPos, Quaternion NewRot)
    {
        m_CameraArm.gameObject.transform.rotation = NewRot;
        m_CharacterController.transform.position = NewPos;
    }
}
