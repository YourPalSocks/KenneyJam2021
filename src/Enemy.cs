using Godot;
using System;
using System.Collections.Generic;

/*
 * Enemies are special types of NPCs that do not talk to the player, but rather try to hurt them. 
 */
public class Enemy : NPC
{
    //Control
    [Export]
    private int health = 2; //Shots to take before dying
    private Player target;
    private bool isExposed = false;
    private Vector2 retreatPoint;

    //Components
    private AudioStreamPlayer2D audio;
    [Export]
    private List<AudioStream> streams = new List<AudioStream>(); //0 for hit, 1 for death, 2 for awakening
    [Export]
    private PackedScene bullet;
    private Timer t;

    private bool doneWithAwakening = false;

    public override void _Ready()
    {
        base._Ready();
        audio = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        t = GetNode<Timer>("Timer");
        GD.Randomize();
        t.WaitTime = GD.Randi() % 0.40f + 0.20f;
        t.Start();
    }

    public override void onInteraction() { } //Empty because we don't want interactions to occur
    public override void onSpawn(string n, Color c, VILLAGER_TYPE type, InterestLocation area)  { } //Still empty for same reason

    public void onSpawn(bool b, VILLAGER_TYPE g, Vector2 startPos)
    {
        retreatPoint = GlobalPosition;
        isExposed = b;
        isBugForm = true;
        mType = g;
        Position = startPos;
        retreatPoint = startPos;
        if (mType == VILLAGER_TYPE.MISC)
        {
            doneWithAwakening = true;
        }
    }


    public override void onHit()
    {
        if (!Visible)
        {
            return;
        }
        health--;
        if (health <= 0)
        {
            if (mType == VILLAGER_TYPE.Bug)
            {
                gameManager.killVillager(this);
            }
            //Play death sound
            Visible = false;
            audio.Stream = streams[1];
            audio.Play();
            return;
        }

        if (doneWithAwakening)
        {
            audio.Stream = streams[0];
            audio.Play();
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Visible && !DialogueBox.textboxActive)
        {
            //Check if distance is too close
            Vector2 dis = gameManager.getPlayer().Position - Position;
            if (dis.Length() >= 100f)
            {
                //Move towards player
                MoveAndCollide(dis * delta);
            }
            else if(dis.Length() < 95f)
            {
                //Move away from player
                MoveAndCollide((retreatPoint - Position) * delta * 0.15f); //Move slower this way
            }
        }
    }

    public void _on_AudioStreamPlayer2D_finished()
    {
        if (audio.Stream == streams[1])
        {
            QueueFree();
        }
        else if (audio.Stream == streams[2])
        {
            doneWithAwakening = true;
        }
    }

    public void playAwakenSound()
    {
        audio.Stream = streams[2];
        audio.Play();
    }

    public void _on_Hit_Area_body_entered(Node body)
    {
        //Controls if play has been hit
        if (body.GetType() == typeof(Player))
        {
            //Damage player, let it take care of the rest
        }
    }
    public void _on_Timer_timeout()
    {
        //Spawn bullet aimed at player
        if (!DialogueBox.textboxActive)
        {
            Bullet nB = (Bullet)bullet.Instance();
            nB.fromEnemy = true;
            nB.Position = Position;
            GetParent().AddChild(nB);
            nB.LookAt(gameManager.getPlayer().Position);
        }

        //Reset
        GD.Randomize();
        t.WaitTime = GD.Randi() % 1.8f + 1.4f;
        t.Start();
    }
}
