using CitizenFX.Core;
using FivePD.API;
using FivePD.API.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Custom_Callouts
{
    [CalloutProperties("Mugging", "HuskyNinja953", "1.0.0")]
    internal class Mugging_Callout : Callout
    {
        private readonly Random rand = new Random();

        private int vicFightChance;
        private Ped victim, mugger;
        private PedData victimData, muggerData;
        private PlayerData playerData;

        private WeaponHash[] weapons = new WeaponHash[]
        {
            WeaponHash.Knife,
            WeaponHash.Bat,
            WeaponHash.KnuckleDuster,
            WeaponHash.Hammer,
            WeaponHash.Hatchet,
            WeaponHash.Machete,
            WeaponHash.Crowbar,
            WeaponHash.SwitchBlade,
            WeaponHash.Unarmed,
            WeaponHash.Pistol
        };

        //Struct to hold coords and name of a Mugging Location
        private struct Location
        {
            public Vector3 Coords;
            public string Name;
            public float StartDistance;

            public Location(Vector3 coords, string name)
            {
                Coords = coords;
                Name = name;
                StartDistance = 35f;
            }

            public Location(Vector3 coords, string name, float startDist)
            {
                Coords = coords;
                Name = name;
                StartDistance = startDist;
            }
        }

        //Hold the selected Location
        private Location calloutLoc;

        //Create a list of locations for the callout
        private List<Location> locations = new List<Location>(){
            new Location(new Vector3(-735.6353f, -276.373f, 36.96165f), "in the parking lot behind Cougari Luxury Goods"),
            new Location(new Vector3(68.57333f, 185.0022f, 104.4937f), "near the parking lot behind Bishop's WTF"),
            new Location(new Vector3(1127.092f, -643.8586f, 56.37044f), "by the pavillion in Mirror Park"),
            new Location(new Vector3(1160.286f, -1651.355f, 36.52641f), "near the garages on Fudge Lane"),
            new Location(new Vector3(260.0455f, -1768.386f, 28.20277f), "in the alley behind the LiquorMart in Strawberry", 40f),
            new Location(new Vector3(56.62909f, -897.9049f, 29.68532f), "in the alley behind Frenchies Resturant"),
            new Location(new Vector3(-1569.943f, -984.1662f, 12.62407f), "near the Del Perro Pier Parking Lot"),
            new Location(new Vector3(-1324.984f, -228.2221f, 42.48089f), "behind Gussets Luxury Underwear"),
            new Location(new Vector3(-309.1848f, 70.48733f, 64.98194f), "in an alley by a large apartment complex"),
            new Location(new Vector3(221.7962f, -168.6344f, 56.07671f), "in the parking lot of Oveure", 25f),
            new Location(new Vector3(381.56f, -345.71f, 46.81f),"by the taco place in Vinewood")
        };

        public Mugging_Callout()
        {
            //Select random Location from list
            calloutLoc = locations.SelectRandom();
            //Get Player Data
            playerData = Utilities.GetPlayerData();

            //Setup callout
            InitInfo(calloutLoc.Coords);
            ShortName = "Mugging in Progress";
            CalloutDescription = String.Format("Unit {0} be advised: Caller reports a possible mugging in progress.\n    -- Unknown description of either party at this time.\n    -- Last known location was {1}.\n    -- Caller stated the mugger was possibly armed.\n    -- Proceed with extreme caution", playerData.Callsign, calloutLoc.Name);
            ResponseCode = 3;
            StartDistance = 150f;
        }
        public override async Task OnAccept()
        {
            try
            {
                InitBlip();
                UpdateData(CalloutDescription);
            }
            catch
            {
                EndCallout();
            }
        }
        public override async void OnStart(Ped player)
        {
            base.OnStart(player);
            vicFightChance = rand.Next(101);

            try
            {
                //Spawn victim and suspect
                victim = await GeneratePed(false);
                mugger = await GeneratePed(true);

                //Grab data
                victimData = await victim.GetData();
                muggerData = await mugger.GetData();

                //Attach Blips
                victim.AttachBlip();
                victim.AttachedBlip.Color = BlipColor.Blue;
                mugger.AttachBlip();

                //Give mugger items
                GenerateStolenItems();
                //Give mugger weapon
                mugger.Weapons.Give(weapons.SelectRandom(), 50, true, true);
            }
            catch
            {
                //If any of the above fails terminate the callout
                EndCallout();
            }

            //play animations
            await victim.Task.PlayAnimation("amb@code_human_cower_stand@male@idle_a", "idle_b", 1f, 1f, -1, AnimationFlags.Loop, 1f);
            await mugger.Task.PlayAnimation("random@mugging2", "ortega_stand_loop_ort", 1f, 1f, -1, AnimationFlags.Loop, 1f);

            //Wait until the player is near the mugger to being the callout
            while (World.GetDistance(AssignedPlayers.FirstOrDefault().Position, mugger.Position) > calloutLoc.StartDistance) { await BaseScript.Delay(250); }

            //Call the callout dialogue to begin
            CalloutDialogue();

            //Give Mugger and Victim their callout questions
            VictimQuestions();
            MuggerQuestions();

            //RNG to see if the victim fights the mugger
            if (vicFightChance <= 5)
            {
                victim.Task.ClearAllImmediately();
                victim.Task.FightAgainst(mugger);
            }
            else
            {
                victim.Task.ClearAllImmediately();
                victim.Task.RunTo(AssignedPlayers.FirstOrDefault().Position);
            }

            //Mugger attacks victim
            mugger.Task.ClearAllImmediately();
            mugger.Task.FightAgainst(victim);
        }
        public override void OnCancelBefore()
        {
            base.OnCancelBefore();

            try
            {
                if (victim.IsAlive) { victim.Task.WanderAround(); }
                if (mugger.IsAlive && !mugger.IsCuffed) { mugger.Task.WanderAround(); }
            }
            catch
            {
                //Do nothing they peds can be deleted manually in game
            }
        }
        private void MuggerQuestions()
        {
            //Question 1
            PedQuestion q1 = new PedQuestion();
            q1.Question = String.Format("What's going on here {0}?", muggerData.Gender == Gender.Male ? "Sir." : "Ma'am");
            q1.Answers = new List<string>(){
                "I want to talk to a lawyer.",
                "~y~*Stares angrily at the victim*~s~",
                "It's not what it looks like!",
                "Fuck you pig!",
                "~y~*Stares regretfully at the ground*~s~",
                "How much to make this go away?"
            };

            //Question 2
            PedQuestion q2 = new PedQuestion();
            q2.Question = String.Format("Why do you have {0} {1}?", victimData.Gender == Gender.Male ? "his" : "her", victimData.Gender == Gender.Male ? "wallet" : "purse");
            q2.Answers = new List<string>(){
                "Found it while walking down the street.",
                "I want a lawyer.",
                "They gave it to me.",
                "~y~*Stares regretfully at the ground*~s~",
                "~y~*Stares angrily at the victim*~s~",
                "I needed money, I did what I had to do.",
                "I saw an opportunity and I took it."
            };


            PedQuestion q3 = new PedQuestion();
            q3.Question = String.Format("{0} at this time I am placing you under arrest.", muggerData.Gender == Gender.Male ? "Sir" : "Ma'am");
            q3.Answers = new List<string>(){
                "I regret nothing.",
                "Officer can we please work this out?",
                "I understand.",
                "Fuck you pig!",
                "~y~*Spits in officer's face*~s~",
                "Let's get this over with."
            };

            //Add questions to array to assign the questions to the mugger
            PedQuestion[] muggerQuestions = new PedQuestion[] { q1, q2, q3 };

            //Add the questions to the mugger
            AddPedQuestions(mugger, muggerQuestions);
        }
        private void VictimQuestions()
        {
            PedQuestion whatsGoingOn = new PedQuestion();
            PedQuestion medicalAttn = new PedQuestion();
            PedQuestion safe = new PedQuestion();
            PedQuestion taxi = new PedQuestion();

            whatsGoingOn.Question = String.Format("Can you tell me what's going on here {0}?", victimData.Gender == Gender.Male ? "Sir" : "Miss");
            whatsGoingOn.Answers = new List<string>(){
                String.Format("I was walking home from work and this creep attacked me out of no where! They stole my phone and {0}!", victimData.Gender == Gender.Male ? "wallet" : "purse"),
                String.Format("I've been mugged!"),
                String.Format("That person is crazy! They stole my {0}!", victimData.Gender == Gender.Male ? "wallet" : "purse"),
                String.Format("Same thing that always happens in this city..."),
                String.Format("Classic greed..."),
                String.Format("{0} came out of no where and demanded all my money and my phone!",muggerData.Gender == Gender.Male ? "He" : "She"),
                String.Format("{0} just stole my phone and {1}!", muggerData.Gender == Gender.Male ? "He" : "She", victimData.Gender == Gender.Male ? "wallet" : "purse")
            };

            medicalAttn.Question = String.Format("Are you okay? Do you need medical attention?");
            medicalAttn.Answers = new List<string>(){
                String.Format("No I'm okay, just frightened."),
                String.Format("Yes please, I'm having trouble breathing!"),
                String.Format("No I don't think so."),
                String.Format("No, you showed up just in time!"),
                String.Format("Nah, I'm good."),
                String.Format("I'm okay, it's just a scratch."),
                String.Format("I don't think so. ~y~*Checks self for wounds*~s~")
            };

            safe.Question = String.Format("Rest assured, you are safe now {0}.", victimData.Gender == Gender.Male ? "Sir" : "Miss");
            safe.Answers = new List<string>(){
                String.Format("Thank you officer."),
                String.Format("I hope that creep rots in jail!"),
                String.Format("What is the world coming to? ~y~*sheds a single tear*~s~"),
                String.Format("I don't know if I'll ever really be safe in this city."),
                String.Format("{0} won't be after I get in touch with my lawyer!", muggerData.Gender == Gender.Male ? "He" : "She"),
                String.Format("Thank God you showed up when you did!"),
                String.Format("I am now thanks to San Andreas' Finest!")
            };

            taxi.Question = String.Format("Let me call you a taxi.");
            taxi.Answers = new List<string>(){
                String.Format("No thank you officer, I'll walk."),
                String.Format("Thank you officer, I appreciate that."),
                String.Format("I'm good, I live right over there."),
                String.Format("Thank you officer but, I can manage."),
                String.Format("And get robbed again? No thank you!"),
                String.Format("Not necessary but, thank you."),
                String.Format("All set, I have a{0} on the way.", rand.Next(0,100) >= 50 ? "n Uber": " Lyft")
            };

            PedQuestion[] victimQuestions = new PedQuestion[]{
                whatsGoingOn,
                medicalAttn,
                safe,
                taxi
            };

            //Add the callout questions to the victim
            AddPedQuestions(victim, victimQuestions);
        }
        private async void CalloutDialogue()
        {

            //Init dialogue
            string muggerDialogue;
            string victimDialogue;
            bool hasGun = CheckForPistol(mugger.Weapons);

            //RNG for some special text
            if (rand.Next(1, 101) <= 2)
            {
                muggerDialogue = String.Format("~r~Suspect~s~: Give me your ~r~phone~s~ and all your ~g~money~s~ {0}!", victimData.Gender == Gender.Male ? "asshole" : "bitch");
            }
            else if (hasGun)
            {
                muggerDialogue = String.Format("~r~Suspect~s~: If you run I'm going to ~r~shoot~s~! Now give me your ~r~phone~s~ and ~r~{0}~s~!", victimData.Gender == Gender.Male ? "wallet" : "purse");
            }
            else
            {
                muggerDialogue = "~r~Suspect~s~: Give me your ~r~phone~s~ and all your ~g~cash~s~!";
            }

            //Show the mugger demaning phone and cash, display for 5 seconds if they are alive
            if (mugger.IsAlive) { ShowDialog(muggerDialogue, 5000, 35f); }

            //Wait for 5 seconds
            await BaseScript.Delay(6000);

            //Show the victim calling for the police as they approach and begin to run away - 6 seconds on screen
            try
            {
                if (!hasGun && victim.IsAlive)
                {
                    victimDialogue = String.Format("~b~Victim~s~: Help me ~b~Officer~s~, {0} robbed me!", muggerData.Gender == Gender.Male ? "He" : "She");
                    ShowDialog(victimDialogue, 6000, 35f);
                }
                else if (hasGun && victim.IsAlive)
                {
                    victimDialogue = String.Format("~b~Victim~s~: {0} got a ~r~gun~s~! Help me ~b~Officer~s~!", muggerData.Gender == Gender.Male ? "He's" : "She's");
                    ShowDialog(victimDialogue, 6000, 35f);
                }
            }
            catch
            {
                //Show no dialogue for victim
            }
        }
        private async Task<Ped> GeneratePed(bool isSuspect)
        {
            Ped ped;

            if (isSuspect)
            {
                ped = await SpawnPed(CalloutUtils.GetRandomPedHash("mugger"), calloutLoc.Coords, 287.88f);
                muggerData = await ped.GetData();
                ped.DropsWeaponsOnDeath = false;
            }
            else
            {
                ped = await SpawnPed(CalloutUtils.GetRandomPedHash("victim"), calloutLoc.Coords);
                victimData = await ped.GetData();
            }

            ped.AlwaysKeepTask = true;
            ped.BlockPermanentEvents = true;

            return ped;
        }
        private void GenerateStolenItems()
        {
            int chance = rand.Next(101);
            Item phone, wallet, cash, watch, purse;
            List<Item> stolenItems = new List<Item>(4);

            //Items
            phone = new Item();
            phone.Name = "phone";
            phone.IsIllegal = true;
            stolenItems.Add(phone);

            wallet = new Item();
            wallet.Name = "wallet";
            wallet.IsIllegal = true;
            stolenItems.Add(wallet);

            purse = new Item();
            purse.Name = "purse";
            purse.IsIllegal = true;
            stolenItems.Add(purse);

            cash = new Item();
            cash.Name = String.Format("${0}.00", rand.Next(475));
            cash.IsIllegal = true;
            stolenItems.Add(cash);

            watch = new Item();
            watch.Name = "Bolex Watch";
            watch.IsIllegal = true;
            stolenItems.Add(watch);

            //Deletes all items from the victim
            victimData.Items.Clear();
            muggerData.Items.Clear();

            //Give guys wallets and ladies purses
            if (victimData.Gender == Gender.Male)
            {
                muggerData.Items.Add(wallet);
            }
            else
            {
                muggerData.Items.Add(purse);
            }
            //Small chance to give the mugger a stolen bolex watch
            if (chance <= 2) { muggerData.Items.Add(watch); }

            //Add items into Muggers Data
            muggerData.Items.Add(phone);
            muggerData.Items.Add(cash);

            //Update data
            mugger.SetData(muggerData);
            victim.SetData(victimData);
        }
        private bool CheckForPistol(WeaponCollection weapons)
        {
            bool hasPistol = false;

            if (weapons.Current.Hash == WeaponHash.Pistol)
            {
                hasPistol = true;
            }

            return hasPistol;
        }
    }
}
