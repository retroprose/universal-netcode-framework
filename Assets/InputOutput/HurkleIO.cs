using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hurkle;
using RetroECS;
using System.Xml.Schema;
using Games;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using JetBrains.Annotations;



public class HurkleLoad
{
    public static Dictionary<ObjType, Cp.Prefab> Load(JObject objects)
    {
        int i;
        string id;
        JObject o, c;
        JToken t, m, n;
        Cp.Prefab p;
        ObjType ot;
        var prefabs = new Dictionary<ObjType, Cp.Prefab>();
        foreach (var property in objects.Properties())
        {
            p = new Cp.Prefab();
            o = property.Value.Value<JObject>();
            Enum.TryParse(property.Name, out ot);
            p.objectId = (ushort)ot;
            p.components = Cp.ObjectId | Cp.Component;
            p.components |= Cp.Active;
            /*
                public Flags components;
                public ushort objectId;
                public Body body;
                public Fixture fixture;
                public Tilemap tilemap;
                public Player player;
                public Enemy enemy;
                public Program program;
                public Vector2 displacement;
            */
            /*
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
            */
            // this should be set from the Key of the objects dictionary
            /*if (o.TryGetValue("objectId", out t))
            {
                p.components |= Cp.ObjectId;
                Enum.TryParse(t.Value<string>(), out p.objectId);
            }*/
            /*
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
            */
            if (o.TryGetValue("Body", out t))
            {
                c = t.Value<JObject>();
                p.components |= Cp.Body;
                if (c.TryGetValue("flags", out m))
                {
                    foreach (var f in m.Value<JArray>())
                    {
                        if (f.Value<string>() == "active") { p.body.flags |= Body.fActive; }
                        if (f.Value<string>() == "sweep") { p.body.flags |= Body.fSweep; }
                    }
                }
                if (c.TryGetValue("velocity", out m))
                {
                    p.body.velocity.x = (FixedMath.Scaler)m.Value<JObject>()["x"].Value<float>();
                    p.body.velocity.y = (FixedMath.Scaler)m.Value<JObject>()["y"].Value<float>();
                }
                if (c.TryGetValue("position", out m))
                {
                    p.body.position.x = (FixedMath.Scaler)m.Value<JObject>()["x"].Value<float>();
                    p.body.position.y = (FixedMath.Scaler)m.Value<JObject>()["y"].Value<float>();
                }
                if (c.TryGetValue("invMass", out m))
                {
                    p.body.invMass = (FixedMath.Scaler)m.Value<float>();
                }
            }
            /*
                Vector2
            */
            if (o.TryGetValue("Displacement", out t))
            {
                p.components |= Cp.Displacement;
                p.displacement.x = (FixedMath.Scaler)t.Value<JObject>()["x"].Value<float>();
                p.displacement.y = (FixedMath.Scaler)t.Value<JObject>()["y"].Value<float>();
            }
            /*
            public struct Fixture
            {
                static public Flag fActive = 0;
                static public Flag fSensor = 1;
                static public Flag fEmpty = 2;
                public Flags flags;
                public Vector2 position;
                public Vector2 size;
            }
            */
            if (o.TryGetValue("Fixture", out t))
            {
                c = t.Value<JObject>();
                p.components |= Cp.Fixture;
                if (c.TryGetValue("flags", out m))
                {
                    foreach (var f in m.Value<JArray>())
                    {
                        if (f.Value<string>() == "active") { p.fixture.flags |= Fixture.fActive; }
                        if (f.Value<string>() == "sweep") { p.fixture.flags |= Fixture.fSensor; }
                        if (f.Value<string>() == "empty") { p.fixture.flags |= Fixture.fEmpty; }
                    }
                }
                if (c.TryGetValue("position", out m))
                {
                    p.fixture.position.x = (FixedMath.Scaler)m.Value<JObject>()["x"].Value<float>();
                    p.fixture.position.y = (FixedMath.Scaler)m.Value<JObject>()["y"].Value<float>();
                }
                if (c.TryGetValue("size", out m))
                {
                    p.fixture.size.x = (FixedMath.Scaler)m.Value<JObject>()["x"].Value<float>();
                    p.fixture.size.y = (FixedMath.Scaler)m.Value<JObject>()["y"].Value<float>();
                }
            }
            /*
            public struct Tilemap
            {
                public int width;
                public int height;
                public byte[] tiles;
            }
            */
            if (o.TryGetValue("Tilemap", out t))
            {
                c = t.Value<JObject>();
                p.components |= Cp.Tilemap;
                if (c.TryGetValue("width", out m))
                {
                    p.tilemap.width = m.Value<int>();
                }
                if (c.TryGetValue("height", out m))
                {
                    p.tilemap.height = m.Value<int>();
                }
                if (c.TryGetValue("tiles", out m))
                {
                    p.tilemap.tiles = new byte[m.Value<JArray>().Count];
                    for (i = 0; i < p.tilemap.tiles.Length; i++)
                    {
                        p.tilemap.tiles[i] = m.Value<JArray>()[i].Value<byte>();
                    }
                }
            }
            /*
            public struct Player
            {
                public byte slot;
                public bool dead;
            }
            */
            if (o.TryGetValue("Player", out t))
            {
                c = t.Value<JObject>();
                p.components |= Cp.Player;
                if (c.TryGetValue("slot", out m))
                {
                    p.player.slot = m.Value<byte>();
                }
                if (c.TryGetValue("dead", out m))
                {
                    p.player.dead = m.Value<bool>();
                }
            }
            /*
            public struct Enemy
            {
                public byte Flow;
            }
            */
            if (o.TryGetValue("Enemy", out t))
            {
                c = t.Value<JObject>();
                p.components |= Cp.Enemy;
                if (c.TryGetValue("flow", out m))
                {
                    p.enemy.Flow = m.Value<byte>();
                }
            }
            /*
            public struct Program
            {
                public ReturnUnion Return;
                public PoolFunction Function;
                public struct PoolFunction
                {
                    public byte State;
                    public ushort FunctionId;
                    public ushort PreviousId;
                    public FunctionUnion Union;
                }
                public FunctionUnion.Id Id => (FunctionUnion.Id)Function.FunctionId;
            }
            InvalidFunction = 0,
            FibFunction,
            RunningFunction,
            DeadFunction,
            PlayerFunction,
            NoOpFunction
            */
            if (o.TryGetValue("Program", out t))
            {
                id = "";
                c = t.Value<JObject>();
                p.components |= Cp.Program;
                p.program.Function.State = 0;
                p.program.Function.PreviousId = 0;
                if (c.TryGetValue("id", out m))
                {
                    id = m.Value<string>();
                    switch (id)
                    {
                        case "NoOpFunction":
                            p.program.Function.FunctionId = (ushort)FunctionUnion.Id.NoOpFunction;
                            break;

                        case "FibFunction":
                            p.program.Function.FunctionId = (ushort)FunctionUnion.Id.FibFunction;
                            p.program.Function.Union.fib = new FibFunction();
                            if (c.TryGetValue("data", out n))
                            {
                                p.program.Function.Union.fib = new FibFunction(n.Value<int>());
                            }
                            break;

                        case "RunningFunction":
                            p.program.Function.FunctionId = (ushort)FunctionUnion.Id.RunningFunction;
                            p.program.Function.Union.running = new RunningFunction();
                            if (c.TryGetValue("data", out n))
                            {
                                p.program.Function.Union.running = new RunningFunction(n.Value<int>());
                            }
                            break;

                        case "DeadFunction":
                            p.program.Function.FunctionId = (ushort)FunctionUnion.Id.DeadFunction;
                            p.program.Function.Union.dead = new DeadFunction();
                            if (c.TryGetValue("data", out n))
                            {
                                p.program.Function.Union.dead = new DeadFunction(n.Value<int>());
                            }
                            break;

                        case "PlayerFunction":
                            p.program.Function.FunctionId = (ushort)FunctionUnion.Id.PlayerFunction;
                            p.program.Function.Union.play = new PlayerFunction();
                            if (c.TryGetValue("data", out n))
                            {
                                p.program.Function.Union.play = new PlayerFunction(n.Value<int>());
                            }
                            break;

                        default:
                            p.program.Function.FunctionId = (ushort)FunctionUnion.Id.InvalidFunction;
                            break;
                    }
                }
            }

            prefabs[(ObjType)p.objectId] = p;

        } // end loop

        return prefabs;

    } // end function
}




