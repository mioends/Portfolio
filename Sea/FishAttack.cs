using UnityEngine;
using UnityEngine.AI;

public class FishAttack : MonoBehaviour
{
    private Transform target;
    Fish fish;
    FishState state;
    AudioSource source;

    public float rotationSpeed = 5f; // ȸ�� �ӵ�

    private void Awake()
    {
        fish = GetComponent<Fish>();
        state = GetComponent<FishState>();
        source = GetComponent<AudioSource>();
    }

    public bool HasTarget => target != null;

    public void SetTarget(Transform t)
    {
        target = t;
    }

    private void Update()
    {
        if (TryGetComponent<FishD>(out var fish)) return;

        if (target != null && (state.currentState == FishAIState.Attack || state.currentState == FishAIState.Chase))
        { // ���� Ȥ�� ���� ������ �� �÷��̾� �ٶ�
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    public void DoAttack()
    {
        if (target == null) return;
        target.GetComponent<PlayerDamage>().Damage(fish.damage);

        if (!target.gameObject.activeInHierarchy)
        {
            target = null;
        }
    }
    public void AttackAudioPlay()
    {
        source.Play();
    }
}
