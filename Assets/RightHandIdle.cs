using UnityEngine;
using UnityEngine.UI;

public class RightHandIdle : StateMachineBehaviour
{
    [SerializeField] private float animationSpeed;
    [SerializeField] private Vector3 idlePosition = new Vector3(632.6f, -401f, 0f);

    

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //e�er idle positionda de�ilse oraya g�t�r
        Vector3 currentPosition = animator.GetComponent<RectTransform>().anchoredPosition;
        animator.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(currentPosition, idlePosition, animationSpeed * Time.deltaTime);

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

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
