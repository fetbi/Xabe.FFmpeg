﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Xabe.FFmpeg.Enums;

namespace Xabe.FFmpeg
{
    /// <inheritdoc />
    public class Conversion: IConversion
    {
        private readonly object _builderLock = new object();
        private string _audio;
        private string _audioSpeed;
        private string _bitsreamFilter;
        private string _codec;
        private string _copy;
        private string _disabled;
        private string _frameCount;
        private string _input;
        private string _loop;
        private string _output;
        private string _reverse;
        private string _rotate;
        private string _scale;
        private string _seek;
        private string _shortestInput;
        private string _size;
        private string _speed;
        private string _split;
        private string _threads;
        private string _video;
        private string _videoSpeed;
        private string _watermark;

        /// <inheritdoc />
        public string Build()
        {
            lock(_builderLock)
            {
                var builder = new StringBuilder();
                builder.Append(_input);
                builder.Append(_watermark);
                builder.Append(_scale);
                builder.Append(_video);
                builder.Append(_speed);
                builder.Append(_audio);
                builder.Append(_threads);
                builder.Append(_disabled);
                builder.Append(_size);
                builder.Append(_codec);
                builder.Append(_bitsreamFilter);
                builder.Append(_copy);
                builder.Append(_seek);
                builder.Append(_frameCount);
                builder.Append(_loop);
                builder.Append(_reverse);
                builder.Append(_rotate);
                builder.Append(_shortestInput);
                builder.Append(BuildVideoFilter());
                builder.Append(BuildAudioFilter());
                builder.Append(_split);
                builder.Append(_output);

                return builder.ToString();
            }
        }

        /// <inheritdoc cref="IConversion.OnProgress" />
        [UsedImplicitly]
        public event ConversionHandler OnProgress;

        /// <inheritdoc cref="IConversion.OnDataReceived" />
        public event DataReceivedEventHandler OnDataReceived;

        /// <inheritdoc />
        public async Task<bool> Start()
        {
            return await Start(Build());
        }

