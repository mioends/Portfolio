using UnityEngine;

public class ZombieknightMove : MonoBehaviour
{
    public Animator animator;
    private Transform tr;
    public Transform[] patrolPoints;
    public Transform player;
    Dialog dialog;

    public int currentPointIndex;
    public float moveSpeed = 0.5f;
    private float stoppingDistance = 0.05f;
    private Vector2 currentTarget;

    private readonly int hashWalk = Animator.StringToHash("Walk");
    private readonly int hashRun = Animator.StringToHash("Run");
    private readonly int hashSkill = Animator.StringToHash("Skill");

    private bool isChasing = false; // 추격 중인지 여부

    void Start()
    {
        animator = GetComponent<Animator>();
        tr = GetComponent<Transform>();
        player = FindAnyObjectByType<PlayerControl>().transform;
        dialog = FindAnyObjectByType<Dialog>();

        Transform patrolParent = GameObject.Find("PatrolPoints").transform;
        patrolPoints = patrolParent.GetComponentsInChildren<Transform>();
        currentPointIndex = Random.Range(1, patrolPoints.Length);
    }

    void Update()
    {
        if (GetComponent<ZombieKnightDamage>().isDead) return;

        if (GameManager.instance.isGameOver || dialog.isTalking)
        {
            // 게임 오버 시 공격/추격 멈추고 순찰로 복귀
            ResumePatrol();
            AnimationChange(true, false, false); // 걷기 애니메이션
            PatrolPointsCheck();
            return;
        }

        if (!isChasing)
            PatrolPointsCheck();

        PlayerChase();
    }

    #region Patrol
    private void PatrolPointsCheck()
    {
        currentTarget = patrolPoints[currentPointIndex].position;
        Vector2 currentPosition = tr.position;

        Vector2 direction = (currentTarget - currentPosition).normalized;
        tr.position = Vector2.MoveTowards(currentPosition, currentTarget, moveSpeed * Time.deltaTime);

        if (direction.x != 0)
        {
            Vector3 scale = tr.localScale;
            scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
            tr.localScale = scale;
        }

        animator.SetBool(hashWalk, true);

        if (Vector2.Distance(currentPosition, currentTarget) < stoppingDistance)
        {
            currentPointIndex = Random.Range(1, patrolPoints.Length);
        }
    }

    /// 외부에서 호출할 수 있는 이동 함수
    public void Move(Vector2 direction)
    {
        isChasing = true;

        if (direction.magnitude > 0.1f)
        {
            Vector3 newPos = tr.position + (Vector3)(direction.normalized * moveSpeed * Time.deltaTime);

            // Y축 고정
            newPos.y = tr.position.y;

            tr.position = newPos;

            Vector3 scale = tr.localScale;
            scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
            tr.localScale = scale;
        }
    }


    /// 추격 상태 종료 (순찰로 돌아감)
    public void ResumePatrol()
    {
        isChasing = false;
    }
    #endregion

    #region Chase
    void PlayerChase()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= 1f)
        {
            AnimationChange(false, false, true);
            Move(Vector2.zero); // 이동 중지
        }
        else if (distance <= 2f)
        {
            AnimationChange(false, true, false);
            moveSpeed = 1f;
            Vector2 direction = (player.position - transform.position).normalized;
            Move(direction); // 방향 이동
        }
        else
        {
            AnimationChange(true, false, false);
            Move(Vector2.zero);
            moveSpeed = 0.5f;
            ResumePatrol(); // 거리 멀어졌을 때 순찰 재개
        }
    }
    public void AnimationChange(bool isWalk, bool isRun, bool isAttack)
    {
        animator.SetBool(hashWalk, isWalk);
        animator.SetBool(hashRun, isRun);
        animator.SetBool(hashSkill, isAttack);
    }
    #endregion
}