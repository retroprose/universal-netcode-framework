using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NativeWebSocket;
using FixedMath;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Rendering;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using JetBrains.Annotations;
using System.Resources;
using RetroECS;
using UnityEngine.Experimental.AI;
using System.Linq;
using Games;
using UnityEngine.Animations;
using Newtonsoft.Json.Bson;
using System.Net;
using Hurkle;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



public static class FloatConversion
{
    public static Scaler n0 = 0;
    public static Scaler n1 = 1;
    public static Scaler n4 = 4;
    public static Scaler n8 = 8;
    public static Scaler n10 = 10;
    public static Scaler n16 = 16;
    public static Scaler n128 = 128;

    public static Scaler n0_005 = (Scaler)0.005f;
    public static Scaler n0_6 = (Scaler)0.6f;
    public static Scaler n0_01 = (Scaler)0.01f;
}



public class Controller : MonoBehaviour
{
    public string webSocketServer;

    public bool ShowOutput;
    public GameObject output;
    public GameObject RoomPanel;

    public int maxForward = 4;
    public int maxDelay = 1;

    public GameObject[] GamePrefabs;
    public Dictionary<GameId, GameObject> GamePrefabMap;

    // Helper functions
    static public sbyte SingleSignedByte(byte[] bytes)
    {
        sbyte r = -1;
        using (MemoryStream m = new MemoryStream(bytes))
        {
            using (BinaryReader reader = new BinaryReader(m))
            {
                r = reader.ReadSByte();
            }
        }
        return r;
    }

