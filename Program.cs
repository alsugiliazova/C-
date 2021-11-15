using System;
using System.Collections.Generic;
using System.Collections;
using System.Numerics;
using System.Linq;
using System.IO;
using System.Globalization;

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
        public double Abs
        {
            get => Complex.Abs(field);
        }
        public string ToLongString(string format)
        {
            return $" X: {xy.X.ToString(format)}, Y: {xy.Y.ToString(format)}, Value: {field.ToString(format)}, Abs: {Abs.ToString(format)}";
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
        public string id { get; protected set; }
        public DateTime date { get; protected set; }
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
        public abstract double Abs(DataItem item);
        public abstract double MaxAbs { get; }
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
        public override double Abs(DataItem item)
        {
            return Complex.Abs(item.field);
        }
        public override double MaxAbs
        {
            get
            {
                double max = 0;
                foreach (DataItem item in ItemList)
                {
                    if (Abs(item) > max)
                        max = Abs(item);
                }
                return max;
            }
        }

        public bool SaveBinary(string filename)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
                {
                    writer.Write(id);
                    writer.Write($"{date}");
                    writer.Write(Count);
                    foreach (var item in ItemList)
                    {
                        writer.Write(item.xy.X);
                        writer.Write(item.xy.Y);
                        writer.Write(item.field.Real);
                        writer.Write(item.field.Imaginary);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Success!");
            }
            return true;
        }
        public bool LoadBinary(string filename, ref V2DataList v2)
        {
            try
            {
                string str;
                string d;
                DateTime date;
                int count;
                float vx, vy;
                double cr, ci;
                if (File.Exists(filename))
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
                    {
                        str = reader.ReadString();
                        d = reader.ReadString();
                        date = DateTime.Parse(d);
                        V2DataList lst = new V2DataList(str, date);
                        count = reader.ReadInt32();
                        for (int i = 0; i < count; ++i)
                        {
                            vx = reader.ReadSingle();
                            vy = reader.ReadSingle();
                            cr = reader.ReadDouble();
                            ci = reader.ReadDouble();
                            lst.Add(new DataItem(new Vector2(vx, vy), new Complex(cr, ci)));
                        }
                        v2 = lst;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Success!");
            }
            return true;
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
        public override double Abs(DataItem item)
        {
            V2DataList V2list = this;
            return V2list.Abs(item);
        }
        public override double MaxAbs
        {
            get
            {
                V2DataList V2list = this;
                return V2list.MaxAbs;
            }
        }
        public bool SaveAsText(string filename)
        {
            CultureInfo cultureInfo = new CultureInfo("ru-RU");
            cultureInfo.NumberFormat.NumberDecimalSeparator = ",";
            try
            {
                StreamWriter sw = new StreamWriter(filename);
                sw.WriteLine($"{id}\n{date}\n{nx}\n{ny}\n{nxy.X}\n{nxy.Y}");
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Saved.");
            }
            return true;
        }
        public bool LoadAsText(string filename, ref V2DataArray v2)
        {
            CultureInfo cultureInfo = new CultureInfo("ru-RU");
            cultureInfo.NumberFormat.NumberDecimalSeparator = ",";
            try
            {
                StreamReader sr = new StreamReader(filename);
                string str = sr.ReadLine();
                DateTime date = DateTime.Parse(sr.ReadLine());
                int nx = int.Parse(sr.ReadLine());
                int ny = int.Parse(sr.ReadLine());
                float x = float.Parse(sr.ReadLine());
                float y = float.Parse(sr.ReadLine());
                Vector2 nxy = new Vector2(x, y);
                v2 = new V2DataArray(str, date, nx, ny, nxy, F1);
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Loaded.");
            }

            return true;
        }
        public static Complex F1(Vector2 v2)
        {
            return new Complex(v2.X, v2.Y);
        }

    }

    class V2MainCollection
    {
        private List<V2Data> V2DataList_;
        public int Count
        {
            get => V2DataList_.Count;
        }
        public V2Data this[int i]
        {
            get => V2DataList_[i];
        }
        public bool Contains(string ID)
        {
            return V2DataList_.Exists(x => x.id == ID);
        }
        public V2MainCollection()
        {
            V2DataList_ = new List<V2Data>();
        }
        public bool Add(V2Data v2Data)
        {
            if (Contains(v2Data.id))
            {
                return false;
            }
            else
            {
                V2DataList_.Add(v2Data);
                return true;
            }
        }
        public string ToLongString(string format)
        {
            string str = "";
            foreach (var data in V2DataList_)
            {
                str += "\n" + " " + data.ToLongString(format);
            }
            return str;
        }
        public override string ToString()
        {
            string str = "";
            foreach (var data in V2DataList_)
            {
                str += data.ToString() + "\n";
            }
            return str;
        }
        public double Max
        {
            get
            {
                double max = 0;
                foreach (var item in V2DataList_)
                {
                    if (item.MaxAbs > max)
                        max = item.MaxAbs;
                }
                return max;
            }
        }
        private V2DataList ToDataList(V2Data elem)
        {
            if(elem is V2DataArray)
            {
                return elem as V2DataArray;
            }
            else
            {
                return elem as V2DataList;
            }
        }
        /* 1. Свойство, возварщающее объект DataItem с максимальным *
         *    по модулю значенияем модуля поля.                     */
        public DataItem? MaxItemX
        {
            get
            {
                if (IsAllNull) return null;
                var data = V2DataList_.Select(ToDataList);
                var a = from list in data
                        from item in list
                        where list.Abs(item) == Max
                        select item;
                return a.First();
            }
        }
        public bool IsAllNull
        {
            get
            {
                foreach (var elem in V2DataList_)
                {
                    if (elem.Count != 0) return false;
                }
                return true;
            }
        }

        /* 2. Точки измерения поля, которые есть в элементах типа V2DataList, *
         *    но их нет в элементах V2DataArray.                              */
        public IEnumerable<Vector2> ListWithoutArray
        {
            get
            {
                //var Array = V2DataList_.Where(x => x is V2DataArray).Select(ToDataList);
                var vectorsDataArray = from data in V2DataList_.Where(x => x is V2DataArray).Select(ToDataList)
                                       from vector in data
                                       select vector.xy;
                //var List = V2DataList_.Where(x => x is V2DataList).Select(ToDataList);
                var vectorsDataList = from data in V2DataList_.Where(x => x is V2DataList).Select(ToDataList)
                                      from vector in data
                                      select vector.xy;
                return vectorsDataList.Except(vectorsDataArray).Distinct();
            }
        }

        /* 3. Группировка элементов из List<V2Data>, по числу результатов измерения поля */
        public IEnumerable<IGrouping<int, V2Data>> Group
        {
            get
            {
                var group = from list in V2DataList_.Select(ToDataList)
                            group list by list.Count into newGroup
                            select newGroup;
                return group;
            }
        }
    }


    class Program
    {
        public static Complex F1(Vector2 v2)
        {
            return new Complex(v2.X, v2.Y);
        }
        static void TestFiles()
        {
            V2DataArray arr1 = new V2DataArray("Array №1 ", new DateTime(2021, 11, 15), 2, 2, new Vector2(1.0f, 1.5f), F1);
            V2DataArray arr2 = new V2DataArray("Array №2 ", new DateTime(2021, 11, 16));
            arr1.SaveAsText("test.txt");
            arr2.LoadAsText("test.txt", ref arr2);
            Console.WriteLine("\nSaved Array:");
            Console.WriteLine(arr1.ToString());
            Console.WriteLine("\nLoaded Array:");
            Console.WriteLine(arr2.ToString());
            Console.WriteLine();
            V2DataList list1 = new V2DataList("List №1", new DateTime(2021, 11, 15));
            list1.Add(new DataItem(new Vector2(3, 4), new Complex(2, 5)));
            list1.Add(new DataItem(new Vector2(1, 4), new Complex(4, 2.5)));
            V2DataList list2 = new V2DataList("List №2", new DateTime(2021, 11, 16));
            list1.SaveBinary("test.bin");
            list2.LoadBinary("test.bin", ref list2);
            Console.WriteLine("\nSaved List:");
            Console.WriteLine(list1.ToLongString("N1"));
            Console.WriteLine("Loaded List:");
            Console.WriteLine(list2.ToLongString("N1"));
        }
        static void TestFilesEmpty()
        {
            V2DataArray arr1 = new V2DataArray("Array №1 ", new DateTime(2021, 11, 15));
            V2DataArray arr2 = new V2DataArray("Array №2", new DateTime(2021, 11, 16));
            arr1.SaveAsText("test.txt");
            arr2.LoadAsText("test.txt", ref arr2);
            Console.WriteLine("\nSaved Array:");
            Console.WriteLine(arr1.ToString());
            Console.WriteLine("\nLoaded Array:");
            Console.WriteLine(arr2.ToString());
            Console.WriteLine();
            V2DataList list1 = new V2DataList("List №1 ", new DateTime(2021, 11, 7));
            V2DataList list2 = new V2DataList("List №2 ", new DateTime(2021, 11, 8));
            list1.SaveBinary("test.bin");
            list2.LoadBinary("test.bin", ref list2);
            Console.WriteLine("\nSaved List:");
            Console.WriteLine(list1.ToLongString("N1"));
            Console.WriteLine("Loaded List:");
            Console.WriteLine(list2.ToLongString("N1"));
        }
        static void TestLinq1()
        {
            V2MainCollection collection = new V2MainCollection();
            V2DataArray arr1 = new V2DataArray("Array №1 ", new DateTime(2021, 11, 8), 2, 2, new Vector2(1.5f, 2.5f), F1);
            V2DataList list1 = new V2DataList("List №1 ", new DateTime(2021, 11, 8));
            list1.Add(new DataItem(new Vector2(3, 4), new Complex(2, 5)));
            list1.Add(new DataItem(new Vector2(1, 4), new Complex(4, 2.5)));
            V2DataArray arr2 = new V2DataArray("Array №2 ", new DateTime(2021, 11, 8));
            V2DataList list2 = new V2DataList("List №2 ", new DateTime(2021, 11, 8));
            V2DataList list3 = new V2DataList("List #3 ", new DateTime(2021, 11, 8));
            list3.Add(new DataItem(new Vector2(2, 3), new Complex(1, 5)));
            list3.Add(new DataItem(new Vector2(0, 2.5f), new Complex(4, 1)));
            collection.Add(arr1);
            collection.Add(list1);
            collection.Add(arr2);
            collection.Add(list2);
            collection.Add(list3);
            Console.WriteLine(collection.ToLongString("N1"));

            Console.WriteLine("\nDataItem with Max Abs value:");
            Console.WriteLine(collection.MaxItemX);

            Console.WriteLine("\nDataList Except DataArray:");
            var data = collection.ListWithoutArray;
            foreach (Vector2 vec in data)
            {
                Console.WriteLine(vec);
            }

            Console.WriteLine("\nGroup:");
            var group = collection.Group;
            foreach (IGrouping<int, V2Data> g in group)
            {
                Console.WriteLine($"Count = {g.Key}");
                foreach (var d in g)
                    Console.WriteLine(d);
                Console.WriteLine();
            }
        }
        
        static void TestLinqEmpty()
        {
            V2MainCollection collection = new V2MainCollection();
            V2DataArray arr1 = new V2DataArray("Array №1 ", new DateTime(2021, 11, 8));
            V2DataList list1 = new V2DataList("List №1 ", new DateTime(2021, 11, 8));
            V2DataList list2 = new V2DataList("List №2 ", new DateTime(2021, 11, 8));
            collection.Add(arr1);
            collection.Add(list1);
            collection.Add(list2);
            Console.WriteLine(collection.ToLongString("N1"));

            Console.WriteLine("\nDataItem With Max Abs value:");
            Console.WriteLine(collection.MaxItemX);

            Console.WriteLine("\nDataList Except DataArray:");
            var data = collection.ListWithoutArray;
            foreach (Vector2 vec in data)
            {
                Console.WriteLine(vec);
            }

            Console.WriteLine("\nGroup:");
            var group = collection.Group;
            foreach (IGrouping<int, V2Data> g in group)
            {
                Console.WriteLine($"Count = {g.Key}");
                foreach (var d in g)
                    Console.WriteLine(d);
                Console.WriteLine();
            }
        }
        static void TestLinqEmptyCollection()
        {
            V2MainCollection collection = new V2MainCollection();
            Console.WriteLine(collection.ToLongString("N1"));

            Console.WriteLine("\nDataItem With Max Abs value:");
            Console.WriteLine(collection.MaxItemX);

            Console.WriteLine("\nDataList Except DataArray:");
            var data = collection.ListWithoutArray;
            foreach (Vector2 vec in data)
            {
                Console.WriteLine(vec);
            }

            Console.WriteLine("\nGroup:");
            var group = collection.Group;
            foreach (IGrouping<int, V2Data> g in group)
            {
                Console.WriteLine($"Count = {g.Key}");
                foreach (var d in g)
                    Console.WriteLine(d);
                Console.WriteLine();
            }
        }
        static void Main()
        {
            ////1. V2DataArray
            //Console.WriteLine("\n" + "1. V2DataArray" + "\n");
            //V2DataArray arr = new V2DataArray("Object", new DateTime(2021, 01, 01), 3, 2, new Vector2(1.5f, 1.5f), F1);
            //Console.WriteLine(arr.ToLongString("N1"));
            //V2DataList list = arr;
            //Console.WriteLine(list.ToLongString("N1"));
            //Console.WriteLine($"Array count: {arr.Count}, Array MinDistance: {arr.MinDistance}");
            //Console.WriteLine($"List Count: {list.Count}, List MinDistance: {list.MinDistance}");

            ////2. V2MainCollection
            //Console.WriteLine("\n" + "2. V2MainCollection" + "\n");
            //V2MainCollection collection = new V2MainCollection();
            //V2DataArray arr2 = new V2DataArray("Object2_2", new DateTime(2021, 01, 01), 2, 1, new Vector2(0.5f, 1.0f), F1);
            //V2DataList list1 = new V2DataList("List_1", new DateTime(2021, 01, 01));
            //collection.Add(arr);
            //collection.Add(arr2);
            //collection.Add(list1);
            //collection.Add(list);
            //Console.WriteLine(collection.ToLongString("N1"));
            //Console.WriteLine($"Collection count: {collection.Count}");

            ////3. Count и MinDistance 
            //Console.WriteLine("\n" + "3. Count и MinDistance" + "\n");
            //for (int i = 0; i < collection.Count; ++i)
            //{
            //    Console.WriteLine($"Count: {collection[i].Count}, MinDistance: {collection[i].MinDistance}");
            //}

            ////4. Testing Iterating
            ////Console.WriteLine("\n" + "4. Testing Iterating" + "\n");
            ////foreach (var i in arr)
            ////{
            ////    Console.WriteLine(i);
            ////}

            ////5. Max Item
            //Console.WriteLine("\n" + "5. Test LINQ Max" + "\n");
            ////Console.WriteLine(collection.MaxItemХ);

            Console.WriteLine("           Testing Files           ");
            TestFiles();
            Console.WriteLine("           Testing empty files           ");
            TestFilesEmpty();
            Console.WriteLine("           Testing LINQ        ");
            TestLinq1();
            Console.WriteLine("           Testing empty LINQ           ");
            TestLinqEmpty();
            Console.WriteLine("           Testing empty collection           ");
            TestLinqEmptyCollection();
        }
    }
}
