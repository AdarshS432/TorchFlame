using Godot;
using System;
using GodotCookies;
using System.Collections.Generic;

public partial class Settings : VBoxContainer
{
	private SpinBox Levels, SizeBox, FireDelay;
	private Button RoomPlacement;
	private Button Map, Compass, Coordinates;
	private Button Reset;

	private System.Collections.Generic.Dictionary<string, bool> Inventory_Items = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Levels = GetNode<SpinBox>("%Levels");
		SizeBox = GetNode<SpinBox>("%Size");
		FireDelay = GetNode<SpinBox>("%FireDelay");

		
		if(Input.GetConnectedJoypads().Count > 0) Levels.GrabFocus();

		Levels.ValueChanged += SaveLevelCount;
		SizeBox.ValueChanged += SaveSize;
		FireDelay.ValueChanged += SaveFireDelay;

		Map = GetNode<Button>("%MapButton");
		Compass = GetNode<Button>("%CompassButton");
		Coordinates = GetNode<Button>("%CoordinatesButton");

		RoomPlacement = GetNode<Button>("%RoomPlacementButton");

		Reset = GetNode<Button>("%ResetButton");


		Levels.Value = Cookies.User.Get<int>("LevelCount", 3);
		SizeBox.Value = Cookies.User.Get<int>("DungeonSize", 10);
		FireDelay.Value = Cookies.User.Get<float>("FireDelay", 10.0f);

		var Items_Save = Cookies.User.Get<System.Collections.Generic.Dictionary<string, bool>>("Inventory_Items");
		if(Items_Save != null && Items_Save.Count >= 0)
		{
			Map.ButtonPressed = !Items_Save["Map"];
			Compass.ButtonPressed = !Items_Save["Compass"];
		}

		Coordinates.ButtonPressed = !Cookies.User.Get<bool>("Coordinates", true);

		RoomPlacement.ButtonPressed = !Cookies.User.Get<bool>("RoomPlacement", true);


		Inventory_Items.Add("Map", !Map.ButtonPressed); //NAMES MUST MATCH PERFECTLY WITH TOOL NAMES
		Inventory_Items.Add("Compass", !Compass.ButtonPressed);

		Cookies.User.Set("Coordinates", !Coordinates.ButtonPressed);

		Map.Pressed += () => SaveToggle(Map, "Map", "Map Enabled", "Map Disabled");
		Compass.Pressed += () => SaveToggle(Compass, "Compass", "Compass Enabled", "Compass Disabled");
		Coordinates.Pressed += () => SaveToggle(Coordinates, "Coordinates", "Coordinates Enabled", "Coordinates Disabled");

		RoomPlacement.Pressed += () => SaveToggle(RoomPlacement, "RoomPlacement", "Room Placement: Default", "Room Placement: Random");

		Reset.Pressed += ResetSettings;

		SaveToggle(Map, "Map", "Map Enabled", "Map Disabled");
		SaveToggle(Compass, "Compass", "Compass Enabled", "Compass Disabled");
		SaveToggle(Coordinates, "Coordinates", "Coordinates Enabled", "Coordinates Disabled");
		SaveToggle(RoomPlacement, "RoomPlacement", "Room Placement: Default", "Room Placement: Random");

		this.TreeExiting += SaveAll;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void SaveLevelCount(double value)
	{
		int finalvalue = (int)value;
		Cookies.User.Set("LevelCount", finalvalue);
	}
	private void SaveSize(double value)
	{
		int finalvalue = (int)value;
		Cookies.User.Set("DungeonSize", finalvalue);
	}
	private void SaveFireDelay(double value)
	{
		float finalvalue = (float)value;
		Cookies.User.Set("FireDelay", finalvalue);
	}
	private void SaveAll()
	{
		SaveLevelCount(Levels.Value);
		SaveSize(SizeBox.Value);
		SaveFireDelay(FireDelay.Value);

		SaveToggle(Map, "Map", "Map Enabled", "Map Disabled");
		SaveToggle(Compass, "Compass", "Compass Enabled", "Compass Disabled");
		SaveToggle(Coordinates, "Coordinates", "Coordinates Enabled", "Coordinates Disabled");
	}
	private void SaveToggle(Button button, string itemName, string enabledText, string disabledText)
	{
		button.Text = !button.ButtonPressed ? enabledText : disabledText;

		Inventory_Items.Clear();
		Inventory_Items.Add("Map", !Map.ButtonPressed);
		Inventory_Items.Add("Compass", !Compass.ButtonPressed);

		Cookies.User.Set("Coordinates", !Coordinates.ButtonPressed);
		Cookies.User.Set("RoomPlacement", !RoomPlacement.ButtonPressed);

		Cookies.User.Set("Inventory_Items", Inventory_Items);
	}
	private void ResetSettings()
	{
		Levels.Value = 3;
		SizeBox.Value = 10;
		FireDelay.Value = 10;

		Map.ButtonPressed = false;
		Compass.ButtonPressed = false;

		Coordinates.ButtonPressed = true;

		SaveAll();
	}
}
