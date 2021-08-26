using Godot;
using System;

public class TitleScreen : Node
{
    //Control
    private bool startingLoading;

    //Componenets
    private TextureRect blackScreen;

    public override void _Ready()
    {
        blackScreen = GetNode<TextureRect>("CanvasLayer/Black Screen");
        ((StatsObserver)GetTree().Root.GetNode<StatsObserver>("StatsObserver")).clear();
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("Interact") && !startingLoading)
        {
            startingLoading = true;
            //Load game scene
            blackScreen.Visible = true;
            if (blackScreen.Visible)
            {
                GetTree().ChangeScene("res://Scenes/GameScene.tscn");
            }
        }
    }
}
