#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LX_Orbwalker;
using Color = System.Drawing.Color;

#endregion


namespace FedAllChampionsUtility
{
    class Sona : Champion
    {
        public static Spell Q, W, E, R;
        public static List<Spell> SpellList = new List<Spell>();

        public static Geometrys.Rectangle rect;

        public Sona()
        {
            LoadMenu();
            LoadSpells();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            
            PluginLoaded();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 650f);
            W = new Spell(SpellSlot.W, 1000f);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, 1000f);

            R.SetSkillshot(0.5f, 125f, 2400f, false, SkillshotType.SkillshotLine);
        }

        private void LoadMenu()
        {
            Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("AutoR", "Auto Crescendo?").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("AutoRTower", "Under Tower?").SetValue(false));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("MinR", "Min to Crescendo!").SetValue<Slider>(new Slider(3, 1, 5)));

            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));

            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("AutoQ", "Auto Cast Q").SetValue(false));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("AutoW", "Auto Cast W").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("AutoHeal", "Auto W when below % hp").SetValue<Slider>(new Slider(60, 0, 100)));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("AutoE", "Auto E in Ally").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("MinE", "Ally in Range").SetValue<Slider>(new Slider(3, 1, 5)));

        }

        private void Game_OnGameUpdate(EventArgs args)
        {            

            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo)
            {
                Combo();
            }
            else
            {

                if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Harass)
                {
                    Harass();
                }
            }

            if (Program.Menu.Item("AutoR").GetValue<bool>())
            {
                AutoCrescendo();
            }

            if (LXOrbwalker.CurrentMode.ToString() != "Combo" && LXOrbwalker.CurrentMode.ToString() != "Mixed")
            {
                if (Program.Menu.Item("AutoQ").GetValue<bool>() && Q.IsReady())
                {
                    CastQ();
                }

                if (Program.Menu.Item("AutoW").GetValue<bool>() && W.IsReady())
                {
                    CastW();
                }
            }

            if (Program.Menu.Item("AutoE").GetValue<bool>() && E.IsReady())
            {
                CastEAlly();
            }
        }

        public void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (spell.DangerLevel == InterruptableDangerLevel.High)
            {
                R.Cast(unit);
            }
        }

        public void Combo()
        {
            if (Program.Menu.Item("UseQCombo").GetValue<bool>())
            {
                CastQ();
            }

            if (Program.Menu.Item("UseWCombo").GetValue<bool>())
            {
                CastW();
            }

            if (Program.Menu.Item("UseECombo").GetValue<bool>())
            {
                CastE();
            }
        }

        public void Harass()
        {
            if (Program.Menu.Item("UseQHarass").GetValue<bool>())
            {
                CastQ();
            }

            if (Program.Menu.Item("UseWHarass").GetValue<bool>())
            {
                CastW();
            }            
        }

        public void CastQ()
        {
            if (Q.IsReady())
            {
                var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
                if (Vector3.Distance(ObjectManager.Player.Position, qTarget.Position) < Q.Range)
                {
                    Q.Cast();
                }
            }
        }

        public void CastW()
        {
            if (W.IsReady())
            {
                if (AllyBelowHP(Program.Menu.Item("AutoHeal").GetValue<Slider>().Value, W.Range))
                {
                    W.Cast();
                }
            }
        }

        public void CastE()
        {
            if (E.IsReady())
            {
                E.Cast();
            }
        }

        public void CastEAlly()
        {
            if (E.IsReady())
            {
                var minHitE = Program.Menu.Item("MinR").GetValue<Slider>().Value;
                var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
                var alliesarround = 0;
                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (ally.IsAlly && !ally.IsMe && ally.IsValidTarget(float.MaxValue, false) &&
                        ally.Distance(eTarget) <= 950)
                    {
                        alliesarround++;
                    }
                }

                if (alliesarround >= minHitE)
                    E.Cast();
            }
        }              

        private void AutoCrescendo()
        {
            var minHit = GetEnemyHitByR(R, Program.Menu.Item("MinR").GetValue<Slider>().Value);
            var underT = Program.Menu.Item("AutoRTower").GetValue<bool>();            

            if (minHit != null)
            {
                if (underT && (Utility.UnderTurret(minHit) || !Utility.UnderTurret(minHit)))
                {
                    R.Cast(minHit, true);
                }
                else if (!underT && !Utility.UnderTurret(minHit))
                {
                    R.Cast(minHit, true);
                }
                else
                {
                    return;
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {           
            
                if (R.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

            
        }

        public static bool AllyBelowHP(int percentHP, float range)
        {
            
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (ally.IsMe)
                {
                    if (((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) * 100) < percentHP)
                    {
                        return true;
                    }
                }
                else if (ally.IsAlly)
                {
                    if (Vector3.Distance(ObjectManager.Player.Position, ally.Position) < range && ((ally.Health / ally.MaxHealth) * 100) < percentHP)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public static Obj_AI_Hero GetEnemyHitByR(Spell R, int numHit)
        {
            int totalHit = 0;
            Obj_AI_Hero target = null;

            foreach (Obj_AI_Hero current in ObjectManager.Get<Obj_AI_Hero>())
            {

                var prediction = R.GetPrediction(current, true);

                if (Vector3.Distance(ObjectManager.Player.Position, prediction.CastPosition) <= R.Range - 50)
                {

                    Vector2 extended = current.Position.To2D().Extend(ObjectManager.Player.Position.To2D(), -R.Range + Vector2.Distance(ObjectManager.Player.Position.To2D(), current.Position.To2D()));
                    rect = new Geometrys.Rectangle(ObjectManager.Player.Position.To2D(), extended, R.Width);

                    if (!current.IsMe && current.IsEnemy)
                    {
                        // SEt to 1 as the current target is hittable.
                        totalHit = 1;
                        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
                        {
                            if (enemy.IsEnemy && current.ChampionName != enemy.ChampionName && !enemy.IsDead && !rect.ToPolygon().IsOutside(enemy.Position.To2D()))
                            {
                                totalHit += 1;
                            }
                        }
                    }

                    if (totalHit >= numHit)
                    {
                        target = current;
                        break;
                    }
                }

            }

            //Console.WriteLine(Game.Time + " | Targets hit is: " + totalHit + " Out of " + numHit);
            return target;
        }        

    }
}