        /// <inheritdoc />
        public async Task<bool> Start(CancellationToken cancellationToken)
        {
            return await Start(Build(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> Start(string parameters)
        {
            return await Start(parameters, new CancellationToken());
        }

        /// <inheritdoc />
        public async Task<bool> Start(string parameters, CancellationToken cancellationToken)
        {
            var ffmpeg = new FFmpeg();
            ffmpeg.OnProgress += OnProgress;
            ffmpeg.OnDataReceived += OnDataReceived;
            bool processing = await ffmpeg.RunProcess(parameters, cancellationToken);
            return processing;
        }


        /// <inheritdoc />
        public void Clear()
        {
            IEnumerable<FieldInfo> fields = GetType()
                .GetFields(BindingFlags.NonPublic |
                           BindingFlags.Instance)
                .Where(x => x.FieldType == typeof(string));
            foreach(FieldInfo fieldinfo in fields)
                fieldinfo.SetValue(this, null);
        }

        /// <inheritdoc />
        public IConversion Rotate(RotateDegrees rotateDegrees)
        {
            if(rotateDegrees == RotateDegrees.Invert)
                _rotate = "-vf \"transpose=2,transpose=2\" ";
            else
                _rotate = $"-vf \"transpose={(int) rotateDegrees}\" ";
            return this;
        }

        /// <inheritdoc />
        public IConversion Split(TimeSpan startTime, TimeSpan duration)
        {
            _split = $"-ss {startTime} -t {duration} ";
            return this;
        }

        /// <inheritdoc />
        public IConversion SetSpeed(Speed speed)
        {
            _speed = $"-preset {speed.ToString() .ToLower()} ";
            return this;
        }

        /// <inheritdoc />
        public IConversion SetSpeed(int cpu)
        {
            _speed = $"-quality good -cpu-used {cpu} -deadline realtime ";
            return this;
        }

        /// <inheritdoc />
        public IConversion SetAudio(AudioCodec codec, AudioQuality bitrate)
        {
            return SetAudio(codec.ToString(), bitrate);
        }

        /// <inheritdoc />
        public IConversion SetAudio(string codec, AudioQuality bitrate)
        {
            _audio = $"-codec:a {codec.ToLower()} -b:a {(int) bitrate}k -strict experimental ";
            return this;
        }

        /// <inheritdoc />
        public IConversion SetVideo(VideoCodec codec, int bitrate = 0)
        {
            return SetVideo(codec.ToString(), bitrate);
        }

        /// <inheritdoc />
        public IConversion SetVideo(string codec, int bitrate = 0)
        {
            _video = $"-codec:v {codec.ToLower()} ";

            if(bitrate > 0)
                _video += $"-b:v {bitrate}k ";
            return this;
        }

        /// <inheritdoc />
        public IConversion UseMultiThread(bool multiThread)
        {
            string threadCount = multiThread
                ? Environment.ProcessorCount.ToString()
                : "1";

            _threads = $"-threads {threadCount} ";
            return this;
        }

        /// <inheritdoc />
        public IConversion SetInput(Uri uri)
        {
            _input = $"-i \"{uri.AbsoluteUri}\" ";
            return this;
        }

        /// <inheritdoc />
        public IConversion DisableChannel(Channel type)
        {
            switch(type)
            {
                case Channel.Video:
                    _disabled = "-vn ";
                    break;
                case Channel.Audio:
                    _disabled = "-an ";
                    break;
            }
            return this;
        }

        /// <inheritdoc />
        public IConversion SetInput(string input)
        {
            _input = $"-i \"{input}\" ";
            return this;
        }

        /// <inheritdoc />
        public IConversion SetInput(FileInfo input)
        {
            return SetInput(input.FullName);
        }

        /// <inheritdoc />
        public IConversion SetInput(params string[] inputs)
        {
            _input = "";
            foreach(string path in inputs)
                _input += $"-i \"{path}\" ";
            return this;
        }

        /// <inheritdoc />
        public IConversion SetOutput(string outputPath)
        {
            _output = $"\"{outputPath}\"";
            return this;
        }

        /// <inheritdoc />
        public IConversion SetScale(VideoSize size)
        {
            if(!string.IsNullOrWhiteSpace(size?.Resolution))
                _scale = $"-vf scale={size.Resolution} ";
            return this;
        }

        /// <inheritdoc />
        public IConversion SetSize(Size? size)
        {
            if(size.HasValue)
                _size = $"-s {size.Value.Width}x{size.Value.Height} ";

            return this;
        }

        /// <inheritdoc />
        public IConversion SetCodec(VideoCodec codec)
        {
            return SetCodec(codec.ToString());
        }

        /// <inheritdoc />
        public IConversion SetCodec(string codec)
        {
            _codec = $"-f {codec.ToLower()} ";
            return this;
        }

        /// <inheritdoc />
        public IConversion SetBitstreamFilter(Channel type, Filter filter)
        {
            return SetBitstreamFilter(type, filter.ToString());
        }

        /// <inheritdoc />
        public IConversion SetBitstreamFilter(Channel type, string filter)
        {
            switch(type)
            {
                case Channel.Audio:
                    _bitsreamFilter = $"-bsf:a {filter.ToLower()} ";
                    break;
                case Channel.Video:
                    _bitsreamFilter = $"-bsf:v {filter.ToLower()} ";
                    break;
            }
            return this;
        }

        /// <inheritdoc />
        public IConversion StreamCopy(Channel type)
        {
            switch(type)
            {
                case Channel.Audio:
                    _copy = "-c:a copy ";
                    break;
                case Channel.Video:
                    _copy = "-c:v copy ";
                    break;
                default:
                    _copy = "-c copy ";
                    break;
            }
            return this;
        }

        /// <inheritdoc />
        public IConversion SetWatermark(string imagePath, Position position)
        {
            string argument = $"-i \"{imagePath}\" -filter_complex ";
            switch(position)
            {
                case Position.Bottom:
                    argument += "\"overlay=(main_w-overlay_w)/2:main_h-overlay_h\" ";
                    break;
                case Position.Center:
                    argument += "\"overlay=x=(main_w-overlay_w)/2:y=(main_h-overlay_h)/2\" ";
                    break;
                case Position.BottomLeft:
                    argument += "\"overlay=5:main_h-overlay_h\" ";
                    break;
                case Position.UpperLeft:
                    argument += "\"overlay=5:5\" ";
                    break;
                case Position.BottomRight:
                    argument += "\"overlay=(main_w-overlay_w):main_h-overlay_h\" ";
                    break;
                case Position.UpperRight:
                    argument += "\"overlay=(main_w-overlay_w):5\" ";
                    break;
                case Position.Left:
                    argument += "\"overlay=5:(main_h-overlay_h)/2\" ";
                    break;
                case Position.Right:
                    argument += "\"overlay=(main_w-overlay_w-5):(main_h-overlay_h)/2\" ";
                    break;
                case Position.Up:
                    argument += "\"overlay=(main_w-overlay_w)/2:5\" ";
                    break;
            }
            _watermark = argument;
            return this;
        }

        /// <inheritdoc />
        public IConversion ChangeVideoSpeed(double multiplication)
        {
            _videoSpeed = $"setpts={string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:N1}", multiplication)}*PTS ";
            return this;
        }

        /// <inheritdoc />
        public IConversion ChangeAudioSpeed(double multiplication)
        {
            _audioSpeed = $"atempo={string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:N1}", multiplication)} ";
            return this;
        }

