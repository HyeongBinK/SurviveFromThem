using UnityEngine;

[System.Serializable]
public class MonsterStatus 
{
    public int MobUniqueNumber; //���Ͱ�����ȣ
    public string MobName; //���� �̸�
    public int MaxHP; //�ִ�ü��
    public int AttackPoint; //���ݷ�
    public float Speed; //�̵��ӵ�
    public float PatrolTime; //�����ֱ�(�پ缺�� ���� �ش��ġ�� 80~120���� ����)
    public float AttackRate; //�����ֱ�
    public float Attack1AnimTime; //���� 1 �ִϸ��̼� ����ð�
    public float Attack2AnimTime; //���� 2 �ִϸ��̼� ����ð�
    public float Attack3AnimTime; //���� 3 �ִϸ��̼� ����ð�
    public float BurfAnimTime; //����/ǥȿ �ִϸ��̼� ����ð� 
    public float DeadAnimTime; //����ִϸ��̼� ����ð�

    public int EXP; //óġ�� �ִ� ����ġ
    public int Gold; //óġ�� �ִ� ��ȭ��(�ش��ġ�� 50~100���� �������)
    public int DropItemUniqueNumber; //��������� ������ȣ
    public int DropRate; //�����۵��Ȯ��

    public int LevelPerMaxHP; //������ ���� �ö󰡴� �ִ� ü�� ��ġ
    public int LevelPerAttackPoint; //������ ���� �ö󰡴� ���ݷ� ��ġ
    public int LevelPerEXP; //������ ���� �ö󰡴� ����ġ �� ��ġ
    public int LevelPerGold; //������ ���� �ö󰡴� ��ȭ �� ��ġ

    public string Attack1EffectSoundName; //���ݽ� ȿ���� 1
    public string Attack2EffectSoundName; //���ݽ� ȿ���� 2 
    public string Attack3EffectSoundName; //���ݽ� ȿ���� 3
    public string BurfEffectSoundName; //����,��ȿ ȿ����
    public string GetDamageEffectSoundName; // �ǰݽ� ȿ����
    public string DeadEffectSoundName; //��� ȿ����

    public void SetStatus(MonsterStatus NewData)
    {
        this.MobUniqueNumber = NewData.MobUniqueNumber;
        this.MobName = NewData.MobName;
        this.MaxHP = NewData.MaxHP;
        this.AttackPoint = NewData.AttackPoint;
        this.Speed = NewData.Speed;
        this.PatrolTime = NewData.PatrolTime;
        this.AttackRate = NewData.AttackRate;
        this.Attack1AnimTime = NewData.Attack1AnimTime;
        this.Attack2AnimTime = NewData.Attack2AnimTime;
        this.Attack3AnimTime = NewData.Attack3AnimTime;
        this.BurfAnimTime = NewData.BurfAnimTime;
        this.DeadAnimTime = NewData.DeadAnimTime;

        this.EXP = NewData.EXP;
        this.Gold = NewData.Gold;
        this.DropItemUniqueNumber = NewData.DropItemUniqueNumber;
        this.DropRate = NewData.DropRate;

        this.LevelPerMaxHP = NewData.LevelPerMaxHP;
        this.LevelPerAttackPoint = NewData.LevelPerAttackPoint;
        this.LevelPerEXP = NewData.LevelPerEXP;
        this.LevelPerGold = NewData.LevelPerGold;
        this.Attack1EffectSoundName = NewData.Attack1EffectSoundName;
        this.Attack2EffectSoundName = NewData.Attack2EffectSoundName;
        this.Attack3EffectSoundName = NewData.Attack3EffectSoundName;
        this.BurfEffectSoundName = NewData.BurfEffectSoundName;
        this.GetDamageEffectSoundName = NewData.GetDamageEffectSoundName;
        this.DeadEffectSoundName = NewData.DeadEffectSoundName;
    }
}
