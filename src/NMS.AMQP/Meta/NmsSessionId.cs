﻿/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Apache.NMS.AMQP.Meta
{
    public class NmsSessionId : INmsResourceId
    {
        private string key;

        public NmsSessionId(NmsConnectionId connectionId, long value)
        {
            this.ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId), "Connection ID cannot be null");
            this.Value = value;
        }

        public NmsConnectionId ConnectionId { get; }
        public long Value { get; }

        protected bool Equals(NmsSessionId other)
        {
            return Equals(ConnectionId, other.ConnectionId) && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NmsSessionId) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ConnectionId != null ? ConnectionId.GetHashCode() : 0) * 397) ^ Value.GetHashCode();
            }
        }

        public override string ToString()
        {
            return key ?? (key = $"{ConnectionId}:{Value.ToString()}");
        }
    }
}