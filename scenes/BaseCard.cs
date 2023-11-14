using Godot;
using System;

public partial class BaseCard : Area2D
{
    [Export] public int Value
    {
        get;
        protected set;
    } = 0;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
