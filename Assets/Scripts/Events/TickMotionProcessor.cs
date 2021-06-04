
// 시계바늘 움직임(모션) 이벤트처리
public class TickMotionProcessor : EventProcessor<TickMotionEvent>
{
    private Clockhand clockhand;
    public override void Start()
    {
        base.Start();
        clockhand = Clockhand.Instance;
    }

    public override void Init()
    {
        clockhand.SetMovingFunction(EasingFunction.Linear);
    }

    public override void OnEventBegin(TickMotionEvent evt)
    {
        clockhand.SetMovingFunction(evt.newFunction);
    }

    public override void OnEventEnd(TickMotionEvent evt)
    {
        clockhand.SetMovingFunction(evt.newFunction);
    }

    public override void OnEventUpdate(TickMotionEvent evt, float t) { } 
}
