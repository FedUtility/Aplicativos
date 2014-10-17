using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LX_Orbwalker;
using Color = System.Drawing.Color;

namespace FedAllChampionsUtility
{
	class Thresh : Champion
	{
		public Spell Q;
		public Spell W;
		public Spell E;
		public Spell R;

		public int QFollowTick = 0;
		public const int QFollowTime = 5000;
        public Thresh()
        {
			LoadMenu();
			LoadSpells();

            Game.OnGameUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

			PluginLoaded();
		}

		private void LoadMenu()
		{
            Program.Menu.AddSubMenu(new Menu("Combo", ObjectManager.Player.ChampionName + "Combo"));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Combo").AddItem(new MenuItem("sep0", "====== Combo"));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Combo").AddItem(new MenuItem("useQ_Combo", "= Use Q").SetValue(true));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Combo").AddItem(new MenuItem("useQ_Combo_follow", "= Follow Q").SetValue(true));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Combo").AddItem(new MenuItem("useW_Combo_Shield", "= Use W Shield at x%").SetValue(new Slider(40, 100, 0)));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Combo").AddItem(new MenuItem("useW_Combo_Engage", "= Use W for Engage").SetValue(true));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Combo").AddItem(new MenuItem("useE_Combo", "= E to Me till % health").SetValue(new Slider(10, 100, 0)));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo_minHit", "= Use R if Hit").SetValue(new Slider(2, 5, 0)));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Combo").AddItem(new MenuItem("sep1", "========="));

            Program.Menu.AddSubMenu(new Menu("Harass", ObjectManager.Player.ChampionName + "Harass"));
            AddManaManager("Harass", 40);
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Harass").AddItem(new MenuItem("sep0", "====== Harass"));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Harass").AddItem(new MenuItem("useQ_Harass", "= Use Q").SetValue(true));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Harass").AddItem(new MenuItem("useW_Harass_safe", "= Use W for SafeFriend").SetValue(true));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Harass").AddItem(new MenuItem("useE_Harass", "= E to Me till % health").SetValue(new Slider(90, 100, 0)));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Harass").AddItem(new MenuItem("sep1", "========="));

            Program.Menu.AddSubMenu(new Menu("LaneClear", ObjectManager.Player.ChampionName + "LaneClear"));
            AddManaManager("LaneClear", 20);
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep0", "====== LaneClear"));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useE_LaneClear", "= Use E").SetValue(true));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep1", "========="));            

            Program.Menu.AddSubMenu(new Menu("Misc", ObjectManager.Player.ChampionName + "Misc"));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Misc").AddItem(new MenuItem("sep0", "====== Misc"));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Misc").AddItem(new MenuItem("useQ_Interrupt", "= Q to Interrupt").SetValue(true));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Misc").AddItem(new MenuItem("useE_Interrupt", "= E to Interrupt").SetValue(true));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Misc").AddItem(new MenuItem("useE_GapCloser", "= E Anti Gapclose").SetValue(true));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Misc").AddItem(new MenuItem("sep1", "========="));

            Program.Menu.AddSubMenu(new Menu("Drawing", ObjectManager.Player.ChampionName + "Drawing"));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Drawing").AddItem(new MenuItem("sep0", "====== Drawing"));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));
            Program.Menu.SubMenu(ObjectManager.Player.ChampionName + "Drawing").AddItem(new MenuItem("sep1", "========="));

		}

		private void LoadSpells()
		{

            Q = new Spell(SpellSlot.Q, 1000);
            Q.SetSkillshot(0.5f, 50f, 1900, true, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 950);

            E = new Spell(SpellSlot.E, 400);

            R = new Spell(SpellSlot.R, 400);
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
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("Draw_E").GetValue<bool>())
                if (E.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("Draw_R").GetValue<bool>())
                if (R.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Program.Menu.Item("useE_GapCloser").GetValue<bool>())
                return;
            if (!(gapcloser.End.Distance(ObjectManager.Player.ServerPosition) <= 100))
                return;
            E.CastIfHitchanceEquals(gapcloser.Sender, HitChance.Medium, Packets());
        }

        private void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (Program.Menu.Item("useE_Interrupt").GetValue<bool>())
            {
                E.CastIfHitchanceEquals(unit, HitChance.Medium, Packets());
                return;
            }
            if (!Program.Menu.Item("useQ_Interrupt").GetValue<bool>() || Environment.TickCount - QFollowTick <= QFollowTime)
                return;
            if (Q.CastIfHitchanceEquals(unit, HitChance.Medium, Packets()))
                QFollowTick = Environment.TickCount;
        }


        private void OnUpdate(EventArgs args)
        {

            switch (LXOrbwalker.CurrentMode)
            {
                case LXOrbwalker.Mode.Combo:
                    if (Program.Menu.Item("useQ_Combo").GetValue<bool>() && Environment.TickCount - QFollowTick >= QFollowTime)
                        if (Cast_BasicLineSkillshot_Enemy(Q) != null)
                            QFollowTick = Environment.TickCount;
                    if (Program.Menu.Item("useQ_Combo_follow").GetValue<bool>() && QFollowTarget.ShouldCast() && Q.IsReady())
                        Q.Cast();
                    Cast_Shield_onFriend(W, Program.Menu.Item("useW_Combo_Shield").GetValue<Slider>().Value, true);
                    if (Program.Menu.Item("useW_Combo_Engage").GetValue<bool>())
                        EngageFriendLatern();
                    if (Program.Menu.Item("useE_Combo").GetValue<Slider>().Value > 0)
                        if (ObjectManager.Player.Health / ObjectManager.Player.MaxHealth * 100 > Program.Menu.Item("useE_Combo").GetValue<Slider>().Value)
                            Cast_E("ToMe");
                        else
                            Cast_E();
                    if (Program.Menu.Item("useR_Combo_minHit").GetValue<Slider>().Value >= 1)
                        if (EnemysinRange(R.Range, Program.Menu.Item("useR_Combo_minHit").GetValue<Slider>().Value))
                            R.Cast();
                    break;
                case LXOrbwalker.Mode.Harass:
                    if (Program.Menu.Item("useQ_Harass").GetValue<bool>() && Environment.TickCount - QFollowTick >= QFollowTime)
                        if (Cast_BasicLineSkillshot_Enemy(Q) != null)
                            QFollowTick = Environment.TickCount;
                    if (Program.Menu.Item("useW_Harass_safe").GetValue<bool>())
                        SafeFriendLatern();
                    break;
                case LXOrbwalker.Mode.LaneClear:
                    if (Program.Menu.Item("useE_LaneClear").GetValue<bool>())
                        Cast_BasicLineSkillshot_AOE_Farm(E);
                    break;
            }
        }

        private void Cast_E(string mode = "")
        {
            if (!E.IsReady())
                return;
            var target = SimpleTs.GetTarget(E.Range - 10, SimpleTs.DamageType.Magical);
            if (target == null)
                return;
            E.Cast(mode == "ToMe" ? GetReversePosition(ObjectManager.Player.Position, target.Position) : target.Position, Packets());
        }

        private void SafeFriendLatern()
        {
            if (!W.IsReady())
                return;
            var bestcastposition = new Vector3(0f, 0f, 0f);
            foreach (
                var friend in
                    Program.AllHerosFriend
                        .Where(
                            hero =>
                                hero.Distance(ObjectManager.Player) <= W.Range - 200 && hero.Health / hero.MaxHealth * 100 <= 20 && !hero.IsDead))
            {
                foreach (var enemy in Program.AllHerosEnemy)
                {
                    if (friend == null)
                        continue;
                    var center = ObjectManager.Player.Position;
                    const int points = 36;
                    var radius = W.Range;

                    const double slice = 2 * Math.PI / points;
                    for (var i = 0; i < points; i++)
                    {
                        var angle = slice * i;
                        var newX = (int)(center.X + radius * Math.Cos(angle));
                        var newY = (int)(center.Y + radius * Math.Sin(angle));
                        var p = new Vector3(newX, newY, 0);
                        if (p.Distance(friend.Position) <= bestcastposition.Distance(friend.Position))
                            bestcastposition = p;
                    }
                    if (friend.Distance(ObjectManager.Player) <= W.Range)
                    {
                        W.Cast(W.GetPrediction(friend).CastPosition, Packets());
                        return;
                    }
                }
                if (bestcastposition.Distance(new Vector3(0f, 0f, 0f)) >= 100)
                    W.Cast(bestcastposition, Packets());
            }
        }

        private void EngageFriendLatern()
        {
            if (!W.IsReady())
                return;
            var bestcastposition = new Vector3(0f, 0f, 0f);
            foreach (var friend in Program.AllHerosFriend.Where(hero => !hero.IsMe && hero.Distance(ObjectManager.Player) <= W.Range + 300 && hero.Distance(ObjectManager.Player) <= W.Range - 300 && hero.Health / hero.MaxHealth * 100 >= 20 && EnemysinRange(150)))
            {
                var center = ObjectManager.Player.Position;
                const int points = 36;
                var radius = W.Range;

                const double slice = 2 * Math.PI / points;
                for (var i = 0; i < points; i++)
                {
                    var angle = slice * i;
                    var newX = (int)(center.X + radius * Math.Cos(angle));
                    var newY = (int)(center.Y + radius * Math.Sin(angle));
                    var p = new Vector3(newX, newY, 0);
                    if (p.Distance(friend.Position) <= bestcastposition.Distance(friend.Position))
                        bestcastposition = friend.Position;
                }
                if (!(friend.Distance(ObjectManager.Player) <= W.Range))
                    continue;
                W.Cast(bestcastposition, Packets());
                return;
            }
            if (bestcastposition.Distance(new Vector3(0f, 0f, 0f)) >= 100)
                W.Cast(bestcastposition, Packets());
        }

        internal class QFollowTarget
        {
            public static bool Exist()
            {
                return ObjectManager.Get<Obj_AI_Base>().Any(unit => unit.HasBuff("ThreshQ") && !unit.IsMe);
            }

            public static Obj_AI_Base Get()
            {
                return ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(unit => unit.HasBuff("ThreshQ") && !unit.IsMe);
            }

            public bool InTower()
            {
                return IsInsideEnemyTower(Get().Position);
            }

            public static bool ShouldCast()
            {
                if (!Exist())
                    return false;
                var buff = Get().Buffs.FirstOrDefault(buf => buf.Name == "ThreshQ");
                if (buff == null)
                    return false;
                return buff.EndTime - Game.Time < 0.5;
            }
        }
	}
}
