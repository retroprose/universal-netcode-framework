using System.Collections.Generic;
using FixedMath;
using RetroECS;

namespace GalacticMarauders
{
    /*
        The next several structs are the components of the game, they all need to be 
        copied durring a game state copy
     */

    public struct Animator
    {
        public ushort frame;
        public ushort count;
    }

    public struct Body
    {
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 size;
    }

    public struct Player
    {
        public sbyte slot;
        public ushort delayFire;
        public ushort damage;
    }

    public struct Enemy
    {
        public Cp.Handle target;
        public sbyte direction;
        public byte counter;
        public ushort delayFire;
    }

    public enum ObjType : byte
    {
        Player = 0,
        Enemy = 1,
        Bullet = 2,
        BadBullet = 3,
        Boom = 4,
        PlayerBoom = 5,
        ShotCleaner = 6
    }

    public class Cp : Components<Cp>
    {
        // constructor
        public Cp() : base() { }

        // Component ids
        static public ComponentId
            ObjectId,
            Body,
            Animator,
            Player,
            Enemy,
            Active;

        // arrays of componenets
        public Vector<byte> _ObjectId;
        public Vector<Body> _Body;
        public Vector<Player> _Player;
        public Vector<Enemy> _Enemy;
        public Vector<Animator> _Animator;

        public Ref Get(ushort i) => new Ref { index = i, comp = this, mask = Flags.None };
        public Ref Get(Handle h) => new Ref { index = h.index, comp = this, mask = Flags.None };
        public Ref Filter(Flags m) => new Ref { index = -1, comp = this, mask = m };

        public struct Ref
        {
            public int index;
            public Cp comp;
            public Flags mask;

            // main iterator function
            public bool MoveNext() => comp.MoveNext(ref index, mask);

            public Handle entity => new Handle(comp._Generation[index], (ushort)index);
            public ref Flags components => ref comp._Component[index];

            public ref byte objectId => ref comp._ObjectId[index];
            public ref Body body => ref comp._Body[index];
            public ref Animator animator => ref comp._Animator[index];
            public ref Player player => ref comp._Player[index];
            public ref Enemy enemy => ref comp._Enemy[index];
        }


        // the prefab and set prefab functions could be deleted if I just store prefabs as deactivated entities
        // however the whole prefab will only need one cache miss to bring into memory with this
        // struct!
        public class Prefab
        {
            public Flags components;
            public byte objectId;
            public Body body;
            public Animator animator;
            public Player player;
            public Enemy enemy;

            public void Set(Cp c, Handle h)
            {
                if (c.Valid(h) == true)
                {
                    if (components.Contains(Cp.Component)) c._Component[h.index] = components;
                    if (components.Contains(Cp.ObjectId)) c._ObjectId[h.index] = objectId;
                    if (components.Contains(Cp.Body)) c._Body[h.index] = body;
                    if (components.Contains(Cp.Player)) c._Player[h.index] = player;
                    if (components.Contains(Cp.Enemy)) c._Enemy[h.index] = enemy;
                    if (components.Contains(Cp.Animator)) c._Animator[h.index] = animator;
                }
            }
        }



    }

}