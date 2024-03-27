using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform m_CameraArm; //�÷��̾ ���󰡴� ī�޶�
    [SerializeField] private Transform m_PlayerEyes; //�÷��̾��� �þ�
    [SerializeField] private GameObject m_PlayerBody; //�÷��̾��� ���� �Ž�(���� ��ȯ�� ������ ��Ȱ��ȭ ��ų����)

    private float m_fMoveSpeed; // �յ� �������� �ӵ�
    private float m_fBaseSpeed; // �⺻�̵� �ӵ�
    private float m_fRunSpeed { get { return m_fBaseSpeed * 2; } } // �ٱ��̵� �ӵ�
    private Vector3 m_MoveForce; // �̵� ��(x,z�� y���� ������ ����� ���� �̵��� ����)

    private float m_fJumpForce = 8; //���� ��
    private bool m_bIsGrounded; //���� ������
    private bool m_bIsDead; //ü���� 0���Ϸ� ������ ����� ����(�����ð��� ��Ȱ)
    private bool m_bIsCanAct = true; // true : ���۰���, false : ���ۺҴ�
    private Animator m_PlayerAnimator; // �÷��̾� ĳ������ �ִϸ�����
    private CharacterController m_CharacterController; //�÷��̾� ĳ���� ��Ʈ�ѷ�

    private readonly float m_fGravity = -18.6f;
    private readonly float BASESPEED = 5;
    private readonly KeyCode KeyCodeRun = KeyCode.LeftShift; //�޸��� Ű
    private readonly KeyCode KeyCodeJump = KeyCode.Space; //���� Ű

    private void Awake()
    {
        // ����� ������Ʈ�� ��������
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

    IEnumerator ActUpdate() //�ൿ���� ������Ʈ �ڷ�ƾ
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

            //���̳� �ڷ� �̵��� ���� �ٱ� ����
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
                // m_PlayerAnimator.SetTrigger("Jump"); //�÷��̾� �����ִϾ���
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
