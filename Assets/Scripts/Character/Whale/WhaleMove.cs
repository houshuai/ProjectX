using UnityEngine;

public class WhaleMove : CharacterMove
{
    public float upSpeed = 3;
    public Transform rayTrans;
    public LayerMask terrainLayer;

    private void Start()
    {
        Initial();

        terrainLayer = 1 << LayerMask.NameToLayer("Terrain");
    }

    private void Update()
    {
        ChangeCurrSpeed();
        Move();

        //到了陆地，上浮
        if (Physics.Raycast(rayTrans.position, Vector3.down, terrainLayer))
        {
            rb.velocity = new Vector3(rb.velocity.x, upSpeed * Time.deltaTime, rb.velocity.z);
        }
    }
    
}
