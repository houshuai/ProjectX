using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public bool isLockEnemy;
    [HideInInspector]
    public Transform currEnemy;
    [HideInInspector]
    public Animator anim;

    private List<Monster> enemyList;
    private PlayerMove playerMove;
    private Vector3 attackPos;
    private int attackCombo = 0;  //处于哪一段招数
    private float fightTime = 2f;  //大于这个时间，退出攻击状态
    private float fightTimer;
    private int fireInputCount;
    private float[] punchTime;
    private float punchTimer;
    private float detectTimer;

    private WaitForSeconds waitPunch1;
    private WaitForSeconds waitPunch2;
    private WaitForSeconds waitPunch3;

    private void Start()
    {
        enemyList = new List<Monster>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        playerMove = GetComponent<PlayerMove>();
        attackPos = new Vector3(0, 1.5f, 0);
        punchTime = new float[3] { 0.333f, 0.667f, 0.867f };  //三个攻击动画的时间

        waitPunch1 = new WaitForSeconds(0.1667f);        //判断是否击中的等待时间
        waitPunch2 = new WaitForSeconds(0.3335f);
        waitPunch3 = new WaitForSeconds(0.4f);
    }

    private void Update()
    {
        if (playerHealth.Value <= 0 || playerMove.isCrouch)
        {
            return;
        }

#if UNITY_EDITOR
        if (Input.GetButtonDown("Fire1") && fireInputCount < 3)//记录最多三次攻击输入
#else
        if (TouchButton.GetButtonDown("Fire") && fireInputCount < 3) //记录最多三次攻击输入
#endif
        {
            if (fightTimer > 0)// 处于攻击状态
            {
                fireInputCount++;
            }
            else             //不在攻击状态
            {
                anim.SetBool(Hashes.FightBool, true); //切换到准备攻击状态
                if (enemyList.Count > 0)
                {
                    isLockEnemy = true;
                    currEnemy = enemyList[0].transform;
                }
            }
            fightTimer = fightTime;
        }

        if (punchTimer > 0) //正在攻击
        {
            punchTimer -= Time.deltaTime;
        }
        else if (fireInputCount > 0) //记录有点击了攻击键
        {
            Punch();
        }
        else if (attackCombo > 0) //没有记录点击了攻击键，切换到准备攻击状态
        {
            attackCombo = 0;
            anim.SetInteger(Hashes.PunchIndexInt, attackCombo);
            fightTimer = fightTime;
        }

        if (fightTimer > 0)
        {
            fightTimer -= Time.deltaTime;
        }
        else              //超时，取消攻击状态
        {
            anim.SetBool(Hashes.FightBool, false);
            isLockEnemy = false;
            currEnemy = null;
        }

        if (detectTimer <= 0)
        {
            DetectEnemy();
            detectTimer = 3f;
        }
        else
        {
            detectTimer -= Time.deltaTime;
        }
    }

    private void Punch()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).shortNameHash != Hashes.DamageState)
        {
            fireInputCount--;
            attackCombo++;
            if (attackCombo == 4)
            {
                attackCombo = 1;
            }
            punchTimer = punchTime[attackCombo - 1];                  //重置攻击timer
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

    private void DetectEnemy()
    {
        enemyList.Clear();

        var enemies = Physics.OverlapSphere(transform.position, 5, enemyLayer);
        if (enemies.Length > 0)
        {
            foreach (var enemy in enemies)
            {
                enemyList.Add(enemy.gameObject.GetComponent<Monster>());
            }
        }

        if (enemyList.Count == 0)
        {
            isLockEnemy = false;
            currEnemy = null;
        }
    }
}
