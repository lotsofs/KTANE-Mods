using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StarParticleGen : MonoBehaviour {
    public ParticleSystem sys;
    public PortalRoom room;

    public AnimationCurve FadeCurve;

    private float distance = 3f;            //The maximum distance from the camera (units)
    private float[] speed = {0.05f, 0.3f};  //The speed of the particles (multiplied by size, units per second)
    private float[] size = {0.005f, 0.03f}; //The size of the particles (units)
    private float emitDelay = 0.001f;       //The delay between each particle being created (seconds)
    private float emitDuration = 40f;       //The duration of the effect (including fade in/out, seconds)
    private float particleLifetime = 10f;   //The lifetime of each particle (seconds)
    private float particleFade = 5f;        //The duration of fade in/out (seconds)
    private float particleFadeInit = 3f;    //The duration to wait before emitting particles (seconds) - Will also prevent particles emitting at the end

    float counter = 0;
    float emitTime = 0;

    public void Start() {
        //Invoke("Trigger", 7);
        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fogDensity = 0;
    }

    public void Trigger() {
        emitTime = emitDuration;
    }

    public bool Active() {
        return emitTime > 0;
    }

    void Update() {
        if(emitTime > 0) {
            emitTime -= Time.deltaTime;
            counter += Time.deltaTime;

            float fade = emitTime;
            if(emitTime > emitDuration / 2) fade = emitDuration - emitTime;
            fade /= particleFade;
            if(fade < 1.2f) {
                if(fade > 1) fade = 1;
                if(fade < 0) fade = 0;

                room.SetAmbient(PortalRoom.ON_LEVEL-fade*(PortalRoom.ON_LEVEL-PortalRoom.FADE_LEVEL));

                fade = FadeCurve.Evaluate(fade);
                RenderSettings.fog = true;
                RenderSettings.fogDensity = Mathf.Pow(fade, 2) * 2;
            }

            if(counter > 1) counter = 1;
            while(counter > emitDelay) {
                counter -= emitDelay;
                if((emitTime > (particleFadeInit + particleLifetime)) && (emitTime < (emitDuration - particleFadeInit))) {
                    Vector3 offset = Random.insideUnitSphere * distance;

                    float strength = 1;
                    float distFromCam = offset.magnitude;
                    if(distFromCam > 1) strength = Mathf.Pow(strength / distFromCam, .5f);

                    float sizeReal = Random.Range(size[0], size[1]);
                    bool fast = Random.value > 0.995f;

                    #pragma warning disable 0618 //Ignore deprecation
                    sys.Emit(offset + transform.position,
                             Random.onUnitSphere * Random.Range(speed[0], speed[1]) * sizeReal * (fast?25:1),
                             sizeReal, particleLifetime,
                             new Color(1, 1, fast?0:1, Random.Range(0.3f, 1) * strength));
                    #pragma warning restore 0618 //Stop ignoring deprecation
                }
            }
        }
        else counter = 0;
    }

    public IEnumerator StarParticleEvent() {
        Trigger();
        yield return new WaitForSeconds(emitTime);
    }
}