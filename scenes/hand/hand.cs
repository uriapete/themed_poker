using Godot;
using System;

public partial class hand : Node2D
{
    // value which controls whether entire hand is revealed
    [Export] public bool Revealed { get; protected set; } = false;

    // value whichcontrols wheter cards are selectable
    [Export] public bool Selectable { get; protected set; } = false;

    // value which controls how long cards take to move into position when they first enter hand
    [Export] public float CardMoveTime { get; protected set; } = 0.5f;

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
    public void OnCardEnteredHand(Node node)
    {

    }
}
