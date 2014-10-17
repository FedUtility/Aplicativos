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
    class Jax : Champion
    {
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        private static SpellSlot IgniteSlot;
        private static SpellSlot SmiteSlot;

        public static bool Eactive = false; 

        public Jax()
        {
            LoadMenu();
            LoadSpells();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;

            PluginLoaded();

        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 700f);
            W = new Spell(SpellSlot.W, 200f);
            E = new Spell(SpellSlot.E, 187f);
            R = new Spell(SpellSlot.R, 200f);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            SmiteSlot = ObjectManager.Player.GetSpellSlot("SummonerSmite");

            SpellList.AddRange(new[] { Q, W, E, R });

        }

        private void LoadMenu()
        {
            Program.Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UnderTurretCombo", "Q Under Turret?").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseItensCombo", "Use Items in Combo").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q in Harass").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W in Harass").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E in Harass").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UnderTurretHarass", "Q Under Turret?").SetValue(false));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("ModeHarass", "Harass Mode: ").SetValue(new StringList(new[] { "Q+W+E", "Q+W", "Q+E", "E+Q", "Default" }, 4)));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("ManaHarass", "Dont Harass if Mana < %").SetValue(new Slider(50, 100, 0)));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Farm", "Farm"));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "Use Q Farm").SetValue(true));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "Use W Farm").SetValue(true));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "Use E Farm").SetValue(true));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("ManaFarm", "Min. Mana Percent").SetValue(new Slider(50, 100, 0)));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "Freeze!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("WardJumpSmite", "WardJump + Smite").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("AutoSmite", "Auto Smite!").SetValue<KeyBind>(new KeyBind('J', KeyBindType.Toggle)));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmActive", "JungleFarm!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("laugh", "Troll laugh?").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("AutoI", "Auto Ignite").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("stun", "Interrupt Spells").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("AutoEWQTower", "Auto Attack my tower").SetValue(false));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("Ward", "Ward Jump!").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));            
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("JumpSmiteRange", "wJumper+Smite Range").SetValue(new Circle(false, Color.FromArgb(150, Color.DodgerBlue))));

            Program.Menu.AddSubMenu(new Menu("Spells", "Spells"));
            Program.Menu.SubMenu("Spells").AddItem(new MenuItem("setQ", "Use Q: ").SetValue(new StringList(new[] { "Out of Range", "Melee Range", "Both" }, 2)));
            Program.Menu.SubMenu("Spells").AddItem(new MenuItem("setW", "Use W: ").SetValue(new StringList(new[] { "Every AA", "After third AA" }, 1)));
            Program.Menu.SubMenu("Spells").AddItem(new MenuItem("setE", "Use E: ").SetValue(new StringList(new[] { "Q Range", "Melee Range", "Both" }, 0)));
            Program.Menu.SubMenu("Spells").AddItem(new MenuItem("AutoUlt", "Enable Auto Ult").SetValue(true));
            Program.Menu.SubMenu("Spells").AddItem(new MenuItem("minEnemies", "Min. Enemies in Range").SetValue(new Slider(2, 5, 0)));           

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Program.Menu.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Program.Menu.Item("Draw_Q").GetValue<bool>())
                if (Q.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("Draw_E").GetValue<bool>())
                if (E.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            var menuItem = Program.Menu.Item("JumpSmiteRange").GetValue<Circle>();
            if (menuItem.Active)
                Utility.DrawCircle(ObjectManager.Player.Position, 1100, menuItem.Color);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead) return;

            if (Program.Menu.Item("setW").GetValue<StringList>().SelectedIndex == 1)
            {
                if (Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active && ObjectManager.Player.HasBuff("jaxrelentlessassaultas", true) && W.IsReady())
                {
                    W.Cast();
                }
            }

            if (Program.Menu.Item("AutoI").GetValue<bool>())
            {
                AutoIgnite();
            }

            if (!Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active && !Program.Menu.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                Eactive = false;
            }

            if (Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (Program.Menu.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                Harass();
            }

            if (Program.Menu.Item("FreezeActive").GetValue<KeyBind>().Active)
            {
                FreezeFarm();
            }
            if (Program.Menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
            {
                LaneClear();
            }

            if (Program.Menu.Item("JungleFarmActive").GetValue<KeyBind>().Active)
            {
                JungleFarm();
            }

            if (Program.Menu.Item("Ward").GetValue<KeyBind>().Active)
            {
                wJumper.wardJump(Game.CursorPos.To2D());
            }

            if (Program.Menu.Item("AutoSmite").GetValue<KeyBind>().Active)
            {
                AutoSmite();
            }

            if (Program.Menu.Item("AutoEWQTower").GetValue<KeyBind>().Active)
            {
                AutoUnderTower();
            }
        }


        private static void AutoUnderTower()
        {
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);

            if (Utility.UnderTurret(qTarget, false) && Q.IsReady() && E.IsReady())
            {
                E.Cast();
                W.Cast();
                Q.Cast(qTarget);
            }
        }

        private static void AutoIgnite()
        {
            if (IgniteSlot == SpellSlot.Unknown || ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) != SpellState.Ready) return;

            const int range = 600;

            foreach (var enemy in Program.Helper.EnemyTeam.Where(hero => hero.IsValidTarget(range) && ObjectManager.Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite) * 0.9 >= hero.Health))
            {
                ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, enemy);
                if (Program.Menu.Item("laugh").GetValue<bool>())
                {
                    Game.Say("/l");
                }
                return;
            }
        }

        private static void AutoUlt()
        {
            int inimigos = Utility.CountEnemysInRange(650);

            if (Program.Menu.Item("minEnemies").GetValue<Slider>().Value <= inimigos)
            {
                R.Cast();
            }
        }

        private static void CastSpellQ()
        {
            var useQi = Program.Menu.Item("setQ").GetValue<StringList>().SelectedIndex;
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);

            if (!Program.Menu.Item("UnderTurretCombo").GetValue<bool>() && Utility.UnderTurret(qTarget) && Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active) return;
            if (!Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && Utility.UnderTurret(qTarget) && Program.Menu.Item("HarassActive").GetValue<KeyBind>().Active) return;

            switch (useQi)
            {
                case 0:
                    if (qTarget.Distance(ObjectManager.Player) >= LXOrbwalker.GetAutoAttackRange(ObjectManager.Player))
                        Q.CastOnUnit(qTarget);
                    break;
                case 1:
                    if (qTarget.Distance(ObjectManager.Player) <= LXOrbwalker.GetAutoAttackRange(ObjectManager.Player))
                        Q.CastOnUnit(qTarget);
                    break;
                case 2:
                    Q.CastOnUnit(qTarget);
                    break;
            }
        }

        private static void CastSpellE()
        {
            var useEi = Program.Menu.Item("setE").GetValue<StringList>().SelectedIndex;

            if (!Eactive && ((useEi == 0 || useEi == 2) && Q.IsReady() && (Utility.CountEnemysInRange((int)Q.Range + 200) >= 1)) ||
                 (useEi >= 1 && (Utility.CountEnemysInRange((int)E.Range + 100) >= 1)))
            {
                E.Cast();
                E.LastCastAttemptT = Environment.TickCount;
                Eactive = true;
            }
        }

        private static void ActivateE()
        {
            if (!E.IsReady() || !Eactive) return;

            if (E.IsReady() && Environment.TickCount - E.LastCastAttemptT <= 5000)
            {
                if (Utility.CountEnemysInRange((int)E.Range) >= 1)
                {
                    E.Cast();
                    Eactive = false;
                }
            }
        }

        private static void Combo()
        {
            var iTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            
            if (Program.Menu.Item("UseECombo").GetValue<bool>() && E.IsReady())
            {
                CastSpellE();
            }
            if (iTarget != null && Program.Menu.Item("setW").GetValue<StringList>().SelectedIndex == 0 && Program.Menu.Item("UseWCombo").GetValue<bool>() && W.IsReady())
            {
                W.Cast();
            }
            if (Program.Menu.Item("UseQCombo").GetValue<bool>() && Q.IsReady())
            {
                CastSpellQ();
            }
            if (Program.Menu.Item("UseECombo").GetValue<bool>())
            {
                ActivateE();
            }

            if (Program.Menu.Item("AutoUlt").GetValue<bool>() && R.IsReady())
            {
                AutoUlt();
            }

        }

        private static void Harass()
        {
            var hTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);

            var HMana = Program.Menu.Item("ManaHarass").GetValue<Slider>().Value;
            var MPercentH = ObjectManager.Player.Mana * 100 / ObjectManager.Player.MaxMana;

            int HarassMode = Program.Menu.Item("ModeHarass").GetValue<StringList>().SelectedIndex;

            if (MPercentH >= HMana && hTarget != null)
            {
                switch (HarassMode)
                {
                    case 0:
                        {
                            if (!Q.IsReady() || !W.IsReady() || !E.IsReady()) return;

                            if ((Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && Utility.UnderTurret(hTarget)) ||
                               (!Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && !Utility.UnderTurret(hTarget)) ||
                               (Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && !Utility.UnderTurret(hTarget)))
                            {
                                if (!Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && Utility.UnderTurret(hTarget)) return;

                                Q.CastOnUnit(hTarget);
                                W.Cast();
                                E.Cast();
                            }
                            break;
                        }
                    case 1:
                        {
                            if (Q.IsReady() && W.IsReady())
                            {
                                if ((Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && Utility.UnderTurret(hTarget)) ||
                                   (!Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && !Utility.UnderTurret(hTarget)) ||
                                   (Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && !Utility.UnderTurret(hTarget)))
                                {
                                    if (!Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && Utility.UnderTurret(hTarget)) return;

                                    Q.CastOnUnit(hTarget);
                                    W.Cast();
                                }
                            }
                            break;
                        }
                    case 2:
                        {
                            if (!Q.IsReady() || !E.IsReady()) return;

                            if ((Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && Utility.UnderTurret(hTarget)) ||
                               (!Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && !Utility.UnderTurret(hTarget)) ||
                               (Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && !Utility.UnderTurret(hTarget)))
                            {
                                if (!Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && Utility.UnderTurret(hTarget)) return;

                                Q.CastOnUnit(hTarget);
                                E.Cast();
                            }
                            break;
                        }
                    case 3:
                        {
                            if (!Q.IsReady() || !E.IsReady()) return;


                            if ((Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && Utility.UnderTurret(hTarget)) ||
                               (!Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && !Utility.UnderTurret(hTarget)) ||
                               (Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && !Utility.UnderTurret(hTarget)))
                            {
                                if (!Program.Menu.Item("UnderTurretHarass").GetValue<bool>() && Utility.UnderTurret(hTarget)) return;

                                CastSpellE();
                                CastSpellQ();
                                ActivateE();
                            }
                            break;
                        }
                    case 4:
                        {
                            if (Program.Menu.Item("UseWHarass").GetValue<bool>() && W.IsReady())
                            {
                                W.Cast();
                            }
                            if (Program.Menu.Item("UseEHarass").GetValue<bool>() && E.IsReady())
                            {
                                CastSpellE();
                            }
                            if (Program.Menu.Item("UseQHarass").GetValue<bool>() && Q.IsReady())
                            {
                                CastSpellQ();
                            }
                            if (Program.Menu.Item("UseEHarass").GetValue<bool>())
                            {
                                ActivateE();
                            }
                            break;
                        }
                }

            }
        }

        private static void FreezeFarm()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);

            foreach (var vMinion in allMinionsQ)
            {
                var Qdamage = ObjectManager.Player.GetSpellDamage(vMinion, SpellSlot.Q) * 0.85;

                if (Program.Menu.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && Qdamage >= Q.GetHealthPrediction(vMinion))
                {
                    Q.CastOnUnit(vMinion);
                }
            }
        }

        private static void LaneClear()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);

            var FMana = Program.Menu.Item("ManaFarm").GetValue<Slider>().Value;
            var MPercent = ObjectManager.Player.Mana * 100 / ObjectManager.Player.MaxMana;

            foreach (var vMinion in allMinionsQ)
            {
                var Qdamage = ObjectManager.Player.GetSpellDamage(vMinion, SpellSlot.Q) * 0.85;

                if (Program.Menu.Item("setW").GetValue<StringList>().SelectedIndex == 0 && W.IsReady() && Program.Menu.Item("UseWFarm").GetValue<bool>() && allMinionsQ.Count > 0)
                {
                    W.Cast();
                }
                if (Program.Menu.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && Qdamage >= Q.GetHealthPrediction(vMinion) && MPercent >= FMana)
                {
                    Q.CastOnUnit(vMinion);
                }

                if (Program.Menu.Item("UseEFarm").GetValue<bool>() && E.IsReady() && Q.IsReady() && allMinionsQ.Count > 2 && MPercent >= FMana)
                {
                    E.Cast();
                    Q.CastOnUnit(vMinion);
                }
            }
        }

        private static void JungleFarm()
        {
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                if (Program.Menu.Item("setW").GetValue<StringList>().SelectedIndex == 0 && W.IsReady() && Program.Menu.Item("UseWJFarm").GetValue<bool>())
                {
                    W.Cast();
                }
                if (Q.IsReady() && Program.Menu.Item("UseQJFarm").GetValue<bool>())
                {
                    Q.CastOnUnit(mobs[0]);
                }
                if (E.IsReady() && Program.Menu.Item("UseEJFarm").GetValue<bool>())
                {
                    E.Cast();
                }
            }
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base vTarget, InterruptableSpell args)
        {
            if (!Program.Menu.Item("stun").GetValue<bool>())
                return;

            if (ObjectManager.Player.Distance(vTarget) < Q.Range - 50)
            {
                E.Cast();
                Q.Cast(vTarget);
            }
        }

        private static InventorySlot GetInventorySlot(int ID)
        {
            return ObjectManager.Player.InventoryItems.FirstOrDefault(item => (item.Id == (ItemId)ID && item.Stacks >= 1) || (item.Id == (ItemId)ID && item.Charges >= 1));
        }        

        private static void AutoSmite()
        {
            if (Program.Menu.Item("AutoSmite").GetValue<KeyBind>().Active)
            {
                float[] SmiteDmg = { 20 * ObjectManager.Player.Level + 370, 30 * ObjectManager.Player.Level + 330, 40 * ObjectManager.Player.Level + 240, 50 * ObjectManager.Player.Level + 100 };
                string[] MonsterNames = { "LizardElder", "AncientGolem", "Worm", "Dragon" };
                string[] Monstersteal = { "Worm", "Dragon" };
                var vMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 350 + ObjectManager.Player.SummonerSpellbook.Spells.FirstOrDefault(
                    spell => spell.Name.Contains("smite")).SData.CastRange[0], MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
                foreach (var vMinion in vMinions)
                {
                    if (vMinion != null
                        && !vMinion.IsDead
                        && !ObjectManager.Player.IsDead
                        && !ObjectManager.Player.IsStunned
                        && SmiteSlot != SpellSlot.Unknown
                        && ObjectManager.Player.SummonerSpellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                    {
                        if ((vMinion.Health < SmiteDmg.Max()) && (MonsterNames.Any(name => vMinion.BaseSkinName.StartsWith(name))))
                        {
                            if (Program.Menu.Item("WardJumpSmite").GetValue<bool>())
                            {
                                if (ObjectManager.Player.Distance(vMinion) > 720 && (Monstersteal.Any(name => vMinion.BaseSkinName.StartsWith(name))))
                                {
                                    wJumper.wardJump(Game.CursorPos.To2D());
                                }
                            }

                            ObjectManager.Player.SummonerSpellbook.CastSpell(SmiteSlot, vMinion);

                            if (Program.Menu.Item("laugh").GetValue<bool>())
                            {
                                Game.Say("/l");
                            }

                        }
                    }
                }
            }
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
