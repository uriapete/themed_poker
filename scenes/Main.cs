using Godot;
using System;

public partial class Main : Node2D
{
    [Export] public PackedScene CardScene { get;private set; }
    [Export] public Node2D CardStack { get; private set; }
    [Export] public UInt16 NumberOfValues { get; private set; } = 6;
    [Export] public UInt16 CardsPerValue { get; private set; } = 5;
    [Export] public Hand HouseHandNode { get;private set; }
    [Export] public Hand PlayerHandNode { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
