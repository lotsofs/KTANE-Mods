﻿using UnityEngine;

public class HopperCar : TrainCar {
    public Resource Coal;
    public Resource Bulk;
    public FreightTableRule CoalRule;
    public FreightTableRule BulkRule;
    public Sprite AppearanceFull;
    public Sprite AppearanceSealed;
    [Space]
    [SerializeField] BombHelper _bombHelper;

    /// <summary>
    /// Hoppers remove stuff and have lots of different appearances, so I hardcoded this.
    /// </summary>
    /// <param name="correct"></param>
    /// <param name="currentStage"></param>
    /// <param name="usedRule"></param>
    public override void FillCar(bool correct, int currentStage, FreightTableRule usedRule) {
        if (correct) {
            if (currentStage == 2 || usedRule == CoalRule) {
                Coal.Count -= 200;
                return;
            }
            else if (usedRule == BulkRule) {
                if (Bulk.DisplayName == "Grain" || Bulk.DisplayName == "Cement") {
                    Bulk.Count -= 500;
                    return;
                }
                else {
                    Bulk.Count -= 500;
                    return;
                }
            }
            else {
                Debug.LogWarningFormat("[Railway Cargo Loading #{0}] Hopper car expected to be filled with coal or bulk, received {1} at stage {2}", _bombHelper.ModuleId, usedRule, currentStage);
            }
            return;
        }
        else if (Coal.Count <= 0 && Bulk.Count <= 0) {
            // do not fill it up if there's nothing
            return;
        }
        else if (Coal.Count < 500 && Bulk.Count > 5 * Coal.Count) {
            if (Bulk.DisplayName == "Grain" || Bulk.DisplayName == "Cement") {
                Bulk.Count -= 500;
                if (Bulk.Count < 0 && Bulk.Count > -9999) {
                    Bulk.Count = 0;
                }
                return;
            }
            else {
                Bulk.Count -= 500;
                if (Bulk.Count < 0 && Bulk.Count > -9999) {
                    Bulk.Count = 0;
                }
                return;
            }
        }
        else {
            Coal.Count -= 200;
            if (Coal.Count < 0 && Coal.Count > -9999) {
                Coal.Count = 0;
            }
            return;
        }
    }

    public override Sprite AttachCar(bool correct, int currentStage, FreightTableRule usedRule) {
        if (correct) {
            if (currentStage == 2 || usedRule == CoalRule) {
                return AppearanceFull;
            }
            else if (usedRule == BulkRule) {
                if (Bulk.DisplayName == "Grain" || Bulk.DisplayName == "Cement") {
                    return AppearanceSealed;
                }
                else {
                    return AppearanceFull;
                }
            }
            else {
                Debug.LogWarningFormat("[Railway Cargo Loading #{0}] Hopper car expected to be filled with coal or bulk, received {1} at stage {2}", _bombHelper.ModuleId, usedRule, currentStage);
            }
            return Appearance;
        }
        else if (Coal.Count <= 0 && Bulk.Count <= 0) {
            return Appearance;
        }
        else if (Coal.Count < 500 && Bulk.Count > 5 * Coal.Count) {
            if (Bulk.DisplayName == "Grain" || Bulk.DisplayName == "Cement") {
                return AppearanceSealed;
            }
            else {
                return AppearanceFull;
            }
        }
        else {
            return AppearanceSealed;
        }
    }
}
