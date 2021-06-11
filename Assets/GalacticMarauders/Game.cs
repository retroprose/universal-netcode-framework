using System.Collections.Generic;
using FixedMath;
using RetroECS;
using Games;

namespace GalacticMarauders
{
    /*
        The main game state class, there will only ever be two of these
        at any time, one for the "official" state, and one for the 
        fast forwarded state. 
     */
    public class Game
    {
        // this is the only reason for the "using Games;" inclusion at the start, it identifies which bits are which
        static public Mask XMask = Mask.Make(0, 16);
        static public Mask LeftMask = Mask.Make(16, 17);
        static public Mask RightMask = Mask.Make(17, 18);
        static public Mask PrimaryMask = Mask.Make(18, 19);
   
        // I don't really utilize the websocket events since I simply render the scene from scratch every frame.
        // delegates for websocket events
        public delegate void MessageDel(string msg);
        public delegate void EntityDel(Cp.Handle e);

        // events for websocket
        public event MessageDel OnMessage;
        public event EntityDel OnCreate;
        public event EntityDel OnDestroy;

        // delegate for collision messages
        public delegate void CollisionDel(Game g, ref Event e);

        // collision table
        public Dictionary<ushort, CollisionDel> CollisionTable = new Dictionary<ushort, CollisionDel>();
        public Dictionary<ushort, ushort> AnimationTable = new Dictionary<ushort, ushort>();

        // static game data
        public Data Const; // loadded from file

        // random number generator and state
        // needs to be left alone
        public Vector<Slot> Slots = new Vector<Slot>();

        // needs to be freshly inited at start of game!
        public GlobalState Global;
        public Random Rand = new Random();
        public Cp Components = new Cp();


        // buffers, tempororay intermediate data that is not considered part of the state but are derived from it
        public Vector<Bounds> BoundList = new Vector<Bounds>();
        public Vector<Event> Events = new Vector<Event>();
        public Vector<Vector2> Targets = new Vector<Vector2>();


        public bool gameOver = false;

        public Game()
        {
            OnMessage = (msg) => { };
            OnCreate = (entity) => { };
            OnDestroy = (entity) => { };

            Slots.Resize(64);

            Const = Static.Const;
            SetupCollisionTable();
        }

        public void Copy(Game other)
        {
            Slots.Copy(other.Slots);
            Rand.Copy(other.Rand);
            Global = other.Global;
            Components.Copy(other.Components);
        }

        public void SetInput(Vector<bool> connected, Vector<bool> broken, Vector<ulong> input)
        {
            ulong b;
            for (int i = 0; i < connected.Size; ++i)
            {
                ref var slot = ref Slots[i];
                b = input[i];

                Slots[i].Connected = connected[i];
                //Slots[i].Broken = broken[i];
                
                // this section is a little bit ugly because I am interpreting the bits differently
                // in different games, idealy the interpretation should happen outside here
                Slots[i].Input.Primary = (PrimaryMask.Decode(input[i]) & 0x01) == 0x01;

                Slots[i].Input.Left = (LeftMask.Decode(input[i]) & 0x01) == 0x01;
                Slots[i].Input.Right = (RightMask.Decode(input[i]) & 0x01) == 0x01;

                Slots[i].Input.X = (short)XMask.Decode(input[i]);               
            }
        }

        public void Init(uint seed, Vector<bool> connectedAtStart)
        {
            Rand.SetSeed(seed);
         
            // set initial global state
            Global = new GlobalState
            {
                playing = false,
                enemySpeed = 3,
                enemyCount = 0,
                textType = (ushort)Images.text_ready,
                textAnimate = 0
            };

            Components.Clear();

            // create player ships
            //OnMessage("count: " + Data.Slots.Capacity + " " + Data.Slots.Size);
            Events.Clear();
            for (int j = 0; j < Slots.Size; ++j)
            {
                if (Slots[j].Connected == true && connectedAtStart[j] == true)
                {
                    if (Events.Append() == true)
                    {
                        Events.Last.CreatePlayer((ushort)j, new Vector2(j * 60 - 960 + 32, -500));
                    }
                }
            }

            // shot cleaners make sure shots don't last forever 
            if (Events.Append() == true)
            {
                Events.Last.CreateEntity(ObjType.ShotCleaner, new Vector2(0, 1090));
            }

            if (Events.Append() == true)
            {
                Events.Last.CreateEntity(ObjType.ShotCleaner, new Vector2(0, -1090));
            }

            //OnMessage("events: " + Events.Capacity + " " + Events.Size);

            ResolveEvents();
        }

