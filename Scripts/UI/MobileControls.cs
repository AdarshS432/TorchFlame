using Godot;
using System;

public partial class MobileControls : Control
{
	private Button Sprint, Cycle;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Visible = OS.HasFeature("mobile");

		Sprint = GetNode<Button>("%Sprint");
		Cycle = GetNode<Button>("%Cycle");

		Sprint.Pressed += () => PlayerInput.IsSprinting = Sprint.ButtonPressed;
		Cycle.Pressed += () => InventoryManager.CycleInventory();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
