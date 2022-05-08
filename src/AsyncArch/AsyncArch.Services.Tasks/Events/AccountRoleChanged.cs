using System.Text.Json.Serialization;

namespace AsyncArch.Services.Tasks.Events;

// {
//     "$schema": "http://json-schema.org/draft-04/schema#",
//
//     "title": "Accounts.RoleChanged.v1",
//     "description": "json schema for BE account events (version 1)",
//
//     "definitions": {
//         "event_data": {
//             "type": "object",
//             "properties": {
//                 "public_id": {
//                     "type": "string"
//                 },
//                 "role": {
//                     "type": "string"
//                 }
//             },
//             "required": [
//             "public_id",
//             "role"
//                 ]
//         }
//     },
//
//     "type": "object",
//
//     "properties": {
//         "event_id":      { "type": "string" },
//         "event_version": { "enum": [1] },
//         "event_name":    { "enum": ["AccountRoleChanged"] },
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
public class AccountRoleChanged
{
    public static string Kind = "AccountRoleChanged";
    
    public string event_id { get; }
    public int event_version { get; }
    public DateTimeOffset event_time { get; }
    public string producer { get; }
    public string event_name { get; }
    public Data data { get; }

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

    [JsonConstructor]
    public AccountRoleChanged(
        string event_id,
        int event_version,
        DateTimeOffset event_time,
        string producer,
        string event_name,
        Data data
    )
    {
        this.event_id = event_id ?? throw new ArgumentNullException(nameof(event_id));
        this.event_version = event_version;
        this.event_time = event_time;
        this.producer = producer ?? throw new ArgumentNullException(nameof(producer));
        this.event_name = event_name ?? throw new ArgumentNullException(nameof(event_name));
        this.data = data ?? throw new ArgumentNullException(nameof(data));
    }
}