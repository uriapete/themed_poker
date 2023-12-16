using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
    //the summaries apparently only work before all [directives]
    //including before [export] and [exportgroup], regardless of how the lines are formatted

    /// <summary>
    /// The PackedScene which contains the scene we will use for cards.
    /// Should be a BaseCard.
    /// </summary>
    [ExportGroup("Packed Scenes")]
    [Export] public PackedScene CardScene { get; private set; }

    /// <summary>
    /// BaseButton that will be used to execute the game.
    /// </summary>
    [ExportGroup("Nodes")]
    [Export] public BaseButton DrawHoldButton { get; private set; }

    /// <summary>
    /// Node to use for the spawn location for new cards.
    /// </summary>
    [Export] public Node2D CardStack { get; private set; }

    /// <summary>
    /// Hand node belonging to the House (AI).
    /// </summary>
    [ExportSubgroup("Hands")]
    [Export] public Hand HouseHandNode { get; private set; }

    /// <summary>
    /// Hand node belonging to the player.
    /// </summary>
    [Export] public Hand PlayerHandNode { get; private set; }

    /// <summary>
    /// Delay in seconds between card deals.
    /// </summary>
    [ExportGroup("Game Settings")]
    [Export] public float DealCardDelay { get; private set; } = .1f;

    /// <summary>
    /// Total number of card values.
    /// </summary>
    [ExportSubgroup("Deck Settings")]
    [Export] public UInt16 NumberOfValues { get; private set; } = 6;

    /// <summary>
    /// Number of cards per value.
    /// </summary>
    [Export] public UInt16 CardsPerValue { get; private set; } = 5;

    /// <summary>
    /// Base, organized array of cards.
    /// </summary>
    public BaseCard[] Cards { get; private set; }

    /// <summary>
    /// List of cards used in the stack to spawn from.
    /// </summary>
    public List<BaseCard> CardPile { get; private set; }

    /// <summary>
    /// Random generator.
    /// </summary>
    public Random Rand { get; private set; }

    /// <summary>
    /// Getter and Setter for whether or now selections for player are enabled.
    /// Depends on PlayerHandNode.Selectable AND DrawHoldButton.Visible AND !DrawHoldButton.Disabled.
    /// </summary>
    public bool PlayerSelectionsEnabled
    {
        get
        {
            return PlayerHandNode.Selectable && DrawHoldButton.Visible && !DrawHoldButton.Disabled;
        }
        private set
        {
            PlayerHandNode.Selectable = value; DrawHoldButton.Visible = value; DrawHoldButton.Disabled = !value;
        }
    }

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// </summary>
    public override void _Ready()
    {
        //set random to now.
        Rand = new Random(DateTime.Now.Millisecond);

        //setup Cards
        Cards = new BaseCard[NumberOfValues * CardsPerValue];
        for (int i = 0; i < NumberOfValues; i++)
        {
            for (int j = 0; j < CardsPerValue; j++)
            {
                BaseCard newCard = CardScene.Instantiate<BaseCard>();
                newCard.Value = i; Cards[(i * CardsPerValue) + j] = newCard;
            }
        }

        //after all is ready, connect pressed signal of the Draw/Hold btn to game.exe
        DrawHoldButton.Pressed += ExecuteGame;
    }

    /// <summary>
    /// Called every frame. 'delta' is the elapsed time since the previous frame.
    /// </summary>
    /// <param name="delta"></param>
    public override void _Process(double delta)
    {
    }

    /// <summary>
    /// Method that executes the game.
    /// </summary>
    public void ExecuteGame()
    {
        DrawSelectedCards(PlayerHandNode);
    }

    public void AutoSelectCards(Hand hand, int preserveOverValue = 2)
    {
        //do counts for all values
        //save all values with >=2 cards
        //also save all values with value >=preserveOverValue
        //UNLESS there is at least cardcount-1 of a kind

        //count lists
        List<int> values = new List<int>();
        List<int> valueCounts = new List<int>();

        int almostAllInAKind = -1;

        //for each card in hand...
        foreach (BaseCard card in hand.HandContainer.GetChildren())
        {
            if (almostAllInAKind > -1) { break; }
            bool valueCounted = false;

            //if value is already part of list of values, increase it's count
            for (int i = 0; i < values.Count && !valueCounted && almostAllInAKind < 0; i++)
            {
                int value = values[i];
                if (value == card.Value)
                {
                    valueCounts[i]++;
                    valueCounted = true;
                    if (valueCounts[i] >= hand.CardCount - 1)
                    {
                        almostAllInAKind = value;
                    }
                    break;
                }
    }
            //otherwise, add new value and count
            if (!valueCounted)
            {
                values.Add(card.Value);
                valueCounts.Add(1);
            }
        }

        //if at least nearly all cards are one value, get the odd one out
        //then return
        if (almostAllInAKind > -1)
        {
            foreach (BaseCard card in hand.HandContainer.GetChildren())
            {
                if (card.Value != almostAllInAKind)
                {
                    hand.SelectCard(card);
                }
            }
            return;
        }

        //select cards that are only one of a kind
        for (int i = 0; i < values.Count; i++)
        {
            if (valueCounts[i] >= 2 || values[i]>=preserveOverValue)
            {
                continue;
            }
            int value = values[i];
            foreach (BaseCard card in hand.HandContainer.GetChildren())
            {
                if (card.Value == value)
                {
                    hand.SelectCard(card);
                }
            }
        }
    }

    /// <summary>
    /// Method that takes out selected cards from hand and adds in new cards.
    /// </summary>
    /// <param name="hand"></param>
    public async void DrawSelectedCards(Hand hand)
    {
        //remove all selected cards from hand
        //and add them to the pile
        while (hand.SelectedCards.Count > 0)
        {
            BaseCard card = hand.SelectedCards[0];
            CardPile.Add(hand.RemoveCard(card));
        }

        //then, fill the hand back up
        while (hand.CardCount < hand.CardLimit)
        {
        await ToSignal(GetTree().CreateTimer(DealCardDelay, false), SceneTreeTimer.SignalName.Timeout);
            hand.MoveCardToHand(SpawnCardInStack());
        }
    }

    /// <summary>
    /// Method that spawns in new cards from the stack.
    /// </summary>
    /// <returns></returns>
    public BaseCard SpawnCardInStack()
    {
        //take a card from top of pile
        //remove it from the list and add it as child of stack

        BaseCard newCard = CardPile[0];
        CardPile.Remove(newCard);

        //reset positon
        newCard.Position = Vector2.Zero;

        CardStack.AddChild(newCard);
        return newCard;
    }

    /// <summary>
    /// Method that sets up a new game.
    /// </summary>
    public async void NewGame()
    {
        //reset pile
        //fill up all hands
        //enable selections

        NewPile();
        for (int i = 0; HouseHandNode.CardCount < HouseHandNode.CardLimit; i += 2)
        {
            HouseHandNode.MoveCardToHand(SpawnCardInStack());
            PlayerHandNode.MoveCardToHand(SpawnCardInStack());

            await ToSignal(GetTree().CreateTimer(DealCardDelay, false), SceneTreeTimer.SignalName.Timeout);
        }
        PlayerSelectionsEnabled = true;
    }

    /// <summary>
    /// Method that removes all cards from all hands.
    /// </summary>
    public void RemoveAllCardsInPlay()
    {
        PlayerSelectionsEnabled = false;
        PlayerHandNode.RemoveAllCards();
        HouseHandNode.RemoveAllCards();
    }

    /// <summary>
    /// Method that shuffles CardPile.
    /// </summary>
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

    /// <summary>
    /// Method that resets CardPile.
    /// </summary>
    public void NewPile()
    {
        RemoveAllCardsInPlay();
        CardPile = new List<BaseCard>(Cards);
        ShufflePile();
    }
}
