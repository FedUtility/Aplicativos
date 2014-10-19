using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LX_Orbwalker;
using LeagueSharp.Common;

namespace FedAllChampionsUtility
{
	class Program
	{
		public const int LocalVersion = 68;
		public static Champion Champion;
		public static Menu Menu;		     
        public static Helper Helper;
        public static Map map;
        //public static jJungleTimers jungler;

        public static IEnumerable<Obj_AI_Hero> AllHeros = ObjectManager.Get<Obj_AI_Hero>();
        public static IEnumerable<Obj_AI_Hero> AllHerosFriend = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly);
        public static IEnumerable<Obj_AI_Hero> AllHerosEnemy = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy);
        

        private static void Main(string[] args)
		{
            Game.PrintChat("======  Federal Utilitys Loaded! ======");
            Game.PrintChat("Beta version v 1.0");
            Game.PrintChat("This is a Beta version, not all is active,");
            Game.PrintChat("=================================");

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;                        
		}	
        
        private static void Game_OnGameLoad(EventArgs args)
		{
			//AutoUpdater.InitializeUpdater();
            /*if (!LiberarHWID())
            {
                Chat.Print("HWID nao Autorizado!...");
                Chat.Print("Entre em Contato com o Federal!");
                Chat.Print("Obrigado!");
            }
            else
            { */

                Helper = new Helper();
                map = new Map();
                //jungler = new jJungleTimers();

                Menu = new Menu("FedAllChampionsUtility", "FedAllChampionsUtility_" + ObjectManager.Player.ChampionName, true);

                var targetSelectorMenu = new Menu("Target Selector", "TargetSelector");
                SimpleTs.AddToMenu(targetSelectorMenu);
                Menu.AddSubMenu(targetSelectorMenu); 

                //Orbwalker submenu
                var orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
                LXOrbwalker.AddToMenu(orbwalkerMenu);
                Menu.AddSubMenu(orbwalkerMenu);
                
                var skinchanger = new SkinChanger();
                var disconect = new DisconnectAlerter();
                var autolevelspells = new AutoLevelSpells();
                //var lasthitmarker = new LasthitMarker();
                //var enemmyrange = new EnemmyRange();
                //var potionManager = new PotionManager();
                //var killability = new Killability();
                //var trinket = new Trinket();                
                //var activator = new Activator();
                var bushRevealer = new AutoRevelarMoita();                
                var baseult = new BaseUlt();

                var trackerMenu = new Menu("Tracker", "Tracker");
                Tracker.AddtoMenu(trackerMenu);

                var trackerward = new Menu("Ward Tracker", "Ward Tracker");
                WardTracker.AttachToMenu(trackerward);

                var revelar = new Revealer();

                var activatorMenu = new Menu("Activator", "Activator");
                Activator.AddtoMenu(activatorMenu);

                try
                {
                    var handle = System.Activator.CreateInstance(null, "FedAllChampionsUtility." + ObjectManager.Player.ChampionName);
                    Champion = (Champion)handle.Unwrap();
                }
                catch (Exception)
                {
                    //Champion = new Champion(); //Champ not supported
                }

                Menu.AddToMainMenu();
            //}
		}

        /*
        private static bool LiberarHWID()
        {
            System.Net.WebClient Wc = new System.Net.WebClient();
            var hwid = Wc.DownloadString("http://goo.gl/mVgCyT");

            if (hwid.Contains(Protect.getUniqueID("C")))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
         */
	}
}
