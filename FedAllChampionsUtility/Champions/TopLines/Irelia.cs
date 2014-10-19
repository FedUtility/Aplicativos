using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Linq;
using Color = System.Drawing.Color;

namespace FedAllChampionsUtility
{
    class Irelia : Champion
    {
        private static Spell _q;
        private static Spell _w;
        private static Spell _e;
        private static Spell _r;

        // Irelia Ultimate stuff.
        private static bool _hasToFire;
        private static int _charges;
        private static bool _packetCast;

        public Irelia()
        {
            LoadSpells();
            LoadMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += InterrupterOnOnPossibleToInterrupt;
            Drawing.OnDraw += Drawing_OnDraw;

            PluginLoaded();
        }

        private void LoadMenu()
        {
            // Combo
            var comboMenu = new Menu("Combo", "cmIreliaCombo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "Use W in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E in combo").SetValue(true));
            comboMenu.AddItem(new MenuItem("useEStun", "Use e only if target can be stunned").SetValue(false));
            comboMenu.AddItem(new MenuItem("useR", "Use R in combo").SetValue(true));
            Program.Menu.AddSubMenu(comboMenu);

            // Lasthiting
            var farmingMenu = new Menu("Farming", "cmIreliaFarming");
            farmingMenu.AddItem(new MenuItem("qLasthitEnable", "Last hitting with Q").SetValue(false));
            farmingMenu.AddItem(new MenuItem("qLastHit", "Last hit with Q").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            farmingMenu.AddItem(new MenuItem("qNoFarmTower", "Don't Q minions under tower").SetValue(false));
            // Wave clear submenu
            var waveClearMenu = new Menu("Wave Clear", "cmIreliaFarmingWaveClear");
            waveClearMenu.AddItem(new MenuItem("useQWC", "Use Q").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("useWWC", "Use W").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("useRWC", "Use R").SetValue(false));
            waveClearMenu.AddItem(new MenuItem("useQWCKillable", "Only Q killable minions").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("waveClear", "Wave Clear!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            farmingMenu.AddSubMenu(waveClearMenu);
            Program.Menu.AddSubMenu(farmingMenu);

            //Drawing menu
            var drawingMenu = new Menu("Drawing", "cmIreliaDraw");
            drawingMenu.AddItem(new MenuItem("qDraw", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("eDraw", "Draw E").SetValue(false));
            drawingMenu.AddItem(new MenuItem("rDraw", "Draw R").SetValue(true));
            Program.Menu.AddSubMenu(drawingMenu);

            //Misc
            var miscMenu = new Menu("Misc", "cmIreliaMisc");
            miscMenu.AddItem(new MenuItem("interruptUlts", "Interrupt ults with E").SetValue(true));
            miscMenu.AddItem(new MenuItem("interruptQE", "Q + E to interrupt if not in range").SetValue(true));            
            miscMenu.AddItem(new MenuItem("diveTower", "Dive tower when combo'ing").SetValue(false));
            miscMenu.AddItem(new MenuItem("diveTowerPercent", "Override dive tower").SetValue(new Slider(10)));
            miscMenu.AddItem(new MenuItem("dontQ", "Dont Q if range is small").SetValue(false));
            miscMenu.AddItem(new MenuItem("dontQRange", "Q Range").SetValue(new Slider(200, 0, 650)));
            Program.Menu.AddSubMenu(miscMenu);

            // Use combo, last hit, c
            Program.Menu.AddItem(new MenuItem("comboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
        }

        private void LoadSpells()
        {
            _q = new Spell(SpellSlot.Q, 650);
            _w = new Spell(SpellSlot.W, Orbwalking.GetRealAutoAttackRange(ObjectManager.Player)); // So confused.
            _e = new Spell(SpellSlot.E, 425);
            _r = new Spell(SpellSlot.R, 1000);
            
            _r.SetSkillshot(0.15f, 80f, 1500f, false, SkillshotType.SkillshotLine); // fix new prediction
        }

        private void InterrupterOnOnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Program.Menu.Item("interruptUlts").GetValue<bool>()) return;
            if (spell.DangerLevel != InterruptableDangerLevel.High || !CanStunTarget(unit)) return;

            var range = unit.Distance(ObjectManager.Player);
            if (range <= _e.Range)
            {
                _e.Cast(unit, _packetCast);
            }
            else if (range <= _q.Range)
            {
                _q.Cast(unit, _packetCast);
                _e.Cast(unit, _packetCast);
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            var drawQ = Program.Menu.Item("qDraw").GetValue<bool>();
            var drawE = Program.Menu.Item("eDraw").GetValue<bool>();
            var drawR = Program.Menu.Item("rDraw").GetValue<bool>();

            var position = ObjectManager.Player.Position;

            if (drawQ)
                Utility.DrawCircle(position, _q.Range, Color.Gray);

            if (drawE)
                Utility.DrawCircle(position, _e.Range, Color.Gray);

            if (drawR)
                Utility.DrawCircle(position, _r.Range, Color.Gray);
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            _packetCast = Packets();

            FireCharges();

            if (!Orbwalking.CanMove(100)) return;

            if (Program.Menu.Item("waveClear").GetValue<KeyBind>().Active && !ObjectManager.Player.IsDead)
            {
                WaveClear();
            }

            if (Program.Menu.Item("comboActive").GetValue<KeyBind>().Active && !ObjectManager.Player.IsDead)
            {
                Combo();
            }

            if (Program.Menu.Item("qLastHit").GetValue<KeyBind>().Active && Program.Menu.Item("qLasthitEnable").GetValue<bool>() &&
                !ObjectManager.Player.IsDead)
            {
                LastHitWithQ();
            }
        }

        private void WaveClear()
        {
            var useQ = Program.Menu.Item("useQWC").GetValue<bool>();
            var useW = Program.Menu.Item("useWWC").GetValue<bool>();
            var useR = Program.Menu.Item("useRWC").GetValue<bool>();

            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, 650);
            foreach (var minion in minions)
            {
                if (useQ)
                {
                    if (Program.Menu.Item("useQWCKillable").GetValue<bool>())
                    {
                        var damage = ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q);

                        if (damage > minion.Health)
                            _q.Cast(minion, _packetCast);
                    }
                    else
                    {
                        _q.Cast(minion, _packetCast);
                    }
                }

                if (useW) _w.Cast();
                if (useR) _r.Cast(minion, _packetCast);
            }
        }

        private void LastHitWithQ()
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range);
            foreach (var minion in minions.Where(minion => ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) > minion.Health))
            {
                var noFarmDangerous = Program.Menu.Item("qNoFarmTower").GetValue<bool>();
                // If do not farm under tower
                if (noFarmDangerous)
                {
                    if (!Utility.UnderTurret(minion))
                    {
                        _q.Cast(minion, _packetCast);
                    }
                }
                else
                {
                    _q.Cast(minion, _packetCast);
                }
            }
        }

        private void FireCharges()
        {
            if (!_hasToFire) return;

            _r.Cast(SimpleTs.GetTarget(1000, SimpleTs.DamageType.Physical), _packetCast); //Dunnno
            _charges -= 1;
            _hasToFire = _charges != 0;
        }

        private void Combo()
        {
            // Simple combo q -> w -> e -> r
            var useQ = Program.Menu.Item("useQ").GetValue<bool>();
            var useW = Program.Menu.Item("useW").GetValue<bool>();
            var useE = Program.Menu.Item("useE").GetValue<bool>();
            var useR = Program.Menu.Item("useR").GetValue<bool>();
            var useEStun = Program.Menu.Item("useEStun").GetValue<bool>();
            var target = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Physical);

            if (target == null || !target.IsValid) return;

            var isUnderTower = Utility.UnderTurret(target);
            var diveTower = Program.Menu.Item("diveTower").GetValue<bool>();
            var doNotCombo = false;

            // if target is under tower, and we do not want to dive
            if (isUnderTower && !diveTower)
            {
                // Calculate percent hp
                var percent = (int)target.Health / target.MaxHealth * 100;
                var overridePercent = Program.Menu.Item("diveTowerPercent").GetValue<Slider>().Value;

                if (percent > overridePercent) doNotCombo = true;
            }

            if (doNotCombo) return;

            if (useW && _w.IsReady())
            {
                _w.Cast();
            }

            // follow up with q
            if (useQ && _q.IsReady())
            {
                if (Program.Menu.Item("dontQ").GetValue<bool>())
                {
                    var distance = ObjectManager.Player.Distance(target);

                    if (distance > Program.Menu.Item("dontQRange").GetValue<Slider>().Value)
                    {
                        _q.Cast(target, _packetCast);
                    }
                }
                else
                {
                    _q.Cast(target, _packetCast);
                }
            }

            // stunerino
            if (useE && _e.IsReady())
            {
                if (useEStun)
                {
                    if (CanStunTarget(target))
                    {
                        _e.Cast(target, _packetCast);
                    }
                }
                else
                {
                    _e.Cast(target, _packetCast);
                }
            }

            // Resharper did this, IDK if it works.
            // Original code:  if (useR && R.IsReady() && !hasToFire)
            if (!useR || !_r.IsReady() || _hasToFire) return;
            _hasToFire = true;
            _charges = 4;
        }

        private bool CanStunTarget(AttackableUnit target)
        {
            var enemyHealthPercent = target.Health / target.MaxHealth * 100;
            var myHealthPercent = ObjectManager.Player.Health / ObjectManager.Player.MaxHealth * 100;

            return enemyHealthPercent > myHealthPercent;
        }
    }
}
