using UnityEngine;
using UnityEngine.AI;

public sealed class NavSkeleton : Skeleton
{
    public Transform[] patrolPos;

    private NavMeshAgent nav;
    private CapsuleCollider capsuleCollider;
    private int patrolPosCount;
    private int currPatrolPos;
    private int patrolDirection = 1;

    private PlayerMove playerMove;

    protected override void Start()
    {
        base.Start();

        nav = GetComponent<NavMeshAgent>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        if (patrolPos != null)
        {
            patrolPosCount = patrolPos.Length;
        }
        nav.destination = patrolPos[0].position;
        nav.speed = patrolSpeed;

        playerMove = FindObjectOfType<PlayerMove>();
    }

    protected override void UpdatePatrol()
    {
        float distance, angle;
        PlayerSite(out distance, out angle);
        bool isHide = playerMove.isInGrass && playerMove.isCrouch;
        bool isInSight = distance < sightRange && angle < sightAngle / 2;

        if (playerHealth.Value > 0 && !isHide && isInSight)
        {
            ChangeToChase();
            return;
        }

        if (nav.remainingDistance < 0.1f)
        {
            if (currPatrolPos == 0)
            {
                currPatrolPos = 1;
                patrolDirection = 1;
            }
            else if (currPatrolPos == patrolPosCount - 1)
            {
                currPatrolPos = patrolPosCount - 2;
                patrolDirection = -1;
            }
            else
            {
                currPatrolPos += patrolDirection;
            }
            nav.destination = patrolPos[currPatrolPos].position;
        }

        anim.SetFloat(Hashes.SpeedFloat, nav.velocity.magnitude);
    }

    protected override void UpdateChase()
    {
        base.UpdateChase();

        if (currState == FSMState.Chase)
        {
            if (!nav.pathPending)
            {
                nav.destination = changeCharacter.currCharacter.position;
            }

            anim.SetFloat(Hashes.SpeedFloat, nav.velocity.magnitude);
        }

    }

    protected override void ChangeToPatrol()
    {
        currState = FSMState.Patrol;
        nav.destination = patrolPos[currPatrolPos].position;
        nav.speed = patrolSpeed;
        nav.isStopped = false;
    }

    protected override void ChangeToChase()
    {
        currState = FSMState.Chase;
        nav.destination = changeCharacter.currCharacter.position;
        nav.speed = chaseSpeed;
        nav.isStopped = false;
    }

    protected override void ChangeToAttack()
    {
        currState = FSMState.Attack;
        nav.isStopped = true;
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if (isDead)
        {
            nav.isStopped = true;
            capsuleCollider.enabled = false;
        }
    }
}
