using UnityEngine;

public enum FIRETYPE
{
    START = 0,
    RAYCAST = 0,//기본총(권총, 소총, 샷건 같은 타입)
    PROJECTILE = 1, //로켓런쳐, 대포 같은 폭발타입
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
    //무기 정보
    public FIRETYPE FireType;
    public WEAPONTYPE WeaponType;
    public int WeaponUniqueNumber = -1; //무기고유번호
    public string WeaponName; //무기이름
    public int MaxAmmo; //한번에 리로드 가능한 최대탄수

    public float ReloadTime; //리로드 하는데 걸리는 시간
    public float RateOfFire; //발사속도
    public float DelayBeforeFire; //발사할때 까지 걸리는 속도(일반적인 총은 0)
    public string FireSoundName; //총 발사 소리 이름
    public int ShotPerRound; //한번에 발사되는 총알의 갯수(샷건 같은 경우 수치변경 그외 1)

    //사격시 정확도 감소,회복수치
    public float MaxAccuracy; //총기의 최대 정확도
    public float AccuracyDropPerShot; //총을 쏠때마다 떨어지는 정확도          
    public float AccuracyRecoverRate; //총을 쏜후 회복되는 정확도수치		

    // 사격시 반동 수치(Recoil)
    public float RecoilKickBackMin; //반동 효과시 뒤로밀려나는 최소수치
    public float RecoilKickBackMax; //반동 효과시 뒤로밀려나는 최대수치
    public float RecoilRotationMin; //반동 효과시 무기가 옆으로 살짝 돌아가는 최소 수치
    public float RecoilRotationMax; //반동 효과시 무기가 옆으로 살짝 돌아가는 최대 수치
    public float RecoilRecoveryRate; //반동 후 원자세까지 회복되는 주기

    // Burst
    public int m_iBurstRate = 5; // The number of shots fired per each burst
    public float m_fBurstPause = 0.0f; // The pause time between bursts

    // 무기 스테이터스
    public int ATK; //공격력 수치
    public int Price; //무기 해금 가격
    public int Reinforce; //현재 강화수치
    public int Max_Reinforce; //한계 강화수치
    public int ReinforcePerATK; //강화 수치당 공격력 증가량
    public int ReinforcePrice; //강화수치 = 해당수치 * 레벨
    public int TotalATK { get { return ATK + (ReinforcePerATK * Reinforce); } } //강화수치에 따른 최종 무기 공격력
    public int TotalReinforcePrice { get { return Price + (ReinforcePrice *Reinforce); } } //강화수치에 따른 최종 무기 강화 비용
    public void SetReinforce(int NewReinforce)
    {
        Reinforce = NewReinforce;
    }
}
