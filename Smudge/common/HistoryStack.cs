using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace pyrochild.effects.common
{
    public class HistoryStack : IDisposable
    {
        List<HistoryItem> stack;
        int step;
        bool errornotified;

        public HistoryStack(Surface surface, bool localcopy)
        {
            stack = new List<HistoryItem>();
            step = -1;
            if (localcopy)
            {
                AddHistoryItem(surface, surface.Bounds);
            }
            else
            {
                AddHistoryItem(surface);
            }
        }

        public void AddHistoryItem(Surface surface)
        {
            try
            {
                if (step < stack.Count - 1)
                {
                    stack.RemoveRange(step + 1, stack.Count - 1 - step);
                }
                stack.Add(new HistoryItem(surface));
                step++;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public void AddHistoryItem(Surface surface, Rectangle bounds)
        {
            try
            {
                if (step < stack.Count - 1)
                {
                    stack.RemoveRange(step + 1, stack.Count - 1 - step);
                }
                stack.Add(new HistoryItem(surface, bounds));
                step++;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void OnError(Exception ex)
        {
            if (!errornotified)
            {
                string errormessage = "There was an error creating the History entry. Further action on this image may or may not result in a corrupted History stack. Restarting this plugin is recommended. Undo and Redo functions may no longer function as expected.\r\n\r\nException Details:\r\n";
                errormessage += ex.ToString();
                MessageBox.Show(errormessage, "History Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                errornotified = true;
            }
        }

        public bool CanStepBack
        {
            get { return step > 0; }
        }

        public bool CanStepForward
        {
            get { return step < stack.Count - 1; }
        }

        public void StepBack(Surface surface)
        {
            if (CanStepBack)
            {
                step--;
                for (int i = 0; i <= step; i++)
                {
                    stack[i].DeltaSurface.ToMemory();
                    surface.CopySurface(stack[i].DeltaSurface.Surface, stack[i].DeltaRect.Location, stack[i].DeltaSurface.Bounds);
                    stack[i].DeltaSurface.ToDisk();
                }
            }
        }

        public void StepForward(Surface surface)
        {
            if (CanStepForward)
            {
                step++;
                for (int i = 0; i <= step; i++)
                {
                    stack[i].DeltaSurface.ToMemory();
                    surface.CopySurface(stack[i].DeltaSurface.Surface, stack[i].DeltaRect.Location, stack[i].DeltaSurface.Bounds);
                    stack[i].DeltaSurface.ToDisk();
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (HistoryItem hi in stack)
            {
                hi.DeltaSurface.Dispose();
            }
            stack.Clear();
        }

        #endregion
    }
}