using UnityEngine;

public class ChaseDragon : Dragon
{
    private ChaseAgent chaseAgent;

    protected override void Start()
    {
        base.Start();

        chaseAgent = GetComponent<ChaseAgent>();
    }

    protected override void UpdateChase()
    {
        if (!player.gameObject.activeSelf)
        {
            currState = FSMState.Idle;
            anim.SetFloat(Hashes.SpeedFloat, 0);
            return;
        }

        base.UpdateChase();

        if (currState == FSMState.Chase)
        {
            chaseAgent.speed = desiredSpeed;
            chaseAgent.Chase(player.position);

            anim.SetFloat(Hashes.SpeedFloat, chaseAgent.actualSpeed);
        }
    }

    protected override void UpdateFlyChase()
    {
        if (!player.gameObject.activeSelf)
        {
            flyTimer = 0;
            currState = FSMState.Land;
            anim.SetBool(Hashes.FlyBool, false);
            return;
        }

        base.UpdateFlyChase();

        if (currState == FSMState.FlyChase)
        {
            chaseAgent.speed = desiredSpeed;
            chaseAgent.Chase(changeCharacter.currCharacter.position);

            flyTimer += Time.deltaTime;
        }
    }
}
