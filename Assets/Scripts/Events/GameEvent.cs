using UnityEngine;

// 게임 이벤트 (추상 클래스)
public abstract class GameEvent
{
    // 필요한 정보들 (박자, 타임 오프셋)
    public double timeOffset = 0;
    public double beatOffset = 0;
    public double endTimeOffset = 0;
    public double endBeatOffset = 0;

    // 길이가 있는 경우 길이 구하기
    public double beatLength => endBeatOffset - beatOffset;

    // 길이가 있는 이벤트인가?
    public bool isContinuous { get; private set; }

    // 이벤트시간 계산에 대한 우선순위
    public abstract int priority { get; }

    public GameEvent(double beatOffset)
    {
        this.beatOffset = beatOffset;
        isContinuous = false;
    }

    public GameEvent(double beatOffset, double endBeatOffset)
    {
        this.beatOffset = beatOffset;
        this.endBeatOffset = endBeatOffset;
        isContinuous = true;
    }

    // ToString 오버라이딩 (디버깅용 출력하기 위함)
    public override string ToString()
    {
        return string.Format("[Start:{0:F3}B ({1:F3}s) / End:{2:F3}B ({3:F3}s)]", beatOffset, timeOffset, endBeatOffset, endTimeOffset);
    }
}

public interface INoteStart
{
    double startNotePosition { get; set; }
}

public interface INoteEnd
{
    double endNotePosition { get; set; }
}

// 기본 노트 이벤트
public class NoteEvent : GameEvent, INoteStart
{
    public override int priority => 0;
    public double startNotePosition { get; set; }
    public NoteEvent(double beatOffset) : base(beatOffset) { }
}

// 롱 노트 이벤트
public class HoldNoteEvent : GameEvent, INoteStart, INoteEnd
{
    public override int priority => 0;
    public double startNotePosition { get; set; }
    public double endNotePosition { get; set; }
    public HoldNoteEvent(double startBeatOffset, double endBeatOffset) : base(startBeatOffset)
    {
        this.endBeatOffset = endBeatOffset;
    }
}

// BPM 변경 이벤트
public class BpmChangeEvent : GameEvent
{
    public override int priority => 3;
    public double newBpm;
    public BpmChangeEvent(double beatOffset, double newBpm) : base(beatOffset)
    {
        this.newBpm = newBpm;
    }
}

// 속도 변경 이벤트
public class SpeedChangeEvent : GameEvent
{
    public override int priority => 2;
    public double newSpeed;
    public SpeedChangeEvent(double beatOffset, double newSpeed) : base(beatOffset)
    {
        this.newSpeed = newSpeed;
    }
}

// 시계바늘 정지 이벤트
public class StopEvent : GameEvent
{
    public override int priority => 1;
    public StopEvent(double startBeatOffset, double endBeatOffset) : base(startBeatOffset, endBeatOffset) { }
}

// 시계바늘 모션 변경 이벤트
public class TickMotionEvent : GameEvent
{
    public override int priority => 0;
    public EasingFunc newFunction;
    public TickMotionEvent(double beatOffset, EasingFunc newFunction) : base(beatOffset)
    {
        this.newFunction = newFunction;
    }
}
// 카메라 움직임 이벤트
public class CameraMoveEvent : GameEvent
{
    public override int priority => 0;
    public Vector2 moveOffset;
    public EasingFunc easingFunc;

    public CameraMoveEvent(double beatOffset, double endBeatOffset, Vector2 offset, EasingFunc easingFunc)
        : base(beatOffset, endBeatOffset)
    {
        this.moveOffset = offset;
        this.easingFunc = easingFunc;
    }
}

// 카메라 확대/축소 이벤트
public class CameraZoomEvent : GameEvent
{
    public override int priority => 0;
    public float startZoom;
    public float endZoom;
    public EasingFunc easingFunc;

    public CameraZoomEvent(double beatOffset, double endBeatOffset, float startZoom, float endZoom, EasingFunc easingFunc)
        : base(beatOffset, endBeatOffset)
    {
        this.startZoom = startZoom;
        this.endZoom = endZoom;
        this.easingFunc = easingFunc;
    }
}

// 카메라 회전 이벤트
public class CameraRotateEvent : GameEvent
{
    public override int priority => 0;
    public float rotateAmount;
    public EasingFunc easingFunc;

    public CameraRotateEvent(double beatOffset, double endBeatOffset, float rotateAmount, EasingFunc easingFunc)
        : base(beatOffset, endBeatOffset)
    {
        this.rotateAmount = rotateAmount;
        this.easingFunc = easingFunc;
    }
}

// 카메라 색수차 이벤트
public class ChromaticAberrationEvent : GameEvent
{
    public override int priority => 0;
    public float strength;
    public EasingFunc easingFunc;

    public ChromaticAberrationEvent(double beatOffset, double endBeatOffset, float strength, EasingFunc easingFunc) : base(beatOffset, endBeatOffset)
    {
        this.strength = strength;
        this.easingFunc = easingFunc;
    }
}

// 시계바늘 파티클 변경 이벤트
public class ClockhandParticleEvent : GameEvent
{
    public string type;
    public bool isOn;
    public bool otherOff;
    public override int priority => 0;

    public ClockhandParticleEvent(double beatOffset, string type, bool isOn, bool otherOff) : base(beatOffset)
    {
        this.type = type;
        this.isOn = isOn;
        this.otherOff = otherOff;
    }
}