using UnityEngine;

public class FreightTableRule : MonoBehaviour {
    [SerializeField] BombHelper _bombHelper;
    [SerializeField] Resource _resource;
    [SerializeField] int _minimum;
    public TrainCar.Types Car;

    private const string DebugMsg = "[Railway Cargo Loading #{0}] Car {1}: Freight Table Rule for {2} is {3} because there's {4} of this resource when there needs to be more than {5}.";
    private const string DebugMsgMetEarlier = "[Railway Cargo Loading #{0}] Car {1}: Freight Table Rule for {2} is False because the rule has already been met by an earlier car.";

    int _metAtStage = 99;
    public int MetAtStage { get { return _metAtStage; } }
    public Resource Resource { get { return _resource; } }

    /// <summary>
    /// Resets the rule's met condition
    /// </summary>
    public void Reset() {
        _metAtStage =99;
    }

    /// <summary>
    /// Evaluate a rule found on the Freight Car Lookup Table and applies it if it's met
    /// </summary>
    /// <param name="rule"></param>
    /// <returns></returns>
    public bool Evaluate(int stage) {
        // dont evaluate the rule if this is stage 12 and it's already been met
        if (stage >= 12 && _metAtStage < stage) {
            Debug.LogFormat(DebugMsgMetEarlier, _bombHelper.ModuleId, stage, _resource.DisplayName);
            return false;
        }
        // wood rule is special, redirect
        if (_resource.Type == Resource.Types.Wood) {
            bool correct = EvaluateWood(stage);
            return correct;
        }
        // evaluate if the amount of resources is more than the minimum required resources asked for by the rule
        if (_resource.Count > _minimum) {
            _metAtStage = stage;
            Debug.LogFormat(DebugMsg, _bombHelper.ModuleId, stage, _resource.DisplayName, true, _resource.Count, _minimum);
            return true;
        }
        Debug.LogFormat(DebugMsg, _bombHelper.ModuleId, stage, _resource.DisplayName, false, _resource.Count, _minimum);
        return false;
    }

    /// <summary>
    /// Reevaluate the rule. Does not log. Does not check for stage progression. Used for correcting player mistakes.
    /// </summary>
    /// <returns></returns>
    public bool ReEvaluate() {
        // wood rule is special, redirect
        if (_resource.Type == Resource.Types.Wood) {
            bool correct = ReEvaluateWood();
            return correct;
        }
        // evaluate if the amount of resources is more than the minimum required resources asked for by the rule
        if (_resource.Count > _minimum) {
            return true;
        }
        return false;
    }

    /// <summary>
    ///  Special case for the table's lumber rule
    /// </summary>
    /// <param name="rule"></param>
    /// <returns></returns>
    bool EvaluateWood(int stage) {
        if (_resource.Count > _minimum) {
            // check if it's logs
            if (_resource.DisplayName == "Logs") {
                _metAtStage = stage;
                Debug.LogFormat(DebugMsg, _bombHelper.ModuleId, stage, _resource.DisplayName, true, _resource.Count, _minimum);
                return true;
            }
            // if it isn't, it's lumber, but the manual asks for twice the amount, so check that
            else if (_resource.Count > 2 * _minimum) {
                _metAtStage = stage;
                Debug.LogFormat(DebugMsg, _bombHelper.ModuleId, stage, _resource.DisplayName, true, _resource.Count, 2 * _minimum);
                return true;
            }
            else {
                Debug.LogFormat(DebugMsg, _bombHelper.ModuleId, stage, _resource.DisplayName, false, _resource.Count, 2 * _minimum);
                return false;
            }
        }
        Debug.LogFormat(DebugMsg, _bombHelper.ModuleId, stage, _resource.DisplayName, false, _resource.Count, _minimum);
        return false;
    }


    bool ReEvaluateWood() {
        if (_resource.Count > _minimum) {
            // check if it's logs
            if (_resource.DisplayName == "Logs") {
                return true;
            }
            // if it isn't, it's lumber, but the manual asks for twice the amount, so check that
            else if (_resource.Count > 2 * _minimum) {
                return true;
            }
            else {
                return false;
            }
        }
        return false;
    }
}