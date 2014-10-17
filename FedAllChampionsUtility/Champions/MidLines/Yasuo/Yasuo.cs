using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Evade;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;
using LX_Orbwalker;
using Geometry = LeagueSharp.Common.Geometry;
using Color = System.Drawing.Color;

namespace FedAllChampionsUtility
{
    class Yasuo : Champion
    { 
        public static Menu skillShotMenu;


        public static string lastSpell = "";

        public static int afterDash = 0;

        public static bool canSave = true;
        public static bool canExport = true;
        public static bool canDelete = true;


        public static List<Skillshot> DetectedSkillshots = new List<Skillshot>();

        public struct IsSafeResult
        {
            public bool IsSafe;
            public List<Skillshot> SkillshotList;
            public List<Obj_AI_Base> casters;
        }

        internal class YasDash
        {
            public Vector3 from = new Vector3(-1, -1, -1);
            public Vector3 to = new Vector3(-1, -1, -1);

            public YasDash()
            {
                from = new Vector3(-1, -1, -1);
                to = new Vector3(-1, -1, -1);
            }

            public YasDash(Vector3 fromV, Vector3 toV)
            {
                from = fromV;
                to = toV;
            }

            public YasDash(YasDash dash)
            {
                from = dash.from;
                to = dash.to;
            }

        }

        internal class YasWall
        {
            public Obj_SpellLineMissile pointL;
            public Obj_SpellLineMissile pointR;
            public float endtime = 0;
            public YasWall()
            {

            }

            public YasWall(Obj_SpellLineMissile L, Obj_SpellLineMissile R)
            {
                pointL = L;
                pointR = R;
                endtime = Game.Time + 4;
            }

            public void setR(Obj_SpellLineMissile R)
            {
                pointR = R;
                endtime = Game.Time + 4;
            }

            public void setL(Obj_SpellLineMissile L)
            {
                pointL = L;
                endtime = Game.Time + 4;
            }
        }

        public static List<YasDash> dashes = new List<YasDash>();

        public static YasDash lastDash = new YasDash();

        public static Obj_AI_Hero Player = ObjectManager.Player;

        public static Vector3 test = new Vector3();

        public static Spellbook sBook = Player.Spellbook;

        public static SpellDataInst Qdata = sBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = sBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = sBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = sBook.GetSpell(SpellSlot.R);
        public static Spell Q = new Spell(SpellSlot.Q, 475);
        public static Spell QEmp = new Spell(SpellSlot.Q, 1000);
        public static Spell QCir = new Spell(SpellSlot.Q, 305);
        public static Spell W = new Spell(SpellSlot.W, 400);
        public static Spell E = new Spell(SpellSlot.E, 475);
        public static Spell R = new Spell(SpellSlot.R, 1200);
        //Much Skillshot                    1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8
        public static Spell[] levelUpSeq = { Q, E, W, Q, Q, R, Q, E, Q, E, R, E, W, E, W, R, W, W };

        //Much NotSoMuch                    1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8
        public static Spell[] levelUpSeq2 = { Q, E, Q, W, Q, R, Q, E, Q, E, R, E, W, E, W, R, W, W };

        public static Vector3 point1 = new Vector3();
        public static Vector3 point2 = new Vector3();

        public static Vector3 castFrom;
        public static bool isDashigPro = false;
        public static float startDash = 0;
        public static float time = 0;

        public static YasWall wall = new YasWall();

        public static JungleTimers jTimers;

        public static SummonerItems sumItems;

        #region WallDashing

        public static void setDashes()
        {
            #region WallDashingValues
            dashes.Add(new YasDash(new Vector3(5997.00f, 5065.00f, 51.67f), new Vector3(6447.35f, 5216.45f, 56.11f)));
            dashes.Add(new YasDash(new Vector3(6897.00f, 5665.00f, 55.66f), new Vector3(6659.32f, 5285.89f, 58.84f)));
            dashes.Add(new YasDash(new Vector3(3847.00f, 5965.00f, 55.13f), new Vector3(3477.00f, 6263.00f, 55.61f)));
            dashes.Add(new YasDash(new Vector3(3197.00f, 6815.00f, 53.86f), new Vector3(3328.71f, 6366.87f, 55.61f)));
            dashes.Add(new YasDash(new Vector3(6615.00f, 5197.00f, 56.40f), new Vector3(6885.00f, 5761.00f, 55.60f)));
            dashes.Add(new YasDash(new Vector3(3435.00f, 6267.00f, 55.61f), new Vector3(4003.00f, 6007.00f, 54.55f)));
            dashes.Add(new YasDash(new Vector3(3353.00f, 6319.00f, 55.61f), new Vector3(3141.00f, 6745.00f, 53.93f)));
            dashes.Add(new YasDash(new Vector3(6511.00f, 5233.00f, 57.02f), new Vector3(5972.25f, 5084.35f, 51.67f)));
            dashes.Add(new YasDash(new Vector3(5997.00f, 5065.00f, 51.67f), new Vector3(6447.35f, 5216.45f, 56.11f)));
            dashes.Add(new YasDash(new Vector3(6897.00f, 5665.00f, 55.66f), new Vector3(6659.32f, 5285.89f, 58.84f)));
            dashes.Add(new YasDash(new Vector3(3847.00f, 5965.00f, 55.13f), new Vector3(3477.00f, 6263.00f, 55.61f)));
            dashes.Add(new YasDash(new Vector3(3197.00f, 6815.00f, 53.86f), new Vector3(3328.71f, 6366.87f, 55.61f)));
            dashes.Add(new YasDash(new Vector3(6615.00f, 5197.00f, 56.40f), new Vector3(6885.00f, 5761.00f, 55.60f)));
            dashes.Add(new YasDash(new Vector3(3435.00f, 6267.00f, 55.61f), new Vector3(4003.00f, 6007.00f, 54.55f)));
            dashes.Add(new YasDash(new Vector3(3353.00f, 6319.00f, 55.61f), new Vector3(3141.00f, 6745.00f, 53.93f)));
            dashes.Add(new YasDash(new Vector3(6511.00f, 5233.00f, 57.02f), new Vector3(5972.25f, 5084.35f, 51.67f)));
            dashes.Add(new YasDash(new Vector3(7319.51f, 7394.79f, 54.01f), new Vector3(7709.69f, 7649.68f, 53.35f)));
            dashes.Add(new YasDash(new Vector3(3423.00f, 7719.00f, 55.06f), new Vector3(3541.61f, 7204.61f, 54.48f)));
            dashes.Add(new YasDash(new Vector3(3469.00f, 7739.00f, 55.01f), new Vector3(3979.00f, 8101.00f, 51.94f)));
            dashes.Add(new YasDash(new Vector3(3579.00f, 7217.00f, 54.39f), new Vector3(3452.67f, 7586.14f, 55.04f)));
            dashes.Add(new YasDash(new Vector3(4027.00f, 7829.00f, 51.99f), new Vector3(3617.00f, 7589.00f, 54.40f)));
            dashes.Add(new YasDash(new Vector3(1789.00f, 8209.00f, 54.92f), new Vector3(1241.82f, 8198.85f, 50.63f)));
            dashes.Add(new YasDash(new Vector3(1297.00f, 8113.00f, 50.86f), new Vector3(1759.00f, 8223.00f, 54.92f)));
            dashes.Add(new YasDash(new Vector3(7901.00f, 2423.00f, 54.28f), new Vector3(7982.06f, 2994.11f, 55.34f)));
            dashes.Add(new YasDash(new Vector3(8213.00f, 2429.00f, 54.28f), new Vector3(8221.72f, 2912.02f, 57.89f)));
            dashes.Add(new YasDash(new Vector3(8245.00f, 2415.00f, 54.28f), new Vector3(8110.77f, 2953.00f, 56.58f)));
            dashes.Add(new YasDash(new Vector3(7259.00f, 3821.00f, 57.42f), new Vector3(7213.00f, 3965.00f, 57.29f)));
            dashes.Add(new YasDash(new Vector3(7795.00f, 2915.00f, 54.70f), new Vector3(7936.29f, 2465.55f, 54.28f)));
            dashes.Add(new YasDash(new Vector3(7423.00f, 3763.00f, 56.91f), new Vector3(7751.41f, 3288.25f, 54.77f)));
            dashes.Add(new YasDash(new Vector3(7293.00f, 3893.00f, 57.30f), new Vector3(6671.00f, 3883.00f, 51.67f)));
            dashes.Add(new YasDash(new Vector3(12311.00f, 6151.00f, 54.84f), new Vector3(12444.78f, 6731.78f, 54.85f)));
            dashes.Add(new YasDash(new Vector3(12233.00f, 6235.00f, 54.86f), new Vector3(12782.00f, 6383.88f, 56.40f)));
            dashes.Add(new YasDash(new Vector3(12267.00f, 6313.00f, 54.83f), new Vector3(12775.00f, 5951.00f, 58.53f)));
            dashes.Add(new YasDash(new Vector3(10445.00f, 7263.00f, 55.32f), new Vector3(10384.54f, 6816.62f, 54.87f)));
            dashes.Add(new YasDash(new Vector3(10701.00f, 8105.00f, 63.34f), new Vector3(10678.48f, 7568.38f, 55.35f)));
            dashes.Add(new YasDash(new Vector3(10529.00f, 8083.00f, 65.67f), new Vector3(9949.00f, 8319.00f, 64.18f)));
            dashes.Add(new YasDash(new Vector3(10595.00f, 6615.00f, 54.87f), new Vector3(9977.00f, 6511.00f, 55.13f)));
            dashes.Add(new YasDash(new Vector3(10645.00f, 7563.00f, 55.35f), new Vector3(10702.72f, 8030.58f, 64.66f)));
            dashes.Add(new YasDash(new Vector3(12745.00f, 6215.00f, 57.56f), new Vector3(12277.06f, 6270.45f, 54.84f)));
            dashes.Add(new YasDash(new Vector3(10545.00f, 6765.00f, 54.87f), new Vector3(10861.00f, 7303.00f, 55.35f)));
            dashes.Add(new YasDash(new Vector3(10695.00f, 7263.00f, 55.35f), new Vector3(10577.00f, 6803.00f, 54.87f)));
            dashes.Add(new YasDash(new Vector3(7411.00f, 9301.00f, 55.46f), new Vector3(7003.00f, 8827.00f, 56.02f)));
            dashes.Add(new YasDash(new Vector3(7535.00f, 9207.00f, 55.51f), new Vector3(7987.00f, 9637.00f, 53.53f)));
            dashes.Add(new YasDash(new Vector3(7923.00f, 9409.00f, 53.53f), new Vector3(7491.00f, 9207.00f, 55.51f)));
            dashes.Add(new YasDash(new Vector3(6197.00f, 11513.00f, 54.63f), new Vector3(6135.00f, 11983.00f, 39.60f)));
            dashes.Add(new YasDash(new Vector3(6561.00f, 10737.00f, 54.64f), new Vector3(6083.00f, 11137.00f, 54.36f)));
            dashes.Add(new YasDash(new Vector3(6147.00f, 12013.00f, 39.61f), new Vector3(6101.00f, 11389.00f, 54.62f)));
            dashes.Add(new YasDash(new Vector3(5797.00f, 11963.00f, 39.60f), new Vector3(6239.00f, 11521.00f, 54.63f)));
            dashes.Add(new YasDash(new Vector3(6577.00f, 10715.00f, 54.64f), new Vector3(6123.00f, 11143.00f, 54.42f)));
            dashes.Add(new YasDash(new Vector3(5843.00f, 12025.00f, 39.62f), new Vector3(5867.00f, 11399.00f, 54.00f)));
            dashes.Add(new YasDash(new Vector3(6581.00f, 10761.00f, 54.64f), new Vector3(6034.87f, 10917.85f, 53.88f)));
            dashes.Add(new YasDash(new Vector3(5797.00f, 11963.00f, 39.60f), new Vector3(6182.85f, 11577.15f, 54.63f)));
            dashes.Add(new YasDash(new Vector3(6085.00f, 11993.00f, 39.61f), new Vector3(6397.00f, 11529.00f, 54.63f)));
            dashes.Add(new YasDash(new Vector3(5801.00f, 11975.00f, 39.60f), new Vector3(6150.74f, 11506.03f, 54.63f)));
            dashes.Add(new YasDash(new Vector3(6073.00f, 12013.00f, 39.61f), new Vector3(6359.35f, 11537.81f, 54.63f)));
            dashes.Add(new YasDash(new Vector3(7195.00f, 8713.00f, 56.02f), new Vector3(7461.15f, 9067.39f, 55.60f)));
            dashes.Add(new YasDash(new Vector3(10605.00f, 6609.00f, 54.86f), new Vector3(9981.00f, 6565.00f, 55.12f)));
            dashes.Add(new YasDash(new Vector3(7527.00f, 9203.00f, 55.52f), new Vector3(7991.00f, 9621.00f, 53.53f)));
            dashes.Add(new YasDash(new Vector3(7450.88f, 9144.42f, 55.55f), new Vector3(7985.00f, 8765.50f, 55.07f)));
            #endregion
            jTimers = new JungleTimers();
            sumItems = new SummonerItems(Player);
        }

