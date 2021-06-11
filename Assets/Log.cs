using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

public static class Log
{
    private static int clear_file = ClearFile();
    private static Stopwatch watch = new Stopwatch();
    public static GameObject text = null;


    static public int ClearFile()
    {
        var sw = new StreamWriter("special_debug_log.txt");
        sw.Close();
        return 0;
    }

    static public void Start()
    {
        watch.Reset();
        watch.Start();
    }

    static public void Stop()
    {
        watch.Stop();
    }
    static public long GetTime()
    {
        return watch.ElapsedMilliseconds;
    }

    static public void Write(string s)
    {
        UnityEngine.Debug.Log(s);
    }
    static public void Force(string s = "")
    {
        var sw = new StreamWriter("special_debug_log.txt", true);
        sw.Write(s);
        sw.Close();
    }
    static public void ForceLine(string s = "")
    {
        var sw = new StreamWriter("special_debug_log.txt", true);
        sw.WriteLine(s);
        sw.Close();
    }

    static public void ConosleDump(System.Object o)
    {
        var s = JsonConvert.SerializeObject(o, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new OnlyFieldsPropertiesResolver()
        });
        text.GetComponent<InputField>().text = s;
    }
}


public class OnlyFieldsPropertiesResolver : DefaultContractResolver
{
    public OnlyFieldsPropertiesResolver()
    {    
    }
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        //Choose the properties you want to serialize/deserialize
        var props = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var l = new List<JsonProperty>();
        foreach (var p in props)
        {
            l.Add(CreateProperty(p, memberSerialization));
        }
        return l;
    }
}

