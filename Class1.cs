using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Numerics;

namespace Lab2
{
    class ListEnumerator : IEnumerator<DataItem>
    {
        List<DataItem> ItemList;
        int position = -1;
        public ListEnumerator(List<DataItem> ItemList)
        {
            this.ItemList = ItemList;
        }

        public DataItem Current
        {
            get
            {
                if (position == -1 || position >= ItemList.Count)
                    throw new InvalidOperationException();
                return ItemList[position];
            }
        }

        object IEnumerator.Current => throw new NotImplementedException();

        public bool MoveNext()
        {
            if (position < ItemList.Count - 1)
            {
                position++;
                return true;
            }
            else
                return false;
        }

        public void Reset()
        {
            position = -1;
        }
        public void Dispose() { }
    }

    class ArrayEnumerator : IEnumerator<DataItem>
    {
        List<DataItem> ArrItemList;
        Complex[,] values;
        int nx;
        int ny;
        Vector2 nxy;
        int position = -1;
        public ArrayEnumerator(Complex[,] values, int nx, int ny, Vector2 nxy)
        {
            this.values = values;
            this.nx = nx;
            this.ny = ny;
            this.nxy = nxy;
            this.ArrItemList = new List<DataItem>();


            for (int i = 0; i < nx; ++i)
            {
                for (int j = 0; j < ny; ++j)
                {
                    float x = i * nxy.X;
                    float y = j * nxy.Y;
                    Vector2 xy = new Vector2(x, y);
                    Complex field = values[i, j];
                    DataItem newItem = new DataItem(xy, field);
                    this.ArrItemList.Add(newItem);
                }
            }
        }
        public DataItem Current
        {
            get
            {
                if (position == -1 || position >= values.Length)
                    throw new InvalidOperationException();
                return ArrItemList[position];
            }
        }

        object IEnumerator.Current => throw new NotImplementedException();

        public bool MoveNext()
        {
            if (position < ArrItemList.Count - 1)
            {
                position++;
                return true;
            }
            else
                return false;
        }

        public void Reset()
        {
            position = -1;
        }
        public void Dispose() { }
    }
}