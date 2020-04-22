using System;

public class Mus
{
    private static readonly byte[] header = new byte[]
    {
        (byte)'M',
        (byte)'U',
        (byte)'S',
        0x1A,
    };

    public Mus(byte[] data)
    {
        CheckHeader(data);

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

        var p = scoreStart;
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
                    break;
                case 4:
                    var controllerNumber = data[p++];
                    var controllerValue = data[p++];
                    Console.WriteLine("    // Change controller");
                    Console.WriteLine("    controllerNumber = " + controllerNumber);
                    Console.WriteLine("    controllerValue = " + controllerValue);
                    break;
                case 5:
                    break;
                case 6:
                    Console.WriteLine("END!!!");
                    goto end;
                case 7:
                    break;
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
            }
        }

    end:
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("TOTAL TIME: " + totalTime + " (" + (totalTime / 140.0) + " sec)");
        Console.WriteLine(p + " / " + data.Length);
    }

    private void CheckHeader(byte[] data)
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
