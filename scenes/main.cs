using Godot;
using System;

public partial class main : Node2D
{
    [Export] public PackedScene CardScene { get;private set; }
    [Export] public Node2D HouseHandNode { get;private set; }
    [Export] public Node2D PlayerHandNode { get; private set; }

    public Vector2 CardStackPosition
    {
        get
        {
            Node2D cardStack = GetNode<Node2D>("CardStack");
            return cardStack.Position;
        }
    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
