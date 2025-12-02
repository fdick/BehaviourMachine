using BehaviourGraph;
using BehaviourGraph.States;
using UnityEngine;

public class RotateState : State, IUpdatableState
{
   private Transform _body;
   private float _rotateSpeed;
   private Vector3 _rotateAxis;

   public RotateState(Transform body, float rotateSpeed, Vector3 rotateAxis)
   {
      _body = body;
      _rotateSpeed = rotateSpeed;
      _rotateAxis = rotateAxis;
   }
   
   public void UpdateState()
   {
      _body.Rotate(_rotateAxis, _rotateSpeed * Time.deltaTime);
   }
}
