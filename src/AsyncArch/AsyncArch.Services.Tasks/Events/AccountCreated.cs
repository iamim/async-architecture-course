using System.Text.Json.Serialization;

namespace AsyncArch.Services.Tasks.Events;

// {
//     "$schema": "http://json-schema.org/draft-04/schema#",
//
//     "title": "Accounts.Created.v1",
//     "description": "json schema for CUD account events (version 1)",
//
//     "definitions": {
//         "event_data": {
//             "type": "object",
//             "properties": {
//                 "public_id": {
//                     "type": "string"
//                 },
//                 "email": {
//                     "type": "string"
//                 },
//                 "full_name": {
//                     "type": ["string", "null"]
//                 },
//                 "position": {
//                     "type": ["string", "null"]
//                 }
//             },
//             "required": [
//             "public_id",
//             "email"
//                 ]
//         }
//     },
//
//     "type": "object",
//
//     "properties": {
//         "event_id":      { "type": "string" },
//         "event_version": { "enum": [1] },
//         "event_name":    { "enum": ["AccountCreated"] },
//         "event_time":    { "type": "string" },
//         "producer":      { "type": "string" },
//
//         "data": { "$ref": "#/definitions/event_data" }
//     },
//
//     "required": [
//     "event_id",
//     "event_version",
//     "event_name",
//     "event_time",
//     "producer",
//     "data"
//     ]
// }
public class AccountCreated
{
    public static string Kind = "AccountCreated";
    
    public string event_id { get; }
    public int event_version { get; }
    public string event_name { get; }
    public DateTimeOffset event_time { get; }
    public string producer { get; }
    public Data data { get; }

    [JsonConstructor]
    public AccountCreated(
        string event_id,
        int event_version,
        string event_name,
        DateTimeOffset event_time,
        string producer,
        Data data
    )
    {
        this.event_id = event_id ?? throw new ArgumentNullException(nameof(event_id));
        this.event_version = event_version;
        this.event_name = event_name ?? throw new ArgumentNullException(nameof(event_name));
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