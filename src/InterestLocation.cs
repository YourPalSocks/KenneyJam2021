using Godot;
using System;

/*
 * InterestLocations are places that Instigator villagers will lead players to, will they be useful?
 */
public class InterestLocation : Node2D
{
    [Export]
    public string descriptor = "";
    public bool inUse = false;
}
