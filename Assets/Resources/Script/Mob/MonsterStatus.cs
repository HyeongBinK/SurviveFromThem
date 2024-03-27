using UnityEngine;

[System.Serializable]
public class MonsterStatus 
{
    public int MobUniqueNumber; //몬스터고유번호
    public string MobName; //몬스터 이름
    public int MaxHP; //최대체력
    public int AttackPoint; //공격력
    public float Speed; //이동속도
    public float PatrolTime; //정찰주기(다양성을 위해 해당수치의 80~120프로 랜덤)
    public float AttackRate; //공격주기
    public float Attack1AnimTime; //공격 1 애니메이션 재생시간
    public float Attack2AnimTime; //공격 2 애니메이션 재생시간
    public float Attack3AnimTime; //공격 3 애니메이션 재생시간
    public float BurfAnimTime; //버프/표효 애니메이션 재생시간 
    public float DeadAnimTime; //사망애니메이션 재생시간

    public int EXP; //처치시 주는 경험치
    public int Gold; //처치시 주는 재화량(해당수치의 50~100프로 랜덤드랍)
    public int DropItemUniqueNumber; //드랍아이템 고유번호
    public int DropRate; //아이템드랍확률

    public int LevelPerMaxHP; //레벨에 따라 올라가는 최대 체력 수치
    public int LevelPerAttackPoint; //레벨에 따라 올라가는 공격력 수치
    public int LevelPerEXP; //레벨에 따라 올라가는 경험치 양 수치
    public int LevelPerGold; //레벨에 따라 올라가는 재화 양 수치

    public string Attack1EffectSoundName; //공격시 효과음 1
    public string Attack2EffectSoundName; //공격시 효과음 2 
    public string Attack3EffectSoundName; //공격시 효과음 3
    public string BurfEffectSoundName; //버프,포효 효과음
    public string GetDamageEffectSoundName; // 피격시 효과음
    public string DeadEffectSoundName; //사망 효과음

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
