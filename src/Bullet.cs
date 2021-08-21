using Godot;
using System;

public class Bullet : KinematicBody2D
{
    [Export]
    private float speed = 500f;
    public override void _PhysicsProcess(float delta)
    {
        Vector2 vel = new Vector2(1, 0).Rotated(Rotation);
        MoveAndSlide(vel * speed);
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (body.GetType() == typeof(NPC))
        {
            ((NPC)body).onHit();
            QueueFree();
        }
        else if (body.GetType() == typeof(Enemy))
        {
            ((Enemy)body).onHit();
            QueueFree();
        }
        else if (body.GetType() == typeof(Spawner))
        {
            ((Spawner)body).onHit();
            QueueFree();
        }
    }

    public void _on_VisibilityNotifier2D_screen_exited()
    {
        //No shots off screen
        QueueFree();
    }
}
