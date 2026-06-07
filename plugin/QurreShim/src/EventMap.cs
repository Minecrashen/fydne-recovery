// Карта Qurre-событие → тип структуры + обвязка событий LabAPI.
// Наполняется ИНКРЕМЕНТАМИ по мере добавления event-структур (см. docs/08).
using System;
using System.Collections.Generic;
using Qurre.Events.Structs;

namespace Qurre.API
{
    internal static class EventMap
    {
        static readonly Dictionary<object, Type> _enumToStruct = new Dictionary<object, Type>();

        /// <summary>Маппинг enum-значений Qurre на конкретные типы структур (для хендлеров с IBaseEvent-параметром).</summary>
        internal static void PopulateEnumMap()
        {
            // Заполняется по мере реализации структур, напр.:
            // _enumToStruct[Qurre.Events.PlayerEvents.Join] = typeof(Qurre.Events.Structs.JoinEvent);
        }

        internal static Type EnumToStruct(object enumValue)
            => enumValue != null && _enumToStruct.TryGetValue(enumValue, out var t) ? t : null;

        /// <summary>Подписка на события LabAPI и трансляция их в Qurre-структуры. Инкрементально.</summary>
        internal static void WireLabApi()
        {
            // Пример (добавляется по событию):
            // LabApi.Events.Handlers.PlayerEvents.Joined += OnJoined;
        }

        internal static void UnwireLabApi()
        {
            // LabApi.Events.Handlers.PlayerEvents.Joined -= OnJoined;
        }
    }
}