        public void SetupCollisionTable()
        {
            // register animations RegisterAnimation(new Images[]
            RegisterAnimation(new Images[]
            {
                Images.player_boom_0,
                Images.player_boom_1,
                Images.player_boom_2,
                Images.player_boom_3,
                Images.player_boom_4,
                Images.player_boom_5,
                Images.player_boom_6,
                Images._null
            });

            RegisterAnimation(new Images[]
            {
                Images.enemy_boom_0,
                Images.enemy_boom_1,
                Images.enemy_boom_2,
                Images.enemy_boom_3,
                Images.enemy_boom_4,
                Images.enemy_boom_5,
                Images.enemy_boom_6,
                Images._null
            });

            RegisterAnimation(new Images[]
            {
                Images.player_ship_0,
                Images.player_ship_1
            });

            for (int i = 0; i < Const.EnemyCount; i++)
            {
                // register animations
                RegisterAnimation(new Images[]
                {
                    (Images)(i + 2),
                    (Images)(i + 2 + Const.EnemyCount)
                });
            }

            RegisterAnimation(new Images[]
            {
                Images.player_ship_0,
                Images.player_ship_1
            });


            // a collision between enemy and bullet
            RegisterCollision(ObjType.ShotCleaner, ObjType.Bullet, (Game g, ref Event e) =>
            {
                // do colision betweem e.A bullet, and e.B enemy!            
                var cp = g.Components;
                var B = cp.Get(e.B);
                //ref var Ba = ref B.animator;
                if (B.animator.frame != (ushort)Images._null)
                {
                    B.animator.frame = (ushort)Images._null;
                }
            });


            RegisterCollision(ObjType.ShotCleaner, ObjType.BadBullet, (Game g, ref Event e) =>
            {
                // do colision betweem e.A bullet, and e.B enemy!
                var cp = g.Components;
                var B = cp.Get(e.B);
                if (B.animator.frame != (ushort)Images._null)
                {
                    B.animator.frame = (ushort)Images._null;
                }
            });


            RegisterCollision(ObjType.Bullet, ObjType.Enemy, (Game g, ref Event e) =>
            {
                // do colision betweem e.A bullet, and e.B enemy!
                var cp = g.Components;
                var A = cp.Get(e.A);
                var B = cp.Get(e.B);
                if (A.animator.frame != (ushort)Images._null && B.animator.frame != (ushort)Images._null)
                {
                    A.animator.frame = (ushort)Images._null;
                    B.animator.frame = (ushort)Images._null;
                    if (Events.Append() == true)
                    {
                        Events.Last.CreateEntity(ObjType.Boom, B.body.position);
                    }
                }
            });

            RegisterCollision(ObjType.BadBullet, ObjType.Player, (Game g, ref Event e) =>
            {
                // do colision betweem e.A bullet, and e.B enemy!
                var cp = g.Components;
                var A = cp.Get(e.A);
                var B = cp.Get(e.B);

                if (A.animator.frame != (ushort)Images._null && B.animator.frame != (ushort)Images._null_persist)
                {
                    A.animator.frame = (ushort)Images._null;

                    B.player.damage = 100;
                    B.animator.frame = (ushort)Images._null_persist;

                    if (Events.Append() == true)
                    {
                        Events.Last.CreateEntity(ObjType.PlayerBoom, B.body.position);
                    }
                }
            });


        }

        public void RegisterAnimation(Images[] list)
        {
            for (int i = 0; i < list.Length - 1; ++i)
            {
                AnimationTable[(ushort)(list[i])] = (ushort)list[i + 1];
            }
            ushort last = (ushort)(list[list.Length - 1]);
            if (last != (ushort)Images._null)
            {
                AnimationTable[last] = (ushort)list[0];
            }
        }

        public void RegisterCollision(ObjType ao, ObjType bo, CollisionDel d)
        {
            byte a = (byte)ao;
            byte b = (byte)bo;
            if (a > b)
            {
                CollisionTable[(ushort)((a << 8) | b)] = d;
            }
            else
            {
                //k = (ushort)((b << 8) | a);
                // throw exception!
                OnMessage($"types reversed! {ao}, {bo}");
            }
        }

