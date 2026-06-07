using Loli.Patches;
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Addons.Audio;
using Qurre.API.Addons.Audio.Objects;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VoiceChat;

namespace Loli.Modules.Voices
{
    static class VoiceCore
    {
        static internal void PlayAudio(List<string> pathes)
        {
            AudioPlayerBot audioPlayer = Qurre.API.Audio.CreateNewAudioPlayer("C.A.S.S.I.E.", RoleTypeId.Spectator, Vector3.zero, Vector3.zero);
            audioPlayer.RunCoroutine();

            FixSpoiled.Whitelist.Add(audioPlayer.ReferenceHub);

            DontPlayInPodval blackList = new();

            foreach (string path in pathes)
            {
                var audioTask = audioPlayer.Play(new StreamAudio(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)), VoiceChatChannel.Intercom);
                audioTask.Blacklist.AccessConditions.Add(blackList);
            }

            Timing.RunCoroutine(audioPlayer.CheckPlayingAndDestroy());
            Timing.RunCoroutine(HideFromList(audioPlayer));

            static IEnumerator<float> HideFromList(AudioPlayerBot audioPlayer)
            {
                for (int i = 0; i < 5; i++)
                {
                    yield return Timing.WaitForSeconds(0.2f);
                    audioPlayer.ReferenceHub.roleManager.ServerSetRole(RoleTypeId.Spectator, RoleChangeReason.None);
                }
            }
        }

        static internal AudioPlayerBot PlayInIntercom(string file, string botName = "Dummy")
        {
            AudioPlayerBot audioPlayer = AudioExtensions.PlayInIntercom(file, botName, blacklist: [new DontPlayInPodval()]);
            FixSpoiled.Whitelist.Add(audioPlayer.ReferenceHub);
            return audioPlayer;
        }
    }
}