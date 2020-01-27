using UnityEngine;

public class BaggageCar : TrainCar {

    [Space]

    public Resource Remove;
    public Resource Add;
    public int Count;

    /// <summary>
    /// Baggage Car removes one resource and adds it to another
    /// </summary>
    public override Sprite FillCar(bool correct, int currentStage, FreightTableRule usedRule) {
        Remove.Count -= Count;
        Add.Count += Count;
        return Appearance;
    }
}
