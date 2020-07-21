using System.Collections;
using UnityEngine;
using System;
using System.Reflection;

public class PortalRoom : MonoBehaviour
{
    public const float OFF_LEVEL = 0.025f, FADE_LEVEL = 0.1f, ON_LEVEL = 0.5f;

    public StarParticleGen StarField;

    private static Color LightsOffAmbient = new Color(0, 0, 0),
                         LightsOnAmbient  = new Color(1, 1, 1);

    public GameObject RoomLight;
    private float LightIntensity;
    public KMAudio Sound;

    private bool BombStarted = false;

    void Start() {
        LightIntensity = RoomLight.GetComponent<Light>().intensity;
        
        GetComponent<KMGameplayRoom>().OnLightChange += delegate(bool on) {
            if(BombStarted) StartCoroutine(HandlePacingLights(on));
            else if(on) {
                RegisterOneMinPacing();
                Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Switch, RoomLight.transform);
                SetAmbient(ON_LEVEL);
                BombStarted = true;
            }
            else SetAmbient(OFF_LEVEL);
        };

        SetAmbient(OFF_LEVEL);

        RegisterPacing(StarField.StarParticleEvent, "Star Particle", 0.8f, 0.2f, 300f);
    }

    private IEnumerator HandlePacingLights(bool on) {
        if(on) {
            Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.LightBuzzShort, RoomLight.transform);
            SetAmbient(FADE_LEVEL);
            yield return new WaitForSeconds(0.611f);
            SetAmbient(ON_LEVEL);
        }
        else {
            Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.LightBuzz, RoomLight.transform);
            SetAmbient(FADE_LEVEL);
            yield return new WaitForSeconds(1.57f);
            SetAmbient(OFF_LEVEL);
        }
    }

    public void SetAmbient(float amt) {
        SetAmbient(amt, Color.white);
    }

    public void SetAmbient(float amt, Color c) {
        SetAmbient(Color.Lerp(LightsOffAmbient, LightsOnAmbient, amt) * c);
        SetLightIntensity(amt);
    }

    public void SetAmbient(Color c) {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = c;
        RenderSettings.ambientIntensity = 1;
        DynamicGI.updateThreshold = 0;
        DynamicGI.UpdateEnvironment();
    }

    public void SetLightIntensity(float intensity) {
        RoomLight.GetComponent<Light>().intensity = LightIntensity * intensity;
    }

    public static Type FindType(string qualifiedTypeName) {
        Type t = Type.GetType(qualifiedTypeName);

        if(t != null) return t;
        else {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
                t = asm.GetType(qualifiedTypeName);
                if(t != null) return t;
            }
            return null;
        }
    }

    private static bool triedReflect = false;
    private static MethodInfo pacingExtenderMethod;
    private static Type pacingExtenderType;

    public void RegisterPacing(Func<IEnumerator> iter, string name, float minDiff, float weight, float cooldown) {
        if(!triedReflect) {
            try {
                triedReflect = true;
                pacingExtenderType = FindType("PacingExtender");
                pacingExtenderMethod = pacingExtenderType.GetMethod("RegisterEvent");
            }
            catch(Exception e) {
                Debug.Log("Failed to register pacing: "+name+"\n"+e.ToString());
            }
        }
        if(pacingExtenderMethod != null)
            pacingExtenderMethod.Invoke(UnityEngine.Object.FindObjectOfType(pacingExtenderType), new object[]{iter, name, minDiff, weight, cooldown});
    }

    public void RegisterOneMinPacing() {
        try {
            Type room = FindType("Room");
            IList arr = (IList)room.GetField("PacingActions").GetValue(UnityEngine.Object.FindObjectOfType(room));

            Type action = FindType("Assets.Scripts.Pacing.PacingAction");
            PropertyInfo eventField = action.GetProperty("EventType");
            object field = FindType("Assets.Scripts.Pacing.PaceEvent").GetField("OneMinuteLeft").GetValue(null);

            bool removed = false;
            foreach(object o in arr) {
                if(eventField.GetValue(o, null).Equals(field)) {
                    arr.Remove(o);
                    removed = true;
                    break;
                }
            }

            if(removed) {
                //We only add a new pacing event if we removed one, so that we don't add our own event if normal pacing events are disabled.
                object pEventObj = Activator.CreateInstance(action, new object[]{"CustomOneMin", field});
                action.GetField("Action").SetValue(pEventObj, new Action(OneMinuteEvent));
                arr.Add(pEventObj);
            }
        }
        catch(Exception e) {
            Debug.Log(e.ToString());
        }
    }

    public void OneMinuteEvent() {
        StartCoroutine(RedLight());
    }

    public IEnumerator RedLight() {
        while(true) {
            RoomLight.GetComponent<Light>().color = new Color(1f, .2f, .2f);
            Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.EmergencyAlarm, RoomLight.transform);
            yield return new WaitForSeconds(1);
            RoomLight.GetComponent<Light>().color = new Color(.5f, .5f, .5f);
            yield return new WaitForSeconds(1.25f);
        }
    }
}