using System;
using System.Runtime.InteropServices;
using FixedMath;
using RetroECS;


namespace Hurkle
{



    public class ProgramVector : Vector<Program>
    {
        public PoolVector<PoolFunction> Pool;

        public ProgramVector() : base()
        {
            Pool = new PoolVector<PoolFunction>();
        }

        public void Push<T>(int index, T t, byte c = 0) where T : struct, FunctionUnion.IFunction
        {
            ref var p = ref m_list[index];
            
            p.Function.State = c;

            // previous automatically being set
            Pool.Store(ref p.Function);

            p.Function.State = 0;
            p.Function.FunctionId = FunctionUnion.Data<T>.Id;
            t.Make(ref p.Function.Union);
        }

        public void Replace<T>(int index, T t) where T : struct, FunctionUnion.IFunction
        {
            ref var p = ref m_list[index];

            p.Function.State = 0;
            p.Function.FunctionId = FunctionUnion.Data<T>.Id;
            t.Make(ref p.Function.Union);
        }

        public void Init<T>(int index, T t) where T : struct, FunctionUnion.IFunction
        {
            ref var p = ref m_list[index];
            
            p.Function.PreviousId = 0;

            p.Function.State = 0;
            p.Function.FunctionId = FunctionUnion.Data<T>.Id;
            t.Make(ref p.Function.Union);
        }

        public void Pop(int index)
        {
            ref var p = ref m_list[index];
            if (p.Function.PreviousId != 0)
            {
                Pool.Retrieve(ref p.Function);
            }
            else
            {
                p.Function.FunctionId = 0;
            }
        }

        public void Pop<T>(int index, T t) where T : struct
        {
            ref var p = ref m_list[index];
            p.Return.Set(t);
            if (p.Function.PreviousId != 0)
            {
                Pool.Retrieve(ref p.Function);
            }
            else
            {
                p.Function.FunctionId = 0;
            }
        }

    }





    public struct NoOpFunction : FunctionUnion.IFunction
    {
        public FunctionUnion.Id Id => FunctionUnion.Id.NoOpFunction;
        public void Make(ref FunctionUnion u) => u.noop = this;
        public bool Invoke(Game g, ref Cp.Ref r)
        {
            // No Operation!
            return false;
        }
    }

    public struct FibFunction : FunctionUnion.IFunction
    {
        public FunctionUnion.Id Id => FunctionUnion.Id.FibFunction;
        public void Make(ref FunctionUnion u) => u.fib = this;

        int n;
        int a;
        int b;

        public FibFunction(int N)
        {
            n = N;
            a = b = 0;
        }

        public bool Invoke(Game g, ref Cp.Ref r)
        {
            ref var p = ref r.program;
            switch (p.Function.State)
            {
                case 0:
                    //Log.Force($"running 0: {n}");
                    if (n <= 1)
                    {
                        r.Pop(n);
                        return true;
                    }
                    r.Push(new FibFunction(n - 1), 1);
                    return true;

                case 1:
                    //Log.Force($"running 1: {n}");
                    a = p.Return._Int32.Value;
                    r.Push(new FibFunction(n - 2), 2);
                    return true;

                case 2:
                    //Log.Force($"running 2: {n}");
                    b = p.Return._Int32.Value;
                    r.Pop(a + b);
                    return true;
            }
            return false;
        }
    }

    public struct RunningFunction : FunctionUnion.IFunction
    {
        public FunctionUnion.Id Id => FunctionUnion.Id.RunningFunction;
        public void Make(ref FunctionUnion u) => u.running = this;
     
        public byte Flow;

        public RunningFunction(int n)
        {
            Flow = 0;
        }

        public bool Invoke(Game g, ref Cp.Ref r)
        {
            
            Vector2 desired, steer, diff, target;
            Scaler sc, sc2;

            g.Global.Ref.enemyCount++;

            desired = Static.Const.FlowDirections[r.enemy.Flow] * 20;

            // run update logic here!

            diff = desired - r.body.velocity;
            sc = diff.Normalize();
            if (sc > (Scaler)0.4f)
            {
                sc = (Scaler)0.4f;
            }
            steer = r.body.velocity + diff * sc;

            diff = -r.body.velocity;
            sc = diff.Normalize();
            if (sc > (Scaler)0.8f)
            {
                sc = (Scaler)0.8f;
            }
            r.body.velocity = r.body.velocity + diff * sc;
            sc = (desired - steer).LengthSquared();
            sc2 = (desired - r.body.velocity).LengthSquared();
            if (sc < sc2)
            {
                r.body.velocity = steer;
            }
            
            return false;
        } 
    }

    public struct PlayerFunction : FunctionUnion.IFunction
    {
        public FunctionUnion.Id Id => FunctionUnion.Id.PlayerFunction;
        public void Make(ref FunctionUnion u) => u.play = this;

        public byte Flow;

        public PlayerFunction(int n)
        {
            Flow = 0;
        }

