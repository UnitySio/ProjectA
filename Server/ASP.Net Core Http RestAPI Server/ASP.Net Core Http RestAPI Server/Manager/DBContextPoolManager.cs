using System;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace ASP.Net_Core_Http_RestAPI_Server
{
    public class DBContextPoolManager<T> where T : DbContext, new()
    {
        //where T : DbContext, new() => 제네릭 T는 DbContext를 상속받아야 하고 new생성이 가능.

        //Pool용 DBContext객체를 담아둘 ConcurrentQueue.
        private ConcurrentQueue<T> ctxQueue;

        public DBContextPoolManager()
        {
            ctxQueue = new ConcurrentQueue<T>();
        }

        ~DBContextPoolManager()
        {
            //ClearQueue.
            while (!ctxQueue.IsEmpty)
            {
                T result = null;

                if (ctxQueue.TryDequeue(out result))
                {
                    result = null;
                }
            }

            ctxQueue = null;
        }

        //Pool에서 사용가능한 DBContext를 가져옴.
        public T Rent()
        {
            if (ctxQueue == null)
                throw new NullReferenceException("DBContextPoolManager::Rent() ctxQueue is Null.");

            T result = null;
            if (ctxQueue.TryDequeue(out result))
            {
                result.ChangeTracker.Clear();
                return result;
            }
            else
            {
                return new T();
            }
        }

        //사용을 마친 DBContext를 Pool로 돌려줌.
        //단, DBContext.Dispose(); 함수의 호출은 하지 않은 객체여야 한다. (재사용)
        public void Return(T ctx)
        {
            if (ctxQueue == null)
                throw new NullReferenceException("DBContextPoolManager::Return() ctxQueue is Null.");

            if (ctx == null)
                throw new NullReferenceException("DBContextPoolManager::Return() DBContext is Null.");

            ctxQueue.Enqueue(ctx);
        }
    }
}