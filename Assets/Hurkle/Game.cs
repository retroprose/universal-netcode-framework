using System.Collections.Generic;
using FixedMath;
using RetroECS;

namespace Hurkle
{
    public class Game
    {
        // collision table
        public Dictionary<uint, Contact.Del> CollisionTable = new Dictionary<uint, Contact.Del>();

        // fast forward mode activated or not
        bool FastForwardMode = false;

        // things that need to be copied... maybe
        public Vector<Slot> Slots = new Vector<Slot>();

        // these need to be initialized every initalize
        public Random Rand = new Random();
        public Single<GlobalState> Global = new Single<GlobalState>();
        public Cp Components = new Cp();

        // intermediate buffers... may be able to use unions with some of these
        public Vector<Bounds> BoundList = new Vector<Bounds>();
        public Vector<Contact> Contacts = new Vector<Contact>();
        public Vector<Event> Events = new Vector<Event>();

        public Game()
        {
            Slots.Resize(64);
            SetupCollisionTable();
        }
        public void SetFastForward(bool f)
        {
            FastForwardMode = f;
        }

        public void Copy(Game other)
        {
            // NOT DOING THIS YET, NEED MORE OPTIMIZATION!
        }

        public void SetInput(Vector<bool> connected, Vector<bool> broken, Vector<ulong> input)
        {
            for (int i = 0; i < connected.Size; ++i)
            {
                Slots[i].Connected = connected[i];
                Slots[i].Dropped = broken[i];

                Slots[i].X = (short)Slot.XMask.Decode(input[i]);
                Slots[i].Y = (short)Slot.YMask.Decode(input[i]);

                Slots[i].Primary = (Slot.PrimaryMask.Decode(input[i]) & 0x01) == 0x01;
                Slots[i].Secondary = (Slot.SecondaryMask.Decode(input[i]) & 0x01) == 0x01;

                Slots[i].Up = (Slot.UpMask.Decode(input[i]) & 0x01) == 0x01;
                Slots[i].Down = (Slot.DownMask.Decode(input[i]) & 0x01) == 0x01;
                Slots[i].Left = (Slot.LeftMask.Decode(input[i]) & 0x01) == 0x01;
                Slots[i].Right = (Slot.RightMask.Decode(input[i]) & 0x01) == 0x01;

                Slots[i].Synced = (Games.Util.SyncedMask.Decode(input[i]) & 0x01) == 0x01;
            }
        }

        public void RegisterCollision(ObjType ao, ObjType bo, Contact.Del d)
        {
            ushort a = (ushort)ao;
            ushort b = (ushort)bo;
            if (a > b)
            {
                CollisionTable[Contact.MakeKey(a, b)] = d;
            }
            else
            {
                //k = (ushort)((b << 8) | a);
                // throw exception!
                //OnMessage($"types reversed! {ao}, {bo}");
                Log.Write($"types reversed! {ao}, {bo}");
            }
        }

        public void SetupCollisionTable()
        {
            // a collision between enemy and bullet
            RegisterCollision(ObjType.Bullet, ObjType.Enemy, (ref Contact c) =>
            {
                // do colision betweem e.A bullet, and e.B enemy!            

                var rA = Components.Get(c.A);
                var rB = Components.Get(c.B);

                if (rB.program.Id != FunctionUnion.Id.DeadFunction)
                {
                    rB.Replace(new DeadFunction(100));
                }

                if (Events.Append() == true)
                {
                    Events.Last.DestroyEntity(c.A);
                }
                
            });

            RegisterCollision(ObjType.Enemy, ObjType.Player, (ref Contact c) =>
            {
                // do colision betweem e.A bullet, and e.B enemy!            

                var rA = Components.Get(c.A);
                var rB = Components.Get(c.B);

                if (rA.program.Id != FunctionUnion.Id.DeadFunction)
                {
                    rB.player.dead = true;
                }

            });

            RegisterCollision(ObjType.Tilemap, ObjType.Bullet, (ref Contact c) =>
            {
                // do colision betweem e.A bullet, and e.B enemy!            
                if (Events.Append() == true)
                {
                    Events.Last.DestroyEntity(c.B);
                }
            });
        }



