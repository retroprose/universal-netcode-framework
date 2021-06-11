using System.Collections.Generic;
using FixedMath;
using RetroECS;
using Games;
using System.Net.Http;

namespace Climber
{
    public struct Slot
    {
        // input masks
        static public Mask XMask = Mask.Make(0, 16);
        static public Mask YMask = Mask.Make(16, 32);
        static public Mask RightMask = Mask.Make(32, 33);
        static public Mask LeftMask = Mask.Make(33, 34);
        static public Mask UpMask = Mask.Make(34, 35);
        static public Mask DownMask = Mask.Make(35, 36);
        static public Mask PrimaryMask = Mask.Make(36, 37);
        static public Mask SecondaryMask = Mask.Make(37, 38);

        public bool Connected;
        public bool Dropped;
        public short X;
        public short Y;
        public bool Right;
        public bool Left;
        public bool Up;
        public bool Down;
        public bool Primary;
        public bool Secondary;

        public Cp.Handle tagged;
        public Vector2 camera;
        public bool Synced;
    }


    public struct GlobalState
    {
        public bool gameOver;
        public bool endGame;
        public int enemyCount;
        public Cp.Handle worldMap;
    }


    public struct Sweep
    {
        private bool _Touch;
        private Vector2 _Normal;
        private Scaler _TimeOfImpact;

        public bool Touch => _Touch;
        public Vector2 Normal => _Normal;
        public Scaler TimeOfImpact => _TimeOfImpact;

        public Sweep(Vector2 origin, Vector2 ray, Vector2 box, Vector2 size)
        {
            Scaler tmin, tmax, swap;
            Scaler x0, x1, y0, y1;
            Scaler xs, ys, ax, ay;

            _Touch = false;
            ax = -1;
            ay = -1;

            if (ray.x == 0 && ray.y == 0)
            {
                // find least out normal?
                _Normal = new Vector2(0, 0);

                tmin = 0;
                tmax = 2;

                if (origin.x < box.x - size.x ||
                    origin.x > box.x + size.x ||
                    origin.y < box.y - size.y ||
                    origin.y > box.y + size.y)
                {
                    tmax = -1;
                }
            }
            else if (ray.x == 0)
            {
                ys = 1 / ray.y;
                y0 = ((box.y - size.y) - origin.y) * ys;
                y1 = ((box.y + size.y) - origin.y) * ys;
                if (y0 > y1)
                {
                    ay = -ay;
                    swap = y0;
                    y0 = y1;
                    y1 = swap;
                }

                tmin = y0;
                tmax = y1;

                _Normal = new Vector2(0, ay);

                if (origin.x < box.x - size.x || origin.x > box.x + size.x)
                {
                    tmin = 0;
                    tmax = -1;
                }
            }
            else if (ray.y == 0)
            {
                xs = 1 / ray.x;
                x0 = ((box.x - size.x) - origin.x) * xs;
                x1 = ((box.x + size.x) - origin.x) * xs;
                if (x0 > x1)
                {
                    ax = -ax;
                    swap = x0;
                    x0 = x1;
                    x1 = swap;
                }

                tmin = x0;
                tmax = x1;

                _Normal = new Vector2(ax, 0);

                if (origin.y < box.y - size.y || origin.y > box.y + size.y)
                {
                    tmin = 0;
                    tmax = -1;
                }
            }
            else
            {
                xs = 1 / ray.x;
                x0 = ((box.x - size.x) - origin.x) * xs;
                x1 = ((box.x + size.x) - origin.x) * xs;
                if (x0 > x1)
                {
                    ax = -ax;
                    swap = x0;
                    x0 = x1;
                    x1 = swap;
                }

                ys = 1 / ray.y;
                y0 = ((box.y - size.y) - origin.y) * ys;
                y1 = ((box.y + size.y) - origin.y) * ys;
                if (y0 > y1)
                {
                    ay = -ay;
                    swap = y0;
                    y0 = y1;
                    y1 = swap;
                }

                if (y0 < x0)
                {
                    tmin = x0;
                    _Normal = new Vector2(ax, 0);
                }
                else
                {
                    tmin = y0;
                    _Normal = new Vector2(0, ay);
                }

                if (x1 < y1)
                {
                    tmax = x1;
                }
                else
                {
                    tmax = y1;
                }

                if (y0 < x0)
                    _Normal = new Vector2(ax, 0);
                else
                    _Normal = new Vector2(0, ay);
            }

            _TimeOfImpact = tmin;

            _Touch = true;
            if (tmax < 0 || tmin > 1 || tmax < tmin)
            {
                _Touch = false;
            }

        }



    }

    public struct DistanceQuery
    {
        static public Flag Up = 0;
        static public Flag Down = 1;
        static public Flag Left = 2;
        static public Flag Right = 3;
        static public Flag Touch = 4;
        static public Flag Penetration = 5;
        static public Flag BadNormal = 6;
        static public Flags All = Up | Down | Left | Right;

        public Flags flags;

