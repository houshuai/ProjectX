using UnityEngine;

public class HorseMove : CharacterMove
{
    private LayerMask terrainLayer;

    private void Start()
    {
        Initial();
        terrainLayer = 1 << LayerMask.NameToLayer("Terrain");
    }

    private void Update()
    {
        ChangeCurrSpeed();

        if (CheckIsGround())
        {
            Move();
        }
        else
        {
            rb.AddForce(Physics.gravity);
        }
    }

    private bool CheckIsGround()
    {
        if (Physics.Raycast(transform.position + new Vector3(0, 0.1f, 0), Vector3.down, 0.2f, terrainLayer))
        {
            return true;
        }
        else if (Physics.Raycast(transform.position + new Vector3(0.2f, 0.1f, -0.65f), Vector3.down, 0.4f, terrainLayer) ||
            Physics.Raycast(transform.position + new Vector3(0.2f, 0.1f, 0.85f), Vector3.down, 0.4f, terrainLayer) ||
            Physics.Raycast(transform.position + new Vector3(-0.2f, 0.1f, 0.85f), Vector3.down, 0.4f, terrainLayer) ||
            Physics.Raycast(transform.position + new Vector3(-0.2f, 0.1f, -0.65f), Vector3.down, 0.4f, terrainLayer))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
