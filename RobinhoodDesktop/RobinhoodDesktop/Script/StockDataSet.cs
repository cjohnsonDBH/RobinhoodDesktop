﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RobinhoodDesktop.Script
{
    public class StockDataSet<T> where T : struct, StockData
    {
        public StockDataSet(string symbol, DateTime start, StockDataFile file, long address = -1)
        {
            this.Symbol = symbol;
            this.Start = start;
            this.File = file;
            this.StreamAddress = address;
        }

        protected StockDataSet()
        {

        }

        #region Types
        public class StockDataArray
        {
            T[] m_array;
            int m_count;

            public StockDataArray()
            {
                m_count = 0;
            }

            public T[] InternalArray { get { return m_array; } }

            public int Count { get { return m_count; } }

            public void Initialize(T[] data)
            {
                m_array = data;
                m_count = data.Length;
            }

            public void Resize(int capacity)
            {
                if(m_array == null)
                {
                    m_array = new T[capacity];
                }
                else if(m_array.Length != capacity)
                {
                    Array.Resize(ref m_array, capacity);
                }
            }

            public void Add(T element)
            {
                if(m_array == null)
                {
                    m_array = new T[1];
                    m_count = 0;
                }
                if(m_count == m_array.Length)
                {
                    Array.Resize(ref m_array, m_array.Length * 2);
                }

                m_array[m_count++] = element;
            }

            public void Clear()
            {
                m_array = null;
                m_count = 0;
            }
        }
        #endregion

        #region Variables
        public string Symbol;
        public DateTime Start;
        public StockDataFile File;
        public long StreamAddress;
        public DateTime End
        {
            get { return Start.AddTicks(Interval.Ticks * DataSet.Count); }
        }
        public virtual TimeSpan Interval
        {
            set {  }
            get { return File.Interval; }
        }

        /// <summary>
        /// The array holding the stock data points
        /// </summary>
        public readonly StockDataArray DataSet = new StockDataArray();

        /// <summary>
        /// Returns the last item in the data set
        /// </summary>
        public T Last
        {
            get { return DataSet.InternalArray[DataSet.InternalArray.Count() - 1]; }
        }

        /// <summary>
        /// The previous data set in the series (allows datasets to reference back across gaps in the time sequence)
        /// </summary>
        public StockDataSet<T> Previous;
        #endregion

        /// <summary>
        /// Allows bracket operator to be used on the data set
        /// </summary>
        /// <param name="i">The index to access</param>
        /// <returns>The specified item in the data set</returns>
        public T this[int i]
        {
            get {
                var set = this;
                while((i < 0) && (set.Previous != null))
                {
                    set = set.Previous;
                    i += set.Count;
                }
                return set.DataSet.InternalArray[(i >= 0) ? i : 0];
            }
        }

        /// <summary>
        /// Limits the specified index to the available data range
        /// </summary>
        /// <param name="index">The index to limit</param>
        /// <returns>The limited index</returns>
        public virtual int LimitIndex(int index)
        {
            int minIdx = 0;
            var prevSet = Previous;
            while((minIdx > index) && (prevSet != null))
            {
                minIdx -= prevSet.Count;
                prevSet = prevSet.Previous;
            }
            return (index >= minIdx) ? index : minIdx;
        }

        /// <summary>
        /// Returns the time corresponding to the specified data point
        /// </summary>
        /// <param name="i">The data point index</param>
        /// <returns>The time that data point was recorded</returns>
        public DateTime Time(int i)
        {
            return Start.AddTicks(Interval.Ticks * i);
        }

        /// <summary>
        /// The number of points in the data set
        /// </summary>
        public int Count
        {
            get { return DataSet.Count; }
        }

        /// <summary>
        /// Indicates if the data set has valid data
        /// </summary>
        /// <returns>True if data is available</returns>
        public virtual bool IsReady()
        {
            return (DataSet.Count > 0);
        }

        /// <summary>
        /// Loads the data from the source file
        /// <param name="session">The session currently being processed</param>
        /// </summary>
        public virtual void Load(StockSession session)
        {
            if(!IsReady())
            {
                File.LoadSegment(this, session);
            }
        }

        /// <summary>
        /// Releases the reference to the data to free up memory
        /// </summary>
        public virtual void Clear()
        {
            this.DataSet.Clear();
        }
    }
}
