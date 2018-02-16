using UnityEngine;

public class ChaseSkeleton : Monster
{
    enum FSMState
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

    private HealthBoard healthBoard;
    private ChaseAgent chaseAgent;
    private float attackTime = 2.767f;
    private float attackTimer;
    private float halfAttackAngle = 40;

    private FSMState currState;
    private Transform playerPos;

    private void Start()
    {
        healthBoard = GetComponentInChildren<HealthBoard>();
        chaseAgent = GetComponent<ChaseAgent>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        health = totalHealth;
        chaseAgent.speed = patrolSpeed;
        currState = FSMState.Patrol;

        var playerObj = GameObject.FindGameObjectWithTag(Tags.Player);
        playerPos = playerObj.transform;
        isDead = false;
    }

    private void Update()
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

    private void UpdatePatrol()
    {
        var distance = Vector3.Distance(transform.position, playerPos.position);
        var angle = Vector3.Angle(transform.forward, playerPos.position - transform.position);

        if (playerHealth.Value > 0 && distance < sightRange && angle < sightAngle / 2)
        {
            ChangeToChase();
            return;
        }
        
        //chaseAgent.Chase(transform.position);

        anim.SetFloat(Hashes.SpeedFloat, chaseAgent.actualSpeed);
    }

    private void UpdateChase()
    {
        var distance = Vector3.Distance(transform.position, playerPos.position);
        var angle = Vector3.Angle(transform.forward, playerPos.position - transform.position);

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

        chaseAgent.Chase(playerPos.position);

        anim.SetFloat(Hashes.SpeedFloat, chaseAgent.actualSpeed);
    }

    private void UpdateAttack()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            return;
        }

        var distance = Vector3.Distance(transform.position, playerPos.position);
        var angle = Vector3.Angle(transform.forward, playerPos.position - transform.position);

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
        if (Vector3.Angle(transform.forward, playerPos.position - transform.position) < halfAttackAngle)
        {
            var distance = Vector3.Distance(transform.position, playerPos.position);
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

    public void ChangeToPatrol()
    {
        currState = FSMState.Patrol;
        chaseAgent.speed = patrolSpeed;
    }

    private void ChangeToChase()
    {
        currState = FSMState.Chase;
        chaseAgent.speed = chaseSpeed;
    }

    private void ChangeToAttack()
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
