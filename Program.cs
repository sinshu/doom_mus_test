using System;

class Program
{
    static void Main(string[] args)
    {
        using (var wad = new Wad("DOOM.WAD"))
        {
            var mus = new Mus(wad.ReadLump("D_E1M1"));
        }
    }
}
