﻿using System;
using AudioSynthesis;
using AudioSynthesis.Synthesis;
using NAudio.Wave;
using ManagedDoom.Audio;
using AudioSynthesis.Bank;

class Program
{
    static void Main(string[] args)
    {
        var synthesizer = new Synthesizer(MusDecoder.SampleRate, 2, MusDecoder.BufferLength, 1);
        var bank = new PatchBank("TimGM6mb.sf2");

        synthesizer.LoadBank(bank);
        var buffer = new byte[synthesizer.RawBufferSize];

        var format = new WaveFormat(MusDecoder.SampleRate, 16, 2);
        using (var wad = new Wad("DOOM.WAD"))
        using (var writer = new WaveFileWriter("out.wav", format))
        {
            var md = new MusDecoder(wad.ReadLump("D_E1M1"), true);
            var sampleCount = 0;
            while (true)
            {
                md.FillBuffer(synthesizer, buffer);
                writer.Write(buffer, 0, buffer.Length);
                sampleCount += MusDecoder.BufferLength;

                if (sampleCount > 240 * MusDecoder.SampleRate)
                {
                    break;
                }
            }
        }
    }
}
