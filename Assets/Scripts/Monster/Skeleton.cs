using UnityEngine;
using UnityEngine.AI;

public class Skeleton : Monster
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
    public Transform[] patrolPos;
    public FloatVariable playerHealth;

    private NavMeshAgent nav;
    private CapsuleCollider capsuleCollider;
    private int patrolPosCount;
    private int currPatrolPos;
    private int patrolDirection = 1;
    private float attackTime = 2.767f;
    private float attackTimer;
    private float halfAttackAngle = 40;

    private FSMState currState;
    private Transform playerPos;
    private PlayerMove playerMove;
    private PlayerAttack playerAttack;

    private void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (patrolPos != null)
        {
            patrolPosCount = patrolPos.Length;
        }
        nav.destination = patrolPos[0].position;
        nav.speed = patrolSpeed;

        currState = FSMState.Patrol;
        var playerObj = GameObject.FindGameObjectWithTag(Tags.Player);
        playerPos = playerObj.transform;
        playerMove = playerObj.GetComponent<PlayerMove>();
        playerAttack = playerObj.GetComponent<PlayerAttack>();
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

        healthSlider.transform.LookAt(Camera.main.transform);
    }

    private void UpdatePatrol()
    {
        var distance = Vector3.Distance(transform.position, playerPos.position);
        var angle = Vector3.Angle(transform.forward, playerPos.position - transform.position);

        if (playerHealth.Value > 0 && !(playerMove.isInGrass && playerMove.isCrouch) &&
             distance < sightRange && angle < sightAngle / 2)
        {
            ChangeToChase();
            playerAttack.EnemyCount++;
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

        if (!nav.pathPending)
        {
            nav.destination = playerPos.position;
        }

        anim.SetFloat(Hashes.SpeedFloat, nav.velocity.magnitude);
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
                if (playerHealth.Value<=0)
                {
                    ChangeToPatrol();
                }
            }
        }
    }

    public void ChangeToPatrol()
    {
        currState = FSMState.Patrol;
        nav.destination = patrolPos[currPatrolPos].position;
        nav.speed = patrolSpeed;
        nav.isStopped = false;
        playerAttack.EnemyCount--;
    }

    private void ChangeToChase()
    {
        currState = FSMState.Chase;
        nav.destination = playerPos.position;
        nav.speed = chaseSpeed;
        nav.isStopped = false;
    }

    private void ChangeToAttack()
    {
        currState = FSMState.Attack;
        nav.isStopped = true;
    }

    public override void TakeDamage(float damage)
    {
        if (isDead)
        {
            return;
        }

        base.TakeDamage(damage);

        if (isDead)
        {
            nav.isStopped = true;
            capsuleCollider.enabled = false;
            playerAttack.EnemyCount--;
        }
        else
        {
            if (currState==FSMState.Patrol)
            {
                ChangeToChase();
                playerAttack.EnemyCount++;
            }
        }
    }
}
