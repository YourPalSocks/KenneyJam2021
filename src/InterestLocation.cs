using Godot;
using System;

/*
 * InterestLocations are places that Instigator villagers will lead players to, will they be useful?
 */
public class InterestLocation : Node
{
    [Export]
    public string descriptor = "";
    public bool inUse = false;
}
