using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Numerics;


namespace Lab2
{
    struct DataItem
    {
        public Vector2 xy { get; set; }
        public Complex field { get; set; }
        public DataItem(Vector2 vvalue, Complex fvalue)
        {
            xy = vvalue;
            field = fvalue;
        }
        public string ToLongString(string format)
        {
            return $" X: {xy.X.ToString(format)}, Y: {xy.Y.ToString(format)}, Value: {field.ToString(format)}, Abs: {Complex.Abs(field).ToString(format)}";
        }
        public override string ToString()
        {
            return ToLongString("");
        }
    }

    public delegate Complex Fv2Complex(Vector2 v2);

    abstract class V2Data : IEnumerable<DataItem>
    {
        public abstract IEnumerator<DataItem> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public string id { get; }
        public DateTime date { get; }
        public V2Data(string id_, DateTime date_)
        {
            id = id_;
            date = date_;
        }
        public abstract int Count { get; }
        public abstract float MinDistance { get; }
        public abstract string ToLongString(string format);
        public override string ToString()
        {
            return $"Str: {id}, Date: {date}";
        }
    }

    class V2DataList : V2Data, IEnumerable<DataItem>
    {
        public List<DataItem> ItemList { get; }
        public override IEnumerator<DataItem> GetEnumerator()
        {
            return new ListEnumerator(ItemList);
        }

        public V2DataList(string id, DateTime d) : base(id, d)
        {
            ItemList = new List<DataItem>();
        }
        public bool Add(DataItem newItem)
        {
            if (ItemList.Exists(x => x.xy == newItem.xy))
            {
                return false;
            }
            else
            {
                ItemList.Add(newItem);
                return true;
            }
        }
        public int AddDefaults(int nItems, Fv2Complex F)
        {
            int n = 0;
            int k = 1;
            int x = k;
            int y = k;
            for (int i = 0; i < nItems; ++i)
            {
                x = k + x;
                y = k + y;
                k += 1;
                Vector2 v = new Vector2(x, y);
                Complex field = F(v);
                DataItem newItem = new DataItem(v, field);
                if (Add(newItem)) ++n;
            }
            return n;
        }
        public override int Count
        {
            get => ItemList.Count;
        }
        public override float MinDistance
        {
            get
            {
                float min_distance = float.MaxValue;
                for (int i = 0; i < Count; ++i)
                    for (int j = i + 1; j < Count; ++j)
                    {
                        float cur_distance = Vector2.Distance(ItemList[i].xy, ItemList[j].xy);
                        if (cur_distance <= min_distance)
                        {
                            min_distance = cur_distance;
                        }
                    }
                if (Count == 0 || Count == 1) min_distance = 0;
                return min_distance;
            }
        }
        public override string ToString()
        {
            return "V2DataList: " + base.ToString() + " Count: " + ItemList.Count;
        }
        public override string ToLongString(string format)
        {
            string str = "";
            for (int i = 0; i < Count; ++i)
            {
                str += "\n" + ItemList[i].ToLongString(format);
            }
            return ToString() + str + "\n";
        }
    }

    class V2DataArray : V2Data, IEnumerable<DataItem>
    {
        public override IEnumerator<DataItem> GetEnumerator()
        {
            return new ArrayEnumerator(values, nx, ny, nxy);
        }
        public Complex[,] values { get; }
        public int nx { get; }
        public int ny { get; }
        public Vector2 nxy { get; }
        public V2DataArray(string id, DateTime d) : base(id, d)
        {
            values = new Complex[0, 0];
        }
        public V2DataArray(string id, DateTime d, int nx, int ny, Vector2 nxy, Fv2Complex F) : base(id, d)
        {
            this.nx = nx;
            this.ny = ny;
            this.nxy = nxy;
            values = new Complex[nx, ny];
            for (int i = 0; i < nx; ++i)
            {
                for (int j = 0; j < ny; ++j)
                {
                    float x = i * nxy.X;
                    float y = j * nxy.Y;
                    Vector2 xy = new Vector2(x, y);
                    values[i, j] = F(xy);
                }
            }
        }
        public override int Count
        {
            get => nx * ny;
        }
        public override float MinDistance
        {
            get
            {
                float min_distance = float.MaxValue;
                min_distance = Math.Min(nxy.X, nxy.Y);
                if (nx == 1) min_distance = nxy.Y;
                if (ny == 1) min_distance = nxy.X;
                if (Count == 0 || Count == 1) min_distance = 0;
                return min_distance;
            }
        }
        public override string ToString()
        {
            return $"V2DataArray: {base.ToString()}, nx: {nx}, ny: {ny}, nxy: {nxy.X} {nxy.Y}";
        }
        public override string ToLongString(string format)
        {
            string str = "";
            for (int i = 0; i < nx; ++i)
            {
                for (int j = 0; j < ny; ++j)
                {
                    str += $"\n X: {(i * nxy.X).ToString(format)}, Y: {(j * nxy.Y).ToString(format)}, Value: {values[i, j].ToString(format)}, Abs: {Complex.Abs(values[i, j]).ToString(format)}";
                }
            }
            return ToString() + str + "\n";
        }

