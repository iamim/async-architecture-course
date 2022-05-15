namespace AsyncArch.Services.Tasks.Events;

// {
//     "$schema": "http://json-schema.org/draft-04/schema#",
//
//     "title": "Accounts.Updated.v1",
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
//             "public_id"
//                 ]
//         }
//     },
//
//     "type": "object",
//
//     "properties": {
//         "event_id":      { "type": "string" },
//         "event_version": { "enum": [1] },
//         "event_name":    { "enum": ["AccountUpdated"] },
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
public class AccountUpdated
{
    public static string Kind = "AccountUpdated";
    
    public string event_id { get; set; }
    public int event_version { get; set; }
    public string event_name { get; set; }
    public DateTimeOffset event_time { get; set; }
    public string producer { get; set; }
    public Data data { get; set; }

    public class Data
    {
        public Guid public_id { get; set; }
        public string email { get; set; }
        public string? full_name { get; set; }
        public string? position { get; set; }
    }
}