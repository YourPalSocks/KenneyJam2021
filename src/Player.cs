using Godot;
using System;

public class Player : KinematicBody2D
{
    //Controls
    [Export]
    private float speed = 150f;
    private Vector2 vel = Vector2.Zero;

    public static bool inInteraction = false;

    public DOCTORS curDoctor = DOCTORS.Monroe;

    #region Doctor read-onlys
    public enum DOCTORS { Monroe, Sanders, Archer, Glenn}
    //Rects of doctor sprites
    public readonly Vector2[] DOCTOR_SPRITES = {
        new Vector2(480, 64), new Vector2(448, 64),
        new Vector2(400, 64), new Vector2(496, 64)
    };

    public readonly Color[] DOCTOR_COLORS = {
        Colors.White, Colors.Green, Colors.Gold, Colors.Blue
    };
    #endregion

    public override void _PhysicsProcess(float delta)
    {
        vel = Vector2.Zero;

        if (inInteraction)
        {
            return; //No moving while textbox is open
        }

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
}
