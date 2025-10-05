using Godot;
using System;

public partial class Blender : StaticBody2D
{

	public override void _Process(double delta)
	{
		Rotation += (float)delta;
	}
}
