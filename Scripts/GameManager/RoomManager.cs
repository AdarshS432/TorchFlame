using Godot;
using System;
using System.Linq.Expressions;
using System.Reflection;

public partial class RoomManager : Node3D
{
	private Node DungeonRoom;
	[Export] public PackedScene Door_Mask;

	public bool playerInRoom = false;
	private bool room_on_fire;
	private bool previous_playerInRoom;

	private RoomFire RoomFire;

	[Signal]
	public delegate void OnPlayerRoomEnterEventHandler();
	[Signal]
	public delegate void OnPlayerRoomExitEventHandler();

	[Signal]
	public delegate void PlayerIsInFireEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		room_on_fire = false;

		DungeonRoom = CheckParentNode(this);

		DungeonRoom.Connect("dungeon_done_generating", Callable.From(() => RemoveUnusedDoors()));
		RoomFire = GetNodeOrNull<RoomFire>("../Room_Fire");
		
		if(RoomFire != null) RoomFire.FireStarted += () => room_on_fire = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//if(Player == null || !GodotObject.IsInstanceValid(Player)) return;
		previous_playerInRoom = playerInRoom;

		float y_diff = this.GlobalPosition.Y - PlayerInput.Player_GlobalPosition.Y;
		Vector2 this2D = new Vector2(this.GlobalPosition.X, this.GlobalPosition.Z);
		playerInRoom = PlayerInput.Player2D_GlobalPosition.DistanceTo(this2D) <= 5.5f && 
			y_diff < 3.0f && y_diff > -3.0f && GameManager.Player.IsOnFloor();


		if(previous_playerInRoom != playerInRoom)
		{
			if(playerInRoom)
			{
				EmitSignal(SignalName.OnPlayerRoomEnter);
			} else
			{
				EmitSignal(SignalName.OnPlayerRoomExit);
			}
		}
		CheckForPlayerInRoom();
	}
	private Node CheckParentNode(Node node)
	{
		if(node == null) return null;
		Node current = node;
		if(current.GetClass() == "DungeonRoom3D" || current.HasSignal("dungeon_done_generating"))
		{
			return current;
		} else
		{
			return CheckParentNode(current.GetParent());
		}
	}
	private void RemoveUnusedDoors()
	{
		var doors = DungeonRoom.Call("get_doors").AsGodotArray();
		foreach(var door in doors)
		{
			GodotObject door_object = door.AsGodotObject();
			if(door_object.Call("get_room_leads_to").Obj == null)
			{
				Node door_node = door_object.Get("door_node").As<Node>();
				if(door_node is Node3D node)
				{
					var mask_instance = Door_Mask.Instantiate<Node3D>();
					door_node.GetParent().AddChild(mask_instance);

					mask_instance.GlobalPosition = node.GlobalPosition;
					mask_instance.GlobalRotation = node.GlobalRotation;
					mask_instance.RotateY(Mathf.Pi / 2.0f);
					//GD.Print($"Instantiating mask at: {mask_instance.GlobalPosition}");
				}
			}
		}
	}
	private void CheckForPlayerInRoom()
	{
		if(playerInRoom && room_on_fire)
		{
			if(GetParent().Get("is_stair_room").As<bool>() == true) return;

			EmitSignal(SignalName.PlayerIsInFire);
			//GD.Print("Player is in fire");
		}
	}
}
