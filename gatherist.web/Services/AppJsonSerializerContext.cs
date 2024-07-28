using System.Text.Json.Serialization;
using gatherist.web.Models;

namespace gatherist.web.Services;

[JsonSerializable(typeof(User[]))]
[JsonSerializable(typeof(Tenant[]))]
internal partial class AppJsonSerializerContext 
    : JsonSerializerContext
{
}