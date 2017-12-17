using UnityEngine;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    public float health = 100f;
    public AudioClip deadClip;
    public Slider healthSlider;

    protected Animator anim;
    protected AudioSource audioSource;
    protected bool isDead;

    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        healthSlider.value = health;

        if (health <= 0)
        {
            anim.SetTrigger(Hashes.DeadTrigger);
            if (audioSource.clip != deadClip)
            {
                audioSource.clip = deadClip;
            }
            audioSource.Play();
            isDead = true;
            Destroy(this.gameObject, 5f);
            var lootController = LootController.Instance;
            if (lootController!=null)
            {
                lootController.GetLoot(2, transform.position);
            }
        }
        
    }
}

