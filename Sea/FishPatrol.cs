using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class FishPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    private List<Transform> patrolPoints = new List<Transform>();

    [Header("Chase Settings")]
    Fish fish;
    public float chaseSpeedMultiplier = 1.5f;
    public float attackRange = 1.2f;
    public LayerMask whatIsTarget;
    public float chaseLoseDistance = 1.2f;

    private NavMeshAgent agent;
    private int currentPointIndex = -1;
    private Collider[] overlapResults = new Collider[10];

    private float baseSpeed;
    private Transform target;
    public bool isInSmoke = false;

    FishState state;

    public bool HasTarget => target != null;
    public Transform Target => target;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        state = GetComponent<FishState>();
        fish = GetComponent<Fish>();
        baseSpeed = agent.speed;
    }

    private void Start()
    {
        SetupPatrolPoints();
    }

    //private void Update()
    //{
    //    if (target != null)
    //    {
    //        PlayerDamage playerDamage = target.GetComponent<PlayerDamage>();
    //        if (playerDamage != null && playerDamage.isInSmoke)
    //        {
    //            isInSmoke = true;
    //        }
    //    }
    //}

    private void SetupPatrolPoints()
    { // 물고기 타입 별로 패트롤 포인트 지정
        string chosenParentName = null;

        if (GetComponent<FishA>() != null)
        {
            chosenParentName = "FishPatrolPointsA"; // A는 하나
        }
        else if (GetComponent<FishB>() != null)
        { // 패트롤 포인트 두 개 중 한 개 랜덤
            string[] patrolParents = { "FishPatrolPointsB", "FishPatrolPointsB_1" };
            chosenParentName = patrolParents[Random.Range(0, patrolParents.Length)];
        }
        else if (GetComponent<FishC>() != null)
        { // 패트롤 포인트 두 개 중 한 개 랜덤
            string[] patrolParents = { "FishPatrolPointsC", "FishPatrolPointsC_1" };
            chosenParentName = patrolParents[Random.Range(0, patrolParents.Length)];
        }
        else if (GetComponent<FishD>() != null)
        {
            chosenParentName = "FishPatrolPointsD"; // D는 하나
        }

        if (!string.IsNullOrEmpty(chosenParentName))
        {
            GameObject patrolParent = GameObject.Find(chosenParentName);
            if (patrolParent != null)
            {
                patrolPoints.AddRange(patrolParent.GetComponentsInChildren<Transform>());
                if (patrolPoints.Count > 0) patrolPoints.RemoveAt(0); // parent 제거
            }
        }
    }


    public void Patrol()
    {
        ResetSpeed();
        if (agent.pathPending || agent.remainingDistance > 1f) return;
        if (patrolPoints.Count == 0) return;

        // 패트롤 포인트 내에서 랜덤 이동
        currentPointIndex = Random.Range(0, patrolPoints.Count);
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }
    public void ForcePatrol()
    {
        target = null;
        isInSmoke = true;
        agent.isStopped = false;   // 혹시 멈춰있을 수도 있으니
        Patrol();                  // 즉시 새로운 패트롤 포인트로 이동
    }

    public bool DetectPlayer(out Transform t)
    { // 플레이어 탐지 bool 메소드
        t = null;
        if (isInSmoke || state.currentState == FishAIState.Stun) return false; // 연막탄 안에 있을 경우 플레이어 탐지 불가
        
        int count = Physics.OverlapSphereNonAlloc(transform.position, fish.curTraceDistance, overlapResults, whatIsTarget);
        for (int i = 0; i < count; i++)
        {
            PlayerCheck playerCheck = overlapResults[i].GetComponent<PlayerCheck>();
            if (playerCheck != null)
            { // 탐지 성공 시 true, 타겟 위치 out
                t = playerCheck.transform;
                return true;
            }
        }
        return false;
    }

    public void SetTarget(Transform t)
    { // 타겟 위치 지정 메소드
        target = t;
    }

    public void ChaseTarget()
    {
        if (target == null || isInSmoke || state.currentState == FishAIState.Stun) return;

        // 플레이어 Damage 체크
        PlayerDamage playerDamage = target.GetComponent<PlayerDamage>();
        if (playerDamage != null && playerDamage.isInSmoke)
        {
            // 플레이어가 연막에 들어가면 추격 중단
            target = null;
            return;
        }

        if (!target.gameObject.activeInHierarchy)
        {
            target = null;
            return;
        }

        // 추적 속도 계산
        float currentSpeed = baseSpeed * chaseSpeedMultiplier;

        FishB bType = GetComponent<FishB>();
        if (bType != null && bType.isPlayerGetPurple)
            currentSpeed *= 2f;

        OpenInvenUI invenUI = FindObjectOfType<OpenInvenUI>(true);
        if (invenUI._isReturnCancle)
            currentSpeed *= 2f;

        agent.isStopped = false;
        agent.speed = currentSpeed;
        agent.SetDestination(target.position);
    }

    public bool IsInAttackRange()
    {
        if (target == null || state.currentState == FishAIState.Stun) return false;

        float range = attackRange;
        if (GetComponent<FishD>() != null)
        {
            range *= 3f;
        }

        return Vector3.Distance(transform.position, target.position) <= range;
    }


    public bool IsTargetTooFar()
    {
        if (target == null) return true;
        return Vector3.Distance(transform.position, target.position) > chaseLoseDistance;
    }

    public void ResetSpeed()
    { // 기본 패트롤 속도로 리셋
        agent.speed = baseSpeed;
    }
}