        public bool Invoke(Game g, ref Cp.Ref r)
        {
            Vector2 desired, steer, diff, target;
            Scaler sc, sc2;

            ref var slot = ref g.Slots[r.player.slot];

            if (slot.Connected == false)
            {
                r.player.dead = true;
            }

            if (r.player.dead == false)
            {
                g.Global.Ref.endGame = false;

                Vector2 move = new Vector2(0, 0);

                if (slot.Left) move.x = -1;
                if (slot.Right) move.x = 1;
                if (slot.Up) move.y = 1;
                if (slot.Down) move.y = -1;

                move.Normalize();

                r.body.velocity = move * 10;

                diff = new Vector2(slot.X, slot.Y);
                diff.Normalize();

                if (slot.Primary)
                {
                    if (g.Events.Append() == true)
                    {
                        g.Events.Last.CreateEntity(ObjType.Bullet, r.body.position, diff * 30);
                    }
                }

                /*
                desired = move * 10;

                // run update logic here!


                diff = desired - r.body.velocity;
                sc = diff.Normalize();
                if (sc > (Scaler)0.8f)
                {
                    sc = (Scaler)0.8f;
                }
                steer = r.body.velocity + diff * sc;

                diff = -r.body.velocity;
                sc = diff.Normalize();
                if (sc > (Scaler)1.6f)
                {
                    sc = (Scaler)1.6f;
                }
                r.body.velocity = r.body.velocity + diff * sc;
                sc = (desired - steer).LengthSquared();
                sc2 = (desired - r.body.velocity).LengthSquared();
                if (sc < sc2)
                {
                    r.body.velocity = steer;
                }
                */

                //r.body.velocity = move * 5;

                /*
                Vector2 move = new Vector2(0, 0);

                if (slot.Left)  move.x = -1;
                if (slot.Right) move.x =  1;
                //if (slot.Up)    move.y = 1;
                //if (slot.Down)  move.y = -1;

                move.Normalize();

                if (slot.Up) r.body.velocity.y += 5;

                r.body.velocity.x = move.x * 5;

                //r.body.velocity = move * 5;
                */
            }
            else
            {
                diff = -r.body.velocity;
                sc = diff.Normalize();
                if (sc > (Scaler)0.4f)
                {
                    sc = (Scaler)0.4f;
                }
                r.body.velocity = r.body.velocity + diff * sc;
            }

            slot.camera = r.body.position + new Vector2(slot.X, slot.Y) / 2;

            return false;
        }
    }

    public struct DeadFunction : FunctionUnion.IFunction
    {
        public FunctionUnion.Id Id => FunctionUnion.Id.DeadFunction;
        public void Make(ref FunctionUnion u) => u.dead = this;

        public int counter;
     
        public DeadFunction(int n)
        {
            counter = n;
        }

        public bool Invoke(Game g, ref Cp.Ref r)
        {
            Vector2 desired, steer, diff, target;
            Scaler sc, sc2;
            
            diff = -r.body.velocity;
            sc = diff.Normalize();
            if (sc > (Scaler)0.4f)
            {
                sc = (Scaler)0.4f;
            }
            r.body.velocity = r.body.velocity + diff * sc;

            counter = counter - 1;
            //Log.Force($"{r.entity.index}: {counter}");
            if (counter == 0)
            {
                if (g.Events.Append() == true)
                {
                    g.Events.Last.DestroyEntity(r.entity);
                }
            }
            
            return false;
        }
    }

    public struct PoolFunction : IPoolable
    {
        public byte State;
        public ushort FunctionId;
        public ushort PreviousId;
        public FunctionUnion Union;

        public bool Active
        {
            set { FunctionId = (ushort)((value == true) ? 1 : 0); }
            get { return FunctionId != 0; }
        }
        public int Link
        {
            set { PreviousId = (ushort)value; }
            get { return PreviousId; }
        }
    }

    public struct Program
    {
        public ReturnUnion Return;
        public PoolFunction Function;

        public FunctionUnion.Id Id => (FunctionUnion.Id)Function.FunctionId;

        public void Invoke(Game g, ref Cp.Ref r)
        {
            while (Function.Union.Invoke(Id, g, ref r) == true) { }
        }
    }


    [StructLayout(LayoutKind.Explicit)]
    public struct ReturnUnion
    {
        public static class Data<T>
        {
            public static Id Id = (Id)ContainerUtil.ComputeId(typeof(Id), typeof(T));
        }
        public enum Id : byte
        {
            _Null = 0,
            _Int32,
            _Boolean,
            _Vector2
        }
        public void Set<T>(T t) where T : struct
        {
            switch (Data<T>.Id)
            {
                case Id._Int32: _Int32 = t as Int32?; break;
                case Id._Boolean: _Boolean = t as Boolean?; break;
                case Id._Vector2: _Vector2 = t as Vector2?; break;
                default: break;
            }
        }
        [FieldOffset(0)] public int? _Int32;
        [FieldOffset(0)] public bool? _Boolean;
        [FieldOffset(0)] public Vector2? _Vector2;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FunctionUnion
    {
        public interface IFunction
        {
            Id Id { get; }
            void Make(ref FunctionUnion union);
        }
        public static class Data<T> where T : IFunction, new()
        {
            public static ushort Id = (ushort)(new T().Id);
        }
        public enum Id : ushort
        {
            InvalidFunction = 0,
            FibFunction,
            RunningFunction,
            DeadFunction,
            PlayerFunction,
            NoOpFunction
        }
        public bool Invoke(Id id, Game g, ref Cp.Ref r)
        {
            switch (id)
            {
                case Id.FibFunction: return fib.Invoke(g, ref r);
                case Id.RunningFunction: return running.Invoke(g, ref r);
                case Id.DeadFunction: return dead.Invoke(g, ref r);
                case Id.PlayerFunction: return play.Invoke(g, ref r);
                default: return false;
            }
        }
        /*
            Running State
        */
        [FieldOffset(0)] public NoOpFunction noop;
        [FieldOffset(0)] public RunningFunction running;
        [FieldOffset(0)] public DeadFunction dead;
        [FieldOffset(0)] public FibFunction fib;
        [FieldOffset(0)] public PlayerFunction play;
    }



















