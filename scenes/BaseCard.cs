using Godot;

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

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // on card ready, write the proper value of the card to the label
        // remember, the label is temporary, once we confirm all the basic logic is working, we can replace the label with actual images
        Label valueLabel = GetNode<Label>("ValueLabel");
        valueLabel.Text = this.Value.ToString();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
