using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Intermission;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices.ComTypes;
using RetroECS;
using Games;

public class IntermissionIO : BaseIO
{
    static public GameId _Id = GameId.Intermission;
    public override GameId CurrentId() => _Id;

  
    public GameObject mainLayer;

    public struct EGAColor
    {
        public static EGAColor FromArgb(byte r, byte g, byte b)
        {
            return new EGAColor { r = r, g = g, b = b };
        }
        public byte r;
        public byte g;
        public byte b;
    }


    public EGAColor[] EGAColors = new EGAColor[]
    {
        EGAColor.FromArgb(  0,  0,  0),
        EGAColor.FromArgb(  0,  0,170),
        EGAColor.FromArgb(  0,170,  0),
        EGAColor.FromArgb(  0,170,170),
        EGAColor.FromArgb(170,  0,  0),
        EGAColor.FromArgb(170,  0,170),
        EGAColor.FromArgb(170, 85,  0),
        EGAColor.FromArgb(170,170,170),
        EGAColor.FromArgb( 85, 85, 85),
        EGAColor.FromArgb( 85, 85,255),
        EGAColor.FromArgb( 85,255, 85),
        EGAColor.FromArgb( 85,255,255),
        EGAColor.FromArgb(255, 85, 85),
        EGAColor.FromArgb(255, 85,255),
        EGAColor.FromArgb(255,255, 85),
        EGAColor.FromArgb(255,255,255)
    };

    private Sprite[] SpriteTable;

    private System.Random Rand = new System.Random();
    private FixedMath.Random MyRand = new FixedMath.Random();

    private int width;
    private int height;
    private ushort[] map;
    private Vector3 Position;
    private bool Ready;

    private Game game;

    int debug = 0;

