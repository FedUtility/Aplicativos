using System;
using LeagueSharp;
using LeagueSharp.Common;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using LX_Orbwalker;

namespace FedAllChampionsUtility
{
    class Activator
    {
        public static BuffType[] BuffnamesCC = { BuffType.Charm, BuffType.Fear, BuffType.Flee, BuffType.Snare, BuffType.Taunt, BuffType.Stun, BuffType.Slow };
        public static List<SummonerSpell> SummonerList = new List<SummonerSpell>();
        public static bool ChampSupported = false;
        public enum Summoner
        {
            //Clairity,
            //Garrison,
            //Ghost,
            Heal,
            //Revive,
            Cleanse,            
            Barrier,
            //Teleport,
            Exhaust,
            Ignite,
            //Clairvoyance,
            Flash,
        }

        internal static void AddtoMenu(Menu menu)
        {
            var tempMenu = menu;

            var summoners = new Menu("Summoner Spells", "act_Summoner");
            AddSummonerstoMenu(summoners);
            tempMenu.AddSubMenu(summoners);

            var items = new Menu("Activateable Items", "act_Items");
            AddItemstoMenu(items);
            tempMenu.AddSubMenu(items);

            Program.Menu.AddSubMenu(tempMenu);

            Game.OnGameUpdate += Game_OnGameUpdate;
            //Drawing.OnDraw += Drawing_OnDraw;
            //Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
        }

        private static void AddItemstoMenu(Menu menu)
        {
            var tempMenu = menu;
            var potionmenu = new Menu("PotionManager", "act_potionmanager");
            PotionManager.AddtoMenusub(potionmenu);
            tempMenu.AddSubMenu(potionmenu);

            var defensivemenu = new Menu("Defensive Items", "act_defensiveItem");

            defensivemenu.AddSubMenu(new Menu("Debuff", "act_debuff"));
            foreach (var buffname in BuffnamesCC)
            {
                defensivemenu.SubMenu("act_debuff").AddItem(new MenuItem("act_debuff_" + buffname, "Anti " + buffname).SetValue(true));
            }

            var offensivemenu = new Menu("Offensive Items", "act_offensiveItem");
            ItemManager.AddSupMenu(defensivemenu, offensivemenu);
            tempMenu.AddSubMenu(defensivemenu);
            tempMenu.AddSubMenu(offensivemenu);

        }

