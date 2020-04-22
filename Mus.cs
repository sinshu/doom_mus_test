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
            instruments[i] = (int)BitConverter.ToUInt16(data, 14 + 2 * i);
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
