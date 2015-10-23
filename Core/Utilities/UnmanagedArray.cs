﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System
{

    /// <summary>
    /// 元素类型为sbyte, byte, char, short, ushort, int, uint, long, ulong, float, double, decimal, bool或其它struct的非托管数组。
    /// <para>不能使用enum类型作为T。</para>
    /// <para>Check http://www.cnblogs.com/bitzhuwei/p/huge-unmanged-array-in-csharp.html </para>
    /// </summary>
    /// <typeparam name="T">sbyte, byte, char, short, ushort, int, uint, long, ulong, float, double, decimal, bool或其它struct, 不能使用enum类型作为T。</typeparam>
    public sealed class UnmanagedArray<T> : UnmanagedArrayBase where T : struct
    {

        /// <summary>
        ///元素类型为sbyte, byte, char, short, ushort, int, uint, long, ulong, float, double, decimal, bool或其它struct的非托管数组。
        /// </summary>
        /// <param name="count"></param>
        //[MethodImpl(MethodImplOptions.Synchronized)]
        public UnmanagedArray(int count)
            : base(count, Marshal.SizeOf(typeof(T)))
        {
        }

        /// <summary>
        /// 获取或设置索引为<paramref name="index"/>的元素。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= this.Length)
                    throw new IndexOutOfRangeException("index of UnmanagedArray is out of range");

                var pItem = this.Header + (index * elementSize);
                var obj = Marshal.PtrToStructure(pItem, typeof(T));
                T result = (T)obj;
                //T result = Marshal.PtrToStructure<T>(pItem);// works in .net 4.5.1
                return result;
            }
            set
            {
                if (index < 0 || index >= this.Length)
                    throw new IndexOutOfRangeException("index of UnmanagedArray is out of range");

                var pItem = this.Header + (index * elementSize);
                Marshal.StructureToPtr(value, pItem, true);
                //Marshal.StructureToPtr<T>(value, pItem, true);// works in .net 4.5.1
            }
        }

        // 速度较慢，不再提供。
        ///// <summary>
        ///// 按索引顺序依次获取各个元素。
        ///// </summary>
        ///// <returns></returns>
        //public IEnumerable<T> Elements()
        //{
        //    for (int i = 0; i < this.Length; i++)
        //    {
        //        yield return this[i];
        //    }
        //}
    }

    /// <summary>
    /// 非托管数组的基类。
    /// </summary>
    public abstract class UnmanagedArrayBase : IDisposable
    {

        /// <summary>
        /// 此数组的起始位置。
        /// </summary>
        public IntPtr Header { get; private set; }

        /// <summary>
        /// 元素的总数。
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// 单个元素的字节数。
        /// </summary>
        protected int elementSize;

        /// <summary>
        /// 申请到的字节数。（元素的总数 * 单个元素的字节数）。
        /// </summary>
        public int ByteLength
        {
            get { return (this.Length * this.elementSize); }
        }

        /// <summary>
        /// 非托管数组。
        /// </summary>
        /// <param name="elementCount">元素的总数。</param>
        /// <param name="elementSize">单个元素的字节数。</param>
        //[MethodImpl(MethodImplOptions.Synchronized)]//这造成死锁，不知道是为什么
        protected UnmanagedArrayBase(int elementCount, int elementSize)
        {
            this.Length = elementCount;
            this.elementSize = elementSize;

            int memSize = elementCount * elementSize;
            this.Header = Marshal.AllocHGlobal(memSize);

            lock (synObj)
            {
                //allocatedArrays.Add(this);
                //allocatedArrays.Add(new WeakReference<IDisposable>(this));
                allocatedArrays.Add(this.Header, new WeakReference<IDisposable>(this));
            }
            //UnmanagedArrayBase.Add(this);
        }

        private static readonly object synObj = new object();
        //private static readonly List<IDisposable> allocatedArrays = new List<IDisposable>();
        //private static readonly List<WeakReference<IDisposable>> allocatedArrays = new List<WeakReference<IDisposable>>();
        private static readonly Dictionary<IntPtr, WeakReference<IDisposable>> allocatedArrays = new Dictionary<IntPtr, WeakReference<IDisposable>>();


        ////[MethodImpl(MethodImplOptions.Synchronized)]//这造成死锁，不知道是为什么
        //private static void Add(UnmanagedArrayBase array)
        //{
        //    //allocatedArrays.Add(array);
        //    allocatedArrays.Add(new WeakReference<IDisposable>(array));
        //}
        /// <summary>
        /// 立即释放所有<see cref="UnmanagedArray"/>。
        /// </summary>
        //[MethodImpl(MethodImplOptions.Synchronized)]//这造成死锁，不知道是为什么
        public static void FreeAll()
        {
            lock (synObj)
            {
                var list = new List<WeakReference<IDisposable>>(allocatedArrays.Values.AsEnumerable());
                foreach (var item in list)
                {
                    //item.Dispose();
                    IDisposable target;
                    if (item.TryGetTarget(out target))
                    {
                        target.Dispose();
                    }
                }

                allocatedArrays.Clear();
            }
        }


        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        } // end sub

        /// <summary>
        /// Destruct instance of the class.
        /// </summary>
        ~UnmanagedArrayBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Backing field to track whether Dispose has been called.
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Dispose managed and unmanaged resources of this instance.
        /// </summary>
        /// <param name="disposing">If disposing equals true, managed and unmanaged resources can be disposed. If disposing equals false, only unmanaged resources can be disposed. </param>
        protected virtual void Dispose(bool disposing)
        {

            if (this.disposedValue == false)
            {
                if (disposing)
                {
                    // TODO: Dispose managed resources.
                } // end if

                // TODO: Dispose unmanaged resources.
                IntPtr ptr = this.Header;

                if (ptr != IntPtr.Zero)
                {
                    this.Length = 0;
                    this.Header = IntPtr.Zero;
                    try
                    {
                        Marshal.FreeHGlobal(ptr);

                        lock (synObj)
                        {
                            allocatedArrays.Remove(ptr);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            } // end if

            this.disposedValue = true;
        } // end sub

        #endregion

    }
}
