using Godot;
using GodotCookies;
using System;

public partial class CopyLegalNotice : Panel
{
	[Export(PropertyHint.File)] public string PrivacyPolicy_File = "";
	[Export(PropertyHint.File)] public string EULA_File = "";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Visible = !Cookies.User.Get<bool>("TermsAccepted", false);

		RichTextLabel PrivacyPolicy = GetNode<RichTextLabel>("%PrivacyPolicyDisplay");
		RichTextLabel EULA = GetNode<RichTextLabel>("%EULADisplay");
		using var PrivacyPolicyFile = Godot.FileAccess.Open(PrivacyPolicy_File, Godot.FileAccess.ModeFlags.Read);
		using var EULAFile = Godot.FileAccess.Open(EULA_File, Godot.FileAccess.ModeFlags.Read);
		PrivacyPolicy.Text = PrivacyPolicyFile.GetAsText();
		EULA.Text = EULAFile.GetAsText();

		Button agreeButton = GetNode<Button>("%AgreeButton");
		agreeButton.Pressed += () =>
		{
			Cookies.User.Set("TermsAccepted", true);
			this.Visible = false;
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
