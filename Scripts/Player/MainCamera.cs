using Godot;
using System;

public partial class MainCamera : Camera3D
{
	private Node3D PlayerGeometry;
	private Skeleton3D PlayerSkeleton;
	private string BoneName = "mixamorig_Head";
	private int BoneIndex;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PlayerGeometry = GetNode<Node3D>("%Player_Model");
		PlayerGeometry.Visible = true;

		PlayerSkeleton = FindSkeleton(PlayerGeometry);

		BoneIndex = PlayerSkeleton.FindBone(BoneName);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//Vector3 BonePos = PlayerSkeleton.ToGlobal(PlayerSkeleton.GetBoneGlobalPose(BoneIndex).Origin);
		//GlobalPosition = BonePos;
		//GD.Print(this.GlobalPosition);
	}
	private Skeleton3D FindSkeleton(Node root)
	{
		if(root is Skeleton3D) return root as Skeleton3D;
		else
		{
			foreach(Node child in root.GetChildren())
			{
				Node result = FindSkeleton(child);
				if(result is Skeleton3D skeleton) return skeleton;
			}
		}
		return null;
	}
}