        public void Init(uint seed, Vector<bool> connectedAtStart)
        {
            //Rand.SetSeed(seed);

            Rand.SetSeed(0x12f3ae32);

            Components.Clear();

            Global.Ref = new GlobalState
            {
                endGame = false,
                gameOver = false,
                enemyCount = 0,
                worldMap = Cp.Handle.Null
            };
            

            Cp.Ref r;
            Cp.Handle entity;

            entity = Components.Create();            
            Static.Const.Objects[ObjType.Tilemap].Set(Components, entity);

          
            Global.Ref.worldMap = entity;

            r = Components.Get(entity);
            int width = Static.Const.MapDataPitch;
            int height = (Static.Const.MapData.Length / Static.Const.MapDataPitch);
            int halfWidth = width / 2;
            int halfHeight = height / 2;

            r.body.position = new Vector2(-halfWidth * 32, -halfHeight * 32);
            var tilemappos = r.body.position;

            r.fixture.position = new Vector2(halfWidth * 32, halfHeight * 32);
            r.fixture.size = new Vector2(halfWidth * 32, halfHeight * 32);

            r.tilemap.width = width;
            r.tilemap.height = height;

            r.tilemap.tiles = new byte[Static.Const.MapData.Length];


            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    ushort tile = Static.Const.MapData[j * Static.Const.MapDataPitch + i];
                    r.tilemap.tiles[j * r.tilemap.width + i] = (byte)tile;
                }
            }

           


