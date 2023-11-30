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

    [Export] public CanvasItem CardQueue { get; protected set; }

    [Export] public CanvasItem HandContainer { get; protected set; }

    [Signal] public delegate void CardsInQueueEventHandler();

    protected bool ProcessingQueue { get; set; } = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetNode<Sprite2D>("HandSprite").QueueFree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    // Cards will come into the hand as children of this node. We must:
    // Move the cards to hand's position
    // Flip if necessary
    public int OnCardEnteredHand(Node node)
    {
        if(node is not BaseCard)
    {
            node.Free();
            return 1;
        }

        BaseCard newCard=(BaseCard)node;

        Tween tween = GetTree().CreateTween().SetParallel();
        tween.TweenProperty(newCard,"position",Position,CardMoveTime);

        switch (Revealed)
        {
            case true:
                newCard.Flip(BaseCard.Sides.front); break;
            case false:
                newCard.Flip(BaseCard.Sides.back); break;
        }
        return 0;
    }
}
