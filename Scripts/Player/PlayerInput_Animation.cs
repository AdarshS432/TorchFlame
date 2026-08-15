using Godot;
using System;

public partial class PlayerInput: CharacterBody3D
{
	private void SetLocomotion(bool running)
	{
		if(running)
		PlayerAnimation.Set("parameters/Locomotion/transition_request", "Running");
		else
		PlayerAnimation.Set("parameters/Locomotion/transition_request", "Idle");
	}
	private void PlayJumpAnimation()
	{
		PlayerAnimation.Set("parameters/Jump/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
	}
	private void PlayDeathAnimation()
	{
		PlayerAnimation.Set("parameters/Die/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
	}
	private void PlayBonesSimulation() {
		
	}
}
