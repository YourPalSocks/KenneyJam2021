using Godot;
using System;

public class CameraFollow : Camera2D
{
    private Node2D target;

    public override void _Ready()
    {
        target = GetTree().Root.GetNode<Node2D>("Root/Player");
    }

    public override void _Process(float delta)
    {
        Position = target.Position;
    }
}
