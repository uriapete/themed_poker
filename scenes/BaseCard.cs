using Godot;
using System;

// BaseCard script, meant as a basic template for displayed cards
// this will probs be inherited
public partial class BaseCard : Area2D
{
    // referring to card value
    // public set but protected set just for memory safety
    // default: 0
    [Export]
    public int Value
    {
        get;
        protected set;
    } = 0;

    //enum for strong typing sides
    //all lower case to match animation names, which follow gdscript naming conventions
    //are signed for easy flipping
    public enum Sides
    {
        front = -1,
        back = 1,
    }

    //var for: what side are we flipping to?
    protected Sides TargetSide { get; set; }

    //var for: is card currently flipping?
    protected bool Flipping = false;

    //var for easy getting and setting current side without getting node
    public Sides CurrentSide
    {
        get
        {
            //get the node
            AnimatedSprite2D animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

            //parse the string anim value into sides
            _ = Enum.TryParse(animatedSprite2D.Animation, out Sides result);

            //then result
            return result;
        }

        //we don't want just anything setting the anim, so protected it is
        protected set
        {
            //get node
            AnimatedSprite2D animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

            //set the value, stringifying the enum value
            animatedSprite2D.Animation = value.ToString();
        }
    }

    //fn for flipping
    //protected for now, but may turn public if necessary
    protected void Flip()
    {
        TargetSide = (Sides)(-1 * (int)CurrentSide);
        Flipping = true;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // on card ready, write the proper value of the card to the label
        // remember, the label is temporary, once we confirm all the basic logic is working, we can replace the label with actual images
        Label valueLabel = GetNode<Label>("ValueLabel");
        valueLabel.Text = Value.ToString();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
