using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

public partial class Compass : Node3D
{
	[Export] public Vector3 North;
	[Export] public Node Rotate;
	//[Export] public Vector3 East;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(Rotate is Node2D s2)
		{
			s2.GlobalRotation = 0;
		} else if(Rotate is Node3D s3)
		{
			s3.GlobalRotation = Vector3.Zero;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Rotate is Node2D s2)
		{
			Vector3 rot = new Vector3(-Mathf.Sin(s2.GlobalRotation), 0.0f, -Mathf.Cos(s2.GlobalRotation));
			Vector3 player_forward = -PlayerInput.Player_Transform.Basis.Z;
			float angle = North.SignedAngleTo(player_forward, Vector3.Up);
			s2.GlobalRotation = angle;
		} else if(Rotate is Node3D s3)
		{
			Vector3 rot = new Vector3(-Mathf.Sin(s3.GlobalRotation.Y), 0.0f, -Mathf.Cos(s3.GlobalRotation.Y));
			Vector3 player_forward = -PlayerInput.Player_Transform.Basis.Z;
			float angle = North.SignedAngleTo(player_forward, Vector3.Up);
			Basis newBasis = Basis.Rotated(Vector3.Down, angle);
			s3.Basis = newBasis.Scaled(s3.Scale);
		}
	}
}
