using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/*
 * MapIndexEntry doens't really hold a whole lot other than local nodepaths for the GameManager
 */
public class MapIndexEntry : Node2D
{
    [Export]
    public NodePath spawnPoint;

    [Export]
    public NodePath doctorPoint1;

    [Export]
    public NodePath doctorPoint2;

    //Lists
    public List<InterestLocation> spots = new List<InterestLocation>();
    public List<Vector2> npcSpawns = new List<Vector2>();

    public override void _Ready()
    {
        foreach (Node2D n in GetChild(5).GetChildren())
        {
            npcSpawns.Add(n.Position);
        }
        //Shuffle spawn locations
        Random rng = new Random();
        int b = npcSpawns.Count();
        Vector2 temp;
        while (b > 1)
        {
            b--;
            int k = rng.Next(b + 1);
            temp = npcSpawns[k];
            npcSpawns[k] = npcSpawns[b];
            npcSpawns[b] = temp;
        }

        //Spots
        foreach (InterestLocation n in GetChild(4).GetChildren())
        {
            //TODO: Random chance of adding a "Red Herring" --Clue with nothing there
            /*if (n.inUse)
            {
                spots.Add(n);
            }*/
            spots.Add(n);
        }
    }
}