    public override void MainUpdate()
    {
        game.SetInput(Passed.Connected, Passed.Dropped, Passed.RawInput);
        game.Update();
        if (game.gameOver == true)
        {
            // we need a way to return what the next game should be here!
            MyRand.SetSeed(game.Seed);

            // 4
            int rnd = MyRand.Next((int)GameId.Count - 3);
            //Passed.NextGame = (GameId)(rnd + 2);
            //Passed.NextGame = GameId.Hurkle;
            //Passed.NextGame = GameId.Climber;
            Passed.NextGame = GameId.GalacticMarauders;

            //Debug.Log($"Next Game: {Passed.NextGame}");

            for (int i = 0; i < Passed.ConnectedAtStart.Size; ++i)
            {
                Passed.ConnectedAtStart[i] = game.ConnectedAtStart[i];
            }
            Passed.Seed = game.Seed;
        }
    }

    
    public override void Init(GameObject c, sbyte l, PassedData p)
    {
        base.Init(c, l, p);

        TextAsset ta = (TextAsset)Resources.Load("Data\\ready");
        var obj = JsonConvert.DeserializeObject<JObject>(ta.text);

        //SpriteTable = Util.GenerateSpriteTable(typeof(Games.Hurkle.Images), "Images/ascii8");

        width = obj["width"].Value<int>();
        height = obj["height"].Value<int>();

        map = new ushort[width * height];

        ushort t;

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                t = obj["layers"][0]["data"][(height - j - 1) * width + i].Value<ushort>();
                map[j * width + i] = (ushort)(t - 1);
            }
        }

        Ready = false;
        Position = new Vector3(0.0f, -1080.0f / 4, 0.0f);

        SpriteTable = GenerateSpriteTable(typeof(Intermission.Images), "Images\\ascii8");

        game = new Game();
    }

    // Start is called before the first frame update
    public override ulong ProcessInput()
    {
        ulong ret = 0x0000000000000000;

        if (game.AllReady == true)
        {
            ret |= Games.Util.SyncedMask.Encode((ulong)0x01);
            return ret;
        }

        //var pos = controller.mainCamera.GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition);

        //var mouse = new Vector3((0.0f - 0.5f) * 1920.0f, (0.0f - 0.5f) * 1080.0f, 0.0f);

        var mouse = mainCamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
        mouse = mainLayer.GetComponent<RenderSystem>().transform.InverseTransformPoint(mouse);

        if (mouse.x > 1920.0f / 2.0f ||
            mouse.x < -1920.0f / 2.0f ||
            mouse.y > 1080.0f / 2.0f ||
            mouse.y < -1080.0f / 2.0f)
        {
            // dont upate psotion!
        }
        else
        {
            Position += (mouse - Position).normalized * 4.0f;
        }

        if (Position.x > 536.0f) Position.x = 536.0f;
        if (Position.x < -536.0f) Position.x = -536.0f;
        if (Position.y > 536.0f) Position.x = 536.0f;
        if (Position.y < -536.0f) Position.x = -536.0f;

        short X = (short)Position.x;
        short Y = (short)Position.y;

        Ready = false;
        if (Y > 0)
        {
            Ready = true;
        }

        int IntReady = (Ready) ? 0x01 : 0x00;
        //int IntReady = 0x00;

        ushort rnd = (ushort)Rand.Next(0xffff);

        ret |= Game.XMask.Encode((ulong)X);
        ret |= Game.YMask.Encode((ulong)Y);
        ret |= Game.ReadyMask.Encode((ulong)IntReady);
        ret |= Game.SeedMask.Encode((ulong)rnd);

        //debug++;
        //Debug.Log($"int ready {debug}: {IntReady}");

        return ret;
    }

    static public Color ConvertColor(EGAColor c)
    {
        return new Color(c.r / 255.0f, c.g / 255.0f, c.b / 255.0f);
    }

    public void DrawText(float x, float y, string w)
    {
        var layer = mainLayer.GetComponent<RenderSystem>();
        GameObject go;
        for (int i = 0; i < w.Length; i++)
        {
            go = layer.NextObject();
            go.transform.localPosition = new Vector3(x, y, -2.0f);
            go.transform.localScale = new Vector3(200.0f, 200.0f, 1.0f);
            go.GetComponent<SpriteRenderer>().sprite = SpriteTable[(int)w[i]];
            go.GetComponent<SpriteRenderer>().color = ConvertColor(EGAColors[0]);
            x += 16;
        }
    }
    public override void ProcessOutput()
    {

        var layer = mainLayer.GetComponent<RenderSystem>();

        layer.ResetObjects();

        GameObject go;

        ushort t;
        int i, j, r, c, k;
        float x, y;

        for (k = 0; k < map.Length; k++)
        {
            t = map[k];
            i = k % width;
            j = k / width;
            r = t % 256;
            c = t / 256;

            go = layer.NextObject();
            go.transform.localPosition = new Vector3(i * 32.0f + 16.0f, j * 32.0f + 16.0f, 0.0f) - new Vector3(1920.0f / 2.0f, 1080.0f / 2.0f, 0.0f) + new Vector3(416.0f, 0.0f, 0.0f);
            go.transform.localScale = new Vector3(400.0f, 400.0f, 1.0f);
            //go.transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.forward);
            // Log.Write($"{t} - {c} - {r}");
            if (c == 4 && (Ready == true || game.AllReady == true))
            {
                c = 12;
            }
            go.GetComponent<SpriteRenderer>().sprite = SpriteTable[(int)r];
            go.GetComponent<SpriteRenderer>().color = ConvertColor(EGAColors[c]);
        }

        string status;
        x = -900.0f;
        y = 500.0f;
        for (i = 0; i < game.Slots.Size; ++i)
        {
            status = "---------";
            if (game.Slots[i].Connected == true && game.Slots[i].InGame == false)
            {
                status = "In Lobby";
                go = layer.NextObject();
                go.transform.localPosition = new Vector3((float)game.Slots[i].X, (float)game.Slots[i].Y, 0.0f);
                go.transform.localScale = new Vector3(400.0f, 400.0f, 1.0f);
                if (i == LocalSlot)
                {
                    go.GetComponent<SpriteRenderer>().sprite = SpriteTable[(int)2];
                    go.GetComponent<SpriteRenderer>().color = ConvertColor(EGAColors[15]);
                }
                else
                {
                    go.GetComponent<SpriteRenderer>().sprite = SpriteTable[(int)1];
                    go.GetComponent<SpriteRenderer>().color = ConvertColor(EGAColors[15]);
                }                
            }
            if (game.Slots[i].Connected == true && game.Slots[i].InGame == true)
            {
                status = "In Game";
            }
            DrawText(x, y, $"Slot {i}: {status}");
            y -= 16.0f;
        }
 
        layer.CleanObjects();
    }


}
