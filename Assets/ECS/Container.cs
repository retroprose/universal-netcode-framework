using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


namespace RetroECS
{

    /*
        ContainerUtil 
     
        Flag
        Flags

        IContainer

        Single<T>
        Vector<T>
        PoolVector (ushort)
        PoolVector<T>

        ComponentId

        Components
        Components<T>

        *HashMap<T>
    */


    public static class ContainerUtil
    {
        // Return Values
        public static int ComputeId(Type e, Type t)
        {
            int id = 0;
            var names = Enum.GetNames(e);
            foreach (var name in names)
            {
                if (name.TrimStart('_') == t.Name)
                {
                    id = (int)Enum.Parse(e, name);
                }
            }
            return id;
        }
    }


    public struct Flag
    {
        public byte m_value;

        public Flag(byte b)
        {
            m_value = b;
        }

        public static implicit operator Flag(byte b) => new Flag(b);

        static public Flags operator |(Flag lhs, Flags rhs)
        {
            return (Flags)lhs | rhs;
        }
    }


    public struct Flags
    {
        static public readonly Flags None = new Flags(0x0000);

        public ushort m_value;

        public static implicit operator Flags(Flag f) => new Flags((ushort)(0x0001 << f.m_value));

        public Flags(ushort v)
        {
            m_value = v;
        }

        public bool Contains(Flags other)
        {
            return (m_value & other.m_value) == other.m_value;
        }

        static public Flags operator |(Flags lhs, Flags rhs)
        {
            return new Flags((ushort)(lhs.m_value | rhs.m_value));
        }

        static public Flags operator ^(Flags lhs, Flags rhs)
        {
            return new Flags((ushort)(lhs.m_value ^ rhs.m_value));
        }

        static public Flags operator &(Flags lhs, Flags rhs)
        {
            return new Flags((ushort)(lhs.m_value & rhs.m_value));
        }

        static public Flags operator ~(Flags value)
        {
            return new Flags((ushort)(~value.m_value));
        }
    }





    public interface IContainer
    {
        void Copy(IContainer other);
        bool Insert(int index);
        void Resize(int size);
        //void CopyPaste(int from, int to);
    }


   

    public class Single<T> : IContainer where T : struct
    {
        public T m_data;

        public void Insert(T element)
        {
            m_data = element;
        }
        public ref T Ref
        {
            get { return ref m_data; }
        }

        public void CopyPaste(int from, int to)
        {
            // do nothing!
        }

        public void Copy(Single<T> other)
        {
            m_data = other.m_data;
        }

        public void Copy(IContainer other)
        {
            Copy((Single<T>)other);
        }

        public bool Insert(int index)
        {
            return true;
        }

        public void Resize(int size)
        {

        }
    }


    public class ComponentId
    {
        public ushort m_value;
        public string m_name;

        public ComponentId(string n, ushort v)
        {
            m_value = v;
            m_name = n;
        }

        public override string ToString()
        {
            return $"{m_value}: {m_name}";
        }

        public static implicit operator ushort(ComponentId id) => id.m_value;
        public static implicit operator int(ComponentId id) => id.m_value;

        public static implicit operator Flags(ComponentId id) => new Flags((ushort)(0x0001 << id.m_value));

        static public Flags operator |(ComponentId lhs, Flags rhs)
        {
            return (Flags)lhs | rhs;
        }
    }


    public class Vector<T> : IContainer where T : struct
    {
        public int m_size;
        public T[] m_list;

        public Vector()
        {
            m_size = 0;
            m_list = new T[1];
        }

        public Vector(int size)
        {
            m_size = size;
            m_list = new T[m_size];
        }
        public void Clear()
        {
            m_size = 0;
        }

        public void Resize(int newSize)
        {
            int size = m_list.Length;
            while (newSize > size)
            {
                size += size;
            }
            if (size > m_list.Length)
            {
                Grow(size);
            }
            m_size = newSize;
        }

        public bool Append()
        {
            if (m_size == m_list.Length)
            {
                Grow(m_list.Length + m_list.Length);
            }
            ++m_size;
            return true;
        }

        public bool Insert(int index)
        {
            int size = m_list.Length;
            while (index >= size)
            {
                size += size;
            }
            if (size > m_list.Length)
            {
                Grow(size);
            }
            if (m_size < index + 1)
            {
                m_size = index + 1;
            }
            return true;
        }