public class HurkleIO : BaseIO
{
    static public GameId _Id = GameId.Hurkle;
    public override GameId CurrentId() => _Id;


    public GameObject mainLayer;

 

    private Sprite[] SpriteTable;

    private int width;
    private int height;
    private ushort[] map;
    private byte[] flowmap;
    private Vector3 Position;
    private bool Ready;


    private Game game;


    public override void MainUpdate()
    {
        game.SetInput(Passed.Connected, Passed.Dropped, Passed.RawInput);
        game.Update();
        if (game.Global.Ref.gameOver == true)
        {
            // we need a way to return what the next game should be here!
            Passed.NextGame = GameId.Intermission;
        }
    }

    public override void Init(GameObject c, sbyte l, PassedData p)
    {
        base.Init(c, l, p);

        //TextAsset ta = (TextAsset)Resources.Load("Data\\survive_open");
        TextAsset ta = (TextAsset)Resources.Load("Data\\survive_complex");
        var obj = JsonConvert.DeserializeObject<JObject>(ta.text);

        //TextAsset ta2 = (TextAsset)Resources.Load("Data\\survive_open_flow");
        TextAsset ta2 = (TextAsset)Resources.Load("Data\\survive_complex_flow");
        var obj2 = JsonConvert.DeserializeObject<JObject>(ta2.text);

        //SpriteTable = Util.GenerateSpriteTable(typeof(Games.Hurkle.Images), "Images/ascii8");

        width = obj["width"].Value<int>();
        height = obj["height"].Value<int>();

        map = new ushort[width * height];
        flowmap = new byte[width * height];

        var ps = new FixedMath.Vector2();
        var es = new Vector<FixedMath.Vector2>();


        ushort t, f;
        byte fm;

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                t = obj["layers"][1]["data"][(height - j - 1) * width + i].Value<ushort>();
                fm = obj2["data"][(height - j - 1) * width + i].Value<byte>();
                //fm = obj2["data"][(height - j - 1) * width + i].Value<byte>();

                if (t == 2012)
                {
                    f = 1;
                }
                else
                {
                    f = 0;
                    if (t == 2819)
                    {
                        ps = new FixedMath.Vector2(i * 32 + 16, j * 32 + 16);
                    }
                    if (t == 1539)
                    {
                        if (es.Append() == true)
                        {
                            es.Last = new FixedMath.Vector2(i * 32 + 16, j * 32 + 16);
                        }
                    }
                }

                map[j * width + i] = f;
                flowmap[j * width + i] = fm;
                //Log.Force($"{f} ");
            }
            //Log.ForceLine();
        }

        Ready = false;
        Position = new Vector3(0.0f, -1080.0f / 4, 0.0f);
        

        Static.Const = new Data();

        Static.Const.PlayerStart = ps;
        Static.Const.EnemySpawn = es;

        TextAsset ta_objects = (TextAsset)Resources.Load("Data\\hurkle_objects");
        var obj_objects = JsonConvert.DeserializeObject<JObject>(ta_objects.text);
        Static.Const.Objects = HurkleLoad.Load(obj_objects);

        Static.Const.MapDataPitch = width;
        Static.Const.MapData = map;
        Static.Const.FlowData = flowmap;

        SpriteTable = GenerateSpriteTable(typeof(Hurkle.Images), "Images\\ascii8");

        game = new Game();
        game.SetInput(Passed.Connected, Passed.Dropped, Passed.RawInput);
        game.Init(Passed.Seed, Passed.ConnectedAtStart);
    }

    // Start is called before the first frame update
    public override ulong ProcessInput()
    {
        ulong ret = 0x0000000000000000;

        var mouse = mainCamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
        mouse = mainLayer.transform.InverseTransformPoint(mouse);

        short X = (short)mouse.x;
        short Y = (short)mouse.y;

        int Primary = (Input.GetMouseButton(0)) ? 0x01 : 0x00;
        int Secondary = (Input.GetMouseButton(1)) ? 0x01 : 0x00;

        int Left = (Input.GetKey(KeyCode.A)) ? 0x01 : 0x00;
        int Right = (Input.GetKey(KeyCode.D)) ? 0x01 : 0x00;
        int Up = (Input.GetKey(KeyCode.W)) ? 0x01 : 0x00;
        int Down = (Input.GetKey(KeyCode.S)) ? 0x01 : 0x00;

        ret |= Slot.XMask.Encode((ulong)X);
        ret |= Slot.YMask.Encode((ulong)Y);

        ret |= Slot.LeftMask.Encode((ulong)Left);
        ret |= Slot.RightMask.Encode((ulong)Right);
        ret |= Slot.UpMask.Encode((ulong)Up);
        ret |= Slot.DownMask.Encode((ulong)Down);

        ret |= Slot.PrimaryMask.Encode((ulong)Primary);
        ret |= Slot.SecondaryMask.Encode((ulong)Secondary);

        ret |= Games.Util.SyncedMask.Encode(0x01);

        return ret;
    }

    static public Color ConvertColor(EGAColor c)
    {
        return new Color(c.r / 255.0f, c.g / 255.0f, c.b / 255.0f);
    }

    public override void ProcessOutput()
    {

        Flags mask;
        GameObject go;
        GameObject de;

        System.Random rand = new System.Random();

        var layer = mainLayer.GetComponent<RenderSystem>();

        layer.ResetObjects();

        //debugLayer.GetComponent<RenderSystem>().ResetObjects();

        var cp = game.Components;
       
        Cp.Handle e = game.Slots[LocalSlot].tagged;

        //var camera = new Vector3((float)cp.Bodies[e.index].position.x, (float)cp.Bodies[e.index].position.y, 0.0f);

        var camera = new Vector3((float)game.Slots[LocalSlot].camera.x, (float)game.Slots[LocalSlot].camera.y, 0.0f);

        Vector3 lower, upper, location;

        //camera.x += 832.0f / 2.0f;
        short sx, sy, ex, ey;

        // 24, 25, 26, 27     up, down, right, left
        var table = new char[]
        {
            ' ',
            (char)26,
            '/',
            (char)24,
            '\\',
            (char)27,
            '%',
            (char)25,
            '\''
        };

        //Debug.Log($"START");
        for (var r = cp.Filter(Cp.Active | Cp.Body); r.MoveNext();)
        {
            //Debug.Log($"{r.index}");

            if (r.objectId == (ushort)ObjType.Tilemap)
            {
                location = new Vector3((float)r.body.position.x, (float)r.body.position.y, 0.0f);
                lower = camera - location;
                upper = lower;

                //Debug.Log($"{lower.x}, {lower.y}");

                //lower -= new Vector3(544.0f, 544.0f, 0.0f);
                //upper += new Vector3(544.0f, 544.0f, 0.0f);

                lower -= new Vector3(964.0f, 544.0f, 0.0f);
                upper += new Vector3(964.0f, 544.0f, 0.0f);

                sx = (short)(lower.x / 32);
                sy = (short)(lower.y / 32);
                ex = (short)(upper.x / 32);
                ey = (short)(upper.y / 32);

                if (sx < 0) sx = 0;
                if (sy < 0) sy = 0;
                if (ex >= (short)(r.tilemap.width - 1))
                    ex = (short)(r.tilemap.width - 1);
                if (ey >= (short)(r.tilemap.height - 1))
                    ey = (short)(r.tilemap.height - 1);

                for (short row = sy; row <= ey; row++)
                {
                    for (short col = sx; col <= ex; col++)
                    {

                        /*
                        byte fl = Global.Const.FlowData[row * r.tilemap.width + col];
                        go = layer.NextObject();
                        go.transform.localPosition = location - camera + new Vector3(col * 32.0f + 16.0f, row * 32.0f + 16.0f, 0.0f);
                        go.transform.localScale = new Vector3(400.0f, 400.0f, 1.0f);
                        //go.transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.forward);
                        go.GetComponent<SpriteRenderer>().sprite = SpriteTable[(int)table[fl]];
                        go.GetComponent<SpriteRenderer>().color = ConvertColor(Global.Const.EGAColors[7]);
                        */

                            
                        byte f = r.tilemap.tiles[row * r.tilemap.width + col];
                        if (Static.Const.MapFixtures[f].flags.Contains(Fixture.fEmpty) == false)
                        {
                            go = layer.NextObject();
                            go.transform.localPosition = location - camera + new Vector3(col * 32.0f + 16.0f, row * 32.0f + 16.0f, 0.0f);
                            go.transform.localScale = new Vector3(400.0f, 400.0f, 1.0f);
                            //go.transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.forward);
                            go.GetComponent<SpriteRenderer>().sprite = SpriteTable[(int)'#'];
                            go.GetComponent<SpriteRenderer>().color = ConvertColor(EGAColor.Index(7));
                        }
                        else
                        {
                            if (f == 2)
                            {
                                // 
                                // c 4 or 12
                                go = layer.NextObject();
                                go.transform.localPosition = location - camera + new Vector3(col * 32.0f + 16.0f, row * 32.0f + 16.0f, 0.0f);
                                go.transform.localScale = new Vector3(400.0f, 400.0f, 1.0f);
                                //go.transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.forward);
                                go.GetComponent<SpriteRenderer>().sprite = SpriteTable[176];
                                go.GetComponent<SpriteRenderer>().color = ConvertColor(EGAColor.Index(4));
                            }
                        }
                            

                    }
                }

            }
            else
            {
                int t = 0, c = 0;
                if (r.objectId == (int)ObjType.Bullet) { t = 249; c = 15; }
                if (r.objectId == (int)ObjType.Player) { 
                    t = 2; 
                    c = 3;
                    if (r.player.dead == true) { t = 1; }
                }
                if (r.objectId == (int)ObjType.Enemy) {
                    t = 2; 
                    c = 6;
                    if (r.program.Id == FunctionUnion.Id.DeadFunction) { t = 1; }
                }
                if (r.entity == e) { c = 11; }

                //Debug.Log("" + r.body.position.x + ", " + r.body.position.y);
                go = layer.NextObject();
                go.transform.localPosition = new Vector3((float)r.body.position.x, (float)r.body.position.y, 0.0f) - camera;
                go.transform.localScale = new Vector3(400.0f, 400.0f, 1.0f);
                //go.transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.forward);
                go.GetComponent<SpriteRenderer>().sprite = SpriteTable[t];
                go.GetComponent<SpriteRenderer>().color = ConvertColor(EGAColor.Index(c));
                    
                //de = debugLayer.GetComponent<RenderSystem>().NextObject();
                //de.GetComponent<DebugHit>().Index = r.entity.index;
                //de.GetComponent<DebugHit>().Components = cp;
            }
           
        }

        //debugLayer.GetComponent<RenderSystem>().CleanObjects();
        layer.CleanObjects();
       
    }


}


