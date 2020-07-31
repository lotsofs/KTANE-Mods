using UnityEngine;

public class SemiSimpleCar : TrainCar {

    [Space]

    public Resource Resource;
    public int Count;
    public Sprite AppearanceFull;

    /// <summary>
    /// Semi Simple Car removes stuff from one resource. It's the same as a simple car, except its appearance changes.
    /// </summary>
    public override void FillCar(bool correct, int currentStage, FreightTableRule usedRule) {
        if (Resource != null) {
            Resource.Count -= Count;
        }
        if (!correct && Resource.Count < 0 && Resource.Count > -9999) {
            Resource.Count = 0;
        }
    }

    public override Sprite AttachCar(bool correct, int currentStage, FreightTableRule usedRule) {
        return AppearanceFull;
    }
}
