using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class GameManager : Node2D
{
    //Components
    private ColorRect CRT_EFFECT;
    private DialogueBox dialogue;
    private Timer t;
    private Timer decayTimer;

    [Export]
    private PackedScene NPCBase;
    [Export]
    private PackedScene MapIndex;
    [Export]
    private PackedScene axeBreakable;
    [Export]
    private PackedScene nestPref;
    [Export]
    private PackedScene exposedPref;

    Color[] possibleColors = { Colors.LightGreen, Colors.LightPink, Colors.LightYellow, Colors.Lime, 
        Colors.MediumAquamarine, Colors.Olive, Colors.OrangeRed, Colors.HotPink, Colors.MediumTurquoise,
        Colors.Orange, Colors.PeachPuff, Colors.Salmon, Colors.Green, Colors.GreenYellow };
    private List<string> names = new List<string>();
    private List<string> jobs = new List<string>();
    private List<NPC> npcs = new List<NPC>();

    readonly Vector2[] villagerRects = { new Vector2(384,0), new Vector2(448, 0), new Vector2(496, 16), 
        new Vector2(400, 16), new Vector2(496, 16), new Vector2(496, 32), new Vector2(384, 64),
        new Vector2(432, 64), new Vector2(416, 0), new Vector2(384, 16), new Vector2(416, 64),
        new Vector2(400, 64), new Vector2(384, 96), new Vector2(400, 144), new Vector2(480, 144)
    };

    //Though random, we want a variety of villager types and a controlled number of infected --16 total
    private VILLAGER_TYPE[] TYPE_POOL = { VILLAGER_TYPE.Bug, VILLAGER_TYPE.Bug, 
       VILLAGER_TYPE.Paranoid, VILLAGER_TYPE.Paranoid, 
       VILLAGER_TYPE.Instigator, VILLAGER_TYPE.Instigator, 
       VILLAGER_TYPE.Clueless, VILLAGER_TYPE.Clueless, VILLAGER_TYPE.Clueless,
       VILLAGER_TYPE.Intuit, VILLAGER_TYPE.Intuit, VILLAGER_TYPE.Intuit, VILLAGER_TYPE.Intuit, VILLAGER_TYPE.Intuit,
       VILLAGER_TYPE.Silent, VILLAGER_TYPE.Silent
   };
    private List<NPC> activeBugs = new List<NPC>();
    private List<Vector2> nestSites = new List<Vector2>();

    /*
     * //Test TYPE_POOL
     * private VILLAGER_TYPE[] TYPE_POOL = {
     * 
     * }; 
     */

    int poolSpot = 0;

    private Player player;

    private DoctorSwitch[] swaps = new DoctorSwitch[2];
    private RandomNumberGenerator rand = new RandomNumberGenerator();

    private string[] evidenceBank;
 


    public override void _Ready()
    {
        rand.Randomize();
        t = GetNode<Timer>("ActivityTimer");
        decayTimer = GetNode<Timer>("DecayTimer");
        t.WaitTime = rand.RandiRange(40, 100);

        //Shuffle the pool
        Random random = new Random();
        TYPE_POOL = TYPE_POOL.OrderBy(x => random.Next()).ToArray();

        //Enable the CRT filter
        CRT_EFFECT = GetNode<ColorRect>("CanvasLayer/ColorRect");
        CRT_EFFECT.Visible = true;

        dialogue = GetNode<DialogueBox>("CanvasLayer/Dialogue Box");
        dialogue.Visible = false;

        player = GetNode<Player>("Player");

        //Build name dictionary
        File f = new File();
        f.Open("res://Assets/FirstNames.tres", File.ModeFlags.Read);
        string[] lines = f.GetAsText().Split('\n');
        foreach (string l in lines)
        {
            names.Add(l);
        }
        f.Close();
        //Jobs too
        f.Open("res://Assets/Jobs.tres", File.ModeFlags.Read);
        lines = f.GetAsText().Split('\n');
        foreach (string l in lines)
        {
            jobs.Add(l);
        }
        f.Close();

        //Spawn and prep a map
        buildMap();
        t.Start();
    }

    public void resetDecayTimer()
    {
        decayTimer.Stop();
        decayTimer.WaitTime = 20;
        decayTimer.Start();
    }

    /*
     * createNPC is responsible for just that, creating NPCs. 
     */
    private void createNPC(Vector2 pos, MapIndexEntry map)
    {
        NPC n_npc = (NPC) NPCBase.Instance();
        //Set name and colors
        n_npc.onSpawn(getJobandName(), getRandomColor(), TYPE_POOL[poolSpot], map.spots[rand.RandiRange(0, map.spots.Count - 1)]);
        if (n_npc.getPersonality() == VILLAGER_TYPE.Bug)
        {
            activeBugs.Add(n_npc);
        }
        //Position --TODO: Based on map stuff
        n_npc.Position = pos;
        //Set sprite
        Vector2 spr = villagerRects[rand.RandiRange(0, villagerRects.Length - 1)];
        ((Sprite)n_npc.GetChild(0)).RegionRect = new Rect2(spr, new Vector2(16,16));


        npcs.Add(n_npc);
        AddChild(n_npc);
        poolSpot++;
    }

    /*
     * Each map comes with a doctor's shed (with two parented Position2Ds for swaps, houses, and 16 villager spawn locations
     */
    private void buildMap()
    {
        //Spawn the mapIndex to create a map, then hand off and delete
        Node mapIndexInstance = MapIndex.Instance();
        MapIndexEntry map = (MapIndexEntry) mapIndexInstance.GetChild(0);
        mapIndexInstance.RemoveChild(map);
        AddChild(map);
        ((Node2D)map).ZIndex -= 1;
        mapIndexInstance.QueueFree();

        //Use children of map to place things
        //First spawn point, should always be chlid(0)
        player.Position = map.GetNode<Node2D>(map.spawnPoint).Position;

        //Place other Doctors
        int i = 0;
        foreach (Node n in GetChildren())
        {
            if (n.GetType() == typeof(DoctorSwitch))
            {
                DoctorSwitch s = (DoctorSwitch)n;
                switch (i)
                {
                    case 0:
                        s.swapSprites(new Rect2(448, 64, new Vector2(16, 16)), DOCTORS.Sanders);
                        s.Position = map.GetNode<Node2D>(map.doctorPoint1).GlobalPosition;
                        break;

                    case 1:
                        s.swapSprites(new Rect2(384, 64, new Vector2(16, 16)), DOCTORS.Archer);
                        s.Position = map.GetNode<Node2D>(map.doctorPoint2).GlobalPosition;
                        break;
                }
                swaps[i] = s;
                i++;
            }
        }

        //Get Nest areas
        foreach (Vector2 v in map.nestAreas)
        {
            nestSites.Add(v);
        }

        //TODO: Spawn clues

        //TODO: Spawn obstacles (Axe breakables)
        foreach (Vector2 p in map.breakables)
        {
            //Spawn a breakable at that location
            AxeBreakable axB = (AxeBreakable) axeBreakable.Instance();
            AddChild(axB);
            axB.Position = p;
        }

        //Make and place villagers --16 MAX
        for (int x = 0; x < TYPE_POOL.Length; x++)
        {
            //Check if out of spawn locations
            if (x >= map.npcSpawns.Count)
            {
                break;
            }
            createNPC(map.npcSpawns[x], map);
        }

        //Build Dialogue after everyone is here
        foreach (NPC n in npcs)
        {
            n.getDialogue(n.getPersonality());
        }
    }

    public DialogueBox getDialogueBox()
    {
        return dialogue;
    }

    private Color getRandomColor()
    {
        return possibleColors[GD.Randi() % possibleColors.Length];
    }

    private string getJobandName()
    {
        GD.Randomize();
        int j = (int)GD.RandRange(0, jobs.Count);
        int n = (int)GD.RandRange(0, names.Count);
        return jobs[j] + " " + names[n];
    }

    /*
     * I know it says random but there is actually a 2/10 chance that this guess is the Bug
     */
    public string getRandomVillager(string name)
    {
        string toRet = "";
        do
        {
            int i = rand.RandiRange(0, 10);
            if (i <= 1)
            {
                //Return a bug
                foreach (NPC n in npcs)
                {
                    if (n.getPersonality() == VILLAGER_TYPE.Bug)
                    {
                        toRet = n.getName();
                    }
                }
            }
            else
            {
                toRet = npcs[rand.RandiRange(0, npcs.Count - 1)].getName();
            }
        } while (toRet.Equals(name) == true); //Because villagers aren't going to rat themselves out
        return toRet;
    }

    public void killVillager(NPC n)
    {
        //Remove villager from list and then QueueFree
        npcs.Remove(n);
        //Bug check
        if (n.getPersonality() == VILLAGER_TYPE.Bug)
        {
            activeBugs.Remove(n);
        }
        n.RotationDegrees = 90;

        if (n.getPersonality() != VILLAGER_TYPE.Bug && !n.isNest && !n.isBugForm)
        {
            //TODO: Lose
            GD.Print("You lose");
            return;
        }
        GD.Print(activeBugs.Count);
        checkForWinOrLose();
    }

    public void turnVillagerToNest()
    {
        //Get a random villager that isn't a bug
        NPC villager;
        do
        {
            villager = npcs[rand.RandiRange(0, npcs.Count - 1)];
        } while (villager.getPersonality() == VILLAGER_TYPE.Bug);
        //Remove them
        villager.isNest = true;
        killVillager(villager);
        //Spawn unbreakable nest on top of them
        Spawner vSp = (Spawner) nestPref.Instance();
        vSp.Position = villager.Position;
        AddChild(vSp);
        vSp.unbreakable = true;

        checkForWinOrLose();
    }

    public void exposeVillagerAsBug(NPC n)
    {
        //Replace this villager with an Exposed bug
        Enemy exposedBug = (Enemy) exposedPref.Instance();
        exposedBug.onSpawn(true, VILLAGER_TYPE.Bug);
        npcs[npcs.IndexOf(n)] = exposedBug;
        activeBugs[activeBugs.IndexOf(n)] = exposedBug;
        AddChild(exposedBug);

        exposedBug.Position = n.Position;
        n.QueueFree();

    }

    public Player getPlayer()
    {
        return player;
    }

    /*
     * This is called after a Doctor dies,
     * Swap the dead doctor with a live one and delete the swap,
     * If no swap available, GameOver
     */
    public void forceChangePlayer()
    {
        int firstAvailableDoctor = -1;
        //Check if any doctors left
        for (int i = 0; i < 2; i++)
        {
            if (swaps[i] != null)
            {
                firstAvailableDoctor = i;
                break;
            }
        }
        if (firstAvailableDoctor == -1)
        {
            GD.Print("No Doctors left, GAME OVER");
        }
        //Force a change, and move to the doctor swap
        Vector2 swapPos = swaps[firstAvailableDoctor].Position;
        swaps[firstAvailableDoctor].onInteraction();
        player.Position = swapPos;

        swaps[firstAvailableDoctor].QueueFree();

    }

    public void _on_ActivityTimer_timeout()
    {
        int action = rand.RandiRange(1, 10);
        //On 1-6, kill villagers, otherwise make a nest
        if (action <= 6)
        {
            GD.Print("NEST EVENT");
            //Kill a villager for each bug
            turnVillagerToNest();
        }
        else
        {
            GD.Print("NEST SPAWNED");
            int i = rand.RandiRange(0, nestSites.Count - 1);
            Spawner sInst = (Spawner)nestPref.Instance();
            AddChild(sInst);

            sInst.Position = nestSites[i];
        }
        checkForWinOrLose();
        t.WaitTime = rand.RandiRange(40, 100);
        t.Start();
    }

    public void _on_DecayTimer_timeout()
    {
        //Player should lose some health due to blight
        player.onHit();
    }

    public void checkForWinOrLose()
    {
        //Check if no active bugs
        if (activeBugs.Count <= 0)
        {
            //TODO: Win
            GD.Print("You win");
        }
    }
}
