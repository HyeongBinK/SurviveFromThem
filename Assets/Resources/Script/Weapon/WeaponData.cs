using UnityEngine;

public enum FIRETYPE
{
    START = 0,
    RAYCAST = 0,//�⺻��(����, ����, ���� ���� Ÿ��)
    PROJECTILE = 1, //���Ϸ���, ���� ���� ����Ÿ��
    END
}
public enum WEAPONTYPE
{
    RIFLE,
    PISTOL,
    SHOTGUN,
    ROCHETRAUNCHER,
    RAILGUN
}
public class WeaponData 
{
    //���� ����
    public FIRETYPE FireType;
    public WEAPONTYPE WeaponType;
    public int WeaponUniqueNumber = -1; //���������ȣ
    public string WeaponName; //�����̸�
    public int MaxAmmo; //�ѹ��� ���ε� ������ �ִ�ź��

    public float ReloadTime; //���ε� �ϴµ� �ɸ��� �ð�
    public float RateOfFire; //�߻�ӵ�
    public float DelayBeforeFire; //�߻��Ҷ� ���� �ɸ��� �ӵ�(�Ϲ����� ���� 0)
    public string FireSoundName; //�� �߻� �Ҹ� �̸�
    public int ShotPerRound; //�ѹ��� �߻�Ǵ� �Ѿ��� ����(���� ���� ��� ��ġ���� �׿� 1)

    //��ݽ� ��Ȯ�� ����,ȸ����ġ
    public float MaxAccuracy; //�ѱ��� �ִ� ��Ȯ��
    public float AccuracyDropPerShot; //���� �򶧸��� �������� ��Ȯ��          
    public float AccuracyRecoverRate; //���� ���� ȸ���Ǵ� ��Ȯ����ġ		

    // ��ݽ� �ݵ� ��ġ(Recoil)
    public float RecoilKickBackMin; //�ݵ� ȿ���� �ڷιз����� �ּҼ�ġ
    public float RecoilKickBackMax; //�ݵ� ȿ���� �ڷιз����� �ִ��ġ
    public float RecoilRotationMin; //�ݵ� ȿ���� ���Ⱑ ������ ��¦ ���ư��� �ּ� ��ġ
    public float RecoilRotationMax; //�ݵ� ȿ���� ���Ⱑ ������ ��¦ ���ư��� �ִ� ��ġ
    public float RecoilRecoveryRate; //�ݵ� �� ���ڼ����� ȸ���Ǵ� �ֱ�

    // Burst
    public int m_iBurstRate = 5; // The number of shots fired per each burst
    public float m_fBurstPause = 0.0f; // The pause time between bursts

    // ���� �������ͽ�
    public int ATK; //���ݷ� ��ġ
    public int Price; //���� �ر� ����
    public int Reinforce; //���� ��ȭ��ġ
    public int Max_Reinforce; //�Ѱ� ��ȭ��ġ
    public int ReinforcePerATK; //��ȭ ��ġ�� ���ݷ� ������
    public int ReinforcePrice; //��ȭ��ġ = �ش��ġ * ����
    public int TotalATK { get { return ATK + (ReinforcePerATK * Reinforce); } } //��ȭ��ġ�� ���� ���� ���� ���ݷ�
    public int TotalReinforcePrice { get { return Price + (ReinforcePrice *Reinforce); } } //��ȭ��ġ�� ���� ���� ���� ��ȭ ���
    public void SetReinforce(int NewReinforce)
    {
        Reinforce = NewReinforce;
    }
}
