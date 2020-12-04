using IPA.Utilities.Async;
using System;
using System.Threading;

namespace BeatSaberPlus.Utils
{
    /*
       Code from https://github.com/brian91292/EnhancedStreamChat-v3

       MIT License

       Copyright (c) 2020 brian91292

       Permission is hereby granted, free of charge, to any person obtaining a copy
       of this software and associated documentation files (the "Software"), to deal
       in the Software without restriction, including without limitation the rights
       to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
       copies of the Software, and to permit persons to whom the Software is
       furnished to do so, subject to the following conditions:

       The above copyright notice and this permission notice shall be included in all
       copies or substantial portions of the Software.

       THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
       IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
       FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
       AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
       LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
       OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
       SOFTWARE.
    */
    public class MainThreadInvoker
    {
        private static CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        internal static void ClearQueue()
        {
            _cancellationToken.Cancel();
            _cancellationToken = new CancellationTokenSource();
        }

        public static void Invoke(Action action)
        {
            if (action != null)
            {
                UnityMainThreadTaskScheduler.Factory.StartNew(action, _cancellationToken.Token);
            }
        }

        public static void Invoke<A>(Action<A> action, A a)
        {
            if (action != null)
            {
                UnityMainThreadTaskScheduler.Factory.StartNew(() => action?.Invoke(a), _cancellationToken.Token);
            }
        }

        public static void Invoke<A, B>(Action<A, B> action, A a, B b)
        {
            if (action != null)
            {
                UnityMainThreadTaskScheduler.Factory.StartNew(() => action?.Invoke(a, b), _cancellationToken.Token);
            }
        }
    }
}
