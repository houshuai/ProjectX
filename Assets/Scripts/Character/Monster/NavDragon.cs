using UnityEngine;

public class NavDragon : Dragon
{
    private float currSpeed;

    protected override void Start()
    {
        base.Start();

    }

    protected override void UpdateChase()
    {
        var random = Random.value;

        var pos = new Vector2(transform.position.x, transform.position.z);
        var playerPos = new Vector2(player.position.x, player.position.z);
        var distance = Vector2.Distance(pos, playerPos);
        var vector = playerPos - pos;
        var angle = Vector2.Angle(new Vector2(transform.forward.x, transform.forward.z), vector);

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

        if (distance > attackRange && distance < flameRange && random < 0.01f)
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

        MoveAndRotate(vector, angle, desiredSpeed);
    }

    protected override void UpdateFlyChase()
    {
        var pos = new Vector2(transform.position.x, transform.position.z);
        var playerPos = new Vector2(player.position.x, player.position.z);
        var distance = Vector2.Distance(pos, playerPos);
        var vector = playerPos - pos;
        var angle = Vector2.Angle(new Vector2(transform.forward.x, transform.forward.z), vector);

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

        float desiredSpeed = 0;
        if (distance < nearby)
        {
            desiredSpeed = glideSpeed;
        }
        else
        {
            desiredSpeed = flySpeed;
        }

        MoveAndRotate(vector, angle, desiredSpeed);
        flyTimer += Time.deltaTime;
    }

    private void MoveAndRotate(Vector2 vector, float angle, float desiredSpeed)
    {
        var rotation = Quaternion.LookRotation(new Vector3(vector.x, 0, vector.y));

        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);

        desiredSpeed *= Mathf.Cos(angle * Mathf.Deg2Rad);
        desiredSpeed = Mathf.Max(0, desiredSpeed);

        currSpeed = Mathf.Lerp(currSpeed, desiredSpeed, 0.1f);

        transform.position = transform.position + transform.forward * currSpeed * Time.deltaTime;

        anim.SetFloat(Hashes.SpeedFloat, currSpeed);
    }
}