        /// <inheritdoc />
        public IConversion Reverse(Channel type)
        {
            switch(type)
            {
                case Channel.Audio:
                    _reverse = "-af areverse ";
                    break;
                case Channel.Video:
                    _reverse = "-vf reverse ";
                    break;
                case Channel.Both:
                    _reverse = "-vf reverse -af areverse ";
                    break;
            }
            return this;
        }

        /// <inheritdoc />
        public IConversion SetSeek(TimeSpan? seek)
        {
            if(seek.HasValue)
                _seek = $"-ss {seek} ";

            return this;
        }

        /// <inheritdoc />
        public IConversion SetOutputFramesCount(int number)
        {
            _frameCount = $"-frames:v {number} ";
            return this;
        }

        /// <inheritdoc />
        public IConversion SetLoop(int count, int delay)
        {
            _loop = $"-loop {count} ";
            if(delay > 0)
                _loop += $"-final_delay {delay / 100} ";
            return this;
        }

        /// <inheritdoc />
        public IConversion UseShortest(bool useShortest)
        {
            if(!useShortest)
                return this;
            _shortestInput = "-shortest ";
            return this;
        }

//        /// <inheritdoc />
//        public IConversion Concat(params string[] paths)
//        {
//            _input = $"-i \"concat:{string.Join(@"|", paths)}\" ";
//            return this;
//        }

        /// <inheritdoc />
        public IConversion Concat(params string[] paths)
        {
            string tmpFile = Path.GetTempFileName();
            File.WriteAllLines(tmpFile, paths.Select(x => $"file '{x}'"));

            _input = $"-f concat -safe 0 -i \"{tmpFile}\" -c copy ";
            return this;
        }

        private string BuildVideoFilter()
        {
            var builder = new StringBuilder();
            builder.Append("-filter:v ");
            builder.Append(_videoSpeed);

            string filter = builder.ToString();
            if(filter == "-filter:v ")
                return "";
            return filter;
        }

        private string BuildAudioFilter()
        {
            var builder = new StringBuilder();
            builder.Append("-filter:a ");
            builder.Append(_audioSpeed);

            string filter = builder.ToString();
            if(filter == "-filter:a ")
                return "";
            return filter;
        }
    }
}
