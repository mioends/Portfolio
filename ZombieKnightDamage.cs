using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class ZombieKnightDamage : Creature
{
    private Slider hpBar;
    private Animator animator;
    private readonly int hashhit_1 = Animator.StringToHash("hit_1");
    private readonly int hashhit_2 = Animator.StringToHash("hit_2");
    private readonly int hashdeath = Animator.StringToHash("death");

    private readonly string bulletTag = "Bullet";
    private ZombieknightMove move;
    private CapsuleCollider2D bodyCollider;
    private BoxCollider2D swordCollider;
    private Rigidbody2D rb;

    private float knockbackForce = 0.7f; // 밀리는 힘
    public SpriteRenderer[] spriteRenderers;
    public Color[] originalColors;
    public GameObject bloodEff;

    public GameObject head; // 머리 오브젝트
    public float respawnDelay = 5f;
    private WaitForSeconds dieWs;
    WaitForSeconds clipWs;

    AudioSource source;
    public AudioClip damageClip;
    public AudioClip dieClip;
    private bool isPlayingDamageSound = false;
    void Awake()
    {
        hpBar = GetComponentInChildren<Slider>();
        animator = GetComponent<Animator>();
        move = GetComponent<ZombieknightMove>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        swordCollider = GetComponentInChildren<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        source = GetComponent<AudioSource>();
        dieWs = new WaitForSeconds(3f);
        clipWs = new WaitForSeconds(0.5f);

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        bloodEff.SetActive(false);
        hpBar.value = curHP;
        LiveOrDie(true);
        animator.enabled =true;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag(bulletTag) && !isDead)
        {
            Vector2 knockbackDir = (transform.position - col.transform.position).normalized;
            if (!isPlayingDamageSound && !isDead)
            {
                source.PlayOneShot(damageClip);
                StartCoroutine(DamageSoundCooldown()); // 재생 쿨타임
            }
            StartCoroutine(KnockbackRoutine(knockbackDir)); // Knockback 호출
            Damage(10f);
        }
    }

    public override void Damage(float damage)
    {
        if (isDead) return;
        base.Damage(damage);
        hpBar.value = curHP;
        bloodEff.SetActive(true);

        int randomHit = Random.Range(0, 2);
        int hitTrigger = (randomHit == 0) ? hashhit_1 : hashhit_2;
        animator.SetTrigger(hitTrigger);
        StartCoroutine(HitFlashEffect());
    }
    private IEnumerator DamageSoundCooldown()
    {
        isPlayingDamageSound = true;
        yield return clipWs;
        // yield return new WaitForSeconds(source.clip.length); // 클립 길이만큼 대기
        isPlayingDamageSound = false;
    }
    private IEnumerator HitFlashEffect()
    {
        // 모든 SpriteRenderer를 빨간색으로
        foreach (var sr in spriteRenderers)
        {
            sr.color = Color.red;
        }

        float duration = 1f;
        float time = 0f;

        // 천천히 원래 색으로 복원
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].color = Color.Lerp(Color.red, originalColors[i], t);
            }

            yield return null;
        }

        // 마지막 보정
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].color = originalColors[i];
        }
    }
    public override void Die()
    {
        isDead = true;
        StartCoroutine(Death());
    }
    IEnumerator Death()
    {
        animator.SetTrigger(hashdeath);
        move.AnimationChange(false, false, false);
        source.PlayOneShot(dieClip);
        LiveOrDie(false);
        yield return dieWs;

        GameObject pooledHead = PoolingManager.p_instance.GetHead();
        if (pooledHead != null)
        {
            pooledHead.transform.position = transform.position;
            pooledHead.transform.rotation = Quaternion.identity;
            pooledHead.SetActive(true);
        }

        // 코인 생성
        int coinCount = Random.Range(1, 4); // 1~3개 랜덤
        List<GameObject> coins = PoolingManager.p_instance.GetCoins(coinCount);
        foreach (var coin in coins)
        {
            coin.transform.position = transform.position + (Vector3)Random.insideUnitCircle * 0.5f; // 약간 퍼트림
            coin.SetActive(true);
        }

        // 좀비 본체 비활성화
        gameObject.SetActive(false);
    }


    public void OnDeathAnimationEnd()
    {
        animator.enabled = false;
    }

    private void LiveOrDie(bool set)
    {
        hpBar.gameObject.SetActive(set);
        move.enabled = set;
        bodyCollider.enabled = set;
        swordCollider.enabled = set;
    }

    // 피격 시 멈추고 밀리는 코루틴
    private IEnumerator KnockbackRoutine(Vector2 direction)
    {
        move.enabled = false;

        //수직 방향 제거, 수평만 유지
        direction.y = 0f;
        direction.Normalize(); // 방향 다시 정규화

        rb.velocity = direction * knockbackForce;

        yield return new WaitForSeconds(0.2f); // 밀리는 시간
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.8f); // 총 1초 대기
        if (!isDead) move.enabled = true;
        bloodEff.SetActive(false);
    }
}
