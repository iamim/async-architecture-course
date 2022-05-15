using System.Text.Json.Serialization;

namespace AsyncArch.Schema.Events;

public static class Account
{
    public class Created_V1
    {
        public static string Kind = "AccountCreated";

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
            public Guid public_id { get; }
            public string email { get; }
            public string? full_name { get; }
            public string? position { get; }

            [JsonConstructor]
            public Data(
                Guid public_id,
                string email,
                string? full_name,
                string? position
            )
            {
                this.public_id = public_id;
                this.email = email ?? throw new ArgumentNullException(nameof(email));
                this.full_name = full_name;
                this.position = position;
            }
        }
    }

    public class Deleted_V1
    {
        public static string Kind = "AccountDeleted";

        public string event_id { get; }
        public int event_version { get; }
        public string event_name { get; }
        public DateTimeOffset event_time { get; }
        public string producer { get; }
        public Data data { get; }
        
        [JsonConstructor]
        public Deleted_V1(
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
            public Guid public_id { get; }
            
            [JsonConstructor]
            public Data(Guid public_id)
            {
                this.public_id = public_id;
            }
        }
    }

    public class RoleChanged_V1
    {
        public static string Kind = "AccountRoleChanged";

        public string event_id { get; }
        public int event_version { get; }
        public DateTimeOffset event_time { get; }
        public string producer { get; }
        public string event_name { get; }
        public Data data { get; }
        
        [JsonConstructor]
        public RoleChanged_V1(
            string event_id,
            int event_version,
            DateTimeOffset event_time,
            string producer,
            string event_name,
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
            public Guid public_id { get; }
            public string role { get; }

            [JsonConstructor]
            public Data(Guid public_id, string role)
            {
                this.public_id = public_id;
                this.role = role ?? throw new ArgumentNullException(nameof(role));
            }
        }
    }

    public class Updated_V1
    {
        public static string Kind = "AccountUpdated";

        public string event_id { get; }
        public int event_version { get; }
        public string event_name { get; }
        public DateTimeOffset event_time { get; }
        public string producer { get; }
        public Data data { get; }
        
        [JsonConstructor]
        public Updated_V1(
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
            public Guid public_id { get; }
            public string email { get; }
            public string? full_name { get; }
            public string? position { get; }
            
            [JsonConstructor]
            public Data(Guid public_id, string email, string? full_name, string? position)
            {
                this.public_id = public_id;
                this.email = email ?? throw new ArgumentNullException(nameof(email));
                this.full_name = full_name;
                this.position = position;
            }
        }
    }
}