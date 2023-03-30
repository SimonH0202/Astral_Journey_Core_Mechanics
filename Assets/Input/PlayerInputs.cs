using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInputs : MonoBehaviour
{
	[Header("Character Input Values")]
	public Vector2 move;
	public Vector2 look;
	public bool jump;
	public bool attack;
	public bool sprint;
	public bool dodge;
<<<<<<< Updated upstream
	public bool grenadeAim;
	public bool grenadeShoot;
=======
	public bool grenadeState;
	public bool meleeState;
	public bool shooterState;

>>>>>>> Stashed changes

	[Header("Movement Settings")]
	public bool analogMovement;

	[Header("Mouse Cursor Settings")]
	public bool cursorLocked = true;
	public bool cursorInputForLook = true;


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
	public void OnAttack(InputValue value)
	{
		AttackInput(value.isPressed);
	}
	public void OnSprint(InputValue value)
	{
		SprintInput(value.isPressed);
	}
	public void OnDodge(InputValue value)
	{
		DodgeInput(value.isPressed);
	}
<<<<<<< Updated upstream
	public void OnGrenadeAim(InputValue value)
	{
		GrenadeAimInput(value.isPressed);
	}
	public void OnGrenadeShoot(InputValue value)
	{
		GrenadeShootInput(value.isPressed);
	}

=======
	public void OnGrenadeState(InputValue value)
	{
		GrenadeStateInput(value.isPressed);
	}
	public void OnMeleeState(InputValue value)
	{
		MeleeStateInput(value.isPressed);
	}
	public void OnShooterState(InputValue value)
	{
		ShooterStateInput(value.isPressed);
	}
>>>>>>> Stashed changes

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
	public void AttackInput(bool newAttackState)
	{
		attack = newAttackState;
	}
	public void SprintInput(bool newSprintState)
	{
		sprint = newSprintState;
	}
	public void DodgeInput(bool newDodgeState)
	{
		dodge = newDodgeState;
	}
<<<<<<< Updated upstream
	public void GrenadeAimInput(bool newGrenadeAimState)
	{
		grenadeAim = newGrenadeAimState;
	}
	public void GrenadeShootInput(bool newGrenadeShootState)
	{
		grenadeShoot = newGrenadeShootState;
	}

=======
	public void GrenadeStateInput(bool newGrenadeState)
	{
		grenadeState = newGrenadeState;
	}
	public void MeleeStateInput(bool newMeleeState)
	{
		meleeState = newMeleeState;
	}
	public void ShooterStateInput(bool newShooterState)
	{
		shooterState = newShooterState;
	}
>>>>>>> Stashed changes

	private void OnApplicationFocus(bool hasFocus)
	{
		SetCursorState(cursorLocked);
	}

	private void SetCursorState(bool newState)
	{
		Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
	}
	}