        public void Update()
        {
            UpdateAnimators();
            UpdatePlayers();
            UpdateEnemies();
            Integrate();
            ResolveState();
            FillContactList();
            ResolveEvents();
        }

        public void FastForward()
        {
            UpdateAnimators();
            UpdatePlayers();
            UpdateEnemies();
            Integrate();
            ResolveState();
        }

        public void ResolveEvents()
        {
            for (int event_index = 0; event_index < Events.Size; ++event_index)
            {
                var cp = Components;
                ref var evt = ref Events[event_index];
                switch (evt.Id)
                {
                    case EventType.Contact:
                        if (CollisionTable.ContainsKey(evt.Key) == true)
                        {
                            if (cp.Valid(evt.A) == true && cp.Valid(evt.B) == true)
                            {
                                CollisionTable[evt.Key](this, ref evt);
                            }
                        }
                        break;

                    case EventType.DestroyEntity:
                        OnDestroy(evt.A);
                        cp.Destroy(evt.A);
                        break;

                    case EventType.CreateEntity:
                        {
                            // create from prefab!
                            var prefab = Const.Objects[(ObjType)evt.type];
                            var entity = cp.Create();
                            if (entity != Cp.Handle.Null)
                            {
                                prefab.Set(cp, entity);
                                var r = cp.Get(entity.index);
                                if (r.components.Contains(Cp.Body))
                                {
                                    r.body.position = evt.v;
                                }
                                if (r.components.Contains(Cp.Player))
                                {
                                    r.player.slot = (sbyte)evt.Key;
                                }
                                if (r.components.Contains(Cp.Enemy))
                                {
                                    r.enemy.delayFire = (ushort)(Rand.GetUint() % 2000);
                                    r.enemy.target = Cp.Handle.Null;
                                    r.animator.frame = (ushort)(Rand.Next(Const.EnemyCount) + 2);
                                }
                                OnCreate(entity);
                            }
                        }
                        break;

                    default:
                        OnMessage($"Attempted to process bad event {evt.Id}");
                        break;
                }
            }

            Events.Clear();
        }

        public void ResolveState()
        {
            if (Global.playing == false)
            {
                Global.textAnimate += Const.v0_00833333333333333;

                if (Global.textAnimate > 1)
                {
                    if (Global.textType != (ushort)Images.text_ready)
                    {
                        gameOver = true;
                    }
                    else
                    {

                        Global.playing = true;

                        // fix all ships
                        //Targets.Clear();

                       
                        for (var r = Components.Filter(Cp.Active | Cp.Player); r.MoveNext(); )
                        {
                            if (Slots[r.player.slot].Connected == true)
                            {
                                r.player.damage = 0;
                                r.animator.frame = (ushort)Images.player_ship_0;
                            }   
                        }

                        // j 24, i 20
                        // create new set of entities
                        for (int j = 0; j < 24; ++j)
                        {
                            for (int i = 0; i < 20; ++i)
                            {
                                if (Events.Append() == true)
                                {
                                    Events.Last.CreateEntity(ObjType.Enemy, new Vector2(j * 60 - 960 + 32, i * 32 - 100));
                                }
                            }
                        }
                    }
                }
            }
        }

        public void FillContactList()
        {
            BoundList.Clear();

            for (var r = Components.Filter(Cp.Active | Cp.Body); r.MoveNext();)
            {
                if (BoundList.Append())
                {
                    BoundList.Last = new Bounds(r.entity, r.objectId, r.body.position, r.body.velocity, r.body.size);
                }   
            }

            BoundList.Sort((ref Bounds lhs, ref Bounds rhs) =>
            {
                return lhs.lower.x < rhs.lower.x;
            });

            for (int i = 0; i < BoundList.Size; ++i)
            {
                ref Bounds iter = ref BoundList[i];
                for (int j = i + 1; j < BoundList.Size; ++j)
                {
                    ref Bounds nextIter = ref BoundList[j];
                    if (iter.Overlap(ref nextIter) == true)
                    {
                        if (Events.Append() == true)
                        {
                            if (iter.type > nextIter.type)
                            {
                                Events.Last.Contact((ushort)((iter.type << 8) | nextIter.type), iter.entity, nextIter.entity);
                            }
                            else
                            {
                                Events.Last.Contact((ushort)((nextIter.type << 8) | iter.type), nextIter.entity, iter.entity);
                            }
                        }
                    }
                }
            }

        }


