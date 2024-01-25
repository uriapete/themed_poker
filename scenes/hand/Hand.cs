using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Hand : Node2D
{
    //the summaries apparently only work before all [directives]
    //including before [export] and [exportgroup],etc

    //static vars on top for easier access
    /// <summary>
    /// Array containing names for the simple list of hand ranks.
    /// </summary>
    public static string[] SimpleHandRankNames { get; protected set; } = { "Junk", "One Pair", "Two Pairs", "Three Of a Kind", "Full House", "Four Of a Kind", "Five In a Kind", "Flush" };

    /// <summary>
    /// Value for the limit of cards that can be in a hand.
    /// </summary>
    [ExportCategory("Hand Settings")]
    [Export] public UInt16 CardLimit { get; protected set; } = 5;

    /// <summary>
    /// Value that controls how long cards take to move into the hand and into position.
    /// </summary>
    [Export] public float CardMoveTime { get; protected set; } = 0.5f;

    /// <summary>
    /// Value that controls how long cards take to move when being sorted.
    /// </summary>
    [Export] public float CardSortTime { get; protected set; } = .25f;

    /// <summary>
    /// Value that controls how long cards take to move when being selected.
    /// </summary>
    [Export] public float CardSelectTime { get; protected set; } = .125f;

    /// <summary>
    /// Node that holds the cards.
    /// </summary>
    [ExportGroup("Nodes")]
    [Export] public CanvasItem HandContainer { get; protected set; }

    /// <summary>
    /// Value for the difference of horizontal positions between cards.
    /// </summary>
    [ExportGroup("Card Positioning")]
    [Export] public UInt16 CardPositionHorizonalOffset { get; protected set; } = 160;

    /// <summary>
    /// Value for the vertical offset for selected cards.
    /// </summary>
    [Export] public float SelectedCardVerticalOffset { get; protected set; } = 120;

    /// <summary>
    /// Value for which side the entire hand is.
    /// </summary>
    [ExportCategory("Current Hand State")]
    [Export] public BaseCard.Sides Side { get; protected set; } = BaseCard.Sides.back;

    /// <summary>
    /// Value for whether cards are selectable or not.
    /// </summary>
    [Export] public bool Selectable { get; set; } = false;

    /// <summary>
    /// List for cards that are selected.
    /// </summary>
    public List<BaseCard> SelectedCards { get; protected set; }

    /// <summary>
    /// Property that controls whether or not cards in HandContainer reposition when the ChildOrder changes.
    /// </summary>
    public bool RepositionOnHandOrderChanged { get; set; } = true;

    /// <summary>
    /// Getter for how many cards are in this hand.
    /// </summary>
    public int CardCount
    {
        get
        {
            return HandContainer.GetChildCount();
        }
    }

    /// <summary>
    /// Enum for simple hand ranks.
    /// </summary>
    public enum SimpleHandRank
    {
        Junk,
        OnePair,
        TwoPair,
        ThreeInAKind,
        FullHouse,
        FourInAKind,
        FiveInAKind,
        Flush,
    }

    public class SimpleHandValue
    {
        public SimpleHandRank Rank { get; protected set; }
        public int[] Values { get; protected set; } = Array.Empty<int>();

        public SimpleHandValue(SimpleHandRank rank = SimpleHandRank.Junk, int[] values = null)
        {
            Rank = rank;
            if (values != null)
            {
                Values = values;
            }
        }

#nullable enable
        public static SimpleHandValue? WinningHandValue(SimpleHandValue value1, SimpleHandValue value2)
        {
            if (value1.Rank > value2.Rank)
            {
                return value1;
            }
            else if (value1.Rank < value2.Rank)
            {
                return value2;
            }

            for (int i = 0; i < value1.Values.Length; i++)
            {
                int currVal1 = value1.Values[i];
                int currVal2 = value2.Values[i];
                if (currVal1 == currVal2)
                {
                    continue;
                }
                if (currVal1 > currVal2)
                {
                    return value1;
                }
                if (currVal2 > currVal1)
                {
                    return value2;
                }
            }
            return null;
        }
#nullable disable
    }

    /// <summary>
    /// Gets appropriate simple rank string from it's enum.
    /// Necessary as enums have naming restrictions that do not allow ranks to be fully expressed.
    /// </summary>
    /// <param name="rank">Rank to get string for.</param>
    /// <returns>String of the given rank.</returns>
    public static string GetSimpleHandRankStr(SimpleHandRank rank)
    {
        return SimpleHandRankNames[(int)rank];
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //free placeholder sprite
        GetNode<Sprite2D>("HandSprite").QueueFree();

        //init selected cards list
        SelectedCards = new List<BaseCard>();

        //connect HandContainer.ChildOrderChanged signal to appropriate method
        HandContainer.ChildOrderChanged += OnHandOrderChanged;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    /// <summary>
    /// Class that puts together info about the value counts of the hands.
    /// </summary>
    public class HandValuesCount
    {
        /// <summary>
        /// Counts of card values.
        /// </summary>
        public Dictionary<int, int> ValueCounts { get; private set; }

        /// <summary>
        /// List of values with only one occurrance in the hand.
        /// </summary>
        public List<int> OneOffValues { get; private set; } = new();

        /// <summary>
        /// The value which makes up all or all but one of the cards. -1 if not applicable.
        /// </summary>
        public int AllButOneOrAllInAKind { get; private set; } = -1;

        public HandValuesCount(Dictionary<int, int> valueCounts, int allButOneOrAllInAKind = -1, List<int> oneOffValues = null)
        {
            ValueCounts = valueCounts;
            AllButOneOrAllInAKind = allButOneOrAllInAKind;

            if (oneOffValues != null)
            {
                OneOffValues = oneOffValues;
            }

        }
    }

    /// <summary>
    /// Method that counts the amount of cards of each value in card.
    /// </summary>
    /// <returns>Value counts in the form of a dictionary(card value as key, count as value) and the value that makes up all or all but one of the cards if applicable or -1 if not.</returns>
    public HandValuesCount CountHandValues()
    {
        //for each card in hand, increase count of value
        //also track if all or all but one of the cards are one value
        //and track values with only once occurrance

        Dictionary<int, int> valueCounts = new();

        int allButOneOrAllInAKind = -1;

        List<int> oneOffValues = new();

        foreach (BaseCard card in HandContainer.GetChildren().Cast<BaseCard>())
        {
            if (!valueCounts.ContainsKey(card.Value))
            {
                valueCounts.Add(card.Value, 1);
                oneOffValues.Add(card.Value);
            }
            else
            {
                valueCounts[card.Value]++;
                oneOffValues.Remove(card.Value);
            }
            if (valueCounts[card.Value] >= CardCount - 1)
            {
                allButOneOrAllInAKind = card.Value;
            }
        }
        return new HandValuesCount(valueCounts, allButOneOrAllInAKind, oneOffValues);
    }

    /// <summary>
    /// Method that evaluates the hand.
    /// Checks for card ranks in order of rank values (from Flush to Junk)
    /// </summary>
    /// <returns>Simple hand rank and values of interest.</returns>
    public SimpleHandValue GetHandValue(int maxValue = 5)
    {
        ///hand evaluation priority:
        ///flush or 5kind
        ///4kind
        ///full house (3kind&2kind)
        ///3kind
        ///2pair
        ///1pair

        //basically the same order as ranks

        //get value counts
        HandValuesCount handValuesCountInfo = CountHandValues();
        Dictionary<int, int> valueCounts = handValuesCountInfo.ValueCounts;

        ///first checking for 5kind
        ///checks if there is a card value that makes up the whole hand
        ///AND there is only one value in the counts
        if (valueCounts.ContainsValue(CardLimit) && valueCounts.Count == 1)
        {
            //getting 5kind value
            int[] rankValue = { valueCounts.Keys.ToArray()[0] };

            ///how the rank value ? operater works:
            ///if the 5kind value is max then flush
            ///otherwise, reg 5kind
            return new(rankValue[0] == maxValue ? SimpleHandRank.Flush : SimpleHandRank.FiveInAKind, rankValue);
        }

        ///now checking for 4kind
        ///if all but one is one value
        if (handValuesCountInfo.AllButOneOrAllInAKind > -1)
        {
            //var flr value of 4kind
            int[] rankValue = new int[1];

            ///checking each valuecount
            ///for each valuecount, check if 4kind then set rankvalue
            foreach (var (value, count) in valueCounts)
            {
                if (count >= 4)
                {
                    rankValue[0] = value;
                    break;
                }
            }
            return new(SimpleHandRank.FourInAKind, rankValue);
        }

        //now using amount of matched up values
        ///NOTE: all five cards are matched up in both 5kinds and full houses, and 4 cards are matched up in both 4kinds and 2pairs
        ///that's why 5kinds and 4kinds aren't checked here in this switch
        switch (CardLimit - handValuesCountInfo.OneOffValues.Count)
        {
            //5 matched up cards -> full house
            case 5:
                {
                    //values
                    int[] rankValues = new int[2];

                    //foreach valuecount
                    foreach (var (value, count) in valueCounts)
                    {
                        //if valuecount makes up more than half the hand
                        //then set it to the first value
                        if (count > CardLimit / 2)
                        {
                            rankValues[0] = value;
                        }
                        //otherwise, set it to the second value
                        else
                        {
                            rankValues[1] = value;
                        }
                    }
                    return new(SimpleHandRank.FullHouse, rankValues);
                }

            //3 matched up cards -> 3kind
            case 3:
                {
                    //value
                    int[] rankValue = new int[1];

                    //foreach valuecount
                    foreach (var (value, count) in valueCounts)
                    {
                        ///if value count is 3, set the value
                        if (count == 3)
                        {
                            rankValue[0] = value;
                            break;
                        }
                    }
                    return new(SimpleHandRank.ThreeInAKind, rankValue);
                }

            //4 matched up cards -> 2pair
            case 4:
                {
                    //pairs list
                    List<int> accountedPairs = new();

                    //while still counting pairs
                    while (accountedPairs.Count < 2)
                    {
                        //paired values need to be accounted for in order of value

                        int highestValue = -1;
                        foreach (var (value, count) in valueCounts)
                        {
                            //skip already accounted pairs
                            if (accountedPairs.Contains(value))
                            {
                                continue;
                            }

                            //skip non pair values
                            if (count < 2)
                            {
                                continue;
                            }

                            //if first value, set and skip
                            if (highestValue < 0)
                            {
                                highestValue = value;
                                continue;
                            }

                            ///if curr count is higher than HV
                            ///OR 
                            if (value > highestValue)
                            {
                                highestValue = value;
                                continue;
                            }
                        }

                        //account value
                        accountedPairs.Add(highestValue);
                    }
                    return new(SimpleHandRank.TwoPair, accountedPairs.ToArray());
                }

            //2 matched cards -> 1pair
            case 2:
                {
                    //pair value
                    int[] pair = new int[1];

                    //foreach valuecount
                    foreach (var (value, count) in valueCounts)
                    {
                        //if this is the pair, set pair value and break
                        if (count >= 2)
                        {
                            pair[0] = value;
                            break;
                        }
                    }
                    return new(SimpleHandRank.OnePair, pair);
                }
            default:
                break;
        }

        //if none of the above tests returned, we got junk lol
        return new();
    }

    /// <summary>
    /// Sorts the hand based on hand value (By amount of cards of value, then by value)
    /// </summary>
    public Tween SortHand()
    {
        SetRepositionOnHandOrderChanged(false);

        //count all values
        //sort cards by value counts, then value itself

        //count all values

        HandValuesCount valuesCount = CountHandValues();
        Dictionary<int, int> valueCounts = valuesCount.ValueCounts;
        int countedValues = valueCounts.Count;

        //sort values
        List<int> sortedValueList = new();

        while (sortedValueList.Count < countedValues)
        {
            int valueMostCount = -1;
            //for (int value = 0; value < numberOfValues; value++)
            foreach (var (value, count) in valueCounts)
            {
                //first set valueMostCount to first counted value
                if (valueMostCount < 0)
                {
                    if (count > 0)
                    {
                        valueMostCount = value;
                    }
                    continue;
                }

                //reset valueMostCount to current value if current value has higher count, or if curr val has same count AND is a higher value
                if (count < 1)
                {
                    continue;
                }
                if (count > valueCounts[valueMostCount] || (count == valueCounts[valueMostCount] && value > valueMostCount))
                {
                    valueMostCount = value;
                }
            }
            //add to list
            sortedValueList.Add(valueMostCount);
            valueCounts[valueMostCount] = -1;
        }

        //for each value in the sorted list, move unsorted cards of that list to the end
        int unsortedItems = CardCount;
        foreach (var value in sortedValueList)
        {
            for (int cardIdx = 0; cardIdx < unsortedItems;)
            {
                BaseCard card = HandContainer.GetChild(cardIdx) as BaseCard;
                if (card.Value == value)
                {
                    HandContainer.MoveChild(card, -1);
                    unsortedItems--;
                }
                else
                {
                    cardIdx++;
                }
            }
        }

        return SetRepositionOnHandOrderChanged(true);
    }

    /// <summary>
    /// Method for handling when the Child Order of HandContainer changes.
    /// </summary>
    public void OnHandOrderChanged()
    {
        if (!RepositionOnHandOrderChanged)
        {
            return;
        }
        RepositionAllCards();
    }

    /// <summary>
    /// Public setter method for RepositionOnHandOrderChanged.
    /// </summary>
    /// <param name="value"></param>
    public Tween SetRepositionOnHandOrderChanged(bool value)
    {
        RepositionOnHandOrderChanged = value;
        if (value)
        {
            return RepositionAllCards();
        }
        return null;
    }

    /// <summary>
    /// Flips all cards over.
    /// </summary>
    public void FlipAll()
    {
        FlipAll((BaseCard.Sides)(-1 * (int)Side));
    }
    /// <summary>
    /// Flips all cards to provided side.
    /// </summary>
    /// <param name="side">Side to flip cards to.</param>
    public void FlipAll(BaseCard.Sides side)
    {
        Side = side;
        foreach (BaseCard card in HandContainer.GetChildren().Cast<BaseCard>())
        {
            card.Flip(side);
        }
    }

    /// <summary>
    /// Either adds or removes selected card to/from the selected list, and positions it accordingly.
    /// </summary>
    /// <param name="card">Card to select.</param>
    public Tween SelectCard(BaseCard card)
    {
        bool removed = SelectedCards.Remove(card);
        float newYPos = 0;
        if (!removed)
        {
            if (
                SelectedCards.Count <= 0
                ||
                SelectedCards[^1].GetIndex() < card.GetIndex()
            )
            {
                SelectedCards.Add(card);
            }
            else
            {
                for (int i = 0; i < SelectedCards.Count; i++)
                {
                    int cardIdx = SelectedCards[i].GetIndex();
                    if (card.GetIndex() < cardIdx)
                    {
                        SelectedCards.Insert(i, card);
                        break;
                    }
                }
            }
            newYPos = card.Position.Y - SelectedCardVerticalOffset;
        }
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(card, "position", new Vector2(card.Position.X, newYPos),CardSelectTime);
        return tween;
    }

    /// <summary>
    /// Method for the computer to select cards.
    /// </summary>
    /// <param name="hand">Hand to select cards from.</param>
    /// <param name="preserveOverValue">Minimum value to hold cards regardless of only being one of a kind.</param>
    public void AutoSelectCards(int preserveOverValue = 2)
    {
        //do counts for all values
        //save all values with >=2 cards
        //also save all values with value >=preserveOverValue
        //UNLESS there is exactly 1 oneoff value

        HandValuesCount valueCountInfo = CountHandValues();

        //count array
        Dictionary<int, int> valueCounts = valueCountInfo.ValueCounts;

        List<int> oneOffVals = valueCountInfo.OneOffValues;

        if (valueCounts.Count == 1)
        {
            return;
        }

        //if there is only one oneoff, get the odd one out
        //then return
        if (oneOffVals.Count == 1)
        {
            foreach (BaseCard card in HandContainer.GetChildren().Cast<BaseCard>())
            {
                if (card.Value == oneOffVals[0])
                {
                    SelectCard(card);
                    return;
                }
            }
            return;
        }

        //select cards that are only one of a kind AND less than int preserveOverValue
        foreach (BaseCard card in HandContainer.GetChildren().Cast<BaseCard>())
        {
            int value = card.Value;
            if (valueCounts[value] < 2 && value < preserveOverValue)
            {
                SelectCard(card);
            }
        }
    }

    /// <summary>
    /// Selects card on click if hand is selectable.
    /// </summary>
    /// <param name="card">Card that was clicked on.</param>
    public void OnCardClick(BaseCard card)
    {
        if (!Selectable)
        {
            return;
        }
        SelectCard(card);
    }

#nullable enable
    /// <summary>
    /// Moves card to this hand.
    /// Returns the Tween that is animating moving the card if successful.
    /// </summary>
    /// <param name="card">Card to move to this hand.</param>
    /// <param name="newIdx">Optional: New index to move the new card to. Disabled (-1) by default.</param>
    /// <returns>The Tween that animates the card moving to this hand, or null if it fails.</returns>
    public Tween? MoveCardToHand(BaseCard card, int newIdx = -1)
    {
        //if hand is full, don't do anything
        if (CardCount >= CardLimit)
        {
            return null;
        }

        //else:
        //reparent the card to handcontainer
        card.Reparent(HandContainer);
        if (newIdx > -1)
        {
            HandContainer.MoveChild(card, newIdx);
        }

        //connect card's click signal to click handler
        card.Click += OnCardClick;

        //animate card moving into position
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(card, "position", new Vector2(card.GetIndex() * CardPositionHorizonalOffset, 0), CardMoveTime);

        //flip card
        card.Flip(Side);

        //done
        return tween;
    }
#nullable disable

    /// <summary>
    /// Repositions all cards to correct positions. Called when HandContainer's Child Order is Changed.
    /// </summary>
    /// <returns>The tween animating the last card to be animated.</returns>
    public Tween RepositionAllCards()
    {
        Tween lastTween = null;
        foreach (BaseCard card in HandContainer.GetChildren().Cast<BaseCard>())
        {
            lastTween=MoveCard(card, card.GetIndex());
        }
        return lastTween;
    }

    /// <summary>
    /// Positions card to provided index position.
    /// </summary>
    /// <param name="card">Card in hand to move.</param>
    /// <param name="newPos">Index position to move the card to.</param>
    /// <param name="tween">Optional: Tween to use to animate the card moving.</param>
    public Tween MoveCard(BaseCard card, int newPos, Tween tween = null)
    {
        if (card.GetParent() != HandContainer)
        {
            return null;
        }

        //create new tween if not supplied in args
        tween ??= GetTree().CreateTween();

        tween.TweenProperty(card, "position", new Vector2(newPos * CardPositionHorizonalOffset, card.Position.Y), CardSortTime);

        return tween;
    }

    /// <summary>
    /// Removes card from this hand (Not deleted, just removed from the hand's tree.). Also removes it from SelectedCards list, resets its position (if specified), and disconnects its click signal. Returns the removed card.
    /// </summary>
    /// <param name="card">Card to remove.</param>
    /// <param name="targetParent">Optional: The node to reparent the card to.</param>
    /// <returns>The removed card.</returns>
    public BaseCard RemoveCard(BaseCard card, Node targetParent = null)
    {
        card.Click -= OnCardClick;
        card.Blinking = false;
        SelectedCards.Remove(card);
        if (targetParent == null)
        {
            HandContainer.RemoveChild(card);
            card.Position = Vector2.Zero;
        }
        else
        {
            card.Reparent(targetParent);
        }
        return card;
    }

    /// <summary>
    /// Removes all cards.
    /// </summary>
    public void RemoveAllCards()
    {
        SetRepositionOnHandOrderChanged(false);
        foreach (BaseCard card in HandContainer.GetChildren().Cast<BaseCard>())
        {
            RemoveCard(card);
        }
        SetRepositionOnHandOrderChanged(true);
    }

    /// <summary>
    /// Makes cards of this value blink.
    /// </summary>
    /// <param name="value">Value to look in cards for to make blink.</param>
    public void BlinkCardValuesOf(int value)
    {
        foreach (BaseCard card in HandContainer.GetChildren().Cast<BaseCard>())
        {
            if (card.Value == value)
            {
                card.Blinking = true;
            }
        }
    }
}
