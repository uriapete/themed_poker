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

    public List<BaseCard> Cards { get; private set; }=new List<BaseCard>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
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
}
