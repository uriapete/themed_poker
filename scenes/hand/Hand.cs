using Godot;
using System;
using System.Collections.Generic;

public partial class Hand : Node2D
{
    //the summaries apparently only work before all [directives]
    //including before [export] and [exportgroup],etc

    /// <summary>
    /// Value for the limit of cards that can be in a hand.
    /// </summary>
    [ExportCategory("Hand Settings")]
    [Export] public UInt16 CardLimit { get; protected set; } = 5;

    /// <summary>
    /// Value that controls how long cards take to move into position.
    /// </summary>
    [Export] public float CardMoveTime { get; protected set; } = 0.5f;

    /// <summary>
    /// Array containing names for the simple list of hand ranks.
    /// </summary>
    [Export] public string[] SimpleHandRankNames { get; protected set; } = { "Junk", "One Pair", "Two Pair", "Three Of a Kind", "Full House", "Four Of a Kind", "Five In a Kind", "Flush" };

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
    public bool RepositionOnHandOrderChanged { get; protected set; } = true;

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

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// </summary>
    public override void _Ready()
    {
        //free placeholder sprite
        GetNode<Sprite2D>("HandSprite").QueueFree();

        //init selected cards list
        SelectedCards = new List<BaseCard>();

        //connect HandContainer.ChildOrderChanged signal to appropriate method
        HandContainer.ChildOrderChanged += OnHandOrderChanged;
    }

    /// <summary>
    /// Called every frame. 'delta' is the elapsed time since the previous frame.
    /// </summary>
    /// <param name="delta"></param>
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
        public Dictionary<int,int> ValueCounts { get; private set; }

        /// <summary>
        /// The value which makes up all or all but one of the cards. -1 if not applicable.
        /// </summary>
        public int AllButOneOrAllInAKind { get; private set; } = -1;

        public HandValuesCount(Dictionary<int,int> valueCounts, int allButOneOrAllInAKind = -1)
        {
            ValueCounts = valueCounts;
            AllButOneOrAllInAKind = allButOneOrAllInAKind;
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

        Dictionary<int, int> valueCounts = new();

        int allButOneOrAllInAKind = -1;

        foreach (BaseCard card in HandContainer.GetChildren())
        {
            if (!valueCounts.ContainsKey(card.Value))
            {
                valueCounts.Add(card.Value, 1);
            }
            else
            {
                valueCounts[card.Value]++;
            }
            if (valueCounts[card.Value] >= CardCount - 1)
            {
                allButOneOrAllInAKind = card.Value;
            }
        }
        return new HandValuesCount(valueCounts,allButOneOrAllInAKind);
    }

    /// <summary>
    /// Sorts the hand based on hand value (By amount of cards of value, then by value)
    /// </summary>
    /// <param name="numberOfValues"></param>
    public void SortHand(int numberOfValues)
    {
        SetRepositionOnHandOrderChanged(false);

        //count all values
        //sort cards by value counts, then value itself

        //count all values

        int[] valueCounts = new int[numberOfValues];

        ushort countedValues = 0;

        foreach (BaseCard card in HandContainer.GetChildren())
        {
            valueCounts[card.Value]++;
            if (valueCounts[card.Value] <= 1)
            {
                countedValues++;
            }
        }

        //sort values
        List<int> sortedValueList = new List<int>();

        while (sortedValueList.Count < countedValues)
        {
            int valueMostCount = -1;
            for (int value = 0; value < numberOfValues; value++)
            {
                //first set valueMostCount to first counted value
                if (valueMostCount < 0)
                {
                    if (valueCounts[value] > 0)
                    {
                        valueMostCount = value;
                    }
                    continue;
                }

                //reset valueMostCount to current value if current value has higher count, or if curr val has same count AND is a higher value
                int count = valueCounts[value];
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

        SetRepositionOnHandOrderChanged(true);
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
    public void SetRepositionOnHandOrderChanged(bool value)
    {
        RepositionOnHandOrderChanged = value;
        if (value)
        {
            RepositionAllCards();
        }
    }

    /// <summary>
    /// Flips all cards over.
    /// </summary>
    public void FlipAll()
    {
        Side = (BaseCard.Sides)(-1 * (int)Side);
        foreach (BaseCard card in HandContainer.GetChildren())
        {
            card.Flip(Side);
        }
    }
    /// <summary>
    /// Flips all cards to provided side.
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

    /// <summary>
    /// Either adds or removes selected card to/from the selected list, and positions it accordingly.
    /// </summary>
    /// <param name="card"></param>
    public void SelectCard(BaseCard card)
    {
        bool removed = SelectedCards.Remove(card);
        if (!removed)
        {
            SelectedCards.Add(card);
            card.Position = new Vector2(card.Position.X, card.Position.Y - SelectedCardVerticalOffset);
        }
        else
        {
            card.Position = new Vector2(card.Position.X, 0);
        }
    }

    /// <summary>
    /// Selects card on click if hand is selectable.
    /// </summary>
    /// <param name="card"></param>
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
    /// <param name="card"></param>
    public Tween? MoveCardToHand(BaseCard card)
    {
        //if hand is full, don't do anything
        if (CardCount >= CardLimit)
        {
            return null;
        }

        //else:
        //reparent the card to handcontainer
        card.Reparent(HandContainer);

        //connect card's click signal to click handler
        card.Click += OnCardClick;

        //animate card moving into position
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(card, "position", new Vector2((card.GetIndex() * CardPositionHorizonalOffset), 0), CardMoveTime);

        //flip card
        card.Flip(Side);

        //done
        return tween;
    }
#nullable disable

    /// <summary>
    /// Repositions all cards to correct positions. Called when HandContainer's Child Order is Changed.
    /// </summary>
    public void RepositionAllCards()
    {
        foreach (BaseCard card in HandContainer.GetChildren())
        {
            MoveCard(card, card.GetIndex());
        }
    }

    /// <summary>
    /// Positions card to provided index position.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="newPos"></param>
    public void MoveCard(BaseCard card, int newPos)
    {
        if (card.GetParent() != HandContainer)
        {
            return;
        }
        card.Position = new Vector2(newPos * CardPositionHorizonalOffset, card.Position.Y);

    }

    /// <summary>
    /// Removes card from this hand. Also removes it from SelectedCards list, resets its position, and disconnects its click signal. Returns the removed card.
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public BaseCard RemoveCard(BaseCard card)
    {
        card.Click -= OnCardClick;
        SelectedCards.Remove(card);
        HandContainer.RemoveChild(card);
        card.Position = Vector2.Zero;
        return card;
    }

    /// <summary>
    /// Removes all cards.
    /// </summary>
    public void RemoveAllCards()
    {
        foreach (BaseCard card in HandContainer.GetChildren())
        {
            RemoveCard(card);
        }
    }
}
