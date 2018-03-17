using UnityEngine;

public class ChangeCharacter : MonoBehaviour
{
    public GameObject player;
    public Transform playerRig;
    public GameObject horse;
    public Transform horseRig;
    public GameObject whale;
    public Transform whaleRig;
    public float waterHeight = -1;

    [HideInInspector]
    public Transform currCharacter;

    private CameraLook cameraLook;
    private GameObject jumpButton;
    private GameObject fightButton;

    private void Awake()
    {
        currCharacter = player.transform;
    }

    private void Start()
    {
        cameraLook = FindObjectOfType<CameraLook>();
        jumpButton = GameObject.Find("JumpTouchButton");
        fightButton = GameObject.Find("FireTouchButton");
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.X))
#else
        if (TouchButton.GetButtonDown("Charactor"))
#endif
        {
            //在水中不能切换
            if (whale.activeSelf)
            {
                return;
            }
            if (player.activeSelf)
            {
                Change(player, horse, horseRig, false);
            }
            else
            {
                Change(horse, player, playerRig, true);
            }
        }

        if (currCharacter.position.y < waterHeight - 1.5f && whale.activeSelf == false)
        {
            if (player.activeSelf)
            {
                Change(player, whale, whaleRig, false);
            }
            else
            {
                Change(horse, whale, whaleRig, false);
            }
        }

        if (currCharacter.position.y > waterHeight && whale.activeSelf == true)
        {
            Change(whale, player, playerRig, true);
        }
    }

    private void Change(GameObject from,GameObject to,Transform camRig,bool isButtonActive)
    {
        from.SetActive(false);
        to.SetActive(true);
        to.transform.position = from.transform.position;
        to.transform.rotation = from.transform.rotation;
        currCharacter = to.transform;
        cameraLook.rig = camRig;
        jumpButton.SetActive(isButtonActive);
        fightButton.SetActive(isButtonActive);
    }
    

}
