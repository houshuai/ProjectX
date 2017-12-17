using UnityEngine;

public class GrassBox : MonoBehaviour
{
    public float height = 5;
    public float width = 5;
    [Range(0, 5000)] public int grassCount = 200;
    
    private PlayerMove player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<PlayerMove>();

        var boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.center = new Vector3(width / 2, 0.5f, height / 2);
        boxCollider.size = new Vector3(width, 1, height);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == Tags.Player)
        {
            player.isInGrass = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == Tags.Player)
        {
            player.isInGrass = false;
        }
    }
}
