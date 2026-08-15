using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
public partial class GameManager : Node3D
{
    public static float FRAMES;
    [Export] public Node Dungeon_Generator;
    public static List<Node3D> Rooms_List = new();

    public static float DistanceToStart, DistanceToEnd;
    public static Stopwatch Time_Survived;


    [Signal]
    public delegate void GameOverEventHandler();

    [Signal]
    public delegate void GameWonEventHandler();


    [Export] public MapManager map_manager;

    public static CharacterBody3D Player;

    public static Vector3 Start_Room, End_Room;

    public override void _Ready()
    {
        FRAMES = 0;
        Time_Survived = new();
        Time_Survived.Reset();

        Player = GetNode<CharacterBody3D>("%Player");
        Dungeon_Generator.Connect("done_generating", Callable.From(() => CheckRooms()));

        Time_Survived.Start();
    }
    public override void _Process(double delta)
    {
        FRAMES++;

        DistanceToStart = PlayerInput.Player_GlobalPosition.DistanceTo(map_manager.Start_Room);
        DistanceToEnd = PlayerInput.Player_GlobalPosition.DistanceTo(map_manager.End_Room);

        CheckForPlayerAtEnd();
    }
    public override void _Input(InputEvent @e)
    {
        if(@e.IsActionPressed("ui_cancel"))
		{
			Input.MouseMode = (Input.MouseMode == Input.MouseModeEnum.Captured) ? 
                        Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
		}
    }
    private void CheckRooms()
    {
        var Rooms = Dungeon_Generator.Get("rooms_container").As<Node3D>();

		foreach(Node3D room in Rooms.GetChildren())
		{
			var room_manager = room.GetNodeOrNull<RoomManager>("RoomManager");
            if(room_manager != null) room_manager.PlayerIsInFire += OnPlayerInFire;
            //if(room_manager != null) room_manager.OnPlayerRoomExit += CheckForPlayerAtEnd;
            Rooms_List.Add(room);
		}
    }
    private void OnPlayerInFire()
    {
        Time_Survived.Stop();
        EmitSignal(SignalName.GameOver);
        //GD.Print("Game Over Signal Emitted");
    }
    public void CheckForPlayerAtEnd()
    {
        float y_diff = End_Room.Y - PlayerInput.Player_GlobalPosition.Y;
        //GD.Print(y_diff);
		Vector2 End2D = new Vector2(End_Room.X, End_Room.Z);
		bool playerInRoom = PlayerInput.Player2D_GlobalPosition.DistanceTo(End2D) <= 6f && 
			y_diff <= 3.0f && y_diff >= -3.0f;
        //GD.Print(PlayerInput.Player2D_GlobalPosition.DistanceTo(End2D));

        if(playerInRoom)
        {
            Time_Survived.Stop();
            EmitSignal(SignalName.GameWon);
            GD.Print("Player Entered End");
        }
    }
}