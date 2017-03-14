using System;
using System.Collections.Concurrent;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Data.Utility
{
    public class MultitenantMethodLock
    {
        private readonly IPushLogger _logger;

        private readonly
            ConcurrentDictionary<string, string>
            _processing = new ConcurrentDictionary<string, string>();

        private readonly object _lock = new object();

        private readonly string _methodName;

        public MultitenantMethodLock(string methodName)
        {
            _methodName = methodName;
        }

        private string BuildKey(string userId)
        {
            return _methodName + ":" + userId;
        }

        public MethodLockResult AttemptToLockMethod(string userId)
        {
            var key = BuildKey(userId);

            lock (_lock)
            {
                try
                {
                    if (_processing.ContainsKey(key))
                    {
                        return new MethodLockResult
                        {
                            Success = false,
                            Message = $"Method {key} is already locked; please try again later",
                        };
                    }
                    else
                    {
                        _processing[key] = key;
                        return new MethodLockResult {Success = true, Message = ""};
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    return new MethodLockResult
                    {
                        Success = false,
                        Message = e.Message,
                        Exception = e,
                    };
                }
            }
        }

        public void FreeProcessLock(string userId)
        {
            lock (_lock)
            {
                try
                {
                    var key = BuildKey(userId);
                    string keyOut;
                    _processing.TryRemove(key, out keyOut);

                }
                catch (Exception e)
                {
                    _logger.Error(e);
                }
            }
        }
    }
}