            // 163*32, 175*32      5216, 5600
            // make players
            for (int i = 0; i < Slots.Size; ++i)
            {
                if (Slots[i].Connected == true && connectedAtStart[i] == true)
                {
                    entity = Components.Create();
                    //Log.Write("e: " + entity.index + " " + entity.generation);

                    Slots[i].tagged = entity;
                    if (entity != Cp.Handle.Null)
                    {
                        Static.Const.Objects[ObjType.Player].Set(Components, entity);
                        r = Components.Get(entity);
                        r.body.position = Static.Const.PlayerStart + tilemappos;
                        r.player.slot = (byte)i;
                    }
                }
            }


        }

        public void Update()
        {
            if (FastForwardMode == false)
            {
                UpdateInput();

                SetDisplacements();
                GeneratePairs();
                DoContacts();       // add callback to override stopping
                Integrate();
                // compute contacts here!
                DoMoreContacts();   // add callback to add more effects

                // other stuff
                UpdatePrograms();

                SpawnEnemies();

                ResolveEvents();

                //Gravity();
            }
            else
            {
                /*
                SetDisplacements();
                Integrate();
                // other stuff
                UpdatePlayers();
                Gravity();

                //Log.Write($"{Contacts.Size}");
                */
            }
        }

        public void UpdateInput()
        {
            for (int i = 0; i < Slots.Size; i++)
            {
                var p = new Vector2(Slots[i].X, Slots[i].Y);

                if (p.x > 544) p.x = 544;
                if (p.x < -544) p.x = -544;
                if (p.y > 544) p.y = 544;
                if (p.y < -544) p.y = -544;
            }
        }

        public void SpawnEnemies()
        {
            int numMake = 250 - Global.Ref.enemyCount;

            if (numMake > 10)
                numMake = 10;

            var pos = Components._Body[Global.Ref.worldMap.index].position;

            // make random blocks
            for (int i = 0; i < numMake; ++i)
            {
                if (Events.Append() == true)
                {
                    Events.Last.CreateEntity(ObjType.Enemy, pos + Static.Const.EnemySpawn[Rand.Next(Static.Const.EnemySpawn.Size)], new Vector2(0, 0));
                }
            }
        }

        public void DoContacts()
        {
            // test timing!
            var badkey = Contact.MakeKey((ushort)ObjType.Bullet, (ushort)ObjType.Player);
            Log.Start();
            for (int j = 0; j < Slop.Iterations; ++j)
            {

                for (int i = 0; i < Contacts.Size; ++i)
                {
                    ref var c = ref Contacts[i];

                    ref var dA = ref Components._Displacement[c.A];
                    ref var dB = ref Components._Displacement[c.B];

                    if (c.Key == badkey)
                    {
                        // nothing
                        //Log.Write($"We dropped it!");
                    }
                    else
                    {
                        if (c.sweep.Touch)
                        //if (c.dq.flags.Contains(DistanceQuery.BadNormal) == false)
                        {
                            if (c.invMassB != 0 || c.invMassA != 0)
                            {
                                var n = c.dq.Normal;

                                var bias = Slop.Bias;

                                var mass = 1 / (c.invMassB + c.invMassA);

                                var relativeNormalVelocity = (dA - dB).Dot(ref n);

                                var slop = Slop.Space;

                                Scaler impulse = 0;
                                if (relativeNormalVelocity > -c.dq.Distance - slop)
                                {
                                    // they are traveling together here!
                                    impulse = (relativeNormalVelocity + (c.dq.Distance + slop) * bias) * mass;
                                }

                                var newImpulse = Scaler.Max(impulse + c.impulse, 0);

                                var change = newImpulse - c.impulse;

                                c.impulse = newImpulse;

                                dA -= n * c.invMassA * change;
                                dB += n * c.invMassB * change;
                            }
                        }
                    }
                }

            }
            Log.Stop();

            //Log.Write($"Measured Time: {Log.GetTime()}");
        }

        public void DoMoreContacts()
        {
            for (int i = 0; i < Contacts.Size; ++i)
            {
                ref var c = ref Contacts[i];

                //c.Recompute();

                //if (c.dq.flags.Contains(DistanceQuery.Touch) == true)
                if (c.impulse > 0)
                {
                    ref var bA = ref Components._Body[c.A];
                    ref var bB = ref Components._Body[c.B];

                    var n = c.dq.Normal;

                    var change = c.impulse * Slop.Bounce;

                    bA.velocity -= n * bA.invMass * change;
                    bB.velocity += n * bB.invMass * change;

                    // Do collisions here!
                    if (CollisionTable.ContainsKey(c.Key) == true)
                    {
                        if (Components.Valid(c.A) == true && Components.Valid(c.B) == true)
                        {
                            CollisionTable[c.Key](ref c);
                        }
                    }
                    
                }
            }
        }

        public void SetDisplacements()
        {
            for (var r = Components.Filter(Cp.Active | Cp.Body); r.MoveNext();)
            {
                r.displacement = r.body.velocity;
            }
        }

        public void Integrate()
        {
            for (var r = Components.Filter(Cp.Active | Cp.Body); r.MoveNext();)
            {
                r.body.position += r.displacement;
            }
        }

        public void GeneratePairs()
        {
            BoundList.Clear();

            for (var r = Components.Filter(Cp.Active | Cp.ObjectId | Cp.Fixture | Cp.Body); r.MoveNext();)
            {
                if (BoundList.Append())
                {
                    BoundList.Last = new Bounds(r.entity, r.objectId, r.body.position + r.fixture.position, r.displacement, r.fixture.size);
                }
            }

            BoundList.Sort((ref Bounds lhs, ref Bounds rhs) =>
            {
                return lhs.lower.x < rhs.lower.x;
            });

            short sx, sy, ex, ey;
            int i, j, len;
            uint key;
            short bx, by;
            bool dead;
            Vector2 vec;
            
            Contacts.Clear();
            

            j = 0;
            len = BoundList.Size;
            while (j < len)
            {
                ref var iter = ref BoundList[j];
                i = j + 1;
             
                while (i < len && BoundList[i].lower.x < iter.upper.x)
                {
                    ref var nextIter = ref BoundList[i];

                    if (iter.Overlap(ref nextIter) == true)
                    {
                        ref var A = ref BoundList[i];
                        ref var B = ref BoundList[j];
                        if (iter.type > nextIter.type)
                        {
                            A = ref BoundList[j];
                            B = ref BoundList[i];
                        }
                        key = Contact.MakeKey(A.type, B.type);

                        if (A.type == (ushort)ObjType.Tilemap)
                        {
                            if (B.type == (ushort)ObjType.Tilemap)
                            {
                                // do nothing
                            }
                            else
                            {
                                var tb = B;

                                tb.lower -= A.lower;
                                tb.upper -= A.lower;

                                sx = (short)(tb.lower.x / 32); 
                                sy = (short)(tb.lower.y / 32);
                                ex = (short)(tb.upper.x / 32);
                                ey = (short)(tb.upper.y / 32);
                            
                                ref var tiles = ref Components._Tilemap[A.entity];

                                if (sx < 0) sx = 0;
                                if (sy < 0) sy = 0;
                                if (ex >= (short)(tiles.width - 1))
                                    ex = (short)(tiles.width - 1);
                                if (ey >= (short)(tiles.height - 1))
                                    ey = (short)(tiles.height - 1);

                                dead = false;
                                bx = -1;
                                by = -1;
                                if (B.type == (ushort)ObjType.Enemy)
                                {
                                    if (Components._Program[B.entity].Id == FunctionUnion.Id.DeadFunction)
                                    {
                                        dead = true;
                                    }
                                    vec = Components._Body[B.entity].position - A.lower;
                                    bx = (short)(vec.x / 32);
                                    by = (short)(vec.y / 32);
                                    Components._Enemy[B.entity].Flow = 0;
                                }

                                for (short row = sy; row <= ey; row++)
                                {
                                    for (short col = sx; col <= ex; col++)
                                    {
                                        byte t = tiles.tiles[row * tiles.width + col];

                                        if (col == bx && row == by)
                                        {
                                            Components._Enemy[B.entity.index].Flow = Static.Const.FlowData[row * tiles.width + col];
                                            //tiles.tiles[row * tiles.width + col] = 2;
                                        }
                                       
                                        if (t == 1)
                                        {
                                            if (Contacts.Append() == true)
                                            {
                                                Contacts.Last = new Contact(key, Components, A.entity, B.entity, col, row);
                                            }
                                        }
                                        else
                                        {
                                            if (t == 0 && dead == true)
                                            {
                                                tiles.tiles[row * tiles.width + col] = 2;
                                            }
                                        }
                                    }
                                }
                               
                            }
                        }                        
                        else
                        {
                            if (Contacts.Append() == true)
                            {
                                Contacts.Last = new Contact(key, Components, A.entity, B.entity, -1, -1);  
                            }
                        }
                    }
                    i++;
                }
                j++;
            }

            // sort contacts!
            //Contacts.Sort((ref Contact lhs, ref Contact rhs) =>
            //{
            //    return lhs.dq.Distance > rhs.dq.Distance;
            //});
        }

        public void Gravity()
        {
            /*
            for (var r = Components.Filter(Cp.Active | Cp.Body); r.MoveNext();)
            {
                r.body.velocity.y -= (Scaler)0.4f;          
            }
            */
        }

        public void UpdatePrograms()
        {
            ref var g = ref Global.Ref;

            g.endGame = true;
            g.enemyCount = 0;

            for (var r = Components.Filter(Cp.Active | Cp.Program); r.MoveNext();)
            {     
                r.program.Invoke(this, ref r);
            }

            if (g.endGame == true)
            {
                g.gameOver = true;
            }
        }

        public void ResolveEvents()
        {
            for (int event_index = 0; event_index < Events.Size; ++event_index)
            {
                var cp = Components;
                ref var evt = ref Events[event_index];
                switch (evt.Id)
                {
                    //case EventType.Contact:
                    //    if (CollisionTable.ContainsKey(evt.Key) == true)
                    //    {
                    //        if (cp.Valid(evt.A) == true && cp.Valid(evt.B) == true)
                    //        {
                    //            CollisionTable[evt.Key](this, ref evt);
                    //        }
                    //    }
                    //    break;

                    case EventType.DestroyEntity:
                        cp.Destroy(evt.A);
                        break;

                    case EventType.CreateEntity:
                        {
                            // create from prefab!
                            var entity = cp.Create();
                            if (entity != Cp.Handle.Null)
                            {
                                Static.Const.Objects[(ObjType)evt.type].Set(cp, entity);
                                var r = cp.Get(entity.index);
                                if (r.components.Contains(Cp.Body))
                                {
                                    r.body.position = evt.p;
                                    r.body.velocity = evt.v;
                                }
                            }
                        }
                        break;

                    default:
                        // bad event
                        break;
                }
            }

            Events.Clear();
        }



    }

}
