using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceHit : MonoBehaviour
{
    AudioSource source;
    [SerializeField] AudioClip hitClip;
    [SerializeField] AudioClip brokenClip;
    public float IceHp = 3;
    float hitTime = 0.0f; // ���������� ���� �ð�
    float hitTimeThreshold = 0.2f; // ���� �� �ٽ� �����ϱ������ �ð� ����
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
            GameManager.Resource.Destroy(hitEffect1.gameObject, 2f); // ��ƼŬ ���� �ð� + 1�� �� ����
        }
    }
}
