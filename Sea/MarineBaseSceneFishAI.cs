using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarineBaseSceneFishAI : MonoBehaviour
{
    public enum PatrolType
    {
        Sequential, // 순서대로
        Random      // 랜덤
    }

    [Header("Patrol Settings")]
    public GameObject patrolPointsParent;
    private List<Transform> patrolPoints = new List<Transform>();

    public PatrolType patrolType = PatrolType.Sequential; // 인스펙터에서 선택 가능
    private int currentPointIndex = 0;

    [Header("Movement Settings")]
    public float speed = 3f;
    public float arriveDistance = 0.2f;
    public float rotationSpeed = 5f; // 회전 속도

    private void Awake()
    {
        if (patrolType == PatrolType.Random && patrolPointsParent == null)
        {
            GameObject foundParent = GameObject.Find("AquariumPatrolPoints");
            if (foundParent != null)
            {
                patrolPointsParent = foundParent;
            }
        }
    }

    void OnEnable()
    {
        patrolPoints.Clear();
        patrolPoints.AddRange(patrolPointsParent.GetComponentsInChildren<Transform>());
        if (patrolPoints.Count > 0) patrolPoints.RemoveAt(0); // parent 제거
        currentPointIndex = 0;
    }

    void Update()
    {
        if (patrolPoints.Count == 0) return;

        // 목표 포인트
        Transform targetPoint = patrolPoints[currentPointIndex];
        Vector3 targetPos;

        if (patrolType == PatrolType.Sequential)
        {
            // y는 고정 (수평 이동만)
            targetPos = new Vector3(targetPoint.position.x, transform.position.y, targetPoint.position.z);
        }
        else
        {
            // 랜덤 모드 → y값도 따라감 (3D 자유 이동)
            targetPos = targetPoint.position;
        }

        // 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // 바라볼 방향
        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 목표 지점에 도착했는지 체크
        if (Vector3.Distance(transform.position, targetPos) < arriveDistance)
        {
            switch (patrolType)
            {
                case PatrolType.Sequential:
                    currentPointIndex++;
                    if (currentPointIndex >= patrolPoints.Count)
                        currentPointIndex = 0; // 다시 처음으로
                    break;

                case PatrolType.Random:
                    int newIndex;
                    do
                    {
                        newIndex = Random.Range(0, patrolPoints.Count);
                    } while (newIndex == currentPointIndex && patrolPoints.Count > 1);

                    currentPointIndex = newIndex;
                    break;
            }
        }
    }
}
