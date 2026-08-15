using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using System.Linq;
using GodotCookies;

public partial class InventoryManager : Node3D
{
	public static List<Node> Tools;
	public static int Selected = 0;
	[Export] public Node3D Location;
	public static Node3D Location_Local;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Tools = this.GetChildren().ToList<Node>();

		var Items = Cookies.User.Get<System.Collections.Generic.Dictionary<string, bool>>("Inventory_Items");

		List<Node> copy = new List<Node>(Tools);
		foreach(Node3D n in copy)
		{
			if(Items.ContainsKey(n.Name))
			{
				if(!Items[n.Name])
				{
					Tools.Remove(n);
					n.QueueFree();
				}
			}
		}

		Location_Local = Location;
		
		foreach(Node3D n in Tools)
		{
			n.Visible = (n == Tools[Selected]);
			if(n.Visible)
			{
				n.GlobalPosition = Location.GlobalPosition;
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
    public override void _Input(InputEvent @event)
    {
        if(@event.IsActionPressed("Cycle_Inventory") && !@event.IsEcho())
		{
			CycleInventory();
		}
    }
	public static void CycleInventory()
	{
		if(Selected < Tools.Count() - 1) Selected++;
		else Selected = 0;

		foreach(Node node in Tools)
		{
			if(node is Node3D node3d)
			{
				node3d.Visible = (node == Tools[Selected]);
				node3d.GlobalPosition = Location_Local.GlobalPosition;
			}
		}
	}
}
