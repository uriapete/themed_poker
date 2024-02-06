using Godot;
using System;

public partial class LicensesScreen : CanvasLayer
{
    [Export]public RichTextLabel LicenseTextDisplay { get;private set; }

    public PackedScene MainMenuScene
    {
        get
        {
            return Global.CurrentInstance.MainMenuScene;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void BackToMainMenu()
    {
        GetTree().ChangeSceneToPacked(MainMenuScene);
    }
}
