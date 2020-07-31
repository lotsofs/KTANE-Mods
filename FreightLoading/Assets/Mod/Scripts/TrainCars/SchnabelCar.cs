using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SchnabelCar : TrainCar {
    [Space]
    public Resource Resource;
    public int Count;
    public Sprite AppearanceNRPV;
    public Sprite AppearanceElectrical;

    /// <summary>
    /// Schnabel cars set a resource to 0, and they can have multiple appearances beyond just empty and full
    /// </summary>
    public override void FillCar(bool correct, int currentStage, FreightTableRule usedRule) {
        if (Resource != null && Resource.Count > 0) {
            Resource.Count = 0;
            if (Resource.DisplayName == "Electrical Transformer") {
                return;
            }
            else {
                return;
            }
        }
        else {
            return;
        }
    }

    public override Sprite AttachCar(bool correct, int currentStage, FreightTableRule usedRule) {
        if (Resource != null && Resource.Count > 0) {
            if (Resource.DisplayName == "Electrical Transformer") {
                return AppearanceElectrical;
            }
            else {
                return AppearanceNRPV;
            }
        }
        else {
            return Appearance;
        }
    }
}
