using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GalacticMarauders;
using RetroECS;
using System.Xml.Schema;
using Games;

public class GalacticMaraudersIO : BaseIO
{
    static public GameId _Id = GameId.GalacticMarauders;
    public override GameId CurrentId() => _Id;


    public GameObject forwardLayer;
    public GameObject laggedLayer;



    private Sprite[] SpriteTable;

    private int width;
    private int height;
    private ushort[] map;
    private Vector3 Position;
    private bool Ready;

    private Game game;
    private Game forwardGame;

    public override void MainUpdate()
    {
        game.SetInput(Passed.Connected, Passed.Dropped, Passed.RawInput);
        game.Update();
        if (game.gameOver == true)
        {
            // we need a way to return what the next game should be here!
            Passed.NextGame = GameId.Intermission;
        }
    }

    public override void ForwardUpdate()
    {
        forwardGame.SetInput(Passed.Connected, Passed.Dropped, Passed.RawInput);
        forwardGame.FastForward();
    }

    public override void PrepForward()
    {
        forwardGame.Copy(game);
    }

    public override void Init(GameObject c, sbyte l, PassedData p)
    {
        base.Init(c, l, p);

        Static.Const = new Data();

        game = new Game();
        forwardGame = new Game();

        SpriteTable = GenerateSpriteTable(typeof(GalacticMarauders.Images), "Images\\invaders");

        game = new Game();
        game.SetInput(Passed.Connected, Passed.Dropped, Passed.RawInput);
        game.Init(Passed.Seed, Passed.ConnectedAtStart);
    }

    // Start is called before the first frame update
    public override ulong ProcessInput()
    {
        ulong ret = 0x0000000000000000;

        var mouse = mainCamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
        mouse = forwardLayer.transform.InverseTransformPoint(mouse);

        short X = (short)mouse.x;

        int Left = (Input.GetKey(KeyCode.LeftArrow)) ? 0x01 : 0x00;
        int Right = (Input.GetKey(KeyCode.RightArrow)) ? 0x01 : 0x00;

        int Primary = (Input.GetKey(KeyCode.Space)) ? 0x01 : 0x00;
        //int Primary = (Input.GetMouseButton(0)) ? 0x01 : 0x00;

        ret |= GalacticMarauders.Game.XMask.Encode((ulong)X);
        ret |= GalacticMarauders.Game.PrimaryMask.Encode((ulong)Primary);
        ret |= GalacticMarauders.Game.LeftMask.Encode((ulong)Left);
        ret |= GalacticMarauders.Game.RightMask.Encode((ulong)Right);

        ret |= Games.Util.SyncedMask.Encode(0x01);

        return ret;
    }


    public override void ProcessOutput()
    {

        float tx = 0, ty = 0;
        ushort tf = 0;
        bool draw;

        Flags mask;
        GalacticMarauders.Cp cp;
        GameObject go;
        forwardLayer.GetComponent<RenderSystem>().ResetObjects();
        laggedLayer.GetComponent<RenderSystem>().ResetObjects();

        
         cp = game.Components;
        for (var r = cp.Filter(GalacticMarauders.Cp.Active | GalacticMarauders.Cp.Body | GalacticMarauders.Cp.ObjectId | GalacticMarauders.Cp.Animator); r.MoveNext();)
        {
            draw = true;

          
            if (r.components.Contains(Cp.Player))
            {
                if (r.player.slot == LocalSlot)
                {
                    draw = false;
                    tx = (float)r.body.position.x;
                    ty = (float)r.body.position.y;
                    tf = r.animator.frame;
                }
            }
            if (draw == true)
            {
                go = laggedLayer.GetComponent<RenderSystem>().NextObject();
                go.transform.localPosition = new Vector3((float)r.body.position.x, (float)r.body.position.y, 0.0f);
                go.transform.localScale = new Vector3(200.0f, 200.0f, 1.0f);
                //go.transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.forward);
                go.GetComponent<SpriteRenderer>().sprite = SpriteTable[r.animator.frame];
                go.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            }
            
        }

        if (tf == (ushort)GalacticMarauders.Images.player_ship_0) tf = (ushort)GalacticMarauders.Images.local_player_0;
        if (tf == (ushort)GalacticMarauders.Images.player_ship_1) tf = (ushort)GalacticMarauders.Images.local_player_1;

        go = laggedLayer.GetComponent<RenderSystem>().NextObject();
        go.transform.localPosition = new Vector3(tx, ty, 0.0f);
        go.transform.localScale = new Vector3(200.0f, 200.0f, 1.0f);
        //go.transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.forward);
        go.GetComponent<SpriteRenderer>().sprite = SpriteTable[tf];
        go.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 0.5f);


        cp = forwardGame.Components;
        for (var r = cp.Filter(GalacticMarauders.Cp.Active | GalacticMarauders.Cp.Body | GalacticMarauders.Cp.ObjectId | GalacticMarauders.Cp.Animator); r.MoveNext();)
        {
            draw = true;

            if (r.components.Contains(GalacticMarauders.Cp.Player))
            {
                if (r.player.slot == LocalSlot)
                {
                    draw = false;
                    tx = (float)r.body.position.x;
                    ty = (float)r.body.position.y;
                    tf = r.animator.frame;
                }
            }
            if (draw == true)
            {
                go = forwardLayer.GetComponent<RenderSystem>().NextObject();
                go.transform.localPosition = new Vector3((float)r.body.position.x, (float)r.body.position.y, 0.0f);
                go.transform.localScale = new Vector3(200.0f, 200.0f, 1.0f);
                //go.transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.forward);
                go.GetComponent<SpriteRenderer>().sprite = SpriteTable[r.animator.frame];
            }
            
        }

        if (tf == (ushort)GalacticMarauders.Images.player_ship_0) tf = (ushort)GalacticMarauders.Images.local_player_0;
        if (tf == (ushort)GalacticMarauders.Images.player_ship_1) tf = (ushort)GalacticMarauders.Images.local_player_1;

        go = forwardLayer.GetComponent<RenderSystem>().NextObject();
        go.transform.localPosition = new Vector3(tx, ty, 0.0f);
        go.transform.localScale = new Vector3(200.0f, 200.0f, 1.0f);
        //go.transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.forward);
        go.GetComponent<SpriteRenderer>().sprite = SpriteTable[tf];



        var g = game;
        if (g.Global.playing == false)
        {
            //float textScale = 900.0f * (float)game.State.textAnimate;

            float x = (float)g.Global.textAnimate;
            float c1 = 1.70158f;
            float c3 = c1 + 1;

            //float textScale = 1 + c3 * (float)Math.Pow(x - 1, 3) + c1 * (float)Math.Pow(x - 1, 2);
            float textScale = 1 - (float)Math.Pow(1 - x, 3);

            textScale *= 800.0f;

            go = forwardLayer.GetComponent<RenderSystem>().NextObject();
            go.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            go.transform.localScale = new Vector3(textScale, textScale, 1.0f);
            //go.transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.forward);
            go.GetComponent<SpriteRenderer>().sprite = SpriteTable[g.Global.textType];
        }

        forwardLayer.GetComponent<RenderSystem>().CleanObjects();
        laggedLayer.GetComponent<RenderSystem>().CleanObjects();

    }


}


