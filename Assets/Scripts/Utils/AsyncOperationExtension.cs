using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class AsyncOperationExtension
    {
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { taskCompletionSource.SetResult(null); };
            return ((Task) taskCompletionSource.Task).GetAwaiter();
        }
    }
}