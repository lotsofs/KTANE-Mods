﻿using UnityEngine;

public class FlatCar : TrainCar {

    public Resource Wood;
    public Resource BigStuff;
    public FreightTableRule WoodRule;
    public FreightTableRule BigStuffRule;
    [Space]
    public Sprite AppearanceLogs;
    public Sprite AppearanceLumber;
    public Sprite AppearanceTractors;
    public Sprite AppearanceTanks;
    public Sprite AppearanceWing;
    [Space]
    [SerializeField] BombHelper _bombHelper;

    /// <summary>
    /// Flatcars remove stuff and have lots of different appearances, so I hardcoded this.
    /// </summary>
    /// <param name="correct"></param>
    /// <param name="currentStage"></param>
    /// <param name="usedRule"></param>
    public override void FillCar(bool correct, int currentStage, FreightTableRule usedRule) {
        if (correct) {
            if (usedRule == WoodRule) {
                if (Wood.DisplayName == "Logs") {
                    Wood.Count -= 50;
                    return;
                }
                else {
                    Wood.Count -= 100;
                    return;
                }
            }
            else if (usedRule == BigStuffRule) {
                if (BigStuff.DisplayName == "Wings") {
                    BigStuff.Count -= 1;
                    return;
                }
                else if (BigStuff.DisplayName == "Military Hardware") {
                    BigStuff.Count -= 2;
                    return;
                }
                else {
                    BigStuff.Count -= 3;
                    return;
                }
            }
            else {
                Debug.LogWarningFormat("[Rail Cargo Loading #{0}] Flat car expected to be filled with wood or big stuff, received {1}", _bombHelper.ModuleId, usedRule.name);
            }
            return;
        }
        else if (Wood.Count <= 0 && BigStuff.Count <= 0) {
            // do not fill it up if there's nothing
            return;
        }
        else if (BigStuff.Count < 8 && Wood.Count > 10 * BigStuff.Count) {
            if (Wood.DisplayName == "Logs") {
                Wood.Count -= 50;
                if (Wood.Count < 0 && Wood.Count > -9999) {
                    Wood.Count = 0;
                }
                return;
            }
            else {
                Wood.Count -= 100;
                if (Wood.Count < 0 && Wood.Count > -9999) {
                    Wood.Count = 0;
                }
                return;
            }
        }
        else {
            if (BigStuff.DisplayName == "Wings") {
                BigStuff.Count -= 1;
                if (BigStuff.Count < 0 && BigStuff.Count > -9999) {
                    BigStuff.Count = 0;
                }
                return;
            }
            else if (BigStuff.DisplayName == "Military Hardware") {
                BigStuff.Count -= 2;
                if (BigStuff.Count < 0 && BigStuff.Count > -9999) {
                    BigStuff.Count = 0;
                }
                return;
            }
            else {
                BigStuff.Count -= 3;
                if (BigStuff.Count < 0 && BigStuff.Count > -9999) {
                    BigStuff.Count = 0;
                }
                return;
            }
        }
    }

    public override Sprite AttachCar(bool correct, int currentStage, FreightTableRule usedRule) {
        if (correct) {
            if (usedRule == WoodRule) {
                if (Wood.DisplayName == "Logs") {
                    return AppearanceLogs;
                }
                else {
                    return AppearanceLumber;
                }
            }
            else if (usedRule == BigStuffRule) {
                if (BigStuff.DisplayName == "Wings") {
                    return AppearanceWing;
                }
                else if (BigStuff.DisplayName == "Military Hardware") {
                    return AppearanceTanks;
                }
                else {
                    return AppearanceTractors;
                }
            }
            else {
                Debug.LogWarningFormat("[Railway Cargo Loading #{0}] Flat car expected to be filled with wood or big stuff, received {1}", _bombHelper.ModuleId, usedRule.name);
            }
            return Appearance;
        }
        else if (Wood.Count <= 0 && BigStuff.Count <= 0) {
            return Appearance;
        }
        else if (BigStuff.Count < 8 && Wood.Count > 10 * BigStuff.Count) {
            if (Wood.DisplayName == "Logs") {
                return AppearanceLogs;
            }
            else {
                return AppearanceLumber;
            }
        }
        else {
            if (BigStuff.DisplayName == "Wings") {
                return AppearanceWing;
            }
            else if (BigStuff.DisplayName == "Military Hardware") {
                return AppearanceTanks;
            }
            else {
                return AppearanceTractors;
            }
        }
    }
}
