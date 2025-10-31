using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FishNeutralize : MonoBehaviour
{
    [Header("Run Settings")]
    public float runDistance = 20f; // �������� �Ÿ�
    public float runWaitTime = 5f; // ������ �� ��� �ð�
    public GameObject fishRunPointsParent;
    private List<Transform> fishRunPoint;

    private NavMeshAgent agent;
    private FishState fishState;
    private FishPatrol patrol;
    Transform tr;

    Transform VertexObj;
    MeshRenderer vertex;

    private Coroutine currentRoutine;
    bool isStun = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        fishState = GetComponent<FishState>();
        patrol = GetComponent<FishPatrol>();
        tr = GetComponent<Transform>();
        fishRunPointsParent = GameObject.Find("FishRunPoints");
        fishRunPoint = new List<Transform>();
        VertexObj = transform.Find("Vertex");
    }

    private void Start()
    {
        fishRunPoint.AddRange(fishRunPointsParent.GetComponentsInChildren<Transform>());
        if (fishRunPoint.Count > 0) fishRunPoint.RemoveAt(0); // parent ����

        if (VertexObj != null) vertex = VertexObj.GetComponent<MeshRenderer>();
        vertex.enabled = false;
    }

    public void Stun(float stunDuration)
    { // �ܺ� ȣ��
        if(isStun) return;
        Debug.Log("stun");
        if (currentRoutine != null) StopCoroutine(currentRoutine); // Run �� �ۻ쿡 ������ Stun���� ����
        fishState.ChangeState(FishAIState.Stun);
        isStun = true;
        currentRoutine = StartCoroutine(StunRoutine(stunDuration));
    }

    private IEnumerator StunRoutine(float stunDuration)
    { // Stun ��ƾ
        agent.isStopped = true;
        patrol.SetTarget(null);

        float elapsed = 0f; // �ð�
        Vector3 startPos = tr.position; // ���� ��ġ ����
        float floatAmplitude = 0.08f; // ���Ʒ� �����̴� ����
        float floatFrequency = 2f; // �ӵ�

        while (elapsed < stunDuration)
        { // ���� �ð���ŭ
            elapsed += Time.deltaTime;

            vertex.enabled = true;
            // ���� �Լ��� ���Ʒ� �̵�
            Vector3 floatOffset = new Vector3(0f, Mathf.Sin(elapsed * floatFrequency) * floatAmplitude, 0f);
            tr.position = startPos + floatOffset; // ���Ʒ� �յ� ���ٴ�

            yield return null;
        }

        tr.position = startPos; // ���� ������ ���� ��ġ ����
        agent.isStopped = false;
        vertex.enabled = false;
        isStun = false;
        fishState.ChangeState(FishAIState.Patrol); // �ٽ� ��Ʈ�� ����
    }


    public void Run()
    { // �ܺ� ȣ��
        fishState.ChangeState(FishAIState.Run);
        currentRoutine = StartCoroutine(RunRoutine());
    }

    private IEnumerator RunRoutine()
    {
        if (!patrol.HasTarget)
        { // �÷��̾� ��� �� ��Ʈ�ѷ� ����
            fishState.ChangeState(FishAIState.Patrol);
            yield break;
        }

        if (fishRunPoint == null || fishRunPoint.Count == 0)
        { // �� ����Ʈ�� ������ ����
            Stun(5f);
            yield break;
        }

        // fishRunPoint ����Ʈ���� ������ ������ ����
        Transform runTarget = null;
        while (runTarget == null)
        {
            int idx = Random.Range(0, fishRunPoint.Count);
            runTarget = fishRunPoint[idx];
        }

        bool canRun = NavMesh.SamplePosition(runTarget.position, out NavMeshHit hit, 5f, NavMesh.AllAreas);
        if (!canRun)
        { // �̵� �Ұ��� ����
            Stun(5f);
            yield break;
        }

        agent.isStopped = false;
        agent.SetDestination(hit.position);

        float elapsed = 0f;
        while (elapsed < runWaitTime)
        { // ���� �ð����� ���
            elapsed += Time.deltaTime;

            // ��ΰ� ����ų� ���� �߻� �� �������� ��ȯ
            if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Stun(5f);
                yield break;
            }

            yield return null;
        }

        agent.isStopped = true;

        // ���� �ð� ������ ��Ʈ�ѷ� ����
        agent.isStopped = false;
        fishState.ChangeState(FishAIState.Patrol);
    }
}