        public int Size
        {
            get { return m_size; }
        }

        public int Capacity
        {
            get { return m_list.Length; }
        }

        public ref T this[int i]
        {
            get { return ref m_list[i]; }
        }

        public ref T Last
        {
            get { return ref m_list[m_size - 1]; }
        }

        public void CopyPaste(int from, int to)
        {
            m_list[to] = m_list[from];
        }

        public void Copy(Vector<T> other)
        {
            if (other.m_list.Length > m_list.Length)
            {
                m_list = new T[other.m_list.Length];
            }
            Array.Copy(other.m_list, m_list, other.m_size);
            m_size = other.m_size;
        }

        public void Copy(IContainer other)
        {
            Copy((Vector<T>)other);
        }

        public delegate bool LessThan(ref T lhs, ref T rhs);
        public void Sort(LessThan f)
        {
            // do things
            T swap;
            bool done = false;
            while (!done)
            {
                done = true;
                for (int i = 0; i < m_size - 1; ++i)
                {
                    if (f(ref m_list[i + 1], ref m_list[i]) == true)
                    {
                        swap = m_list[i];
                        m_list[i] = m_list[i + 1];
                        m_list[i + 1] = swap;
                        done = false;
                    }
                }
            }
        }

        protected void Grow(int size)
        {
            var temp = m_list;
            m_list = new T[size];
            Array.Copy(temp, m_list, m_size);
            temp = null;
        }
    }




    public interface IPoolable
    {
        int Link { get; set; }
        bool Active { get; set; }
    }

    public class PoolVector<T> : Vector<T> where T : struct, IPoolable
    {
        public static readonly int EndOfList = 0;

        public int m_head;

        public PoolVector() : base()
        {
            m_head = EndOfList;
            // started with zero removed from list
            Reset();
        }

        public void Reset()
        {
            m_size = 1;
            m_head = EndOfList;
            m_list[0] = new T();
        }

        public void Copy(PoolVector<T> other)
        {
            m_head = other.m_head;
            base.Copy(other);
        }

        public new void Copy(IContainer other)
        {
            Copy((PoolVector<T>)other);
        }

        public void Store(ref T t)
        {
            int value = m_head;
            if (value == EndOfList)
            {
                if (m_size == m_list.Length)
                {
                    // don't do anything
                    Grow(m_list.Length + m_list.Length);
                }
                //else
                {
                    value = (ushort)m_size;
                    ++m_size;
                    m_list[value] = t;
                }
            }
            else
            {
                m_head = m_list[value].Link;
                m_list[value] = t;
            }
            t.Link = value;
        }

        public void Retrieve(ref T t)
        {
            int value = t.Link;
            if (m_list[value].Active == true)
            {
                t = m_list[value];
                m_list[value].Link = m_head;
                m_list[value].Active = false;
                m_head = value;
            }
        }


    }

    public class PoolVector : Vector<ushort> 
    {
        public static readonly ushort EndOfList = 0;

        public ushort m_head;

        public PoolVector() : base()
        {
            m_head = EndOfList;
            // started with zero removed from list
            Reset();
        }

        public void Reset()
        {
            m_size = 1;
            m_head = EndOfList;
            m_list[0] = EndOfList;
        }

        public void Copy(PoolVector other)
        {
            m_head = other.m_head;
            base.Copy(other);
        }

        public new void Copy(IContainer other)
        {
            Copy((PoolVector)other);
        }

        public ushort Allocate()
        {
            ushort value = m_head;
            if (value == EndOfList)
            {
                if (m_size == m_list.Length)
                {
                    // don't do anything
                    Grow(m_list.Length + m_list.Length);
                }
                //else
                {
                    value = (ushort)m_size;
                    ++m_size;
                    m_list[value] = EndOfList;
                }
            }
            else
            {
                m_head = m_list[value];
                m_list[value] = EndOfList;
            }
            return value;
        }

        public void Free(ushort value)
        {
            if (m_list[value] == EndOfList)
            {
                m_list[value] = m_head;
                m_head = value;
            }
        }


    }



    



    public class Components : IContainer
    {
        public IContainer[] CompleteList;
        public IContainer[] ComponentList;

        public void Copy(IContainer other)
        {
            Copy((Components)other);
        }

