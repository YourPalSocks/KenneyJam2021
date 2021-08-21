using Godot;
using System;

public enum DOCTORS { Monroe, Sanders, Archer }
public class Player : KinematicBody2D
{
    //Controls
    [Export]
    private float speed = 150f;
    private Vector2 vel = Vector2.Zero;
    public int health = 14;

    //Componenets
    private Sprite exclamation;
    private Node2D pivot;
    private Sprite axe; //TODO: Axe and Gun classes
    private Sprite gun;

    public static bool inInteraction = false;
    public static bool showInteractionIcon = false;

    public static DOCTORS curDoctor = DOCTORS.Monroe;

    public override void _Ready()
    {
        exclamation = GetNode<Sprite>("Exclamation");
        exclamation.Visible = false;

        //Get Components
        pivot = GetNode<Node2D>("Pivot");
        axe = GetNode<Sprite>("Pivot/Axe");
        gun = GetNode<Sprite>("Pivot/Gun");
        axe.Visible = false;
        gun.Visible = false;
    }

    public override void _PhysicsProcess(float delta)
    {
        vel = Vector2.Zero;

        if (showInteractionIcon)
        {
            exclamation.Visible = true;
        }
        else
        {
            exclamation.Visible = false;
        }

        if (inInteraction)
        {
            return; //No moving while textbox is open
        }

        //Rotate pivot, possibly flip
        if (pivot.GlobalRotationDegrees > 90 || pivot.GlobalRotationDegrees < -90)
        {
            pivot.Scale = new Vector2(pivot.Scale.x, -1);
        }
        else
        {
            pivot.Scale = new Vector2(pivot.Scale.x, 1);
        }
        pivot.LookAt(GetGlobalMousePosition());

        if (Input.IsActionPressed("Up"))
        {
            vel.y -= 1;
        }
        if (Input.IsActionPressed("Down"))
        {
            vel.y += 1;
        }
        if (Input.IsActionPressed("Right"))
        {
            vel.x += 1;
        }
        if (Input.IsActionPressed("Left"))
        {
            vel.x -= 1;
        }

        MoveAndSlide(vel * speed);
    }

    public void switchDoctors()
    {
        //Enable/Disable certain things like gun and axe
        switch (curDoctor)
        {
            case DOCTORS.Monroe: //Monroe is a talker, doesn't carry anything but gets more dialogue options
                axe.Visible = false;
                gun.Visible = false;
                break;

            case DOCTORS.Sanders: //Sanders has an axe which can clear some environmental things and do close-range damage
                axe.Visible = true;
                gun.Visible = false;
                break;

            case DOCTORS.Archer: //Archer is purely offensive, his gun can get enemies at long range
                axe.Visible = false;
                gun.Visible = true;
                break;
        }
    }
}
