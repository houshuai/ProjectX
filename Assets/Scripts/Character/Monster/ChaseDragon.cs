using System.Collections;
using UnityEngine;

public class ChaseDragon : Monster
{
    public enum FSMState
    {
        Idle,
        Chase,
        FlyChase,
        Attack,
        FlyAttack,
        TakeOff,
        Land
    }

    public float turnSpeed = 90f;
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float glideSpeed = 5f;
    public float flySpeed = 8f;
    public float flyTime = 50f;
    public float chaseDistance = 30f;
    public float nearby = 10f;
    public float flameRange = 15f;
    public float attackRange = 5f;
    public float flyAttackRange = 10f;
    public float attackDamage = 48f;
    public GameObject flame;
    public ParticleSystem dustParticle;
    public ParticleSystem fireParticle;
    public AudioClip takeOffClip;
    public AudioClip landClip;
    public AudioClip flameClip;
    public AudioClip damageClip;
    public Transform flamePos;
    public FloatVariable playerHealth;

    private ChaseAgent chaseAgent;

    private FSMState currState;
    private float currSpeed;
    private float flyTimer;
    private float AttackTimer;
    private Transform player;
    private bool firstFly, secondFly;
    private bool playerInBox;
    private LayerMask terrainLayer;
    private WaitForSeconds waitForAttack;
    private WaitForSeconds waitClawAttack1;
    private WaitForSeconds waitClawAttack2;

    private void Start()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        health = totalHealth;
        chaseAgent = GetComponent<ChaseAgent>();

        currState = FSMState.Idle;
        dustParticle.Stop();
        fireParticle.Stop();
        var playerObject = GameObject.FindGameObjectWithTag(Tags.Player);
        player = playerObject.transform;
        terrainLayer = 1 << LayerMask.NameToLayer("Terrain");

