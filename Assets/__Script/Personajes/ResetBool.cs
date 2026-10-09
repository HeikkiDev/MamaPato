using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetBool : StateMachineBehaviour
{
   public string interactuandoBool;
   public bool interactuandoEstado;
   //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
   override public void OnStateEnter(Animator animador, AnimatorStateInfo stateInfo, int layerIndex)
   {
       animador.SetBool(interactuandoBool, interactuandoEstado);
   }

}