        public Vector2 Axis;
        public Vector2 Over;
        public Vector2 Normal;
        public Vector2 vPoint;

        public Scaler Distance;
        public Scaler Manifold;

        public DistanceQuery(Body bA, Body bB, Fixture fA, Fixture fB)
        {
            flags = Up | Right;
            Axis = new Vector2(1, 1);
            Distance = 0;
            Manifold = 0;
            Normal = new Vector2(0, 0);
            vPoint = new Vector2(0, 0);

            var overlapSlop = Static.Const.Physics.Overlap;

            var pA = bA.position + fA.position;
            var pB = bB.position + fB.position;

            var pos = (pA + fA.size) - ((pB - fB.size) + new Vector2(overlapSlop, overlapSlop));
            var neg = ((pB + fB.size) - new Vector2(overlapSlop, overlapSlop)) - (pA - fA.size);

            Over = pos;
            if (neg.x < pos.x)
            {
                flags = flags ^ (Right | Left);
                Axis.x = -Axis.x;
                Over.x = neg.x;
            }

            if (neg.y < pos.y)
            {
                flags = flags ^ (Up | Down);
                Axis.y = -Axis.y;
                Over.y = neg.y;
            }

            if (Over.x < 0 && Over.y < 0)
            {
                flags |= BadNormal;
                if (Over.x < Over.y)
                {
                    Distance = Over.x;
                    Normal = new Vector2(Axis.x, 0);
                    flags &= ~(Up | Down);
                }
                else
                {
                    Distance = Over.y;
                    Normal = new Vector2(0, Axis.y);
                    flags &= ~(Left | Right);
                }
            }
            else
            {
                Scaler mid;
                if (Over.x < Over.y)
                {
                    Distance = Over.x;
                    Normal = new Vector2(Axis.x, 0);
                    flags &= ~(Up | Down);
                    mid = GetMid(pB.y - fB.size.y + 1, pB.y + fB.size.y - 1, pA.y - fA.size.y, pA.y + fA.size.y);
                    vPoint = new Vector2(pA.x + fA.size.x * Axis.x, mid);
                }
                else
                {
                    Distance = Over.y;
                    Normal = new Vector2(0, Axis.y);
                    flags &= ~(Left | Right);
                    mid = GetMid(pB.x - fB.size.x + 1, pB.x + fB.size.x - 1, pA.x - fA.size.x, pA.x + fA.size.x);
                    vPoint = new Vector2(mid, pA.y + fA.size.y * Axis.y);
                }
                if (Distance > -overlapSlop)
                    flags |= Touch;
                if (Distance > 0)
                    flags |= Penetration;
            }
        }

        private Scaler GetMid(Scaler A_min, Scaler A_max, Scaler B_min, Scaler B_max)
        {
            if (B_min >= A_min && B_min <= A_max)
                A_min = B_min;

            if (B_max >= A_min && B_max <= A_max)
                A_max = B_max;

            Manifold = (A_max - A_min) * ((Scaler)1 / (Scaler)2);
            return A_min + Manifold;
        }

    }

    public struct Contact
    {
        // collision delegate
        public delegate void Del(ref Contact c);

        public static uint MakeKey(ushort a, ushort b)
        {
            if (b > a)
            {
                ushort swap = a;
                a = b;
                b = swap;
            }
            return (uint)((a << 16) | b);    
        }

        public Cp Components;

        public Cp.Handle A;
        public Cp.Handle B;
        public uint Key;

        //public Sweep sweep;
        public DistanceQuery dq;
        public Scaler impulse;

        public Scaler invMassA;
        public Scaler invMassB;

        public short TileX;
        public short TileY;

