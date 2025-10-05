using Godot;
using System;

public partial class Ball : RigidBody2D
{
    public Battery Host;
    [Export] public Label ValueLabel;
    [Export] public MeshInstance2D MeshInstance;
    public System.Numerics.BigInteger value = 10;
    public override void _Ready()
    {
        MeshInstance.Modulate = Host.Color;
        LinearVelocity = Vector2.Up.Rotated(GD.Randf()) * 500;
        ValueLabel.Text = value.ToString();
        if (value > 9999999999)
            value = 9999999999;
    }

}
// using Godot;
// using System;

// public partial class Ball : RigidBody2D
// {
//     public Battery Host;
//     [Export]public Label ValueLabel;
//     [Export]public MeshInstance2D MeshInstance;
//     public int value=1;

//     public override void _Process(double delta)
//     {
//         if (Engine.GetPhysicsFrames() % 60 == 0)
//         {
//             if (LinearVelocity.LengthSquared() <= 400 && LinearVelocity.Y > 0)
//             {
//                 ApplyCentralForce(Vector2.Up * 10000);
//             }
//         }
//     }
//     public override void _PhysicsProcess(double delta)
//     {
//         if (ToRebirth)
//         {
//             ToRebirth = false;
//             Rebirth();
//         }
//     }

//     public bool ToRebirth = false;
//     public void Rebirth()
//     {
//         MeshInstance.Modulate = Host.Color;

//         GlobalPosition = ((Main)GetTree().CurrentScene).BirthPos.GlobalPosition;
//         // LinearVelocity = Vector2.Up.Rotated(GD.Randf()) * 100;
//     }
// }