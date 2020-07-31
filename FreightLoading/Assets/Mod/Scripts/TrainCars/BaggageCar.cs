using UnityEngine;

public class BaggageCar : TrainCar {

    [Space]

    public Resource Remove;
    public Resource Add;
    public int Count;

    /// <summary>
    /// Baggage Car removes one resource and adds it to another
    /// </summary>
    public override void FillCar(bool correct, int currentStage, FreightTableRule usedRule) {
        if (correct) {
            Remove.Count -= Count;
            Add.Count += Count;
        }
        else {
            Add.Count += Remove.Count;
            Remove.Count -= Remove.Count;
        }
    }

    public override Sprite AttachCar(bool correct, int currentStage, FreightTableRule usedRule) {
        return Appearance;
    }
}
