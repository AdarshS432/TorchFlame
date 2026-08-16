using Godot;
using System;

public partial class GameAudio : AudioStreamPlayer
{
	private GameManager GameManager;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GameManager = GetNode<GameManager>("%GameManager");
		GameManager.GameOver += FadeOut;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void FadeOut()
	{
		Tween fade = this.CreateTween();
		fade.TweenProperty(this, "volume_db", -45.0f, 3.0f);
	}
}
