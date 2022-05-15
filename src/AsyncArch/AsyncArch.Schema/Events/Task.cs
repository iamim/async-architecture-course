using System.Text.Json.Serialization;

namespace AsyncArch.Schema.Events;

public static class Task
{
    public class Completed_V1
    {
        public static string Kind = "TaskCompleted";

        public string event_id { get; }
        public int event_version { get; }
        public string event_name { get; }
        public DateTimeOffset event_time { get; }
        public string producer { get; }
        public Data data { get; }

        [JsonConstructor]
        public Completed_V1(
            string event_id,
            int event_version,
            string event_name,
            DateTimeOffset event_time,
            string producer,
            Data data
        )
        {
            this.event_version = event_version != 1 ? throw new SchemaException(nameof(event_version)) : event_version;
            this.event_name = event_name != Kind ? throw new SchemaException(nameof(event_name)) : event_name;

            this.event_id = event_id ?? throw new ArgumentNullException(nameof(event_id));
            this.event_time = event_time;
            this.producer = producer ?? throw new ArgumentNullException(nameof(producer));
            this.data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public class Data
        {
            public Guid task_uuid { get; }

            [JsonConstructor]
            public Data(Guid task_uuid)
            {
                this.task_uuid = task_uuid;
            }
        }
    }

    public class Created_V1
    {
        public static string Kind = "TaskCreated";

        public string event_id { get; }
        public int event_version { get; }
        public string event_name { get; }
        public DateTimeOffset event_time { get; }
        public string producer { get; }
        public Data data { get; }

        [JsonConstructor]
        public Created_V1(
            string event_id,
            int event_version,
            string event_name,
            DateTimeOffset event_time,
            string producer,
            Data data
        )
        {
            this.event_version = event_version != 1 ? throw new SchemaException(nameof(event_version)) : event_version;
            this.event_name = event_name != Kind ? throw new SchemaException(nameof(event_name)) : event_name;

            this.event_id = event_id ?? throw new ArgumentNullException(nameof(event_id));
            this.event_time = event_time;
            this.producer = producer ?? throw new ArgumentNullException(nameof(producer));
            this.data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public class Data
        {
            public Guid task_uuid { get; }
            public Guid assignee { get; }
            public string title { get; }
            public string? description { get; }
            public bool done { get; }

            [JsonConstructor]
            public Data(Guid task_uuid, Guid assignee, string title, string? description, bool done)
            {
                this.task_uuid = task_uuid;
                this.assignee = assignee;
                this.title = title ?? throw new ArgumentNullException(nameof(title));
                this.description = description;
                this.done = done;
            }
        }
    }

    public class Reassigned_V1
    {
        public static string Kind = "TaskReassigned";

        public string event_id { get; }
        public int event_version { get; }
        public string event_name { get; }
        public DateTimeOffset event_time { get; }
        public string producer { get; }
        public Data data { get; }

        [JsonConstructor]
        public Reassigned_V1(
            string event_id,
            int event_version,
            string event_name,
            DateTimeOffset event_time,
            string producer,
            Data data
        )
        {
            this.event_version = event_version != 1 ? throw new SchemaException(nameof(event_version)) : event_version;
            this.event_name = event_name != Kind ? throw new SchemaException(nameof(event_name)) : event_name;

            this.event_id = event_id ?? throw new ArgumentNullException(nameof(event_id));
            this.event_time = event_time;
            this.producer = producer ?? throw new ArgumentNullException(nameof(producer));
            this.data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public class Data
        {
            public Guid task_uuid { get; }
            public Guid was_assignee_uuid { get; }
            public Guid now_assignee_uuid { get; }

            [JsonConstructor]
            public Data(Guid task_uuid, Guid was_assignee_uuid, Guid now_assignee_uuid)
            {
                this.task_uuid = task_uuid;
                this.was_assignee_uuid = was_assignee_uuid;
                this.now_assignee_uuid = now_assignee_uuid;
            }
        }
    }
}