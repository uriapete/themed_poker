using Godot;
using System;

public partial class hand : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetNode<Sprite2D>("HandSprite").Free();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    // Cards will come into the hand as children of this node. We must:
    // Move the cards to hand's position
    // Flip if necessary
    public void OnChildEnteredTree(Node node)
    {

    }
}
