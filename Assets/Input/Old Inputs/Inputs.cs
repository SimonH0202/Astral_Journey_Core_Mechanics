using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inputs : MonoBehaviour
{
    [Header("Character Inputs Values")]

    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool run;
    public bool attack;

    public void OnMove(InputValue value)
	{
		MoveInput(value.Get<Vector2>());
	}

	public void OnLook(InputValue value)
	{
		LookInput(value.Get<Vector2>());
	}
	public void OnJump(InputValue value)
	{
		JumpInput(value.isPressed);
	}
    public void OnRun(InputValue value)
	{
		RunInput(value.isPressed);
	}
	public void OnAttack1(InputValue value)
	{
		AttackInput(value.isPressed);
	}

    //Events to bools
	public void MoveInput(Vector2 newMoveDirection)
	{
		move = newMoveDirection;
	} 
	public void LookInput(Vector2 newLookDirection)
	{
		look = newLookDirection;
	}
	public void JumpInput(bool newJumpState)
	{
		jump = newJumpState;
	}
    public void RunInput(bool newSprintState)
	{
		run = newSprintState;
	}
	public void AttackInput(bool newAttackState)
	{
		attack = newAttackState;
	}
}
