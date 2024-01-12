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
    
    /// <summary>
    /// Offset between the end of the screen and the labels when set offscreen.
    /// </summary>
    [Export]float OffScreenRestOffset {  get; set; }

    /// <summary>
    /// How long the animation of the ranks sliding on screen will last in seconds.
    /// </summary>
    [Export(PropertyHint.Range,"0,5,or_greater")]
    float DisplayAnimationDuration { get; set; } = 0.5f;
    
    /// <summary>
    /// Property that controls the visibility of both HouseRankLabel and PlayerRankLabel.
    /// </summary>
    public bool RankLabelsVisible
    {
        get
        {
            return HouseRankLabel.Visible&&PlayerRankLabel.Visible;
        }
        set
        {
            HouseRankLabel.Visible = value;
            PlayerRankLabel.Visible = value;
        }
    }

    /// <summary>
    /// Property holding the X Position of both labels on screen. Should be set to their position as set on editor on _Ready.
    /// </summary>
    public float OnScreenXPosition { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        OnScreenXPosition = (HouseRankLabel.Position.X+PlayerRankLabel.Position.X)/2;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    /// <summary>
    /// Hides rank displays and puts off-screen.
    /// </summary>
    public void MoveDisplaysOffScreen()
    {
    }

    /// <summary>
    /// Animates ranks siding on screen.
    /// </summary>
    /// <param name="HouseRank">The rank of the House's hand, as a string.</param>
    /// <param name="PlayerRank">The rank of the Player's hand, as a string.</param>
    public void DisplayRanks(string HouseRank,string PlayerRank)
    {
        HouseRankLabel.Text = HouseRank;
        PlayerRankLabel.Text = PlayerRank;

        RankLabelsVisible=true;

        Tween tween = GetTree().CreateTween().SetParallel();

        tween.TweenProperty(HouseRankLabel, "position", new Vector2(OnScreenXPosition, HouseRankLabel.Position.Y),DisplayAnimationDuration);
        tween.TweenProperty(PlayerRankLabel, "position", new Vector2(OnScreenXPosition, PlayerRankLabel.Position.Y),DisplayAnimationDuration);
    }
}
