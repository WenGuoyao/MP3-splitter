﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Un4seen.Bass;

namespace ColdCutsNS
{
    public class SoundSplit
    {
        /// <summary>
        /// Finds sections of silence in a sound stream.
        /// </summary>
        /// <param name="file">Filename for the sound stream.</param>
        /// <param name="block">The block of time of a sample to get the level (peak amplitude), default is 100 miliseconds.</param>
        /// <param name="minGap">The minimum time for the splits, default is 480000 miliseconds (8 mins).</param>
        /// <param name="bgWorker">The BackgroundWorker to report progress.</param>
        /// <returns>List of sound files</returns>
        public static List<SoundFile> FindSilence(string file, float block = 100, float minGap = 480000, BackgroundWorker bgWorker = null)
        {
            int level = 0;
            int count = 0;
            float gap = 0;
            float start = 0;
            float position = block;
            var buffer = new IntPtr();
            var sounds = new List<SoundFile>();

            var chan = Bass.BASS_StreamCreateFile(file, 0, 0, BASSFlag.BASS_STREAM_DECODE);
            var len = Bass.BASS_ChannelSeconds2Bytes(chan, block / (float)1000 - (float)0.02);

            while ((level = Bass.BASS_ChannelGetLevel(chan)) != -1)
            {
                int left = Utils.LowWord32(level);
                int right = Utils.HighWord32(level);
                if (((count = ((left + right) < 40000) ? count + 1 : 0) == 200) && (gap > minGap))
                {
                    var pos = (int)Math.Round(position / 1000);
                    var sound = new SoundFile($"File_{pos}", start / 1000.0, position / 1000.0);
                    bgWorker?.ReportProgress(pos, sound);
                    start = position + 1;
                    sounds.Add(sound);
                    gap = 0;
                }
                else if (position % 50000 == 0)
                {
                    bgWorker?.ReportProgress((int)Math.Round(position / 1000), null);
                }
                Bass.BASS_ChannelGetData(chan, buffer, (int)len);
                position += block;
                gap += block;
            }
            Bass.BASS_StreamFree(chan);
            return sounds;
        }
    }
}
