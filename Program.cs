using System;

class Program
{
    static void Main(string[] args)
    {
        using (var wad = new Wad("DOOM2.WAD"))
        {
            MusTest.Test(wad.ReadLump("D_MESSAG"));
        }
    }
}
