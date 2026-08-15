using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class CopyLicenseText : RichTextLabel
{
	[Export(PropertyHint.Dir)] public Godot.Collections.Array<string> License_Files = new();

	List<string> FileContents = new();
	List<string> FileNames = new();

	string Name_Separator = "\n";
	string File_Separator = "\n\n";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		foreach(string path in License_Files)
		{
			using var File = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
			GD.Print($"Found License File {path}");
			FileContents.Add(File.GetAsText());
			FileNames.Add($"[font_size=36]{path.GetFile().GetBaseName()}[/font_size]");
		}
		string[] JoinedFiles = FileNames.Zip(FileContents, (name, contents) => $"{name}{Name_Separator}{contents}").ToArray();
		string final = string.Join(File_Separator, JoinedFiles);
		this.Text = final;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
