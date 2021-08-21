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

    //Components
    private AudioStreamPlayer2D audio;
    [Export]
    private List<AudioStream> streams = new List<AudioStream>(); //0 for hit, 1 for death, 2 for attack, 3 for awakening

    public override void _Ready()
    {
        base._Ready();
        audio = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
    }

    public override void onInteraction() { } //Empty because we don't want interactions to occur
    public override void onSpawn(string n, Color c, VILLAGER_TYPE type, InterestLocation area)  { } //Still empty for same reason

    public void onSpawn(bool b, VILLAGER_TYPE g)
    {
        isExposed = b;
        isBugForm = true;
        mType = g;
        //TODO: Play some sort of sound when exposed 
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
            GD.Print(mType);
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
        audio.Stream = streams[0];
        audio.Play();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Visible)
        {
            //Move towards player
            Vector2 dis = gameManager.getPlayer().Position - Position;
            MoveAndCollide(dis * delta);
        }
    }

    public void _on_AudioStreamPlayer2D_finished()
    {
        if (audio.Stream == streams[1])
        {
            QueueFree();
        }
    }

    public void _on_Hit_Area_body_entered(Node body)
    {
        //Controls if play has been hit
        if (body.GetType() == typeof(Player))
        {
            //Damage player, let it take care of the rest
        }
    }
}
