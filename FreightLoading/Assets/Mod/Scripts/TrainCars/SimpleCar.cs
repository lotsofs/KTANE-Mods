using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCar : TrainCar {

    [Space]

    public Resource Resource;
    public int Count;

    /// <summary>
    /// Simple cars just remove some from one resource
    /// </summary>
    public override void FillCar(bool correct, int currentStage, FreightTableRule usedRule) {
        if (Resource == null) {
            return;
        }
        Resource.Count -= Count;
        if (!correct && Resource.Count < 0 && Resource.Count > -9999) {
            Resource.Count = 0;
        }
    }

    public override Sprite AttachCar(bool correct, int currentStage, FreightTableRule usedRule) {
        return Appearance;
    }
}
