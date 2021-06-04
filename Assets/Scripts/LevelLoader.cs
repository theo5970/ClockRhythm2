using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UnityEngine;

public class LevelLoader
{
    // JSON 내용을 가지고 레벨을 불러들인다
    public static LevelData Load(string json)
    {
        LevelData result = new LevelData();
        JObject level = JObject.Parse(json);

        result.settings = ParseSettings(level);
        result.timingData.initBpm = result.settings.bpm;
        result.timingData.initSpeed = result.settings.anglePerBeat;

        ParseEvents(level, result.timingData);
        ParseNotes(level, result.timingData);

        result.timingData.CalculateEventTimes();
        result.timingData.CalculateNoteStartPositions();

        return result;
    }

    // 레벨 설정 파싱
    private static LevelSettings ParseSettings(JObject level)
    {
        JObject settings = level["settings"] as JObject;
        return new LevelSettings
        {
            musicName = settings["musicName"].Value<string>(),
            artistName = settings["artistName"].Value<string>(),
            songName = settings["songName"].Value<string>(),
            backgroundName = settings["backgroundName"].Value<string>(),
            bpm = settings["bpm"].Value<float>(),
            pitch = settings["pitch"].Value<float>(),
            offset = settings["offset"].Value<float>(),
            anglePerBeat = settings["anglePerBeat"].Value<float>()
        };
    }

    // 이벤트 파싱
    private static void ParseEvents(JObject level, TimingData timingData)
    {
        JArray events = level["events"] as JArray;
        events = new JArray(events.OrderBy(obj => ParseBeat(obj["offset"])));

        for (int i = 0; i < events.Count; i++)
        {
            JObject evt = events[i] as JObject;

            GameEvent eventToAdd = null;

            double beat = ParseBeat(evt["offset"]);
            double beatLength;

            string type = evt["type"].Value<string>();

            switch (type)
            {
                case "bpmChange":
                    double bpm = evt["bpm"].Value<double>();
                    eventToAdd = new BpmChangeEvent(beat, bpm);
                    break;
                case "speedChange":
                    double speed = evt["speed"].Value<double>();
                    eventToAdd = new SpeedChangeEvent(beat, speed);
                    break;
                case "stop":
                    beatLength = ParseBeat(evt["duration"]);
                    eventToAdd = new StopEvent(beat, beat + beatLength);
                    break;
                case "cameraMove":
                    beatLength = ParseBeat(evt["duration"]);

                    JArray jmoveOffset = evt["moveOffset"] as JArray;
                    Vector2 moveOffset = new Vector2(jmoveOffset[0].Value<float>(), jmoveOffset[1].Value<float>());
                    EasingFunc easingFunc = EasingFunction.Parse(evt["easingFunc"].Value<string>());

                    eventToAdd = new CameraMoveEvent(beat, beat + beatLength, moveOffset, easingFunc);
                    break;
                case "cameraZoom":
                    beatLength = ParseBeat(evt["duration"]);

                    float startZoom = evt["start"].Value<float>();
                    float endZoom = evt["end"].Value<float>();
                    easingFunc = EasingFunction.Parse(evt["easingFunc"].Value<string>());
                    eventToAdd = new CameraZoomEvent(beat, beat + beatLength, startZoom, endZoom, easingFunc);
                    break;
                case "cameraRotate":
                    beatLength = ParseBeat(evt["duration"]);

                    float rotateAmount = evt["amount"].Value<float>();
                    easingFunc = EasingFunction.Parse(evt["easingFunc"].Value<string>());
                    eventToAdd = new CameraRotateEvent(beat, beat + beatLength, rotateAmount, easingFunc);
                    break;
                case "tickMotion":
                    easingFunc = EasingFunction.Parse(evt["easingFunc"].Value<string>());
                    eventToAdd = new TickMotionEvent(beat, easingFunc);
                    break;

                case "chromaticAberration":
                    beatLength = ParseBeat(evt["duration"]);

                    float strength = evt["strength"].Value<float>();
                    easingFunc = EasingFunction.Parse(evt["easingFunc"].Value<string>());
                    eventToAdd = new ChromaticAberrationEvent(beat, beat + beatLength, strength, easingFunc);
                    break;

                case "clockhandParticle":
                    string particleType = evt["particle"].Value<string>();
                    bool isOn = evt["isOn"].Value<bool>();
                    bool otherOff = evt["otherOff"].Value<bool>();
                    eventToAdd = new ClockhandParticleEvent(beat, particleType, isOn, otherOff);
                    break;
            }

            if (eventToAdd != null)
            {
                timingData.AddEvent(eventToAdd);
            }
        }
    }

    // 노트 파싱
    private static void ParseNotes(JObject level, TimingData timingData)
    {
        JArray notes = level["notes"] as JArray;

        for (int i = 0; i < notes.Count; i++)
        {
            JObject noteData = notes[i] as JObject;
            double beatOffset = ParseBeat(noteData["offset"]);

            if (noteData.TryGetValue("holdLength", out JToken holdLengthToken))
            {
                // Hold Note (Deprecated)
                double holdLength = ParseBeat(holdLengthToken);
                timingData.AddEvent(new HoldNoteEvent(beatOffset, beatOffset + holdLength));
            }
            else
            {
                // Normal (Tap) Note
                timingData.AddEvent(new NoteEvent(beatOffset));
            }
        }
    }

    /* 박자 단위 파싱 (1단계)
     ex) 1/1 => 1박자   (4분음표)
     ex) 3/2 => 1.5박자 (점8분음표) */
    private static double ParseBeat(JToken token)
    {
        string text = token.Value<string>();
        double result = 0;
        if (!text.Contains('+'))
        {
            result = ParseBeatSecond(text);
        }
        else
        {
            // + 기호가 포함되어 있다면 더 분리한다.
            string[] plusSplited = text.Split('+');
            int count = plusSplited.Length;
            for (int i = 0; i < count; i++)
            {
                result += ParseBeatSecond(plusSplited[i]);
            }
        }
        return result;
    }

    // 박자 단위 파싱 (2단계)
    private static double ParseBeatSecond(string str)
    {
        string[] splited = str.Split('/');
        int numerator = Convert.ToInt32(splited[0]);
        int denominator = Convert.ToInt32(splited[1]);

        if (denominator <= 0) return 0;
        return numerator * (1d / denominator);
    }
}