using System;
using System.Collections.Generic;
using System.Diagnostics;
using RetroECS;

using System.Linq;
using System.Reflection;


namespace Games
{

    public class PassedData
    {
        // these come from the connection to server
        public Vector<bool> Connected = new Vector<bool>();
        public Vector<bool> Dropped = new Vector<bool>();
        public Vector<ulong> RawInput = new Vector<ulong>();

        // this is state that persists between games
        public Vector<bool> ConnectedAtStart = new Vector<bool>();
        public uint Seed;
        public GameId NextGame;

        public PassedData()
        {
            Connected.Resize(64);
            Dropped.Resize(64);
            RawInput.Resize(64);
            ConnectedAtStart.Resize(64);
            Seed = 0x00000000;
            NextGame = GameId.None;
        }
    }

    public struct Mask
    {
        static public Mask Make(int start, int end)
        {
            ulong mask = 0x0000000000000000;
            int shift = start;
            for (int i = start; i < end && i < 64; ++i)
            {
                mask |= ((ulong)1 << i);
            }
            return new Mask { mask = mask, shift = shift };
        }

        public ulong mask;
        public int shift;

        public ulong Decode(ulong value)
        {
            return (value & mask) >> shift;
        }
        public ulong Encode(ulong value)
        {
            return (value << shift) & mask;
        }
    }

    public struct EGAColor
    {
        public static EGAColor Index(int index) => Table[index];
        public static EGAColor[] Table = new EGAColor[]
        {
            FromArgb(  0,  0,  0),
            FromArgb(  0,  0,170),
            FromArgb(  0,170,  0),
            FromArgb(  0,170,170),
            FromArgb(170,  0,  0),
            FromArgb(170,  0,170),
            FromArgb(170, 85,  0),
            FromArgb(170,170,170),
            FromArgb( 85, 85, 85),
            FromArgb( 85, 85,255),
            FromArgb( 85,255, 85),
            FromArgb( 85,255,255),
            FromArgb(255, 85, 85),
            FromArgb(255, 85,255),
            FromArgb(255,255, 85),
            FromArgb(255,255,255)
        };
        public static EGAColor FromArgb(byte r, byte g, byte b)
        {
            return new EGAColor { r = r, g = g, b = b };
        }
        public byte r;
        public byte g;
        public byte b;
    }

    /*
        The main game state class, there will only ever be two of these
        at any time, one for the "official" state, and one for the 
        fast forwarded state. 
     */
    public static class Util
    {
        public static Mask SyncedMask = Mask.Make(55, 56);

 

    }
}

