using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public FloatVariable health;
    public AudioSource audioSource;
    public AudioClip hitClip;

    [HideInInspector]
    public Animator anim;

    private Slider healthSlider;
    private PlayerMove playerMove;

    private void Start()
    {
        healthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        playerMove = GetComponent<PlayerMove>();

        healthSlider.value = health.Value;
    }

    private void OnEnable()
    {
        health.ValueChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        health.ValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(object sender, ValueChangedEventArgs args)
    {
        healthSlider.value = health.Value;

        if (anim == null)
        {
            anim = playerMove.anim;
        }

        if (health.Value <= 0)
        {
            anim.SetTrigger(Hashes.DeadTrigger);
            audioSource.Stop();
        }
        else
        {
            anim.SetTrigger(Hashes.DamageTrigger);

            audioSource.clip = hitClip;
            audioSource.loop = false;
            audioSource.Play();
        }
    }
}
