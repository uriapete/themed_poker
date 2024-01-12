using Godot;
using System;

public partial class RankLabelParent : Control
{
    /// <summary>
    /// Label that displays House's Rank.
    /// </summary>
    [Export]Label HouseRankLabel { get; set; }

    /// <summary>
    /// Label that displays Player's Rank.
    /// </summary>
    [Export]Label PlayerRankLabel { get; set; }
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
