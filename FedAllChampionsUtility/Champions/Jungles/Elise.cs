using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using Menu = LeagueSharp.Common.Menu;
using MenuItem = LeagueSharp.Common.MenuItem;

namespace FedAllChampionsUtility
{
	class Elise : Champion 
	{       
        private static bool _human;
        private static bool _spider;
        private static Spell _humanQ, _humanW, _humanE, _r, _spiderQ, _spiderW, _spiderE;
        
        private static SpellSlot _igniteSlot;
        private static SpellDataInst _smiteSlot;

        private static readonly float[] HumanQcd = { 6, 6, 6, 6, 6 };
        private static readonly float[] HumanWcd = { 12, 12, 12, 12, 12 };
        private static readonly float[] HumanEcd = { 14, 13, 12, 11, 10 };
        private static readonly float[] SpiderQcd = { 6, 6, 6, 6, 6 };
        private static readonly float[] SpiderWcd = { 12, 12, 12, 12, 12 };
        private static readonly float[] SpiderEcd = { 26, 23, 20, 17, 14 };
        private static float _humQcd = 0, _humWcd = 0, _humEcd = 0;
        private static float _spidQcd = 0, _spidWcd = 0, _spidEcd = 0;
        private static float _humaQcd = 0, _humaWcd = 0, _humaEcd = 0;
        private static float _spideQcd = 0, _spideWcd = 0, _spideEcd = 0;
        
		public Elise()
        {
			LoadMenu();
			LoadSpells();

            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

			PluginLoaded();
		}

