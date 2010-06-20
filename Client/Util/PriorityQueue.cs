namespace Util
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	
	public interface IPriorityQueue<T> 
	{ 
        int Push(T item); 
        T Pop(); 
        T Peek(); 
        void Update(int i); 
    } 

    public class PriorityQueueB<T> : IPriorityQueue<T> 
	{ 
        protected List<T> InnerList = new List<T>(); 
        protected Comparison<T> mComparer; 
     
        public PriorityQueueB(Comparison<T> comparer) 
		{ 
            mComparer = comparer; 
        } 

        public PriorityQueueB(Comparison<T> comparer, int capacity) { 
            mComparer = comparer; 
            InnerList.Capacity = capacity; 
        } 
        protected void SwitchElements(int i, int j) { 
            T h = InnerList[i]; 
            InnerList[i] = InnerList[j]; 
            InnerList[j] = h; 
        } 
        protected virtual int OnCompare(int i, int j) { 
            return mComparer(InnerList[i], InnerList[j]); 
        } 
        public int Push(T item) { 
            int p = InnerList.Count, p2; 
            InnerList.Add(item); 
            do { 
                if (p == 0) 
                    break; 
                p2 = (p - 1) / 2; 
                if (OnCompare(p, p2) < 0) { 
                    SwitchElements(p, p2); 
                    p = p2; 
                } 
                else 
                    break; 
            } while (true); 
            return p; 
        } 
        public T Pop() { 
            T result = InnerList[0]; 
            int p = 0, p1, p2, pn; 
            InnerList[0] = InnerList[InnerList.Count - 1]; 
            InnerList.RemoveAt(InnerList.Count - 1); 
            do { 
                pn = p; 
                p1 = 2 * p + 1; 
                p2 = 2 * p + 2; 
                if (InnerList.Count > p1 && OnCompare(p, p1) > 0) 
                    p = p1; 
                if (InnerList.Count > p2 && OnCompare(p, p2) > 0) 
                    p = p2; 

                if (p == pn) 
                    break; 
                SwitchElements(p, pn); 
            } while (true); 

            return result; 
        } 
        public void Update(int i) { 
            int p = i, pn; 
            int p1, p2; 
            do { 
                if (p == 0) 
                    break; 
                p2 = (p - 1) / 2; 
                if (OnCompare(p, p2) < 0) { 
                    SwitchElements(p, p2); 
                    p = p2; 
                } 
                else 
                    break; 
            } while (true); 
            if (p < i) 
                return; 
            do { 
                pn = p; 
                p1 = 2 * p + 1; 
                p2 = 2 * p + 2; 
                if (InnerList.Count > p1 && OnCompare(p, p1) > 0) 
                    p = p1; 
                if (InnerList.Count > p2 && OnCompare(p, p2) > 0) 
                    p = p2; 

                if (p == pn) 
                    break; 
                SwitchElements(p, pn); 
            } while (true); 
        } 
        public T Peek() { 
            if (InnerList.Count > 0) 
                return InnerList[0]; 
            return default(T); 
        } 
        public void Clear() { 
            InnerList.Clear(); 
        } 
        public int Count { 
            get { return InnerList.Count; } 
        } 
        public void RemoveLocation(T item) { 
            int index = -1; 
            for (int i = 0; i < InnerList.Count; i++) { 
                if (mComparer(InnerList[i], item) == 0) 
                    index = i; 
            } 
            if (index != -1) 
                InnerList.RemoveAt(index); 
        } 
        public T this[int index] { 
            get { return InnerList[index]; } 
            set { 
                InnerList[index] = value; 
                Update(index); 
            } 
        } 
    } 
}
