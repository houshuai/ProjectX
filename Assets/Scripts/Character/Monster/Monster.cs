using UnityEngine;

public class Monster : MonoBehaviour
{
    public float totalHealth = 100f;
    public AudioClip deadClip;

    protected Animator anim;
    protected AudioSource audioSource;
    protected float health;
    protected bool isDead;
    protected ChangeCharacter changeCharacter;

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        health = totalHealth;
        isDead = false;
        changeCharacter = FindObjectOfType<ChangeCharacter>();
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;

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