    public struct Enemy
    {
        public byte Flow;
    }

    public struct Fixture
    {
		static public Flag fActive = 0;
		static public Flag fSensor = 1;
        static public Flag fEmpty = 2;
        public Flags flags;
		public Vector2 position;
		public Vector2 size;
    }

	public struct Body
    {
		static public Flag fActive = 0;
		static public Flag fSweep = 1;
		public Flags flags;
		//public Vector2 displacement;
		public Vector2 velocity;
		public Vector2 position;
        public Scaler invMass;
    }
	
	public struct Tilemap
    {
		public int width;
		public int height;
		public byte[] tiles;
    }

    public struct Player
    {
        public byte slot;
        public bool dead;
    }

    public enum ObjType : ushort
    {
        Player = 0,
        Enemy = 1,
        Bullet = 2,
        BadBullet = 3,
        Boom = 4,
        PlayerBoom = 5,
        ShotCleaner = 6,
        Block = 7,
        Tilemap = 8
    }


 









    public class Cp : Components<Cp>
    {
        // constructor
        public Cp() : base() { }
       
        // Component ids
        public static ComponentId 
            ObjectId,
            Body,
            Tilemap,
            Fixture,
            Player,
            Program,
            Enemy,
            Displacement,
            Active;

        // Component data
        public Vector<ushort> _ObjectId;
        public Vector<Body> _Body;
        public Vector<Tilemap> _Tilemap;
        public Vector<Fixture> _Fixture;
        public Vector<Player> _Player;
        public Vector<Enemy> _Enemy;
        public Vector<Vector2> _Displacement;
        public ProgramVector _Program;

        // get helpers
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

            // component helpers
            public Cp.Handle entity => new Cp.Handle(comp._Generation[index], (ushort)index);
            public ref Flags components => ref comp._Component[index];
            public ref ushort objectId => ref comp._ObjectId[index];
            public ref Body body => ref comp._Body[index];
            public ref Fixture fixture => ref comp._Fixture[index];
            public ref Tilemap tilemap => ref comp._Tilemap[index];
            public ref Player player => ref comp._Player[index];
            public ref Vector2 displacement => ref comp._Displacement[index];
            public ref Enemy enemy => ref comp._Enemy[index];
            public ref Program program => ref comp._Program[index];

            // program helpers
            public void Push<T>(T t, byte c = 0) where T : struct, FunctionUnion.IFunction { comp._Program.Push<T>(index, t, c); }
            public void Replace<T>(T t) where T : struct, FunctionUnion.IFunction { comp._Program.Replace<T>(index, t); }
            public void Init<T>(T t) where T : struct, FunctionUnion.IFunction { comp._Program.Init<T>(index, t); }
            public void Pop() { comp._Program.Pop(index); }
            public void Pop<T>(T t) where T : struct { comp._Program.Pop<T>(index, t); }
        }

        // the prefab and set prefab functions could be deleted if I just store prefabs as deactivated entities
        // however the whole prefab will only need one cache miss to bring into memory with this
        // struct!
        public class Prefab
        {
            public Flags components;
            public ushort objectId;
            public Body body;
            public Fixture fixture;
            public Tilemap tilemap;
            public Player player;
            public Enemy enemy;
            public Program program;
            public Vector2 displacement;

            public void Set(Cp c, Cp.Handle h)
            {
                if (c.Valid(h) == true)
                {
                    if (components.Contains(Cp.Component)) c._Component[h.index] = components;
                    if (components.Contains(Cp.ObjectId)) c._ObjectId[h.index] = objectId;
                    if (components.Contains(Cp.Body)) c._Body[h.index] = body;
                    if (components.Contains(Cp.Body)) c._Displacement[h.index] = displacement;
                    if (components.Contains(Cp.Tilemap)) c._Tilemap[h.index] = tilemap;
                    if (components.Contains(Cp.Fixture)) c._Fixture[h.index] = fixture;
                    if (components.Contains(Cp.Player)) c._Player[h.index] = player;
                    if (components.Contains(Cp.Program)) c._Program[h.index] = program;
                    if (components.Contains(Cp.Enemy)) c._Enemy[h.index] = enemy;
                }
            }
        }       

      
    }




}