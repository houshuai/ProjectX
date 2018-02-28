using UnityEngine;

public class ChangeCharacter : MonoBehaviour
{
    public GameObject player;
    public Transform playerRig;
    public GameObject horse;
    public Transform hroseRig;
    public GameObject whale;
    public Transform whaleRig;
    public float waterHeight = -1;

    [HideInInspector]
    public Transform currCharacter;

    private CameraLook cameraLook;

    private void Start()
    {
        cameraLook = FindObjectOfType<CameraLook>();
        currCharacter = player.transform;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.X))
#else
        if (TouchButton.GetButtonDown("Charactor"))
#endif
        {
            if (player.activeSelf)
            {
                player.SetActive(false);
                horse.SetActive(true);
                Transform(player.transform, horse.transform);
                cameraLook.rig = hroseRig;
            }
            else
            {
                player.SetActive(true);
                horse.SetActive(false);
                Transform(horse.transform, player.transform);
                cameraLook.rig = playerRig;
            }
        }

        if (currCharacter.position.y < waterHeight && whale.activeSelf == false)
        {
            whale.SetActive(true);
            if (player.activeSelf)
            {
                Transform(player.transform, whale.transform);
                player.SetActive(false);
            }
            else
            {
                Transform(horse.transform, whale.transform);
                horse.SetActive(false);
            }
            cameraLook.rig = whaleRig;
        }

        if (currCharacter.position.y > waterHeight && whale.activeSelf == true)
        {
            whale.SetActive(false);
            player.SetActive(true);
            Transform(whale.transform, player.transform);
            cameraLook.rig = playerRig;
        }
    }

    /// <summary>
    /// 将一个角色的transform复制给另一个角色
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    private void Transform(Transform from, Transform to)
    {
        to.position = from.position;
        to.rotation = from.rotation;
        currCharacter = to;
    }

}
