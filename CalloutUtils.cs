using CitizenFX.Core;
using FivePD.API.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Custom_Callouts
{
    internal static class CalloutUtils
    {
        static readonly List<PedHash> victims = new List<PedHash>(){
            PedHash.Bankman,
            PedHash.Barry,
            PedHash.Business01AMM,
            PedHash.Business02AFY,
            PedHash.KorBoss01GMM,
            PedHash.Vinewood03AFY,
            PedHash.MrsPhillips,
            PedHash.Socenlat01AMM,
            PedHash.ShopMidSFY,
            PedHash.Bevhills02AMM,
            PedHash.Abigail,
            PedHash.Miranda,
            PedHash.Bevhills01AFY,
            PedHash.Finguru01,
            PedHash.Hipster03AMY,
            PedHash.Paparazzi,
            PedHash.Tourist01AFM,
            PedHash.Baygor,
            PedHash.Bevhills01AMM,
            PedHash.Soucent02AFY,
            PedHash.KerryMcintosh,
            PedHash.Bevhills02AFY,
            PedHash.Mistress,
            PedHash.Salton02AMM,
            PedHash.PopovCutscene,
            PedHash.Fatlatin01AMM,
            PedHash.Eastsa02AFM,
            PedHash.Downtown01AFM,
            PedHash.Bevhills02AMY,
            PedHash.Azteca01GMY,
            PedHash.Epsilon01AFY,
            PedHash.Hasjew01AMM,
            PedHash.MPros01,
            PedHash.HughCutscene,
            PedHash.Soucent01AFM,
            PedHash.Car3Guy2,
            PedHash.Bevhills01AMY,
            PedHash.Epsilon01AMY,
            PedHash.Josh,
            PedHash.KorLieut01GMY,
            PedHash.Beachvesp01AMY,
            PedHash.Hipster01AFY,
            PedHash.Polynesian01AMY,
            PedHash.G,
            PedHash.Car3Guy1,
            PedHash.Stlat01AMY,
            PedHash.Soucent03AFY,
            PedHash.Soucent04AMY,
            PedHash.ScreenWriterCutscene,
            PedHash.Korean02GMY,
            PedHash.Tourist02AFY,
            PedHash.Strvend01SMY,
            PedHash.Farmer01AMM,
            PedHash.Hotposh01,
            PedHash.PrologueHostage01AMM,
            PedHash.Hipster02AFY,
            PedHash.Genstreet01AMY,
            PedHash.AviSchwartzmanCutscene,
            PedHash.Stbla02AMY,
            PedHash.Paper,
            PedHash.Tourist01AMM,
            PedHash.Taphillbilly,
            PedHash.Busicas01AMY,
            PedHash.Soucent02AMM,
            PedHash.Bevhills02AFM,
            PedHash.Business03AMY,
            PedHash.CustomerCutscene,
            PedHash.Soucent02AFO,
            PedHash.Gay02AMY,
            PedHash.Hipster03AFY,
            PedHash.ShopLowSFY,
            PedHash.Polynesian01AMM,
            PedHash.RoccoPelosiCutscene,
            PedHash.Epsilon02AMY,
            PedHash.WeiCheng,
            PedHash.Genstreet01AMO,
            PedHash.Busker01SMO,
            PedHash.ShopHighSFM,
            PedHash.Business03AFY,
            PedHash.GunVend01,
            PedHash.Business02AMY,
            PedHash.DeniseFriendCutscene
        };
        static readonly List<PedHash> muggers = new List<PedHash>(){
            PedHash.RampHicCutscene,
            PedHash.FibMugger01,
            PedHash.Strvend01SMY,
            PedHash.MexGoon03GMY,
            PedHash.Taphillbilly,
            PedHash.Trevor,
            PedHash.G,
            PedHash.PoloGoon02GMY,
            PedHash.ChinGoonCutscene,
            PedHash.MexLabor01AMM,
            PedHash.Salton03AMM,
            PedHash.Robber01SMY,
            PedHash.PrologueHostage01,
            PedHash.ArmGoon02GMY,
            PedHash.CletusCutscene,
            PedHash.Hunter,
            PedHash.Salton01AMY
        };

        internal static Ped GetNearestPlayer(Ped shooter, List<Ped> playersToShoot){
            return playersToShoot.OrderBy(x => World.GetDistance(x.Position, shooter.Position)).FirstOrDefault();
        }

        internal static Ped GetNearestPed(Ped p, List<Ped> ignoreList){
            Dictionary<Ped, float> closePeds = new Dictionary<Ped, float>();

            //Get all peds in the world and checks if they are players or if they are in the ignore list and are alive
            //If the check is passed sorts the list by closest distance
            World.GetAllPeds().Where(ped => ped != Game.PlayerPed && !ignoreList.Contains(ped) && ped.IsAlive).ToList()
                .ForEach(ped => closePeds.Add(ped, (World.GetDistance(ped.Position, p.Position))));

            //No peds return null
            if (closePeds.Count == 0){
                return null;
            }

            //return first closest ped
            return closePeds.OrderBy(distance => distance.Value).FirstOrDefault().Key;
        }

        internal static Vector3 GetRandomPOS(int min = 200, int max = 650){
            Random rand = new Random();

            int distance = rand.Next(min, max);
            float offsetX = rand.Next(-1 * distance, distance);
            float offsetY = rand.Next(-1 * distance, distance);

            return new Vector3(offsetX, offsetY, 0);
        }

        internal static PedHash GetRandomPedHash(string role){
            switch (role){
                case "victim":
                    return victims.SelectRandom();

                case "mugger":
                    return muggers.SelectRandom();

                default:
                    return RandomUtils.GetRandomPed();
            }
        }
    }
}
