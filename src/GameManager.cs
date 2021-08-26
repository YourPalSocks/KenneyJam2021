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
    private AudioStreamPlayer musicBox;
    private UI ui;
    [Export]
    private AudioStream[] streams; //0 for ambiant theme, 1 for Villager reveal

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
    [Export]
    private PackedScene cluePref;
    private int clueSpot = 0;

    Color[] possibleColors = { Colors.LightGreen, Colors.LightPink, Colors.LightYellow, Colors.Lime, 
        Colors.MediumAquamarine, Colors.Olive, Colors.OrangeRed, Colors.HotPink, Colors.MediumTurquoise,
        Colors.Orange, Colors.PeachPuff, Colors.Salmon, Colors.Green, Colors.GreenYellow };
    private List<string> names = new List<string>();
    private List<string> jobs = new List<string>();
    private List<string> jobEvidence = new List<string>();
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

    int poolSpot = 0;

    private Player player;

    private List<DoctorSwitch> swaps = new List<DoctorSwitch>();
    private RandomNumberGenerator rand = new RandomNumberGenerator();

    private List<Clue> evidenceBank = new List<Clue>();

    private StatsObserver stats;

    public override void _Ready()
    {
        rand.Randomize();
        t = GetNode<Timer>("ActivityTimer");
        decayTimer = GetNode<Timer>("DecayTimer");
        musicBox = GetNode<AudioStreamPlayer>("Music");
        stats = GetTree().Root.GetNode<StatsObserver>("StatsObserver");
        ui = GetNode<UI>("CanvasLayer/UI");
        musicBox.Stream = streams[0];
        musicBox.Play();

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
            if (!l.Empty())
            {
                names.Add(l);
            }
        }
        f.Close();
        //Jobs too
        f.Open("res://Assets/Jobs.tres", File.ModeFlags.Read);
        lines = f.GetAsText().Split('\n');
        foreach (string l in lines)
        {
            if (!l.Empty())
            {
                jobs.Add(l);
            }
        }
        f.Close();

        //Get job evidence
        f.Open("res://Assets/ClueOptions.res", File.ModeFlags.Read);
        lines = f.GetAsText().Split('\n');
        foreach (string l in lines)
        {
            if (!l.Empty())
            {
                jobEvidence.Add(l);
            }
        }
        //Spawn and prep a map
        buildMap();
        t.Start();
    }

    public void resetDecayTimer()
    {
        decayTimer.Stop();
        decayTimer.WaitTime = 30;
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
            stats.addBugName(n_npc.ToString());
        }
        //Position --TODO: Based on map stuff
        n_npc.Position = pos;
        //Set sprite
        Vector2 spr = villagerRects[rand.RandiRange(0, villagerRects.Length - 1)];
        ((Sprite)n_npc.GetChild(0)).RegionRect = new Rect2(spr, new Vector2(16,16));


        npcs.Add(n_npc);
        AddChild(n_npc);
        //Generate 2 clues for the bugs
        if (n_npc.getPersonality() == VILLAGER_TYPE.Bug)
        {
            //First clue is related to the NPC's job
            Clue jobClue = (Clue) cluePref.Instance();
            string[] brokenString = n_npc.ToString().Split(" ");
            jobClue.changeClueName(jobEvidence[jobs.IndexOf(brokenString[0])]);
            //Spawn clue at interest point (already shuffled in MapIndexEntry
            AddChild(jobClue);
            jobClue.Position = map.spots[clueSpot].Position;
            clueSpot++;

            //Second clue contains a part of the bug's name
            Clue nameClue = (Clue)cluePref.Instance();
            nameClue.changeClueName(jobEvidence[jobEvidence.Count - 1] + " " + brokenString[1].Substring(0, 2));
            //Spawn this clue
            AddChild(nameClue);
            nameClue.Position = map.spots[clueSpot].Position;
            clueSpot++;

        }
        poolSpot++;
    }

    /*
     * Each map comes with a doctor's shed (with two parented Position2Ds for swaps, houses, and 16 villager spawn locations
     */
    private void buildMap()
    {
        //Spawn the mapIndex to create a map, then hand off and delete
        Node mapIndexInstance = MapIndex.Instance();

        MapIndexEntry map = (MapIndexEntry) mapIndexInstance.GetChild(rand.RandiRange(0, mapIndexInstance.GetChildCount() - 1));
        mapIndexInstance.RemoveChild(map);
        AddChild(map);
        map.Position = Vector2.Zero;
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
                swaps.Add(s);
                i++;
            }
        }

        //Get Nest areas
        foreach (Vector2 v in map.nestAreas)
        {
            nestSites.Add(v);
        }

        //Spawn obstacles (Axe breakables)
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

    /*
     * Checks the evidenceBank and compares it to the NPC's job
     * Returns either null (false) or with the relevant evidence
     */
    public string hasRelevantEvidence(NPC n)
    {
        string job = n.ToString().Split(" ")[0];
        foreach (Clue c in evidenceBank)
        {
            if (c.ToString().Contains("A tag saying:"))
            {
                continue;
            }
            if (jobEvidence.IndexOf(c.ToString()) == jobs.IndexOf(job))
            {
                return c.ToString();
            }
        }
        return null;
    }

    public int getEvidenceAmount()
    {
        return evidenceBank.Count;
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
    public string getRandomVillager(NPC me)
    {
        string toRet = "";
        do
        {
            int i = rand.RandiRange(0, 10);
            //Slightly higher chance if intuit
            if (me.getPersonality() == VILLAGER_TYPE.Intuit)
            {
                i = rand.RandiRange(0, 8); //20% chance
            }
            else if (me.getPersonality() == VILLAGER_TYPE.Instigator) //Even higher if instigator
            {
                i = rand.RandiRange(0, 4); //50% chance
            }

            //Actually rat out a bug
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
        } while (toRet.Equals(me.ToString()) == true); //Because villagers aren't going to rat themselves out
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
            //Change music back
            musicBox.Stop();
            musicBox.Stream = streams[0];
            musicBox.Play();
        }
        else
        {
            stats.addDeadVillager();
        }
        n.RotationDegrees = 90;

        if (n.getPersonality() != VILLAGER_TYPE.Bug && !n.isNest && !n.isBugForm)
        {
            //LOSE: Cannot kill innocents
            stats.setEndScenario(3);
            musicBox.Stop();
            ui.goToEnding();
            return;
        }
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
        exposedBug.onSpawn(true, VILLAGER_TYPE.Bug, n.Position);
        npcs[npcs.IndexOf(n)] = exposedBug;
        activeBugs[activeBugs.IndexOf(n)] = exposedBug;
        AddChild(exposedBug);


        n.QueueFree();

        //Change music
        musicBox.Stop();
        musicBox.Stream = streams[1];
        musicBox.Play();

        exposedBug.playAwakenSound();
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
        //Check if no doctors left
        if (swaps.Count < 1)
        {
            stats.setEndScenario(4);
            ui.goToEnding();
        }
        else
        {
            DoctorSwitch docS = swaps[0];
            //Force a change, and move to the doctor swap
            Vector2 swapPos = docS.Position;
            docS.onInteraction();
            player.Position = swapPos;

            swaps.Remove(docS);
            docS.QueueFree();
            //Check if at last doctor and is Monroe, this is a lose condition
            if (swaps.Count < 1 && Player.curDoctor == DOCTORS.Monroe)
            {
                stats.setEndScenario(1);
                ui.goToEnding();
            }
        }
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
            //Change destroyable texture
            sInst.Modulate = new Color(0,0,1);
            AddChild(sInst);

            sInst.Position = nestSites[i];
        }
        checkForWinOrLose();
        t.WaitTime = rand.RandiRange(60, 180);
        t.Start();
    }

    public void _on_DecayTimer_timeout()
    {
        if (DialogueBox.textboxActive)
        {
            return;
        }
        //Player should lose some health due to blight
        player.onHit();
    }

    public void checkForWinOrLose()
    {
        //Check if no active bugs
        if (activeBugs.Count <= 0)
        {
            //Win
            stats.setEndScenario(0);
            ui.goToEnding();
        }

        //Check if only bugs are left
        if (npcs.Count == activeBugs.Count)
        {
            stats.setEndScenario(2);
            ui.goToEnding();
        }
    }

    public void addToClueBank(Clue c)
    {
        ui.updateEvidence(c.ToString());
        evidenceBank.Add(c);
    }
}
