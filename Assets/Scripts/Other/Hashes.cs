using UnityEngine;

public static class Hashes
{
    public static readonly int DeadTrigger;
    public static readonly int JumpTrigger;
    public static readonly int DamageTrigger;
    public static readonly int KnockBackTrigger;
    public static readonly int SpeedFloat;
    public static readonly int AttackTrigger;
    public static readonly int IdleBool;
    public static readonly int CrouchBool;
    public static readonly int FightBool;
    public static readonly int PunchIndexInt;
    public static readonly int CrouchState;
    public static readonly int FightMoveState;
    public static readonly int DamageState;

    public static readonly int BasicAttackTrigger;
    public static readonly int ClawAttackTrigger;
    public static readonly int FlameAttackTrigger;
    public static readonly int FlyBool;
    public static readonly int FlyFlameAttackTrigger;
    public static readonly int TakeOffState;
    public static readonly int FlyState;
    public static readonly int FlyFlameAttackState;
    public static readonly int LocomotionState;
    public static readonly int FlameAttackState;
    
    static Hashes()
    {
        DeadTrigger = Animator.StringToHash("Dead");
        JumpTrigger = Animator.StringToHash("Jump");
        DamageTrigger = Animator.StringToHash("Damage");
        KnockBackTrigger = Animator.StringToHash("KnockBack");
        SpeedFloat = Animator.StringToHash("Speed");
        AttackTrigger = Animator.StringToHash("Attack");
        IdleBool = Animator.StringToHash("Idle");
        CrouchBool = Animator.StringToHash("Crouch");
        FightBool = Animator.StringToHash("Fight");
        PunchIndexInt = Animator.StringToHash("PunchIndex");
        CrouchState = Animator.StringToHash("Crouch");
        FightMoveState = Animator.StringToHash("FightMove");
        DamageState = Animator.StringToHash("damage_20");

        BasicAttackTrigger = Animator.StringToHash("BasicAttack");
        ClawAttackTrigger = Animator.StringToHash("ClawAttack");
        FlameAttackTrigger = Animator.StringToHash("FlameAttack");
        FlyBool = Animator.StringToHash("Fly");
        FlyFlameAttackTrigger = Animator.StringToHash("FlyFlameAttack");
        TakeOffState = Animator.StringToHash("Take Off");
        FlyState = Animator.StringToHash("Fly");
        FlyFlameAttackState = Animator.StringToHash("Fly Flame Attack");
        LocomotionState = Animator.StringToHash("Locomotion");
        FlameAttackState = Animator.StringToHash("Flame Attack");
    }

}
