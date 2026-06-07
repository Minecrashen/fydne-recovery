# Паттерн: транспайлер-заглушка → префикс

12 файлов в `Loli-Merged` используют транспайлеры. Многие просто вырезают тело метода
(`yield return Ret`). Это самый хрупкий вид патча: при смене сигнатуры целевого метода
рассинхрон стека даёт `InvalidProgramException` и краш JIT.

Замена на префикс делает патч устойчивее и читаемее.

## Случай A — метод просто «выключают» (вырезают тело)

```csharp
// БЫЛО (хрупко):
[HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.ServerProcessCmd))]
static class FixSkeletonAttack
{
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
    {
        yield return new CodeInstruction(OpCodes.Ret);
    }
}

// СТАЛО (устойчиво):
[HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.ServerProcessCmd))]
static class FixSkeletonAttack
{
    [HarmonyPrefix]
    static bool Prefix() => false;   // false = не выполнять оригинал
}
```

Для методов, возвращающих значение, префикс с `ref __result`:
```csharp
[HarmonyPrefix]
static bool Prefix(ref bool __result)
{
    __result = false;   // подставить нужное возвращаемое значение
    return false;       // оригинал не выполнять
}
```

## Случай B — нужна частичная подмена (вызвать свой код вместо части)

Если транспайлер был сложнее (`Ldarg_0; Call своийМетод; Ret`), переноси логику в префикс:
```csharp
// БЫЛО: транспайлер, который зовёт FixSkeletonUpdate.Update(instance) и делает Ret
// СТАЛО:
[HarmonyPrefix]
static bool Prefix(Scp3114Strangle __instance)
{
    FixSkeletonUpdate.Update(__instance);
    return false;
}
```
`__instance` = тот самый `this` (бывший `Ldarg_0`). Так уходишь от ручного IL вообще.

## Когда транспайлер реально нужен

Только если надо менять **середину** метода (вставить/убрать инструкции, не весь метод).
Тогда транспайлер оставляем, но добавляем защиту — матчинг по образцу, а не по индексам,
и `try/catch` вокруг применения (см. `04_HARMONY_PATCHES_AUDIT.md`, Приоритет 1).

## Файлы-кандидаты на конвертацию (из аудита)
`AntiCringeUpdates.cs`, `AntiNewYear.cs`, `DeleteDeadman.cs`, `FixPockerEscape.cs`,
`Anti079Recontain.cs`, `ScpSpeaks.cs`, `_DisableCassie.cs`, `NotFileLogs.cs`,
`SaveLogs.cs`, `PrintPlayer.cs`, `HideRaAuth.cs`, `RealisticArmory.cs`.
