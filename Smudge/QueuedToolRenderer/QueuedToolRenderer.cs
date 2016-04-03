using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace pyrochild.effects.common
{
    public abstract class QueuedToolRenderer : IDisposable
    {
        Queue<QueuedToolEventArgs> eventQueue;

        WeakReference wr;

        Thread renderThread;

        protected Surface Surface { get { return wr.Target as Surface; } }

        object invalidrectlock = new object();
        private Rectangle totalinvalidrect = Rectangle.Empty;
        public Rectangle PopTotalInvalidRect()
        {
            lock (invalidrectlock)
            {
                Rectangle retval = totalinvalidrect;
                totalinvalidrect = Rectangle.Empty;
                return retval;
            }
        }

        private void PushTotalInvalidRect(Rectangle newrect)
        {
            lock (invalidrectlock)
            {
                totalinvalidrect = Rectangle.Union(totalinvalidrect, newrect);
            }
        }

        public QueuedToolRenderer(Surface surface)
        {
            wr = new WeakReference(surface);

            eventQueue = new Queue<QueuedToolEventArgs>();
        }

        public void AddEvent(QueuedToolEventArgs args)
        {
            lock (eventQueue)
            {
                eventQueue.Enqueue(args);
            }
            OnEventQueued();
        }

        public void AddEvents(IEnumerable<QueuedToolEventArgs> args)
        {
            lock (eventQueue)
            {
                foreach (QueuedToolEventArgs arg in args)
                    eventQueue.Enqueue(arg);
            }
            OnEventQueued();
        }

        private void OnEventQueued()
        {
            if (renderThread == null || !renderThread.IsAlive)
            {
                renderThread = new Thread(new ThreadStart(Render));
                renderThread.Start();
            }
        }

        bool aborted = false;
        public void Abort()
        {
            aborted = true;
            lock (eventQueue)
            {
                if (eventQueue.Count > 0)
                {
                    eventQueue.Clear();
                    eventQueue.Enqueue(new QueuedToolAbortEventArgs());
                }
            }
            OnEventQueued();
        }

        public bool IsAborted { get { return aborted; } }

        public int GetQueueSize()
        {
            lock (eventQueue)
            {
                return eventQueue.Count;
            }
        }

        private void Render()
        {
            bool didsomething = false;
            while (GetQueueSize() > 0)
            {
                didsomething = true;
                QueuedToolEventArgs args;
                lock (eventQueue)
                {
                    args = eventQueue.Dequeue();
                }
                if (args != null)
                {
                    switch (args.EventType)
                    {
                        case QueuedToolEventType.MouseDown:
                            QueuedMouseDown(args);
                            break;
                        case QueuedToolEventType.MouseMove:
                            QueuedMouseMove(args);
                            break;
                        case QueuedToolEventType.MouseUp:
                            QueuedMouseUp(args);
                            break;
                        case QueuedToolEventType.Abort:
                            QueuedAbort();
                            break;
                        case QueuedToolEventType.MouseHold:
                            QueuedMouseHold(args);
                            break;
                        case QueuedToolEventType.Custom:
                            QueuedCustomEvent(args);
                            break;
                        default:
                            throw new ArgumentException("invalid event in queue");
                    }
                }
            }
            if (didsomething)
            {
                OnQueueEmptied();
                didsomething = false;
            }
        }

        public event EventHandler CustomEvent;
        private void QueuedCustomEvent(QueuedToolEventArgs args)
        {
            OnCustomEvent(args);
            if (CustomEvent != null)
            {
                CustomEvent(this, args);
            }
        }

        public event QueuedToolEventHandler MouseHold;
        private void QueuedMouseHold(QueuedToolEventArgs args)
        {
            OnMouseHold(args);
            if (MouseHold != null)
            {
                MouseHold(this, args);
            }
        }

        public event QueuedToolEventHandler MouseUp;
        private void QueuedMouseUp(QueuedToolEventArgs args)
        {
            OnMouseUp(args);
            if (MouseUp != null)
            {
                MouseUp(this, args);
            }
        }

        public event QueuedToolEventHandler MouseMove;
        private void QueuedMouseMove(QueuedToolEventArgs args)
        {
            OnMouseMove(args);
            if (MouseMove != null)
            {
                MouseMove(this, args);
            }
        }

        public event QueuedToolEventHandler MouseDown;
        private void QueuedMouseDown(QueuedToolEventArgs args)
        {
            OnMouseDown(args);
            if (MouseDown != null)
            {
                MouseDown(this, args);
            }
        }

        protected virtual void OnCustomEvent(QueuedToolEventArgs args)
        {
        }

        protected virtual void OnMouseHold(QueuedToolEventArgs args)
        {
        }

        protected virtual void OnMouseUp(QueuedToolEventArgs args)
        {
        }

        protected virtual void OnMouseMove(QueuedToolEventArgs args)
        {
        }

        protected virtual void OnMouseDown(QueuedToolEventArgs args)
        {
        }

        private void QueuedAbort()
        {
            lock (eventQueue)
            {
                eventQueue.Clear();
            }
            OnAborted();
            aborted = false;
        }

        public event EventHandler Aborted;
        private void OnAborted()
        {
            if (Aborted != null)
            {
                Aborted(this, EventArgs.Empty);
                aborted = false;
            }
        }

        public event InvalidateEventHandler Invalidated;
        protected void OnInvalidated(Rectangle invalidRect)
        {
            invalidRect.Intersect(Surface.Bounds);
            PushTotalInvalidRect(invalidRect);
            if (Invalidated != null)
            {
                Invalidated(this, new InvalidateEventArgs(invalidRect));
            }
        }

        public event EventHandler QueueEmptied;
        private void OnQueueEmptied()
        {
            if (QueueEmptied != null)
                QueueEmptied(this, EventArgs.Empty);
        }

        private bool disposed = false;
        public bool Disposed { get { return disposed; } }

        public void Dispose()
        {
            Abort();
            disposed = true;
            OnDispose();
        }

        protected virtual void OnDispose()
        {
        }
    }
}