        public static void setSkillShots()
        {
            Q.SetSkillshot(getNewQSpeed(), 50f, float.MaxValue, false, SkillshotType.SkillshotLine);
            QEmp.SetSkillshot(0.1f, 50f, 1200f, false, SkillshotType.SkillshotLine);
            QCir.SetSkillshot(0f, 375f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        public static float getNewQSpeed()
        {
            float ds = 0.5f;//s
            float a = 1 / ds * Player.AttackSpeedMod;
            return 1 / a;
        }

        public static void doCombo(Obj_AI_Hero target)
        {

            if (target == null) return;
            useHydra(target);
            if (target.Distance(Player) < 500)
            {
                sumItems.cast(SummonerItems.ItemIds.Ghostblade);
            }
            if (target.Distance(Player) < 500 && (Player.Health / Player.MaxHealth) * 100 < 85)
            {
                sumItems.cast(SummonerItems.ItemIds.BotRK, target);

            }
            if (Program.Menu.Item("smartR").GetValue<bool>() && R.IsReady())
                useRSmart();
            if (Program.Menu.Item("smartW").GetValue<bool>())
                putWallBehind(target);
            if (Program.Menu.Item("useEWall").GetValue<bool>())
                eBehindWall(target);

            if (!useESmart(target))
            {
                //Console.WriteLine("test");
                List<Obj_AI_Hero> ignore = new List<Obj_AI_Hero>();
                ignore.Add(target);
                gapCloseE(target.Position.To2D());
            }

            useQSmart(target);
        }

        public static Vector2 getNextPos(Obj_AI_Hero target)
        {
            Vector2 dashPos = target.Position.To2D();
            if (target.IsMoving)
            {
                Vector2 tpos = target.Position.To2D();
                Vector2 path = target.Path[0].To2D() - tpos;
                path.Normalize();
                dashPos = tpos + (path * 100);
            }
            return dashPos;
        }

        public static void putWallBehind(Obj_AI_Hero target)
        {
            if (!W.IsReady() || !E.IsReady() || target.IsMelee())
                return;
            Vector2 dashPos = getNextPos(target);
            PredictionOutput po = Prediction.GetPrediction(target, 0.5f);

            float dist = Player.Distance(po.UnitPosition);
            if (!target.IsMoving || Player.Distance(dashPos) <= dist + 40)
                if (dist < 330 && dist > 100 && W.IsReady())
                    W.Cast(po.UnitPosition);
        }

        public static void eBehindWall(Obj_AI_Hero target)
        {
            if (!E.IsReady() || !enemyIsJumpable(target) || target.IsMelee())
                return;
            float dist = Player.Distance(target);
            var pPos = Player.Position.To2D();
            Vector2 dashPos = target.Position.To2D();
            if (!target.IsMoving || Player.Distance(dashPos) <= dist)
            {
                foreach (Obj_AI_Base enemy in ObjectManager.Get<Obj_AI_Base>().Where(enemy => enemyIsJumpable(enemy)))
                {
                    Vector2 posAfterE = pPos + (Vector2.Normalize(enemy.Position.To2D() - pPos) * E.Range);
                    if ((target.Distance(posAfterE) < dist
                        || target.Distance(posAfterE) < LXOrbwalker.GetAutoAttackRange(target) + 100)
                        && goesThroughWall(target.Position, posAfterE.To3D()))
                    {
                        if (useENormal(target))
                            return;
                    }
                }
            }

        }

        public static bool goesThroughWall(Vector3 vec1, Vector3 vec2)
        {
            if (wall.endtime < Game.Time || wall.pointL == null || wall.pointL == null)
                return false;
            Vector2 inter = LineIntersectionPoint(vec1.To2D(), vec2.To2D(), wall.pointL.Position.To2D(), wall.pointR.Position.To2D());
            float wallW = (300 + 50 * W.Level);
            if (wall.pointL.Position.To2D().Distance(inter) > wallW ||
                wall.pointR.Position.To2D().Distance(inter) > wallW)
                return false;
            var dist = vec1.Distance(vec2);
            if (vec1.To2D().Distance(inter) + vec2.To2D().Distance(inter) - 30 > dist)
                return false;

            return true;
        }

        public static void doLastHit(Obj_AI_Hero target)
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + 50);
            foreach (var minion in minions.Where(minion => minion.IsValidTarget(Q.Range)))
            {
                if (Player.Distance(minion) < LXOrbwalker.GetAutoAttackRange(minion) && minion.Health < Player.GetAutoAttackDamage(minion))
                    return;
                if (Program.Menu.Item("useElh").GetValue<bool>() && minion.Health < Player.GetSpellDamage(minion, E.Slot))
                    useENormal(minion);

                if (Program.Menu.Item("useQlh").GetValue<bool>() && !isQEmpovered() && minion.Health < Player.GetSpellDamage(minion, Q.Slot))
                    if (!(target != null && isQEmpovered() && Player.Distance(target) < 1050))
                    {
                        if (canCastFarQ())
                            Q.Cast(minion);
                    }
            }
        }