        public void Recompute()
        {
            if (TileX == -1)
            {
                ref var bA = ref Components._Body[A.index];
                ref var bB = ref Components._Body[B.index];

                ref var fA = ref Components._Fixture[A.index];
                ref var fB = ref Components._Fixture[B.index];

                dq = new DistanceQuery(bA, bB, fA, fB);
            }
            else
            {
                ref var bA = ref Components._Body[A.index];
                ref var bB = ref Components._Body[B.index];

                ref var tm = ref Components._Tilemap[A.index];

                Fixture fA = Static.Const.MapFixtures[tm.tiles[TileY * tm.width + TileX]];
                ref var fB = ref Components._Fixture[B.index];

                fA.position = new Vector2(TileX * 32 + 16, TileY * 32 + 16);

                dq = new DistanceQuery(bA, bB, fA, fB);
            }
        }
        /*
        public void ComputeSweep()
        {
            if (TileX == -1)
            {
                ref var dA = ref Components._Displacement[A.index];
                ref var dB = ref Components._Displacement[B.index];

                ref var bA = ref Components._Body[A.index];
                ref var bB = ref Components._Body[B.index];

                ref var fA = ref Components._Fixture[A.index];
                ref var fB = ref Components._Fixture[B.index];

                var origin = bB.position + fB.position;
                var ray = dB - dA;
                var box = bA.position + fA.position;
                var size = fA.size + fB.size - new Vector2(Slop.Overlap, Slop.Overlap);

                sweep = new Sweep(origin, ray, box, size);
            }
            else
            {
                ref var dA = ref Components._Displacement[A.index];
                ref var dB = ref Components._Displacement[B.index];

                ref var bA = ref Components._Body[A.index];
                ref var bB = ref Components._Body[B.index];

                ref var tm = ref Components._Tilemap[A.index];

                Fixture fA = Static.Const.MapFixtures[tm.tiles[TileY * tm.width + TileX]];
                ref var fB = ref Components._Fixture[B.index];

                fA.position = new Vector2(TileX * 32 + 16, TileY * 32 + 16);

                //Log.Write($"{TileX}, {TileY} - {fA.position.x}, {fA.position.y}");

                var origin = bB.position + fB.position;
                var ray = dB - dA;
                var box = bA.position + fA.position;
                var size = fA.size + fB.size - new Vector2(Slop.Overlap, Slop.Overlap);

                sweep = new Sweep(origin, ray, box, size);
            }

        }
        */
        public Contact(uint k, Cp c, Cp.Handle a, Cp.Handle b, short tx, short ty)
        //public Contact(Body bA, Body bB, Fixture fA, Fixture fB)
        {
            /*A = Entity.Null;
            B = Entity.Null;
            Key = 0;
            Components = null;*/
            
            var overlapSlop = Static.Const.Physics.Overlap;

            A = a;
            B = b;
            Key = k;
            Components = c;

            TileX = tx;
            TileY = ty;
            impulse = 0;

            if (TileX == -1)
            {
                ref var dA = ref Components._Displacement[A.index];
                ref var dB = ref Components._Displacement[B.index];

                ref var bA = ref Components._Body[A.index];
                ref var bB = ref Components._Body[B.index];

                ref var fA = ref Components._Fixture[A.index];
                ref var fB = ref Components._Fixture[B.index];

                var origin = bB.position + fB.position;
                var ray = dB - dA;
                var box = bA.position + fA.position;
                var size = fA.size + fB.size - new Vector2(overlapSlop, overlapSlop);

                //sweep = new Sweep(origin, ray, box, size);

                dq = new DistanceQuery(bA, bB, fA, fB);

                invMassA = bA.invMass;
                invMassB = bB.invMass;
            }
            else
            {
                ref var dA = ref Components._Displacement[A.index];
                ref var dB = ref Components._Displacement[B.index];

                ref var bA = ref Components._Body[A.index];
                ref var bB = ref Components._Body[B.index];

                ref var tm = ref Components._Tilemap[A.index];

                Fixture fA = Static.Const.MapFixtures[tm.tiles[TileY * tm.width + TileX]];
                ref var fB = ref Components._Fixture[B.index];

                fA.position = new Vector2(TileX * 32 + 16, TileY * 32 + 16);

                //Log.Write($"{TileX}, {TileY} - {fA.position.x}, {fA.position.y}");

                var origin = bB.position + fB.position;
                var ray = dB - dA;
                var box = bA.position + fA.position;
                var size = fA.size + fB.size - new Vector2(overlapSlop, overlapSlop);

                //sweep = new Sweep(origin, ray, box, size);

                dq = new DistanceQuery(bA, bB, fA, fB);

                invMassA = bA.invMass;
                invMassB = bB.invMass;
            }

        }
    }

    public enum EventType
    {
        Null = 0,
        DestroyEntity,
        CreateEntity,
        Contact,
        Shoot,
        Explode,
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

        public void CreateEntity(ObjType t, Vector2 _p, Vector2 _v)
        {
            Id = EventType.CreateEntity;
            A = Cp.Handle.Null;
            B = Cp.Handle.Null;
            type = (byte)t;
            p = _p;
            v = _v;
        }

        public void Explode(Cp.Handle a, Scaler radius, Scaler power)
        {
            Id = EventType.Explode;
            A = a;
            B = Cp.Handle.Null;
            v = new Vector2(radius, power);
        }

        public EventType Id;

        public Cp.Handle A;
        public Cp.Handle B;

        public ushort Key;
        public byte type;
        public Vector2 v;
        public Vector2 p;
    }





    public struct Bounds
    {
        public ushort type;
        public Cp.Handle entity;
        public Vector2 lower;
        public Vector2 upper;

        public Bounds(Cp.Handle e, ushort t, Vector2 p, Vector2 d, Vector2 s)
        {
            var extendSlop = Static.Const.Physics.Extend;

            type = t;
            entity = e;
            lower = p - s;
            upper = p + s;

            if (d.x > 0)
            {
                upper.x += d.x;
            }
            else
            {
                lower.x += d.x;
            }

            if (d.y > 0)
            {
                upper.y += d.y;
            }
            else
            {
                lower.y += d.y;
            }

            lower -= new Vector2(extendSlop, extendSlop);
            upper += new Vector2(extendSlop, extendSlop);
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