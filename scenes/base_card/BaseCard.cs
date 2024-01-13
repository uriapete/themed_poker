using Godot;
using System;

// BaseCard script, meant as a basic template for displayed cards
// this will probs be inherited
public partial class BaseCard : Area2D
{
    /// <summary>
    /// The card's numerical value.
    /// </summary>
    [Export]
    public int Value
    {
        get;
        set;
    }

    /// <summary>
    /// How long in seconds it takes for the card to flip over.
    /// </summary>
    [Export] public float FlipDuration { get; protected set; } = 0.2f;

    /// <summary>
    /// Node that serves as the parent to all visual nodes for the card.
    /// </summary>
    [ExportCategory("Node Paths")]
    [Export]public Node2D DisplayParent { get; protected set; }

    /// <summary>
    /// AnimatedSprite that is used to show the base of the card as well as control what side it's flipped to via its Animation setting.
    /// </summary>
    [Export]public AnimatedSprite2D Sprite { get; protected set; }

    /// <summary>
    /// The node that displays the card's value.
    /// </summary>
    [Export] public CanvasItem ValueDisplayNode { get; protected set; }

    /// <summary>
    /// Timer used to control delay between visibility state changes while blinking.
    /// </summary>
    [Export]public Timer BlinkingTimer { get; protected set; }

    /// <summary>
    /// Boolean that signifies if the mouse is inside the card node area.
    /// Changed via connected signal methods OnClickEntered/Exited.
    /// </summary>
    protected bool MouseInside { get; set; }

    /// <summary>
    /// Signal emitted when this card is clicked on.
    /// </summary>
    /// <param name="card"></param>
    [Signal] public delegate void ClickEventHandler(BaseCard card);

    public bool Blinking { get
        {
            return !BlinkingTimer.IsStopped();
        }
        set
        {
            if(value)
            {
                BlinkingTimer.Timeout += ToggleBlink;
                BlinkingTimer.Start();
            }
            else
            {
                BlinkingTimer.Timeout -= ToggleBlink;
                DisplayParent.Show();
                BlinkingTimer.Stop();
            }
        } 
    }

    protected void ToggleBlink()
    {
        DisplayParent.Visible=!DisplayParent.Visible;
    }

    //enum for strong typing sides
    //all lower case to match animation names, which follow gdscript naming conventions
    //are signed for easy flipping
    /// <summary>
    /// An Enum for Sides.
    /// Named in lower case to match animation names, which follow GDScript naming conventions.
    /// Uses -1 and 1 for easy flipping.
    /// </summary>
    public enum Sides
    {
        front = -1,
        back = 1,
    }

    /// <summary>
    /// Boolean that signifies if card is currently flipping or not.
    /// </summary>
    protected bool Flipping = false;

    /// <summary>
    /// Easy getter and setter for the card's current side.
    /// On get, parses the card's animatedSprite's current animation (for the card base) for the current side.
    /// On set, converts the Side enum to a srting for the animation name.
    /// </summary>
    public Sides CurrentSide
    {
        get
        {
            //parse the string anim value into sides
            _ = Enum.TryParse(Sprite.Animation, out Sides result);

            //then result
            return result;
        }

        //we don't want just anything setting the anim, so protected it is
        protected set
        {
            //set the value, stringifying the enum value
            Sprite.Animation = value.ToString();

            //hide or show face value sprite depending on current side
            switch (value)
            {
                case Sides.front:
                    ValueDisplayNode.Show();
                    break;
                case Sides.back:
                    ValueDisplayNode.Hide();
                    break;
            }
        }
    }

    /// <summary>
    /// Method to quickly flip this card to the other side without tweening animations.
    /// </summary>
    public void QuickFlip()
    {
        QuickFlip((Sides)(-1 * (int)CurrentSide));
    }
    /// <summary>
    /// Method to quickly flip this card to a side without tweening animations.
    /// </summary>
    /// <param name="sides"></param>
    public void QuickFlip(Sides side)
    {
        if (Flipping)
        {
            return;
        }
        CurrentSide = side;
    }

    /// <summary>
    /// Public function for flipping the card.
    /// </summary>
    /// <param name="side"></param>
    public void Flip(Sides side)
    {
        if (side == CurrentSide)
        {
            return;
        }
        BeginFlip();
        return;
    }

    /// <summary>
    /// Protected method to start a flip.
    /// </summary>
    protected void BeginFlip()
    {
        //set this bool so in case to stop something from running during flip
        Flipping = true;

        //create a tween for the animation
        Tween tween = GetTree().CreateTween();

        //make this node x-scale to 0
        tween.TweenProperty(this, "scale", Vector2.Down, FlipDuration / 2);

        //run FinishFlip when done
        tween.Finished += FinishFlip;
    }

    /// <summary>
    /// Protected method to finish a flip.
    /// Should ONLY be connected to BeginFlip, DO NOT CALL.
    /// </summary>
    protected async void FinishFlip()
    {
        //switch current side
        CurrentSide = (Sides)(-1 * (int)CurrentSide);

        //create the next tween
        Tween tween = GetTree().CreateTween();

        //scale the node back to normal
        tween.TweenProperty(this, "scale", Vector2.One, FlipDuration / 2);

        //after the tween is done, reset Flipping to completely finish
        await ToSignal(tween, Tween.SignalName.Finished);
        Flipping = false;
    }

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// </summary>
    public override void _Ready()
    {
        //it is up to the individual node script of ValueDisplayNode to have their value render fn implemented locally
        ValueDisplayNode.Call("RenderValue", Value);
        Flipping = false;
    }

    /// <summary>
    /// Called every frame. 'delta' is the elapsed time since the previous frame.
    /// </summary>
    /// <param name="delta"></param>
    public override void _Process(double delta)
    {
    }

    /// <summary>
    /// Method to handle MouseInside bool when the mouse enters the area.
    /// </summary>
    protected void OnMouseEntered()
    {
        MouseInside = true;
    }
    /// <summary>
    /// Method to handle MouseInside bool when the mouse exits the area.
    /// </summary>
    protected void OnMouseExited()
    {
        MouseInside = false;
    }

    public override void _Input(InputEvent @event)
    {
        // catches mouse inputs
        // emits click signal when mouse is inside
        if (@event is InputEventMouseButton)
        {
            InputEventMouseButton e = @event as InputEventMouseButton;
            if (e.ButtonIndex == MouseButton.Left && e.Pressed && MouseInside)
            {
                EmitSignal(SignalName.Click, this);
            }
        }
    }
}
