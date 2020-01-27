using UnityEngine;

public class PassengerTableRule : MonoBehaviour {
    [SerializeField] BombHelper _bombHelper;
    [SerializeField] Resource[] _resources;
    [SerializeField] int _minimum;
    [SerializeField] bool _requiresLongHaul;
    [SerializeField] bool _vehicleHasToBeExclusive;
    public TrainCar Car;
    public int SameAsStage;

    private const string DebugMsgTooLittle = "[Railway Cargo Loading #{0}] Car {1}: Rule for the {2} is False because there are {3} of the required passenger type when there need to be more than {4}.";
    private const string DebugMsgCarExists = "[Railway Cargo Loading #{0}] Car {1}: Rule for the {2} is False because there already is a {2}.";
    private const string DebugMsgLongHaul = "[Railway Cargo Loading #{0}] Car {1}: Rule for the {2} is False because the train is not long-haul.";
    private const string DebugMsgTrue = "[Railway Cargo Loading #{0}] Car {1}: Rule for the {2} is True.";

    /// <summary>
    /// Evaluates whether the rule is true
    /// </summary>
    /// <param name="longHaul"></param>
    /// <param name="train"></param>
    /// <param name="stage"></param>
    /// <returns></returns>
    public bool Evaluate(bool longHaul, TrainCar[] train, int stage) {
        string carName;
        if (Car != null) {
            carName = Car.FriendlyName;
        }
        else {
            carName = string.Format("Same car as car {0}", SameAsStage);
        }
        // Does the train need to be long haul for this rule?
        if (_requiresLongHaul && !longHaul) {
            Debug.LogFormat(DebugMsgLongHaul, _bombHelper.ModuleId, stage, carName);
            return false;
        }
        // Can the vehicle already exist for this rule?
        if (_vehicleHasToBeExclusive) {
            foreach (TrainCar car in train) {
                if (car != null && Car != null && car.Type == Car.Type) {
                    Debug.LogFormat(DebugMsgCarExists, _bombHelper.ModuleId, stage, carName);
                    return false;
                }
            }
        }
        // Do we actually meet the resource requirement?
        int count = 0;
        foreach (Resource pax in _resources) {
            count += pax.Count;
        }
        if (count > _minimum) {
            Debug.LogFormat(DebugMsgTrue, _bombHelper.ModuleId, stage, carName);
            return true;
        }
        else {
            Debug.LogFormat(DebugMsgTooLittle, _bombHelper.ModuleId, stage, carName, count, _minimum);
            return false;
        }
    }
}