        public void Copy(Components other)
        {
            for (int i = 0; i < CompleteList.Length; ++i)
            {
                if (CompleteList[i] != null)
                {
                    CompleteList[i].Copy(other.CompleteList[i]);
                }
            }
        }

        public bool Insert(int index)
        {
            bool fail = false;
            for (int i = 0; i < ComponentList.Length; ++i)
            {
                if (ComponentList[i] != null)
                {
                    if (ComponentList[i].Insert(index) == false)
                    {
                        fail = true;
                    }
                }
            }
            return fail;
        }

        public void Resize(int size)
        {
            for (int i = 0; i < CompleteList.Length; ++i)
            {
                if (CompleteList[i] != null)
                {
                    CompleteList[i].Resize(size);
                }
            }
        }

    }



    public class Components<T> : Components where T : Components<T>
    {
        public static Dictionary<string, ComponentId> Table = GenTable();
        
        public static ComponentId Component;
        //public static ComponentId Generation;
        //public static ComponentId Manager;

        public Vector<Flags> _Component;
        public Vector<ushort> _Generation;
        public PoolVector _Manager;


        public struct Handle
        {
            static public Handle Null = new Handle(0, 0);

            public Handle(ushort g, ushort i)
            {
                generation = g;
                index = i;
            }

            public static implicit operator int(Handle h) => h.index;

            public static bool operator ==(Handle lhs, Handle rhs)
            {
                return (lhs.generation == rhs.generation && lhs.index == rhs.index);
            }

            public static bool operator !=(Handle lhs, Handle rhs)
            {
                return (lhs.generation != rhs.generation || lhs.index != rhs.index);
            }

            public ushort generation;
            public ushort index;
        }


        public static Dictionary<string, ComponentId> GenTable()
        {
            var table = new Dictionary<string, ComponentId>();
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.FieldType == typeof(ComponentId)).ToArray();

            Component = new ComponentId("Component", 0);
            table["Component"] = Component;

            ushort id = 1;
            foreach (var field in fields)
            {
                var cid = new ComponentId(field.Name, id);
                field.SetValue(null, cid);
                table[field.Name] = cid;
                id++;
            }

            return table;
        }

        public Components()
        {
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => f.FieldType != typeof(IContainer[])).ToArray();
            //var fields = typeof(T).
            //    GetFields(BindingFlags.Public | BindingFlags.Instance).
            //    Where(f =>
            //        f.Name != "CompleteList" &&
            //        f.Name != "ComponentList"
            //    ).ToArray();

            CompleteList = new IContainer[fields.Length];
            ComponentList = new IContainer[Table.Count];

            int index = 0;
            foreach (var f in fields)
            {
                var component = (IContainer)Activator.CreateInstance(f.FieldType);
                f.SetValue(this, component);
                CompleteList[index] = component;

                string name = f.Name.TrimStart('_');
                if (Table.ContainsKey(name))
                {
                    ComponentList[Table[name].m_value] = component;
                }

                index++;
            }
        }


        


        public void Clear()
        {
            for (int i = 0; i < _Generation.Capacity; ++i)
            {
                _Generation[i] = 0;
            }
            Resize(0);
            _Manager.Reset();
        }


        public bool Valid(Handle entity)
        {
            return entity != Handle.Null && entity.generation == _Generation[entity.index];
        }

        public Handle FromIndex(ushort index)
        {
            return new Handle(_Generation[index], index);
        }

        public void Destroy(Handle entity)
        {
            if (Valid(entity) == true)
            {
                ++_Generation[entity.index];
                _Manager.Free(entity.index);
                _Component[entity.index] = Flags.None;
            }

        }

        public Handle Create()
        {
            Handle entity = Handle.Null;
            ushort value = _Manager.Allocate();
            if (value != PoolVector.EndOfList)
            {
                _Generation.Insert(value);
                Insert(value);
                entity.index = value;
                entity.generation = _Generation[entity.index];
            }
            return entity;
        }


        public bool MoveNext(ref int index, Flags mask)
        {
            for (;;)
            {
                index++;
                if (index >= _Component.Size)
                {
                    break;
                }
                if (_Component[index].Contains(mask) == true)
                {
                    break;
                }
            }
            return index < _Component.Size;
        }

      
    }






}