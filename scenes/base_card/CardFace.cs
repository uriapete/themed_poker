using Godot;
using System;

public partial class CardFace : Node2D
{
    [Export]public Sprite2D FaceSprite { get;private set; }
    [Export]public Godot.Collections.Array<ValueLabel> Labels { get; private set; }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
