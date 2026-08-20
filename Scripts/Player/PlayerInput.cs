using Godot;
using System;
using System.Net;
using System.Threading;

public partial class PlayerInput : CharacterBody3D
{
	[Export] public float Gravity = -9.81f;
	private float Gravity_Save;
	[Export] public float GravityScale = 2f;
	public Camera3D cam;
	private Vector2 joystick_sensitivity = new Vector2(0.01f, 1.0f);
	private float sensitivity = 0.0015f;
	[Export] public float Jump_Strength = 6f;
	[Export] public int Max_Wall_Jumps = 1;
	private bool isJumping, IsDashing;
	[Export] public float BaseSpeed = 6f;
	[Export] public float Sprint = 1.6f;
	[Export] public float WalkSpeed = 6f;
	[Export] public float Dash_Speed = 125f;
	[Export] public float Dash_Decay = 250f;
	[Export] public bool UpDownEnabled = false;

	[Export] public GameManager gameManager;
	private float Dash_Current, Dash_Decay_Current;
	public static bool IsSprinting;
	private CanvasLayer GameUI;
	private Vector3 DefaultPos;
	public Vector3 velocity;
	private Node3D Preview;

	public static Vector3 CamRotation;
	private float x_rotation;
	private Node3D PlayerGeometry;
	private VirtualJoystick Movement;
	private VirtualJoystick RotationJoystick;

	private Vector3 previous_velocity;


	public static Vector3 Player_GlobalPosition;
	public static Vector2 Player2D_GlobalPosition;
	public static Transform3D Player_Transform;

	private AnimationTree PlayerAnimation;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		cam = GetViewport().GetCamera3D();

		//Movement = GetNode<CanvasLayer>("%GameUI").FindChild("Movement", true, false) as VirtualJoystick;
		//RotationJoystick = GetNode<CanvasLayer>("%GameUI").FindChild("Rotation", true, false) as VirtualJoystick;

		//Movement.Visible = OS.HasFeature("mobile");
		//RotationJoystick.Visible = OS.HasFeature("mobile");

		PlayerAnimation = GetNode<AnimationTree>("%Player_AnimationTree");
      
		Input.MouseMode = Input.MouseModeEnum.Captured;

		DefaultPos = cam.Position;

		//this.GlobalPosition = GameManager.End_Room;
		//GD.Print(this.GlobalPosition);
		Gravity_Save = Gravity;

		gameManager.GameOver += () => {
			PlayCameraDeathAnimation();
			//GD.Print("PlayerInput received signal");
			this.SetProcess(false);
			this.SetPhysicsProcess(false);
			this.SetProcessInput(false);
		};
		gameManager.GameWon += () => {
			//GD.Print("PlayerInput received signal");
			PlayWinAnimation();
			/*this.SetProcess(false);
			this.SetPhysicsProcess(false);
			this.SetProcessInput(false);*/
		};
		
		//Preview = GetNode<Node3D>("%Preview");
		this.Visible = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		previous_velocity = Velocity;

		Move();
		RotateMobile((float)delta);
		if(isJumping) {
			if(IsOnFloor()/*Only()*/)
			{
				velocity.Y = Jump_Strength;
			}/* else if(IsOnWall())
			{
				velocity.Y = Jump_Strength;
				velocity.X = GetWallNormal().X;
				velocity.Z = GetWallNormal().Z;
				velocity = velocity.Normalized() * Jump_Strength;
			}*/
			isJumping = false;
		}
		if(!IsOnFloor()) {
			if(!UpDownEnabled)
			velocity.Y += Gravity * (float)delta * GravityScale;
		}
		else {
			if(velocity.Y < 0) {
				velocity.Y = 0;
			}
		}


		CamRotation = cam.GlobalRotation;
		
		Velocity = velocity;
		MoveAndSlide();
		if(IsOnFloor()) CameraBounce(0.1f, Velocity.Length() * 0.05f + 0.01f, (float)delta);


		Player_Transform = GlobalTransform;
		Player_GlobalPosition = GlobalPosition;
		Player2D_GlobalPosition = new Vector2(GlobalPosition.X, GlobalPosition.Z);

