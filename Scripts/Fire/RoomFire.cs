using Godot;
using GodotCookies;
using System;
using System.Diagnostics;

public partial class RoomFire : MeshInstance3D
{
	private GpuParticles3D Fire;
	private Timer timer;
	private Stopwatch elapsed_time;
	private ShaderMaterial fire_shader;
	private RoomManager roomManager;
	private float fire_delay;
	private AudioStreamPlayer3D Sound;

	[Export] public float Fire_Width_Coefficient = 1.0f;
	[Export] public float Fire_Height_Coefficient = 1.0f;

	[Signal]
	public delegate void FireStartedEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Visible = false;

		Fire = GetNode<GpuParticles3D>("%Sparks");
		timer = GetNode<Timer>("Timer");
		Sound = GetNode<AudioStreamPlayer3D>("Sound");
		roomManager = GetNode<RoomManager>("../RoomManager");

		elapsed_time = new Stopwatch();
		timer.Stop();
		timer.Paused = true;

		Sound.Playing = false;

		fire_delay = Cookies.User.Get<float>("FireDelay");

		roomManager.OnPlayerRoomEnter += () => {
			if(timer.Paused)
			{
				ResetTimer();
			}
		};

		timer.Timeout += StartStopwatch;

		elapsed_time.Reset();

		if(this.GetActiveMaterial(0) is ShaderMaterial sh)
		{
			fire_shader = (ShaderMaterial)sh.Duplicate();
			this.SetSurfaceOverrideMaterial(0, fire_shader);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		SetFireWidth((float)elapsed_time.Elapsed.TotalSeconds);
		SetFireHeight((float)elapsed_time.Elapsed.TotalSeconds);
	}
	private void SetFireWidth(float width)
	{
		float current_width = fire_shader.GetShaderParameter("fire_width").As<float>();
		float final_value = Fire_Width_Coefficient * (float)Mathf.Lerp(current_width, width, 0.75);

		fire_shader.SetShaderParameter("fire_width", Mathf.Clamp(final_value, 0.0f, 20.0f));
		float scale = Mathf.Clamp(final_value / 24.0f + 0.5f, float.Epsilon, 1.0f);
		this.Scale = new Vector3(scale, 1.0f, scale);

		//D.Print($"Set Fire Width To: {final_value}");
	}
	private void SetFireHeight(float height)
	{
		float current_height = fire_shader.GetShaderParameter("fire_height").As<float>();
		float exp = Mathf.Pow(height, 2/5);
		float final_value = Fire_Height_Coefficient * (float)Mathf.Lerp(current_height, height, 0.75);

		fire_shader.SetShaderParameter("fire_height", Mathf.Clamp(final_value, 0.0f, 4.0f));
	}
	private void ResetTimer()
	{
		timer.Paused = false;
		timer.Start(fire_delay);
	
		//GD.Print("Timer Reset");
	}
	private void StartStopwatch()
	{
		elapsed_time.Start();
		Sound.Playing = true;
		this.Visible = true;
		EmitSignal(SignalName.FireStarted);
		//GD.Print("Starting Fire");
	}
}
