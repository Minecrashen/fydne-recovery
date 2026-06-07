using Hints;
using MEC;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using Qurre.API.Controllers;
using TMPro;
using UnityEngine;

namespace Loli.HintsCore;

static class Worker
{
    static TextMeshProUGUI textMesh;


    static internal Vector2 CalculateContentSize(string text)
    {
        return textMesh.GetPreferredValues(text.Replace(" ", "-"));
    }


    static Worker()
    {
        Timing.RunCoroutine(Coroutine());

        static IEnumerator<float> Coroutine()
        {
            while (true)
            {
                foreach (Player pl in Player.List)
                {
                    try
                    {
                        Process(pl);
                    }
                    catch (Exception ex)
                    {
#if MRP
                        Log.Debug(ex);
#elif NR
                        Log.Error(ex);
#endif
                    }
                }
                yield return Timing.WaitForSeconds(1f);
            }
        }

        static void Process(Player pl)
        {
            if (!pl.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
                return;

            display.Prepare();

            string text = $"<line-height=0><voffset={Constants.MaxVOffset}><color=red>Loli.Core</color>\n";

            foreach (DisplayBlock block in display.GetBlocks())
            {
                text += block.GetText(display);
            }

            //Log.Info(text.Replace("<", "{").Replace(">", "}"));

            pl.Client.HintDisplay.Show(new TextHint(text, new HintParameter[] { new StringHintParameter(string.Empty) }, null, 1.1f));
        }
    }


    [EventMethod(RoundEvents.Waiting, int.MaxValue)]
    static void Waiting()
    {
        textMesh = GameObject.Find("Hint Display").GetComponent<TextMeshProUGUI>();
    }

    [EventMethod(PlayerEvents.Join, int.MaxValue)]
    static void Join(JoinEvent ev)
    {
        new PlayerDisplay(ev.Player);
    }
}