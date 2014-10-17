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
    class Orianna : Champion
    {
        public static Spell Q, W, E, R;

        public BallControl Ball = new BallControl();
        public Vector3 CastRPosition;
        public int RInterruptTry = 0;        

        public Orianna()
        {
            LoadSpells();
            LoadMenu();

            Game.OnGameUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;            
            Game.OnGameSendPacket += Game_OnSendPacket;

            PluginLoaded();
        }

        private void LoadMenu()
        {
            Program.Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("sep0", "====== Combo"));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("useQ_Combo", "= Use Q").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("useW_Combo", "= Use W").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("useR_Combo", "= Use R KillSecure").SetValue(false));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("sep1", "========="));

            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("sep0", "====== Harass"));
            AddManaManager("Harass", 40);
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "= Use Q").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass", "= Use W").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useHarass_Auto", "= AutoHarras").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("sep1", "========="));

            Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("sep0", "====== LaneClear"));
            AddManaManager("LaneClear", 20);
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear", "= Use Q").SetValue(true));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useW_LaneClear", "= Use W").SetValue(true));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("sep1", "=========")); 

            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("sep0", "====== Misc"));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("useR_Interrupt", "= R to Interrupt").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("useW_Auto", "= Auto W if hit").SetValue(new Slider(2, 0, 5)));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("useR_Auto", "= Auto R if hit").SetValue(new Slider(3, 0, 5)));                        
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("useE_Auto", "= Shield at % health").SetValue(new Slider(40, 100, 0)));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("RunLikeHell", "Mode Escape!").SetValue<KeyBind>(new KeyBind('Z', KeyBindType.Press)));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("sep1", "========="));

            Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("sep0", "====== Drawing"));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));

            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            Program.Menu.SubMenu("Drawing").AddItem(dmgAfterComboItem);
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("sep1", "========="));            
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 825);
            Q.SetSkillshot(0, 135, 1150, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 220);

            E = new Spell(SpellSlot.E, 1095);
            E.SetTargetted(0.25f, 1700);

            R = new Spell(SpellSlot.R, 300);
        }

        private void OnDraw(EventArgs args)
        {            

            if (Program.Menu.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Program.Menu.Item("Draw_Q").GetValue<bool>())
                if (Q.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("Draw_W").GetValue<bool>())
                if (W.Level > 0)
                    Utility.DrawCircle(Ball.BallPosition, W.Range, W.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("Draw_E").GetValue<bool>())
                if (E.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("Draw_R").GetValue<bool>())
                if (R.Level > 0)
                    Utility.DrawCircle(Ball.BallPosition, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }

        private void OnUpdate(EventArgs args)
        {
            CastE();
            CastW();
            CastR();            

            switch (LXOrbwalker.CurrentMode)
            {
                case LXOrbwalker.Mode.Combo:
                    if (Program.Menu.Item("useQ_Combo").GetValue<bool>())
                        CastQ();
                    break;
                case LXOrbwalker.Mode.Harass:
                    if (!ManaManagerAllowCast(Q))
                        return;
                    if (Program.Menu.Item("useQ_Harass").GetValue<bool>())
                        CastQ();
                    break;
                case LXOrbwalker.Mode.LaneClear:
                    LaneClear();
                    break;                
            }

            if (Program.Menu.Item("useHarass_Auto").GetValue<KeyBind>().Active)
                CastQ();

            if (Program.Menu.Item("RunLikeHell").GetValue<KeyBind>().Active)
            {
                ModeEscape();
            }
        }        

        private void LaneClear()
        {            
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

            var useQ = Program.Menu.Item("useQ_LaneClear").GetValue<bool>();
            var useW = Program.Menu.Item("useW_LaneClear").GetValue<bool>();

            var hit = 0;

            if (useQ && Q.IsReady() && ManaManagerAllowCast(Q))
            {
                foreach (var enemy in allMinionsW)
                {
                    Q.UpdateSourcePosition(Ball.BallPosition, ObjectManager.Player.Position);
                    if (!Q.IsReady() || !(ObjectManager.Player.Distance(enemy) <= Q.Range))
                        continue;
                    hit += allMinionsW.Count(enemy2 => enemy2.Distance(Q.GetPrediction(enemy).CastPosition) < Q.Width);
                    if (hit < 1)
                        continue;
                    if (Q.GetPrediction(enemy).Hitchance >= HitChance.High)
                        Q.Cast(Q.GetPrediction(enemy).CastPosition, true);
                }
            }

            if (!useW || !W.IsReady() || !ManaManagerAllowCast(W))
                return;
            hit = allMinionsW.Count(enemy => enemy.Distance(Ball.BallPosition) < W.Range);
            if (hit >= 1)
                W.Cast();
        }

        private void ModeEscape()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (E.IsReady())
            {
                E.CastOnUnit(ObjectManager.Player, Packets());                
            }
            if (W.IsReady() && !E.IsReady())
            {
                W.Cast();
            }

        }

        private void CastQ()
        {
            if (Ball.IsMoving || !Q.IsReady())
                return;            
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target == null)
                return;
            Q.UpdateSourcePosition(Ball.BallPosition, ObjectManager.Player.Position);
            if (Q.GetPrediction(target).Hitchance < HitChance.High)
                return;
            Ball.IsMoving = true;
            Q.Cast(target, Packets());
        }

        private void CastW()
        {
            if (Ball.IsMoving || !W.IsReady())
                return;
            W.UpdateSourcePosition(Ball.BallPosition, ObjectManager.Player.Position);
            switch (LXOrbwalker.CurrentMode)
            {
                case LXOrbwalker.Mode.Combo:
                    if (EnemysinRange(W.Range, 1, Ball.BallPosition))
                        W.Cast();
                    break;
                case LXOrbwalker.Mode.Harass:
                    if (!ManaManagerAllowCast(W))
                        return;
                    if (EnemysinRange(W.Range, 1, Ball.BallPosition))
                        W.Cast();
                    break;
                default:
                    if (EnemysinRange(W.Range, Program.Menu.Item("useW_Auto").GetValue<Slider>().Value, Ball.BallPosition))
                        W.Cast();
                    break;
            }
            if (Program.Menu.Item("useHarass_Auto").GetValue<KeyBind>().Active)
                if (!ManaManagerAllowCast(W))
                    if (EnemysinRange(W.Range, 1, Ball.BallPosition))
                        W.Cast();
        } 

        private void CastE()
        {
            if (Ball.IsMoving || !E.IsReady())
                return;
            var healhpercentuse = Program.Menu.Item("useE_Auto").GetValue<Slider>().Value;
            Obj_AI_Hero[] lowestFriend = { null };
            foreach (var friend in Program.AllHerosFriend.Where(
                hero =>
                    hero.Health / hero.MaxHealth * 100 < healhpercentuse && hero.IsValid && !hero.IsDead &&
                    hero.Distance(ObjectManager.Player) < E.Range).Where(friend => lowestFriend[0] == null || lowestFriend[0].Health / lowestFriend[0].MaxHealth * 100 > friend.Health / friend.MaxHealth * 100))
            {
                lowestFriend[0] = friend;
            }
            if (lowestFriend[0] != null && LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo)
                E.Cast(lowestFriend[0], Packets());           
        }

        private void CastR()
        {
            if (Ball.IsMoving || !R.IsReady())
                return;
            if (EnemysinRange(R.Range, Program.Menu.Item("useR_Auto").GetValue<Slider>().Value, Ball.BallPosition))
                R.Cast();
            switch (LXOrbwalker.CurrentMode)
            {
                case LXOrbwalker.Mode.Combo:
                    if (Program.AllHerosEnemy.Any(hero => hero.IsValidTarget() && hero.Position.Distance(Ball.BallPosition) < R.Range && (ObjectManager.Player.GetSpellDamage(hero, SpellSlot.R) > hero.Health || ObjectManager.Player.GetSpellDamage(hero, SpellSlot.R) + ObjectManager.Player.GetSpellDamage(hero, SpellSlot.W) > hero.Health && W.IsReady())))
                        R.Cast();
                    break;
            }
        }

        private void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Program.Menu.Item("useR_Interrupt").GetValue<bool>())
                return;
            if (Ball.BallPosition.Distance(unit.Position) < R.Range && !Ball.IsMoving)
            {
                R.Cast();
            }
            else if (Ball.BallPosition.Distance(unit.Position) < Q.Range)
            {
                if (EnoughManaFor(SpellSlot.Q, SpellSlot.R) && Q.IsReady() && R.IsReady())
                {
                    Q.Cast(unit.Position, Packets());
                    Utility.DelayAction.Add(100, CastRonReachPosition);
                    CastRPosition = unit.Position;
                }
            }
        }

        private void CastRonReachPosition()
        {
            if (Ball.BallPosition.Distance(CastRPosition) < 50)
                if (R.Cast())
                {
                    CastRPosition = new Vector3();
                    RInterruptTry = 0;
                }
            if (RInterruptTry < 15)
            {
                RInterruptTry += 1;
                Utility.DelayAction.Add(100, CastRonReachPosition);
            }
            else
            {
                RInterruptTry = 0;
                CastRPosition = new Vector3();
            }
        }

        private void Game_OnSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] != Packet.C2S.Cast.Header)
                return;
            var decodedPacket = Packet.C2S.Cast.Decoded(args.PacketData);
            if (decodedPacket.Slot != SpellSlot.R)
                return;
            if (!EnemysinRange(R.Range, 1, Ball.BallPosition))
                args.Process = false;

        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (Q.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q);
            if (W.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);
            if (E.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E);
            if (R.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.R);
            return (float)damage;
        }       

        internal class BallControl
        {
            public Vector3 BallPosition;
            public bool IsMoving;
            public bool IsonMe;
            public BallControl()
            {
                Drawing.OnDraw += CheckBallLocation;
                Obj_AI_Base.OnProcessSpellCast += OnSpellcast;
            }

            private void CheckBallLocation(EventArgs args)
            {
                if (ObjectManager.Player.HasBuff("orianaghostself", true))
                {
                    BallPosition = ObjectManager.Player.ServerPosition;
                    IsMoving = false;
                    IsonMe = true;
                    return;
                }

                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly && !ally.IsDead && ally.HasBuff("orianaghost", true)))
                {
                    BallPosition = ally.ServerPosition;
                    IsMoving = false;
                    IsonMe = false;
                    return;
                }
            }

            private void OnSpellcast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
            {
                var castedSlot = ObjectManager.Player.GetSpellSlot(args.SData.Name, false);
                if (!sender.IsMe)
                    return;

                if (castedSlot == SpellSlot.Q)
                {
                    IsMoving = true;
                    Utility.DelayAction.Add((int)Math.Max(1, 1000 * args.End.Distance(BallPosition) / 1200), () =>
                    {
                        BallPosition = args.End;
                        IsMoving = false;
                    });
                }

                if (castedSlot == SpellSlot.E)
                {
                    IsMoving = true;
                    Utility.DelayAction.Add((int)Math.Max(1, 1000 * args.End.Distance(BallPosition) / 1700), () =>
                    {
                        IsMoving = false;
                    });
                }
                
            }
        }
    }
}
