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

    //how long it takes for a complete flip in secs
    [Export] public float FlipDuration { get; protected set; } = 0.2f;

    //node to use for the card's front side
    [Export] public CanvasItem ValueDisplayNode { get; protected set; }

    //enum for strong typing sides
    //all lower case to match animation names, which follow gdscript naming conventions
    //are signed for easy flipping
    public enum Sides
    {
        front = -1,
        back = 1,
    }

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
    //when starting flip, you MUST call this!
    protected void BeginFlip()
    {
        //set this bool so in case to stop something from running during flip
        Flipping = true;

        //create a tween for the animation
        Tween tween = GetTree().CreateTween();

        //make this node x-scale to 0
        tween.TweenProperty(this, "scale", Vector2.Down, FlipDuration / 2);

        //run FinishFlip when done
        tween.Connect("finished", new Callable(this, MethodName.FinishFlip));
    }

    //fn after first flip tween is done
    //this fn SHOULD NOT BE CALLED BY ANYTHING OTHER THAN BEGINFLIP'S TWEEN!!!
    protected void FinishFlip()
    {
        //switch current side
        CurrentSide = (Sides)(-1 * (int)CurrentSide);

        //hide or show face value sprite depending on current side
        switch (CurrentSide)
        {
            case Sides.front:
                ValueDisplayNode.Show();
                break;
            case Sides.back:
                ValueDisplayNode.Hide();
                break;
        }

        //create the next tween
        Tween tween = GetTree().CreateTween();

        //scale the node back to normal
        tween.TweenProperty(this, "scale", Vector2.One, FlipDuration / 2);

        //now run CompleteFlip once done
        tween.Connect("finished", new Callable(this, MethodName.CompleteFlip));
    }

    //fn to complete the flip
    //DO NOT CALL! ONLY THE TWEEN CREATED IN FINISHFLIP SHOULD CALL THIS!!!
    protected void CompleteFlip()
    {
        Flipping = false;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // on card ready, write the proper value of the card to the label
        // remember, the label is temporary, once we confirm all the basic logic is working, we can replace the label with actual images
        Label valueLabel = GetNode<Label>("ValueLabel");
        valueLabel.Text = Value.ToString();
        Flipping = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        //this code is for testing
        if (Input.IsActionPressed("ui_accept") && !Flipping)
        {
            BeginFlip();
        }
    }
}