    static public byte[] ToBinary(ulong input)
    {
        using (MemoryStream m = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(m))
            {
                uint a = (uint)(input & 0x00000000ffffffff);
                ushort b = (ushort)((input & 0x0000ffff00000000) >> 32);
                byte c = (byte)((input & 0x00ff000000000000) >> 48);

                writer.Write(a);
                writer.Write(b);
                writer.Write(c);
            }
            return m.ToArray();
        }
    }

    static public void ToClass(PassedData passed, byte[] bytes)
    {
        using (MemoryStream m = new MemoryStream(bytes))
        {
            using (BinaryReader reader = new BinaryReader(m))
            {
                uint a;
                ushort b;
                byte c;
                var active = reader.ReadUInt64();
                var broken = reader.ReadUInt64();
                for (int i = 0; i < passed.RawInput.Size; ++i)
                {
                    passed.Connected[i] = (active & (ulong)(0x1UL << i)) != 0x0;
                    passed.Dropped[i] = (broken & (ulong)(0x1UL << i)) != 0x0;
                    a = reader.ReadUInt32();
                    b = reader.ReadUInt16();
                    c = reader.ReadByte();
                    passed.RawInput[i] = ((ulong)c << 48) | ((ulong)b << 32) | (ulong)a;
                    //slots[i] = ((ulong)b << 32) | (ulong)a;
                }
            }
        }
    }

    // helper data structures
    private struct QueueEntry
    {
        public QueueEntry(byte[] b)
        {
            bytes = b;
        }
        public Byte[] bytes;
    }

    private struct UploadQueueEntry
    {
        public UploadQueueEntry(ulong d)
        {
            data = d;
        }
        public ulong data;
    }

    private GameObject RunningGame = null;
    private WebSocket websocket;
    private sbyte LocalSlot;

    private PassedData Passed = new PassedData();

    private int debugFrame;

    private Queue<QueueEntry> InputQueue;
    private Queue<UploadQueueEntry> UploadQueue;

    private float deltaTime = 0.0f;
    private string deltaTimeText;

    public enum UpdateState
    {
        Null = 0,
        WaitingToConnect,
        CreatingFirst,
        WaitingOnFirst,
        Running
    }
    private UpdateState updateState;

    private int emptyFrames = 0;
    private int nonEmptyFrames = 0;
    private float waitTime = 0.0f;


    void Awake()
    {
        //QualitySettings.vSyncCount = 0;  // VSync must be disabled
        //Application.targetFrameRate = 30;
    }

    // Start is called before the first frame update
    void Start()
    {
        //ReturnUnion testret = new ReturnUnion();
        //FunctionUnion testfunc = new FunctionUnion();

        Log.text = output;

        var test1 = new Hurkle.Data();
        File.WriteAllText("test_hurkle_1.txt", JsonConvert.SerializeObject(test1.Objects));


        TextAsset ta = (TextAsset)Resources.Load("Data\\hurkle_objects");
        var obj = JsonConvert.DeserializeObject<JObject>(ta.text);
        var test2 = HurkleLoad.Load(obj);
        File.WriteAllText("test_hurkle_2.txt", JsonConvert.SerializeObject(test2));



        // do silly stuff
        //Hurkle.Cp.Prefab prefab = (Hurkle.Cp.Prefab)Prefab.Load(typeof(Hurkle.Cp.Prefab), obj);
        //Prefab.Print(prefab.GetType(), prefab);


        //foreach (var t in Cp.Table)
        //{
        //    Log.Write($"{t.Key} - {t.Value}");
        //}

        //Log.Write($"pack: {Pack.Count}");
        //Log.Write($"base: {BasePack.Count}");


        // fill dictionary of prefabs
        GamePrefabMap = new Dictionary<GameId, GameObject>();
        foreach (var p in GamePrefabs)
        {
            GamePrefabMap[p.GetComponent<BaseIO>().CurrentId()] = p;
        }

        LocalSlot = -1;

        debugFrame = 3;

        ShowOutput = false;


        InputQueue = new Queue<QueueEntry>();
        UploadQueue = new Queue<UploadQueueEntry>();

        updateState = UpdateState.WaitingToConnect;
    }

    // Update is called once per frame
    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        deltaTimeText = Mathf.Ceil(1.0f / deltaTime).ToString();

        if (Input.GetKeyDown(KeyCode.Tab) == true) {
            ShowOutput = !ShowOutput;
            output.transform.parent.gameObject.SetActive(ShowOutput);
        }


        byte[] bytes;
        ulong GameInput;
        bool done;
        UploadQueueEntry up;
        int frameSkip = 0;

        switch (updateState)
        {
            case UpdateState.WaitingToConnect:
                // don't do anything!
                break;

            case UpdateState.CreatingFirst:
                if (GamePrefabMap.ContainsKey(GameId.Intermission) == false) throw new Exception($"no game id {GameId.Intermission}!");
                RunningGame = Instantiate(GamePrefabMap[GameId.Intermission]);
                RunningGame.GetComponent<BaseIO>().Init(gameObject, LocalSlot, Passed);
                // create game here!
                updateState = UpdateState.WaitingOnFirst;
                break;

            case UpdateState.WaitingOnFirst:
                GameInput = RunningGame.GetComponent<BaseIO>().ProcessInput();

                // Send out input for this frame
                waitTime += Time.deltaTime;
                if (waitTime > 0.033f)
                {
                    deltaTime += (waitTime - deltaTime) * 0.1f;
                    deltaTimeText = Mathf.Ceil(1.0f / deltaTime).ToString();
                    waitTime -= 0.033f;

                    if (websocket != null && websocket.State == WebSocketState.Open)
                    {
                        if (UploadQueue.Count < maxForward)
                        {
                            up = new UploadQueueEntry(GameInput);
                            UploadQueue.Enqueue(up);
                            websocket.Send(ToBinary(GameInput));
                        }
                    }
                }

                done = false;
                while (!done)
                {
                    if (InputQueue.Count == 0)
                    {
                        done = true;
                    }
                    else
                    {
                        bytes = InputQueue.Peek().bytes;
                        ToClass(Passed, bytes);
                        if (Passed.Dropped[LocalSlot] == false)
                        {
                            updateState = UpdateState.Running;
                            done = true;
                        }
                        else
                        {
                            InputQueue.Dequeue();
                        }
                    }
                }
                break;

            case UpdateState.Running:
                GameInput = RunningGame.GetComponent<BaseIO>().ProcessInput();

                // Send out input for this frame
                waitTime += Time.deltaTime;
                if (waitTime > 0.033f)
                {
                    deltaTime += (waitTime - deltaTime) * 0.1f;
                    deltaTimeText = Mathf.Ceil(1.0f / deltaTime).ToString();
                    waitTime -= 0.033f;

                    if (websocket != null && websocket.State == WebSocketState.Open)
                    {
                        if (frameSkip > 0)
                        {
                            frameSkip--;
                        }
                        else
                        {
                            if (UploadQueue.Count < maxForward)
                            {
                                up = new UploadQueueEntry(GameInput);
                                UploadQueue.Enqueue(up);
                                websocket.Send(ToBinary(GameInput));
                            }
                        }
                    }
                }

                int frameCount = 0;
                int dropped = 0;
                // pick off the incoming stream and update the frame
                while (InputQueue.Count > 0)
                {
                    ToClass(Passed, InputQueue.Dequeue().bytes);
                    if (Passed.Dropped[LocalSlot] == false)
                    {
                        UploadQueue.Dequeue();
                        nonEmptyFrames++;
                    }
                    else
                    {
                        frameSkip++;
                        dropped++;
                        //UploadQueue.Dequeue();
                        emptyFrames++;
                    }
                    frameCount++;
                    
                    RunningGame.GetComponent<BaseIO>().MainUpdate();
                    if (Passed.NextGame != GameId.None)
                    {
                        if (GamePrefabMap.ContainsKey(GameId.Intermission) == false) throw new Exception($"no game id {Passed.NextGame}!");
                        Destroy(RunningGame);
                        RunningGame = Instantiate(GamePrefabMap[Passed.NextGame]);
                        RunningGame.GetComponent<BaseIO>().Init(gameObject, LocalSlot, Passed);
                    }
                }


                // copy subset of gamestate for the fast forward 
                //forwardGame.Copy(game);
                RunningGame.GetComponent<BaseIO>().PrepForward();

                // fast forward until...

                //Debug.Log("--------------------");
                int frameCounter = 0;
                foreach (var ui in UploadQueue)
                {
                    if (frameCounter < UploadQueue.Count - maxDelay)
                    {
                        //Debug.Log($"{ui.frame}");
                        // insert next player input
                        Passed.RawInput[LocalSlot] = ui.data;
                        Passed.Dropped[LocalSlot] = false;
                        //forwardGame.SetInput(Connected, Dropped, RawInput);
                        //forwardGame.Update();
                        RunningGame.GetComponent<BaseIO>().ForwardUpdate();
                    }
                    frameCounter++;
                }

                //  IO.ProcessOutput();
                //RunningGame.GetComponent<BaseIO>().ProcessOutput();
                break;

            default:
                break;
        }



    }

    void LateUpdate()
    {
        if (updateState == UpdateState.Running)
        {
            RunningGame.GetComponent<BaseIO>().ProcessOutput();
        }
        if (ShowOutput == true)
        {
            var s = "";

            s += $"{Input.mousePosition.x}, {Input.mousePosition.y}\r\n";
            s += $"dropped: {emptyFrames}/{(emptyFrames + nonEmptyFrames)}\r\n";


            /*var contacts = (game.game as Hurkle.Game).Contacts;
            for (int i = 0; i < contacts.Size; i++)
            {
                ref var c = ref contacts[i];
                s += $"{c.dq.Distance}\r\n";
            }*/

            //output.GetComponent<Text>().text = s;
        }
    }

    private void ChangeRunning(GameId id)
    {

    }

    private void JustLeave()
    {
        if (websocket != null)
        {
            Leave();
        }
    }

    public void ReadyButtonClick()
    {
        //GameInput.State = 1;
    }

    public void JustJoin(string s)
    {
        if (websocket == null)
        {
            RoomPanel.SetActive(false);
            //ReadyPanel.SetActive(true);
            Join(s);
        }
    }

    private void JoinOrLeave(string s)
    {
        if (websocket == null)
        {
            Join(s);
        }
        else
        {
            Leave();
        }
    }

    void Leave()
    {
        websocket.Close();
        //websocket = null;
    }

    void Join(string s)
    {
        websocket = new WebSocket($"{webSocketServer}{s}");

        websocket.OnOpen += () => {
            UnityEngine.Debug.Log("Connection open!");
            ulong TestInput = 0x0000000000000000;
           
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                /*UploadQueue.Enqueue(new Util.UploadQueueEntry(sentFrame, TestInput));
                sentFrame++;
                websocket.Send(Util.ToBinary(TestInput));

                UploadQueue.Enqueue(new Util.UploadQueueEntry(sentFrame, TestInput));
                sentFrame++;
                websocket.Send(Util.ToBinary(TestInput));

                UploadQueue.Enqueue(new Util.UploadQueueEntry(sentFrame, TestInput));
                sentFrame++;
                websocket.Send(Util.ToBinary(TestInput));*/
            }
            else
            {
                Debug.Log("This is a problem");
            }

        };
        
        websocket.OnError += (e) => { UnityEngine.Debug.Log("Error! " + e); };
        websocket.OnClose += (e) => { UnityEngine.Debug.Log("Connection closed!"); };

        websocket.OnMessage += (bytes) => {
            // do something based on size of message!  set player slot is 1 byte.
            if (bytes.Length == 1)
            {
                LocalSlot = SingleSignedByte(bytes);
                updateState = UpdateState.CreatingFirst;
                UnityEngine.Debug.Log($"Assigned {LocalSlot}");
            }
            else
            {
                //Debug.Log($"bytes.length: {bytes.Length}");
                InputQueue.Enqueue(new QueueEntry(bytes));
            }
        };

        // waiting for messages
        websocket.Connect();
    }
}
