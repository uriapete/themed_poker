using Godot;
using System;

public partial class MainMenu : Control
{
    /// <summary>
    /// Scene to change to upon play.
    /// </summary>
    [Export]public PackedScene MainPlayScene { get;private set; }

    /// <summary>
    /// Scene containing 3rd Party License info.
    /// </summary>
    [Export]public PackedScene LicensesScene { get;private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    /// <summary>
    /// Function to change the main menu to the gameplay scene.
    /// </summary>
    public void Play()
    {
        GetTree().ChangeSceneToPacked(MainPlayScene);
    }

    /// <summary>
    /// Changes the scene to the Licenses screen.
    /// </summary>
    public void ToLicenses()
    {
        GetTree().ChangeSceneToPacked(LicensesScene);
    }
}