        public void Integrate()
        {
            for (var r = Components.Filter(Cp.Active | Cp.Body); r.MoveNext();)
            {
                r.body.position += r.body.velocity;
            }
        }

        public void UpdateAnimators()
        {
            for (var r = Components.Filter(Cp.Active | Cp.Animator); r.MoveNext();)
            {
                ++r.animator.count;
                if (r.animator.count > 3)
                {
                    r.animator.count = 0;
                    if (AnimationTable.ContainsKey(r.animator.frame))
                    {
                        r.animator.frame = AnimationTable[r.animator.frame];
                    }
                }
                if (r.animator.frame == (ushort)Images._null)
                {
                    if (Events.Append() == true)
                    {
                        Events.Last.DestroyEntity(r.entity);
                    }
                }   
            }
        }

        public void UpdateEnemies()
        {
            Global.enemyCount = 0;
            for (var r = Components.Filter(Cp.Active | Cp.Body | Cp.Enemy); r.MoveNext();)
            {
            
                ++r.enemy.counter;
                if (r.enemy.counter > 150)
                {
                    r.enemy.counter = 0;
                    r.enemy.direction = (sbyte)(-r.enemy.direction);
                }

                r.body.velocity.x = r.enemy.direction * Global.enemySpeed;

                if (r.enemy.delayFire > 0)
                    --r.enemy.delayFire;

                if (r.enemy.delayFire == 0)
                {
                    r.enemy.delayFire = 2000;
                    if (Events.Append() == true)
                    {
                        Events.Last.CreateEntity(ObjType.BadBullet, r.body.position);
                    }
                }

                Global.enemyCount++;
            
            }
            // calculate enemy speed based on count
            Global.enemySpeed = 3;

            // if enemy count is zero, set playing to false, text to great job!
            if (Global.playing == true && Global.enemyCount == 0)
            {
                Global.playing = false;
                Global.textAnimate = 0;
                Global.textType = (ushort)Images.text_great;
                // also repair all ships!
                // actually this is done by the reset function!
            }

        }

        public void UpdatePlayers()
        {
            //Targets.Clear();
            bool livePlayer = false;

            for (var r = Components.Filter(Cp.Active | Cp.Body | Cp.Player); r.MoveNext();)
            {
        
                ref var slot = ref Slots[r.player.slot];

                r.body.velocity.x = 0;
                if (r.player.delayFire > 0)
                    --r.player.delayFire;

                if (r.player.damage > 0)
                {
                    // DO NOTHING!
                }
                else
                {
                    if (slot.Connected == true)
                    {
                        livePlayer = true;
                    }
                    else
                    {
                        // kill if disconnect!
                        r.player.damage = 100;
                        r.animator.frame = (ushort)Images._null_persist;

                        if (Events.Append() == true)
                        {
                            Events.Last.CreateEntity(ObjType.PlayerBoom, r.body.position);
                        }
                    }

                    if (slot.Input.Left == true) r.body.velocity.x = -5;
                    if (slot.Input.Right == true) r.body.velocity.x = 5;

                    /*if ( Scaler.Abs(r.body.position.x - slot.Input.X) < Const.v0_00005 )
                    {

                    }
                    else if (slot.Input.X < r.body.position.x)
                    {
                        r.body.velocity.x = -5;
                    }
                    else if (slot.Input.X > r.body.position.x)
                    {
                        r.body.velocity.x = 5;
                    }*/

                    if (slot.Input.Primary && r.player.delayFire == 0)
                    {
                        r.player.delayFire = 24;

                        if (Events.Append() == true)
                        {
                            Events.Last.CreateEntity(ObjType.Bullet, r.body.position);
                        }
                    }
                }


                
            }

            // if all players are damaged, set playing to false, text to no way! and destroy all enemies
            if (Global.playing == true && livePlayer == false)
            {
                Global.playing = false;
                Global.textAnimate = 0;
                Global.textType = (ushort)Images.text_no;

                for (var r = Components.Filter(Cp.Active | Cp.Body | Cp.Enemy); r.MoveNext();)
                {
                    if (Events.Append() == true)
                    {
                        Events.Last.DestroyEntity(r.entity);
                    }   
                }
            }


        }


    }

}