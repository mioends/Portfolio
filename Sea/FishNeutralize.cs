using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FishNeutralize : MonoBehaviour
{
    [Header("Run Settings")]
    public float runDistance = 20f; // 도망가는 거리
    public float runWaitTime = 5f; // 도망간 뒤 대기 시간
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
        if (fishRunPoint.Count > 0) fishRunPoint.RemoveAt(0); // parent 제거

        if (VertexObj != null) vertex = VertexObj.GetComponent<MeshRenderer>();
        vertex.enabled = false;
    }

    public void Stun(float stunDuration)
    { // 외부 호출
        if(isStun) return;
        Debug.Log("stun");
        if (currentRoutine != null) StopCoroutine(currentRoutine); // Run 중 작살에 맞으면 Stun으로 변경
        fishState.ChangeState(FishAIState.Stun);
        isStun = true;
        currentRoutine = StartCoroutine(StunRoutine(stunDuration));
    }

    private IEnumerator StunRoutine(float stunDuration)
    { // Stun 루틴
        agent.isStopped = true;
        patrol.SetTarget(null);

        float elapsed = 0f; // 시간
        Vector3 startPos = tr.position; // 현재 위치 저장
        float floatAmplitude = 0.08f; // 위아래 움직이는 높이
        float floatFrequency = 2f; // 속도

        while (elapsed < stunDuration)
        { // 기절 시간만큼
            elapsed += Time.deltaTime;

            vertex.enabled = true;
            // 사인 함수로 위아래 이동
            Vector3 floatOffset = new Vector3(0f, Mathf.Sin(elapsed * floatFrequency) * floatAmplitude, 0f);
            tr.position = startPos + floatOffset; // 위아래 둥둥 떠다님

            yield return null;
        }

        tr.position = startPos; // 스턴 끝나면 원래 위치 복원
        agent.isStopped = false;
        vertex.enabled = false;
        isStun = false;
        fishState.ChangeState(FishAIState.Patrol); // 다시 패트롤 시작
    }


    public void Run()
    { // 외부 호출
        fishState.ChangeState(FishAIState.Run);
        currentRoutine = StartCoroutine(RunRoutine());
    }

    private IEnumerator RunRoutine()
    {
        if (!patrol.HasTarget)
        { // 플레이어 사망 시 패트롤로 변경
            fishState.ChangeState(FishAIState.Patrol);
            yield break;
        }

        if (fishRunPoint == null || fishRunPoint.Count == 0)
        { // 런 포인트가 없으면 스턴
            Stun(5f);
            yield break;
        }

        // fishRunPoint 리스트에서 랜덤한 목적지 선택
        Transform runTarget = null;
        while (runTarget == null)
        {
            int idx = Random.Range(0, fishRunPoint.Count);
            runTarget = fishRunPoint[idx];
        }

        bool canRun = NavMesh.SamplePosition(runTarget.position, out NavMeshHit hit, 5f, NavMesh.AllAreas);
        if (!canRun)
        { // 이동 불가시 스턴
            Stun(5f);
            yield break;
        }

        agent.isStopped = false;
        agent.SetDestination(hit.position);

        float elapsed = 0f;
        while (elapsed < runWaitTime)
        { // 도주 시간동안 대기
            elapsed += Time.deltaTime;

            // 경로가 끊기거나 오류 발생 시 스턴으로 전환
            if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Stun(5f);
                yield break;
            }

            yield return null;
        }

        agent.isStopped = true;

        // 도주 시간 끝나면 패트롤로 복귀
        agent.isStopped = false;
        fishState.ChangeState(FishAIState.Patrol);
    }
}