        private static void AddSummonerstoMenu(Menu menu)
        {
            SummonerList.Add(new SummonerSpell("summonerbarrier", Summoner.Barrier));
            SummonerList.Add(new SummonerSpell("summonerheal", Summoner.Heal));
            SummonerList.Add(new SummonerSpell("summonerdot", Summoner.Ignite));
            SummonerList.Add(new SummonerSpell("summonerexhaust", Summoner.Exhaust));            
            SummonerList.Add(new SummonerSpell("summonerboost", Summoner.Cleanse));

            foreach (var spell in SummonerList.Where(spell => spell.IsActive()))
            {
                spell.AddtoMenusub(menu);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            foreach (var summonerSpell in SummonerList.Where(spell => spell.IsActive() && spell.IsReady()))
                CheckSummoner(summonerSpell);
        }

        private static void CheckSummoner(SummonerSpell spell)
        {
            bool healperzent;
            bool healhealth;
            switch (spell.Summoner)
            {                
                case Summoner.Ignite:
                    {
                        if (ChampSupported)
                            break;
                        if (!Program.Menu.Item("act_ignite_ks").GetValue<bool>())
                            break;
                        const int igniterange = 600;
                        foreach (var enemy in Program.AllHerosEnemy.Where(hero => hero.IsValidTarget(igniterange) && ObjectManager.Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite) > hero.Health))
                        {
                            spell.CastSpell(enemy);
                            break;
                        }
                    }
                    break;                
                case Summoner.Heal:
                    if (!Program.Menu.Item("act_heal_use").GetValue<bool>())
                        break;
                    const int healrange = 700;
                    foreach (var friend in Program.AllHerosFriend.Where(hero => hero.IsValid && !hero.IsDead && hero.Distance(ObjectManager.Player) < healrange))
                    {
                        if (friend.IsMe || Program.Menu.Item("useHealFriend").GetValue<bool>())
                        {
                            healperzent = false;
                            healhealth = false;
                            if (friend.Health / friend.MaxHealth * 100 <= Program.Menu.Item("act_heal_Percent").GetValue<Slider>().Value)
                                healperzent = true;
                            if (friend.Health < Program.Menu.Item("act_heal_Health").GetValue<Slider>().Value)
                                healhealth = true;
                            if (healperzent)
                            {
                                if (Program.Menu.Item("act_heal_ifEnemy1").GetValue<bool>())
                                {
                                    if (Utility.CountEnemysInRange(friend, 1000) > 0)
                                    {
                                        spell.CastSpell();
                                        break;
                                    }
                                }
                                else
                                {
                                    spell.CastSpell();
                                    break;
                                }
                            }
                            if (healhealth)
                            {
                                if (Program.Menu.Item("act_heal_ifEnemy2").GetValue<bool>())
                                {
                                    if (Utility.CountEnemysInRange(friend, 1000) > 0)
                                    {
                                        spell.CastSpell();
                                        break;
                                    }
                                }
                                else
                                {
                                    spell.CastSpell();
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case Summoner.Barrier:
                    if (!Program.Menu.Item("act_barrier_use").GetValue<bool>())
                        break;
                    healperzent = false;
                    healhealth = false;
                    if (ObjectManager.Player.Health / ObjectManager.Player.MaxHealth * 100 <= Program.Menu.Item("act_barrier_Percent").GetValue<Slider>().Value)
                        healperzent = true;
                    if (ObjectManager.Player.Health < Program.Menu.Item("act_barrier_Health").GetValue<Slider>().Value)
                        healhealth = true;
                    if (healperzent)
                    {
                        if (Program.Menu.Item("act_barrier_ifEnemy1").GetValue<bool>())
                        {
                            if (Utility.CountEnemysInRange(ObjectManager.Player, 1000) > 0)
                            {
                                spell.CastSpell();
                                break;
                            }
                        }
                        else
                        {
                            spell.CastSpell();
                            break;
                        }
                    }
                    if (healhealth)
                    {
                        if (Program.Menu.Item("act_barrier_ifEnemy2").GetValue<bool>())
                        {
                            if (Utility.CountEnemysInRange(ObjectManager.Player, 1000) > 0)
                                spell.CastSpell();
                        }
                        else
                            spell.CastSpell();
                    }
                    break;
                case Summoner.Exhaust:
                    if (!Program.Menu.Item("act_exhoust_use").GetValue<bool>())
                        break;
                    const int range = 550;
                    var maxDpsHero = Program.AllHerosEnemy.Where(hero => hero.IsValidTarget(range + 200)).OrderByDescending(x => x.BaseAttackDamage * x.AttackSpeedMod).FirstOrDefault();
                    if (maxDpsHero == null)
                        return;
                    var maxDps = maxDpsHero.BaseAttackDamage * maxDpsHero.AttackSpeedMod;
                    if (Program.AllHerosFriend.Where(hero => hero.IsAlly && hero.Distance(ObjectManager.Player) <= range).Any(friend => friend.Health <= maxDps * Program.Menu.Item("act_exhoust_killfriend").GetValue<Slider>().Value))
                        spell.CastSpell(maxDpsHero);
                    if (ObjectManager.Player.Health <= maxDps * Program.Menu.Item("act_exhoust_killme").GetValue<Slider>().Value)
                        spell.CastSpell(maxDpsHero);
                    break;
                case Summoner.Cleanse:
                    if (BuffnamesCC.Any(bufftype => ObjectManager.Player.HasBuffOfType(bufftype) && Program.Menu.Item("act_cleanse_" + bufftype).GetValue<bool>()))
                        spell.CastSpell();
                    break;
            }
        }

        internal static void Draw()
        {
            // todo
        }

        internal class SummonerSpell
        {
            public string CodedName;
            public Summoner Summoner;
            public SpellSlot SpellSlot;

            public SummonerSpell(string codedName, Summoner summoner)
            {
                CodedName = codedName;
                Summoner = summoner;
                var spell = ObjectManager.Player.SummonerSpellbook.Spells.FirstOrDefault(x => x.Name.ToLower() == codedName);
                SpellSlot = spell != null ? spell.Slot : SpellSlot.Unknown;
            }

            public bool IsActive()
            {
                return (SpellSlot.Unknown != SpellSlot);
            }

            public bool IsReady()
            {
                return (ObjectManager.Player.SummonerSpellbook.CanUseSpell(SpellSlot) == SpellState.Ready);
            }

            public void CastSpell()
            {
                ObjectManager.Player.SummonerSpellbook.CastSpell(SpellSlot);
            }

            public void CastSpell(Obj_AI_Base target)
            {
                ObjectManager.Player.SummonerSpellbook.CastSpell(SpellSlot, target);
            }

            public void CastSpell(Vector3 position)
            {
                ObjectManager.Player.SummonerSpellbook.CastSpell(SpellSlot, position);
            }

            public void AddtoMenusub(Menu menu)
            {
                switch (Summoner)
                {
                    case Summoner.Barrier:
                        LoadBarrierMenu(menu);
                        break;
                    case Summoner.Heal:
                        LoadHealMenu(menu);
                        break;
                    case Summoner.Ignite:
                        LoadIgniteMenu(menu);
                        break;                   
                    case Summoner.Exhaust:
                        LoadExhoustMenu(menu);
                        break;
                    case Summoner.Cleanse:
                        LoadCleanseMenu(menu);
                        break;
                }
            }

            private void LoadBarrierMenu(Menu menu)
            {
                menu.AddSubMenu(new Menu("Barrier", "act_barrier"));
                menu.SubMenu("act_barrier").AddItem(new MenuItem("act_barrier_sep0", "====== Conditions"));
                menu.SubMenu("act_barrier").AddItem(new MenuItem("act_barrier_use", "= Use Barrier").SetValue(true));
                menu.SubMenu("act_barrier").AddItem(new MenuItem("act_barrier_sep1", "=== Condition 1"));
                menu.SubMenu("act_barrier").AddItem(new MenuItem("act_barrier_Percent", "= At percent Health").SetValue(new Slider(20, 99, 1)));
                menu.SubMenu("act_barrier").AddItem(new MenuItem("act_barrier_ifEnemy1", "= just if Enemy near").SetValue(false));
                menu.SubMenu("act_barrier").AddItem(new MenuItem("act_barrier_sep2", "=== Condition 2"));
                menu.SubMenu("act_barrier").AddItem(new MenuItem("act_barrier_Health", "= At Health").SetValue(new Slider(50, 500, 1)));
                menu.SubMenu("act_barrier").AddItem(new MenuItem("act_barrier_ifEnemy2", "= just if Enemy near").SetValue(false));
                menu.SubMenu("act_barrier").AddItem(new MenuItem("act_barrier_sep3", "=== Condition 3"));
                menu.SubMenu("act_barrier").AddItem(new MenuItem("act_barrier_onSpell", "= On Danger Spell").SetValue(false));
                menu.SubMenu("act_barrier").AddItem(new MenuItem("act_barrier_sep4", "========="));
            }

            private void LoadHealMenu(Menu menu)
            {
                menu.AddSubMenu(new Menu("Heal", "act_heal"));
                menu.SubMenu("act_heal").AddItem(new MenuItem("act_heal_sep0", "====== Conditions"));
                menu.SubMenu("act_heal").AddItem(new MenuItem("act_heal_use", "= Use Heal").SetValue(true));
                menu.SubMenu("act_heal").AddItem(new MenuItem("act_heal_sep1", "=== Condition 1"));
                menu.SubMenu("act_heal").AddItem(new MenuItem("act_heal_Percent", "= At percent Health").SetValue(new Slider(20, 99, 1)));
                menu.SubMenu("act_heal").AddItem(new MenuItem("act_heal_ifEnemy1", "= just if Enemy near").SetValue(false));
                menu.SubMenu("act_heal").AddItem(new MenuItem("act_heal_sep2", "=== Condition 2"));
                menu.SubMenu("act_heal").AddItem(new MenuItem("act_heal_Health", "= At Health").SetValue(new Slider(50, 500, 1)));
                menu.SubMenu("act_heal").AddItem(new MenuItem("act_heal_ifEnemy2", "= just if Enemy near").SetValue(false));
                menu.SubMenu("act_heal").AddItem(new MenuItem("act_heal_sep3", "=== Condition 3"));
                menu.SubMenu("act_heal").AddItem(new MenuItem("act_heal_forFriend", "= Help Friend to").SetValue(false));
                menu.SubMenu("act_heal").AddItem(new MenuItem("act_heal_sep4", "========="));
            }

            private void LoadIgniteMenu(Menu menu)
            {
                menu.AddSubMenu(new Menu("Ignite", "act_ignite"));
                menu.SubMenu("act_ignite").AddItem(new MenuItem("act_ignite_sep0", "====== Conditions"));
                menu.SubMenu("act_ignite").AddItem(new MenuItem("act_ignite_useChamp", "= ComboKiller takes Care"));
                menu.SubMenu("act_ignite").AddItem(new MenuItem("act_ignite_ks", "= not Supportet Champ - KS").SetValue(true));
                menu.SubMenu("act_ignite").AddItem(new MenuItem("act_ignite_sep4", "========="));
            }  
            private void LoadExhoustMenu(Menu menu)
            {
                menu.AddSubMenu(new Menu("Exhoust", "act_exhoust"));
                menu.SubMenu("act_exhoust").AddItem(new MenuItem("act_exhoust_sep0", "====== Conditions"));
                menu.SubMenu("act_exhoust").AddItem(new MenuItem("act_exhoust_use", "= Use Exhoust").SetValue(true));
                menu.SubMenu("act_exhoust").AddItem(new MenuItem("act_exhoust_sep1", "=== Condition 1"));
                menu.SubMenu("act_exhoust").AddItem(new MenuItem("act_exhoust_killme", "= enemy kill me in Seconds").SetValue(new Slider(3, 6, 1)));
                menu.SubMenu("act_exhoust").AddItem(new MenuItem("act_exhoust_sep2", "=== Condition 2"));
                menu.SubMenu("act_exhoust").AddItem(new MenuItem("act_exhoust_killfriend", "=  enemy kill Friend in Seconds").SetValue(new Slider(3, 6, 1)));
                menu.SubMenu("act_exhoust").AddItem(new MenuItem("act_exhoust_sep3", "=== Override"));
                foreach (var enemy in Program.AllHerosEnemy)
                {
                    menu.SubMenu("act_exhoust").AddItem(new MenuItem("act_exhoust_enemy_" + enemy.ChampionName, "Use on " + enemy.ChampionName).SetValue(true));
                }
                menu.SubMenu("act_exhoust").AddItem(new MenuItem("act_exhoust_sep4", "========="));
            }

            private void LoadCleanseMenu(Menu menu)
            {
                menu.AddSubMenu(new Menu("Cleanse", "act_cleanse"));
                menu.SubMenu("act_cleanse").AddItem(new MenuItem("act_cleanse_sep0", "====== Conditions"));
                foreach (var buffname in BuffnamesCC)
                {
                    menu.SubMenu("act_cleanse").AddItem(new MenuItem("act_cleanse_" + buffname, "Anti " + buffname).SetValue(true));
                }
                menu.SubMenu("act_cleanse").AddItem(new MenuItem("act_cleanse_sep4", "========="));
            }
        }

        internal class ItemManager
        {
            public static List<Database.ActiveItem> Itemlist;

            internal static void AddSupMenu(Menu def, Menu off)
            {
                Itemlist = Database.GetActiveItemList();
                foreach (var item in Itemlist.Where(item => item.IsActive()))
                {
                    Menu tempmenu;
                    switch (item.TypeItem)
                    {
                        case Database.ActiveItem.ItemType.Cleanse:
                            tempmenu = def;
                            break;
                        case Database.ActiveItem.ItemType.Offensive:
                            tempmenu = off;
                            break;
                        default:
                            tempmenu = def;
                            break;
                    }
                    switch (item.TypeMenu)
                    {
                        case Database.ActiveItem.MenuType.Defensive:
                            AddDefensiveMenu(tempmenu, item);
                            break;
                        case Database.ActiveItem.MenuType.Offensive:
                            AddOffensiveMenu(tempmenu, item);
                            break;
                    }

                }
                Game.OnGameUpdate += OnGameUpdate;
            }

            private static void AddDefensiveMenu(Menu menu, Database.ActiveItem item)
            {
                menu.AddSubMenu(new Menu(item.Name, "act_item_" + item.Id));
                menu.SubMenu("act_item_" + item.Id).AddItem(new MenuItem("act_item_" + item.Id + "_sep0", "====== Conditions"));
                menu.SubMenu("act_item_" + item.Id).AddItem(new MenuItem("act_item_" + item.Id + "_use", "= Use").SetValue(true));
                menu.SubMenu("act_item_" + item.Id).AddItem(new MenuItem("act_item_" + item.Id + "_sep1", "========="));
            }

            private static void AddOffensiveMenu(Menu menu, Database.ActiveItem item)
            {
                menu.AddSubMenu(new Menu(item.Name, "act_item_" + item.Id));
                menu.SubMenu("act_item_" + item.Id).AddItem(new MenuItem("act_item_" + item.Id + "_sep0", "====== Conditions"));
                menu.SubMenu("act_item_" + item.Id).AddItem(new MenuItem("act_item_" + item.Id + "_useCombo", "= Use in Combo").SetValue(true));
                menu.SubMenu("act_item_" + item.Id).AddItem(new MenuItem("act_item_" + item.Id + "_useHarass", "= Use in Harass").SetValue(false));
                menu.SubMenu("act_item_" + item.Id).AddItem(new MenuItem("act_item_" + item.Id + "_sep1", "========="));
            }

            private static void AddDefensiveSupportedMenu(Menu menu, Database.ActiveItem item)
            {
                menu.AddSubMenu(new Menu(item.Name, "act_item_" + item.Id));
                menu.SubMenu("act_item_" + item.Id).AddItem(new MenuItem("act_item_" + item.Id + "_sep0", "====== Conditions"));
                menu.SubMenu("act_item_" + item.Id).AddItem(new MenuItem("act_item_" + item.Id + "_sep1", "= ComboKiller takes Care"));
                menu.SubMenu("act_item_" + item.Id).AddItem(new MenuItem("act_item_" + item.Id + "_use", "= not Supportet Champ - Use").SetValue(true));
                menu.SubMenu("act_item_" + item.Id).AddItem(new MenuItem("act_item_" + item.Id + "_sep2", "========="));
            }

            private static void OnGameUpdate(EventArgs args)
            {
                foreach (var item in Itemlist.Where(item => item.IsActive() && item.IsReady()))
                {
                    CheckUseage(item);
                }
            }

            private static void CheckUseage(Database.ActiveItem item)
            {
                if (!item.IsReady() || !item.IsEnabled())
                    return;
                switch (item.TypeItem)
                {
                    case Database.ActiveItem.ItemType.Buff:

                        break;
                    case Database.ActiveItem.ItemType.Cleanse:

                        // Dervish Blade, Mercurial Scimitar, Quicksilver Sash
                        if (item.Id == 3137 || item.Id == 3139 || item.Id == 3140)
                            if (BuffnamesCC.Any(bufftype => ObjectManager.Player.HasBuffOfType(bufftype) && Program.Menu.Item("act_debuff_" + bufftype).GetValue<bool>() && CleanseIsDown()))
                                item.CastItem();

                        // Mikael's Crucible
                        if (item.Id == 3222)
                        {
                            var friend = Program.AllHerosFriend.FirstOrDefault(hero => !hero.IsDead && hero.Distance(ObjectManager.Player) < 750 && (BuffnamesCC.Any(hero.HasBuffOfType)));
                            if (friend == null)
                                return;
                            item.CastItem(friend);
                        }
                        break;
                    case Database.ActiveItem.ItemType.Offensive:
                        // Muramana
                        if (item.Id == 3042 || item.Id == 3043)
                        {
                            var muramanaActive = false;
                            var muramanaNeeded = false;
                            if (ObjectManager.Player.Buffs.Any(buff => ObjectManager.Player.HasBuff(item.Name)))
                                muramanaActive = true;
                            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo &&
                                Program.Menu.Item("act_item_" + item.Id + "_useCombo").GetValue<bool>() ||
                                LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Harass &&
                                Program.Menu.Item("act_item_" + item.Id + "_useHarass").GetValue<bool>())
                                if (Utility.CountEnemysInRange((int)ObjectManager.Player.AttackRange + 100) >= 1)
                                    muramanaNeeded = true;
                            if ((muramanaNeeded && !muramanaActive) || (!muramanaNeeded && muramanaActive))
                                item.CastItem();
                        }
                        // Youmuu's Ghostblade
                        if (item.Id == 3142)
                        {
                            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo &&
                                Program.Menu.Item("act_item_" + item.Id + "_useCombo").GetValue<bool>() ||
                                LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Harass &&
                                Program.Menu.Item("act_item_" + item.Id + "_useHarass").GetValue<bool>())
                            {
                                int attackrange = (int)LXOrbwalker.GetAutoAttackRange(ObjectManager.Player);
                                if (Utility.CountEnemysInRange(attackrange) >= 1)
                                    item.CastItem();
                            }
                        }
                        // List below
                        if (item.Id == 3077 || item.Id == 3074 || item.Id == 3146 || item.Id == 3144 || item.Id == 3153 || item.Id == 3128)
                        {
                            var range = 0;
                            var onenemy = true;
                            switch (item.Id)
                            {
                                case 3074:
                                case 3077:
                                    range = 400;
                                    onenemy = false;
                                    break;
                                case 3146:
                                    range = 700;
                                    break;
                                case 3153:
                                case 3144:
                                    range = 450;
                                    break;
                                case 3128:
                                    range = 750;
                                    break;
                            }

                            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo &&
                                Program.Menu.Item("act_item_" + item.Id + "_useCombo").GetValue<bool>() ||
                                LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Harass &&
                                Program.Menu.Item("act_item_" + item.Id + "_useHarass").GetValue<bool>())
                            {
                                if (SimpleTs.GetTarget(range, SimpleTs.DamageType.True) != null)
                                    if (onenemy)
                                        item.CastItem(SimpleTs.GetTarget(range, SimpleTs.DamageType.True));
                                    else
                                        item.CastItem();
                            }
                        }
                        break;
                }
            }

            private static bool CleanseIsDown()
            {
                var spell = ObjectManager.Player.SummonerSpellbook.Spells.FirstOrDefault(x => x.Name.ToLower() == "summonerboost");
                var spellSlot = spell != null ? spell.Slot : SpellSlot.Unknown;
                if (spellSlot == SpellSlot.Unknown)
                    return true;
                return ObjectManager.Player.SummonerSpellbook.CanUseSpell(spellSlot) != SpellState.Ready;
            }
        }

        internal class PotionManager
        {
            // Based on AutoPotions from Nikita Bernthaler
            private enum PotionType
            {
                Health,
                Mana
            };

            private class Potion
            {
                public string Name
                {
                    get;
                    set;
                }
                public int MinCharges
                {
                    get;
                    set;
                }
                public ItemId ItemId
                {
                    get;
                    set;
                }
                public int Priority
                {
                    get;
                    set;
                }
                public List<PotionType> TypeList
                {
                    get;
                    set;
                }
            }

            private static List<Potion> _potions;

            public static void AddtoMenusub(Menu menu)
            {
                _potions = new List<Potion>
				{
					new Potion
					{
						Name = "ItemCrystalFlask",
						MinCharges = 1,
						ItemId = (ItemId) 2041,
						Priority = 1,
						TypeList = new List<PotionType> {PotionType.Health, PotionType.Mana}
					},
					new Potion
					{
						Name = "RegenerationPotion",
						MinCharges = 0,
						ItemId = (ItemId) 2003,
						Priority = 2,
						TypeList = new List<PotionType> {PotionType.Health}
					},
					new Potion
					{
						Name = "ItemMiniRegenPotion",
						MinCharges = 0,
						ItemId = (ItemId) 2010,
						Priority = 4,
						TypeList = new List<PotionType> {PotionType.Health, PotionType.Mana}
					},
					new Potion
					{
						Name = "FlaskOfCrystalWater",
						MinCharges = 0,
						ItemId = (ItemId) 2004,
						Priority = 3,
						TypeList = new List<PotionType> {PotionType.Mana}
					}
				};

                _potions = _potions.OrderBy(x => x.Priority).ToList();

                var tempMenu = menu;
                tempMenu.AddItem(new MenuItem("act_potionmanager_sep0", "====== Health"));
                tempMenu.AddItem(new MenuItem("act_potionmanager_HealthPotion", "= Use Health Potion").SetValue(true));
                tempMenu.AddItem(new MenuItem("act_potionmanager_HealthPercent", "= HP Trigger Percent").SetValue(new Slider(30)));
                tempMenu.AddItem(new MenuItem("act_potionmanager_sep1", "====== Mana"));
                tempMenu.AddItem(new MenuItem("act_potionmanager_ManaPotion", "Use Mana Potion").SetValue(true));
                tempMenu.AddItem(new MenuItem("act_potionmanager_ManaPercent", "MP Trigger Percent").SetValue(new Slider(30)));
                tempMenu.AddItem(new MenuItem("act_potionmanager_sep2", "========="));


                Game.OnGameUpdate += OnGameUpdate;
            }

            private static void OnGameUpdate(EventArgs args)
            {
                try
                {
                    if (Program.Menu.Item("act_potionmanager_HealthPotion").GetValue<bool>())
                    {
                        if (GetPlayerHealthPercentage() <= Program.Menu.Item("act_potionmanager_HealthPercent").GetValue<Slider>().Value)
                        {
                            InventorySlot healthSlot = GetPotionSlot(PotionType.Health);
                            if (!IsBuffActive(PotionType.Health))
                                healthSlot.UseItem();
                        }
                    }
                    if (Program.Menu.Item("act_potionmanager_ManaPotion").GetValue<bool>())
                    {
                        if (GetPlayerManaPercentage() <= Program.Menu.Item("act_potionmanager_ManaPercent").GetValue<Slider>().Value)
                        {
                            InventorySlot manaSlot = GetPotionSlot(PotionType.Mana);
                            if (!IsBuffActive(PotionType.Mana))
                                manaSlot.UseItem();
                        }
                    }
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {

                }
            }

            private static InventorySlot GetPotionSlot(PotionType type)
            {
                return (from potion in _potions
                        where potion.TypeList.Contains(type)
                        from item in ObjectManager.Player.InventoryItems
                        where item.Id == potion.ItemId && item.Charges >= potion.MinCharges
                        select item).FirstOrDefault();
            }

            private static bool IsBuffActive(PotionType type)
            {
                return (from potion in _potions
                        where potion.TypeList.Contains(type)
                        from buff in ObjectManager.Player.Buffs
                        where buff.Name == potion.Name && buff.IsActive
                        select potion).Any();
            }

            internal static float GetPlayerHealthPercentage()
            {
                return ObjectManager.Player.Health * 100 / ObjectManager.Player.MaxHealth;
            }

            internal static float GetPlayerManaPercentage()
            {
                return ObjectManager.Player.Mana * 100 / ObjectManager.Player.MaxMana;
            }
        }
    }
}
