using Godot;
using System;

public partial class Hand : Node2D
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

    // node that holds cards in hand
    [Export] public CanvasItem HandContainer { get; protected set; }


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
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
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

    //method for moving the card to hand
    //overloaded method
    //overload 1:
    /// <summary>
    /// if hand is full, do nothing
    /// else:
    /// reparent node to hand
    /// animate moving card to proper position
    /// flip card to current side
    /// </summary>
    /// <param name="card"></param>
    public bool MoveCardToHand(BaseCard card)
    {
        if (CardCount >= CardLimit)
        {
            return false;
        }
        int newIdxPos = CardCount;
        card.Reparent(HandContainer);
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(card, "position", new Vector2(Position.X + (newIdxPos * CardPositionHorizonalOffset), Position.Y), CardMoveTime);

        switch (Side)
        {
            case BaseCard.Sides.front:
                card.Flip(BaseCard.Sides.front); break;
            case BaseCard.Sides.back:
                card.Flip(BaseCard.Sides.back); break;
        }
        return true;
    }
    /// <summary>
    /// if hand is full, do nothing
    /// else:
    /// reparent node to hand
    /// animate moving card to proper position
    /// flip card to provided side arg
    /// </summary>
    /// <param name="card"></param>
    /// <param name="flipToSide"></param>
    /// <param name="idxPosition"></param>
    /// <returns></returns>
    public bool MoveCardToHand(BaseCard card, BaseCard.Sides flipToSide, int idxPosition)
    {
        if(CardCount >= CardLimit)
        {
            return false;
        }
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
        return true;
    }
}