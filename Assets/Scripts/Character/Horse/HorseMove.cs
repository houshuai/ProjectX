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

        RaycastHit hit;
        if (CheckIsGround(out hit))
        {
            Move();

            //transform.rotation *= Quaternion.FromToRotation(Vector3.up, hit.normal);
            //var angle = transform.rotation.eulerAngles;
            //angle.z = 0;                             //不绕z轴旋转
            //transform.rotation = Quaternion.Euler(angle);
        }
        else
        {
            rb.AddForce(Physics.gravity);
        }
    }

    private bool CheckIsGround(out RaycastHit hit)
    {
        if (Physics.Raycast(transform.position + new Vector3(0, 0.1f, 0), Vector3.down, out hit, 0.3f, terrainLayer))
        {
            return true;
        }
        else if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, -1.15f), Vector3.down, out hit, 0.8f, terrainLayer) ||
            Physics.Raycast(transform.position + new Vector3(0, 0.5f, 1.25f), Vector3.down, out hit, 0.8f, terrainLayer))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
