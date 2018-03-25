public sealed class ChaseSkeleton : Skeleton
{
    private ChaseAgent chaseAgent;
    
    protected override void Start()
    {
        base.Start();

        chaseAgent = GetComponent<ChaseAgent>();
        chaseAgent.speed = patrolSpeed;
    }

    protected override void UpdatePatrol()
    {
        float distance, angle;
        PlayerSite(out distance, out angle);

        if (playerHealth.Value > 0 && distance < sightRange && angle < sightAngle / 2)
        {
            ChangeToChase();
            return;
        }

        //chaseAgent.Chase(transform.position);

        anim.SetFloat(Hashes.SpeedFloat, chaseAgent.actualSpeed);
    }

    protected override void UpdateChase()
    {
        base.UpdateChase();

        if (currState == FSMState.Chase)
        {
            chaseAgent.Chase(changeCharacter.currCharacter.position);
            anim.SetFloat(Hashes.SpeedFloat, chaseAgent.actualSpeed);
        }
    }

    protected override void ChangeToPatrol()
    {
        currState = FSMState.Patrol;
        chaseAgent.speed = patrolSpeed;
    }

    protected override void ChangeToChase()
    {
        currState = FSMState.Chase;
        chaseAgent.speed = chaseSpeed;
    }

    protected override void ChangeToAttack()
    {
        currState = FSMState.Attack;
    }
}
