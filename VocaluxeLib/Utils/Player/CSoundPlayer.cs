﻿#region license
// This file is part of Vocaluxe.
// 
// Vocaluxe is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Vocaluxe is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Vocaluxe. If not, see <http://www.gnu.org/licenses/>.
#endregion

namespace VocaluxeLib.Utils.Player
{
    public class CSoundPlayer
    {
        protected int _StreamID = -1;
        protected int _Volume = 100;
        protected float _StartPosition;
        protected readonly float _FadeTime = CBase.Settings.GetSoundPlayerFadeTime();

        public int Volume
        {
            set
            {
                _Volume = value;
                _ApplyVolume();
            }
            get { return _Volume; }
        }
        public bool Loop;

        public float Position
        {
            set
            {
                if (_StreamID == -1)
                    return;
                _StartPosition = value;
            }
            get
            {
                if (_StreamID == -1)
                    return -1;
                return CBase.Sound.GetPosition(_StreamID);
            }
        }

        public float Length
        {
            get
            {
                if (_StreamID == -1)
                    return 0;
                return CBase.Sound.GetLength(_StreamID);
            }
        }

        public bool RepeatSong;
        public bool IsPlaying { get; protected set; }

        public bool IsFinished
        {
            get { return !RepeatSong && CBase.Sound.IsFinished(_StreamID); }
        }

        public CSoundPlayer(bool loop = false)
        {
            Loop = loop;
        }

        public CSoundPlayer(string file, bool loop = false, float position = 0f, bool autoplay = false)
        {
            _StreamID = CBase.Sound.Load(file, false, true);
            if (position > 0f)
                Position = position;
            Loop = loop;
            if (autoplay)
                Play();
        }

        public void Load(string file, float position = 0f, bool autoplay = false)
        {
            if (IsPlaying)
                Stop();

            _StreamID = CBase.Sound.Load(file, false, true);
            if (position > 0f)
                Position = position;
            if (autoplay)
                Play();
        }

        public void Play()
        {
            if (_StreamID == -1 || IsPlaying)
                return;

            CBase.Sound.SetPosition(_StreamID, _StartPosition);
            CBase.Sound.SetStreamVolume(_StreamID, 0);
            CBase.Sound.Fade(_StreamID, Volume, _FadeTime);
            CBase.Sound.Play(_StreamID);
            IsPlaying = true;
        }

        public virtual void TogglePause()
        {
            if (_StreamID == -1)
                return;

            if (IsPlaying)
                CBase.Sound.Fade(_StreamID, 0, _FadeTime, EStreamAction.Pause);
            else
            {
                CBase.Sound.Fade(_StreamID, Volume, _FadeTime);
                CBase.Sound.Play(_StreamID);
            }

            IsPlaying = !IsPlaying;
        }

        public virtual void Stop()
        {
            if (_StreamID == -1 || !IsPlaying)
                return;

            CBase.Sound.Fade(_StreamID, 0, _FadeTime, EStreamAction.Close);
            _StreamID = -1;

            IsPlaying = false;
        }

        public void Update()
        {
            if (_StreamID == -1 || !IsPlaying)
                return;

            float len = CBase.Sound.GetLength(_StreamID);
            float timeToPlay = (len > 0f) ? len - CBase.Sound.GetPosition(_StreamID) : _FadeTime + 1f;

            bool finished = CBase.Sound.IsFinished(_StreamID);
            if (timeToPlay <= _FadeTime || finished)
            {
                if (RepeatSong)
                {
                    //Set to false for restarting
                    IsPlaying = false;
                    Play();
                }
                else
                    Stop();
            }
        }

        private void _ApplyVolume()
        {
            if (_StreamID == -1)
                return;

            CBase.Sound.SetStreamVolume(_StreamID, Volume);
        }
    }
}