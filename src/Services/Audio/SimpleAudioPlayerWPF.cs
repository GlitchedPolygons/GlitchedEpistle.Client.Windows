/*
    Glitched Epistle - Windows Client
    Copyright (C) 2019 Raphael Beck

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using NAudio.Wave;
using Plugin.SimpleAudioPlayer;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Audio
{
    public class SimpleAudioPlayerWPF : ISimpleAudioPlayer
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;

        public event EventHandler PlaybackEnded;

        public double Duration => audioFile?.TotalTime.TotalSeconds ?? 0;

        public double CurrentPosition => audioFile?.CurrentTime.TotalSeconds ?? 0;

        public double Volume
        {
            get => outputDevice?.Volume ?? 1;
            set
            {
                if (outputDevice != null)
                {
                    outputDevice.Volume = value < 0 ? 0f : value > 1 ? 1f : (float)value;
                }
            }
        }
        public double Balance
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public bool IsPlaying => outputDevice != null ? outputDevice.PlaybackState == PlaybackState.Playing : false;

        public bool Loop { get; set; }

        public bool CanSeek => true;

        private volatile string deleteOnDispose;

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(deleteOnDispose) && File.Exists(deleteOnDispose))
            {
                File.Delete(deleteOnDispose);
            }

            outputDevice?.Stop();
            outputDevice?.Dispose();
            outputDevice = null;

            audioFile?.Dispose();
            audioFile = null;
        }

        public bool Load(Stream audioStream)
        {
            if (audioStream is null)
            {
                return false;
            }

            try
            {
                Dispose();

                string tempFile = Path.GetTempFileName();

                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                using (var fileStream = File.OpenWrite(tempFile))
                {
                    audioStream.CopyTo(fileStream);
                }

                if (Load(tempFile))
                {
                    deleteOnDispose = tempFile;
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public bool Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }

            try
            {
                Dispose();

                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;

                audioFile = new AudioFileReader(fileName);
                outputDevice.Init(audioFile);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (Loop)
            {
                Seek(0);

                if (!IsPlaying)
                {
                    Play();
                }

                return;
            }
        }

        public void Pause()
        {
            outputDevice?.Pause();
        }

        public void Play()
        {
            outputDevice?.Play();
        }

        public void Seek(double position)
        {
            if (audioFile is null)
            {
                return;
            }

            audioFile.CurrentTime = TimeSpan.FromSeconds(position);
        }

        public void Stop()
        {
            outputDevice?.Stop();
        }
    }
}
