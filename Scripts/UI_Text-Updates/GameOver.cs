using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public partial class GameOver : Panel
{
	private GameManager gameManager;

	private Label Survived, Distance;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gameManager = GetNode<GameManager>("../%GameManager");
		Survived = FindChild("Survived") as Label;
		Distance = FindChild("Distance") as Label;

		this.Visible = false;
		this.GetChildren().ToList().ForEach(child => (child as Control).Visible = false);

		ProcessMode = ProcessModeEnum.Always;

		gameManager.GameOver += OnGameOver;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void OnGameOver()
	{
		this.Visible = true;
		Tween animation = this.CreateTween();
		animation.SetPauseMode(Tween.TweenPauseMode.Process);
		animation.TweenProperty(this, "modulate:a", 1.0f, 3.0f);
		animation.Parallel().TweenCallback(Callable.From(() =>
		{
			//GetTree().Paused = true;
		})).SetDelay(0.5f);
		SetText();
		foreach(Control child in this.FindChildren("*", "Control", recursive: true)) {
			child.Visible = true;
			if(child is Label) {
				child.SelfModulate = Colors.Transparent;
				animation.Chain().TweenProperty(child, "self_modulate", Colors.White, 1.0f);
			}
		}
		animation.TweenInterval(3.0f);
		foreach(Control child in this.FindChildren("*", "Control", recursive: true)) {
			child.Visible = true;
			if(child is Label) {
				animation.Chain().TweenProperty(child, "self_modulate", Colors.Transparent, 0.5f);
			}
		}
		animation.Chain().TweenCallback(Callable.From(() =>
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
			GetTree().Paused = false;
			GetTree().ChangeSceneToFile("res://Scenes/TitleScreen.tscn");
		}));
	}
	private void SetText()
	{
		Survived.Text = $"Time: {GameManager.Time_Survived.Elapsed.ToString(@"m\:ss")}";
		Distance.Text = $"{GameManager.DistanceToEnd:###.##} Meters from End";
	}
}
