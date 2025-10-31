using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class IcePick : MonoBehaviour
{
    [SerializeField] float hitThreshold = 0.1f; // 충돌 속도 임계값
    float damage = 1f;
    readonly string iceTag = "Ice";

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(iceTag))
        {
            float impactSpeed = collision.relativeVelocity.magnitude;
            Debug.Log($"맞았을 때 속도 : {impactSpeed}");
            if (impactSpeed > hitThreshold)
            {
                ContactPoint contact = collision.contacts[0];

                var iceHit = collision.gameObject.GetComponent<IceHit>();
                if (iceHit != null && iceHit.isActiveAndEnabled)
                {
                    iceHit.OnHit(damage, contact.point);
                }
            }
        }
    }
}