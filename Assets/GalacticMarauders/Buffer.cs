using System.Collections.Generic;
using FixedMath;
using RetroECS;

namespace GalacticMarauders
{
    /*
        These two structs define the controls for this specific game and
        the slot that holds information about the state of the player

        These 3 are state!
    */
    public struct Control
    {
        // 3 bits state - 0 is unready, 1+ difficulty, 7 synced
        // non empty 1 bit
        // left, right, primary 3 bits
        public byte State;

        public short X;

        // 10 bits, 6 left over
        public bool NonEmpty;

        public bool Left;
        public bool Right;

        public bool Primary;

        public uint Debug;
    }

    public struct Slot
    {
        public bool Connected;
        //public bool Broken;
        public Control Input;
    }

    public struct GlobalState
    {
        public bool playing;

        public Scaler enemySpeed;
        public int enemyCount;

        public ushort textType;
        public Scaler textAnimate;
    }

    /*
        The next few structs define the intermediate buffers used
        to calculate the next frame, they do not need to be
        copied on a game state copy

        These 3 are buffer!
     */
    public enum EventType
    {
        Null = 0,
        DestroyEntity,
        CreateEntity,
        Contact,
        Shoot,
        Count
    }

    public struct Event
    {
        public void DestroyEntity(Cp.Handle a)
        {
            Id = EventType.DestroyEntity;
            A = a;
            B = Cp.Handle.Null;
            Key = 0;
            type = 0;
            v = new Vector2(0, 0);
        }

        public void Contact(ushort k, Cp.Handle a, Cp.Handle b)
        {
            Id = EventType.Contact;
            A = a;
            B = b;
            Key = k;
        }

        public void CreatePlayer(ushort d, Vector2 p)
        {
            Id = EventType.CreateEntity;
            A = Cp.Handle.Null;
            B = Cp.Handle.Null;
            type = (byte)ObjType.Player;
            Key = d;
            v = p;
        }

        public void CreateEntity(ObjType t, Vector2 p)
        {
            Id = EventType.CreateEntity;
            A = Cp.Handle.Null;
            B = Cp.Handle.Null;
            type = (byte)t;
            v = p;
        }

        public EventType Id;

        public Cp.Handle A;
        public Cp.Handle B;

        public ushort Key;
        public byte type;
        public Vector2 v;
    }


    public struct Bounds
    {
        public byte type;
        public Cp.Handle entity;
        public Vector2 lower;
        public Vector2 upper;

        public Bounds(Cp.Handle e, byte t, Vector2 p, Vector2 d, Vector2 s)
        {
            type = t;
            entity = e;
            lower = p - s;
            upper = p + s;
        }

        public bool Overlap(ref Bounds b)
        {
            if (b.lower.x > upper.x || b.upper.x < lower.x ||
                b.lower.y > upper.y || b.upper.y < lower.y)
            {
                return false;
            }
            return true;
        }
    }

}