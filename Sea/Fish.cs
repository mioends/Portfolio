using UnityEngine.AI;
using UnityEngine;

public class Fish : MonoBehaviour
{
    public float curTraceDistance = 30f;
    public float damage;
    public NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Initialize(FishData data)
    { // ����� ������ �ʱ�ȭ
        curTraceDistance = data.traceDistance;
        damage = data.damage;
        agent.speed = data.speed;
    }
}
