﻿using System;
using System.Collections.Generic;
using Agent.Core.Utils;

namespace Agent.Core.ServerOperations
{
    class OperationQueue
    {
        private readonly Queue<string> _queue;
        private bool _operationInProgress;

        public OperationQueue()
        {
            _queue = new Queue<string>();
            _operationInProgress = false;
        }

        public bool Put(string message)
        {
            try
            {
                _queue.Enqueue(message);
                return true;
            }
            catch (Exception e)
            {
                Logger.Log("Error adding operation to queue.", LogLevel.Error);
                Logger.Log("Message: {0}", LogLevel.Error, e.Message);
                return false;
            }
        }

        public string Get()
        {
            string message = null;

            if (!_operationInProgress)
            {
                try
                {
                    message = _queue.Dequeue();
                    Pause();
                }
                catch (InvalidOperationException)
                {
                    message = null;
                }
                catch (Exception e)
                {
                    Logger.Log("Error accessing operation queue.", LogLevel.Error);
                    Logger.Log("Message: {0}", LogLevel.Error, e.Message);
                    message = null;
                }
            }

            return message;
        }

        public void Pause()
        {
            _operationInProgress = true;
            Logger.Log("Queue paused.", LogLevel.Warning);
        }

        public void Done()
        {
            _operationInProgress = false;
            Logger.Log("Queue resuming.", LogLevel.Info);
        }
    }
}
