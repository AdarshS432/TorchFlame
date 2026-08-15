using Godot;
using System;
using System.Collections.Generic;

public partial class TitleAudio : AudioStreamPlayer
{
	public string[] Scene_List = {"TitleScreen", "GameConfig", "Credits"};
    public override void _Ready()
    {
		Callable.From(CheckForPlaying).CallDeferred();

		GetTree().SceneChanged += OnNewScene;
        GD.Print("TitleScreen Audio Initialized");
    }
	public override void _Process(double delta)
	{
	}
	private void OnNewScene()
	{
		CheckForPlaying();
	}
	private void CheckForPlaying()
	{
		GD.Print($"Number of Scenes: {Scene_List.Length}");
		bool play = false;
		foreach(string name in Scene_List)
		{
			GD.Print($"Input Name: {name}, Scene Name: {GetTree().CurrentScene.SceneFilePath.GetBaseName().Contains(name)}");
			if (GetTree().CurrentScene.SceneFilePath.GetBaseName().Contains(name))
			{
				play = true;
				break;
			}
		}
		if(this.Playing != play)
		{
			this.Playing = play;
		}
	}
}
