using UnityEngine;

public class PlayAnimationEvent : MonoBehaviour
{
    private PlayerMove playerMove;

    private void Start()
    {
        playerMove = GetComponentInParent<PlayerMove>();
    }

    private void PlayAudio()
    {
        playerMove.PlayRunAudio();
    }
}
