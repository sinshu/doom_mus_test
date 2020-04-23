using System;
using System.Collections.Generic;
using AudioSynthesis;
using AudioSynthesis.Synthesis;
using NAudio.Wave;

public static class MusTest
{
    private static readonly byte[] header = new byte[]
    {
        (byte)'M',
        (byte)'U',
        (byte)'S',
        0x1A,
    };

    // Now the synthesizer seems to work, but everything is played as piano. Why???
    public static void Test(byte[] data)
    {
        var sf = 44100;
        var format = new WaveFormat(sf, 16, 2);
        var writer = new WaveFileWriter("out.wav", format);

        CheckHeader(data);

        var synth = new Synthesizer(sf, 2, sf / 140, 1);
        synth.LoadBank("TimGM6mb.sf2");
        var outBuf = new byte[synth.RawBufferSize];

        var scoreLen = (int)BitConverter.ToUInt16(data, 4);
        var scoreStart = (int)BitConverter.ToUInt16(data, 6);
        var channels = (int)BitConverter.ToUInt16(data, 8);
        var sec_channels = (int)BitConverter.ToUInt16(data, 10);
        var instrCnt = (int)BitConverter.ToUInt16(data, 12);
        var instruments = new int[instrCnt];
        for (var i = 0; i < instruments.Length; i++)
        {
            instruments[i] = (int)BitConverter.ToUInt16(data, 16 + 2 * i);
        }

        Console.WriteLine("scoreLen = " + scoreLen);
        Console.WriteLine("scoreStart = " + scoreStart);
        Console.WriteLine("channels = " + channels);
        Console.WriteLine("sec_channels = " + sec_channels);
        Console.WriteLine("instrCnt = " + instrCnt);
        for (var i = 0; i < instruments.Length; i++)
        {
            Console.WriteLine("instruments[" + i + "] = " + instruments[i]);
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();

        var totalTime = 0;

        var synthSamples = 0;

        var p = scoreStart;
        var actions = new List<Action>();
        var lastVels = new int[16];
        while (true)
        {
            var channelNumber = data[p] & 0xF;
            var eventType = (data[p] & 0x70) >> 4;
            var last = (data[p] >> 7) != 0;

            Console.WriteLine("channelNumber = " + channelNumber);
            Console.WriteLine("eventType = " + eventType);
            Console.WriteLine("last = " + last);

            p++;

            switch (eventType)
            {
                case 0:
                    var releaseNote = data[p++];

                    Console.WriteLine("    // Release note");
                    Console.WriteLine("    releaseNote = " + releaseNote);

                    actions.Add(() =>
                    {
                        var cn = channelNumber;
                        var rn = releaseNote;
                        if (cn == 15)
                        {
                            cn = 9;
                        }
                        synth.NoteOff(cn, rn);
                    });

                    break;

                case 1:
                    var playNote = data[p++];
                    var noteNumber = playNote & 127;
                    var noteVolume = (playNote & 128) != 0 ? data[p++] : -1;

                    Console.WriteLine("    // Play note");
                    Console.WriteLine("    playNote = " + playNote);
                    if (noteVolume != -1)
                    {
                        Console.WriteLine("    noteVolume = " + noteVolume);
                    }

                    actions.Add(() =>
                    {
                        var cn = channelNumber;
                        var nn = noteNumber;
                        var nv = noteVolume;
                        if (cn == 15)
                        {
                            cn = 9;
                        }
                        if (nv != -1)
                        {
                            lastVels[cn] = nv;
                        }
                        else
                        {
                            nv = lastVels[cn];
                        }
                        synth.NoteOn(cn, nn, nv);
                    });

                    break;

                case 2:
                    var pitchWheel = data[p++];

                    Console.WriteLine("    // Pitch wheel");
                    Console.WriteLine("    pitchWheel = " + pitchWheel);

                    break;

                case 3:
                    var systemEvent = data[p++];

                    Console.WriteLine("    // System event");
                    Console.WriteLine("    systemEvent = " + systemEvent);

                    Console.ReadKey();

                    break;

                case 4:
                    var controllerNumber = data[p++];
                    var controllerValue = data[p++];

                    Console.WriteLine("    // Change controller");
                    Console.WriteLine("    controllerNumber = " + controllerNumber);
                    Console.WriteLine("    controllerValue = " + controllerValue);

                    if (controllerNumber == 0)
                    {
                        actions.Add(() =>
                        {
                            var cn = channelNumber;
                            var cv = controllerValue;
                            synth.ProcessMidiMessage(cn, 0xC0, cv, 0);
                        });
                    }
                    else if (controllerNumber == 3)
                    {
                        actions.Add(() =>
                        {
                            var cn = channelNumber;
                            var cv = controllerValue;
                            synth.ProcessMidiMessage(cn, 0xB0, 0x07, cv);
                        });
                    }
                    else if (controllerNumber == 4)
                    {
                        actions.Add(() =>
                        {
                            var cn = channelNumber;
                            var cv = controllerValue;
                            synth.ProcessMidiMessage(cn, 0xB0, 0x0A, cv);
                        });
                    }
                    else
                    {
                        Console.ReadKey();
                    }

                    break;

                //case 5:
                //break;

                case 6:
                    Console.WriteLine("END!!!");
                    goto end;

                //case 7:
                //break;

                default:
                    throw new Exception("OOPS!!!");
            }

            Console.WriteLine();

            if (last)
            {
                var time = 0;
                while (true)
                {
                    var value = data[p++];
                    time = time * 128 + value & 127;
                    if ((value & 128) == 0)
                    {
                        break;
                    }
                }
                Console.WriteLine("================================================================================");
                Console.WriteLine(" TIME: " + time);
                Console.WriteLine("================================================================================");
                Console.WriteLine();

                totalTime += time;

                restart:
                var totalSec = totalTime / 140.0;
                var synthSec = (double)synthSamples / synth.SampleRate;
                if (synthSec < totalSec)
                {
                    foreach (var action in actions)
                    {
                        action();
                    }
                    actions.Clear();

                    synth.GetNext(outBuf);
                    writer.Write(outBuf, 0, outBuf.Length);
                    synthSamples += outBuf.Length / 4;

                    goto restart;
                }
            }
        }

    end:
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("TOTAL TIME: " + totalTime + " (" + (totalTime / 140.0) + " sec)");
        Console.WriteLine(p + " / " + data.Length);

        writer.Dispose();
    }

    private static void CheckHeader(byte[] data)
    {
        for (var p = 0; p < header.Length; p++)
        {
            if (data[p] != header[p])
            {
                throw new Exception("Invalid format!!!");
            }
        }
    }
}
