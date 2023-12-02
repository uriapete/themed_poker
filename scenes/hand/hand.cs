using Godot;
using System;

public partial class hand : Node2D
{
    // value which controls whether entire hand is revealed
    [Export] public BaseCard.Sides Side { get; protected set; } = BaseCard.Sides.back;

    // value whichcontrols wheter cards are selectable
    [Export] public bool Selectable { get; protected set; } = false;

    // value for limit of cards that can be in a hand
    [Export] public UInt16 CardLimit { get; protected set; } = 5;

    // value which controls how long cards take to move into position when they first enter hand
    [Export] public float CardMoveTime { get; protected set; } = 0.5f;

    // value for difference of horizontal positions between cards
    [Export] public UInt16 CardPositionHorizonalOffset { get; protected set; } = 160;

    // node that holds card queue
    [Export] public CanvasItem CardQueue { get; protected set; }

    // node that holds cards in hand
    [Export] public CanvasItem HandContainer { get; protected set; }

    // signal to indicate cards in queue
    [Signal] public delegate void CardsInQueueEventHandler();

    protected bool ProcessingQueue { get; set; } = false;

    // getter for amt of cards in hand
    public int CardCount
    {
        get
        {
            return HandContainer.GetChildCount();
        }
    }

    // Called when the node enters the scene tree for the first time.
    /// <summary>
    /// delete the place holder sprite
    /// connect signal CardsInQueue to method ProcessCardsInQueue
    /// </summary>
    public override void _Ready()
    {
        GetNode<Sprite2D>("HandSprite").QueueFree();
        CardsInQueue += ProcessCardsInQueue;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // if queue is not being processed and there are cards in queue,
        // emit signal that cards are in queue
        if (!ProcessingQueue && CardQueue.GetChildCount() > 0)
        {
            EmitSignal(SignalName.CardsInQueue);
        }
    }

    // method to flip all cards
    /// <summary>
    /// for each card in the hand, flip it to the provided side
    /// </summary>
    /// <param name="side"></param>
    public void FlipAll(BaseCard.Sides side)
    {
        Side = side;
        foreach (BaseCard card in HandContainer.GetChildren())
        {
            card.Flip(side);
        }
    }

    //method to add a card to queue
    /// <summary>
    /// if hand is full, do nothing
    /// else, move the card to queue
    /// </summary>
    /// <param name="card"></param>
    public void AddCardToQueue(BaseCard card)
    {
        if (CardCount >= CardLimit)
        {
            return;
        }
        card.Reparent(CardQueue);
    }

    // method to process cards in queue
    /// <summary>
    /// for each card:
    ///     delete if node is not a card node or hand is full
    ///     else, move card to hand
    /// </summary>
    protected void ProcessCardsInQueue()
    {
        ProcessingQueue = true;
        foreach (var card in CardQueue.GetChildren())
        {
            if ((card is not BaseCard) || (CardCount >= CardLimit))
            {
                card.QueueFree();
                continue;
            }
            BaseCard newCard = (BaseCard)card;
            MoveCardToHand(newCard, HandContainer.GetChildCount(), Side);
        }
        ProcessingQueue = false;
    }

    //method for moving the card to hand
    /// <summary>
    /// reparent node to hand
    /// animate moving card to proper position
    /// flip card to provided side arg
    /// </summary>
    /// <param name="card"></param>
    /// <param name="idxPosition"></param>
    /// <param name="flipToSide"></param>
    protected void MoveCardToHand(BaseCard card, int idxPosition, BaseCard.Sides flipToSide = BaseCard.Sides.back)
    {
        card.Reparent(HandContainer);
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(card, "position", new Vector2(Position.X + (idxPosition * CardPositionHorizonalOffset), Position.Y), CardMoveTime);

        switch (flipToSide)
        {
            case BaseCard.Sides.front:
                card.Flip(BaseCard.Sides.front); break;
            case BaseCard.Sides.back:
                card.Flip(BaseCard.Sides.back); break;
        }
    }
}
