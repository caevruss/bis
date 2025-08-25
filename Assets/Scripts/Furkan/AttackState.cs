using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;

public class AttackState : StateMachineBehaviour
{
    [Header("Settings")]
    [SerializeField] private float timeToWaitBeforeAttack;

    [Header("References")]
    private Transform playerTransform;
    private NavMeshAgent agent;
    private ChaseState chaseState;
    private EnemyAttack enemyAttack;

    private float timeWaited;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponent<NavMeshAgent>();
        chaseState = animator.GetBehaviour<ChaseState>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        enemyAttack = animator.GetComponent<EnemyAttack>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timeWaited  += Time.deltaTime;
        if(timeWaited >= timeToWaitBeforeAttack)
        {
            Debug.Log("TIMES UP!");
            enemyAttack.EnemyAttacker();
        }
        Debug.Log($"Time is ticking {timeWaited}");
        Vector3 distance = playerTransform.position - animator.transform.position;
        if(distance.magnitude > chaseState.attackDistance)
        {
            animator.SetBool("isAttacking", false);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timeWaited = 0f;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}

}
