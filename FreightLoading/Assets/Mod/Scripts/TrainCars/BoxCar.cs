using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxCar : TrainCar {

    public List<Resource> Resources;
    public int Capacity;

    /// <summary>
    /// Boxcars removes lots of stuff, so I hardcoded this.
    /// </summary>
    /// <param name="correct"></param>
    /// <param name="currentStage"></param>
    /// <param name="usedRule"></param>
    public override void FillCar(bool correct, int currentStage, FreightTableRule usedRule) {
        // get a list of the resources and remove any resources that are 0
        List<Resource> list = new List<Resource>(Resources);
        for (int i = list.Count - 1; i >= 0; i--) {
            if (list[i].Count <= 0) {
                list.RemoveAt(i);
            }
        }
        // if the list is empty, there's no resources, so we're done
        if (list.Count == 0) {
            return;
        }
        // randomly remove a resource to place in the box car, while performing the above check
        for (int i = 0; i < Capacity; i++) {
            int j = Random.Range(0, list.Count);
            list[j].Count -= 1;
            if (list[j].Count <= 0) {
                list.RemoveAt(j);
            }
            if (list.Count == 0) {
                return;
            }
        }
    }

    public override Sprite AttachCar(bool correct, int currentStage, FreightTableRule usedRule) {
        return Appearance;
    }
}
