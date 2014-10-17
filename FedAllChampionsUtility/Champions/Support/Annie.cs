#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LX_Orbwalker;
using Color = System.Drawing.Color;

#endregion

namespace FedAllChampionsUtility
{
    class Annie : Champion
    {
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q, W, E, R, R1;        

        public static float DoingCombo = 0;        
        public static SpellSlot FlashSlot;        

        public Annie()
        {
            
            LoadMenu();
            LoadSpells();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += OnGameUpdate;
            GameObject.OnCreate += OnCreateObject;
            LXOrbwalker.BeforeAttack += OrbwalkingBeforeAttack;

            PluginLoaded();
        }

        public void LoadSpells()
        {            
            FlashSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");

            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W, 625f);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 600f);
            R1 = new Spell(SpellSlot.R, 900f);

            W.SetSkillshot(0.60f, 0.872664626f, float.MaxValue, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.20f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R1.SetSkillshot(0.25f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            // Add to spell list
            SpellList.AddRange(new[] { Q, W, E, R, R1 });
        }

        public void LoadMenu()
        {
            Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("comboItems", "Use Items")).SetValue(true);            
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("flashCombo", "Targets needed to Flash -> R").SetValue(new Slider(4, 5, 1)));
            
            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("qHarass", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("wHarass", "Use W").SetValue(false));            
            AddManaManager("Harass", 40);            

            Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("qFarm", "Use Q").SetValue(true));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("wFarm", "Use W").SetValue(true));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("saveqStun", "Don't Last Hit with Q while stun is up").SetValue(true));            

            Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R1", "Draw R + Flash").SetValue(true));

        }

        private int StunCount
        {
            get
            {
                foreach (var buff in
                    ObjectManager.Player.Buffs.Where(
                        buff => buff.Name == "pyromania" || buff.Name == "pyromania_particle"))
                {
                    switch (buff.Name)
                    {
                        case "pyromania":
                            return buff.Count;
                        case "pyromania_particle":
                            return 4;
                    }
                }

                return 0;
            }
        }

        private void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.IsAlly || !(sender is Obj_SpellMissile))
            {
                return;
            }

            var missile = (Obj_SpellMissile)sender;
            if (!(missile.SpellCaster is Obj_AI_Hero) || !(missile.Target.IsMe))
            {
                return;
            }

            if (E.IsReady())
            {
                E.Cast();
            }
            else if (!ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(missile.SpellCaster.NetworkId).IsMelee())
            {
                var ecd = (int)(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).CooldownExpires - Game.Time) *
                          1000;
                if ((int)Vector3.Distance(missile.Position, ObjectManager.Player.ServerPosition) /
                    ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(missile.SpellCaster.NetworkId)
                        .BasicAttack.MissileSpeed * 1000 > ecd)
                {
                    Utility.DelayAction.Add(ecd, () => E.Cast());
                }
            }
        }

        private void OnGameUpdate(EventArgs args)
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var flashRtarget = SimpleTs.GetTarget(900, SimpleTs.DamageType.Magical);

            switch (LXOrbwalker.CurrentMode)
            {
                case LXOrbwalker.Mode.Combo:
                    Combo(target, flashRtarget);
                    break;
                case LXOrbwalker.Mode.Harass:
                    Harass(target);
                    break;
                case LXOrbwalker.Mode.Lasthit:
                    Farm(false);
                    break;
                case LXOrbwalker.Mode.LaneClear:
                    Farm(true);
                    break;
            }
        }

        private void OrbwalkingBeforeAttack(LXOrbwalker.BeforeAttackEventArgs args)
        {
            args.Process = Environment.TickCount - DoingCombo > 500;
        }

        private void Harass(Obj_AI_Base target)
        {
            if (Program.Menu.Item("qHarass").GetValue<bool>() && Q.IsReady())
            {
                Q.CastOnUnit(target, Packets());
            }
            if (Program.Menu.Item("wHarass").GetValue<bool>() && W.IsReady())
            {
                W.CastOnUnit(target, Packets());
            }
        }

        private void Combo(Obj_AI_Base target, Obj_AI_Base flashRtarget)
        {
            Console.WriteLine("[" + Game.Time + "]Combo started");
            if ((target == null && flashRtarget == null) || Environment.TickCount - DoingCombo < 500 ||
                (!Q.IsReady() && !W.IsReady() && !R.IsReady()))
            {
                return;
            }

            Console.WriteLine("[" + Game.Time + "]Target acquired");
            if (Program.Menu.Item("comboItems").GetValue<bool>() && target != null)
            {
                Items.UseItem(3128, target);
            }

            switch (StunCount)
            {
                case 3:
                    Console.WriteLine("[" + Game.Time + "]Case 3");
                    if (Q.IsReady())
                    {
                        DoingCombo = Environment.TickCount;
                        Q.CastOnUnit(target, Packets());
                        Utility.DelayAction.Add(
                            (int)(ObjectManager.Player.Distance(target) / Q.Speed * 1000 - 100 - Game.Ping / 2.0),
                            () =>
                            {
                                if (R.IsReady() &&
                                    !(ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) * 0.6 > target.Health))
                                {
                                    R.Cast(target, false, true);
                                }
                            });
                    }
                    else if (W.IsReady())
                    {
                        DoingCombo = Environment.TickCount;
                    }

                    W.Cast(target, false, true);

                    Utility.DelayAction.Add(
                        650 - 100 - Game.Ping / 2, () =>
                        {
                            if (R.IsReady() && !(ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) * 0.6 > target.Health))
                            {
                                R.Cast(target, false, true);
                            }

                            DoingCombo = Environment.TickCount;
                        });

                    break;
                case 4:
                    Console.WriteLine("[" + Game.Time + "]Case 4");
                    if (ObjectManager.Player.SummonerSpellbook.CanUseSpell(FlashSlot) == SpellState.Ready && R.IsReady() &&
                        target == null)
                    {
                        var position = R1.GetPrediction(flashRtarget, true).CastPosition;

                        if (ObjectManager.Player.Distance(position) > 600 &&
                            GetEnemiesInRange(flashRtarget.ServerPosition, 250) >=
                            Program.Menu.Item("flashCombo").GetValue<Slider>().Value)
                        {
                            ObjectManager.Player.SummonerSpellbook.CastSpell(FlashSlot, position);
                        }

                        Items.UseItem(3128, flashRtarget);
                        R.Cast(flashRtarget, false, true);

                        if (W.IsReady())
                        {
                            W.Cast(flashRtarget, false, true);
                        }
                    }
                    else if (R.IsReady() && !(ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) * 0.6 > target.Health))
                    {
                        R.Cast(target, false, true);
                    }

                    if (W.IsReady())
                    {
                        W.Cast(target, false, true);
                    }

                    if (Q.IsReady())
                    {
                        Q.Cast(target, false, true);
                    }

                    break;
                default:
                    Console.WriteLine("[" + Game.Time + "]Case default");
                    if (Q.IsReady())
                    {
                        Q.CastOnUnit(target, Packets());
                    }

                    if (W.IsReady())
                    {
                        W.Cast(target, false, true);
                    }

                    break;
            }            
        }

        private void Farm(bool laneclear)
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var jungleMinions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral);
            minions.AddRange(jungleMinions);

            if (laneclear && Program.Menu.Item("wFarm").GetValue<bool>() && W.IsReady())
            {
                if (minions.Count > 0)
                {
                    W.Cast(W.GetLineFarmLocation(minions).Position.To3D());
                }
            }

            if (!Program.Menu.Item("qFarm").GetValue<bool>() || (Program.Menu.Item("saveqStun").GetValue<bool>() && StunCount == 4) ||
                !Q.IsReady())
            {
                return;
            }

            foreach (var minion in
                from minion in
                    minions.OrderByDescending(Minions => Minions.MaxHealth)
                        .Where(minion => minion.IsValidTarget(Q.Range))
                let predictedHealth = Q.GetHealthPrediction(minion)
                where predictedHealth < ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) * 0.9 && predictedHealth > 0
                select minion)
            {
                Q.CastOnUnit(minion, Packets());
            }
        }

        private int GetEnemiesInRange(Vector3 pos, float range)
        {
            //var Pos = pos;
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero.Team != ObjectManager.Player.Team)
                    .Count(hero => Vector3.Distance(pos, hero.ServerPosition) <= range);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Program.Menu.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Program.Menu.Item("Draw_Q").GetValue<bool>())
                if (Q.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("Draw_W").GetValue<bool>())
                if (W.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("Draw_E").GetValue<bool>())
                if (E.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("Draw_R").GetValue<bool>())
                if (R.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("Draw_R1").GetValue<bool>())
                if (R.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, R1.Range, R.IsReady() ? Color.Green : Color.Red);
                        
        }


    }
}
