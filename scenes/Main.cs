using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
    [Export] public PackedScene CardScene { get;private set; }
    [Export] public Node2D CardStack { get; private set; }
    [Export] public UInt16 NumberOfValues { get; private set; } = 6;
    [Export] public UInt16 CardsPerValue { get; private set; } = 5;
    [Export] public Hand HouseHandNode { get;private set; }
    [Export] public Hand PlayerHandNode { get; private set; }
    [Export] public float DealCardDelay { get; private set; } = .1f;
    [Export] public BaseButton DrawHoldButton { get; private set; }

    public List<BaseCard> Cards { get; private set; }
    public List<BaseCard> CardPile {  get; private set; }
    public Random Rand { get; private set; }

    public bool PlayerSelectionsEnabled
    {
        get
        {
            return PlayerHandNode.Selectable && DrawHoldButton.Visible && !DrawHoldButton.Disabled;
        }
        private set
        {
            PlayerHandNode.Selectable=value; DrawHoldButton.Visible=value; DrawHoldButton.Disabled=!value;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Rand = new Random(DateTime.Now.Millisecond);
        base._Ready();
        Cards = new List<BaseCard>();
        for (int i = 0; i < NumberOfValues; i++)
        {
            for (int j = 0; j < CardsPerValue; j++)
            {
                BaseCard newCard=CardScene.Instantiate<BaseCard>();
                newCard.Value=i; Cards.Add(newCard);
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public async void NewGame()
    {
        NewPile();
        for (int i = 0; HouseHandNode.CardCount < HouseHandNode.CardLimit; i+=2)
        {
            BaseCard newHouseCard = CardPile[i];
            CardPile.Remove(newHouseCard);
            BaseCard newPlayerCard = CardPile[i+1];
            CardPile.Remove(newPlayerCard);

            newHouseCard.Position = Vector2.Zero;
            CardStack.AddChild(newHouseCard);
            HouseHandNode.MoveCardToHand(newHouseCard);

            newPlayerCard.Position = Vector2.Zero;
            CardStack.AddChild(newPlayerCard) ;
            PlayerHandNode.MoveCardToHand(newPlayerCard);

            await ToSignal(GetTree().CreateTimer(DealCardDelay, false), SceneTreeTimer.SignalName.Timeout);
        }
        PlayerSelectionsEnabled = true;
    }

    public void RemoveAllCardsInPlay()
    {
        PlayerSelectionsEnabled = false;
        PlayerHandNode.RemoveAllCards();
        HouseHandNode.RemoveAllCards();
    }

    public void ShufflePile()
    {
        for (int i = CardPile.Count - 1; i > 0; i--)
        {
            BaseCard currCard = CardPile[i];
            int swapWithIdx = Rand.Next(0, i);
            CardPile[i] = CardPile[swapWithIdx];
            CardPile[swapWithIdx] = currCard;
        }
    }

    public void NewPile()
    {
        RemoveAllCardsInPlay();
        CardPile=new List<BaseCard>(Cards);
        ShufflePile();
    }
}
