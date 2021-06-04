using UnityEngine;
using System.Collections;

public class ClockhandParticleEventProcessor : EventProcessor<ClockhandParticleEvent>
{
    [System.Serializable]
    public struct ParticleInfo
    {
        public string type;
        public GameObject particleObject;
    }

    public ParticleInfo[] particleInfos;

    public override void Init()
    {
        StopAllParticles();
    }

    public override void Start()
    {
        base.Start();
        StopAllParticles();
    }

    public override void OnEventBegin(ClockhandParticleEvent evt)
    {
        InternalProcessEvent(evt);
    }

    public override void OnEventEnd(ClockhandParticleEvent evt)
    {
        InternalProcessEvent(evt);
    }

    public override void OnEventUpdate(ClockhandParticleEvent evt, float t) { }

    private void StopAllParticles()
    {
        for (int i = 0; i < particleInfos.Length; i++)
        {
            ToggleParticle(particleInfos[i].type, false);
        }
    }

    private void ToggleParticle(string type, bool isOn)
    {
        GameObject particleObject = null;
        for (int i = 0; i < particleInfos.Length; i++)
        {
            var info = particleInfos[i];
            
            if (info.type == type)
            {
                particleObject = info.particleObject;
                Debug.Log(info.type + "," + type);
            }
        }

        if (particleObject == null)
        {
            Debug.LogError("해당하는 타입의 파티클 오브젝트가 존재하지 않습니다.");
            return;
        }

        var particles = particleObject.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particles.Length; i++)
        {
            var particleSystem = particles[i];
            if (isOn)
            {
                if (!particleSystem.isPlaying) particleSystem.Play();
            }
            else
            {
                if (!particleSystem.isStopped) particleSystem.Stop();
            }
        }
    }

    private void InternalProcessEvent(ClockhandParticleEvent evt)
    {
        if (evt.otherOff)
        {
            StopAllParticles();
        }

        switch (evt.type)
        {
            case "all":
                StopAllParticles();
                break;
            default:
                ToggleParticle(evt.type, evt.isOn);
                break;
        }
    }

   
}
