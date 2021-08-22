using Godot;
using System;

public class UI : Control
{
    //Componenets
    private TextureRect face;
    private TextureRect blightBar;
    private TextureRect warningIcon;
    private AnimationPlayer anim;
    private Control pauseMenu;

    //Component information
    private Texture[] faces = new Texture[4];
    private readonly int BAR_MAX_X = 168;
    private readonly int BAR_STEP = 12;
    private readonly float BAR_WARNING_RANGE = 0.30f;
    private bool canPause = true;

    public override void _Ready()
    {
        //Get componenets
        face = GetNode<TextureRect>("Face");
        blightBar = GetNode<TextureRect>("Bar");
        warningIcon = GetNode<TextureRect>("Warning Icon");
        anim = GetNode<AnimationPlayer>("AnimationPlayer");
        pauseMenu = GetNode<Control>("PauseMenu");
        pauseMenu.Visible = false;

        anim.Stop();
        warningIcon.Visible = false;

        //Get faces
        for (int i = 0; i < 4; i++)
        {
            string exp = "";
            switch (i)
            {
                case 0:
                    exp = "Sad";
                    break;

                case 1:
                    exp = "Angry";
                    break;

                case 2:
                    exp = "Smile";
                    break;

                case 3:
                    exp = "Joy";
                    break;
            }
            faces[i] = (Texture) ResourceLoader.Load("res://Assets/Sprites_Tilemaps/Face_" + exp + ".png");
        }
    }

    public override void _Process(float delta)
    {
        //Check for pausing
        if (Input.IsActionJustPressed("Pause") && canPause)
        {
            if (GetTree().Paused)
            {
                pauseMenu.Visible = false;
                GetTree().Paused = false;

            }
            else
            {
                pauseMenu.Visible = true;
                GetTree().Paused = true;
            }
        }

        if (GetTree().Paused && Input.IsActionJustPressed("Interact") && canPause)
        {
            GetTree().Paused = false;
            GetTree().ChangeScene("res://Scenes/Title Screen.tscn"); //Main Menu
        }
    }

    public void updateUI(int health)
    {
        if (!canPause)
        {
            return;
        }
        //Chance blight bar's rect based on health and step
        blightBar.RectSize = new Vector2(health * BAR_STEP, 40);
        //Adjust face based on percentage
        float healthPercent = (float) (health * BAR_STEP) / BAR_MAX_X;
        if (healthPercent <= 1.0f)
        {
            face.Texture = faces[3];
            if (healthPercent <= 0.82f)
            {
                face.Texture = faces[2];
                if (healthPercent <= 0.55f)
                {
                    face.Texture = faces[1];
                    if (healthPercent <= 0.25f)
                    {
                        face.Texture = faces[0];
                    }
                }
            }
        }
        //See if warning is needed
        if (healthPercent <= BAR_WARNING_RANGE)
        {
            warningIcon.Visible = true;
            anim.Play("Warning Icon");
        }
        else
        {
            anim.Stop();
            warningIcon.Visible = false;
        }
    }

    /*
     * This method deals with the nitty-gritty regarding moving to the End Screen
     * Information needed is stored in the StatsObserver and should be filled before this is called
     */
    public void goToEnding()
    {
        anim.Stop();
        anim.Play("Fade Out");
        GetTree().Paused = true;
        canPause = false;
    }


    /*
     * Actually move to the endscreen
     */
    public void _on_AnimationPlayer_animation_finished(string anim_name)
    {
        if (anim_name.Equals("Fade Out"))
        {
            GetTree().Paused = false;
            GetTree().ChangeScene("res://Scenes/EndScreen.tscn");
        }
    }
}
