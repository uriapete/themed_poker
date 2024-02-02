using Godot;
using System;

public partial class Global : Node
{
    [Export]public PackedScene MainMenuScene { get;private set; }
    [Export]public PackedScene LicensesScene { get;private set; }
    [Export]public PackedScene MainGameScene { get;private set; }

    /// <summary>
    /// Tree Path to the Global Autoload.
    /// </summary>
    public static string TreePath { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TreePath = GetPath();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
