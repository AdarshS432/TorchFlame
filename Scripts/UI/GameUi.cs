using Godot;
using System;
using System.Linq.Expressions;

public partial class GameUi : CanvasLayer
{
	private float width = 256;
	private MarginContainer Menu;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Menu = GetNode<MarginContainer>("Menu_Margins");

		if(OS.HasFeature("mobile"))
		{
			SetVisibility(true);
		} else
		{
			Menu.Visible = false;
			SetVisibility(false);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
    public override void _Input(InputEvent @event)
    {
        if(@event.IsActionPressed("Show_Menu") && !@event.IsEcho())
		{
			SetVisibility(!Menu.Visible);
		}
    }
	private void SetVisibility(bool Visible)
	{
		if(Visible)
		{
			Menu.Visible = true;
			Tween tween = Menu.CreateTween();
			tween.TweenProperty(Menu, new NodePath("position:x"), 0.0f, 0.5f);
		} else
		{
			Tween tween = Menu.CreateTween();
			tween.TweenProperty(Menu, new NodePath("position:x"), -width, 0.5f);
			tween.TweenCallback(Callable.From(() =>
			{
				Menu.Visible = false;
			}));
		}
	}
}
