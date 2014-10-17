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
    class Vayne : Champion
    {        
        public Spell E, QE, Q, R;

        public static string[] interrupt;
        public static string[] notarget;
        public static string[] gapcloser;

        public Vayne()
        {
            LoadMenu();
            LoadSpells();

            Drawing.OnDraw += Drawing_OnDraw;            
            Game.OnGameUpdate += Game_OnGameUpdate;
            LXOrbwalker.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

            PluginLoaded();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 300f);
            E = new Spell(SpellSlot.E, 550f);
            QE = new Spell(SpellSlot.Q, 800f);
            R = new Spell(SpellSlot.R, 0f);

            E.SetTargetted(0.25f, 2200f);
            QE.SetTargetted(0.50f, 2200f);
            
        } 
        
        private void LoadMenu()
         {
             Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
             Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseQC", "Use Q").SetValue(true));
             Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseEC", "Use E").SetValue(true));
             Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseQEC", "Use QE if not in E range").SetValue(true));
             Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseET", "Use E (Toggle)").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));

             Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
             Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q").SetValue(true));             

             Program.Menu.AddSubMenu(new Menu("Passive", "Passive"));
             Program.Menu.SubMenu("Passive").AddItem(new MenuItem("AntiGP", "Use AntiGapcloser").SetValue(true));
             Program.Menu.SubMenu("Passive").AddItem(new MenuItem("UseEInterrupt", "Interrupt Spells").SetValue(true));
             Program.Menu.SubMenu("Passive").AddItem(new MenuItem("QDefense", "Use Q Defensive").SetValue(true));
             Program.Menu.SubMenu("Passive").AddItem(new MenuItem("AdvE", "Test Advense E").SetValue(false)); 
             Program.Menu.SubMenu("Passive").AddItem(new MenuItem("PushDistance", "E Push Distance").SetValue(new Slider(450, 475, 400)));
             Program.Menu.SubMenu("Passive").AddItem(new MenuItem("AutoR", "Auto R").SetValue(true));
             Program.Menu.SubMenu("Passive").AddItem(new MenuItem("MinR", "Min Enemys for (R)").SetValue<Slider>(new Slider(3, 1, 5)));

             Program.Menu.AddSubMenu(new Menu("Use Items", "Items"));
             Program.Menu.SubMenu("Items").AddItem(new MenuItem("Botrk", "Use BOTRK").SetValue(true));
             Program.Menu.SubMenu("Items").AddItem(new MenuItem("Youmuu", "Use Youmuu").SetValue(true));
             Program.Menu.SubMenu("Items").AddItem(new MenuItem("OwnHPercBotrk", "Min Own HP % Botrk").SetValue(new Slider(50, 1, 100)));
             Program.Menu.SubMenu("Items").AddItem(new MenuItem("EnHPercBotrk", "Min Enemy HP % Botrk").SetValue(new Slider(50, 1, 100)));
             Program.Menu.SubMenu("Items").AddItem(new MenuItem("ItInMix", "Use Items In Harass").SetValue(false));

             Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
             Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
             Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q + Attack").SetValue(true));             
             Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(false));

             Program.Menu.AddSubMenu(new Menu("Gapcloser", "gap"));
             Program.Menu.AddSubMenu(new Menu("Gapcloser 2", "gap2"));
             Program.Menu.AddSubMenu(new Menu("Interrupts", "int"));
             GPIntmenuCreate();
         }

        public void Game_OnGameUpdate(EventArgs args)
        {
            var normalTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            var qeTarget = SimpleTs.GetTarget(200 + E.Range, SimpleTs.DamageType.Physical);            

            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo || Program.Menu.Item("UseET").GetValue<KeyBind>().Active)
            {               

                if (normalTarget.IsValid)
                {
                    if (IsStunable(normalTarget) && E.IsReady())
                    {
                        E.Cast(normalTarget, Packets());
                    }
                }
                else if (qeTarget.IsValid)
                {
                    if (!IsStunable(qeTarget) || !Program.Menu.Item("UseQEC").GetValue<bool>() || !Q.IsReady() || !E.IsReady())
                    {
                        return;
                    }
                    Q.Cast(qeTarget.ServerPosition, Packets());
                    Utility.DelayAction.Add(250, () => E.Cast(qeTarget, Packets()));
                }
            }

            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo && Program.Menu.Item("AutoR").GetValue<bool>())
            {
                AutoUlt();
            }

            if (Program.Menu.Item("QDefense").GetValue<bool>())
            {
                QDefense();
            }

            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo || (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Harass && Program.Menu.Item("ItInMix").GetValue<bool>()))
            {
                useItems(normalTarget);
            }
        }

        private void useItems(Obj_AI_Hero tar)
        {
            float OwnH = getPlHPer();
            if (Program.Menu.Item("Botrk").GetValue<bool>() && (Program.Menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= OwnH) && ((Program.Menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= getEnH(tar))))
            {
                useItem(3144, tar);
                useItem(3153, tar);
            }
            if (Program.Menu.Item("Youmuu").GetValue<bool>())
            {
                useItem(3142);
            }
        }

        private void AutoUlt()
        {
            int inimigos = Utility.CountEnemysInRange(650);            

            if (Program.Menu.Item("MinR").GetValue<Slider>().Value <= inimigos)
            {
                R.Cast();
            }

        }        

        private void QDefense()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsValidTarget(E.Range) && enemy.Distance(ObjectManager.Player) <= enemy.BoundingRadius + enemy.AttackRange + ObjectManager.Player.BoundingRadius && enemy.IsMelee() && !enemy.IsDead)
                {
                    var pos = ObjectManager.Player.ServerPosition.To2D().Extend(enemy.Position.To2D(), -300).To3D();
                    Q.Cast(pos, Packets());                    
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Program.Menu.Item("Draw_Disabled").GetValue<bool>())
                return;
            
            var myRange = LXOrbwalker.GetAutoAttackRange(null);
            
            if (Program.Menu.Item("Draw_Q").GetValue<bool>())
                if (Q.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range + myRange + 65, Q.IsReady() ? Color.Green : Color.Red);            

            if (Program.Menu.Item("Draw_E").GetValue<bool>())
                if (E.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range+107, E.IsReady() ? Color.Green : Color.Red);
            
        }        
        public void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (unit.IsMe && (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo && Program.Menu.Item("UseQC").GetValue<bool>() ||
                              LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Harass && Program.Menu.Item("UseQH").GetValue<bool>()))
            {
                CastTumble((Obj_AI_Hero)target);
            }
        }
        public bool IsStunable(Obj_AI_Hero enemy)
        {
            var prediction =
                Vector3.DistanceSquared(enemy.ServerPosition, ObjectManager.Player.ServerPosition) > 550 * 550
                    ? QE.GetPrediction(enemy)
                    : E.GetPrediction(enemy);

            if (prediction.Hitchance.Equals(HitChance.OutOfRange))
            {
                return false;
            }

            for (var i = 0; i < Program.Menu.Item("PushDistance").GetValue<Slider>().Value; i += (int)enemy.BoundingRadius)
            {
                if (Program.Menu.Item("AdvE").GetValue<bool>())
                {
                    if (
                        NavMesh.GetCollisionFlags(
                            prediction.UnitPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -i).To3D())
                            .HasFlag(CollisionFlags.Wall) ||
                        NavMesh.GetCollisionFlags(
                            prediction.UnitPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -i).To3D())
                            .HasFlag(CollisionFlags.Building) ||
                        NavMesh.GetCollisionFlags(
                            prediction.UnitPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -i).To3D())
                            .HasFlag(CollisionFlags.Prop))
                    {
                        return true;
                    }
                }
                else
                {
                    if (
                        NavMesh.GetCollisionFlags(
                            prediction.UnitPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -i).To3D())
                            .HasFlag(CollisionFlags.Wall))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void CastTumble(Obj_AI_Hero target)
        {
            var posAfterTumble =
                ObjectManager.Player.ServerPosition.To2D().Extend(Game.CursorPos.To2D(), 300).To3D();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if (distanceAfterTumble < 550 * 550 && distanceAfterTumble > 100 * 100)
            {
                Q.Cast(Game.CursorPos, Packets());
            }
        }
        private void GPIntmenuCreate()
        {
            gapcloser = new[]
            {
                "AkaliShadowDance", "Headbutt", "DianaTeleport", "IreliaGatotsu", "JaxLeapStrike", "JayceToTheSkies",
                "MaokaiUnstableGrowth", "MonkeyKingNimbus", "Pantheon_LeapBash", "PoppyHeroicCharge", "QuinnE",
                "XenZhaoSweep", "blindmonkqtwo", "FizzPiercingStrike", "RengarLeap"
            };
            notarget = new[]
            {
                "AatroxQ", "GragasE", "GravesMove", "HecarimUlt", "JarvanIVDragonStrike", "JarvanIVCataclysm", "KhazixE",
                "khazixelong", "LeblancSlide", "LeblancSlideM", "LeonaZenithBlade", "UFSlash", "RenektonSliceAndDice",
                "SejuaniArcticAssault", "ShenShadowDash", "RocketJump", "slashCast"
            };
            interrupt = new[]
            {
                "KatarinaR", "GalioIdolOfDurand", "Crowstorm", "Drain", "AbsoluteZero", "ShenStandUnited", "UrgotSwap2",
                "AlZaharNetherGrasp", "FallenOne", "Pantheon_GrandSkyfall_Jump", "VarusQ", "CaitlynAceintheHole",
                "MissFortuneBulletTime", "InfiniteDuress", "LucianR"
            };
            for (int i = 0; i < gapcloser.Length; i++)
            {
                Program.Menu.SubMenu("gap").AddItem(new MenuItem(gapcloser[i], gapcloser[i])).SetValue(true);
            }
            for (int i = 0; i < notarget.Length; i++)
            {
                Program.Menu.SubMenu("gap2").AddItem(new MenuItem(notarget[i], notarget[i])).SetValue(true);
            }
            for (int i = 0; i < interrupt.Length; i++)
            {
                Program.Menu.SubMenu("int").AddItem(new MenuItem(interrupt[i], interrupt[i])).SetValue(true);
            }
        }
        private bool isEn(String opt)
        {
            return Program.Menu.Item(opt).GetValue<bool>();
        }
        private void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            String spellName = args.SData.Name;
            //Interrupts
            if (isEn(spellName) && sender.IsValidTarget(550f) && isEn("UseEInterrupt"))
            {
                E.Cast((Obj_AI_Hero)sender, Packets());
            }
            //Targeted GapClosers
            if (isEn(spellName) && sender.IsValidTarget(550f) && isEn("AntiGP") && gapcloser.Any(str => str.Contains(args.SData.Name))
                && args.Target.IsMe)
            {
                E.Cast((Obj_AI_Hero)sender, Packets());
            }
            //NonTargeted GP
            if (isEn(spellName) && sender.IsValidTarget(550f) && isEn("AntiGP") && notarget.Any(str => str.Contains(args.SData.Name))
                && ObjectManager.Player.Distance(args.End) <= 320f)
            {
                E.Cast((Obj_AI_Hero)sender, Packets());
            }
        }
        private void useItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }
        private float getEnH(Obj_AI_Hero target)
        {
            float h = (target.Health / target.MaxHealth) * 100;
            return h;
        }
        private float getPlHPer()
        {
            float h = (ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) * 100;
            return h;
        }
        
    }
}
