using System;
using UnityEngine;

//[CreateAssetMenu(fileName = "TrainCar", menuName = "Scriptable Objects/TrainCar/Standard", order = 1)]
public abstract class TrainCar : MonoBehaviour {
    public enum Types {
        None,
        LocomotiveElectric,
        LocomotiveCoal,
        LocomotiveDiesel,

        CrewCar,
        Caboose,

        BaggageCar,
        DomeCar,
        DoubleDecker,
        OpenCoach,
        ClosedCoach,
        Sleeper,
        DiningCar,

        PostOffice,

        WellCar,
        FlatCar,
        Hopper,
        BoxCar,
        RefrigeratedWagon,
        TankCar,
        CoilCar,
        SchnabelCar,
        LivestockCar,
        Autorack,

    }

    public string FriendlyName;
    public Sprite Appearance;
    public Types Type;
    public bool Promptable;
    [NonSerialized] public bool Prompted;

    /// <summary>
    /// Fills the car with resources
    /// </summary>
    /// <param name="correct"></param>
    /// <param name="currentStage"></param>
    /// <param name="usedRule"></param>
    /// <returns></returns>
    public abstract Sprite FillCar(bool correct, int currentStage, FreightTableRule usedRule);
}
