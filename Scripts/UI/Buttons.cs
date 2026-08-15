using Godot;
using System;

public partial class Buttons : VBoxContainer
{
	private Button Pause, Quit;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pause = GetNode<Button>("Pause");
		Quit = GetNode<Button>("Quit");

		Pause.Pressed += PauseGame;
		Quit.Pressed += QuitGame;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void PauseGame()
	{
		GetTree().Paused = !GetTree().Paused;
		Pause.Text = (GetTree().Paused) ? "Resume" : "Pause";
	}
	private void QuitGame()
	{
		GetTree().CallDeferred("change_scene_to_file", "res://Scenes/TitleScreen.tscn");
	}
}
