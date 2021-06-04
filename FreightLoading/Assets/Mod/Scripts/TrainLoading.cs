using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KModkit;
using System.Linq;
using System;
using System.IO;

public class TrainLoading : MonoBehaviour {
    const int schnabelOdds = 80;

    [SerializeField] KMBombInfo _bombInfo;
    [SerializeField] KMBombModule _bombModule;
    [SerializeField] BombHelper _bombHelper;

    [Space]

    [SerializeField] KMSelectable _upButton;
    [SerializeField] KMSelectable _downButton;
    [SerializeField] KMSelectable _okButton;

    [Space]

    [SerializeField] Resource[] _resources;
    [SerializeField] Stage[] _stages;
    [SerializeField] FreightTableRule[] _freightTable;
    [SerializeField] TrainCar[] _trainCars;

    [Space]

    [SerializeField] TrainCycler _trainCycler;
    [SerializeField] Screen _topScreen;
    [SerializeField] Screen _bottomScreen;
    [SerializeField] Note _note;

    [Space]

    [SerializeField] Resource[] _resourcesToRandomizeAtStart;
    [SerializeField] Resource[] _resourcesToRandomizeInBetween;

    TrainCar[] _train = new TrainCar[15];
    List<TrainCar> _selectableCars;

    bool _electricalWiring = false;
    bool _longHaul = false;
    bool _nuclear = false;

    int _currentStage = 0;
    TrainCar _correctCar;
    FreightTableRule _correctRule;

	#region balancing code

    /// <summary>
    /// Test Code, do not run
    /// </summary>
    void TestStart() {
        ResetModule();  
        StartModule();

        int trains = 0;
        StreamWriter sw = File.CreateText("C:\\Users\\w10-upgrade\\Desktop\\log.txt");
        while (trains < 1000) {
            string str = string.Empty;
            _currentStage++;
            while (_currentStage <= 15) {
                AddRandomResources();
                switch (_currentStage) {
                    case 1:
                        Stage1();
                        break;
                    case 2:
                        Stage2();
                        break;
                    case 3:
                        bool correct = EvaluatePassengerCarLookupTable(3);
                        if (correct) {
                            break;
                        }
                        Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: No rules from car 3 apply, treat this as car 4.", _bombHelper.ModuleId, _currentStage);
                        GenericStage(4);
                        break;
                    case 4:
                        GenericStage(4);
                        break;
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                        GenericStage(_currentStage);
                        break;
                    case 13:
                        Stage13();
                        break;
                    case 14:
                        Stage14();
                        break;
                    case 15:
                        SetCorrectCar(TrainCar.Types.Caboose);
                        break;
                }
                _correctCar.FillCar(true, _currentStage, _correctRule);
                _currentStage++;
            }
            foreach (TrainCar car in _train) {
                str += (car.FriendlyName + "\t");
            }
            str += "\n";
            sw.WriteLine(str);
            trains++;
            FullReset();
        }
        sw.Close();
        Debug.Log("Done");
    }

	#endregion

	#region start of the bomb

	/// <summary>
	/// Start is called automatically
	/// </summary>
	void Start() {
        //TestStart();
        //return;
        
        ResetModule();
        StartModule();
        _bombModule.OnActivate += ActivateModule;

        _upButton.OnInteract += delegate { _bombHelper.GenericButtonPress(_upButton, true, 0.15f); return false; };
        _okButton.OnInteract += delegate { _bombHelper.GenericButtonPress(_okButton, true, 0.25f); return false; };
        _downButton.OnInteract += delegate { _bombHelper.GenericButtonPress(_downButton, true, 0.15f); return false; };
    }

    /// <summary>
    /// Things the module needs to do before the game begins
    /// </summary>
    void StartModule() {
        _electricalWiring = _bombInfo.IsIndicatorPresent("TRN");
        _longHaul = UnityEngine.Random.Range(0f, 1f) < 0.25f;
        _nuclear = UnityEngine.Random.Range(0f, 1f) < 0.1f;

        AddInitialResources();
    }

