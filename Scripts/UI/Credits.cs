using Godot;
using System;

public partial class Credits : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Pressed += ChangeScene;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void ChangeScene()
	{
		GetTree().CallDeferred("change_scene_to_file", "res://Scenes/Credits.tscn");
	}
}
