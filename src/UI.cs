using Godot;
using System;

public class UI : Control
{
    //Componenets
    private TextureRect face;
    private TextureRect blightBar;
    private TextureRect warningIcon;
    private AnimationPlayer anim;

    public override void _Ready()
    {
        //Get componenets
        face = GetNode<TextureRect>("Face");
        blightBar = GetNode<TextureRect>("Bar");
        warningIcon = GetNode<TextureRect>("Warning Icon");
        anim = GetNode<AnimationPlayer>("AnimationPlayer");

        anim.Stop();
        warningIcon.Visible = false;
    }
}
