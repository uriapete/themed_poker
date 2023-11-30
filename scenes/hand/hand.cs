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

    [Export] public UInt16 CardPositionHorizonalOffset { get; protected set; } = 160;

    [Export] public CanvasItem CardQueue { get; protected set; }

    [Export] public CanvasItem HandContainer { get; protected set; }

    [Signal] public delegate void CardsInQueueEventHandler();

    protected bool ProcessingQueue { get; set; } = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetNode<Sprite2D>("HandSprite").QueueFree();
        CardsInQueue += ProcessCardsInQueue;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (!ProcessingQueue && CardQueue.GetChildCount() > 0)
        {
            EmitSignal(SignalName.CardsInQueue);
        }
    }

    protected void ProcessCardsInQueue()
    {
        ProcessingQueue=true;
        foreach (var card in CardQueue.GetChildren())
        {
            if(card is not BaseCard)
            {
                card.QueueFree();
                continue;
            }
            BaseCard newCard = (BaseCard)card;
            MoveCardToHand(newCard, HandContainer.GetChildCount(), Revealed ? BaseCard.Sides.front: BaseCard.Sides.back);
        }
        ProcessingQueue = false;
    }

    protected void MoveCardToHand(BaseCard card, int idxPosition, BaseCard.Sides flipToSide=BaseCard.Sides.back)
    {
        card.Reparent(HandContainer);
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(card, "position", new Vector2(Position.X + (idxPosition * CardPositionHorizonalOffset),Position.Y), CardMoveTime);

        switch (flipToSide)
        {
            case BaseCard.Sides.front:
                card.Flip(BaseCard.Sides.front); break;
            case BaseCard.Sides.back:
                card.Flip(BaseCard.Sides.back); break;
        }
    }
}
