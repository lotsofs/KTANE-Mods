using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class FreightTableRule
{
    public Resource.Types type;
    public int minimum;
    public TrainLoading.TrainCars car;
}
