﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace HangFire
{
    public class JobDescription
    {
        public static JobDescription Deserialize(string serializedJob)
        {
            return JsonHelper.Deserialize<JobDescription>(serializedJob);
        }

        public JobDescription(Type workerType, object args)
        {
            Jid = Guid.NewGuid().ToString();
            JobType = workerType.AssemblyQualifiedName;
            Args = new Dictionary<string, string>();
            Properties = new Dictionary<string, string>();
            EnqueuedAt = DateTime.UtcNow;

            AddValues(Args, args);
        }

        public string Jid { get; internal set; }
        public string JobType { get; internal set; }
        public IDictionary<string, string> Args { get; internal set; }
        public IDictionary<string, string> Properties { get; internal set; } 
        public DateTime EnqueuedAt { get; internal set; }
        public DateTime? SucceededAt { get; internal set; }
        public DateTime? FailedAt { get; internal set; }

        [IgnoreDataMember]
        public TimeSpan Latency
        {
            get
            {
                if (SucceededAt.HasValue)
                {
                    return SucceededAt.Value.Subtract(EnqueuedAt);
                }
                if (FailedAt.HasValue)
                {
                    return FailedAt.Value.Subtract(EnqueuedAt);
                }

                return DateTime.UtcNow.Subtract(EnqueuedAt);
            }
        }

        [IgnoreDataMember]
        public bool Canceled { get; private set; }

        public void Cancel()
        {
            Canceled = true;
        }

        public string Serialize()
        {
            return JsonHelper.Serialize(this);
        }

        public string SerializeArgs()
        {
            return JsonHelper.Serialize(Args);
        }

        private static void AddValues(IDictionary<string, string> dictionary, object values)
        {
            if (values != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
                {
                    var obj2 = descriptor.GetValue(values);
                    string value = null;

                    if (obj2 != null)
                    {
                        // TODO: handle conversion exception and display it in a friendly way.
                        var converter = TypeDescriptor.GetConverter(obj2.GetType());
                        value = converter.ConvertToInvariantString(obj2);
                    }

                    dictionary.Add(descriptor.Name.ToLowerInvariant(), value);
                }
            }
        }
    }
}