using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Dragon : Monster
{
    protected enum FSMState
    {
        Idle,
        Chase,
        FlyChase,
        Attack,
        FlyAttack,
        TakeOff,
        Land
    }

    #region parameter
    public float turnSpeed = 1f;
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float glideSpeed = 5f;
    public float flySpeed = 8f;
    public float flyTime = 20f;
    public float chaseDistance = 30f;
    public float nearby = 10f;
    public float flameRange = 15f;
    public float attackRange = 5f;
    public float flyAttackRange = 10f;
    public float attackDamage = 38f;
    public GameObject flame;
    public ParticleSystem dustParticle;
    public ParticleSystem fireParticle;
    public AudioClip takeOffClip;
    public AudioClip landClip;
    public AudioClip flameClip;
    public AudioClip damageClip;
    public Transform flamePos;
    public FloatVariable playerHealth;
    public Slider healthSlider;

    protected FSMState currState;
    protected float flyTimer;
    protected float attackTimer;
    protected bool firstFly, secondFly;
    protected LayerMask terrainLayer;
    protected WaitForSeconds waitForAttack;
    protected WaitForSeconds waitClawAttack1;
    protected WaitForSeconds waitClawAttack2;
    protected float desiredSpeed;
    #endregion

    protected override void Start()
    {
        base.Start();

        currState = FSMState.Idle;
        dustParticle.Stop();
        fireParticle.Stop();
        terrainLayer = 1 << LayerMask.NameToLayer("Terrain");

        waitForAttack = new WaitForSeconds(0.32f);
        waitClawAttack1 = new WaitForSeconds(0.5f);
        waitClawAttack2 = new WaitForSeconds(0.433f);
    }

    protected virtual void Update()
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

    protected virtual void UpdateIdle()
    {
        if (Vector3.Distance(changeCharacter.currCharacter.position, transform.position) < chaseDistance)
        {
            currState = FSMState.Chase;
            return;
        }
    }

    protected virtual void UpdateChase()
    {
        var random = Random.value;

        float distance, angle;
        PlayerSite(out distance, out angle);

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
            desiredSpeed = walkSpeed;
        }
        else
        {
            desiredSpeed = runSpeed;
        }
    }

    protected virtual void UpdateFlyChase()
    {
        float distance, angle;
        PlayerSite(out distance, out angle);

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
            desiredSpeed = glideSpeed;
        }
        else
        {
            desiredSpeed = flySpeed;
        }
    }

    protected virtual void UpdateAttack()
    {
        if (attackTimer < 0.7f)
        {
            attackTimer += Time.deltaTime;
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
            attackTimer = 0;
            currState = FSMState.Chase;
            flame.SetActive(false);
        }
    }

    protected virtual void UpdateFlyAttack()
    {
        flyTimer += Time.deltaTime;

        attackTimer += Time.deltaTime;
        if (attackTimer < 1f)   //  the state of animator need time to shift
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

            if (attackTimer > 1.7f)  //after this, the flame attack on the ground and spreaded
            {
                if (Vector3.Distance(hit.point, changeCharacter.currCharacter.position) < 7)
                {
                    playerHealth.Value -= attackDamage;
                }
                attackTimer = 1.0f; //控制攻击频率
            }
        }

        //finished fly attack
        if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash == Hashes.FlyState)
        {
            attackTimer = 0;
            flame.SetActive(false);
            currState = FSMState.FlyChase;
        }
    }

    protected virtual void UpdateTakeOff()
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

    protected virtual void UpdateLand()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash == Hashes.LocomotionState)
        {
            currState = FSMState.Chase;
        }
    }

    protected IEnumerator BasicAttack()
    {
        yield return waitForAttack;
        Attack();
    }

    protected IEnumerator ClawAttack()
    {
        yield return waitClawAttack1;
        Attack();
        yield return waitClawAttack2;
        Attack();
    }

    private void Attack()
    {
        if (Vector3.Distance(transform.position + transform.forward * 5, changeCharacter.currCharacter.position) < 2)
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

        if (healthSlider != null)
        {
            healthSlider.value = health;
        }

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
