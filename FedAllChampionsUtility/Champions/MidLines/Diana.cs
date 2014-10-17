using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace FedAllChampionsUtility
{
    class Diana : Champion
    {
        private static readonly List<Spell> SpellList = new List<Spell>();
        private static Spell _q, _w, _e, R;
        private static Obj_SpellMissile _qpos;
        private static bool _qcreated = false;       

        private static Items.Item _dfg;
        private static SpellSlot _igniteSlot;        
        
        public Diana()
        {
            LoadSpells();
            LoadMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete; 
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

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
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseRSecond", "Use Second R")).SetValue(false);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));            

            //Extra
            Program.Menu.AddSubMenu(new Menu("Extra", "Extra"));
            Program.Menu.SubMenu("Extra").AddItem(new MenuItem("Inter_E", "Interrupter E")).SetValue(true);
            Program.Menu.SubMenu("Extra").AddItem(new MenuItem("Gap_W", "GapClosers W")).SetValue(true);
            Program.Menu.SubMenu("Extra").AddItem(new MenuItem("AutoShield", "Auto W")).SetValue(true);
            Program.Menu.SubMenu("Extra").AddItem(new MenuItem("Shieldper", "Self Health %")).SetValue(new Slider(40, 1, 100));
            Program.Menu.SubMenu("Extra").AddItem(new MenuItem("Escape", "Escape Key!").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));            

            //Harass
            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("ActiveHarass", "Harass key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("harasstoggle", "Harass(toggle)").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("Harrasmana", "Minimum Mana").SetValue(new Slider(50, 1, 100)));

            //Harass
            Program.Menu.AddSubMenu(new Menu("Lane", "Lane"));
            Program.Menu.SubMenu("Lane").AddItem(new MenuItem("UseQLane", "Use Q")).SetValue(false);
            Program.Menu.SubMenu("Lane").AddItem(new MenuItem("UseWLane", "Use W")).SetValue(false);
            Program.Menu.SubMenu("Lane").AddItem(new MenuItem("ActiveLane", "Farm key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Lane").AddItem(new MenuItem("Lanemana", "Minimum Mana").SetValue(new Slider(50, 1, 100)));

            //jungle
            Program.Menu.AddSubMenu(new Menu("Jungle", "Jungle"));
            Program.Menu.SubMenu("Jungle").AddItem(new MenuItem("UseQJungle", "Use Q")).SetValue(true);
            Program.Menu.SubMenu("Jungle").AddItem(new MenuItem("UseWJungle", "Use W")).SetValue(true);
            Program.Menu.SubMenu("Jungle").AddItem(new MenuItem("ActiveJungle", "Jungle key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Jungle").AddItem(new MenuItem("Junglemana", "Minimum Mana").SetValue(new Slider(20, 1, 100)));

            //Kill Steal
            Program.Menu.AddSubMenu(new Menu("KillSteal", "Ks"));
            Program.Menu.SubMenu("Ks").AddItem(new MenuItem("ActiveKs", "Use KillSteal")).SetValue(true);
            Program.Menu.SubMenu("Ks").AddItem(new MenuItem("UseQKs", "Use Q")).SetValue(true);
            Program.Menu.SubMenu("Ks").AddItem(new MenuItem("UseRKs", "Use R")).SetValue(true);
            Program.Menu.SubMenu("Ks").AddItem(new MenuItem("TargetRange", "R use if range >").SetValue(new Slider(400, 200, 600)));

            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after a rotation").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            //Drawings
            Program.Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("ShowPassive", "Show Passive")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));
        }

        private void LoadSpells()
        {
            _q = new Spell(SpellSlot.Q, 830f);
            _w = new Spell(SpellSlot.W, 200f);
            _e = new Spell(SpellSlot.E, 420f);
            R = new Spell(SpellSlot.R, 825f);

            _q.SetSkillshot(0.35f, 200f, 1800, false, SkillshotType.SkillshotCircle);

            SpellList.Add(_q);
            SpellList.Add(_w);
            SpellList.Add(_e);
            SpellList.Add(R);

            _dfg = new Items.Item(3128, 750f);

            _igniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
        }

        private void Game_OnGameUpdate(EventArgs args)
        {           

            Program.Orbwalker.SetAttack(true);

            if (Program.Menu.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Misaya();
            }
            
            if ((Program.Menu.Item("ActiveHarass").GetValue<KeyBind>().Active || Program.Menu.Item("harasstoggle").GetValue<KeyBind>().Active) && (100 * (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana)) > Program.Menu.Item("Harrasmana").GetValue<Slider>().Value)
            {
                Harass();
            }
            if (Program.Menu.Item("ActiveLane").GetValue<KeyBind>().Active && (100 * (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana)) > Program.Menu.Item("Lanemana").GetValue<Slider>().Value)
            {
                Farm();
            }
            if (Program.Menu.Item("ActiveJungle").GetValue<KeyBind>().Active && (100 * (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana)) > Program.Menu.Item("Junglemana").GetValue<Slider>().Value)
            {
                JungleClear();
            }
            if (Program.Menu.Item("Escape").GetValue<KeyBind>().Active)
            {
                Tragic();
            }
            if (Program.Menu.Item("ActiveKs").GetValue<bool>())
            {
                KillSteal();
            }
            if (Program.Menu.Item("AutoShield").GetValue<bool>() && !Program.Menu.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                AutoW();
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_w.IsReady() && gapcloser.Sender.IsValidTarget(_w.Range) && Program.Menu.Item("Gap_W").GetValue<bool>())
            {
                _w.Cast(gapcloser.Sender, Packets());
            }
        }
        private void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (_e.IsReady() && unit.IsValidTarget(_e.Range) && Program.Menu.Item("Inter_E").GetValue<bool>())
                _e.Cast();
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                Game.PrintChat("Spell name: " + args.SData.Name.ToString());
            }
        }
        
        private void Misaya()
        {
            var target = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
            if (target != null)
            {
                if (ObjectManager.Player.Distance(target) <= _dfg.Range && Program.Menu.Item("UseItems").GetValue<bool>() && _dfg.IsReady() && target.Health <= ComboDamage(target))
                {
                    _dfg.Cast(target);
                }

                if (ObjectManager.Player.Distance(target) <= _q.Range && Program.Menu.Item("UseQCombo").GetValue<bool>() && _q.IsReady() && _q.GetPrediction(target).Hitchance >= HitChance.High)
                {
                    _q.CastIfHitchanceEquals(target, HitChance.High, Packets());
                }
                if (ObjectManager.Player.Distance(target) <= R.Range && Program.Menu.Item("UseRCombo").GetValue<bool>() && R.IsReady() && ((_qcreated == true)
                    || target.HasBuff("dianamoonlight", true)))
                {
                    R.Cast(target, Packets());
                }
                if (ObjectManager.Player.Distance(target) <= _w.Range && Program.Menu.Item("UseWCombo").GetValue<bool>() && _w.IsReady() && !_q.IsReady())
                {
                    _w.Cast();
                }
                if (ObjectManager.Player.Distance(target) <= _e.Range && ObjectManager.Player.Distance(target) >= _w.Range && Program.Menu.Item("UseECombo").GetValue<bool>() && _e.IsReady() && !_w.IsReady())
                {
                    _e.Cast();
                }
                if (ObjectManager.Player.Distance(target) <= R.Range && Program.Menu.Item("UseRSecond").GetValue<bool>() && R.IsReady() && !_w.IsReady() && !_q.IsReady())
                {
                    R.Cast(target, Packets());
                }
            }
        }        

        private float ComboDamage(Obj_AI_Hero hero)
        {

            var dmg = 0d;

            if (_q.IsReady())
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.Q);
            if (_w.IsReady())
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.W);
            if (R.IsReady())
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.R) * 2;
            if (Items.HasItem(3128))
            {
                dmg += ObjectManager.Player.GetItemDamage(hero, Damage.DamageItems.Dfg);
                dmg = dmg * 1.2;
            }
            if (ObjectManager.Player.GetSpellSlot("SummonerIgnite") != SpellSlot.Unknown)
            {
                dmg += ObjectManager.Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            return (float)dmg;
        }

        private void Harass()
        {
            var target = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
            if (target != null)
            {
                if (ObjectManager.Player.Distance(target) <= _q.Range && Program.Menu.Item("UseQHarass").GetValue<bool>() && _q.IsReady())
                {
                    _q.CastIfHitchanceEquals(target, HitChance.Medium, Packets());
                }
                if (ObjectManager.Player.Distance(target) <= 200 && Program.Menu.Item("UseWHarass").GetValue<bool>() && _w.IsReady())
                {
                    _w.Cast();
                }
            }
        }

        private void Farm()
        {            

            var rangedMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
            MinionTypes.Ranged);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
            MinionTypes.All);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _w.Range, MinionTypes.All);

            var useQ = Program.Menu.Item("UseQLane").GetValue<bool>();
            var useW = Program.Menu.Item("UseWLane").GetValue<bool>();
            if (_q.IsReady() && useQ)
            {
                var fl1 = _q.GetCircularFarmLocation(rangedMinionsQ, _q.Width);
                var fl2 = _q.GetCircularFarmLocation(allMinionsQ, _q.Width);

                if (fl1.MinionsHit >= 3)
                {
                    _q.Cast(fl1.Position);
                }
                else if (fl2.MinionsHit >= 2 || allMinionsQ.Count == 1)
                {
                    _q.Cast(fl2.Position);
                }
                else
                    foreach (var minion in allMinionsQ)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                        minion.Health < 0.75 * ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q))
                            _q.Cast(minion);
            }
            if (_w.IsReady() && useW && allMinionsW.Count > 2)
            {
                _w.Cast(allMinionsW[0]);
            }
        }

        private void Tragic()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
            MinionTypes.All);
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range,
            MinionTypes.All,
            MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (_q.IsReady()) _q.Cast(Game.CursorPos);
            if (R.IsReady())
            {
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];

                    R.CastOnUnit(mob);
                }
                else
                    if (allMinionsQ.Count >= 1)
                    {
                        R.Cast(allMinionsQ[0]);
                    }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range,
  MinionTypes.All,
  MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                _q.CastOnUnit(mob);
                _w.CastOnUnit(mob);
            }
        }        

        private void KillSteal()
        {
            var target = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
            var igniteDmg = ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            var qhDmg = ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
            var rhDmg = ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);
            var rRange = (ObjectManager.Player.Distance(target) >= Program.Menu.Item("TargetRange").GetValue<Slider>().Value);
            
            if (_q.IsReady() && ObjectManager.Player.Distance(target) <= _q.Range && target != null && Program.Menu.Item("UseQKs").GetValue<bool>())
            {
                if (target.Health <= qhDmg)
                {
                    _q.Cast(target, Packets());
                }
            }

            if (R.IsReady() && ObjectManager.Player.Distance(target) <= R.Range && rRange && target != null && Program.Menu.Item("UseRKs").GetValue<bool>())
            {
                if (target.Health <= rhDmg)
                {
                    R.Cast(target, Packets());
                }
            }
        }

        private void AutoW()
        {
            if (ObjectManager.Player.HasBuff("Recall")) return;
            if (_w.IsReady() && ObjectManager.Player.Health <= (ObjectManager.Player.MaxHealth * (Program.Menu.Item("Shieldper").GetValue<Slider>().Value) / 100))
            {
                _w.Cast();
            }

        }       

        private void OnCreate(GameObject sender, EventArgs args)
        {
            var spell = (Obj_SpellMissile)sender;
            var unit = spell.SpellCaster.Name;
            var name = spell.SData.Name;

            //debug
            //if (unit == ObjectManager.Player.Name)


            if (unit == ObjectManager.Player.Name && (name == "dianaarcthrow"))
            {
                // Game.PrintChat("Spell: " + name);
                _qpos = spell;
                _qcreated = true;
                return;
            }
        }

        //misaya by xSalice
        private void OnDelete(GameObject sender, EventArgs args)
        {
            var spell = (Obj_SpellMissile)sender;
            var unit = spell.SpellCaster.Name;
            var name = spell.SData.Name;

            if (unit == ObjectManager.Player.Name && (name == "dianaarcthrow"))
            {
                _qpos = null;
                _qcreated = false;
                return;
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            var diana = Drawing.WorldToScreen(ObjectManager.Player.Position);
            if (Program.Menu.Item("CircleLag").GetValue<bool>())
            {
                if (_qpos != null)
                    Utility.DrawCircle(_qpos.Position, _qpos.BoundingRadius, System.Drawing.Color.Red, 5, 30, false);
                if (Program.Menu.Item("ShowPassive").GetValue<bool>())
                {
                    if (ObjectManager.Player.HasBuff("dianaarcready"))
                        Drawing.DrawText(diana[0] - 10, diana[1], Color.White, "P On");
                    else
                        Drawing.DrawText(diana[0] - 10, diana[1], Color.Orange, "P Off");
                }

                if (Program.Menu.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.White,
                        Program.Menu.Item("CircleThickness").GetValue<Slider>().Value,
                        Program.Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Program.Menu.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.White,
                        Program.Menu.Item("CircleThickness").GetValue<Slider>().Value,
                        Program.Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Program.Menu.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.White,
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
                    Drawing.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.White);
                }
                if (Program.Menu.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.White);
                }
                if (Program.Menu.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.White);
                }

                if (Program.Menu.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.White);
                }

            }
        }
    }
}