		if(previous_velocity.Length() < 0.25f && Velocity.Length() > 0.25f)
		{
			SetLocomotion(true);
		} else if(previous_velocity.Length() > 0.25f && Velocity.Length() < 0.25f)
		{
			SetLocomotion(false);
		}
	}
	public override void _Input(InputEvent @e) {
		if(@e is InputEventMouseMotion mm) {
			if(!OS.HasFeature("mobile"))
			{
				RotateY(-mm.Relative.X * sensitivity);

				x_rotation += -mm.Relative.Y * sensitivity;
				x_rotation = (float) Math.Clamp(x_rotation, Mathf.DegToRad(-89), Mathf.DegToRad(89));

				//PlayerGeometry.RotateY(-mm.Relative.X * sensitivity);
				Vector3 rot = cam.Rotation;
				rot.Z = 0;
				rot.X = x_rotation;

				cam.Rotation = rot;
			}
		}
		if(@e.IsActionPressed("Jump") && !@e.IsEcho() && CanJump()) {
			if(!UpDownEnabled)
			isJumping = true;
			//PlayJumpAnimation();
		}
		//GD.Print(GlobalPosition.Y);
		if(@e.IsActionPressed("Up"))
		{
			if(UpDownEnabled)
			GlobalPosition += Vector3.Up;
		}
		if(@e.IsActionPressed("Down"))
		{
			if(UpDownEnabled)
			GlobalPosition -= Vector3.Up;
		}
		/*if(@e.IsActionPressed("Dash") && !@e.IsEcho())
		{
			if(!IsOnFloor() && !IsOnWall()) return;
			Dash_Current = Dash_Speed;
			IsDashing = true;
		}*/
		/*if(@e.IsActionPressed("UI_Toggle")) {
			GameUI.Visible = (GameUI.Visible) ? false : true;
		}
		if(@e.IsActionPressed("Perspective_Change", false))
		{

			GetViewport().SetInputAsHandled();
		}*/
	}
	private void CameraBounce(float Amp, float Freq, float delta) {
		Vector3 offset = new Vector3((float)Math.Abs(0.5 * Math.Cos(GameManager.FRAMES * Freq)), (float) Math.Sin(GameManager.FRAMES * Freq), 0.0f) * Amp;
		cam.Position = cam.Position.Lerp(offset, delta * 10.0f) + Vector3.Up * DefaultPos.Y * 0.167f;
	}
	private void Move()
	{
		Vector3 movement = Vector3.Zero;

		if(Input.IsActionPressed("Sprint")) IsSprinting = true;
		if(Input.IsActionJustReleased("Sprint")) IsSprinting = false;

		WalkSpeed = (IsSprinting) ? BaseSpeed * Sprint : BaseSpeed;
		
		velocity = Velocity;
		velocity = new Vector3(0.0f, velocity.Y, 0.0f);

		Vector2 keys = Input.GetVector("Left", "Right", "Forward", "Backward");

		movement = cam.GlobalTransform.Basis * new Vector3(keys.X, 0, keys.Y);
//FIX THIS PART
		/*Vector2 rot = Input.GetVector("Camera_Right", "Camera_Left", "Camera_Up", "Camera_Down");
		if(Input.MouseMode == Input.MouseModeEnum.Captured && rot != Vector2.Zero)
		{
			Vector2 movepos = new Vector2((rot.X > 0.0f) ? 1.0f : -1.0f, (rot.Y > 0.0f) ? 1.0f : -1.0f);
			movepos *= 0.1f;
			movement += cam.GlobalTransform.Basis * new Vector3(movepos.X, 0, movepos.Y);
		} */
		movement.Y = 0;
		velocity += movement * WalkSpeed;
		//CameraBounce(0.05f, 0.1f);
	}
	private void RotateMobile(float delta)
	{
		Vector2 rot = Input.GetVector("Camera_Right", "Camera_Left", "Camera_Up", "Camera_Down");
		
			//GD.Print(rot);
		if(rot != Vector2.Zero)
		{
			RotateY(rot.X * joystick_sensitivity.Y * (float)delta);
			//PlayerGeometry.RotateY(rot.X * sensitivity);

			x_rotation += -rot.Y * joystick_sensitivity.X;
			x_rotation = (float) Math.Clamp(x_rotation, Mathf.DegToRad(-89), Mathf.DegToRad(89));

			Vector3 camrot = cam.Rotation;
			camrot.Z = 0;
			camrot.X = x_rotation;

			cam.Rotation = camrot;
		} 

	}
	private bool CanJump()
	{
		return IsOnFloor();// || IsOnWall();
	}
	private float CalculateMaxHeight()
	{
		return (Jump_Strength * Jump_Strength) / (2f*Mathf.Abs(Gravity));
	}
	private void PlayCameraDeathAnimation()
	{
		this.SetProcess(false);
		this.SetPhysicsProcess(false);
		this.SetProcessInput(false);

		Tween animation = cam.CreateTween();
		animation.SetPauseMode(Tween.TweenPauseMode.Process);
		animation.TweenProperty(cam, "position:y", 0.5f, 0.5f);
		animation.Parallel().TweenProperty(cam, "rotation:x", Mathf.Pi/2, 0.5f);
		animation.TweenMethod(Callable.From<float>((var) =>
		{
			cam.Position += new Vector3(0.0f, Mathf.Sin(var), Mathf.Cos(var)) * -cam.GlobalTransform.Basis.Z;
		}), 0.0f, Mathf.Pi/2, 1.0f);
	}
	private void PlayWinAnimation()
	{
		/*this.SetProcess(false);
		this.SetPhysicsProcess(false);
		this.SetProcessInput(false);*/
	}
	public void SpawnPlayer(Vector3 pos)
	{
		this.GlobalPosition = pos;
		GD.Print($"Spawing Player At: {this.GlobalPosition}");
	}
}