		private void LoadMenu()
		{
            //Combo
            Program.Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseHumanQ", "Human Q")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseHumanW", "Human W")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseHumanE", "Human E")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Auto use R")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseSpiderQ", "Spider Q")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseSpiderW", "Spider W")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseSpiderE", "Spider E")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass
            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Human Q")).SetValue(true);
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Human W")).SetValue(true);
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("Harrasmana", "Minimum Mana").SetValue(new Slider(50, 1, 100)));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("ActiveHarass", "Harass key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //Farm
            Program.Menu.AddSubMenu(new Menu("Farm", "Farm"));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("HumanQFarm", "Human Q")).SetValue(true);
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("HumanWFarm", "Human W")).SetValue(true);
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("SpiderQFarm", "Spider Q")).SetValue(false);
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("SpiderWFarm", "Spider W")).SetValue(true);
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("Farm_R", "Auto Switch(toggle)").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle)));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("ActiveFreeze", "Freeze Lane").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("ClearActive", "Clear Lane").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("Lanemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            //Farm
            Program.Menu.AddSubMenu(new Menu("Jungle", "Jungle"));
            Program.Menu.SubMenu("Jungle").AddItem(new MenuItem("HumanQFarmJ", "Human Q")).SetValue(true);
            Program.Menu.SubMenu("Jungle").AddItem(new MenuItem("HumanWFarmJ", "Human W")).SetValue(true);
            Program.Menu.SubMenu("Jungle").AddItem(new MenuItem("SpiderQFarmJ", "Spider Q")).SetValue(false);
            Program.Menu.SubMenu("Jungle").AddItem(new MenuItem("SpiderWFarmJ", "Spider W")).SetValue(true);
            Program.Menu.SubMenu("Jungle").AddItem(new MenuItem("smite", "Auto Smite").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
            Program.Menu.SubMenu("Jungle").AddItem(new MenuItem("ActiveJungle", "Jungle").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Jungle").AddItem(new MenuItem("Junglemana", "Minimum Mana").SetValue(new Slider(40, 1, 100)));

            //misc
            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));            
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("Spidergapcloser", "SpiderE to GapCloser")).SetValue(true);
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("Humangapcloser", "HumanE to GapCloser")).SetValue(true);
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("UseEInt", "HumanE to Interrupt")).SetValue(true);
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("smite", "Auto Smite").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("autoE", "HUmanE with VeryHigh Use").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Misc")
                                   .AddItem(new MenuItem("Echange", "E Hit").SetValue(
                                    new StringList(new[] { "Low", "Medium", "High", "Very High" })));



            //Kill Steal
            Program.Menu.AddSubMenu(new Menu("KillSteal", "Ks"));
            Program.Menu.SubMenu("Ks").AddItem(new MenuItem("ActiveKs", "Use KillSteal")).SetValue(true);
            Program.Menu.SubMenu("Ks").AddItem(new MenuItem("HumanQKs", "Human Q")).SetValue(true);
            Program.Menu.SubMenu("Ks").AddItem(new MenuItem("HumanWKs", "Human W")).SetValue(true);
            Program.Menu.SubMenu("Ks").AddItem(new MenuItem("SpiderQKs", "Spider Q")).SetValue(true);
            Program.Menu.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);


            //Drawings
            Program.Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Human Q")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Human W")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Human E")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("SpiderDrawQ", "Spider Q")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("SpiderDrawE", "Spider E")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));			
		}

		private void LoadSpells()
		{
            _humanQ = new Spell(SpellSlot.Q, 625f);
            _humanW = new Spell(SpellSlot.W, 950f);
            _humanE = new Spell(SpellSlot.E, 1075f);
            _spiderQ = new Spell(SpellSlot.Q, 475f);
            _spiderW = new Spell(SpellSlot.W, 0);
            _spiderE = new Spell(SpellSlot.E, 750f);
            _r = new Spell(SpellSlot.R, 0);

            _humanW.SetSkillshot(0.25f, 100f, 1000, true, SkillshotType.SkillshotLine);
            _humanE.SetSkillshot(0.25f, 55f, 1300, true, SkillshotType.SkillshotLine);

            //DFG = new Items.Item(3128, 750f);

            _igniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            _smiteSlot = ObjectManager.Player.SummonerSpellbook.GetSpell(ObjectManager.Player.GetSpellSlot("summonersmite"));			
		}

        private void Game_OnGameUpdate(EventArgs args)
        {
            Cooldowns();           

            Program.Orbwalker.SetAttack(true);

            CheckSpells();

            if (Program.Menu.Item("ActiveFreeze").GetValue<KeyBind>().Active ||
                Program.Menu.Item("ClearActive").GetValue<KeyBind>().Active)

                FarmLane();

            if (Program.Menu.Item("ActiveJungle").GetValue<KeyBind>().Active)
            {
                JungleFarm();

            }
            if (Program.Menu.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (Program.Menu.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
                Harass();

            }
            if (Program.Menu.Item("ActiveKs").GetValue<bool>())
            {
                KillSteal();
            }
            if (Program.Menu.Item("autoE").GetValue<KeyBind>().Active)
            {
                AutoE();

            }
        }
        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
                //Game.PrintChat("Spell name: " + args.SData.Name.ToString());
                GetCDs(args);
        }

        private void Combo()
        {
            var target = SimpleTs.GetTarget(_humanW.Range, SimpleTs.DamageType.Magical);
            var sReady = (_smiteSlot != null && _smiteSlot.Slot != SpellSlot.Unknown && _smiteSlot.State == SpellState.Ready);
            var qdmg = ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
            var wdmg = ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);
            if (target == null) return; //buffelisecocoon
            if (_human)
            {
                if (target.Distance(ObjectManager.Player.Position) < _humanE.Range && Program.Menu.Item("UseHumanE").GetValue<bool>() && _humanE.IsReady())
                {
                    if (sReady && Program.Menu.Item("smite").GetValue<bool>() &&
                        _humanE.GetPrediction(target).CollisionObjects.Count == 1)
                    {
                        CheckingCollision(target);
                        _humanE.Cast(target, Packets());
                    }
                    else if (_humanE.GetPrediction(target).Hitchance >= Echange())
                    {
                        _humanE.Cast(target, Packets());
                    }
                }

                if (ObjectManager.Player.Distance(target) <= _humanQ.Range && Program.Menu.Item("UseHumanQ").GetValue<bool>() && _humanQ.IsReady())
                {
                    _humanQ.Cast(target, Packets());
                }
                if (ObjectManager.Player.Distance(target) <= _humanW.Range && Program.Menu.Item("UseHumanW").GetValue<bool>() && _humanW.IsReady())
                {
                    _humanW.Cast(target, Packets());
                }
                if (!_humanQ.IsReady() && !_humanW.IsReady() && !_humanE.IsReady() && Program.Menu.Item("UseRCombo").GetValue<bool>() && _r.IsReady())
                {
                    _r.Cast();
                }
                if (!_humanQ.IsReady() && !_humanW.IsReady() && ObjectManager.Player.Distance(target) <= _spiderQ.Range && Program.Menu.Item("UseRCombo").GetValue<bool>() && _r.IsReady())
                {
                    _r.Cast();
                }
            }
            if (!_spider) return;
            if (ObjectManager.Player.Distance(target) <= _spiderQ.Range && Program.Menu.Item("UseSpiderQ").GetValue<bool>() && _spiderQ.IsReady())
            {
                _spiderQ.Cast(target, Packets());
            }
            if (ObjectManager.Player.Distance(target) <= 200 && Program.Menu.Item("UseSpiderW").GetValue<bool>() && _spiderW.IsReady())
            {
                _spiderW.Cast();
            }
            if (ObjectManager.Player.Distance(target) <= _spiderE.Range && ObjectManager.Player.Distance(target) > _spiderQ.Range && Program.Menu.Item("UseSpiderE").GetValue<bool>() && _spiderE.IsReady() && !_spiderQ.IsReady())
            {
                _spiderE.Cast(target, Packets());
            }
            if (ObjectManager.Player.Distance(target) > _spiderQ.Range && !_spiderE.IsReady() && _r.IsReady() && !_spiderQ.IsReady() && Program.Menu.Item("UseRCombo").GetValue<bool>())
            {
                _r.Cast();
            }
            if (_humanQ.IsReady() && _humanW.IsReady() && _r.IsReady() && Program.Menu.Item("UseRCombo").GetValue<bool>())
            {
                _r.Cast();
            }
            if (_humanQ.IsReady() && _humanW.IsReady() && _r.IsReady() && Program.Menu.Item("UseRCombo").GetValue<bool>())
            {
                _r.Cast();
            }
            if ((_humanQ.IsReady() && qdmg >= target.Health || _humanW.IsReady() && wdmg >= target.Health) && Program.Menu.Item("UseRCombo").GetValue<bool>())
            {
                _r.Cast();
            }
        }

        private void Harass()
        {
            var target = SimpleTs.GetTarget(_humanQ.Range, SimpleTs.DamageType.Magical);
            if (target != null)
            {

                if (_human && ObjectManager.Player.Distance(target) <= _humanQ.Range && Program.Menu.Item("UseQHarass").GetValue<bool>() && _humanQ.IsReady())
                {
                    _humanQ.Cast(target, Packets());
                }

                if (_human && ObjectManager.Player.Distance(target) <= _humanW.Range && Program.Menu.Item("UseWHarass").GetValue<bool>() && _humanW.IsReady())
                {
                    _humanW.Cast(target, Packets());
                }
            }
        }

        private void JungleFarm()
        {
            var jungleQ = (Program.Menu.Item("HumanQFarmJ").GetValue<bool>() && (100 * (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana)) > Program.Menu.Item("Junglemana").GetValue<Slider>().Value);
            var jungleW = (Program.Menu.Item("HumanWFarmJ").GetValue<bool>() && (100 * (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana)) > Program.Menu.Item("Junglemana").GetValue<Slider>().Value);
            var spiderjungleQ = Program.Menu.Item("SpiderQFarmJ").GetValue<bool>();
            var spiderjungleW = Program.Menu.Item("SpiderWFarmJ").GetValue<bool>();
            var switchR = (100 * (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana)) < Program.Menu.Item("Junglemana").GetValue<Slider>().Value;
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _humanQ.Range,
            MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                foreach (var minion in mobs)
                    if (_human)
                    {
                        if (jungleQ && _humanQ.IsReady() && minion.IsValidTarget() && ObjectManager.Player.Distance(minion) <= _humanQ.Range)
                        {
                            _humanQ.Cast(minion, Packets());
                        }
                        if (jungleW && _humanW.IsReady() && !_humanQ.IsReady() && minion.IsValidTarget() && ObjectManager.Player.Distance(minion) <= _humanW.Range)
                        {
                            _humanW.Cast(minion, Packets());
                        }
                        if ((!_humanQ.IsReady() && !_humanW.IsReady()) || switchR)
                        {
                            _r.Cast();
                        }
                    }
                foreach (var minion in mobs)
                {
                    if (_spider)
                    {
                        if (spiderjungleQ && _spiderQ.IsReady() && minion.IsValidTarget() && ObjectManager.Player.Distance(minion) <= _spiderQ.Range)
                        {
                            _spiderQ.Cast(minion, Packets());
                        }
                        if (spiderjungleW && _spiderW.IsReady() && minion.IsValidTarget() && ObjectManager.Player.Distance(minion) <= 150)
                        {
                            Program.Orbwalker.SetAttack(true);
                            _spiderW.Cast();
                        }
                        if (_r.IsReady() && _humanQ.IsReady() && !_spiderQ.IsReady() && !_spiderW.IsReady() && _spider)
                        {
                            _r.Cast();
                        }
                    }
                }
            }
        }

        private void FarmLane()
        {
            var ManaUse = (100 * (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana)) > Program.Menu.Item("Lanemana").GetValue<Slider>().Value;
            var useR = Program.Menu.Item("Farm_R").GetValue<KeyBind>().Active;
            var useHumQ = (Program.Menu.Item("HumanQFarm").GetValue<bool>() && (100 * (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana)) > Program.Menu.Item("Lanemana").GetValue<Slider>().Value);
            var useHumW = (Program.Menu.Item("HumanWFarm").GetValue<bool>() && (100 * (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana)) > Program.Menu.Item("Lanemana").GetValue<Slider>().Value);
            var useSpiQFarm = (_spiderQ.IsReady() && Program.Menu.Item("SpiderQFarm").GetValue<bool>());
            var useSpiWFarm = (_spiderW.IsReady() && Program.Menu.Item("SpiderWFarm").GetValue<bool>());
            var allminions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _humanQ.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
            {
                if (Program.Menu.Item("ClearActive").GetValue<KeyBind>().Active)
                {
                    foreach (var minion in allminions)
                        if (_human)
                        {
                            if (useHumQ && _humanQ.IsReady() && minion.IsValidTarget() && ObjectManager.Player.Distance(minion) <= _humanQ.Range)
                            {
                                _humanQ.Cast(minion);
                            }
                            if (useHumW && _humanW.IsReady() && minion.IsValidTarget() && ObjectManager.Player.Distance(minion) <= _humanW.Range)
                            {
                                _humanW.Cast(minion);
                            }
                            if (useR && _r.IsReady())
                            {
                                _r.Cast();
                            }
                        }
                    foreach (var minion in allminions)
                        if (_spider)
                        {
                            if (useSpiQFarm && _spiderQ.IsReady() && minion.IsValidTarget() && ObjectManager.Player.Distance(minion) <= _spiderQ.Range)
                            {
                                _spiderQ.Cast(minion);
                            }
                            if (useSpiWFarm && _spiderW.IsReady() && minion.IsValidTarget() && ObjectManager.Player.Distance(minion) <= 125)
                            {
                                _spiderW.Cast();
                            }
                        }
                }
                if (Program.Menu.Item("ActiveFreeze").GetValue<KeyBind>().Active)
                {
                    foreach (var minion in allminions)
                        if (_human)
                        {
                            if (useHumQ && ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) > minion.Health && _humanQ.IsReady() && minion.IsValidTarget() && ObjectManager.Player.Distance(minion) <= _humanQ.Range)
                            {
                                _humanQ.Cast(minion);
                            }
                            if (useHumW && ObjectManager.Player.GetSpellDamage(minion, SpellSlot.W) > minion.Health && _humanW.IsReady() && minion.IsValidTarget() && ObjectManager.Player.Distance(minion) <= _humanW.Range)
                            {
                                _humanW.Cast(minion);
                            }
                            if (useR && _r.IsReady())
                            {
                                _r.Cast();
                            }
                        }
                    foreach (var minion in allminions)
                        if (_spider)
                        {
                            if (useSpiQFarm && _spiderQ.IsReady() && ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) > minion.Health && _spiderQ.IsReady() && minion.IsValidTarget() && ObjectManager.Player.Distance(minion) <= _spiderQ.Range)
                            {
                                _spiderQ.Cast(minion);
                            }
                            if (useSpiQFarm && _spiderW.IsReady() && minion.IsValidTarget() && ObjectManager.Player.Distance(minion) <= 125)
                            {
                                _spiderW.Cast();
                            }
                        }
                }
            }
        }
        private void AutoE()
        {
            var target = SimpleTs.GetTarget(_humanE.Range, SimpleTs.DamageType.Magical);

            if (_human && ObjectManager.Player.Distance(target) < _humanE.Range && _humanE.IsReady() && _humanE.GetPrediction(target).Hitchance >= HitChance.VeryHigh)
            {
                _humanE.Cast(target, Packets());
            }
        }  

        private void Interrupter_OnPosibleToInterrupt(Obj_AI_Base target, InterruptableSpell spell)
        {
            if (!Program.Menu.Item("UseEInt").GetValue<bool>()) return;
            if (ObjectManager.Player.Distance(target) < _humanE.Range && target != null && _humanE.GetPrediction(target).Hitchance >= HitChance.Low)
            {
                _humanE.Cast(target, Packets());
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_spiderE.IsReady() && _spider && gapcloser.Sender.IsValidTarget(_spiderE.Range) && Program.Menu.Item("Spidergapcloser").GetValue<bool>())
            {
                _spiderE.Cast(gapcloser.Sender, Packets());
            }
            if (_humanE.IsReady() && _human && gapcloser.Sender.IsValidTarget(_humanE.Range) && Program.Menu.Item("Humangapcloser").GetValue<bool>())
            {
                _humanE.Cast(gapcloser.Sender, Packets());
            }
        }

        private float CalculateCd(float time)
        {
            return time + (time * ObjectManager.Player.PercentCooldownMod);
        }

        private void Cooldowns()
        {
            _humaQcd = ((_humQcd - Game.Time) > 0) ? (_humQcd - Game.Time) : 0;
            _humaWcd = ((_humWcd - Game.Time) > 0) ? (_humWcd - Game.Time) : 0;
            _humaEcd = ((_humEcd - Game.Time) > 0) ? (_humEcd - Game.Time) : 0;
            _spideQcd = ((_spidQcd - Game.Time) > 0) ? (_spidQcd - Game.Time) : 0;
            _spideWcd = ((_spidWcd - Game.Time) > 0) ? (_spidWcd - Game.Time) : 0;
            _spideEcd = ((_spidEcd - Game.Time) > 0) ? (_spidEcd - Game.Time) : 0;
        }

        private void GetCDs(GameObjectProcessSpellCastEventArgs spell)
        {
            if (_human)
            {
                if (spell.SData.Name == "EliseHumanQ")
                    _humQcd = Game.Time + CalculateCd(HumanQcd[_humanQ.Level]);
                if (spell.SData.Name == "EliseHumanW")
                    _humWcd = Game.Time + CalculateCd(HumanWcd[_humanW.Level]);
                if (spell.SData.Name == "EliseHumanE")
                    _humEcd = Game.Time + CalculateCd(HumanEcd[_humanE.Level]);
            }
            else
            {
                if (spell.SData.Name == "EliseSpiderQCast")
                    _spidQcd = Game.Time + CalculateCd(SpiderQcd[_spiderQ.Level]);
                if (spell.SData.Name == "EliseSpiderW")
                    _spidWcd = Game.Time + CalculateCd(SpiderWcd[_spiderW.Level]);
                if (spell.SData.Name == "EliseSpiderEInitial")
                    _spidEcd = Game.Time + CalculateCd(SpiderEcd[_spiderE.Level]);
            }
        }

        private HitChance Echange()
        {
            switch (Program.Menu.Item("Echange").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }
        // Credits to Brain0305
        private bool CheckingCollision(Obj_AI_Hero target)
        {
            foreach (var col in MinionManager.GetMinions(ObjectManager.Player.Position, 1500, MinionTypes.All, MinionTeam.NotAlly))
            {
                var segment = Geometry.ProjectOn(col.ServerPosition.To2D(), ObjectManager.Player.ServerPosition.To2D(),
                    col.Position.To2D());
                if (segment.IsOnSegment &&
                    target.ServerPosition.To2D().Distance(segment.SegmentPoint) <= GetHitBox(col) + 40)
                {
                    if (col.IsValidTarget(_smiteSlot.SData.CastRange[0]) &&
                        col.Health < ObjectManager.Player.GetSummonerSpellDamage(col, Damage.SummonerSpell.Smite))
                    {
                        ObjectManager.Player.SummonerSpellbook.CastSpell(_smiteSlot.Slot, col);
                        return true;
                    }
                }
            }
            return false;
        }
        // Credits to Brain0305
        private float GetHitBox(Obj_AI_Base minion)
        {
            var nameMinion = minion.Name.ToLower();
            if (nameMinion.Contains("mech")) return 65;
            if (nameMinion.Contains("wizard") || nameMinion.Contains("basic")) return 48;
            if (nameMinion.Contains("wolf") || nameMinion.Contains("wraith")) return 50;
            if (nameMinion.Contains("golem") || nameMinion.Contains("lizard")) return 80;
            if (nameMinion.Contains("dragon") || nameMinion.Contains("worm")) return 100;
            return 50;
        }

        private void KillSteal()
        {
            var target = SimpleTs.GetTarget(_humanQ.Range, SimpleTs.DamageType.Magical);
            var igniteDmg = ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            var qhDmg = ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
            var wDmg = ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);

            if (target != null && Program.Menu.Item("UseIgnite").GetValue<bool>() && _igniteSlot != SpellSlot.Unknown &&
            ObjectManager.Player.SummonerSpellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                if (igniteDmg > target.Health)
                {
                    ObjectManager.Player.SummonerSpellbook.CastSpell(_igniteSlot, target);
                }
            }
            if (_human)
            {
                if (_humanQ.IsReady() && ObjectManager.Player.Distance(target) <= _humanQ.Range && target != null && Program.Menu.Item("HumanQKs").GetValue<bool>())
                {
                    if (target.Health <= qhDmg)
                    {
                        _humanQ.Cast(target);
                    }
                }
                if (_humanW.IsReady() && ObjectManager.Player.Distance(target) <= _humanW.Range && target != null && Program.Menu.Item("HumanWKs").GetValue<bool>())
                {
                    if (target.Health <= wDmg)
                    {
                        _humanW.Cast(target);
                    }
                }
            }
            if (_spider && _spiderQ.IsReady() && ObjectManager.Player.Distance(target) <= _spiderQ.Range && target != null && Program.Menu.Item("SpiderQKs").GetValue<bool>())
            {
                if (target.Health <= qhDmg)
                {
                    _spiderQ.Cast(target);
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            var elise = Drawing.WorldToScreen(ObjectManager.Player.Position);

            if (Program.Menu.Item("CircleLag").GetValue<bool>())
            {
                if (_human && Program.Menu.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _humanQ.Range, System.Drawing.Color.LightGray,
                        Program.Menu.Item("CircleThickness").GetValue<Slider>().Value,
                        Program.Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_human && Program.Menu.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _humanW.Range, System.Drawing.Color.LightGray,
                        Program.Menu.Item("CircleThickness").GetValue<Slider>().Value,
                        Program.Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_human && Program.Menu.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _humanE.Range, System.Drawing.Color.LightGray,
                        Program.Menu.Item("CircleThickness").GetValue<Slider>().Value,
                        Program.Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_spider && Program.Menu.Item("SpiderDrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _spiderQ.Range, System.Drawing.Color.LightGray,
                        Program.Menu.Item("CircleThickness").GetValue<Slider>().Value,
                        Program.Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_spider && Program.Menu.Item("SpiderDrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _spiderE.Range, System.Drawing.Color.LightGray,
                   Program.Menu.Item("CircleThickness").GetValue<Slider>().Value,
                   Program.Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (_human && Program.Menu.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _humanQ.Range, System.Drawing.Color.LightGray);
                }
                if (_human && Program.Menu.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _humanW.Range, System.Drawing.Color.LightGray);
                }
                if (_human && Program.Menu.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _humanE.Range, System.Drawing.Color.LightGray);
                }
                if (_spider && Program.Menu.Item("SpiderDrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _spiderQ.Range, System.Drawing.Color.LightGray);
                }
                if (_spider && Program.Menu.Item("SpiderDrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _spiderE.Range, System.Drawing.Color.LightGray);
                }
            }
            if (!_spider)
            {
                if (_spideQcd == 0)
                    Drawing.DrawText(elise[0] - 60, elise[1], Color.White, "SQ Rdy");
                else
                    Drawing.DrawText(elise[0] - 60, elise[1], Color.Orange, "SQ: " + _spideQcd.ToString("0.0"));
                if (_spideWcd == 0)
                    Drawing.DrawText(elise[0] - 20, elise[1] + 30, Color.White, "SW Rdy");
                else
                    Drawing.DrawText(elise[0] - 20, elise[1] + 30, Color.Orange, "SW: " + _spideWcd.ToString("0.0"));
                if (_spideEcd == 0)
                    Drawing.DrawText(elise[0], elise[1], Color.White, "SE Rdy");
                else
                    Drawing.DrawText(elise[0], elise[1], Color.Orange, "SE: " + _spideEcd.ToString("0.0"));
            }
            else
            {
                if (_humaQcd == 0)
                    Drawing.DrawText(elise[0] - 60, elise[1], Color.White, "HQ Rdy");
                else
                    Drawing.DrawText(elise[0] - 60, elise[1], Color.Orange, "HQ: " + _humaQcd.ToString("0.0"));
                if (_humaWcd == 0)
                    Drawing.DrawText(elise[0] - 20, elise[1] + 30, Color.White, "HW Rdy");
                else
                    Drawing.DrawText(elise[0] - 20, elise[1] + 30, Color.Orange, "HW: " + _humaWcd.ToString("0.0"));
                if (_humaEcd == 0)
                    Drawing.DrawText(elise[0], elise[1], Color.White, "HE Rdy");
                else
                    Drawing.DrawText(elise[0], elise[1], Color.Orange, "HE: " + _humaEcd.ToString("0.0"));
            }
        }

        private void CheckSpells()
        {
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "EliseHumanQ" ||
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "EliseHumanW" ||
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Name == "EliseHumanE")
            {
                _human = true;
                _spider = false;
            }

            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "EliseSpiderQCast" ||
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "EliseSpiderW" ||
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Name == "EliseSpiderEInitial")
            {
                _human = false;
                _spider = true;
            }
        }		
	}
}