    /// <summary>
    /// Add initial resources to get the module started.
    /// </summary>
    void AddInitialResources() {
        // Add Coal & Liquid Fuel, but only if it's a good amount. Otherwise don't bother
        Resource coal = FindResource(Resource.Types.Coal);
        int initialCoal = UnityEngine.Random.Range(0, 10);
        initialCoal = initialCoal <= 3 ? 0 : initialCoal;
        for (int i = 0; i < initialCoal; i++) {
            AddResource(coal);
        }
        Resource fuel = FindResource(Resource.Types.LiquidFuel);
        int initialFuel = UnityEngine.Random.Range(0, 10);
        initialFuel = initialFuel <= 3 ? 0 : initialFuel;
        for (int i = 0; i <= initialFuel; i++) {
            AddResource(fuel);
        }

        //Passenger generation
        AddResource(Resource.Types.PassengersNormal);
        AddResource(Resource.Types.PassengersBaggage);
        AddResource(Resource.Types.PassengersMedium);
        AddResource(Resource.Types.PassengersElite);

        //Add more random resources
        // TODO: Current balancing has balanced trains, but the concept says to add more stuff up front. This is only a 1/3 chance.
        do {
            int nextResource = UnityEngine.Random.Range(0, _resourcesToRandomizeAtStart.Length);
            AddResource(_resourcesToRandomizeAtStart[nextResource]);
        }
        while (UnityEngine.Random.Range(0, 3) >= 2);

        _note.UpdateText();
    }

    /// <summary>
    /// Reset the module to its default state, upon strike
    /// </summary>
    void ResetModule() {
        foreach (FreightTableRule rule in _freightTable) {
            rule.Reset();
        }
        for (int i = 0; i < _train.Length; i++) {
            _train[i] = null;
        }
        _currentStage = 0;
    }

    /// <summary>
    /// Hard resets the module for testing, including generated resources, longhaul & electricalwiring settings.
    /// </summary>
    void FullReset() {
        ResetModule();
        foreach (Resource resource in _resources) {
            resource.Reset();
        }

        _electricalWiring = UnityEngine.Random.Range(0f, 1f) > 0.91f;
        _longHaul = UnityEngine.Random.Range(0f, 1f) > 0.75f;

        AddInitialResources();
        ActivateModule();
    }

    /// <summary>
    /// Activate the module (lights turn on)
    /// </summary>
    void ActivateModule() {
        _topScreen.TurnOn(true);
        _bottomScreen.TurnOn(_longHaul);
        AdvanceStage();

        _upButton.OnInteract += delegate { _trainCycler.Cycle(true); return false; };
        _okButton.OnInteract += delegate { SelectCar(); return false; };
        _downButton.OnInteract += delegate { _trainCycler.Cycle(false); return false; };
    }

    /// <summary>
    /// Advances the stage, doing all the necessary steps and calculations for it.
    /// </summary>
    void AdvanceStage() {
        _currentStage++;
        if (_currentStage <= 15) {
            AddRandomResources();
            StartStage(_currentStage);
        }
        else {
            StartCoroutine(DelayedPass(17f));
            _bottomScreen.TurnOnDelay(false, 8f);
            _topScreen.ChangeDisplay(true, 1f, 1.5f, 18f);
            _trainCycler.VictoryLap(2.5f, 1f);
        }
    }

    IEnumerator DelayedPass(float delay) {
        while (delay > 0) {
            yield return null;
            delay -= Time.deltaTime;
            if (_bombInfo.GetTime() < 2f) {
                delay = 0;
            }
        }
        _currentStage = 17;
        _bombModule.HandlePass();
    }

    #endregion

    #region data lookups

