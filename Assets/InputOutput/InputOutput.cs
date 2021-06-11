using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using RetroECS;
using System;
using Games;
using Newtonsoft.Json.Linq;

/*
public static class Prefab
{
    public static object Load(Type t, JToken token)
    {
        //Log.ForceLine($"FUNCTION: {t}: {token}");
        object r = null;
        if (token != null)
        {
            if (t == typeof(byte)) { r = token.Value<byte>(); }
            else if (t == typeof(bool)) { r = token.Value<bool>(); }
            else if (t == typeof(ushort)) { r = token.Value<ushort>(); }
            else if (t == typeof(FixedMath.Scaler)) { r = (FixedMath.Scaler)token.Value<float>(); }
            else if (t == typeof(Flags)) { r = new Flags(token.Value<ushort>()); }
            else if (t == typeof(byte[])) { }
            else
            {
                var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance).ToArray();
                r = Activator.CreateInstance(t);
                foreach (var field in fields)
                {
                    //Log.ForceLine($"{field.Name}: {field.FieldType}");
                    field.SetValue(r, Load(field.FieldType, token.Value<JObject>()[field.Name]));
                }
            }
        }
        return r;
    }

    public static void Print(Type t, object o)
    {
        var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance).ToArray();
        if (fields.Length == 0)
        {
            Log.ForceLine($"{t}: {o}");
        }
        else
        {
            foreach (var field in fields)
            {
                Print(field.FieldType, field.GetValue(o));
            }
        }
    }
}
*/


public abstract class BaseIO : MonoBehaviour
{
    static public Sprite[] GenerateSpriteTable(Type t, string path)
    {
        Sprite[] table = Resources.LoadAll<Sprite>(path);
        var names = Enum.GetNames(t);
        for (ushort i = 0; i < names.Length; ++i)
        {
            bool fail = true;
            for (int j = 0; j < table.Length; ++j)
            {
                if (names[i] == table[j].name)
                {
                    Sprite swap = table[i];
                    table[i] = table[j];
                    table[j] = swap;
                    fail = false;
                }
            }
            if (fail == true)
            {
                Debug.Log($"failed to locate sprite name: {names[i]}");
            }
        }
        return table;
    }

    protected PassedData Passed;
    protected GameObject mainCamera;
    protected sbyte LocalSlot;

    public virtual GameId CurrentId() => GameId.None;

    public virtual void Init(GameObject c, sbyte l, PassedData p)
    {
        mainCamera = c;
        LocalSlot = l;
        Passed = p;
        Passed.NextGame = GameId.None;
    }
   

    public virtual void MainUpdate()
    {

    }

    public virtual void ForwardUpdate()
    {

    }

    public virtual void PrepForward()
    {

    }

    public virtual ulong ProcessInput()
    {
        return 0x0000000000000000;
    }
   

    public virtual void ProcessOutput()
    {

    }

   
}
