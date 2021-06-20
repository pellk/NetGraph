using System.Collections.Generic;
using System.Collections.Specialized;

namespace NetGraph
{
    public class ObservableQueue<T> : Queue<T>, INotifyCollectionChanged
    {
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, e);
        }

        public ObservableQueue(int capacity) : base(capacity)
        {
        }

        public new void Clear()
        {
            base.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, base.Count - 1));
        }

        public new void Dequeue()
        {
            var item = base.Dequeue();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, 0));
        }
    }
}