        public static void doLaneClear(Obj_AI_Hero target)
        {
            List<Obj_AI_Base> minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 1000, MinionTypes.All, MinionTeam.NotAlly);
            if (Program.Menu.Item("useElc").GetValue<bool>() && E.IsReady())
                foreach (var minion in minions.Where(minion => minion.IsValidTarget(E.Range) && enemyIsJumpable(minion)))
                {
                    if (minion.Health < Player.GetSpellDamage(minion, E.Slot)
                        || Q.IsReady() && minion.Health < (Player.GetSpellDamage(minion, E.Slot) + Player.GetSpellDamage(minion, Q.Slot)))
                    {
                        if (useENormal(minion))
                            return;
                    }
                }

            if (Q.IsReady() && Program.Menu.Item("useQlc").GetValue<bool>())
            {
                if (isQEmpovered() && !(target != null && Player.Distance(target) < 1050))
                {
                    if (canCastFarQ())
                    {
                        List<Vector2> minionPs = GetCastMinionsPredictedPositions(minions, getNewQSpeed(), 50f, 1200f, Player.ServerPosition, 900f, false, SkillshotType.SkillshotLine);
                        MinionManager.FarmLocation farm = QEmp.GetLineFarmLocation(minionPs); //MinionManager.GetBestLineFarmLocation(minionPs, 50f, 900f);
                        if (farm.MinionsHit >= Program.Menu.Item("useEmpQHit").GetValue<Slider>().Value)
                        {
                            //Console.WriteLine("Cast q simp Emp");
                            QEmp.Cast(farm.Position, false);
                            return;
                        }
                    }
                    else
                    {
                        if (minions.Count(min => min.IsValid && !min.IsDead && min.Distance(getDashEndPos()) < QCir.Range) >= Program.Menu.Item("useEmpQHit").GetValue<Slider>().Value)
                        {
                            QCir.Cast(Player.Position, false);
                            Console.WriteLine("Cast q circ simp");
                        }
                    }
                }
                else
                {
                    if (!isDashigPro)
                    {
                        List<Vector2> minionPs = GetCastMinionsPredictedPositions(minions, getNewQSpeed() * 0.3f, 30f, float.MaxValue, Player.ServerPosition, 465, false, SkillshotType.SkillshotLine);
                        Vector2 clos = Geometry.Closest(Player.ServerPosition.To2D(), minionPs);
                        if (Player.Distance(clos) < 475)
                        {
                            Console.WriteLine("Cast q simp");

                            Q.Cast(clos, false);
                            return;
                        }
                    }
                    else
                    {
                        if (minions.Count(min => !min.IsDead && min.IsValid && min.Distance(getDashEndPos()) < QCir.Range) > 1)
                        {
                            QCir.Cast(Player.Position, false);
                            Console.WriteLine("Cast q circ simp");
                            return;
                        }
                    }
                }
            }

        }

        public static void doHarass(Obj_AI_Hero target)
        {
            if (!inTowerRange(Player.ServerPosition.To2D()) || Program.Menu.Item("harassTower").GetValue<bool>())
                useQSmart(target);
        }


        public static void doFastGetTo()
        {
            List<Obj_AI_Base> jumpies = ObjectManager.Get<Obj_AI_Base>().Where(enemy => enemyIsJumpable(enemy)).ToList();

        }

        public static void useHydra(Obj_AI_Base target)
        {

            if ((Items.CanUseItem(3074) || Items.CanUseItem(3074)) && target.Distance(Player.ServerPosition) < (400 + target.BoundingRadius - 20))
            {
                Items.UseItem(3074, target);
                Items.UseItem(3077, target);
            }
        }


