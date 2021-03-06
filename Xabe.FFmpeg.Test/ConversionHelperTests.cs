﻿using System;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg.Enums;
using Xunit;

namespace Xabe.FFmpeg.Test
{
    public class ConversionHelperTests

    {
        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        public async Task ToGifTest(int loopCount, int delay)
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Extensions.Gif);

            bool result = await ConversionHelper.ToGif(Resources.Mp4.FullName, output, loopCount, delay)
                                                .Start();

            Assert.True(result);
            var videoInfo = new VideoInfo(output);
            Assert.Equal(TimeSpan.FromSeconds(0), videoInfo.VideoProperties.Duration);
            Assert.Equal("gif", videoInfo.VideoProperties.VideoFormat);
            Assert.Null(videoInfo.VideoProperties.AudioFormat);
            Assert.Equal("16:9", videoInfo.VideoProperties.Ratio);
            Assert.Equal(25, videoInfo.VideoProperties.FrameRate);
            Assert.Equal(1280, videoInfo.VideoProperties.Width);
            Assert.Equal(720, videoInfo.VideoProperties.Height);
        }

        [Fact]
        public async Task AddAudio()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), Extensions.Mp4);

            bool result = await ConversionHelper.AddAudio(Resources.Mp4.FullName, Resources.Mp3.FullName, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = new VideoInfo(output);
            Assert.Equal("aac", outputInfo.VideoProperties.AudioFormat);
            Assert.Equal(TimeSpan.FromSeconds(13), outputInfo.VideoProperties.Duration);
        }

        [Fact]
        public async Task ExtractAudio()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), Extensions.Mp3);
            bool result = await ConversionHelper.ExtractAudio(Resources.Mp4WithAudio.FullName, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = new VideoInfo(output);
            Assert.Equal("mp3", outputInfo.VideoProperties.AudioFormat);
            Assert.Null(outputInfo.VideoProperties.VideoFormat);
        }

        [Fact]
        public async Task ExtractVideo()
        {
            FileInfo fileInfo = Resources.Mp4WithAudio;
            string output = Path.ChangeExtension(Path.GetTempFileName(), fileInfo.Extension);

            bool result = await ConversionHelper.ExtractVideo(fileInfo.FullName, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = new VideoInfo(output);
            Assert.Equal("h264", outputInfo.VideoProperties.VideoFormat);
            Assert.Null(outputInfo.VideoProperties.AudioFormat);
        }

        [Fact]
        public async Task JoinWith()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), Extensions.Mp4);

            bool result = await ConversionHelper.JoinWith(output, Resources.MkvWithAudio.FullName, Resources.Mp4WithAudio.FullName);

            Assert.True(result);
            var outputInfo = new VideoInfo(output);
            Assert.Equal(TimeSpan.FromSeconds(23), outputInfo.VideoProperties.Duration);
            Assert.Equal("h264", outputInfo.VideoProperties.VideoFormat);
            Assert.Equal("aac", outputInfo.VideoProperties.AudioFormat);
        }

        [Fact]
        public async Task SnapshotTest()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Extensions.Png);
            bool result = await ConversionHelper.Snapshot(Resources.Mp4WithAudio.FullName, output)
                                                .Start();

            Assert.True(result);
            Assert.True(File.Exists(output));
            Assert.Equal(1890492, (await File.ReadAllBytesAsync(output)).LongLength);
        }

        [Fact]
        public async Task SplitVideoTest()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), Extensions.Mp4);
            bool result = await ConversionHelper.Split(Resources.Mp4WithAudio.FullName, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(8), output)
                                                .Start();

            Assert.True(result);
            var outputInfo = new VideoInfo(output);
            Assert.Equal("aac", outputInfo.VideoProperties.AudioFormat);
            Assert.Equal("h264", outputInfo.VideoProperties.VideoFormat);
            Assert.Equal(TimeSpan.FromSeconds(8), outputInfo.VideoProperties.Duration);
        }

        [Fact]
        public async Task ToMp4Test()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Extensions.Mp4);

            bool result = await ConversionHelper.ToMp4(Resources.MkvWithAudio.FullName, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = new VideoInfo(output);
            Assert.Equal(TimeSpan.FromSeconds(9), outputInfo.VideoProperties.Duration);
            Assert.Equal("h264", outputInfo.VideoProperties.VideoFormat);
            Assert.Equal("aac", outputInfo.VideoProperties.AudioFormat);
        }

        [Fact]
        public async Task ToOgvTest()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Extensions.Ogv);

            bool result = await ConversionHelper.ToOgv(Resources.MkvWithAudio.FullName, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = new VideoInfo(output);
            Assert.Equal(TimeSpan.FromSeconds(9), outputInfo.VideoProperties.Duration);
            Assert.Equal("theora", outputInfo.VideoProperties.VideoFormat);
            Assert.Equal("vorbis", outputInfo.VideoProperties.AudioFormat);
        }

        [Fact]
        public async Task ToTsTest()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Extensions.Ts);

            bool result = await ConversionHelper.ToTs(Resources.Mp4WithAudio.FullName, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = new VideoInfo(output);
            Assert.Equal(TimeSpan.FromSeconds(13), outputInfo.VideoProperties.Duration);
            Assert.Equal("h264", outputInfo.VideoProperties.VideoFormat);
            Assert.Equal("aac", outputInfo.VideoProperties.AudioFormat);
        }

        [Fact]
        public async Task ToWebMTest()
        {
            string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Extensions.WebM);

            bool result = await ConversionHelper.ToWebM(Resources.Mp4WithAudio.FullName, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = new VideoInfo(output);
            Assert.Equal(TimeSpan.FromSeconds(13), outputInfo.VideoProperties.Duration);
            Assert.Equal("vp8", outputInfo.VideoProperties.VideoFormat);
            Assert.Equal("vorbis", outputInfo.VideoProperties.AudioFormat);
        }

        [Fact]
        public async Task WatermarkTest()
        {
            string output = Path.ChangeExtension(Path.GetTempFileName(), Extensions.Mp3);
            bool result = await ConversionHelper.SetWatermark(Resources.Mp4WithAudio.FullName, Resources.PngSample.FullName, Position.Center, output)
                                                .Start();

            Assert.True(result);
            var outputInfo = new VideoInfo(output);
            Assert.Equal("mp3", outputInfo.VideoProperties.AudioFormat);
            Assert.Equal("png", outputInfo.VideoProperties.VideoFormat);
        }
    }
}
