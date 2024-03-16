using Godot;
using System;

public partial class CardFace : Node2D
{
    [ExportCategory("Face Sprite")]
    [Export]public AnimatedSprite2D FaceSprite { get;private set; }
    [Export]public Godot.Collections.Dictionary<int,string> ValueFaceNames { get; private set; }
    [ExportCategory("Value Labels")]
    [Export]public Godot.Collections.Array<Label> Labels { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
