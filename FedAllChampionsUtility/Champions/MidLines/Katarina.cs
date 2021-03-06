﻿#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using BuffLib;
#endregion

namespace FedAllChampionsUtility
{
    internal class QMark
    {
        public string unit { get; private set; }

        public float endtime { get; private set; }

        public QMark(string Unit, float EndTime)
        {
            unit = Unit;
            endtime = EndTime;
        }
    }
    class Katarina : Champion
    {
        public static List<Spell> SpellList = new List<Spell>();
        public static List<QMark> MarkList = new List<QMark>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot IgniteSlot;        
        
        public Katarina()
        {
            LoadSpells();
            LoadMenu();

            BuffMngr.BuffMngrInit();
            BuffMngr.OnGainBuff += BuffGained;
            BuffMngr.OnLoseBuff += BuffLost;

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnGameSendPacket += GameOnOnGameSendPacket;

            PluginLoaded();
        }

        private void LoadMenu()
        {
            Program.Menu.AddSubMenu(new Menu("HotKeys:", "hotkeys"));
            Program.Menu.SubMenu("hotkeys").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            Program.Menu.SubMenu("hotkeys").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Harass Options:", "harassspells"));
            Program.Menu.SubMenu("harassspells").AddItem(new MenuItem("useQHarass", "Use - Bouncing Blades (Q)").SetValue(true));
            Program.Menu.SubMenu("harassspells").AddItem(new MenuItem("useWHarass", "Use - Sinister Steel (W)").SetValue(true));
            Program.Menu.SubMenu("harassspells").AddItem(new MenuItem("useEHarass", "Use - Shunpo (E)").SetValue(true));

            Program.Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after Combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit += hero => (float)CalculateDamageDrawing(hero);

            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };
            Program.Menu.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("ignite", "Use Ignite")).SetValue(true);
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("dfg", "Use DFG")).SetValue(true);
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("wDelay", "Delay W to proc mark")).SetValue(false);
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("Ward", "Ward Jump!").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 675);
            W = new Spell(SpellSlot.W, 400);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 550);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private static void BuffGained(Obj_AI_Base target, Obj_AI_Base source, BuffMngr.OnGainBuffArgs args)
        {
            if (target.IsMe && args.BuffID == 3334932)
            {
                tSpells.rEndTick = args.EndTime;
                tSpells.rStartTick = args.StartTime;
                tSpells.ulting = true;
            }
            else if (args.BuffID == 84848667) //mark
            {
                MarkList.Add(new QMark(target.BaseSkinName, args.EndTime));
            }
        }

        private static void BuffLost(Obj_AI_Base target, Obj_AI_Base source, BuffMngr.OnGainBuffArgs args)
        {
            if (target.IsMe && args.BuffID == 3334932)
            {
                tSpells.ulting = false;
            }
            else if (args.BuffID == 84848667) // mark
            {
                foreach (var mark in MarkList)
                {
                    if (mark.unit == target.BaseSkinName)
                    {
                        MarkList.Remove(mark);
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Draw the ranges of the spells.
            foreach (var spell in SpellList)
            {
                var menuItem = Program.Menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
                }
            }
        }
        public static class tSpells
        {
            public static float rEndTick;
            public static float rStartTick;
            public static bool ulting;
            public static float wLastUse;
            public static float qlastuse;
            public static bool usedfg;
            public static bool useignite;

        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            };
            if (Program.Menu.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                Harass(Program.Menu.Item("useQHarass").GetValue<bool>(), Program.Menu.Item("useWHarass").GetValue<bool>(), Program.Menu.Item("useEHarass").GetValue<bool>());
            };

            if (Program.Menu.Item("Ward").GetValue<KeyBind>().Active)
            {
                wJumper.wardJump(Game.CursorPos.To2D());
            }

            if (Program.Menu.Item("StackE").GetValue<KeyBind>().Active)
            {
                if (ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                {
                }
            }

        }

        private static void Combo()
        {
            var qtarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var wtarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var etarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            var rtarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            var curtarget = SimpleTs.GetTarget(400, SimpleTs.DamageType.Magical); ;
            if (E.IsReady())
                curtarget = etarget;
            else if (Q.IsReady())
                curtarget = qtarget;
            else if (W.IsReady())
                curtarget = wtarget;
            else if (R.IsReady())
                curtarget = rtarget;
            //Console.WriteLine(CalculateDamage(curtarget).ToString());

            if (((CalculateDamage(curtarget) > curtarget.Health || rtarget == null) && tSpells.ulting == true) || tSpells.ulting == false)
            {
                DoCombo(curtarget);
            }
        }


        private static void GameOnOnGameSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.C2S.Move.Header && Environment.TickCount < tSpells.rStartTick + 300)
            {
                args.Process = false;
                Console.WriteLine("BLOCK PACKET");
            }
        }

        private static void DoCombo(Obj_AI_Base target)
        {
            if (Program.Menu.Item("dfg").GetValue<bool>() && tSpells.usedfg && Items.HasItem(3128) && Items.CanUseItem(3128))
            {
                Items.UseItem(3128, target);
            }
            if (Q.IsReady() && ObjectManager.Player.Distance(target) < Q.Range)
            {
                Q.Cast(target, false);
                tSpells.qlastuse = Environment.TickCount;
            }
            if (E.IsReady() && ObjectManager.Player.Distance(target) < E.Range)
            {
                E.Cast(target);
            }
            if (W.IsReady() && !Q.IsReady() && ObjectManager.Player.Distance(target) < W.Range && Environment.TickCount > tSpells.wLastUse + 250 && (!Program.Menu.Item("wDelay").GetValue<bool>() || checkformark(target) || Environment.TickCount > tSpells.qlastuse + 100 || R.IsReady()))
            {
                W.Cast();
                tSpells.wLastUse = Environment.TickCount;
                //Console.WriteLine("CAST W");
            }
            if (R.IsReady() && !W.IsReady() && ObjectManager.Player.Distance(target) < R.Range && !tSpells.ulting && Environment.TickCount > tSpells.rStartTick + 300)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.HoldPosition, new Vector3(ObjectManager.Player.ServerPosition.X, ObjectManager.Player.ServerPosition.Y, ObjectManager.Player.ServerPosition.Z));
                R.Cast();
                tSpells.rStartTick = Environment.TickCount;
                Console.WriteLine("CAST ULT");
            }
            if (Program.Menu.Item("ignite").GetValue<bool>() && tSpells.useignite)
            {
                ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
            }
        }

        private static bool checkformark(Obj_AI_Base target)
        {
            foreach (QMark mark in MarkList)
            {
                if (mark.unit == target.BaseSkinName)
                {
                    Console.WriteLine(mark.unit + " : marked");
                    return true;

                }
            }
            //Console.WriteLine(target.BaseSkinName + " : not marked");
            return false;
        }

        public static double CalculateDamage(Obj_AI_Base target)
        {
            double totaldamage = 0;
            bool marked = checkformark(target);
            tSpells.useignite = false;
            tSpells.usedfg = false;
            if ((ObjectManager.Player.Distance(target) < Q.Range || ObjectManager.Player.Distance(target) < E.Range && E.IsReady()) && Q.IsReady() && (W.IsReady() || E.IsReady() || R.IsReady()))
            {
                totaldamage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
            }
            if ((ObjectManager.Player.Distance(target) < W.Range || ObjectManager.Player.Distance(target) < E.Range && E.IsReady()) && W.IsReady())
            {
                totaldamage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);
            }
            if (ObjectManager.Player.Distance(target) < E.Range && E.IsReady())
            {
                totaldamage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.E);
            }
            if ((ObjectManager.Player.Distance(target) < R.Range || ObjectManager.Player.Distance(target) < E.Range && E.IsReady()) && R.IsReady())
            {
                totaldamage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);
            }
            if (!Q.IsReady() && marked)
            {
                totaldamage += ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level * 15) + (0.15 * ObjectManager.Player.FlatMagicDamageMod));
            }

            if (totaldamage > target.Health)
            {
                return totaldamage;
            }

            if (Program.Menu.Item("dfg").GetValue<bool>() && Items.HasItem(3128) && Items.CanUseItem(3128))
            {
                totaldamage += (totaldamage * 1.2) + ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, target.MaxHealth * 0.15);
            }

            if (totaldamage > target.Health)
            {
                tSpells.usedfg = true;
                return totaldamage;
            }

            if (Program.Menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && ObjectManager.Player.Distance(target) < 600)
            {

                if (totaldamage + ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
                {
                    tSpells.useignite = true;
                    totaldamage += ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
                }
            }
            tSpells.usedfg = true;

            return totaldamage;
        }


        public static double CalculateDamageDrawing(Obj_AI_Base target)
        {
            double totaldamage = 0;
            bool marked = checkformark(target);
            if (Q.IsReady() && (W.IsReady() || E.IsReady() || R.IsReady()))
            {
                totaldamage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
            }
            if (E.IsReady() && W.IsReady())
            {
                totaldamage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);
            }
            if (E.IsReady())
            {
                totaldamage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.E);
            }
            if (R.IsReady())
            {
                totaldamage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);
            }
            if (!Q.IsReady() && marked)
            {
                totaldamage += ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level * 15) + (0.15 * ObjectManager.Player.FlatMagicDamageMod));
            }

            if (Program.Menu.Item("dfg").GetValue<bool>() && Items.HasItem(3128) && Items.CanUseItem(3128))
            {
                totaldamage += (totaldamage * 1.2) + ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, target.MaxHealth * 0.15);
            }

            if (Program.Menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                totaldamage += ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            }
            return totaldamage;
        }
        private static void Harass(bool useQ, bool useW, bool useE)
        {
            var qtarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var wtarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var etarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (qtarget != null && useQ && Q.IsReady())
            {
                Q.Cast(qtarget, false);
                tSpells.qlastuse = Environment.TickCount;
            }
            else if (wtarget != null && useW && W.IsReady() && (!Program.Menu.Item("wDelay").GetValue<bool>() || checkformark(wtarget)))
            {
                W.Cast();
                tSpells.wLastUse = Environment.TickCount;
            }
            else if (etarget != null && useE && E.IsReady())
                E.Cast(etarget, false);
        }

        private static void OnProcessSpell(LeagueSharp.Obj_AI_Base obj, LeagueSharp.GameObjectProcessSpellCastEventArgs arg)
        {
            if (wJumper.testSpells.ToList().Contains(arg.SData.Name))
            {
                wJumper.testSpellCast = arg.End.To2D();
                Polygon pol;
                if ((pol = Program.map.getInWhichPolygon(arg.End.To2D())) != null)
                {
                    wJumper.testSpellProj = pol.getProjOnPolygon(arg.End.To2D());
                }
            }
        }
    }
}