        public static bool inTowerRange(Vector2 pos)
        {
            //  if (!Program.Menu.Item("djTur").GetValue<bool>())
            //      return false;
            foreach (Obj_AI_Turret tur in ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsEnemy && tur.Health > 0))
            {
                if (pos.Distance(tur.Position.To2D()) < (850 + Player.BoundingRadius))
                    return true;
            }
            return false;
        }

        public static Vector3 getDashEndPos()
        {
            Vector2 dashPos2 = Player.GetDashInfo().EndPos;
            return new Vector3(dashPos2, Player.ServerPosition.Z);
        }

        public static bool isQEmpovered()
        {
            return Player.HasBuff("yasuoq3w", true);
        }

        public static bool isDashing()
        {
            return isDashigPro;
        }

        public static bool canCastFarQ()
        {
            return !isDashigPro;
        }

        public static bool canCastCircQ()
        {
            return isDashigPro;
        }


        public static List<Obj_AI_Hero> getKockUpEnemies(ref float lessKnockTime)
        {
            List<Obj_AI_Hero> enemKonck = new List<Obj_AI_Hero>();
            foreach (Obj_AI_Hero enem in ObjectManager.Get<Obj_AI_Hero>().Where(enem => enem.IsEnemy))
            {
                foreach (BuffInstance buff in enem.Buffs)
                {
                    if (buff.Type == BuffType.Knockback || buff.Type == BuffType.Knockup)
                    {
                        if (buff.Type == BuffType.Knockup)
                            lessKnockTime = (buff.EndTime - Game.Time) < lessKnockTime
                                ? (buff.EndTime - Game.Time)
                                : lessKnockTime;
                        enemKonck.Add(enem);
                        break;
                    }
                }
            }
            if (!Program.Menu.Item("useRHitTime").GetValue<bool>())
                lessKnockTime = 0;
            return enemKonck;
        }


        public static void setUpWall()
        {
            if (wall == null)
                return;

        }

        public static void useQSmart(Obj_AI_Hero target, bool onlyEmp = false)
        {
            if (!Q.IsReady())
                return;
            if (isQEmpovered())
            {
                if (canCastFarQ())
                {
                    PredictionOutput po = QEmp.GetPrediction(target); //QEmp.GetPrediction(target, true);
                    if (po.Hitchance >= HitChance.Medium)
                    {
                        QEmp.Cast(po.CastPosition);
                        return;
                    }
                }
                else//dashing
                {
                    float trueRange = QCir.Range - 10;
                    Vector3 endPos = getDashEndPos();
                    if (Player.Distance(endPos) < 10 && target.Distance(endPos) < QCir.Range)
                    {
                        QCir.Cast(target.Position);
                        return;
                    }
                }
            }
            else if (!onlyEmp)
            {
                if (canCastFarQ())
                {
                    PredictionOutput po = Q.GetPrediction(target);
                    if (po.Hitchance >= HitChance.Medium)
                        Q.Cast(po.CastPosition);
                    return;

                }
                else//dashing
                {
                    float trueRange = QCir.Range - 10;
                    Vector3 endPos = getDashEndPos();
                    if (Player.Distance(endPos) < 5 && target.Distance(endPos) < QCir.Range)
                    {
                        QCir.Cast(target.Position);
                        return;
                    }
                }
            }
        }


        public static IsSafeResult isSafePoint(Vector2 point)
        {
            var result = new IsSafeResult();
            result.SkillshotList = new List<Skillshot>();
            result.casters = new List<Obj_AI_Base>();

            foreach (var skillshot in DetectedSkillshots)
            {
                if (skillshot.IsDanger(point))
                {
                    Console.WriteLine("not safe");
                    result.SkillshotList.Add(skillshot);
                    result.casters.Add(skillshot.Unit);
                }
            }

            result.IsSafe = (result.SkillshotList.Count == 0);
            return result;
        }

        public static Obj_AI_Minion GetCandidates(Obj_AI_Hero player, List<Skillshot> skillshots)
        {
            float currentDashSpeed = 700 + player.MoveSpeed;//At least has to be like this
            IEnumerable<Obj_AI_Minion> minions = ObjectManager.Get<Obj_AI_Minion>();
            Obj_AI_Minion candidate = new Obj_AI_Minion();
            double closest = 10000000000000;
            foreach (Obj_AI_Minion minion in minions)
            {
                if (Vector2.Distance(player.Position.To2D(), minion.Position.To2D()) < 475 && minion.IsEnemy && enemyIsJumpable(minion) && closest > Vector3.DistanceSquared(Game.CursorPos, minion.Position))
                {
                    foreach (Skillshot skillshot in skillshots)
                    {
                        //Get intersection point
                        //  Vector2 intersectionPoint = LineIntersectionPoint(startPos, player.Position.To2D(), endPos, V2E(player.Position, minion.Position, 475));
                        //Time when yasuo will be in intersection point
                        //  float arrivingTime = Vector2.Distance(player.Position.To2D(), intersectionPoint) / currentDashSpeed;
                        //Estimated skillshot position
                        //  Vector2 skillshotPosition = V2E(startPos.To3D(), intersectionPoint.To3D(), speed * arrivingTime);
                        if (skillshot.IsDanger(V2E(player.Position, minion.Position, 475)))
                        {
                            candidate = minion;
                            closest = Vector3.DistanceSquared(Game.CursorPos, minion.Position);
                        }
                    }
                }
            }
            return candidate;
        }

        private static Vector2 V2E(Vector3 from, Vector3 direction, float distance)
        {
            return (from + distance * Vector3.Normalize(direction - from)).To2D();
        }

        public static bool wontHitOnDash(Skillshot ss, Obj_AI_Base jumpOn)
        {
            float currentDashSpeed = 700 + Player.MoveSpeed;//At least has to be like this
            //Get intersection point
            Vector2 intersectionPoint = LineIntersectionPoint(Player.Position.To2D(), V2E(Player.Position, jumpOn.Position, 475), ss.Start, ss.End);
            //Time when yasuo will be in intersection point
            float arrivingTime = Vector2.Distance(Player.Position.To2D(), intersectionPoint) / currentDashSpeed;
            //Estimated skillshot position
            Vector2 skillshotPosition = ss.GetMissilePosition((int)(arrivingTime * 1000));

            if (Vector2.DistanceSquared(skillshotPosition, intersectionPoint) <
                (ss.SpellData.Radius + Player.BoundingRadius))
                return false;
            return true;


        }

        public static void useEtoSafe(Skillshot skillShot)
        {
            if (!E.IsReady())
                return;
            Console.WriteLine("try to safe");
            float closest = float.MaxValue;
            Obj_AI_Base closestTarg = null;
            float currentDashSpeed = 700 + Player.MoveSpeed;
            foreach (Obj_AI_Base enemy in ObjectManager.Get<Obj_AI_Base>().Where(ob => ob.NetworkId != skillShot.Unit.NetworkId && enemyIsJumpable(ob) && ob.Distance(Player) < E.Range))
            {
                var pPos = Player.Position.To2D();
                Vector2 posAfterE = V2E(Player.Position, enemy.Position, 475);


                if (isSafePoint(posAfterE).IsSafe && wontHitOnDash(skillShot, enemy)/* && skillShot.IsSafePath(new List<Vector2>() { posAfterE }, 0, (int)currentDashSpeed, 0).IsSafe*/)
                {
                    float curDist = Vector2.DistanceSquared(Game.CursorPos.To2D(), posAfterE);
                    if (curDist < closest)
                    {
                        closestTarg = enemy;
                        closest = curDist;
                    }
                }
            }
            if (closestTarg != null)
                useENormal(closestTarg);
        }

        public static void useWSmart(Skillshot skillShot)
        {
            //try douge with E if cant windWall

            if (!W.IsReady())
                return;
            if (skillShot.IsAboutToHit(250, Player))
            {
                var sd = SpellDatabase.GetByMissileName(skillShot.SpellData.MissileSpellName);
                if (sd == null)
                    return;

                //If enabled
                if (!EvadeSpellEnabled(sd.MenuItemName))
                    return;

                //if only dangerous
                if (Program.Menu.Item("wwDanger").GetValue<bool>() &&
                    !skillShotIsDangerous(sd.MenuItemName))
                    return;

                //Console.WriteLine("dmg: " + missle.SpellCaster.GetSpellDamage(Player, sd.SpellName));
                float spellDamage = (float)skillShot.Unit.GetSpellDamage(Player, sd.SpellName);
                int procHp = (int)((spellDamage / Player.MaxHealth) * 100);

                if (procHp < Program.Menu.Item("wwDmg").GetValue<Slider>().Value && Player.Health - spellDamage > 0)
                    return;


                Vector3 blockwhere = Player.ServerPosition + Vector3.Normalize(skillShot.MissilePosition.To3D() - Player.ServerPosition) * 10; // missle.Position; 
                W.Cast(blockwhere);
            }

        }

        public static void useWSmartOld(Obj_SpellMissile missle)
        {
            if (!W.IsReady())
                return;
            try
            {
                if (missle.SpellCaster is Obj_AI_Hero && missle.IsEnemy)
                {
                    var sd = SpellDatabase.GetByMissileName(missle.SData.Name);
                    if (sd == null)
                        return;

                    //If enabled
                    if (!EvadeSpellEnabled(sd.MenuItemName))
                        return;

                    //if only dangerous
                    if (Program.Menu.Item("wwDanger").GetValue<bool>() &&
                        !skillShotIsDangerous(sd.MenuItemName))
                        return;

                    //Console.WriteLine("dmg: " + missle.SpellCaster.GetSpellDamage(Player, sd.SpellName));
                    float spellDamage = (float)missle.SpellCaster.GetSpellDamage(Player, sd.SpellName);
                    int procHp = (int)((spellDamage / Player.MaxHealth) * 100);

                    if (procHp < Program.Menu.Item("wwDmg").GetValue<Slider>().Value && Player.Health - spellDamage > 0)
                        return;

                    Obj_AI_Base enemHero = missle.SpellCaster;
                    float dmg = (float)enemHero.GetAutoAttackDamage(Player);
                    //enemHero.BaseAttackDamage + enemHero.FlatPhysicalDamageMod);
                    if (missle.SData.Name.Contains("Crit"))
                        dmg *= 2;
                    if (!missle.SData.Name.Contains("Attack") ||
                        (enemHero.CombatType == GameObjectCombatType.Ranged && dmg > Player.MaxHealth / 8))
                    {
                        if (missleWillHit(missle))
                        {
                            Vector3 blockWhere = missle.Position;
                            //Player.ServerPosition + Vector3.Normalize(missle.Position - Player.ServerPosition)*30; // missle.Position; 
                            if (Player.Distance(missle.Position) < 420)
                            {
                                if (missle.Target.IsMe || isMissileCommingAtMe(missle))
                                {
                                    lastSpell = missle.SData.Name;
                                    W.Cast(blockWhere, true);
                                    //YasuoSharp.skillShots.Clear();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static bool missleWillHit(Obj_SpellMissile missle)
        {
            if (missle.Target.IsMe || interCir(missle.StartPosition.To2D(), missle.EndPosition.To2D(), Player.Position.To2D(), missle.SData.LineWidth + Player.BoundingRadius))
            {
                if (missle.StartPosition.Distance(Player.Position) < (missle.StartPosition.Distance(missle.EndPosition)))
                    return true;
            }
            return false;
        }


        public static bool useENormal(Obj_AI_Base target)
        {
            if (!E.IsReady())
                return false;
            Vector2 posAfter = V2E(Player.Position, target.Position, 475);
            if (!Program.Menu.Item("djTur").GetValue<bool>())
            {
                if (isSafePoint(posAfter).IsSafe)
                    E.Cast(target, false);
                return true;
            }
            else
            {
                //Console.WriteLine("gapcloseer?");
                Vector2 pPos = Player.ServerPosition.To2D();
                Vector2 posAfterE = pPos + (Vector2.Normalize(target.Position.To2D() - pPos) * E.Range);
                if (!inTowerRange(posAfterE))
                {
                    if (isSafePoint(posAfter).IsSafe)
                        E.Cast(target, false);
                    return true;
                }
            }
            return false;

        }

        public static bool useESmart(Obj_AI_Hero target, List<Obj_AI_Hero> ignore = null)
        {
            if (!E.IsReady())
                return false;
            float trueAARange = Player.AttackRange + target.BoundingRadius;
            float trueERange = target.BoundingRadius + E.Range;

            float dist = Player.Distance(target);
            Vector2 dashPos = new Vector2();
            if (target.IsMoving && target.Path.Count() != 0)
            {
                Vector2 tpos = target.Position.To2D();
                Vector2 path = target.Path[0].To2D() - tpos;
                path.Normalize();
                dashPos = tpos + (path * 100);
            }
            float targ_ms = (target.IsMoving && Player.Distance(dashPos) > dist) ? target.MoveSpeed : 0;
            float msDif = (Player.MoveSpeed - targ_ms) == 0 ? 0.0001f : (Player.MoveSpeed - targ_ms);
            float timeToReach = (dist - trueAARange) / msDif;
            //Console.WriteLine(timeToReach);
            if (dist > trueAARange && dist < E.Range)
            {
                if (timeToReach > 1.7f || timeToReach < 0.0f)
                {
                    //  Console.WriteLine("test2");
                    if (useENormal(target))
                        return true;
                }
            }
            return false;
        }

        public static void gapCloseE(Vector2 pos, List<Obj_AI_Hero> ignore = null)
        {
            if (!E.IsReady())
                return;

            // Console.WriteLine("gapcloseer?");
            //Player.IssueOrder(GameObjectOrder.MoveTo, pos.To3D());
            Vector2 pPos = Player.ServerPosition.To2D();
            Obj_AI_Base bestEnem = null;
            //Check if can go through wall
            // foreach (Obj_AI_Base enemy in ObjectManager.Get<Obj_AI_Base>().Where(enemy => enemyIsJumpable(enemy, ignore)))
            // {
            //
            //}


            float distToPos = Player.Distance(pos);
            if (((distToPos < Q.Range)) &&
                goesThroughWall(pos.To3D(), Player.Position))
                return;
            Vector2 bestLoc = pPos + (Vector2.Normalize(pos - pPos) * (Player.MoveSpeed * 0.35f));
            float bestDist = pos.Distance(bestLoc);
            try
            {
                foreach (Obj_AI_Base enemy in ObjectManager.Get<Obj_AI_Base>().Where(ob => enemyIsJumpable(ob, ignore)))
                {

                    float trueRange = E.Range + enemy.BoundingRadius;
                    float distToEnem = Player.Distance(enemy);
                    if (distToEnem < trueRange && distToEnem > 15)
                    {
                        Vector2 posAfterE = pPos + (Vector2.Normalize(enemy.Position.To2D() - pPos) * E.Range);
                        float distE = pos.Distance(posAfterE);
                        if (distE < bestDist)
                        {
                            bestLoc = posAfterE;
                            bestDist = distE;
                            bestEnem = enemy;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (bestEnem != null)
                useENormal(bestEnem);

        }


        public static void useRSmart()
        {
            float timeToLand = float.MaxValue;
            List<Obj_AI_Hero> enemInAir = getKockUpEnemies(ref timeToLand);
            foreach (Obj_AI_Hero enem in enemInAir)
            {
                int aroundAir = 0;
                foreach (Obj_AI_Hero enem2 in enemInAir)
                {
                    if (Vector3.DistanceSquared(enem.ServerPosition, enem2.ServerPosition) < 400 * 400)
                        aroundAir++;

                }
                if (aroundAir >= Program.Menu.Item("useRHit").GetValue<Slider>().Value && timeToLand < 0.4f)
                    R.Cast(enem);
            }
        }
        public static void useRKill()
        {
            float timeToLand = float.MaxValue;
            List<Obj_AI_Hero> enemInAir = getKockUpEnemies(ref timeToLand);
            foreach (Obj_AI_Hero enem in enemInAir)
            {
                if (enem.Health < ObjectManager.Player.GetSpellDamage(enem, SpellSlot.R) && R.IsReady())
                {
                    R.Cast(enem);
                }
            }
        }

        public static bool isMissileCommingAtMe(Obj_SpellMissile missle)
        {
            Vector3 step = missle.StartPosition + Vector3.Normalize(missle.StartPosition - missle.EndPosition) * 10;
            return (!(Player.Distance(step) < Player.Distance(missle.StartPosition)));
        }

        public static bool enemyIsJumpable(Obj_AI_Base enemy, List<Obj_AI_Hero> ignore = null)
        {
            //Console.WriteLine("gapcloseerawfawfaw23125?");
            if (enemy.IsValid && enemy.IsEnemy && !enemy.IsInvulnerable && !enemy.MagicImmune && !enemy.IsDead)
            {
                if (ignore != null)
                    foreach (Obj_AI_Hero ign in ignore)
                    {
                        if (ign.NetworkId == enemy.NetworkId)
                            return false;
                    }


                foreach (BuffInstance buff in enemy.Buffs)
                {
                    if (buff.Name == "YasuoDashWrapper")
                        return false;
                }
                return true;
            }
            return false;
        }

        public static float getSpellCastTime(Spell spell)
        {
            return sBook.GetSpell(spell.Slot).SData.SpellCastTime;
        }

        public static float getSpellCastTime(SpellSlot slot)
        {
            return sBook.GetSpell(slot).SData.SpellCastTime;
        }

        public static YasDash getClosestDash(float dist = 350)
        {
            YasDash closestWall = dashes[0];
            for (int i = 1; i < dashes.Count; i++)
            {
                closestWall = closestDashToMouse(closestWall, dashes[i]);
            }
            if (closestWall.to.Distance(Game.CursorPos) < dist)
                return closestWall;
            return null;
        }

        public static YasDash closestDashToMouse(YasDash w1, YasDash w2)
        {
            return Vector3.DistanceSquared(w1.to, Game.CursorPos) + Vector3.DistanceSquared(w1.from, Player.Position) > Vector3.DistanceSquared(w2.to, Game.CursorPos) + Vector3.DistanceSquared(w2.from, Player.Position) ? w2 : w1;
        }

        public static void saveLastDash()
        {
            if (lastDash.from.X != -1 && lastDash.from.Y != -1)
                dashes.Add(new YasDash(lastDash));
            lastDash = new YasDash();
        }

        public static void fleeToMouse()
        {
            try
            {
                YasDash closeDash = getClosestDash();
                if (closeDash != null)
                {
                    List<Obj_AI_Base> jumps = canGoThrough(closeDash);
                    if (jumps.Count > 0 || ((W.IsReady() || (wall != null && (wall.endtime - Game.Time) > 3f)) && jTimers.closestJCUp(closeDash.to)))
                    {
                        var distToDash = Player.Distance(closeDash.from);

                        if (W.IsReady() && distToDash < 136f && jumps.Count == 0 && MinionManager.GetMinions(Game.CursorPos, 350).Where(min => min.IsVisible).Count() < 2)
                        {
                            W.Cast(closeDash.to);
                        }

                        if (distToDash > 2f)
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, closeDash.from);
                            return;
                        }

                        if (distToDash < 3f && jumps.Count > 0)
                        {
                            E.Cast(jumps.First());
                        }
                        return;
                    }
                }
                if (getClosestDash(400) == null)
                    gapCloseE(Game.CursorPos.To2D());
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static List<Obj_AI_Base> canGoThrough(YasDash dash)
        {
            List<Obj_AI_Base> jumps = ObjectManager.Get<Obj_AI_Base>().Where(enemy => enemyIsJumpable(enemy) && enemy.IsValidTarget(550, true, dash.to)).ToList();
            List<Obj_AI_Base> canBejump = new List<Obj_AI_Base>();
            foreach (var jumpe in jumps)
            {
                if (interCir(dash.from.To2D(), dash.to.To2D(), jumpe.Position.To2D(), 35) /*&& jumpe.Distance(dash.to) < Player.Distance(dash.to)*/)
                {
                    canBejump.Add(jumpe);
                }
            }
            return canBejump.OrderBy(jum => Player.Distance(jum)).ToList();
        }


        public static float getLengthTillPos(Vector3 pos)
        {
            return 0;
        }

        #endregion

        public Yasuo()
        {
            LoadMenu();

            setSkillShots();
            setDashes();
            point1 = Player.Position;
            
            Drawing.OnDraw += onDraw;
            Game.OnGameUpdate += OnGameUpdate;

            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            CustomEvents.Unit.OnLevelUp += OnLevelUp;

            Game.OnGameSendPacket += OnGameSendPacket;
            Game.OnGameProcessPacket += OnGameProcessPacket;

            SkillshotDetector.OnDetectSkillshot += OnDetectSkillshot;
            SkillshotDetector.OnDeleteMissile += OnDeleteMissile;

            PluginLoaded();
        }

        private void LoadMenu()
        {
            //Combo
            Program.Menu.AddSubMenu(new Menu("Combo", "combo"));
            Program.Menu.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items")).SetValue(true);
            //SmartR
            Program.Menu.SubMenu("combo").AddItem(new MenuItem("smartR", "Smart R")).SetValue(true);
            Program.Menu.SubMenu("combo").AddItem(new MenuItem("useRHit", "Use R if hit")).SetValue(new Slider(3, 5, 1));
            Program.Menu.SubMenu("combo").AddItem(new MenuItem("useRHitTime", "Use R when they land")).SetValue(true);
            Program.Menu.SubMenu("combo").AddItem(new MenuItem("useEWall", "use E to safe")).SetValue(true);
            //Flee away
            Program.Menu.SubMenu("combo").AddItem(new MenuItem("flee", "E away")).SetValue(new KeyBind('Z', KeyBindType.Press, false));


            //LastHit
            Program.Menu.AddSubMenu(new Menu("LastHit", "lHit"));
            Program.Menu.SubMenu("lHit").AddItem(new MenuItem("useQlh", "Use Q")).SetValue(true);
            Program.Menu.SubMenu("lHit").AddItem(new MenuItem("useElh", "Use E")).SetValue(true);
            //LaneClear
            Program.Menu.AddSubMenu(new Menu("LaneClear", "lClear"));
            Program.Menu.SubMenu("lClear").AddItem(new MenuItem("useQlc", "Use Q")).SetValue(true);
            Program.Menu.SubMenu("lClear").AddItem(new MenuItem("useEmpQHit", "Emp Q Min hit")).SetValue(new Slider(3, 6, 1));
            Program.Menu.SubMenu("lClear").AddItem(new MenuItem("useElc", "Use E")).SetValue(true);
            //Harass
            Program.Menu.AddSubMenu(new Menu("Harass", "harass"));
            Program.Menu.SubMenu("harass").AddItem(new MenuItem("harassTower", "Harass under tower")).SetValue(false);
            Program.Menu.SubMenu("harass").AddItem(new MenuItem("harassOn", "Harass enemies")).SetValue(true);
            Program.Menu.SubMenu("harass").AddItem(new MenuItem("harQ3Only", "Use only Q3")).SetValue(false);
            //Drawings
            Program.Menu.AddSubMenu(new Menu("Drawing", "drawing"));
            Program.Menu.SubMenu("drawing").AddItem(new MenuItem("disDraw", "Dissabel drawing")).SetValue(false);
            Program.Menu.SubMenu("drawing").AddItem(new MenuItem("drawQ", "Draw Q range")).SetValue(true);
            Program.Menu.SubMenu("drawing").AddItem(new MenuItem("drawE", "Draw E range")).SetValue(true);
            Program.Menu.SubMenu("drawing").AddItem(new MenuItem("drawR", "Draw R range")).SetValue(true);
            Program.Menu.SubMenu("drawing").AddItem(new MenuItem("drawWJ", "Draw Wall Jumps")).SetValue(true);

            //Extra
            Program.Menu.AddSubMenu(new Menu("Extra", "extra"));
            Program.Menu.SubMenu("extra").AddItem(new MenuItem("djTur", "Dont Jump turrets")).SetValue(true);
            Program.Menu.SubMenu("extra").AddItem(new MenuItem("useR_KS", "Use R for KS").SetValue(true));
            Program.Menu.SubMenu("extra").AddItem(new MenuItem("autoLevel", "Auto Level")).SetValue(true);
            Program.Menu.SubMenu("extra").AddItem(new MenuItem("levUpSeq", "")).SetValue(new StringList(new string[2] { "Q E W Q start", "Q E Q W start" }));

            //LastHit
            Program.Menu.AddSubMenu(new Menu("Anti Skillshots", "aShots"));
            //SmartW
            Program.Menu.SubMenu("aShots").AddItem(new MenuItem("smartW", "Smart WW")).SetValue(true);
            Program.Menu.SubMenu("aShots").AddItem(new MenuItem("smartEDogue", "E use dogue")).SetValue(true);
            Program.Menu.SubMenu("aShots").AddItem(new MenuItem("wwDanger", "WW only dangerous")).SetValue(false);
            Program.Menu.SubMenu("aShots").AddItem(new MenuItem("wwDmg", "WW if does proc HP")).SetValue(new Slider(0, 100, 1));
            skillShotMenu = getSkilshotMenu();
            Program.Menu.SubMenu("aShots").AddSubMenu(skillShotMenu);
            //Debug
            Program.Menu.AddSubMenu(new Menu("Debug", "debug"));
            Program.Menu.SubMenu("debug").AddItem(new MenuItem("WWLast", "Print last ww blocked")).SetValue(new KeyBind('T', KeyBindType.Press, false));
            Program.Menu.SubMenu("debug").AddItem(new MenuItem("saveDash", "saveDashd")).SetValue(new KeyBind('O', KeyBindType.Press, false));
            Program.Menu.SubMenu("debug").AddItem(new MenuItem("exportDash", "export dashes")).SetValue(new KeyBind('P', KeyBindType.Press, false));
            Program.Menu.SubMenu("debug").AddItem(new MenuItem("deleteDash", "deleteLastDash")).SetValue(new KeyBind('I', KeyBindType.Press, false));
        }  

        public static Menu getSkilshotMenu()
        {
            //Create the skillshots submenus.
            var skillShots = new Menu("Enemy Skillshots", "aShotsSkills");

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.Team != ObjectManager.Player.Team)
                {
                    foreach (var spell in SpellDatabase.Spells)
                    {
                        if (spell.ChampionName == hero.ChampionName)
                        {
                            var subMenu = new Menu(spell.MenuItemName, spell.MenuItemName);

                            subMenu.AddItem(
                                new MenuItem("DangerLevel" + spell.MenuItemName, "Danger level").SetValue(
                                    new Slider(spell.DangerValue, 5, 1)));

                            subMenu.AddItem(
                                new MenuItem("IsDangerous" + spell.MenuItemName, "Is Dangerous").SetValue(
                                    spell.IsDangerous));

                            subMenu.AddItem(new MenuItem("Draw" + spell.MenuItemName, "Draw").SetValue(true));
                            subMenu.AddItem(new MenuItem("Enabled" + spell.MenuItemName, "Enabled").SetValue(true));

                            skillShots.AddSubMenu(subMenu);
                        }
                    }
                }
            }
            return skillShots;
        }

        public static bool skillShotIsDangerous(string Name)
        {
            if (skillShotMenu.Item("IsDangerous" + Name) != null)
            {
                return skillShotMenu.Item("IsDangerous" + Name).GetValue<bool>();
            }
            return true;
        }

        public static bool EvadeSpellEnabled(string Name)
        {
            if (skillShotMenu.Item("Enabled" + Name) != null)
            {
                return skillShotMenu.Item("Enabled" + Name).GetValue<bool>();
            }
            return true;
        }

        public static void updateSkillshots()
        {
            foreach (var ss in DetectedSkillshots)
            {
                ss.Game_OnGameUpdate();
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            try
            {
                Q.SetSkillshot(getNewQSpeed(), 50f, float.MaxValue, false, SkillshotType.SkillshotLine);

                if (startDash + 1f < Game.Time && isDashigPro)
                {
                    isDashigPro = false;
                }

                //updateSkillshots();
                //Remove the detected skillshots that have expired.
                DetectedSkillshots.RemoveAll(skillshot => !skillshot.IsActive());

                Obj_AI_Hero target = SimpleTs.GetTarget(1250, SimpleTs.DamageType.Physical);
                if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo)
                {
                    doCombo(target);
                }

                if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Lasthit)
                {
                    doLastHit(target);
                    useQSmart(target);
                }

                if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Harass)
                {
                    doLastHit(target);
                    useQSmart(target);
                }

                if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneClear)
                {
                    doLaneClear(target);
                }

                if (Program.Menu.Item("flee").GetValue<KeyBind>().Active)
                {
                    fleeToMouse();
                }

                if (Program.Menu.Item("saveDash").GetValue<KeyBind>().Active && canSave)
                {
                    saveLastDash();
                    canSave = false;
                }
                else
                {
                    canSave = true;
                }

                if (Program.Menu.Item("deleteDash").GetValue<KeyBind>().Active && canDelete)
                {
                    if (dashes.Count > 0)
                        dashes.RemoveAt(dashes.Count - 1);
                    canDelete = false;
                }
                else
                {
                    canDelete = true;
                }
                if (Program.Menu.Item("exportDash").GetValue<KeyBind>().Active && canExport)
                {
                    using (var file = new System.IO.StreamWriter(@"C:\YasuoDashes.txt"))
                    {

                        foreach (var dash in dashes)
                        {
                            string dashS = "dashes.Add(new YasDash(new Vector3(" +
                                           dash.from.X.ToString("0.00").Replace(',', '.') + "f," +
                                           dash.from.Y.ToString("0.00").Replace(',', '.') + "f," +
                                           dash.from.Z.ToString("0.00").Replace(',', '.') +
                                           "f),new Vector3(" + dash.to.X.ToString("0.00").Replace(',', '.') + "f," +
                                           dash.to.Y.ToString("0.00").Replace(',', '.') + "f," +
                                           dash.to.Z.ToString("0.00").Replace(',', '.') + "f)));";
                            //new YasDash(new Vector3(X,Y,Z),new Vector3(X,Y,Z))

                            file.WriteLine(dashS);
                        }
                        file.Close();
                    }

                    canExport = false;
                }
                else
                {
                    canExport = true;
                }
                //if (!isSafePoint(Player.Position.To2D()).IsSafe)
                //    Console.WriteLine("not safe");
                if (Program.Menu.Item("WWLast").GetValue<KeyBind>().Active)
                {
                    Console.WriteLine("Last WW skill blocked: " + lastSpell);
                    Game.PrintChat("Last WW skill blocked: " + lastSpell);
                }

                if (Program.Menu.Item("useR_KS").GetValue<bool>())
                {
                    useRKill();
                }  

                if (Program.Menu.Item("harassOn").GetValue<bool>() && LXOrbwalker.CurrentMode.ToString() == "None")
                {
                    useQSmart(target, Program.Menu.Item("harQ3Only").GetValue<bool>());
                }
                // if (Program.Menu.Item("smartW").GetValue<bool>() && !Program.Menu.Item("flee").GetValue<KeyBind>().Active)
                foreach (var mis in DetectedSkillshots)
                {
                    useWSmart(mis);
                    // if (Program.Menu.Item("smartEDogue").GetValue<bool>())

                    if (/*mis.IsAboutToHit(300,Player)*/  !isSafePoint(Player.Position.To2D()).IsSafe)
                        useEtoSafe(mis);
                }
                //smartEDog
                if (Program.Menu.Item("smartEDogue").GetValue<bool>())
                {
                    //useEtoSafe(target, (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo)?true:false);
                }



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        private static void onDraw(EventArgs args)
        {
            if (Program.Menu.Item("disDraw").GetValue<bool>())
                return;

            if (Program.Menu.Item("drawQ").GetValue<bool>())
                Utility.DrawCircle(Player.Position, 475, (isDashigPro) ? Color.Red : Color.Green);
            if (Program.Menu.Item("drawR").GetValue<bool>())
                Utility.DrawCircle(Player.Position, 1200, Color.Green);

            if (Program.Menu.Item("flee").GetValue<KeyBind>().Active && Program.Menu.Item("drawWJ").GetValue<bool>())
            {
                Utility.DrawCircle(Game.CursorPos, 350, Color.Cyan);

                Utility.DrawCircle(lastDash.from, 60, Color.BlueViolet);
                Utility.DrawCircle(lastDash.to, 60, Color.BlueViolet);

                foreach (YasDash dash in dashes)
                {
                    if (dash.from.Distance(Game.CursorPos) < 1200)
                    {
                        var SA = Drawing.WorldToScreen(dash.from);
                        var SB = Drawing.WorldToScreen(dash.to);
                        Drawing.DrawLine(SA.X, SA.Y, SB.X, SB.Y, 3, Color.Green);
                    }
                }

            }
            foreach (var ss in DetectedSkillshots)
            {
                ss.Draw(Color.CadetBlue, Color.Red, 1);
            } 
        }
        
        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            //wall
            if (sender is Obj_SpellLineMissile)
            {
                Obj_SpellLineMissile missle = (Obj_SpellLineMissile)sender;
                if (missle.SData.Name == "yasuowmovingwallmisl")
                {
                    wall.setL(missle);
                }

                if (missle.SData.Name == "yasuowmovingwallmisr")
                {
                    wall.setR(missle);
                }
                //   Console.WriteLine(missle.SData.Name);
            }
            if (sender is Obj_SpellMissile && sender.IsEnemy)
            {

                Obj_SpellMissile missle = (Obj_SpellMissile)sender;
            }
        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {

        }

        public static void OnProcessSpell(Obj_AI_Base obj, GameObjectProcessSpellCastEventArgs arg)
        {
            if (obj.IsMe)
            {
                if (arg.SData.Name == "YasuoDashWrapper")//start dash
                {
                    Console.WriteLine("--- DAhs started---");
                    lastDash.from = Player.Position;
                    isDashigPro = true;
                    castFrom = Player.Position;
                    startDash = Game.Time;
                }
            }
        }

        public static void OnLevelUp(LeagueSharp.Obj_AI_Base sender, LeagueSharp.Common.CustomEvents.Unit.OnLevelUpEventArgs args)
        {
            if (sender.NetworkId == Player.NetworkId)
            {
                if (!Program.Menu.Item("autoLevel").GetValue<bool>())
                    return;
                if (Program.Menu.Item("levUpSeq").GetValue<StringList>().SelectedIndex == 0)
                    sBook.LevelUpSpell(levelUpSeq[args.NewLevel - 1].Slot);
                else if (Program.Menu.Item("levUpSeq").GetValue<StringList>().SelectedIndex == 1)
                    sBook.LevelUpSpell(levelUpSeq2[args.NewLevel - 1].Slot);
            }
        }



        private static void OnGameProcessPacket(GamePacketEventArgs args)
        {//28 16 176 ??184
            if (args.PacketData[0] == 41)//135no 100no 183no 34no 101 133 56yesss? 127 41yess
            {
                GamePacket gp = new GamePacket(args.PacketData);
                //Console.WriteLine(Encoding.UTF8.GetString(args.PacketData, 0, args.PacketData.Length));
                gp.Position = 1;
                if (gp.ReadInteger() == Player.NetworkId /*&&  Encoding.UTF8.GetString(args.PacketData, 0, args.PacketData.Length).Contains("Spell3")*/)
                {
                    Console.WriteLine("----");
                    lastDash.to = Player.Position;
                    isDashigPro = false;
                    time = Game.Time - startDash;
                }                
            }                        
        }

        private static void OnGameSendPacket(GamePacketEventArgs args)
        {
            
        }

        private static void OnDeleteMissile(Skillshot skillshot, Obj_SpellMissile missile)
        {
            if (skillshot.SpellData.SpellName == "VelkozQ")
            {
                var spellData = SpellDatabase.GetByName("VelkozQSplit");
                var direction = skillshot.Direction.Perpendicular();
                if (DetectedSkillshots.Count(s => s.SpellData.SpellName == "VelkozQSplit") == 0)
                {
                    for (var i = -1; i <= 1; i = i + 2)
                    {
                        var skillshotToAdd = new Skillshot(
                            DetectionType.ProcessSpell, spellData, Environment.TickCount, missile.Position.To2D(),
                            missile.Position.To2D() + i * direction * spellData.Range, skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                    }
                }
            }
        }

        private static void OnDetectSkillshot(Skillshot skillshot)
        {
            var alreadyAdded = false;

            foreach (var item in DetectedSkillshots)
            {
                if (item.SpellData.SpellName == skillshot.SpellData.SpellName &&
                    (item.Unit.NetworkId == skillshot.Unit.NetworkId &&
                     (skillshot.Direction).AngleBetween(item.Direction) < 5 &&
                     (skillshot.Start.Distance(item.Start) < 100 || skillshot.SpellData.FromObjects.Length == 0)))
                {
                    alreadyAdded = true;
                }
            }

            //Check if the skillshot is from an ally.
            if (skillshot.Unit.Team == ObjectManager.Player.Team)
            {
                return;
            }

            //Check if the skillshot is too far away.
            if (skillshot.Start.Distance(ObjectManager.Player.ServerPosition.To2D()) >
                (skillshot.SpellData.Range + skillshot.SpellData.Radius + 1000) * 1.5)
            {
                return;
            }

            //Add the skillshot to the detected skillshot list.
            if (!alreadyAdded)
            {
                //Multiple skillshots like twisted fate Q.
                if (skillshot.DetectionType == DetectionType.ProcessSpell)
                {
                    if (skillshot.SpellData.MultipleNumber != -1)
                    {
                        var originalDirection = skillshot.Direction;

                        for (var i = -(skillshot.SpellData.MultipleNumber - 1) / 2;
                            i <= (skillshot.SpellData.MultipleNumber - 1) / 2;
                            i++)
                        {
                            var end = skillshot.Start +
                                      skillshot.SpellData.Range *
                                      originalDirection.Rotated(skillshot.SpellData.MultipleAngle * i);
                            var skillshotToAdd = new Skillshot(
                                skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                                skillshot.Unit);

                            DetectedSkillshots.Add(skillshotToAdd);
                        }
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "UFSlash")
                    {
                        skillshot.SpellData.MissileSpeed = 1600 + (int)skillshot.Unit.MoveSpeed;
                    }

                    if (skillshot.SpellData.Invert)
                    {
                        var newDirection = -(skillshot.End - skillshot.Start).Normalized();
                        var end = skillshot.Start + newDirection * skillshot.Start.Distance(skillshot.End);
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.Centered)
                    {
                        var start = skillshot.Start - skillshot.Direction * skillshot.SpellData.Range;
                        var end = skillshot.Start + skillshot.Direction * skillshot.SpellData.Range;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "SyndraE" || skillshot.SpellData.SpellName == "syndrae5")
                    {
                        var angle = 60;
                        var edge1 =
                            (skillshot.End - skillshot.Unit.ServerPosition.To2D()).Rotated(
                                -angle / 2 * (float)Math.PI / 180);
                        var edge2 = edge1.Rotated(angle * (float)Math.PI / 180);

                        foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            var v = minion.ServerPosition.To2D() - skillshot.Unit.ServerPosition.To2D();
                            if (minion.Name == "Seed" && edge1.CrossProduct(v) > 0 && v.CrossProduct(edge2) > 0 &&
                                minion.Distance(skillshot.Unit) < 800 &&
                                (minion.Team != ObjectManager.Player.Team))
                            {
                                var start = minion.ServerPosition.To2D();
                                var end = skillshot.Unit.ServerPosition.To2D()
                                    .Extend(
                                        minion.ServerPosition.To2D(),
                                        skillshot.Unit.Distance(minion) > 200 ? 1300 : 1000);

                                var skillshotToAdd = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                                    skillshot.Unit);
                                DetectedSkillshots.Add(skillshotToAdd);
                            }
                        }
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "AlZaharCalloftheVoid")
                    {
                        var start = skillshot.End - skillshot.Direction.Perpendicular() * 400;
                        var end = skillshot.End + skillshot.Direction.Perpendicular() * 400;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "ZiggsQ")
                    {
                        var d1 = skillshot.Start.Distance(skillshot.End);
                        var d2 = d1 * 0.4f;
                        var d3 = d2 * 0.69f;


                        var bounce1SpellData = SpellDatabase.GetByName("ZiggsQBounce1");
                        var bounce2SpellData = SpellDatabase.GetByName("ZiggsQBounce2");

                        var bounce1Pos = skillshot.End + skillshot.Direction * d2;
                        var bounce2Pos = bounce1Pos + skillshot.Direction * d3;

                        bounce1SpellData.Delay =
                            (int)(skillshot.SpellData.Delay + d1 * 1000f / skillshot.SpellData.MissileSpeed + 500);
                        bounce2SpellData.Delay =
                            (int)(bounce1SpellData.Delay + d2 * 1000f / bounce1SpellData.MissileSpeed + 500);

                        var bounce1 = new Skillshot(
                            skillshot.DetectionType, bounce1SpellData, skillshot.StartTick, skillshot.End, bounce1Pos,
                            skillshot.Unit);
                        var bounce2 = new Skillshot(
                            skillshot.DetectionType, bounce2SpellData, skillshot.StartTick, bounce1Pos, bounce2Pos,
                            skillshot.Unit);

                        DetectedSkillshots.Add(bounce1);
                        DetectedSkillshots.Add(bounce2);
                    }

                    if (skillshot.SpellData.SpellName == "ZiggsR")
                    {
                        skillshot.SpellData.Delay =
                            (int)(1500 + 1500 * skillshot.End.Distance(skillshot.Start) / skillshot.SpellData.Range);
                    }

                    if (skillshot.SpellData.SpellName == "JarvanIVDragonStrike")
                    {
                        var endPos = new Vector2();

                        foreach (var s in DetectedSkillshots)
                        {
                            if (s.Unit.NetworkId == skillshot.Unit.NetworkId && s.SpellData.Slot == SpellSlot.E)
                            {
                                endPos = s.End;
                            }
                        }

                        foreach (var m in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            if (m.BaseSkinName == "jarvanivstandard" && m.Team == skillshot.Unit.Team &&
                                skillshot.IsDanger(m.Position.To2D()))
                            {
                                endPos = m.Position.To2D();
                            }
                        }

                        if (!endPos.IsValid())
                        {
                            return;
                        }

                        skillshot.End = endPos + 200 * (endPos - skillshot.Start).Normalized();
                        skillshot.Direction = (skillshot.End - skillshot.Start).Normalized();
                    }
                }

                if (skillshot.SpellData.SpellName == "OriannasQ")
                {
                    var endCSpellData = SpellDatabase.GetByName("OriannaQend");

                    var skillshotToAdd = new Skillshot(
                        skillshot.DetectionType, endCSpellData, skillshot.StartTick, skillshot.Start, skillshot.End,
                        skillshot.Unit);

                    DetectedSkillshots.Add(skillshotToAdd);
                }


                //Dont allow fow detection.
                if (skillshot.SpellData.DisableFowDetection && skillshot.DetectionType == DetectionType.RecvPacket)
                {
                    return;
                }

                Console.WriteLine(Environment.TickCount + "Adding new skillshot: " + skillshot.SpellData.SpellName);

                DetectedSkillshots.Add(skillshot);
            }
        }

        public static bool interact(Vector2 p1, Vector2 p2, Vector2 pC, float radius)
        {

            Vector2 p3 = new Vector2();
            p3.X = pC.X + radius;
            p3.Y = pC.Y + radius;
            float m = ((p2.Y - p1.Y) / (p2.X - p1.X));
            float Constant = (m * p1.X) - p1.Y;

            float b = -(2f * ((m * Constant) + p3.X + (m * p3.Y)));
            float a = (1 + (m * m));
            float c = ((p3.X * p3.X) + (p3.Y * p3.Y) - (radius * radius) + (2f * Constant * p3.Y) + (Constant * Constant));
            float D = ((b * b) - (4f * a * c));
            if (D > 0)
            {
                return true;
            }
            else
                return false;

        }

        public static bool interCir(Vector2 E, Vector2 L, Vector2 C, float r)
        {
            Vector2 d = L - E;
            Vector2 f = E - C;

            float a = Vector2.Dot(d, d);
            float b = 2 * Vector2.Dot(f, d);
            float c = Vector2.Dot(f, f) - r * r;

            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                // no intersection
            }
            else
            {
                // ray didn't totally miss sphere,
                // so there is a solution to
                // the equation.

                discriminant = (float)Math.Sqrt(discriminant);

                // either solution may be on or off the ray so need to test both
                // t1 is always the smaller value, because BOTH discriminant and
                // a are nonnegative.
                float t1 = (-b - discriminant) / (2 * a);
                float t2 = (-b + discriminant) / (2 * a);

                // 3x HIT cases:
                //          -o->             --|-->  |            |  --|->
                // Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 

                // 3x MISS cases:
                //       ->  o                     o ->              | -> |
                // FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)

                if (t1 >= 0 && t1 <= 1)
                {
                    // t1 is the intersection, and it's closer than t2
                    // (since t1 uses -b - discriminant)
                    // Impale, Poke
                    return true;
                }

                // here t1 didn't intersect so we are either started
                // inside the sphere or completely past it
                if (t2 >= 0 && t2 <= 1)
                {
                    // ExitWound
                    return true;
                }

                // no intn: FallShort, Past, CompletelyInside
                return false;
            }
            return false;
        }

        public static float DistanceFromPointToLine(Vector2 l1, Vector2 l2, Vector2 point)
        {
            return Math.Abs((l2.X - l1.X) * (l1.Y - point.Y) - (l1.X - point.X) * (l2.Y - l1.Y)) /
                    (float)Math.Sqrt(Math.Pow(l2.X - l1.X, 2) + Math.Pow(l2.Y - l1.Y, 2));
        }

        public static Vector2 LineIntersectionPoint(Vector2 ps1, Vector2 pe1, Vector2 ps2,
                Vector2 pe2)
        {
            // Get A,B,C of first line - points : ps1 to pe1
            float A1 = pe1.Y - ps1.Y;
            float B1 = ps1.X - pe1.X;
            float C1 = A1 * ps1.X + B1 * ps1.Y;

            // Get A,B,C of second line - points : ps2 to pe2
            float A2 = pe2.Y - ps2.Y;
            float B2 = ps2.X - pe2.X;
            float C2 = A2 * ps2.X + B2 * ps2.Y;

            // Get delta and check if the lines are parallel
            float delta = A1 * B2 - A2 * B1;
            if (delta == 0)
                return new Vector2(-1, -1);

            // now return the Vector2 intersection point
            return new Vector2(
                (B2 * C1 - B1 * C2) / delta,
                (A1 * C2 - A2 * C1) / delta
            );
        }

        public static List<Vector2> GetCastMinionsPredictedPositions(List<Obj_AI_Base> minions,
            float delay,
            float width,
            float speed,
            Vector3 from,
            float range,
            bool collision,
            SkillshotType stype,
            Vector3 rangeCheckFrom = new Vector3())
        {
            var result = new List<Vector2>();
            from = from.To2D().IsValid() ? from : ObjectManager.Player.ServerPosition;
            foreach (var minion in minions)
            {
                var pos = Prediction.GetPrediction(new PredictionInput
                {
                    Unit = minion,
                    Delay = delay,
                    Radius = width,
                    Speed = speed,
                    From = from,
                    Range = range,
                    Collision = collision,
                    Type = stype,
                    RangeCheckFrom = rangeCheckFrom
                });

                if (pos.Hitchance >= HitChance.High)
                {
                    result.Add(pos.CastPosition.To2D());
                }
            }

            return result;
        }

    }
}
