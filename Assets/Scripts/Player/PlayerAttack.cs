using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAttack : MonoBehaviour
{
    public float punch1Damage = 10f;
    public float punch2Damage = 20f;
    public float punch3Damage = 30f;
    public LayerMask enemyLayer;
    public AudioSource audioSource;
    public AudioClip attackClip;
    public FloatVariable playerHealth;

    [HideInInspector]
    public Animator anim;
    [HideInInspector]
    public int EnemyCount
    {
        set
        {
            enemyCount = value;
            anim.SetBool(Hashes.FightBool, true);
        }

        get { return enemyCount; }
    }

    private int enemyCount;
    private PlayerMove playerMove;
    private Vector3 attackPos;
    private int attackCombo = 0;
    private float comboTime = 1;
    private float comboTimer = 0;
    private float fightTime = 3;
    private float fightTimer;
    private int fireInputCount;
    private float[] punchTime;
    private float punchTimer;

    private WaitForSeconds waitPunch1;
    private WaitForSeconds waitPunch2;
    private WaitForSeconds waitPunch3;

    private void Start()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
        playerMove = GetComponent<PlayerMove>();
        attackPos = new Vector3(0, 1.5f, 0);
        punchTime = new float[3] { 0.333f, 0.667f, 0.867f };

        waitPunch1 = new WaitForSeconds(0.1667f);
        waitPunch2 = new WaitForSeconds(0.3335f);
        waitPunch3 = new WaitForSeconds(0.4f);
    }

    private void Update()
    {
        if (playerHealth.Value <= 0 || playerMove.isCrouch || EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetButtonDown("Fire1") && fireInputCount < 3) //记录最多三次攻击输入
        {
            if (enemyCount > 0 || fightTimer > 0)
            {
                fireInputCount++;
            }
            else
            {
                anim.SetBool(Hashes.FightBool, true);
            }
            fightTimer = fightTime;
        }

        if (punchTimer > 0)
        {
            //正在攻击
            punchTimer -= Time.deltaTime;
        }
        else
        {
            Punch();
        }



        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
        }
        else
        {
            //连击终止
            if (attackCombo != 0)
            {
                attackCombo = 0;
                anim.SetInteger(Hashes.PunchIndexInt, attackCombo);
                fightTimer = fightTime;
            }

        }

        if (enemyCount == 0)
        {
            if (fightTimer > 0)
            {
                fightTimer -= Time.deltaTime;
            }
            else
            {
                anim.SetBool(Hashes.FightBool, false);
            }
        }

    }

    private void Punch()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash != Hashes.DamageState && fireInputCount > 0)
        {
            fireInputCount--;
            attackCombo++;
            if (attackCombo == 4)
            {
                attackCombo = 1;
            }
            punchTimer = punchTime[attackCombo - 1];                  //重置攻击timer
            comboTimer = comboTime;                                   //重置连击timer
            anim.SetInteger(Hashes.PunchIndexInt, attackCombo);

            if (attackCombo == 1)
            {
                StartCoroutine(Attack(punch1Damage, waitPunch1));
            }
            else if (attackCombo == 2)
            {
                StartCoroutine(Attack(punch2Damage, waitPunch2));
            }
            else
            {
                StartCoroutine(Attack(punch3Damage, waitPunch3));
            }
        }
    }

    private IEnumerator Attack(float damage, WaitForSeconds wait)
    {
        yield return wait;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + attackPos, transform.forward, out hit, 1f, enemyLayer))
        {
            var monster = hit.collider.gameObject.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(damage);
                audioSource.clip = attackClip;
                audioSource.Play();
            }

        }
    }
}
