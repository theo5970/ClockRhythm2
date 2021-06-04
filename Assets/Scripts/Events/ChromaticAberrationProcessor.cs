using UnityEngine;

// 색수차 이펙트 처리
public class ChromaticAberrationProcessor : EventProcessor<ChromaticAberrationEvent>
{
    private ChromaticAberration effect;

    public override void Start()
    {
        base.Start();
        effect = Camera.main.GetComponent<ChromaticAberration>();
    }

    public override void Init()
    {
        effect.SetStrength(0);
    }

    public override void OnEventBegin(ChromaticAberrationEvent evt)
    {
        effect.SetStrength(evt.strength);
    }

    public override void OnEventEnd(ChromaticAberrationEvent evt)
    {
        effect.SetStrength(0);
    }

    public override void OnEventUpdate(ChromaticAberrationEvent evt, float t)
    {
        // 이펙트 강도 값을 가감속 함수로 보간
        float newStrength = evt.easingFunc(evt.strength, 0, t);
        effect.SetStrength(newStrength);
    }
}
