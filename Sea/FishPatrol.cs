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
    { // ����� Ÿ�� ���� ��Ʈ�� ����Ʈ ����
        string chosenParentName = null;

        if (GetComponent<FishA>() != null)
        {
            chosenParentName = "FishPatrolPointsA"; // A�� �ϳ�
        }
        else if (GetComponent<FishB>() != null)
        { // ��Ʈ�� ����Ʈ �� �� �� �� �� ����
            string[] patrolParents = { "FishPatrolPointsB", "FishPatrolPointsB_1" };
            chosenParentName = patrolParents[Random.Range(0, patrolParents.Length)];
        }
        else if (GetComponent<FishC>() != null)
        { // ��Ʈ�� ����Ʈ �� �� �� �� �� ����
            string[] patrolParents = { "FishPatrolPointsC", "FishPatrolPointsC_1" };
            chosenParentName = patrolParents[Random.Range(0, patrolParents.Length)];
        }
        else if (GetComponent<FishD>() != null)
        {
            chosenParentName = "FishPatrolPointsD"; // D�� �ϳ�
        }

        if (!string.IsNullOrEmpty(chosenParentName))
        {
            GameObject patrolParent = GameObject.Find(chosenParentName);
            if (patrolParent != null)
            {
                patrolPoints.AddRange(patrolParent.GetComponentsInChildren<Transform>());
                if (patrolPoints.Count > 0) patrolPoints.RemoveAt(0); // parent ����
            }
        }
    }


    public void Patrol()
    {
        ResetSpeed();
        if (agent.pathPending || agent.remainingDistance > 1f) return;
        if (patrolPoints.Count == 0) return;

        // ��Ʈ�� ����Ʈ ������ ���� �̵�
        currentPointIndex = Random.Range(0, patrolPoints.Count);
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }
    public void ForcePatrol()
    {
        target = null;
        isInSmoke = true;
        agent.isStopped = false;   // Ȥ�� �������� ���� ������
        Patrol();                  // ��� ���ο� ��Ʈ�� ����Ʈ�� �̵�
    }

    public bool DetectPlayer(out Transform t)
    { // �÷��̾� Ž�� bool �޼ҵ�
        t = null;
        if (isInSmoke || state.currentState == FishAIState.Stun) return false; // ����ź �ȿ� ���� ��� �÷��̾� Ž�� �Ұ�
        
        int count = Physics.OverlapSphereNonAlloc(transform.position, fish.curTraceDistance, overlapResults, whatIsTarget);
        for (int i = 0; i < count; i++)
        {
            PlayerCheck playerCheck = overlapResults[i].GetComponent<PlayerCheck>();
            if (playerCheck != null)
            { // Ž�� ���� �� true, Ÿ�� ��ġ out
                t = playerCheck.transform;
                return true;
            }
        }
        return false;
    }

    public void SetTarget(Transform t)
    { // Ÿ�� ��ġ ���� �޼ҵ�
        target = t;
    }

    public void ChaseTarget()
    {
        if (target == null || isInSmoke || state.currentState == FishAIState.Stun) return;

        // �÷��̾� Damage üũ
        PlayerDamage playerDamage = target.GetComponent<PlayerDamage>();
        if (playerDamage != null && playerDamage.isInSmoke)
        {
            // �÷��̾ ������ ���� �߰� �ߴ�
            target = null;
            return;
        }

        if (!target.gameObject.activeInHierarchy)
        {
            target = null;
            return;
        }

        // ���� �ӵ� ���
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
    { // �⺻ ��Ʈ�� �ӵ��� ����
        agent.speed = baseSpeed;
    }
}
