using Godot;
using System;

public class Bullet : KinematicBody2D
{
    [Export]
    private float speed = 500f;
    public bool fromEnemy = false;
    public override void _PhysicsProcess(float delta)
    {
        Vector2 vel = new Vector2(1, 0).Rotated(Rotation);
        MoveAndSlide(vel * speed);
    }

    public void _on_Area2D_body_entered(Node body)
    {
        if (body.GetType() == typeof(NPC) && !fromEnemy) //Kinda unfair if enemies can do this
        {
            ((NPC)body).onHit();
            QueueFree();
        }
        else if (body.GetType() == typeof(Enemy) && !fromEnemy) //The above comment still applies
        {
            ((Enemy)body).onHit();
            QueueFree();
        }
        else if (body.GetType() == typeof(Spawner) && !fromEnemy)
        {
            ((Spawner)body).onHit();
            QueueFree();
        }
        else if (body.GetType() == typeof(Player) && fromEnemy) //The player shouldn't be able to be hit by their own bullets but whatevs
        {
            ((Player)body).onHit();
            QueueFree();
        }
    }

    public void _on_VisibilityNotifier2D_screen_exited()
    {
        //No shots off screen
        QueueFree();
    }
}
