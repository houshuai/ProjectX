using UnityEngine;

public class Skeleton : Monster
{
    protected enum FSMState
    {
        Patrol,
        Chase,
        Attack,
    }

    public float sightRange = 10f;
    public float sightAngle = 60f;
    public float patrolSpeed = 1f;
    public float chaseSpeed = 2f;
    public float attackRange = 2f;
    public float attackDamage = 24f;
    public AudioClip attackClip;
    public FloatVariable playerHealth;
    
    protected HealthBoard healthBoard;
    protected float attackTime = 2.767f;
    protected float attackTimer;
    protected float halfAttackAngle = 40;
    protected FSMState currState;

    protected override void Start()
    {
        base.Start();

        healthBoard = GetComponentInChildren<HealthBoard>();
    }

    protected void Update()
    {
        if (isDead)
        {
            return;
        }

        switch (currState)
        {
            case FSMState.Patrol:
                UpdatePatrol();
                break;
            case FSMState.Chase:
                UpdateChase();
                break;
            case FSMState.Attack:
                UpdateAttack();
                break;
            default:
                break;
        }
    }

    protected virtual void UpdatePatrol()
    {

    }

    protected virtual void UpdateChase()
    {
        float distance, angle;
        PlayerSite(out distance, out angle);

        if (distance < attackRange && angle < halfAttackAngle)
        {
            ChangeToAttack();
            return;
        }
        else if (distance > sightRange)
        {
            ChangeToPatrol();
            return;
        }
    }

    protected virtual void UpdateAttack()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            return;
        }

        float distance, angle;
        PlayerSite(out distance, out angle);

        if (distance > attackRange || angle > halfAttackAngle)
        {
            ChangeToChase();
            return;
        }

        anim.SetTrigger(Hashes.AttackTrigger);
        if (audioSource.clip != attackClip)
        {
            audioSource.clip = attackClip;
        }
        audioSource.Play();
        attackTimer = attackTime;
    }

    //由animation event 调用
    private void Attack()
    {
        var playerPos = changeCharacter.currCharacter.position;
        if (Vector3.Angle(transform.forward, playerPos - transform.position) < halfAttackAngle)
        {
            var distance = Vector3.Distance(transform.position, playerPos);
            if (distance > 0.3f && distance < 1.5f)
            {
                playerHealth.Value -= attackDamage;
                if (playerHealth.Value <= 0)
                {
                    ChangeToPatrol();
                }
            }
        }
    }

    protected virtual void ChangeToPatrol()
    {
        currState = FSMState.Patrol;
    }

    protected virtual void ChangeToChase()
    {
        currState = FSMState.Chase;
    }

    protected virtual void ChangeToAttack()
    {
        currState = FSMState.Attack;
    }

    public override void TakeDamage(float damage)
    {
        if (isDead)
        {
            return;
        }

        base.TakeDamage(damage);
        healthBoard.ChangeMaterial(health / totalHealth);

        if (currState == FSMState.Patrol)
        {
            ChangeToChase();
        }
    }
}
