using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Onebot.Protocol.Models.Actions;
using Onebot.Protocol.Models.Events;
using Onebot.Protocol.Models.Events.Message;
using Onebot.Protocol.Models.Events.Meta;
using Onebot.Protocol.Models.Events.Notice;
using Onebot.Protocol.Models.Receipts;

namespace Onebot.Protocol.Models;

public static class ModelFactory
{
    private static readonly JsonSerializer serializer;

    private static readonly JsonSerializerSettings settings;

    static ModelFactory()
    {
        settings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            NullValueHandling = NullValueHandling.Ignore
        };
        serializer = JsonSerializer.Create(settings);
    }

    private static readonly Dictionary<string, Type> eventRegistry = new()
    {
        // meta
        { "meta.heartbeat", typeof(HeartbeatEvent) },
        // message
        { "message.group", typeof(GroupMessageEvent) },
        { "message.private", typeof(PrivateMessageEvent) },
        // notice.private
        { "notice.friend_increase", typeof(FriendIncreaseEvent) },
        { "notice.friend_decrease", typeof(FriendDecreaseEvent) },
        { "notice.private_message_delete", typeof(PrivateMessageDeleteEvent) },
        // notice.group
        { "notice.group_member_increase", typeof(GroupMemberIncreaseEvent) },
        { "notice.group_member_decrease", typeof(GroupMemberDecreaseEvent) },
        { "notice.group_member_ban", typeof(GroupMemberBanEvent) },
        { "notice.group_member_unban", typeof(GroupMemberUnbanEvent) },
        { "notice.group_admin_set", typeof(GroupAdminSetEvent) },
        { "notice.group_admin_unset", typeof(GroupAdminUnsetEvent) },
        { "notice.group_message_delete", typeof(GroupMessageDeleteEvent) }
    };

    public static EventBase ConstructEvent(JObject obj)
    {
        string type = obj.Value<string>("type");
        string detailedType = obj.Value<string>("detail_type");
        string subType = obj.Value<string>("sub_type");

        string key = $"{type}.{detailedType}";
        if (eventRegistry.ContainsKey(key))
        {
            var evt = obj.ToObject(eventRegistry[key], serializer) as EventBase;
            return evt;
        }
        else
        {
            return new UnknownEvent();
        }
    }

    public static string SerializeAction(ActionBase action, string echo)
    {
        dynamic obj = new
        {
            action = action.Action,
            @params = action,
            echo
        };

        return JsonConvert.SerializeObject(obj, settings);
    }

    public static ReceiptBase ConstructReceipt(JObject obj, Type receipt)
    {
        return obj["data"].ToObject(receipt, serializer) as ReceiptBase;
    }
}