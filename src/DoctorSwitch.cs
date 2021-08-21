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


    public override void onInteraction()
    {
        //Using the player, swtich the sprites, and DOCTOR enum of the two doctors
        //Also switch stats
        Rect2 tempRect = ((Sprite)gameManager.getPlayer().GetChild(0)).RegionRect;
        DOCTORS tempName = Player.curDoctor;

        //Swap player
        ((Sprite)gameManager.getPlayer().GetChild(0)).RegionRect = ((Sprite)GetChild(0)).RegionRect;
        Player.curDoctor = mDoctor;
        gameManager.getPlayer().switchDoctors();

        //Swap NPC
        ((Sprite)GetChild(0)).RegionRect = tempRect;
        mDoctor = tempName;

        // TODO: Change UI
        // TODO: SFX
    }

    public void swapSprites(Rect2 r, DOCTORS d)
    {
        ((Sprite)GetChild(0)).RegionRect = r;
        mDoctor = d;
    }
}
