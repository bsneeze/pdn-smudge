using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PaintDotNet;

namespace pyrochild.effects.common
{
    public class PngBrushCollection : IEnumerable<PngBrush>, IList<PngBrush>, ICollection<PngBrush>, IDisposable
    {
        private List<PngBrush> brushes;
        private static string brushpath;

        public PngBrushCollection(IServiceProvider serviceprovider, string ownername)
        {
            brushpath = Path.Combine(serviceprovider.GetService<PaintDotNet.AppModel.IUserFilesService>().UserFilesPath, ownername + " Brushes");
            brushes = new List<PngBrush>();

            if (Directory.Exists(BrushesPath))
            {
                string[] filenames = Directory.GetFiles(BrushesPath, "*.png", SearchOption.TopDirectoryOnly);
                foreach (string s in filenames)
                {
                    string filename = Path.GetFileNameWithoutExtension(s);

                    PngBrush brush = new PngBrush(filename);
                    if (!brushes.Contains(brush))
                    {
                        brushes.Add(new PngBrush(filename));
                    }
                }
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(BrushesPath);
                }
                catch { }
            }
        }

        public static string BrushesPath
        {
            get
            {
                return brushpath;
            }
        }

        #region IEnumerable<PngBrush> Members

        public IEnumerator<PngBrush> GetEnumerator()
        {
            return brushes.GetEnumerator();
        }

        #endregion

        #region IList<PngBrush> Members

        public int IndexOf(PngBrush item)
        {
            return brushes.IndexOf(item);
        }

        public void Insert(int index, PngBrush item)
        {
            brushes.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            brushes.RemoveAt(index);
        }

        public PngBrush this[int index]
        {
            get
            {
                return brushes[index];
            }
            set
            {
                brushes[index] = value;
            }
        }

        #endregion

        #region ICollection<PngBrush> Members

        public void Add(PngBrush item)
        {
            brushes.Add(item);
        }

        public void Clear()
        {
            brushes.Clear();
        }

        public bool Contains(PngBrush item)
        {
            return brushes.Contains(item);
        }

        public void CopyTo(PngBrush[] array, int arrayIndex)
        {
            brushes.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return brushes.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(PngBrush item)
        {
            return brushes.Remove(item);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return brushes.GetEnumerator();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            foreach (PngBrush pb in brushes)
            {
                pb.Thumbnail.Dispose();
            }
        }

        #endregion
    }
}