    /// <summary>
    /// Checks if the previous car is a passenger car
    /// </summary>
    /// <returns></returns>
    bool PreviousCarIsPassenger() {
        TrainCar car = _train[_currentStage - 2];  // currentstage is 1 based, whereas the train is 0 based
        switch (car.Type) {
            case TrainCar.Types.BaggageCar:
            case TrainCar.Types.ClosedCoach:
            case TrainCar.Types.DiningCar:
            case TrainCar.Types.DoubleDecker:
            case TrainCar.Types.DomeCar:
            case TrainCar.Types.OpenCoach:
            case TrainCar.Types.Sleeper:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Checks if the train contains a specific car
    /// </summary>
    /// <param name="car"></param>
    /// <returns></returns>
    bool TrainContains(TrainCar.Types car) {
        for (int i = 0; i < _currentStage - 1; i++) {
            if (_train[i].Type == car) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the total amount of passengers of all types combined
    /// </summary>
    /// <returns></returns>
    int TotalPassengers() {
        return FindResource(Resource.Types.PassengersNormal).Count
            + FindResource(Resource.Types.PassengersBaggage).Count
            + FindResource(Resource.Types.PassengersMedium).Count
            + FindResource(Resource.Types.PassengersElite).Count;
    }

    /// <summary>
    /// Finds the resource of type Type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    Resource FindResource(Resource.Types type) {
        foreach (Resource resource in _resources) {
            if (resource.Type == type) {
                return resource;
            }
        }
        throw new Exception("No resource found");
    }

    /// <summary>
    /// Finds the train car of type Type
    /// </summary>
    /// <param name="Type"></param>
    /// <returns></returns>
    TrainCar FindTrainCar(TrainCar.Types Type) {
        foreach (TrainCar car in _trainCars) {
            if (car.Type == Type) {
                return car;
            }
        }
        throw new Exception("No car found");
    }

    #endregion

    #region Evaluating cars and rules

    bool EvaluatePassengerCarLookupTable(int stageNo) {
        bool correct = false;
        Stage stage = _stages[stageNo - 1];
        for (int i = 0; i < stage.PassengerTableRules.Length; i++) {
            PassengerTableRule rule = stage.PassengerTableRules[i];
            correct = rule.Evaluate(_longHaul, _train, _currentStage);
            if (correct && rule.Car != null) {
                SetCorrectCar(rule.Car);
                break;
            }
            else if (correct && rule.Car == null) {
                SetCorrectCar(_train[rule.SameAsStage - 1]);
                break;
            }
        }
        return correct;
    }

    /// <summary>
    /// Loops through each relevant rule for the current stage's freight train lookup table, and evaluates it.
    /// </summary>
    bool EvaluateFreightCarLookupTable() {
        bool correct = false;
        Stage stage = _stages[_currentStage - 1];
        for (int i = 0; i < stage.FreightTableLookupOrder.Length; i++) {
            int rule = stage.FreightTableLookupOrder[i];
            correct = _freightTable[rule].Evaluate(_currentStage);
            if (correct) {
                _correctRule = _freightTable[rule];
                SetCorrectCar(_freightTable[rule].Car);
                break;
            }
        }
        if (!correct) {
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: None of the Freight Table Rules apply.", _bombHelper.ModuleId, _currentStage);
            SetCorrectCar(TrainCar.Types.BoxCar);
            correct = true;
        }
        return correct;
    }

    #endregion

    #region stage handling

    /// <summary>
    /// Generates a list of selectable cars to cycle through
    /// </summary>
    void GenerateSelectableCars() {
        // clear queue from previous stage
        foreach (TrainCar car in _trainCars) {
            car.Prompted = false;
        }
        _selectableCars = new List<TrainCar>();
        Stage currentStage = _stages[_currentStage - 1];
        // add all the cars definitely possible in the stage, with a small chance of not
        foreach (TrainCar car in currentStage.FixedTrainCars) {
            if (UnityEngine.Random.Range(0f, 1f) >= 0.25f) {
                car.Prompted = true;
                _selectableCars.Add(car);
            }
        }
        // then, add randomly some cars that will never be possible in the stage
        foreach (TrainCar car in _trainCars) {
            if (car.Promptable && !car.Prompted && UnityEngine.Random.Range(0f, 1f) < 0.25f) {
                car.Prompted = true;
                _selectableCars.Add(car);
            }
        }
        // check whether the correct car is actually selectable, add it if not
        if (!_selectableCars.Contains(_correctCar)) {
            _correctCar.Prompted = true;
            _selectableCars.Add(_correctCar);
        }
        // last, shuffle the list
        _selectableCars = _selectableCars.Shuffle();
    }

    /// <summary>
    /// Starts a stage
    /// </summary>
    /// <param name="stage"></param>
    void StartStage(int stage) {
        _correctRule = null;
        // decide what the correct car would be

        bool correct = false;
        switch (stage) {
            case 1:
                Stage1();
                break;
            case 2:
                Stage2();
                break;
            case 3:
                correct = EvaluatePassengerCarLookupTable(3);
                if (correct) {
                    break;
                }
                Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: No rules from car 3 apply, treat this as car 4.", _bombHelper.ModuleId, _currentStage);
                GenericStage(4);
                break;
            case 4:
                GenericStage(4);
                break;
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
            case 12:
                GenericStage(stage);
                break;
            case 13:
                Stage13();
                break;
            case 14:
                Stage14();
                break;
            case 15:
                SetCorrectCar(TrainCar.Types.Caboose);
                break;
        }

        Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1} correct car: {2}!", _bombHelper.ModuleId, _currentStage, _correctCar.FriendlyName);
        // assemble list of cars to choose from
        GenerateSelectableCars();
        _trainCycler.NewStage(_selectableCars, stage);
    }

    #endregion

    #region specific stages

    /// <summary>
    /// Used for generic stages (passenger table -> freight table) (dining cars are part of the pax table code wise)
    /// </summary>
    /// <param name="stage"></param>
    void GenericStage(int stage) {
        if (!PreviousCarIsPassenger() && stage >= 5) {
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: Previous car is not a passenger car, skipping straight to freight.", _bombHelper.ModuleId, _currentStage);
            EvaluateFreightCarLookupTable();
            return;
        }
        else if (stage >= 5) {
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: Previous car is a passenger car.", _bombHelper.ModuleId, _currentStage);
        }
        bool correct = EvaluatePassengerCarLookupTable(stage);
        if (correct) {
            return;
        }
        Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: None of the passenger car rules apply. Moving to freight.", _bombHelper.ModuleId, _currentStage);
        EvaluateFreightCarLookupTable();
    }

    /// <summary>
    /// Stage 1 is special
    /// </summary>
    void Stage1() {
        if (_electricalWiring) {
            SetCorrectCar(TrainCar.Types.LocomotiveElectric);
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: A TRN indicator is present.", _bombHelper.ModuleId, _currentStage);
        }
        else if (FindResource(Resource.Types.LiquidFuel).Count > 200) {
            SetCorrectCar(TrainCar.Types.LocomotiveDiesel);
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: There is more than 200 {2} ({3}).", _bombHelper.ModuleId, _currentStage, FindResource(Resource.Types.LiquidFuel).DisplayName, FindResource(Resource.Types.LiquidFuel).Count);
        }
        else if (FindResource(Resource.Types.Coal).Count > 250) {
            SetCorrectCar(TrainCar.Types.LocomotiveCoal);
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: There is less or equal than 200 {2} ({3}).", _bombHelper.ModuleId, _currentStage, FindResource(Resource.Types.LiquidFuel).DisplayName, FindResource(Resource.Types.LiquidFuel).Count);
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: There is more than 250 {2} ({3}).", _bombHelper.ModuleId, _currentStage, FindResource(Resource.Types.Coal).DisplayName, FindResource(Resource.Types.Coal).Count);
        }
        else if (FindResource(Resource.Types.LiquidFuel).Count > FindResource(Resource.Types.Coal).Count) {
            SetCorrectCar(TrainCar.Types.LocomotiveDiesel);
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: There is more {2} than {3} ({4} vs {5}).", _bombHelper.ModuleId, _currentStage, FindResource(Resource.Types.LiquidFuel).DisplayName, FindResource(Resource.Types.Coal).DisplayName, FindResource(Resource.Types.LiquidFuel).Count, FindResource(Resource.Types.Coal).Count);
        }
        else {
            SetCorrectCar(TrainCar.Types.LocomotiveCoal);
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: There is less or equal {2} than {3} ({4} vs {5}).", _bombHelper.ModuleId, _currentStage, FindResource(Resource.Types.LiquidFuel).DisplayName, FindResource(Resource.Types.Coal).DisplayName, FindResource(Resource.Types.LiquidFuel).Count, FindResource(Resource.Types.Coal).Count);
        }
    }

    /// <summary>
    /// Stage 2 is special
    /// </summary>
    void Stage2() {
        if (_train[0].Type == TrainCar.Types.LocomotiveCoal) {
            SetCorrectCar(TrainCar.Types.Hopper);
            return;
        }
        Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: No hopper because the locomotive isn't steam powered.", _bombHelper.ModuleId, _currentStage);
        bool correct = EvaluatePassengerCarLookupTable(2);
        if (correct) {
            return;
        }
        Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: No rules from car 2 apply, treat this as car 3.", _bombHelper.ModuleId, _currentStage);
        // stage 3
        correct = EvaluatePassengerCarLookupTable(3);
        if (correct) {
            return;
        }
        Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: No rules from car 3 apply, treat this as car 4.", _bombHelper.ModuleId, _currentStage);
        GenericStage(4);
    }

    /// <summary>
    /// Stage 13 is special
    /// </summary>
    void Stage13() {
        if (PreviousCarIsPassenger()) {
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: Previous car is a passenger car.", _bombHelper.ModuleId, _currentStage);
            bool correct = EvaluatePassengerCarLookupTable(13);
            if (!correct) {
                Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: None of the passenger car rules apply. Moving to freight.", _bombHelper.ModuleId, _currentStage);
            }
            else {
                return;
            }
        }
        else {
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: Previous car is not a passenger car, skipping straight to freight.", _bombHelper.ModuleId, _currentStage);
        }
        if (FindResource(Resource.Types.Oversized).Count > 0) {
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: {2} is available, add Box Car.", _bombHelper.ModuleId, _currentStage, FindResource(Resource.Types.Oversized).DisplayName);
            SetCorrectCar(TrainCar.Types.BoxCar);
            return;
        }
        Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: No rules from car 13 apply, treat this as car 12.", _bombHelper.ModuleId, _currentStage);
        GenericStage(12);
    }

    /// <summary>
    /// Stage 14 is special
    /// </summary>
    void Stage14() {
        if (PreviousCarIsPassenger()) {
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: Previous car is a passenger car.", _bombHelper.ModuleId, _currentStage);
            bool correct = EvaluatePassengerCarLookupTable(14);
            if (!correct) {
                Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: None of the passenger car rules apply. Moving to freight.", _bombHelper.ModuleId, _currentStage);
            }
            else {
                return;
            }
        }
        else {
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: Previous car is not a passenger car, skipping straight to freight.", _bombHelper.ModuleId, _currentStage);
        }
        if (FindResource(Resource.Types.Oversized).Count > 0) {
            Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: {2} is available, add Schnabel Car.", _bombHelper.ModuleId, _currentStage, FindResource(Resource.Types.Oversized).DisplayName);
            SetCorrectCar(TrainCar.Types.SchnabelCar);
            return;
        }
        Debug.LogFormat("[Railway Cargo Loading #{0}] Car {1}: No rules from car 14 apply, treat this as car 13.", _bombHelper.ModuleId, _currentStage);
        Stage13();
    }

    #endregion

    #region resource management

    /// <summary>
    /// Adds random resources to the available resources
    /// </summary>
    void AddRandomResources() {
        if (UnityEngine.Random.Range(0, 10) >= 4) {
            //Passenger generation
            AddResource(Resource.Types.PassengersNormal);
            AddResource(Resource.Types.PassengersBaggage);
            AddResource(Resource.Types.PassengersMedium);
            AddResource(Resource.Types.PassengersElite);
        }

        // Freight generation       
        // TODO: Concept says to only SOMETIMES generate resources. A do while always generates at least one time.
        // However, as it currently is, the kinds of trains generated work quite nicely.
        // It might still be too mean on the defuser like this though.
        // See if I can change these (and only these) 6 lines of code + initial generation around without having to rebalance the entire thing
        do {
            int nextResource = UnityEngine.Random.Range(0, _resourcesToRandomizeInBetween.Length);
            AddResource(_resourcesToRandomizeInBetween[nextResource]);
        }
        while (UnityEngine.Random.Range(0, 5) >= 2);

        // Generate Schnabel Car food, maybe
        if (_currentStage <= 12 && UnityEngine.Random.Range(0, schnabelOdds) < 1) {
            Resource res = FindResource(Resource.Types.Oversized);
            res.Count = 1;
            if (_nuclear) {
                res.DisplayName = res.DisplayNames[1];
            }
            else {
                res.DisplayName = res.DisplayNames[0];
            }
        }
        _note.UpdateText();
        foreach (Resource res in _resources) {
            Debug.LogFormat("[Railway Cargo Loading #{0}] {1}: {2}.", _bombHelper.ModuleId, res.DisplayName, res.Count);
        }
    }

    /// <summary>
    /// Adds a random amount of resource type based on that resource's multiplier
    /// </summary>
    /// <param name="type"></param>
    void AddResource(Resource.Types type) {
        Resource resource = FindResource(type);
        float increase = UnityEngine.Random.Range(0f, 1f);
        increase *= resource.Multiplier;
        resource.Count += (int)increase;
    }

    /// <summary>
    /// Adds a random amount of resource based on that resource's multiplier
    /// </summary>
    /// <param name="resource"></param>
    void AddResource(Resource resource) {
        float increase = UnityEngine.Random.Range(0f, 1f);
        increase *= resource.Multiplier;
        resource.Count += (int)increase;
    }

    #endregion

    #region car selection

    /// <summary>
    /// Sets the correct answer of the current stage to the specified car
    /// </summary>
    /// <param name="car"></param>
    void SetCorrectCar(TrainCar.Types car) {
        _correctCar = FindTrainCar(car);
        _train[_currentStage - 1] = _correctCar;
    }

    /// <summary>
    /// Sets the correct answer of the current stage to the specified car
    /// </summary>
    /// <param name="car"></param>
    void SetCorrectCar(TrainCar car) {
        _correctCar = car;
        _train[_currentStage - 1] = _correctCar;
    }

    List<TrainCar> _wrongCars = new List<TrainCar>();

    /// <summary>
    /// Handles the car selected by the player, eg. evaluates it against the correct car.
    /// </summary>
    /// <param name="car"></param>
    void SelectCar() {
        int index = _trainCycler.SelectCurrent();
        if (index == -1) {
            return;
        }
        TrainCar car = _selectableCars[index];

        bool success = EvaluateCar(car);

        if (success) {
            Sprite sprite = car.AttachCar(success, _currentStage, _correctRule);
            foreach (TrainCar wrongCar in _wrongCars) {
                wrongCar.FillCar(false, _currentStage, _correctRule);
            }
            car.FillCar(true, _currentStage, _correctRule);
            _wrongCars.Clear();
            _trainCycler.MoveTrain(sprite);
            AdvanceStage();
        }
        else {
            if (_currentStage >= 12) {
                HideResourcesCarTwelve(car);
                _note.UpdateText();
            }
            Sprite sprite = car.AttachCar(false, _currentStage, _correctRule);
            _wrongCars.Add(car);
            _trainCycler.DerailTrain(sprite);
            _topScreen.TemporaryShutOff(2f, 2f);
            if (_currentStage > 2) {
                _trainCycler.ResetTrain(2.5f, 1.5f);
                _topScreen.TemporaryShutOff(4f + _currentStage * 0.9f, 1.5f, true);
            }
            //ResetModule();
            //AdvanceStage();
        }

    }

    void HideResourcesCarTwelve(TrainCar car) {
        if (_currentStage < 12) {
            // wrong stage
            return;
        }

        if (car.Type == TrainCar.Types.BoxCar) {
            // car is box car. Either the player thought there was a super large object, or the player mistook a not previously applied rule as already applied.
            // Assume the latter, just remove every resource of which there is enough but has already been applied. 

            foreach (FreightTableRule tableRule in _freightTable) {
                if (tableRule.MetAtStage < _currentStage && tableRule.ReEvaluate()) {
                    tableRule.Resource.Remove();
                }
            }
            return;
        }

        bool horriblyWrong = true;
        foreach (FreightTableRule tableRule in _freightTable) {
            if (tableRule.Car == car.Type) {
                horriblyWrong = false;
                break;
            }
        }
        if (horriblyWrong) {
            // the player connected some car that isn't even part of the freight table. Do nothing. 
            return;  
        }

        // player connected some car from the freight table.
        bool falseNegative = false;
        bool falsePositive = false;
        foreach (FreightTableRule tableRule in _freightTable) {
            if (!(tableRule.MetAtStage < _currentStage) && tableRule.Car != car.Type && !falseNegative && tableRule.ReEvaluate()) {
                // the player assumed a rule was previously met when it wasn't.
                falsePositive = true;
            }
            else if (tableRule.MetAtStage < _currentStage && tableRule.Car == car.Type) {
                // the player assumed a rule wasn't met when it was. 
                if (tableRule.ReEvaluate()) {
                    tableRule.Resource.Remove();
                }
                falseNegative = true;   // anything after this we don't need to check for anymore, as the player didn't either.
            }
        }
        if (falseNegative) {
            return;
        }
        if (falsePositive) {
            // the player thought a rule applied when it didnt. I dont know what to do, so for now Ill just remove all the incorrect rules.
            // TODO: Think of something.
            foreach (FreightTableRule tableRule in _freightTable) {
                if (tableRule.MetAtStage < _currentStage && tableRule.ReEvaluate()) {
                    tableRule.Resource.Remove();
                }
            }
        }
    }

    /// <summary>
    /// Evaluates whether the chosen car is correct, and handles accordingly
    /// </summary>
    /// <param name="car"></param>
    bool EvaluateCar(TrainCar car) {
        if (car != _correctCar) {
            Debug.LogFormat("[Railway Cargo Loading #{0}] Coupled {1} as car {2}, but should've coupled {3}!", _bombHelper.ModuleId, car.FriendlyName, _currentStage, _correctCar.FriendlyName);
            _bombModule.HandleStrike();
            return false;
        }
        else {
            Debug.LogFormat("[Railway Cargo Loading #{0}] Coupled {1} as car {2}, as supposed to.", _bombHelper.ModuleId, car.FriendlyName, _currentStage);
            Debug.LogFormat("[Railway Cargo Loading #{0}] ----------", _bombHelper.ModuleId);
            return true;
        }
    }

    #endregion

    #region twitch plays

    #pragma warning disable 414
    public readonly string TwitchHelpMessage = "Couple a railroad car with '!{0} couple [car name]'. Eg. '!{0} couple baggage car'. " +
        "Flip the note with !{0} magnet. " +
        "Alternative controls: '!{0} down', '!{0} up' & '!{0} select' to press those buttons. '!{0} cycle' to cycle through all the cars.";
    #pragma warning restore 414

    int _twitchSelectionindex = 0;

    public IEnumerator ProcessTwitchCommand(string command) {

        
        command = command.ToLowerInvariant().Trim();
        
        if (_trainCycler.Transitioning) {
            yield return "sendtochat Please wait for the train assembly crew to finish before attaching another train car.";
        }
        else if (command == "down") {
            _downButton.OnInteract();
            _twitchSelectionindex -= 1;
            _twitchSelectionindex %= _selectableCars.Count;
            yield return null;
        }
        else if (command == "up") {
            _upButton.OnInteract();
            _twitchSelectionindex += 1;
            _twitchSelectionindex %= _selectableCars.Count;
            yield return null;
        }
        else if (command == "select" || command == "submit") {
            _okButton.OnInteract();
            _twitchSelectionindex = 0;
            yield return null;
        }
        else if (command == "magnet") {
            if (_note.MagnetPressable.gameObject.activeInHierarchy) {
                _note.MagnetPressable.OnInteract();
                yield return null;
            }
            yield return "sendtochat No notes to flip";
        }
        else if (command == "cycle") {
            for (int i = 0; i < _selectableCars.Count; i++) {
                _upButton.OnInteract();
                _twitchSelectionindex += 1;
                _twitchSelectionindex %= _selectableCars.Count;
                yield return "trycancel";
                yield return new WaitForSeconds(0.5f);
            }
        }
        else {
            List<string> split = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (split[0] == "couple" || split[0] == "c" || split[0] == "add" || split[0] == "a") {
                split.RemoveAt(0);
            }

            if (split.Count > 0 && split[0] == "locomotive") {
                split.RemoveAt(0);
            }

            if (split.Count == 0) {
                yield break;
            }

            TrainCar submittedCar = null;

            string crop = split[0].Substring(0, Mathf.Min(5, split[0].Length));

            switch (crop) {
                case "elect":
                    submittedCar = FindTrainCar(TrainCar.Types.LocomotiveElectric);
                    break;
                case "steam":
                case "coal":
                    submittedCar = FindTrainCar(TrainCar.Types.LocomotiveCoal);
                    break;
                case "inter":
                case "combu":
                case "diese":
                    submittedCar = FindTrainCar(TrainCar.Types.LocomotiveDiesel);
                    break;
                case "bagga":
                    submittedCar = FindTrainCar(TrainCar.Types.BaggageCar);
                    break;
                case "close":
                    submittedCar = FindTrainCar(TrainCar.Types.ClosedCoach);
                    break;
                case "dinin":
                case "diner":
                    submittedCar = FindTrainCar(TrainCar.Types.DiningCar);
                    break;
                case "dome":
                case "domec":
                case "dome-":
                    submittedCar = FindTrainCar(TrainCar.Types.DomeCar);
                    break;
                case "doubl":
                    submittedCar = FindTrainCar(TrainCar.Types.DoubleDecker);
                    break;
                case "open":
                case "open-":
                case "openc":
                    submittedCar = FindTrainCar(TrainCar.Types.OpenCoach);
                    break;
                case "sleep":
                    submittedCar = FindTrainCar(TrainCar.Types.Sleeper);
                    break;
                case "auto":
                case "autor":
                case "auto-":
                    submittedCar = FindTrainCar(TrainCar.Types.Autorack);
                    break;
                case "box":
                case "box-c":
                case "boxca":
                    submittedCar = FindTrainCar(TrainCar.Types.BoxCar);
                    break;
                case "coil":
                case "coilc":
                case "coil-":
                    submittedCar = FindTrainCar(TrainCar.Types.CoilCar);
                    break;
                case "flat":
                case "flatc":
                case "flat-":
                    submittedCar = FindTrainCar(TrainCar.Types.FlatCar);
                    break;
                case "hoppe":
                    submittedCar = FindTrainCar(TrainCar.Types.Hopper);
                    break;
                case "refri":
                case "fridg":
                    submittedCar = FindTrainCar(TrainCar.Types.RefrigeratedWagon);
                    break;
                case "schna":
                    submittedCar = FindTrainCar(TrainCar.Types.SchnabelCar);
                    break;
                case "stock":
                case "lives":
                    submittedCar = FindTrainCar(TrainCar.Types.LivestockCar);
                    break;
                case "tank":
                case "tankc":
                case "tank-":
                    submittedCar = FindTrainCar(TrainCar.Types.TankCar);
                    break;
                case "caboo":
                    submittedCar = FindTrainCar(TrainCar.Types.Caboose);
                    break;
                case "crew":
                case "crewc":
                case "crew-":
                    submittedCar = FindTrainCar(TrainCar.Types.CrewCar);
                    break;
                case "trave":
                case "post":
                case "posta":
                case "posto":
                    submittedCar = FindTrainCar(TrainCar.Types.PostOffice);
                    break;
                default:
                    yield break;
            }

            int submittedIndex;
            submittedIndex = _selectableCars.FindIndex(car => car == submittedCar);
            for (int i = 0; i < _selectableCars.Count; i++) {
                if (_twitchSelectionindex == submittedIndex) {
                    _okButton.OnInteract();
                    _twitchSelectionindex = 0;
                    yield return null;
                    break;
                }
                _upButton.OnInteract();
                _twitchSelectionindex += 1;
                _twitchSelectionindex %= _selectableCars.Count;
                yield return new WaitForSeconds(0.15f);
            }
            if (submittedIndex == -1) {
                yield return "sendtochat That railroad car is unavailable for coupling.";
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve() {
        int correctIndex;
        while (_currentStage < 16) {
            while (_trainCycler.Transitioning) {
                yield return true;
                yield return new WaitForSeconds(0.1f);
            }
            if (_note.MagnetPressable.gameObject.activeInHierarchy == true) {
                _note.MagnetPressable.OnInteract();
                yield return new WaitForSeconds(0.3f);
            }
            correctIndex = _selectableCars.FindIndex(car => car == _correctCar);
            for (int i = 0; i < _selectableCars.Count; i++) {
                if (_twitchSelectionindex == correctIndex) {
                    _okButton.OnInteract();
                    _twitchSelectionindex = 0;
                    if (_note.MagnetPressable.gameObject.activeInHierarchy == true) {
                        _note.MagnetPressable.OnInteract();
                    }
                    break;
                }
                _upButton.OnInteract();
                _twitchSelectionindex += 1;
                _twitchSelectionindex %= _selectableCars.Count;
                yield return new WaitForSeconds(0.1f);
            }
        }
        if (_note.MagnetPressable.gameObject.activeInHierarchy == true) {
            _note.MagnetPressable.OnInteract();
        }
        while (_currentStage < 17) {
            yield return true;
        }
    }

    #endregion
}
