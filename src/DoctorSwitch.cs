using Godot;
using System;


/*
 * The DoctorSwitch is a special type of NPC meant to switch the doctors and not directly interact with.
 * 
 */

public class DoctorSwitch : NPC
{
    //Control
    [Export]
    public DOCTORS mDoctor = DOCTORS.Sanders;
    public int storedHealth = 14;
    private readonly int MAX_HEALTH = 14;

    //Components
    private Timer t;

    public override void _Ready()
    {
        base._Ready();
        t = GetNode<Timer>("Timer");
    }


    public override void onInteraction()
    {
        //Using the player, swtich the sprites, and DOCTOR enum of the two doctors
        Rect2 tempRect = ((Sprite)gameManager.getPlayer().GetChild(0)).RegionRect;
        DOCTORS tempName = Player.curDoctor;
        int tempHealth = gameManager.getPlayer().getCurHealth();

        //Swap player
        ((Sprite)gameManager.getPlayer().GetChild(0)).RegionRect = ((Sprite)GetChild(0)).RegionRect;
        Player.curDoctor = mDoctor;
        gameManager.getPlayer().switchDoctors(storedHealth);

        //Swap NPC
        ((Sprite)GetChild(0)).RegionRect = tempRect;
        mDoctor = tempName;
        storedHealth = tempHealth;

        //Reset
        gameManager.resetDecayTimer();
        t.Stop();
        t.WaitTime = 15;
        t.Start();
    }

    public void swapSprites(Rect2 r, DOCTORS d)
    {
        ((Sprite)GetChild(0)).RegionRect = r;
        mDoctor = d;
    }

    public void _on_Timer_timeout()
    {
        //Heal doctor if not done so
        if (storedHealth < MAX_HEALTH)
        {
            storedHealth++;
            GD.Print("Healed " + mDoctor.ToString());
        }
    }
}
