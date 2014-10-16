using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace FedAllChampionsUtility
{
    class JarvanIV : Champion
    {
        public static Spell Q, W, E, R;
        public static List<Spell> SpellList = new List<Spell>();        

        public static SpellSlot IgniteSlot;
        public static bool ult;

        public JarvanIV()
        {
            LoadSpells();
            LoadMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnCreate += OnCreateObj;
            Obj_AI_Hero.OnDelete += OnDeleteObj;

            PluginLoaded();

        }

        private void LoadMenu()
        {
            //Combo
            Program.Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("MinEnemys", "Enemys R").SetValue(new Slider(2, 5, 1)));            
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass
            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("ActiveHarass", "Harass key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //Farm
            Program.Menu.AddSubMenu(new Menu("Lane Clear", "Lane"));
            Program.Menu.SubMenu("Lane").AddItem(new MenuItem("UseQLane", "Use Q")).SetValue(true);
            Program.Menu.SubMenu("Lane").AddItem(new MenuItem("ActiveLane", "Lane Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //Kill Steal
            Program.Menu.AddSubMenu(new Menu("KillSteal", "Ks"));
            Program.Menu.SubMenu("Ks").AddItem(new MenuItem("ActiveKs", "Use KillSteal")).SetValue(true);
            Program.Menu.SubMenu("Ks").AddItem(new MenuItem("UseQKs", "Use Q")).SetValue(true);
            Program.Menu.SubMenu("Ks").AddItem(new MenuItem("UseEKs", "Use E")).SetValue(true);
            Program.Menu.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);

            //Drawings
            Program.Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 700f);
            W = new Spell(SpellSlot.W, 300f);
            E = new Spell(SpellSlot.E, 830f);
            R = new Spell(SpellSlot.R, 650f);

            Q.SetSkillshot(0.5f, 70, float.MaxValue, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 75, float.MaxValue, false, SkillshotType.SkillshotCircle);
            //R.SetSkillshot(0.5f, 325, 0, false, SkillshotType.SkillshotCircle);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            Program.Orbwalker.SetAttack(true);
            
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);

            if (Program.Menu.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo(target);
            }
            if (Program.Menu.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
                Harass(target);
            }
            if (Program.Menu.Item("ActiveKs").GetValue<bool>())
            {
                KillSteal(target);
            }
            if (Program.Menu.Item("ActiveLane").GetValue<KeyBind>().Active)
            {
                Farm();
            }

        }

        private static void Combo(Obj_AI_Hero target)
        {
            if (target != null)
            {
                if (Program.Menu.Item("UseECombo").GetValue<bool>() && E.IsReady() && ObjectManager.Player.Distance(target) <= Q.Range)
                {
                    E.Cast(target, true);
                }
                if (Program.Menu.Item("UseQCombo").GetValue<bool>() && Q.IsReady())
                {
                    Q.Cast(target, true);
                }
                if (Program.Menu.Item("UseWCombo").GetValue<bool>() && W.IsReady())
                {
                    W.Cast(target);
                }

                if (Program.Menu.Item("UseRCombo").GetValue<bool>() && R.IsReady() && !ult)
                {
                    if (GetEnemys(target) >= Program.Menu.Item("MinEnemys").GetValue<Slider>().Value)
                    {
                        R.Cast(target);
                    }
                }                
            }
        }

        private static void Harass(Obj_AI_Hero target)
        {
            if (target != null)
            {
                if (Program.Menu.Item("UseQHarass").GetValue<bool>() && Q.IsReady())
                {
                    Q.Cast(target);
                }
            }

        }

        private static void KillSteal(Obj_AI_Hero target)
        {            
            var Qdmg = ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
            var Edmg = ObjectManager.Player.GetSpellDamage(target, SpellSlot.E);
            var igniteDmg = ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);            

            if (target != null)
            {
                if (Program.Menu.Item("UseQKs").GetValue<bool>() && Q.IsReady())
                {
                    if (target.Health <= Qdmg)
                        Q.Cast(target);

                }

                if (Program.Menu.Item("UseEKs").GetValue<bool>() && E.IsReady())
                {
                    if (target.Health <= Edmg)
                        E.Cast(target);
                }
                if (Program.Menu.Item("UseIgnite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {

                    if (target.Health < igniteDmg)
                    {
                       ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                    }
                }
            }
        }

        private static int GetEnemys(Obj_AI_Hero target)
        {
            int Enemys = 0;
            foreach (Obj_AI_Hero enemys in ObjectManager.Get<Obj_AI_Hero>())
            {

                var pred = R.GetPrediction(enemys, true);
                if (pred.Hitchance >= HitChance.High && !enemys.IsMe && enemys.IsEnemy && Vector3.Distance(ObjectManager.Player.Position, pred.UnitPosition) <= R.Range)
                {
                    Enemys = Enemys + 1;
                }
            }
            return Enemys;
        }
        private static void Farm()
        {
            var Minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);

            foreach (var minion in Minions)
            {
                if (Program.Menu.Item("UseQLane").GetValue<bool>())
                {
                    if (Q.IsReady() && ObjectManager.Player.Distance(minion) <= Q.Range)
                    {
                        Q.Cast(minion);
                    }
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Program.Menu.Item("Draw_Disabled").GetValue<bool>()) return;

            if (Program.Menu.Item("CircleLag").GetValue<bool>())
            {
                if (Program.Menu.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White,
                        Program.Menu.Item("CircleThickness").GetValue<Slider>().Value,
                        Program.Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Program.Menu.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.White,
                        Program.Menu.Item("CircleThickness").GetValue<Slider>().Value,
                        Program.Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Program.Menu.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White,
                        Program.Menu.Item("CircleThickness").GetValue<Slider>().Value,
                        Program.Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Program.Menu.Item("DrawR").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.White,
                        Program.Menu.Item("CircleThickness").GetValue<Slider>().Value,
                        Program.Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (Program.Menu.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White);
                }
                if (Program.Menu.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.White);
                }
                if (Program.Menu.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White);
                }
                if (Program.Menu.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.White);
                }

            }
        }

        private static void OnCreateObj(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_GeneralParticleEmmiter)) return;
            var obj = (Obj_GeneralParticleEmmiter)sender;
            if (obj != null && obj.IsMe && obj.Name == "JarvanCataclysm_tar")
            {
                ult = true;
            }

        }

        private static void OnDeleteObj(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_GeneralParticleEmmiter)) return;
            var obj = (Obj_GeneralParticleEmmiter)sender;
            if (obj != null && obj.IsMe && obj.Name == "JarvanCataclysm_tar")
            {
                ult = false;
            }

        }
    }
}
