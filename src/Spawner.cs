using Godot;
using System;

public class Spawner : KinematicBody2D
{
    //Components
    private AudioStreamPlayer2D audio;
    private Position2D spawnPos;
    private Timer t;
    private RandomNumberGenerator rand = new RandomNumberGenerator();
    [Export]
    private PackedScene enemy;

    //Control
    public bool unbreakable = false; //Villager nests cannot be destroyed


    public override void _Ready()
    {
        audio = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        spawnPos = GetNode<Position2D>("Sprite/Position2D");
        t = GetNode<Timer>("Timer");

        rand.Randomize();
        t.Start();
    }

    public void onHit()
    {
        if (unbreakable)
        {
            return;
        }
        //TODO: Sound
        Visible = false;
        audio.Play();
        t.Stop();
    }

    public void _on_AudioStreamPlayer2D_finished()
    {
        QueueFree();
    }

    public void _on_Timer_timeout()
    {
        //Spawn enemy instance at spawn location
        Enemy e = (Enemy)enemy.Instance();
        e.onSpawn(false, VILLAGER_TYPE.MISC);
        GetParent().AddChild(e);
        e.Position = spawnPos.GlobalPosition;
        t.WaitTime = rand.RandiRange(20, 45);
        t.Start();

    }
}
