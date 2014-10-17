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
    class LeeSin : Champion
    {
        public static Vector2 testSpellCast;
        public static Vector2 testSpellProj;
        public static Obj_AI_Hero Player = ObjectManager.Player;        
        public static Obj_AI_Hero target;

        public static string[] testSpells = { "RelicSmallLantern", "RelicLantern", "SightWard", "wrigglelantern", "ItemGhostWard", "VisionWard",
                                              "BantamTrap", "JackInTheBox","CaitlynYordleTrap", "Bushwhack"};

        public static Spell Q, W, E, R;
        public static Spellbook sBook = Player.Spellbook;
        public static SpellDataInst Qdata = sBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = sBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = sBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = sBook.GetSpell(SpellSlot.R);

        public static Obj_AI_Hero LockedTarget;
        public static float lastwardjump = 0;
        public static bool daInsec = false;        
        public LeeSin()
        {
            LoadMenu();
            LoadSpells();

            Drawing.OnDraw += onDraw;
            Game.OnGameUpdate += OnGameUpdate;            
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

            PluginLoaded();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 375);
            
            Q.SetSkillshot(0.5f, 60f, 1800f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 350f, 0f, false, SkillshotType.SkillshotCircle);
        }

        private void LoadMenu()
        {            
            Program.Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo2", "Combo 2!").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("ActiveHarass", "Harass!").SetValue((new KeyBind("C".ToCharArray()[0], KeyBindType.Press, false))));

            Program.Menu.AddSubMenu(new Menu("Insec", "Insec"));
            Program.Menu.SubMenu("Insec").AddItem(new MenuItem("ActiveInsec", "Insec!").SetValue((new KeyBind("G".ToCharArray()[0], KeyBindType.Press, false))));

            Program.Menu.AddSubMenu(new Menu("KillSteal", "KillSteal"));
            Program.Menu.SubMenu("KillSteal").AddItem(new MenuItem("UseR", "R killsteal")).SetValue(true);

            Program.Menu.AddSubMenu(new Menu("WardJump", "WardJump"));
            Program.Menu.SubMenu("WardJump").AddItem(new MenuItem("ActiveWard", "WardJump!").SetValue((new KeyBind("Z".ToCharArray()[0], KeyBindType.Press, false))));

            Program.Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawInsec", "Draw Insec")).SetValue(true);            
        }

        private void checkLock(Obj_AI_Hero target)
        {
            //if (!target.IsValidTarget())
            //    return;
            if (!Program.Menu.Item("ActiveHarass").GetValue<KeyBind>().Active && LockedTarget != null)
            {
                LockedTarget = null;
            }
            else if (Program.Menu.Item("ActiveCombo").GetValue<KeyBind>().Active)
                LockedTarget = target;
            else if (target.IsValidTarget() && LockedTarget == null || Program.Menu.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
                LockedTarget = target;
            }
        }

        private void OnGameUpdate(EventArgs args)
        {
            loaidraw();
            CastR_kill();
            target = SimpleTs.GetTarget(1500, SimpleTs.DamageType.Physical);
            checkLock(target);
            LXOrbwalker.SetAttack(true);            

            if (Program.Menu.Item("ActiveWard").GetValue<KeyBind>().Active)
            {
                wardJump(Game.CursorPos.To2D());
            }

            if (Program.Menu.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
                doHarass();
            }


            if (Program.Menu.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (Program.Menu.Item("ActiveCombo2").GetValue<KeyBind>().Active)
            {
                Combo2();

            }

            if (Program.Menu.Item("ActiveInsec").GetValue<KeyBind>().Active)
            {
               useinsec();
            }

            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneClear)
            {

            }
        }
        private void Combo()
        {
            castQFirstSmart();
            castQSecondSmart();
            castEFirst();
            castE2();
            castR();
 
        }
        private void Combo2()
        {
            if (inDistance(LockedTarget.Position.To2D(), Player.ServerPosition.To2D(), 375))
            {
                castQFirstSmart();
                castEFirst();
                if (targetHasQ(LockedTarget))
                    R.Cast(LockedTarget);
                if (!R.IsReady())
                    castQSecondSmart();
                castE2();
            }
        }
        private void doHarass()
        {
            if (LockedTarget == null)
                return;

            if (!castQFirstSmart())
                if (!castQSecondSmart())
                    if (!castEFirst())
                        getBackHarass();
        }
        private void useinsec()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsAlly && !hero.IsMe && hero != null && hero.Distance(Player) < 1500)
                {
                    insec1();
                }
                else
                {
                    insec();
                }
            }

        }
        private void insec()
        {
            if (!R.IsReady())
            {
                daInsec = false;
                return;
            }
            try
            {
                if (daInsec && !W.IsReady())
                {
                    R.Cast(LockedTarget);
                }
                if (Player.Distance(getward(LockedTarget)) > 600 && W.IsReady())
                {
                    castQFirstSmart();
                    castQSecondSmart();
                }
                if (Player.Distance(getward(LockedTarget)) <= 600 && W.IsReady())
                {
                    wardJump(getward(LockedTarget).To2D());
                    daInsec = true;
                }
            }
            catch
            {

            }
        }
        private void insec1()
        {
            if (!R.IsReady())
            {
                daInsec = false;
                return;
            }
            try
            {
                if (daInsec && !W.IsReady())
                {
                    R.Cast(LockedTarget);
                }
                if (Player.Distance(getward2(LockedTarget)) > 600 && W.IsReady())
                {
                    castQFirstSmart();
                    castQSecondSmart();
                }
                if (Player.Distance(getward2(LockedTarget)) < 600 && W.IsReady())
                {
                    wardJump(getward2(LockedTarget).To2D());
                    daInsec = true;

                }
            }
            catch
            {

            }
        }
        private bool getBackHarass()
        {
            Obj_AI_Turret closest_tower = ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly).OrderBy(tur => tur.Distance(Player.ServerPosition)).First();
            Obj_AI_Base jumpOn = ObjectManager.Get<Obj_AI_Base>().Where(ally => ally.IsAlly && !(ally is Obj_AI_Turret) && !ally.IsMe && ally.Distance(LeeSin.Player.ServerPosition) < 700).OrderBy(tur => tur.Distance(closest_tower.ServerPosition)).First();
            W.Cast(jumpOn);
            // wardJump(closest_tower.Position.To2D());
            return false;
        }
        private bool castQFirstSmart()
        {
            if (!Q.IsReady() || Qdata.Name != "BlindMonkQOne" || LockedTarget == null)
                return false;

            PredictionOutput predict = Q.GetPrediction(LockedTarget);
            if (predict.Hitchance > HitChance.Low)
            {
                Q.Cast(predict.CastPosition);
                return true;
            }
            return true;
        }
        private bool castQSecondSmart()
        {
            if (Qdata.Name != "blindmonkqtwo" || LockedTarget == null)
                return false;
            if (targetHasQ(LockedTarget) && inDistance(LockedTarget.Position.To2D(), Player.ServerPosition.To2D(), 1200))
            {
                Q.Cast();
                return true;
            }
            return true;
        }
        private bool targetHasQ(Obj_AI_Hero target)
        {
            foreach (BuffInstance buf in target.Buffs)
            {
                if (buf.Name == "BlindMonkQOne" || buf.Name == "blindmonkqonechaos")
                    return true;
            }
            return false;            
        }
        private bool targetHasUlti(Obj_AI_Hero target)
        {
            foreach (BuffInstance buf in target.Buffs)
            {
                if (buf.Name == "JudicatorIntervention" || buf.Name == "UndyingRage")
                    return false;
            }
            return true;            
        }
        private bool castEFirst()
        {
            if (!E.IsReady() || LockedTarget == null || Edata.Name != "BlindMonkEOne")
                return false;
            if (inDistance(LockedTarget.Position.To2D(), Player.ServerPosition.To2D(), E.Range))
            {
                E.Cast();
                return true;
            }
            return true;
        }
        private bool castE2()
        {
            if (LockedTarget == null) return false;
            if (inDistance(LockedTarget.Position.To2D(), Player.ServerPosition.To2D(), 350))
            {
                E.Cast();
                return true;
            }
            return true;
        }
        private void castR()
        {
            var target = SimpleTs.GetTarget(375, SimpleTs.DamageType.Physical);
            if (target == null || !R.IsReady() || !Program.Menu.Item("UseRCombo").GetValue<bool>())
                return;
            R.Cast(target);
        }
        private void CastR_kill()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Range) && ObjectManager.Player.GetSpellDamage(hero, SpellSlot.R) >= hero.Health))
            {
                if (targetHasUlti(LockedTarget))
                    R.Cast(enemy);
                return;
            }
        }
        private void castRKill()
        {
            if (!Program.Menu.Item("UseRcombo").GetValue<bool>())
                return;
            if (!(LockedTarget != null))
                return;
            if (ObjectManager.Player.GetSpellDamage(LockedTarget, SpellSlot.R) <= LockedTarget.Health)
                return;
            if (!R.IsReady())
                return;
            R.Cast(LockedTarget);
        }
        private int getJumpWardId()
        {
            int[] wardIds = { 3340, 3350, 3205, 3207, 2049, 2045, 2044, 3361, 3154, 3362, 3160, 2043 };
            foreach (int id in wardIds)
            {
                if (Items.HasItem(id) && Items.CanUseItem(id))
                    return id;
            }
            return -1;
        }
        private void moveTo(Vector2 Pos)
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Pos.To3D());
        }
        private void wardJump(Vector2 pos)
        {
            Vector2 posStart = pos;
            if (!W.IsReady())
                return;
            bool wardIs = false;
            if (!inDistance(pos, Player.ServerPosition.To2D(), W.Range + 15))
            {
                pos = Player.ServerPosition.To2D() + Vector2.Normalize(pos - Player.ServerPosition.To2D()) * 600;
            }

            if (!W.IsReady() && W.ChargedSpellName == "")
                return;
            foreach (Obj_AI_Base ally in ObjectManager.Get<Obj_AI_Base>().Where(ally => ally.IsAlly
                && !(ally is Obj_AI_Turret) && inDistance(pos, ally.ServerPosition.To2D(), 200)))
            {
                wardIs = true;
                moveTo(pos);
                if (inDistance(Player.ServerPosition.To2D(), ally.ServerPosition.To2D(), W.Range + ally.BoundingRadius))
                {
                    W.Cast(ally);

                }
                return;
            }
            Polygon pol;
            if ((pol = Program.map.getInWhichPolygon(pos)) != null)
            {
                if (inDistance(pol.getProjOnPolygon(pos), Player.ServerPosition.To2D(), W.Range + 15) && !wardIs && inDistance(pol.getProjOnPolygon(pos), pos, 200))
                {
                    if (lastwardjump < Environment.TickCount)
                    {
                        putWard(pos);
                        lastwardjump = Environment.TickCount + 1000;
                    }
                }
            }
            else if (!wardIs)
            {
                if (lastwardjump < Environment.TickCount)
                {
                    putWard(pos);
                    lastwardjump = Environment.TickCount + 1000;
                }
            }

        }
        private bool putWard(Vector2 pos)
        {
            int wardItem;
            if ((wardItem = getJumpWardId()) != -1)
            {
                foreach (var slot in Player.InventoryItems.Where(slot => slot.Id == (ItemId)wardItem))
                {
                    slot.UseItem(pos.To3D());
                    return true;
                }
            }
            return false;
        }
        private bool inDistance(Vector2 pos1, Vector2 pos2, float distance)
        {
            float dist2 = Vector2.DistanceSquared(pos1, pos2);
            return (dist2 <= distance * distance) ? true : false;
        }
        private Vector3 getward(Obj_AI_Hero target)
        {
            Obj_AI_Turret turret = ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly && tur.Health > 0 && !tur.IsMe).OrderBy(tur => tur.Distance(Player.ServerPosition)).First();
            return target.ServerPosition + Vector3.Normalize(turret.ServerPosition - target.ServerPosition) * (-300);
        }
        private Vector3 getward2(Obj_AI_Hero target)
        {
            Obj_AI_Hero hero = ObjectManager.Get<Obj_AI_Hero>().Where(tur => tur.IsAlly && tur.Health > 0 && !tur.IsMe).OrderBy(tur => tur.Distance(Player.ServerPosition)).First();
            return target.ServerPosition + Vector3.Normalize(hero.ServerPosition - target.ServerPosition) * (-300);
        }
        private Vector3 getward1(Obj_AI_Hero target)
        {
            Obj_AI_Turret turret = ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly && tur.Health > 0 && !tur.IsMe).OrderBy(tur => tur.Distance(Player.ServerPosition)).First();
            return target.Position + Vector3.Normalize(turret.Position - target.Position) * (600);
        }
        private Vector3 getward3(Obj_AI_Hero target)
        {
            Obj_AI_Hero hero = ObjectManager.Get<Obj_AI_Hero>().Where(tur => tur.IsAlly && tur.Health > 0 && !tur.IsMe).OrderBy(tur => tur.Distance(Player.ServerPosition)).First();
            return target.Position + Vector3.Normalize(hero.Position - target.Position) * (600);
        }
        private bool loaidraw()
        {
            foreach (Obj_AI_Hero hero1 in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero1.IsAlly && !hero1.IsMe && hero1 != null && hero1.Distance(Player) < 1500)
                    return true;
            }
            return false;
        }
        private void onDraw(EventArgs args)
        {
            if (Program.Menu.Item("DrawQ").GetValue<bool>())
            {
                if (Q.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, 1100, Q.IsReady() ? Color.Green : Color.Red);
            }
            if (Program.Menu.Item("DrawW").GetValue<bool>())
            {
                if (W.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, 700, W.IsReady() ? Color.Green : Color.Red);
            }
            if (Program.Menu.Item("DrawE").GetValue<bool>())
            {
                if (W.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, 350, E.IsReady() ? Color.Green : Color.Red);
            }
            if (Program.Menu.Item("DrawR").GetValue<bool>())
            {
                if (R.Level > 0)
                Utility.DrawCircle(ObjectManager.Player.Position, 375, R.IsReady() ? Color.Green : Color.Red);
            }
            if (Program.Menu.Item("DrawInsec").GetValue<bool>() && LeeSin.R.IsReady())
            {
                if (!loaidraw())
                {
                    Vector2 heroPos = Drawing.WorldToScreen(LockedTarget.Position);
                    Vector2 diempos = Drawing.WorldToScreen(getward1(LockedTarget));
                    Drawing.DrawLine(heroPos[0], heroPos[1], diempos[0], diempos[1], 1, System.Drawing.Color.White);
                }
                else
                {
                    Vector2 heroPos = Drawing.WorldToScreen(LockedTarget.Position);
                    Vector2 diempos = Drawing.WorldToScreen(getward3(LockedTarget));
                    Drawing.DrawLine(heroPos[0], heroPos[1], diempos[0], diempos[1], 1, System.Drawing.Color.White);
                }
            }
        }
        private void OnProcessSpell(LeagueSharp.Obj_AI_Base obj, LeagueSharp.GameObjectProcessSpellCastEventArgs arg)
        {
            if (testSpells.ToList().Contains(arg.SData.Name))
            {
                testSpellCast = arg.End.To2D();
                Polygon pol;
                if ((pol = Program.map.getInWhichPolygon(arg.End.To2D())) != null)
                {
                    testSpellProj = pol.getProjOnPolygon(arg.End.To2D());
                }
            }
        }
    }
}
