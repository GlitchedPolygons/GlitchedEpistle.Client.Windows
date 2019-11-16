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
using System.Windows.Media;
using Plugin.SimpleAudioPlayer;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Audio
{
    public class SimpleAudioPlayerWPF : ISimpleAudioPlayer
    {
        public event EventHandler PlaybackEnded;

        private MediaPlayer player;

        ///<Summary>
        /// Length of audio in seconds.
        ///</Summary>
        public double Duration
        {
            get
            {
                if (!player?.NaturalDuration.HasTimeSpan ?? false)
                {
                    return 0;
                }
                return player?.NaturalDuration.TimeSpan.TotalSeconds ?? 0;
            }
        }

        ///<Summary>
        /// Current position of audio in seconds.
        ///</Summary>
        public double CurrentPosition => player?.Position.TotalSeconds ?? 0;

        ///<Summary>
        /// Playback volume [0;1].
        ///</Summary>
        public double Volume
        {
            get => player?.Volume ?? 0;
            set => SetVolume(value, Balance);
        }

        ///<Summary>
        /// Balance left/right: -1 is 100% left : 0% right, 1 is 100% right : 0% left, 0 is equal volume left/right.
        ///</Summary>
        public double Balance
        {
            get => player?.Balance ?? 0;
            set => SetVolume(Volume, value);
        }

        private bool isPlaying;

        ///<Summary>
        /// Indicates if the currently loaded audio file is playing.
        ///</Summary>
        public bool IsPlaying
        {
            get
            {
                if (player == null)
                    return false;

                return isPlaying;
            }
        }

        ///<Summary>
        /// Continously repeats the currently playing sound.
        ///</Summary>
        public bool Loop { get; set; }

        ///<Summary>
        /// Indicates if the position of the loaded audio file can be updated.
        ///</Summary>
        public bool CanSeek => player != null;

        private volatile bool disposed;

        ///<Summary>
        /// Load wave or mp3 audio file from a stream.
        ///</Summary>
        public bool Load(Stream audioStream)
        {
            DeletePlayer();

            player = new MediaPlayer();

            if (player != null)
            {
                string fileName = Path.GetTempFileName();

                using (var fileStream = File.OpenWrite(fileName))
                {
                    audioStream.CopyTo(fileStream);
                }

                player.Open(new Uri(fileName));
                player.MediaEnded += OnPlaybackEnded;
            }

            return player != null && player.Source != null;
        }

        ///<Summary>
        /// Load wave or mp3 audio file from assets folder.
        ///</Summary>
        public bool Load(string fileName)
        {
            DeletePlayer();

            player = new MediaPlayer();

            if (player != null)
            {
                player.Open(new Uri(fileName, UriKind.Relative));
                player.MediaEnded += OnPlaybackEnded;
            }

            return player != null && player.Source != null;
        }

        private void DeletePlayer()
        {
            Stop();

            if (player != null)
            {
                if (File.Exists(player.Source.AbsoluteUri))
                {
                    File.Delete(player.Source.AbsoluteUri);
                }

                player.MediaEnded -= OnPlaybackEnded;
                player = null;
            }
        }

        private void OnPlaybackEnded(object sender, EventArgs args)
        {
            if (isPlaying && Loop)
            {
                Play();
            }

            PlaybackEnded?.Invoke(sender, args);
        }

        ///<Summary>
        /// Begin playback or resume if paused.
        ///</Summary>
        public void Play()
        {
            if (player == null || player.Source == null)
                return;

            if (IsPlaying)
            {
                Pause();
                Seek(0);
            }

            isPlaying = true;
            player.Play();
        }

        ///<Summary>
        /// Pause playback if playing (does not resume)
        ///</Summary>
        public void Pause()
        {
            isPlaying = false;
            player?.Pause();
        }

        ///<Summary>
        /// Stop playack and set the current position to the beginning
        ///</Summary>
        public void Stop()
        {
            Pause();
            Seek(0);
        }

        ///<Summary>
        /// Seek a position in seconds in the currently loaded sound file 
        ///</Summary>
        public void Seek(double position)
        {
            if (player == null) return;
            player.Position = TimeSpan.FromSeconds(position);
        }

        private void SetVolume(double volume, double balance)
        {
            if (player == null || disposed) return;

            player.Volume = Math.Min(1, Math.Max(0, volume));
            player.Balance = Math.Min(1, Math.Max(-1, balance));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed || player == null)
                return;

            if (disposing)
                DeletePlayer();

            disposed = true;
        }

        ~SimpleAudioPlayerWPF()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
