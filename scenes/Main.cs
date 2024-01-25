using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    /// Node that will handle displaying hand ranks.
    /// </summary>
    [Export] public RankLabelParent HandRanksDisplay { get; private set; }

    /// <summary>
    /// Label that will announce the winner.
    /// </summary>
    [Export] public Label WinnerLabel { get; private set; }

    /// <summary>
    /// Hand node belonging to the House (Computer Player).
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
    [Export(PropertyHint.Range, "0,1,or_greater")] public float DealCardDelay { get; private set; } = .1f;

    /// <summary>
    /// Duration of the animation for discarding cards.
    /// </summary>
    [Export(PropertyHint.Range, "0,2,or_greater")] public float DiscardAnimDuration { get; private set; } = .5f;

    /// <summary>
    /// The discarding card animation makes the cards go up and off the screen. This value is how much the cards will be displaced from their hands.
    /// </summary>
    [Export] public float DiscardTargetYPos { get; private set; } = 1080;

    /// <summary>
    /// The delay in seconds between the hands sorting and the hands flipping over.
    /// </summary>
    [Export(PropertyHint.Range, "0,2,or_greater")] public float SortFlipHandDelay { get; private set; } = .5f;

    /// <summary>
    /// Delay in seconds between hand values being revealed and the winner being announced.
    /// </summary>
    [Export(PropertyHint.Range, "0,3,or_greater")] public float WinnerAnnounceDelay { get; private set; } = 1f;

    /// <summary>
    /// String to display on the WinnerLabel when the player wins.
    /// </summary>
    [Export] public string PlayerWinString { get; private set; } = "You Win!";

    /// <summary>
    /// String to display on the WinnerLabel when the house wins.
    /// </summary>
    [Export] public string HouseWinString { get; private set; } = "House Wins...";

    /// <summary>
    /// String to display on the WinnerLabel in the case of a Draw.
    /// </summary>
    [Export] public string TieString { get; private set; } = "Draw";

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
    /// Label to use as our debug menu.
    /// </summary>
    [ExportGroup("Debug")]
    [Export] public Label DebugMenuLabel { get; private set; }

    private int _DebugValue;
    /// <summary>
    /// Value to change a card to in debug mode.
    /// </summary>
    private int DebugValue
    {
        get
        {
            return _DebugValue;
        }
        set
        {
            _DebugValue = value;
            EmitSignal(SignalName.DebugUpdate);
        }
    }

    private int _DebugCardID;
    /// <summary>
    /// ID of card to change in debug mode.
    /// </summary>
    private int DebugCardID
    {
        get { return _DebugCardID; }
        set
        {
            _DebugCardID = value;
            EmitSignal(SignalName.DebugUpdate);
        }
    }

    private Hand _DebugHand;
    /// <summary>
    /// Hand to screw with in debug mode.
    /// </summary>
    private Hand DebugHand
    {
        get { return _DebugHand; }
        set
        {
            _DebugHand = value;
            EmitSignal(SignalName.DebugUpdate);
        }
    }

    /// <summary>
    /// Signal which signifies a debug value was changed.
    /// </summary>
    [Signal] public delegate void DebugUpdateEventHandler();

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

    //Called when the node enters the scene tree for the first time.
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

        WinnerLabel.Hide();
        WinnerLabel.Text = "";

        //after all is ready, connect pressed signal of the Draw/Hold btn to game.exe
        DrawHoldButton.Pressed += ExecuteGame;

        if (DebugMenuLabel.Visible)
        {
            DebugValue = 0;
            DebugCardID = 0;
            DebugHand = PlayerHandNode;
            DebugMenuLabel.Text = $"value: {DebugValue}\ncardid: {DebugCardID}\nhand: {DebugHand.Name}";
            DebugUpdate += UpdateDebugMenu;
        }
    }

    //Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (DebugMenuLabel.Visible)
        {
            if (Input.IsActionJustPressed("ui_accept"))
            {
                NewGame();
            }
            if (Input.IsActionJustPressed("ui_up"))
            {
                DebugValue++;
            }
            if (Input.IsActionJustPressed("ui_down"))
            {
                DebugValue--;
            }
            if (Input.IsActionJustPressed("ui_right"))
            {
                DebugCardID++;
            }
            if (Input.IsActionJustPressed("ui_left"))
            {
                DebugCardID--;
            }
            if (Input.IsActionJustPressed("ui_focus_next"))
            {
                if (DebugHand == PlayerHandNode)
                {
                    DebugHand = HouseHandNode;
                }
                else
                {
                    DebugHand = PlayerHandNode;
                }
            }
            if (Input.IsActionJustPressed("ui_home"))
            {
                BaseCard debuggingCard = DebugHand.HandContainer.GetChild<BaseCard>(DebugCardID);
                debuggingCard.Value = DebugValue;
                debuggingCard.ValueDisplayNode.Call("RenderValue", debuggingCard.Value);
            }
        }
    }

    /// <summary>
    /// Method that executes the game.
    /// </summary>
    public async void ExecuteGame()
    {
        //disable player selections
        PlayerSelectionsEnabled = false;

        //draw selected cards of player
        Tween playerLastTween = await DrawSelectedCards(PlayerHandNode);

        //wait for it to finish animating
        if (playerLastTween != null && playerLastTween.IsRunning())
        {
            await ToSignal(playerLastTween, Tween.SignalName.Finished);
        }

        //small delay
        await ToSignal(GetTree().CreateTimer(DealCardDelay, false), SceneTreeTimer.SignalName.Timeout);

        //computer selects house cards
        HouseHandNode.AutoSelectCards(NumberOfValues - 2);

        //draw selected cards of house
        Tween houseLastTween = await DrawSelectedCards(HouseHandNode);

        //wait for finish animation
        if (houseLastTween != null && houseLastTween.IsRunning())
        {
            await ToSignal(houseLastTween, Tween.SignalName.Finished);
        }

        //small delay
        await ToSignal(GetTree().CreateTimer(DealCardDelay, false), SceneTreeTimer.SignalName.Timeout);

        //sort both hands
        HouseHandNode.SortHand();
        Tween playerSortLastTween=PlayerHandNode.SortHand();

        if (playerSortLastTween.IsRunning())
        {
            await ToSignal(playerSortLastTween, Tween.SignalName.Finished);
        }

        //small delay for anticipation
        await ToSignal(GetTree().CreateTimer(SortFlipHandDelay, false), SceneTreeTimer.SignalName.Timeout);

        //get rank infos
        Hand.SimpleHandValue HouseHandValue = HouseHandNode.GetHandValue(NumberOfValues - 1);
        Hand.SimpleHandValue PlayerHandValue = PlayerHandNode.GetHandValue(NumberOfValues - 1);

        //flip all
        HouseHandNode.FlipAll(BaseCard.Sides.front);

        //display ranks
        Tween handRankDisplayTween = HandRanksDisplay.DisplayRanks(Hand.GetSimpleHandRankStr(HouseHandValue.Rank), Hand.GetSimpleHandRankStr(PlayerHandValue.Rank));

        //if player and house have same rank, make deciding values blink
        if
            (
                HouseHandValue.Rank == PlayerHandValue.Rank
                ||
                (HouseHandValue.Rank == Hand.SimpleHandRank.Flush && PlayerHandValue.Rank == Hand.SimpleHandRank.FiveInAKind)
                ||
                (HouseHandValue.Rank == Hand.SimpleHandRank.FiveInAKind && PlayerHandValue.Rank == Hand.SimpleHandRank.Flush)
            )
        {
            for (int i = 0; i < HouseHandValue.Values.Length; i++)
            {
                int houseValue = HouseHandValue.Values[i];
                int playerValue = PlayerHandValue.Values[i];
                if (houseValue != playerValue)
                {
                    HouseHandNode.BlinkCardValuesOf(houseValue);
                    PlayerHandNode.BlinkCardValuesOf(playerValue);
                    break;
                }
            }
        }

        //get winning value
        Hand.SimpleHandValue winningValue = Hand.SimpleHandValue.WinningHandValue(HouseHandValue, PlayerHandValue);

        //change winner announcing text accordingly
        if (winningValue == HouseHandValue)
        {
            WinnerLabel.Text = HouseWinString;
        }
        else if (winningValue == PlayerHandValue)
        {
            WinnerLabel.Text = PlayerWinString;
        }
        else
        {
            WinnerLabel.Text = TieString;
        }

        //wait for handrankdisplay to finish
        if (handRankDisplayTween.IsRunning())
        {
            await ToSignal(handRankDisplayTween, Tween.SignalName.Finished);
        }

        //small delay for anticipation
        await ToSignal(GetTree().CreateTimer(WinnerAnnounceDelay, false), SceneTreeTimer.SignalName.Timeout);

        WinnerLabel.Show();
    }

    /// <summary>
    /// Method that takes out selected cards from hand and adds in new cards.
    /// Returns the Tween that is animating moving the final card.
    /// </summary>
    /// <param name="hand">Hand to take out and fill from.</param>
    public async Task<Tween> DrawSelectedCards(Hand hand)
    {
        //remove all selected cards from hand
        //and add them to the pile

        if (hand.SelectedCards.Count <= 0)
        {
            return null;
        }

        hand.RepositionOnHandOrderChanged = false;

        //array for selected cards
        BaseCard[] selectedCards = hand.SelectedCards.ToArray();
        int[] selectedCardsIdxs = new int[selectedCards.Length];

        //tween
        Tween finalMoveTween = GetTree().CreateTween();

        //all cards will come up at the same time
        finalMoveTween.SetParallel(true);

        //move each selected card off the screen
        //foreach (BaseCard card in selectedCards)
        for (int i = selectedCards.Length - 1; i >= 0; i--)
        {
            BaseCard card = selectedCards[i];
            selectedCardsIdxs[i] = card.GetIndex();
            finalMoveTween.TweenProperty(card, "position", new Vector2(card.Position.X, card.Position.Y - DiscardTargetYPos), DiscardAnimDuration);
        }

        //wait for cards to move off screen
        if (finalMoveTween.IsRunning())
        {
            await ToSignal(finalMoveTween, Tween.SignalName.Finished);
        }

        //remove out cards
        foreach (BaseCard card in selectedCards)
        {
            hand.RemoveCard(card);
            CardPile.Add(card);
        }

        //reset tween
        finalMoveTween = null;

        //then, fill the hand back up
        //return latest tween
        for (int i = 0; hand.CardCount < hand.CardLimit; i++)
        {
            await ToSignal(GetTree().CreateTimer(DealCardDelay, false), SceneTreeTimer.SignalName.Timeout);
            finalMoveTween = hand.MoveCardToHand(SpawnCardInStack(), i < selectedCards.Length ? selectedCardsIdxs[i] : -1);
        }
        hand.RepositionOnHandOrderChanged = true;
        return finalMoveTween;
    }

    /// <summary>
    /// Method that spawns in new cards from the stack.
    /// </summary>
    /// <returns>The new card node/scene.</returns>
    public BaseCard SpawnCardInStack()
    {
        //take a card from top of pile
        //remove it from the list and add it as child of stack

        BaseCard newCard = CardPile[0];
        CardPile.Remove(newCard);

        //reset positon
        newCard.Position = Vector2.Zero;

        //set card side
        newCard.QuickFlip(BaseCard.Sides.back);

        //spawn card ingame
        CardStack.AddChild(newCard);

        //return card
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

        //reset the pile
        NewPile();

        //hide the winner text if any
        WinnerLabel.Hide();
        WinnerLabel.Text = "";

        //hide the hand rank displays
        HandRanksDisplay.MoveDisplaysOffScreen();

        //flip househandnode back to backside
        HouseHandNode.FlipAll(BaseCard.Sides.back);

        //prevent reposition of cards
        HouseHandNode.SetRepositionOnHandOrderChanged(false);
        PlayerHandNode.SetRepositionOnHandOrderChanged(false);

        //deal cards
        while (HouseHandNode.CardCount < HouseHandNode.CardLimit)
        {
            HouseHandNode.MoveCardToHand(SpawnCardInStack());
            PlayerHandNode.MoveCardToHand(SpawnCardInStack());

            await ToSignal(GetTree().CreateTimer(DealCardDelay, false), SceneTreeTimer.SignalName.Timeout);
        }

        //allow card reposition again
        HouseHandNode.SetRepositionOnHandOrderChanged(true);
        PlayerHandNode.SetRepositionOnHandOrderChanged(true);

        //allow player to select their cards
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

    public void UpdateDebugMenu()
    {
        DebugMenuLabel.Text = $"value: {DebugValue}\ncardid: {DebugCardID}\nhand: {DebugHand.Name}";
    }
}
