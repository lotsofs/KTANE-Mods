using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankCar : TrainCar {
    public Resource[] Resources;
    public FreightTableRule[] Rules;
    public int[] Counts;
    [Space]
    [SerializeField] BombHelper _bombHelper;

    /// <summary>
    /// Checks hwhether all resources are empty
    /// </summary>
    /// <returns></returns>
    bool NoResources() {
        foreach (Resource res in Resources) {
            if (res.Count > 0) {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Tanks removes lots of stuff, so I hardcoded this.
    /// </summary>
    /// <param name="correct"></param>
    /// <param name="currentStage"></param>
    /// <param name="usedRule"></param>
    public override void FillCar(bool correct, int currentStage, FreightTableRule usedRule) {
        if (correct) {
            for (int i = 0; i < Rules.Length; i++) {
                if (Rules[i] == usedRule) {
                    Resources[i].Count -= Counts[i];
                    return;
                }
            }
            Debug.LogWarningFormat("[Railway Cargo Loading #{0}] Tank car expected to be filled with something valid, received invalid resource {1}", _bombHelper.ModuleId, usedRule);
            return;
        }
        else if (NoResources()) {
            // do not fill it up if there's nothing
            return;
        }
        else {
            Resource comp = Resources[0];
            foreach (Resource res in Resources) {
                if (res.Count > comp.Count) {
                    comp = res;
                }
            }
            comp.Count -= comp.Multiplier;
            if (comp.Count < 0 && comp.Count > -9999) {
                comp.Count = 0;
            }
        }
    }

    public override Sprite AttachCar(bool correct, int currentStage, FreightTableRule usedRule) {
        return Appearance;
    }
}
