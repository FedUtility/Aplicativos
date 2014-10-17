#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LX_Orbwalker;

#endregion

namespace FedAllChampionsUtility
{
    class Ashe : Champion
    {
        private static readonly Obj_AI_Hero player = ObjectManager.Player;        
        private static Spell Q, W, R;
        private static bool hasQ = false; 
        private const double WAngle = 57.5 * Math.PI / 180;

        public Ashe()
        {
            LoadMenu();
            LoadSpells();

            Game.OnGameSendPacket += OnSendPacket;
            Game.OnGameUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;

            PluginLoaded();            
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1150);
            R = new Spell(SpellSlot.R);

            W.SetSkillshot(0.25f, 100, 1600f, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.3f, 250f, 1600f, false, SkillshotType.SkillshotLine);
        }

        private void LoadMenu()
        {
            Program.Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            
            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));            
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));

            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("interrupt", "Interrupt spells").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("antigapcloser", "Anti-Gapscloser").SetValue(true));           
        }        

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Program.Menu.Item("antigapcloser").GetValue<bool>() && Vector3.Distance(gapcloser.Sender.Position, player.Position) < 1000)
            {
                if (R.IsReady())
                {
                    R.Cast(gapcloser.End, Packets());
                }
                else
                {
                    W.Cast(gapcloser.End, Packets());
                }
            }
        }       
        private void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (R.IsReady() &&
                Vector3.Distance(player.Position, unit.Position) < 1000 &&
                Program.Menu.Item("interrupt").GetValue<bool>() &&
                spell.DangerLevel >= InterruptableDangerLevel.Medium)
            {
                R.Cast(unit.Position, Packets());
            }
        }
        private void Game_OnGameUpdate(EventArgs args)
        {
            // Combo
            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo)
                Combo();

            // Harass
            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Harass)
                Harass();
        }
        private void Combo()
        {
            bool useW = W.IsReady() && Program.Menu.Item("UseWCombo").GetValue<bool>();
            bool useR = R.IsReady() && Program.Menu.Item("UseR").GetValue<bool>();
            Obj_AI_Hero targetW = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Physical);
            Obj_AI_Hero targetR = SimpleTs.GetTarget(700, SimpleTs.DamageType.Magical);
            if (useW)
            {
                PredictionOutput wPred = W.GetPrediction(targetW);
                if (wPred.Hitchance >= HitChance.High)
                    W.Cast(wPred.CastPosition, Packets());
            }
            if (useR)
            {
                PredictionOutput rPred = R.GetPrediction(targetR);
                if (rPred.Hitchance >= HitChance.High)
                    R.Cast(rPred.CastPosition, Packets());
            }
        }
        private void Harass()
        {
            bool useW = W.IsReady() && Program.Menu.Item("UseWHarass").GetValue<bool>();
            Obj_AI_Hero targetW = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Physical);
            if (useW)
            {
                PredictionOutput wPred = W.GetPrediction(targetW);
                if (wPred.Hitchance >= HitChance.High)
                    W.Cast(wPred.CastPosition, Packets());
            }
        }
        private void OnSendPacket(GamePacketEventArgs args)
        {
            if (!Program.Menu.Item("UseQ").GetValue<bool>()) return;
            if (args.PacketData[0] == Packet.C2S.Move.Header && Packet.C2S.Move.Decoded(args.PacketData).SourceNetworkId == player.NetworkId && Packet.C2S.Move.Decoded(args.PacketData).MoveType == 3)
            {
                bool heroFound;
                foreach (BuffInstance buff in player.Buffs)
                    if (buff.Name == "FrostShot") hasQ = true;
                heroFound = false;
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                    if (hero.NetworkId == Packet.C2S.Move.Decoded(args.PacketData).TargetNetworkId)
                        heroFound = true;
                if (heroFound)
                {
                    if (!hasQ) Q.Cast();
                    hasQ = true;
                }
                else
                {
                    if (hasQ) Q.Cast();
                    hasQ = false;
                }
            }
        }       

    }
}
