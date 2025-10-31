using System.Collections;
using UnityEngine;

public enum FishAIState
{
    Patrol,
    Chase,
    Attack,
    Run,
    Stun
}

public class FishState : MonoBehaviour
{
    public FishAIState currentState;

    private FishPatrol patrol;
    private FishAttack attack;
    private Animator animator;

    private WaitForSeconds stateWs;
    private readonly int hashAttackAni = Animator.StringToHash("IsAttack");
    private readonly int hashStunAni = Animator.StringToHash("IsStun");

    private void Awake()
    {
        patrol = GetComponent<FishPatrol>();
        attack = GetComponent<FishAttack>();
        animator = GetComponent<Animator>();
        stateWs = new WaitForSeconds(0.25f);
    }

    private void OnEnable()
    {
        ChangeState(FishAIState.Patrol); // 시작 시 패트롤
        StartCoroutine(StateUpdate());
    }

    private IEnumerator StateUpdate()
    {
        while (true)
        {
            if (currentState == FishAIState.Stun || currentState == FishAIState.Run)
            {
                yield return stateWs;
                continue;
            }
            switch (currentState)
            {
                case FishAIState.Patrol:
                    patrol.Patrol();
                    if (patrol.DetectPlayer(out Transform t))
                    {
                        patrol.SetTarget(t);

                        if (GetComponent<FishD>() != null)
                        {
                            if (patrol.IsInAttackRange())
                            {
                                attack.SetTarget(patrol.Target);
                                ChangeState(FishAIState.Attack);
                            }
                            else
                            {
                                ChangeState(FishAIState.Patrol);
                            }
                        }
                        else
                        {
                            ChangeState(FishAIState.Chase);
                        }
                    }
                    break;

                case FishAIState.Chase:
                    if (patrol.isInSmoke)
                    {
                        ChangeState(FishAIState.Patrol);
                        patrol.ForcePatrol();
                    }
                    if (GetComponent<FishD>() != null)
                    {
                        ChangeState(FishAIState.Attack);
                        break;
                    }

                    patrol.ChaseTarget();
                    if (!patrol.HasTarget || patrol.IsTargetTooFar() || patrol.isInSmoke)
                        ChangeState(FishAIState.Patrol);
                    else if (patrol.IsInAttackRange())
                    {
                        attack.SetTarget(patrol.Target);
                        ChangeState(FishAIState.Attack);
                    }
                    break;

                case FishAIState.Attack:
                    if (patrol.isInSmoke)
                    {
                        ChangeState(FishAIState.Patrol);
                        patrol.ForcePatrol();
                    }
                    if (!patrol.IsInAttackRange())
                    {
                        if (GetComponent<FishD>() != null)
                        {
                            ChangeState(FishAIState.Patrol);
                            break;
                        }
                        ChangeState(FishAIState.Chase);
                    }
                    if (!attack.HasTarget)
                    {
                        ChangeState(FishAIState.Patrol);
                    }
                    break;

                case FishAIState.Run:
                    break;

                case FishAIState.Stun:
                    break;
            }
            yield return stateWs;
        }
    }


    public void ChangeState(FishAIState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        // 상태별 애니메이션
        switch (currentState)
        {
            case FishAIState.Patrol:
                AnimationSet(false, false);
                break;

            case FishAIState.Chase:
                AnimationSet(false, false);
                break;

            case FishAIState.Run:
                AnimationSet(false, false);
                break;

            case FishAIState.Attack:
                AnimationSet(true, false);
                break;

            case FishAIState.Stun:
                AnimationSet(false, true);
                break;
        }
    }

    private void AnimationSet(bool attack, bool stun)
    {
        animator.SetBool(hashAttackAni, attack);
        animator.SetBool(hashStunAni, stun);
    }
}
