using Godot;
using System;
using System.Collections.Generic;

public enum DOCTORS { Monroe, Sanders, Archer }
public class Player : KinematicBody2D
{
    //Controls
    [Export]
    private float speed = 150f;
    private Vector2 vel = Vector2.Zero;
    public int health = 14;
    private bool canUseAction = true;

    private float axeCooldown = 0.5f;
    private float gunCooldown = 0.2f;
    private float gunReloadTime = 1.2f;
    private int shotsLeft = 6;


    //Componenets
    private Sprite exclamation;
    private Node2D pivot;
    private Sprite axe; //TODO: Axe and Gun classes
    private Sprite gun;
    private Timer cooldown;
    private AudioStreamPlayer audio;
    private GameManager gameManager;
    /*
     * 0 == Doctor Swap
     * 1 == Axe Swing
     * 2 == Gun Shoot
     * 3 == Gun Reload
     * 4 == Hurt
     * 5 == Dead
     */
    [Export]
    private List<AudioStream> streams = new List<AudioStream>();
    [Export]
    private PackedScene bulletPref;

    public static bool inInteraction = false;
    public static bool showInteractionIcon = false;
    public static readonly int HEALTH_MAX = 14;

    private int curHealth;
    private UI mUI;
 

    public static DOCTORS curDoctor = DOCTORS.Monroe;

    public override void _Ready()
    {
        exclamation = GetNode<Sprite>("Exclamation");
        exclamation.Visible = false;

        //Get Components
        pivot = GetNode<Node2D>("Pivot");
        axe = GetNode<Sprite>("Pivot/Axe");
        gun = GetNode<Sprite>("Pivot/Gun");
        cooldown = GetNode<Timer>("Cooldown");
        audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        mUI = GetParent().GetNode<UI>("CanvasLayer/UI");
        gameManager = (GameManager) GetParent();
        axe.Visible = false;
        gun.Visible = false;

        curHealth = HEALTH_MAX;
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

        //Handle click events
        if (Input.IsActionJustPressed("Doctor Move") && canUseAction)
        {
            handleClickEvent();
            canUseAction = false;
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

    public void handleClickEvent()
    {
        switch (curDoctor)
        {
            case DOCTORS.Sanders: //Swing Axe
                audio.Stream = streams[1];
                audio.Play();
                ((Node2D)axe.GetChild(0)).Visible = true;
                //Start timer
                ((Timer)axe.GetChild(0).GetChild(1)).Start();
                ((Area2D)axe.GetChild(0).GetChild(0)).Monitoring = true;
                cooldown.WaitTime = axeCooldown;
                break;

            case DOCTORS.Archer: //Fire Gun
                fireGun();
                break;
        }
        cooldown.Start();
    }

    public void switchDoctors(int nHealth)
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
        curHealth = nHealth;

        audio.Stream = streams[0];
        mUI.updateUI(curHealth);
        audio.Play();        
    }

    public int getCurHealth()
    {
        return curHealth;
    }

    public void fireGun()
    {
        //Either fire gun or start reloading
        if (shotsLeft > 0)
        {
            shotsLeft--;
            //Create a bullet at firepoint
            Bullet nBullet = (Bullet)bulletPref.Instance();
            nBullet.Position = ((Node2D)gun.GetChild(0)).GlobalPosition;
            nBullet.Rotation = ((Node2D)gun.GetParent()).Rotation;
            GetTree().Root.GetChild(0).AddChild(nBullet);
            cooldown.WaitTime = gunCooldown;
            //TODO: Fire sound
            audio.Stream = streams[2];
            audio.Play();
        }
        else 
        {
            cooldown.WaitTime = gunReloadTime;
            shotsLeft = 6;
            //TODO: Reload Sound
            audio.Stream = streams[3];
            audio.Play();
        }
        cooldown.Start();
    }

    public void onHit()
    {
        audio.Stream = streams[4];
        audio.Play();
        //TODO: Play sound
        curHealth--;
        //TODO: Check if dead
        if (curHealth <= 0)
        {
            gameManager.forceChangePlayer();
        }
        //TODO: Update UI
        mUI.updateUI(curHealth);
    }

    public void _on_Axe_Cooldown_timeout()
    {
        ((Node2D)axe.GetChild(0)).Visible = false;
        ((Area2D)axe.GetChild(0).GetChild(0)).Monitoring = false;
    }

    public void _on_Cooldown_timeout()
    {
        canUseAction = true;
    }

    public void _on_Area2D_body_entered(Node body) //Axe collision
    {

        if (body.GetType() == typeof(NPC))
        {
            ((NPC)body).onHit();
        }
        else if (body.GetType() == typeof(Enemy))
        {
            ((Enemy)body).onHit();
        }
        else if (body.GetType() == typeof(Spawner))
        {
            ((Spawner)body).onHit();
        }
        else if (body.GetType() == typeof(AxeBreakable))
        {
            body.QueueFree();
        }
    }
}
