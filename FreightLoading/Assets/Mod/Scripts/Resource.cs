using System;
using UnityEngine;

public class Resource : MonoBehaviour {
    public enum Types {
        None,
        PassengersNormal,
        PassengersBaggage,
        PassengersMedium,
        PassengersElite,
        Coal,
        LiquidFuel,
        Acid,
        Automobiles,
        Food,
        IndustrialGas,
        LargeObjects,
        Liquids,
        Livestock,
        LooseBulkCommodities,
        Mail,
        Oil,
        SheetMetal,
        Wood,
        Oversized,
    }

    public Types Type;
    public string[] DisplayNames;
    public int[] Multipliers;
    [NonSerialized] public string DisplayName;
    [NonSerialized] public int Multiplier;
    [NonSerialized] public int Count;
    [Space]
    public int MinimumDisplayCount;
    public int MaximumDisappearCount;

    /// <summary>
    /// Randomizes this resource, sets its name, spawn rate, etc.
    /// </summary>
    public void Reset() {
        int r = UnityEngine.Random.Range(0, DisplayNames.Length);
        DisplayName = DisplayNames[r];
        Multiplier = Multipliers[r];
        Count = 0;
    }

    /// <summary>
    /// Start
    /// </summary>
    public void Awake() {
        // TODO: Had to make this awake because it would generate resources before the appropriate values were set. Do some research into how I can prevent this/solve this more elegantly in the future, this doesn't feel like good practice to me.
        Reset();
    }
}
