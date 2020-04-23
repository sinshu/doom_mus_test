using System;

class Program
{
    static void Main(string[] args)
    {
        using (var wad = new Wad("DOOM.WAD"))
        {
            MusTest.Test(wad.ReadLump("D_E1M1"));
        }
    }
}
