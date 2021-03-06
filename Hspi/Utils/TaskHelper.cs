﻿using System;
using System.Threading;
using System.Threading.Tasks;
using static System.FormattableString;

#nullable enable

namespace Hspi.Utils
{
    internal static class TaskHelper
    {
        private readonly static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static T ResultForSync<T>(this Task<T> @this)
        {
            // https://blogs.msdn.microsoft.com/pfxteam/2012/04/13/should-i-expose-synchronous-wrappers-for-asynchronous-methods/
            return Task.Run(() => @this).Result;
        }

        public static void ResultForSync(this Task @this)
        {
            Task.Run(() => @this).Wait();
        }

        public static void StartAsyncWithErrorChecking(string taskName,
                                                       Func<Task> taskAction,
                                                       CancellationToken token,
                                                       TimeSpan? delayAfterError = null)
        {
            _ = Task.Factory.StartNew(() => RunInLoop(taskName, taskAction, delayAfterError, token), token,
                                          TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach,
                                          TaskScheduler.Current);
        }

        private static async Task RunInLoop(string taskName, Func<Task> taskAction, TimeSpan? delayAfterError, CancellationToken token)
        {
            bool loop = true;
            while (loop && !token.IsCancellationRequested)
            {
                try
                {
                    logger.Debug(Invariant($"{taskName} Starting"));
                    await taskAction().ConfigureAwait(false);
                    logger.Debug(Invariant($"{taskName} Finished"));
                    loop = false;  //finished sucessfully
                }
                catch (Exception ex)
                {
                    if (ex.IsCancelException())
                    {
                        throw;
                    }

                    if (delayAfterError.HasValue)
                    {
                        logger.Error(Invariant($"{taskName} failed with {ex.GetFullMessage()}. Restarting after {delayAfterError.Value.TotalSeconds}s ..."));
                        await Task.Delay(delayAfterError.Value, token).ConfigureAwait(false);
                    }
                    else
                    {
                        logger.Error(Invariant($"{taskName} failed with {ex.GetFullMessage()}. Restarting ..."));
                    }
                }
            }
        }
    }
}