namespace AsyncArch.Services.Tasks.Events;

// {
//     "$schema": "http://json-schema.org/draft-04/schema#",
//
//     "title": "Accounts.Deleted.v1",
//     "description": "json schema for CUD account events (version 1)",
//
//     "definitions": {
//         "event_data": {
//             "type": "object",
//             "properties": {
//                 "public_id": {
//                     "type": "string"
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
//         "event_name":    { "enum": ["AccountDeleted"] },
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
public class AccountDeleted
{
    public static string Kind = "AccountDeleted";
    
    public string event_id { get; set; }
    public int event_version { get; set; }
    public string event_name { get; set; }
    public DateTimeOffset event_time { get; set; }
    public string producer { get; set; }
    public Data data { get; set; }

    public class Data
    {
        public Guid public_id { get; set; }
    }
}