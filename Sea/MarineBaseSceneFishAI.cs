using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarineBaseSceneFishAI : MonoBehaviour
{
    public enum PatrolType
    {
        Sequential, // �������
        Random      // ����
    }

    [Header("Patrol Settings")]
    public GameObject patrolPointsParent;
    private List<Transform> patrolPoints = new List<Transform>();

    public PatrolType patrolType = PatrolType.Sequential; // �ν����Ϳ��� ���� ����
    private int currentPointIndex = 0;

    [Header("Movement Settings")]
    public float speed = 3f;
    public float arriveDistance = 0.2f;
    public float rotationSpeed = 5f; // ȸ�� �ӵ�

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
        if (patrolPoints.Count > 0) patrolPoints.RemoveAt(0); // parent ����
        currentPointIndex = 0;
    }

    void Update()
    {
        if (patrolPoints.Count == 0) return;

        // ��ǥ ����Ʈ
        Transform targetPoint = patrolPoints[currentPointIndex];
        Vector3 targetPos;

        if (patrolType == PatrolType.Sequential)
        {
            // y�� ���� (���� �̵���)
            targetPos = new Vector3(targetPoint.position.x, transform.position.y, targetPoint.position.z);
        }
        else
        {
            // ���� ��� �� y���� ���� (3D ���� �̵�)
            targetPos = targetPoint.position;
        }

        // �̵�
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // �ٶ� ����
        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // ��ǥ ������ �����ߴ��� üũ
        if (Vector3.Distance(transform.position, targetPos) < arriveDistance)
        {
            switch (patrolType)
            {
                case PatrolType.Sequential:
                    currentPointIndex++;
                    if (currentPointIndex >= patrolPoints.Count)
                        currentPointIndex = 0; // �ٽ� ó������
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