        waitForAttack = new WaitForSeconds(0.32f);
        waitClawAttack1 = new WaitForSeconds(0.5f);
        waitClawAttack2 = new WaitForSeconds(0.433f);
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        switch (currState)
        {
            case FSMState.Idle:
                UpdateIdle();
                break;
            case FSMState.Chase:
                UpdateChase();
                break;
            case FSMState.FlyChase:
                UpdateFlyChase();
                break;
            case FSMState.Attack:
                UpdateAttack();
                break;
            case FSMState.FlyAttack:
                UpdateFlyAttack();
                break;
            case FSMState.TakeOff:
                UpdateTakeOff();
                break;
            case FSMState.Land:
                UpdateLand();
                break;
            default:
                break;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.Player))
        {
            playerInBox = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Tags.Player))
        {
            playerInBox = false;
        }
    }

    private void UpdateIdle()
    {
        if (Vector3.Distance(player.position, transform.position) < chaseDistance)
        {
            currState = FSMState.Chase;
            return;
        }


    }

    private void UpdateChase()
    {
        var random = Random.value;

        var distance = Vector3.Distance(player.position, transform.position);
        var angle = Vector3.Angle(transform.forward, player.position - transform.position);

        if (distance < attackRange && angle < 20)
        {
            currState = FSMState.Attack;
            if (random < 0.5f)
            {
                anim.SetTrigger(Hashes.ClawAttackTrigger);
                StartCoroutine(ClawAttack());
            }
            else
            {
                anim.SetTrigger(Hashes.BasicAttackTrigger);
                StartCoroutine(BasicAttack());
            }

            return;
        }

        if (distance > attackRange && distance < flameRange && angle < 30 && random < 0.01f)
        {
            currState = FSMState.Attack;
            anim.SetTrigger(Hashes.FlameAttackTrigger);
            return;
        }

        if (distance < nearby)
        {
            chaseAgent.speed = walkSpeed;
        }
        else
        {
            chaseAgent.speed = runSpeed;
        }

        chaseAgent.Chase(player.position);

        anim.SetFloat(Hashes.SpeedFloat, chaseAgent.actualSpeed);
    }

    private void UpdateFlyChase()
    {
        var distance = Vector3.Distance(player.position, transform.position);
        var angle = Vector3.Angle(transform.forward, player.position - transform.position);

        if (distance < flyAttackRange && angle < 45)
        {
            currState = FSMState.FlyAttack;
            anim.SetTrigger(Hashes.FlyFlameAttackTrigger);
            audioSource.clip = flameClip;
            audioSource.Play();
            return;
        }

        if (flyTimer >= flyTime)
        {
            flyTimer = 0;
            currState = FSMState.Land;
            anim.SetBool(Hashes.FlyBool, false);
            return;
        }

        if (distance < nearby)
        {
            chaseAgent.speed = glideSpeed;
        }
        else
        {
            chaseAgent.speed = flySpeed;
        }

        chaseAgent.Chase(player.position);

        flyTimer += Time.deltaTime;
    }

    private void UpdateAttack()
    {
        if (AttackTimer < 0.7f)
        {
            AttackTimer += Time.deltaTime;
            return;
        }

        var animStateHase = anim.GetCurrentAnimatorStateInfo(0).shortNameHash;

        if (animStateHase == Hashes.FlameAttackState)
        {
            if (!flame.activeSelf)
            {
                flame.SetActive(true);
            }

            RaycastHit hit;
            if (Physics.Raycast(flamePos.position, flamePos.forward, out hit, 15))
            {
                if (hit.collider.CompareTag(Tags.Player))
                {
                    playerHealth.Value -= attackDamage;
                }

            }
        }
        else if (animStateHase == Hashes.LocomotionState)
        {
            AttackTimer = 0;
            currState = FSMState.Chase;
            flame.SetActive(false);
        }

    }

    private void UpdateFlyAttack()
    {
        flyTimer += Time.deltaTime;

        AttackTimer += Time.deltaTime;
        if (AttackTimer < 1f)   //  the state of animator need time to shift
        {
            return;
        }

        if (!flame.activeSelf)
        {
            flame.SetActive(true);
            fireParticle.Play();
        }


        RaycastHit hit;
        if (Physics.Raycast(flamePos.position, flamePos.forward, out hit, 20, terrainLayer))
        {
            fireParticle.transform.position = hit.point;//new Vector3(hit.point.x, 1, hit.point.z);

            if (AttackTimer > 1.7f)  //after this, the flame attack on the ground and spreaded
            {
                if (Vector3.Distance(hit.point, player.position) < 7)
                {
                    playerHealth.Value -= attackDamage;
                }
            }
        }


        if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash == Hashes.FlyState)
        {
            AttackTimer = 0;
            flame.SetActive(false);
            currState = FSMState.FlyChase;
        }

    }

    private void UpdateTakeOff()
    {
        var nameHash = anim.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (nameHash == Hashes.TakeOffState)
        {
            if (audioSource.clip != takeOffClip)
            {
                audioSource.clip = takeOffClip;
            }
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else if (nameHash == Hashes.FlyState)
        {
            currState = FSMState.FlyChase;
        }

    }

    private void UpdateLand()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash == Hashes.LocomotionState)
        {
            currState = FSMState.Chase;
        }

    }

    private IEnumerator BasicAttack()
    {
        yield return waitForAttack;
        Attack();
    }

    private IEnumerator ClawAttack()
    {
        yield return waitClawAttack1;
        Attack();
        yield return waitClawAttack2;
        Attack();
    }

    private void Attack()
    {
        if (playerInBox)
        {
            playerHealth.Value -= attackDamage;
        }
    }

    private void PlayLandSound()
    {
        audioSource.clip = landClip;
        audioSource.Play();
        StartCoroutine(PlayDustParticle());
    }

    private IEnumerator PlayDustParticle()
    {
        yield return new WaitForSeconds(0.9f);
        dustParticle.Play();
    }

    public override void TakeDamage(float damage)
    {
        if (isDead)
        {
            return;
        }

        if (currState == FSMState.TakeOff)  // do not take damage when take off
        {
            return;
        }

        base.TakeDamage(damage);

        if ((health < 700 && !firstFly) || (health < 400 && !secondFly))
        {
            audioSource.clip = damageClip;
            audioSource.Play();
            anim.SetTrigger(Hashes.DamageTrigger);

            currState = FSMState.TakeOff;
            anim.SetBool(Hashes.FlyBool, true);
            if (!firstFly)
            {
                firstFly = true;
            }
            else
            {
                secondFly = true;
            }
        }

    }
}
