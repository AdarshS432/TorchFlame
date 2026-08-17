using Godot;
using GodotCookies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public partial class MapManager : Node3D
{
	[ExportGroup("Nodes")] 
	[Export] public Node Dungeon_Generator;
	[Export] public MeshInstance3D Map;
	[Export] public PlayerInput Player;

	//[ExportGroup("Pre-placed Room Locations")] 
	//[Export] public Vector3 Start_Room, End_Room;
	public Vector3 Start_Room, End_Room;

	public static List<Vector3> Stair_Locations = new();
	public static List<Rect2I> Used_Terrain_Locations = new();
	public static Vector3 World_Size;
	private Sprite2D Stair, Start, End, Mountains, River, Player_Icon;

	private ImageTexture Rendered_Texture;

	private Image Modified_Image;
	private Image Rendered_Image; 

	private Image Player_Image;
	private Rect2I Player_Rect;

	[Export] public int mountain_count = 2;
	[Export] public int river_count = 1;

	float minX;
	float maxX;
	float minZ;
	float maxZ;

	public int width = 256;
	public int height = 256;
	
	Vector4I margins = new Vector4I(16, 16, 16, 16); //Left, Up, Right, Down

	float usablewidth;
	float usableheight;

	bool coordinates_enabled = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetWorldSize();
		GameManager.Start_Room = this.Start_Room;
		GameManager.End_Room = this.End_Room;
		
		//World_Size = new Vector3(Mathf.Abs(World_Size.X), Mathf.Abs(World_Size.Y), Mathf.Abs(World_Size.Z));
		
		GD.Print($"World_Size: {World_Size}");

		Stair_Locations.Clear();
		Used_Terrain_Locations.Clear();

		Stair = GetNode<Sprite2D>("%Stair");
		Start = GetNode<Sprite2D>("%Start");
		End = GetNode<Sprite2D>("%End");

		Mountains = GetNode<Sprite2D>("%Mountains");
		River = GetNode<Sprite2D>("%River");

		Player_Icon = GetNode<Sprite2D>("%Player_Icon");


		minX = Mathf.Min(Start_Room.X, End_Room.X);
		maxX = Mathf.Max(Start_Room.X, End_Room.X);
		minZ = Mathf.Min(Start_Room.Z, End_Room.Z);
		maxZ = Mathf.Max(Start_Room.Z, End_Room.Z);

		usablewidth = width - margins.X - margins.Z;
 		usableheight = height - margins.Y - margins.W;

		coordinates_enabled = Cookies.User.Get<bool>("Coordinates");

		GD.Randomize();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		RenderPlayerIcon();
	}
	private void RenderPlayerIcon()
	{
		if(!coordinates_enabled) return;
		Rect2I mapRect = new Rect2I(0, 0, Rendered_Image.GetWidth(), Rendered_Image.GetHeight());
		Modified_Image.BlitRect(Rendered_Image, mapRect, Vector2I.Zero);

		Vector2I coords = (Vector2I)PlayerInput.Player2D_GlobalPosition;

		float nX = Mathf.InverseLerp(minX, maxX, coords.X);
		float nZ = Mathf.InverseLerp(minZ, maxZ, coords.Y);

		float halfW = Player_Image.GetWidth() / 2.0f;
		float halfH = Player_Image.GetHeight() / 2.0f;

		float min_map_x = margins.X + halfW;
		float max_map_x = width - margins.Z - halfW;

		float min_map_y = margins.Y + halfH;
		float max_map_y = height - margins.W - halfH;

		float x = Mathf.Lerp(min_map_x, max_map_x, nZ);
		float y = Mathf.Lerp(max_map_y, min_map_y, nX); //INVERT Y COORDINATE

		Vector2I finalcoords = new Vector2I((int)x, (int)y);
		finalcoords -= new Vector2I((int)halfW, (int)halfH);

		Modified_Image.BlendRect(Player_Image, Player_Rect, finalcoords);

		Rendered_Texture.Update(Modified_Image);
	}
	private void GetStairRooms()
	{
		var Rooms = Dungeon_Generator.Get("rooms_container").As<Node3D>();

		foreach(Node3D room in Rooms.GetChildren())
		{
			bool is_stair_room = room.Get("is_stair_room").As<bool>();
			if(is_stair_room)
			{
				Stair_Locations.Add(room.GlobalPosition);
				GD.Print($"Found stair at: {room.GlobalPosition}");
			}
		}
		CreateTexture();
		//InstantiateSpriteMarkers();
	}
	private void CreateTexture()
	{
		Image baseImage = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
		baseImage.Fill(new Color(0.0f, 0.0f, 0.0f, 0.0f));
		int iconsize = 32;

		Image stair = Stair.Texture.GetImage();
		Image start = Start.Texture.GetImage();
		Image end = End.Texture.GetImage();

		Image mountains = Mountains.Texture.GetImage();
		Image river = River.Texture.GetImage();

		Image player = Player_Icon.Texture.GetImage();

		stair.Decompress();
		stair.Convert(Image.Format.Rgba8);

		start.Decompress();
		start.Convert(Image.Format.Rgba8);

		end.Decompress();
		end.Convert(Image.Format.Rgba8);

		mountains.Decompress();
		mountains.Convert(Image.Format.Rgba8);
		river.Decompress();
		river.Convert(Image.Format.Rgba8);

		player.Decompress();
		player.Convert(Image.Format.Rgba8);

		stair.Resize(iconsize, iconsize);
		start.Resize(iconsize, iconsize);
		end.Resize(iconsize, iconsize);

		player.Resize(iconsize, iconsize);

		float river_z_rot = GD.Randi() % 2;

		mountains.Resize(iconsize * 2, iconsize * 2);
		if(river_z_rot == 0)
		{
			river.Resize(iconsize * 2, iconsize);
		}
		else if(river_z_rot == 1)
		{
			river.Rotate90(ClockDirection.Clockwise);
			river.Resize(iconsize, iconsize * 2);
		}

		Rect2I stair_r = new Rect2I(0, 0, stair.GetWidth(), stair.GetHeight());
		Rect2I start_r = new Rect2I(0, 0, start.GetWidth(), start.GetHeight());
		Rect2I end_r = new Rect2I(0, 0, end.GetWidth(), end.GetHeight());

		Rect2I mountains_r = new Rect2I(0, 0, mountains.GetWidth(), mountains.GetHeight());
		Rect2I river_r = new Rect2I(0, 0, river.GetWidth(), river.GetHeight());

		Player_Rect = new Rect2I(0, 0, player.GetWidth(), player.GetHeight());
		Player_Image = player;


		Vector2I start_dest = new Vector2I(width - start.GetWidth() - margins.Z, height - start.GetHeight() - margins.W);
		Vector2I end_dest = new Vector2I(margins.X, margins.Y);

		GD.Print($"Start at: {start_dest}, End at: {end_dest}");

		List<Vector2I> stair_dest = new();

		float minX = Mathf.Min(Start_Room.X, End_Room.X);
		float maxX = Mathf.Max(Start_Room.X, End_Room.X);
		float minZ = Mathf.Min(Start_Room.Z, End_Room.Z);
		float maxZ = Mathf.Max(Start_Room.Z, End_Room.Z);

		foreach(Vector3 loc in Stair_Locations) //Forward: +X, Rightward: +Z
		{
			/*Vector2 remap = new((loc.Z + World_Size.Z / 2.0f) / World_Size.Z, (loc.X + World_Size.X / 2.0f) / World_Size.X);
			float x = Mathf.Remap(remap.X, 0.0f, 1.0f, margins.X, margins.X + usablewidth);
			float y = Mathf.Remap(remap.Y, 0.0f, 1.0f, margins.Y + usableheight, margins.Y);

			x -= stair.GetWidth() / 2.0f;
			y -= stair.GetHeight() / 2.0f;

			stair_dest.Add(new Vector2I(
						(int)x,
						(int)y));*/
						
			float nX = Mathf.InverseLerp(minX, maxX, loc.X);
			float nZ = Mathf.InverseLerp(minZ, maxZ, loc.Z);

			float halfW = stair.GetWidth() / 2.0f;
			float halfH = stair.GetHeight() / 2.0f;

			float min_map_x = margins.X + halfW;
			float max_map_x = width - margins.Z - halfW;

			float min_map_y = margins.Y + halfH;
			float max_map_y = height - margins.W - halfH;

			float x = Mathf.Lerp(min_map_x, max_map_x, nZ);
			float y = Mathf.Lerp(max_map_y, min_map_y, nX); //INVERT Y COORDINATE

			Vector2I finalcoords = new Vector2I((int)x, (int)y);
			finalcoords -= new Vector2I((int)halfW, (int)halfH);

			stair_dest.Add(finalcoords);
		}
		for(int i = 0; i < mountain_count; i++)
		{
			float mountains_x = margins.X + GD.Randf() * usablewidth - mountains.GetSize().X;
			float mountains_y = margins.Y + GD.Randf() * usableheight - mountains.GetSize().Y;
			int iterations = 0;
			while(!canPlace(new Vector2I((int)mountains_x, (int)mountains_y)))
			{
				mountains_x = margins.X + GD.Randf() * usablewidth - mountains.GetSize().X;
				mountains_y = margins.Y + GD.Randf() * usableheight - mountains.GetSize().Y;
				if(iterations > 64) break;
				iterations++;
			}
			if(iterations > 64) break;
			
			Vector2I mountains_dest = new Vector2I((int)Mathf.Abs(mountains_x), (int)Mathf.Abs(mountains_y));

			baseImage.BlendRect(mountains, mountains_r, mountains_dest);
			Used_Terrain_Locations.Add(mountains_r);
		}
		for(int i = 0; i < river_count; i++)
		{
			float river_x = margins.X + GD.Randf() * usablewidth - river.GetSize().X;
			float river_y = margins.Y + GD.Randf() * usableheight - river.GetSize().Y;
			int iterations = 0;
			while(!canPlace(new Vector2I((int)river_x, (int)river_y)))
			{
				river_x = margins.X + GD.Randf() * usablewidth - river.GetSize().X;
				river_y = margins.Y + GD.Randf() * usableheight - river.GetSize().Y;
				if(iterations > 64) break;
				iterations++;
			}
			if(iterations > 64) break;
			
			Vector2I river_dest = new Vector2I((int)Mathf.Abs(river_x), (int)Mathf.Abs(river_y));

			baseImage.BlendRect(river, river_r, river_dest);
			Used_Terrain_Locations.Add(river_r);
		}

		foreach(Vector2I v in stair_dest)
		{
			baseImage.BlendRect(stair, stair_r, v);
		}

		baseImage.BlendRect(start, start_r, start_dest);
		baseImage.BlendRect(end, end_r, end_dest);

		ImageTexture finalTexture = ImageTexture.CreateFromImage(baseImage);
		var mat = Map.GetSurfaceOverrideMaterial(0) as StandardMaterial3D;
		mat.AlbedoTexture = finalTexture;

		Rendered_Image = baseImage;
		Rendered_Texture = finalTexture;

		Modified_Image = Image.CreateEmpty(Rendered_Image.GetWidth(), Rendered_Image.GetHeight(), false, Rendered_Image.GetFormat());
	}
	/*private void InstantiateSpriteMarkers()
	{
		Sprite3D start = new Sprite3D();
		start.Texture = Start.Texture;
		Sprite3D end = new Sprite3D();
		end.Texture = End.Texture;

		Sprite3D stair = new Sprite3D();
		stair.Texture = Stair.Texture;

		start.GlobalPosition = Map.GlobalPosition + Vector3.Back;


		/*Vector2I start_dest = new Vector2I(width - Start.GetWidth() / 2, Start.GetHeight() / 2);
		Vector2I end_dest = new Vector2I(End.GetWidth() / 2, height - End.GetHeight() / 2);

		List<Vector2I> stair_dest = new();
		foreach(Vector3 loc in Stair_Locations)
		{
			float x = loc.X / World_Size.X;
			float y = loc.Z / World_Size.Z;
			stair_dest.Add(new Vector2I((int)(x * width - (stair.GetWidth() / 2.0f)), (int)(y * height + (stair.GetHeight() / 2.0f))));
		}
		foreach(Vector2I v in stair_dest)
		{
			
		}
	}*/
	private bool canPlace(Vector2I vect)
	{
		foreach(Rect2I r in Used_Terrain_Locations)
		{
			if (r.HasPoint(vect))
			{
				return false;
			}
		}
		return true;
	}
	private void GetWorldSize()
	{
		int sidelength = Cookies.User.Get<int>("DungeonSize");
		int height = Cookies.User.Get<int>("LevelCount");

		Vector3I grid_size = new Vector3I(sidelength, height, sidelength);
		GD.Print($"Side Length: {sidelength}, Height: {height}, Size: {grid_size}");
		Dungeon_Generator.Set("dungeon_size", grid_size);

		Vector3 voxel_scale = Dungeon_Generator.Get("voxel_scale").As<Vector3>();

		World_Size = voxel_scale * (grid_size - Vector3I.One);
		Start_Room = new Vector3(-World_Size.X / 2.0f, World_Size.Y / 2.0f, World_Size.Z / 2.0f);
        End_Room = -Start_Room;

		bool room_placement = Cookies.User.Get<bool>("RoomPlacement");
		if(!room_placement) //TRUE: DEFAULT, FALSE: RANDOM
		{
			int steps = Mathf.FloorToInt(Mathf.Abs(End_Room.Y - Start_Room.Y) / voxel_scale.Y) + 1;

			float start_step = GD.Randi() % steps;
			float end_step = GD.Randi() % steps;

			Start_Room.Y -= start_step * voxel_scale.Y;
			End_Room.Y += end_step * voxel_scale.Y;
		}

		var StartRoom_Geometry = Dungeon_Generator.FindChild("Start", true, false) as Node3D;
		var EndRoom_Geometry = Dungeon_Generator.FindChild("End", true, false) as Node3D;

		StartRoom_Geometry.GlobalPosition = Start_Room;
		EndRoom_Geometry.GlobalPosition = End_Room;

		GD.Print($"Set Start Room to: {StartRoom_Geometry.GlobalPosition}, and End Room to: {EndRoom_Geometry.GlobalPosition}");

		Player.SpawnPlayer(Start_Room + new Vector3(0.0f, -2.5f, 0.0f));

		Dungeon_Generator.Connect("done_generating", Callable.From(() => GetStairRooms()));
		Dungeon_Generator.CallDeferred("generate");
	}
}
