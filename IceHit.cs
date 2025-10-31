using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceHit : MonoBehaviour
{
    AudioSource source;
    [SerializeField] AudioClip hitClip;
    [SerializeField] AudioClip brokenClip;
    public float IceHp = 3;
    float hitTime = 0.0f; // 마지막으로 맞은 시간
    float hitTimeThreshold = 0.2f; // 맞은 후 다시 반응하기까지의 시간 간격
    private void OnEnable()
    {
        source = GetComponentInParent<AudioSource>();
    }
    public void OnHit(float damage, Vector3 attackPoint)
    {
        if (!isActiveAndEnabled) return;
        if (Time.time - hitTime > hitTimeThreshold)
        {
            IceHp -= damage;
            hitTime = Time.time;
            if (IceHp <= 0)
            {
                gameObject.SetActive(false);
            }
            ParticleSystem hitEffect1 = GameManager.Resource.Instantiate<ParticleSystem>($"Ice/IceHit", attackPoint, Quaternion.identity, null, true);
            GameManager.Resource.Destroy(hitEffect1.gameObject, 2f); // 파티클 지속 시간 + 1초 후 제거
        }
    }
}
