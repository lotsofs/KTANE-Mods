using UnityEngine;

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
    public override Sprite FillCar(bool correct, int currentStage, FreightTableRule usedRule) {
        if (correct) {
            if (usedRule == WoodRule) {
                if (Wood.DisplayName == "Logs") {
                    Wood.Count -= 50;
                    return AppearanceLogs;
                }
                else {
                    Wood.Count -= 100;
                    return AppearanceLumber;
                }
            }
            else if (usedRule == BigStuffRule) {
                if (BigStuff.DisplayName == "Wings") {
                    BigStuff.Count -= 1;
                    return AppearanceWing;
                }
                else if (BigStuff.DisplayName == "Military Hardware") {
                    BigStuff.Count -= 2;
                    return AppearanceTanks;
                }
                else {
                    BigStuff.Count -= 3;
                    return AppearanceTractors;
                }
            }
            else {
                Debug.LogWarningFormat("[Rail Cargo Loading #{0}] Flat car expected to be filled with wood or big stuff, received {1}", _bombHelper.ModuleId, usedRule.name);
            }
            return Appearance;
        }
        else if (Wood.Count <= 0 && BigStuff.Count <= 0) {
            // do not fill it up if there's nothing
            return Appearance;
        }
        else if (BigStuff.Count < 8 && Wood.Count > 10 * BigStuff.Count) {
            if (Wood.DisplayName == "Logs") {
                Wood.Count -= 50;
                if (Wood.Count < 0) {
                    Wood.Count = 0;
                }
                return AppearanceLogs;
            }
            else {
                Wood.Count -= 100;
                if (Wood.Count < 0) {
                    Wood.Count = 0;
                }
                return AppearanceLumber;
            }
        }
        else {
            if (BigStuff.DisplayName == "Wings") {
                BigStuff.Count -= 1;
                if (BigStuff.Count < 0) {
                    BigStuff.Count = 0;
                }
                return AppearanceWing;
            }
            else if (BigStuff.DisplayName == "Military Hardware") {
                BigStuff.Count -= 2;
                if (BigStuff.Count < 0) {
                    BigStuff.Count = 0;
                }
                return AppearanceTanks;
            }
            else {
                BigStuff.Count -= 3;
                if (BigStuff.Count < 0) {
                    BigStuff.Count = 0;
                }
                return AppearanceTractors;
            }
        }
    }
}
