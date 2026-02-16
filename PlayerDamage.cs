using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using Unity.VisualScripting;

public class PlayerDamage : Creature
{
    public Slider hpBar;
    private Animator animator;
    private readonly int hashAttacked = Animator.StringToHash("Attacked");
    private readonly int hashDie = Animator.StringToHash("Die");
    private readonly string bulletTag = "Bullet";
    private WaitForSeconds attackedWs;
    private WaitForSeconds gameoverWs;
    public bool isAttacked;
    public bool isGameClear = false;
    public GameObject bloodEff;
    AudioSource source;
    public AudioClip damageCilp;
    public AudioClip dieCilp;
    BGMCtrl BGMCtrl;

    [SerializeField] CinemachineVirtualCamera vCam;
    [SerializeField] CinemachineBasicMultiChannelPerlin noise;
    public override void OnEnable()
    {
        base.OnEnable();
        isAttacked = false;
        LiveOrDie(true);
        attackedWs = new WaitForSeconds(0.5f);
        gameoverWs = new WaitForSeconds(2f);
        animator = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
        animator.SetBool(hashDie, false);
        hpBar.value = curHP;
        bloodEff.SetActive(false);

        vCam = FindAnyObjectByType<CinemachineVirtualCamera>();
        BGMCtrl = FindAnyObjectByType<BGMCtrl>();
        noise = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        StopCameraShake();
    }

    public override void Damage(float damage)
    {
        if(GameManager.instance.isGameOver) return;
        isAttacked = true;
        animator.SetTrigger(hashAttacked);
        base.Damage(damage);
        hpBar.value = curHP;
        source.PlayOneShot(damageCilp);
        StartCoroutine(Attacked());
    }
    public override void RestoreHP(float HPUp)
    {
        base.RestoreHP(HPUp);
        hpBar.value = curHP;
    }
    public override void Die()
    {
        base.Die();
        animator.SetTrigger(hashDie);
        LiveOrDie(false);
        source.PlayOneShot(dieCilp);
        BGMCtrl.source.Pause();
        GameManager.instance.isGameOver = true;
        StartCoroutine(gameoverCan());
    }
    IEnumerator gameoverCan()
    {
        yield return gameoverWs;
        if (!isGameClear)
            {
                GameManager.instance.PlayGameoverSound();
                UIManager.u_Instance.gameoverCanvas.gameObject.SetActive(true);
            }
    }
    public void OnDeathAnimationEnd()
    {
        animator.enabled = false; // 정확히 끝난 시점에서 멈춤
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(bulletTag) && UIManager.u_Instance.iWantKillMyself)
        {
            Damage(100f);
            col.gameObject.SetActive(false);
            isGameClear = true;
        }
    }
    private void LiveOrDie(bool set)
    {
        gameObject.GetComponent<PlayerInputSystem>().enabled = set;
        gameObject.GetComponent<PlayerControl>().enabled = set;
        gameObject.GetComponent<PlayerFire>().enabled = set;
    }
    IEnumerator Attacked()
    {
        LiveOrDie(false);
        bloodEff.SetActive(true);
        ShakeCamera();

        yield return attackedWs;
        isAttacked = false;
        if (!isDead)
            LiveOrDie(true);
        bloodEff.SetActive(false);
    }

    public void ShakeCamera()
    {
        noise.m_AmplitudeGain = 1.5f;
        noise.m_FrequencyGain = 1.2f;
        Invoke("StopCameraShake", 0.3f);
    }
    public void StopCameraShake()
    {
        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 0f;
    }
}
