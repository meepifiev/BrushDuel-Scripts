using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace _Project.EditorTools
{
    public static class ResultRecorder
    {
        private static RecorderController _controller;

        public static string StartRecording(string outputFileNoExt)
        {
            RecorderControllerSettings settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            settings.SetRecordModeToManual();
            settings.FrameRate = 60f;

            MovieRecorderSettings movie = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            movie.name = "ResultMovie";
            movie.Enabled = true;
            movie.EncoderSettings = new CoreEncoderSettings
            {
                Codec = CoreEncoderSettings.OutputCodec.MP4,
                EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High,
            };
            movie.ImageInputSettings = new GameViewInputSettings
            {
                OutputWidth = 1280,
                OutputHeight = 720,
            };
            movie.CaptureAudio = true;
            movie.OutputFile = outputFileNoExt;

            settings.AddRecorderSettings(movie);

            _controller = new RecorderController(settings);
            _controller.PrepareRecording();
            _controller.StartRecording();
            return "recording -> " + outputFileNoExt + ".mp4";
        }

        public static string StopRecording()
        {
            if (_controller == null)
            {
                return "no controller";
            }

            _controller.StopRecording();
            _controller = null;
            return "stopped";
        }
    }
}