        public static implicit operator V2DataList(V2DataArray arr)
        {
            V2DataList list = new V2DataList(arr.id, arr.date);
            for (int i = 0; i < arr.nx; ++i)
            {
                for (int j = 0; j < arr.ny; ++j)
                {
                    float x = i * arr.nxy.X;
                    float y = j * arr.nxy.Y;
                    Vector2 xy = new Vector2(x, y);
                    Complex field = arr.values[i, j];
                    DataItem newItem = new DataItem(xy, field);
                    list.Add(newItem);
                }
            }
            return list;
        }
    }

    class V2MainCollection
    {
        private List<V2Data> V2DataList;
        public int Count
        {
            get => V2DataList.Count;
        }
        public V2Data this[int i]
        {
            get => V2DataList[i];
        }
        public bool Contains(string ID)
        {
            return V2DataList.Exists(x => x.id == ID);
        }
        public V2MainCollection()
        {
            V2DataList = new List<V2Data>();
        }
        public bool Add(V2Data v2Data)
        {
            if (Contains(v2Data.id))
            {
                return false;
            }
            else
            {
                V2DataList.Add(v2Data);
                return true;
            }
        }
        public string ToLongString(string format)
        {
            string str = "";
            foreach (var data in V2DataList)
            {
                str += "\n" + " " + data.ToLongString(format);
            }
            return str;
        }
        public override string ToString()
        {
            string str = "";
            foreach (var data in V2DataList)
            {
                str += data.ToString() + "\n";
            }
            return str;
        }
    }

    class Program
    {
        public static Complex F1(Vector2 v2)
        {
            return new Complex(v2.X, v2.Y);
        }
        static void Main()
        {
            //1. V2DataArray
            Console.WriteLine("\n" + "1. V2DataArray" + "\n");
            V2DataArray arr = new V2DataArray("Object", new DateTime(2021, 01, 01), 3, 2, new Vector2(1.5f, 1.5f), F1);
            Console.WriteLine(arr.ToLongString("N1"));
            V2DataList list = arr;
            Console.WriteLine(list.ToLongString("N1"));
            Console.WriteLine($"Array count: {arr.Count}, Array MinDistance: {arr.MinDistance}");
            Console.WriteLine($"List Count: {list.Count}, List MinDistance: {list.MinDistance}");

            //2. V2MainCollection
            Console.WriteLine("\n" + "2. V2MainCollection" + "\n");
            V2MainCollection collection = new V2MainCollection();
            V2DataArray arr2 = new V2DataArray("Object2_2", new DateTime(2021, 01, 01), 2, 1, new Vector2(0.5f, 1.0f), F1);
            V2DataList list1 = new V2DataList("List_1", new DateTime(2021, 01, 01));
            collection.Add(arr);
            collection.Add(arr2);
            collection.Add(list1);
            collection.Add(list);
            Console.WriteLine(collection.ToLongString("N1"));
            Console.WriteLine($"Collection count: {collection.Count}");

            //3. Count и MinDistance 
            Console.WriteLine("\n" + "3. Count и MinDistance" + "\n");
            for (int i = 0; i < collection.Count; ++i)
            {
                Console.WriteLine($"Count: {collection[i].Count}, MinDistance: {collection[i].MinDistance}");
            }

            //4. Testing Iterating
            Console.WriteLine("\n" + "4. Testing Iterating" + "\n");
            foreach (var i in arr)
            {
                Console.WriteLine(i);
            }
        }
    }
}
