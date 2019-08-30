namespace EnsoulSharp.Irelia
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using EnsoulSharp.SDK.MenuUI.Values;
    using EnsoulSharp.SDK.Utility;
    using EnsoulSharp.SDK.Prediction;


    using Color = System.Drawing.Color;
    using static EnsoulSharp.SDK.Items;

    internal class Irelia
    {

        public static Spell Ignite;
        public static Item Hydra;
        public static Item Tiamat;
        public static Item Titanic;
        public static Item Botrk;
        public static Item Bil;
        public static Menu Menu, Items;
        private static AIHeroClient _Player;
        private static readonly Menu MenuIrelia;
        public static Spell Q, W, E1, E2, R;
        private static AIBaseClient target;

        public class ComboIrelia
        {
            public static readonly MenuBool Q = new MenuBool("q", "Use Q");
            public static readonly MenuBool W = new MenuBool("w", "Use W");
            public static readonly MenuBool E = new MenuBool("e", "Use E");
            public static readonly MenuBool R = new MenuBool("r", "Use R");
        }


        public class HaraSs
        {
            public static readonly MenuBool hQ = new MenuBool("useq", "Use Q");
            public static readonly MenuBool hW = new MenuBool("usew", "Use W");
            public static readonly MenuBool hE = new MenuBool("usee", "Use E");
        }

        public class Junglemenu
        {
            public static readonly MenuBool jQ = new MenuBool("useq", "Use Q");
            public static readonly MenuBool jW = new MenuBool("usew", "Use W");
            public static readonly MenuBool jE = new MenuBool("usee", "Use E");
        }

        public class LaneclearMenu
        {
            public static readonly MenuBool lQ = new MenuBool("useq", "Use Q");
            public static readonly MenuBool lW = new MenuBool("usew", "Use W");
            public static readonly MenuBool lE = new MenuBool("usee", "Use E");
        }

        public class KSIrelia
        {
            public static readonly MenuBool KW = new MenuBool("DW", "Killsteal with W");
            public static readonly MenuBool KE = new MenuBool("DE", "Killsteal with E");
            public static readonly MenuBool KQ = new MenuBool("DQ", "Killsteal with Q");
        }


        public class Draws
        {
            public static readonly MenuBool DQ = new MenuBool("Drawq", "Draw Q Range");
            public static readonly MenuBool DW = new MenuBool("Draww", "Draw W Range");
            public static readonly MenuBool DE = new MenuBool("Drawe", "Draw E Range");
            public static readonly MenuBool DR = new MenuBool("Drawr", "Draw R Range");

        }

        public static void OnLoad()
        {
            Q = new Spell(SpellSlot.Q, 860);
            W = new Spell(SpellSlot.W, 300);
            E1 = new Spell(SpellSlot.E, 900);
            E2 = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R, 800);
            Ignite = new Spell(ObjectManager.Player.GetSpellSlot("summonerdot"), 600);
            Tiamat = new Item(ItemId.Tiamat_Melee_Only, 400);
            Hydra = new Item(ItemId.Ravenous_Hydra_Melee_Only, 400);
            Titanic = new Item(ItemId.Titanic_Hydra, Player.Instance.GetRealAutoAttackRange());
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King, 400);
            Bil = new Item(3144, 475f);
            var MenuIrelia = new Menu("EnsoulSharp.Irelia", "EnsoulSharp.Irelia", true);
            //Combo
            var combomenu = new Menu("combo", "Combo");
            combomenu.Add(ComboIrelia.Q);
            combomenu.Add(ComboIrelia.W);
            combomenu.Add(ComboIrelia.E);
            combomenu.Add(ComboIrelia.R);
            MenuIrelia.Add(combomenu);

            //Harras
            var HarassMenu = new Menu("harass", "Harass");
            HarassMenu.Add(HaraSs.hQ);
            HarassMenu.Add(HaraSs.hW);
            HarassMenu.Add(HaraSs.hE);
            MenuIrelia.Add(HarassMenu);

            var junglem = new Menu("jungle", "JungleClear");
            junglem.Add(Junglemenu.jQ);
            junglem.Add(Junglemenu.jW);
            junglem.Add(Junglemenu.jE);
            MenuIrelia.Add(junglem);

            var laneclm = new Menu("lane", "LaneClear");
            laneclm.Add(LaneclearMenu.lQ);
            laneclm.Add(LaneclearMenu.lE);
            laneclm.Add(LaneclearMenu.lW);
            MenuIrelia.Add(laneclm);
            Items = new Menu("Items Settings", "Items");
            Items.Add(new MenuSeparator("Items Settings", "Items Settings"));
            Items.Add(new MenuBool("hydra", "Use [Hydra] Reset AA"));
            Items.Add(new MenuBool("titanic", "Use [Titanic]"));
            Items.Add(new MenuBool("BOTRK", "Use [Botrk]"));
            Items.Add(new MenuSlider("ihp", "My HP Use BOTRK <=", 50));
            Items.Add(new MenuSlider("ihpp", "Enemy HP Use BOTRK <=", 50));
            MenuIrelia.Add(Items);
            //Last

            //Ks
            var KSMenu = new Menu("killsteal", "Killsteal");
            KSMenu.Add(KSIrelia.KW);
            KSMenu.Add(KSIrelia.KE);
            KSMenu.Add(KSIrelia.KQ);
            MenuIrelia.Add(KSMenu);
            //Draw
            var DrawMenu = new Menu("drawings", "Drawings");
            DrawMenu.Add(Draws.DQ);

            DrawMenu.Add(Draws.DE);
            DrawMenu.Add(Draws.DR);
            MenuIrelia.Add(DrawMenu);
            //Flee

            //Loading
            MenuIrelia.Attach();
            Game.OnUpdate += OnTick;
            Drawing.OnDraw += OnDraw;
        }


        private static void Killsteal()
        {
            foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range) && !x.IsInvulnerable && x.Health < Q.GetDamage(x)))
            {
                if (Q.IsReady() && KSIrelia.KQ.Enabled)
                {
                    if (GameObjects.Player.GetSpellDamage(target, SpellSlot.Q) >= target.Health && target.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(target);
                    }
                }
            }
            foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(W.Range) && !x.IsInvulnerable && x.Health < W.GetDamage(x)))
            {
                if (W.IsReady() && KSIrelia.KW.Enabled)
                {
                    if (GameObjects.Player.GetSpellDamage(target, SpellSlot.W) >= target.Health && target.IsValidTarget(W.Range))
                    {
                        W.CastOnUnit(target);
                    }
                }
            }



           

        }

        private static void Harass()
        {
            bool useQ = HaraSs.hQ.Enabled;
            bool useW = HaraSs.hW.Enabled;
            bool useE = HaraSs.hE.Enabled;
            var target = TargetSelector.GetTarget(900);
            if (target != null && target.IsValidTarget(900))
            {
                if (!target.IsValidTarget())
                {
                    return;
                }

                if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range))
                {
                    if (target != null)
                    {
                        Q.CastOnUnit(target);
                    }
                }


                if (W.IsReady() && useW && target.IsValidTarget(W.Range))
                {
                    if (target != null)
                    {
                        W.Cast();
                    }
                }
            }
        }

        private static void OnTick(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling() || MenuGUI.IsChatOpen || ObjectManager.Player.IsWindingUp)
            {
                return;
            }
            Killsteal();
            Item();
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                        Combat();
                        Killsteal();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    Clearing();
                    Jungle();
                    break;
            }
        }

        public static List<AIMinionClient> GetGenericJungleMinionsTargets()
        {
            return GetGenericJungleMinionsTargetsInRange(float.MaxValue);
        }

        public static List<AIMinionClient> GetGenericJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.Jungle.Where(m => !GameObjects.JungleSmall.Contains(m) && m.IsValidTarget(range)).ToList();
        }

        public static List<AIMinionClient> GetEnemyLaneMinionsTargets()
        {
            return GetEnemyLaneMinionsTargetsInRange(float.MaxValue);
        }

        public static List<AIMinionClient> GetEnemyLaneMinionsTargetsInRange(float range)
        {
            return GameObjects.EnemyMinions.Where(m => m.IsValidTarget(range)).ToList();
        }

        private static void Jungle()
        {
            bool useQ = Junglemenu.jQ.Enabled;
            bool useW = Junglemenu.jW.Enabled;
            bool useE = Junglemenu.jE.Enabled;
            var mob = GameObjects.Jungle
                .Where(x => x.IsValidTarget(Q.Range) && x.GetJungleType() != JungleType.Unknown)
                .OrderByDescending(x => x.MaxHealth).FirstOrDefault();


            if (mob != null)
            {
                if (E1.IsReady() && mob.IsValidTarget(900) && !Player.HasBuff("IreliaE"))
                {
                    if (mob != null)
                    {
                        E1.Cast(mob.Position - 400 );
                    }
                }
                if (E2.IsReady() && mob.IsValidTarget(800) && Player.HasBuff("IreliaE"))
                {
                    if (mob!= null)
                    {
                        E2.Cast(mob.Position + 700);
                    }
                }
                if (Q.IsReady() && mob.IsValidTarget(Q.Range) && mob.HasBuff("ireliamark"))
                {
                    Q.CastOnUnit(mob);
                }
               
                if (Q.IsReady() && mob.IsValidTarget(Q.Range) && mob.Health <= GameObjects.Player.GetSpellDamage(mob, SpellSlot.Q))
                {
                    Q.CastOnUnit(mob);
                }
            }
        }
        private static void Clearing()
        {
            bool useQ = LaneclearMenu.lQ.Enabled;
            bool useW = LaneclearMenu.lW.Enabled;
            bool useE = LaneclearMenu.lE.Enabled;
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
            {
                if (minion.Health <= GameObjects.Player.GetSpellDamage(minion, SpellSlot.Q) && Q.IsReady())
                {
                    if (useQ)
                    {
                        {
                            Q.Cast(minion);
                        }
                    }
                    if (useQ)
                    {
                        Q.Cast(minion);
                    }
                }
            }
            if (W.IsReady() && useW)
            {
                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(W.Range))
                {

                    if (minion.IsValidTarget(W.Range))
                    {
                        W.Cast(minion.Position);
                    }

                }
            }
        }
        private static void Combat()
        {
            var target = TargetSelector.GetTarget(925);
            bool useQ = ComboIrelia.Q.Enabled;
            bool useW = ComboIrelia.W.Enabled;
            bool useE = ComboIrelia.E.Enabled;
            bool useR = ComboIrelia.R.Enabled;
            if (!target.IsValidTarget())
            {
                return;
            }
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
            {

                if (minion.Health <= GameObjects.Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    if (useE)
                    {
                        if (315 > minion.Distance(target))
                        {
                            if (!target.IsValidTarget(315))
                            {
                                Q.CastOnUnit(minion);
                            }
                        }
                    }
                    if (useE)
                    {
                        if (!target.IsValidTarget(307))
                        {
                            Q.CastOnUnit(minion);
                        }
                    }

                }

            }
            if (R.IsReady() && target.IsValidTarget(R.Range))
            {
                R.Cast(target.Position);
            }
            /*if (W.IsReady() && (_Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1))
                if (_Player.Position.Distance(target.Position) < W.Range)
                {

                    E1.Cast();

                }
            if (W.IsReady() && (_Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 2))
                if (_Player.Position.Distance(target.Position) > W.Range)
                {


                    E2.Cast();

                }*/
            if (E1.IsReady() && target.IsValidTarget(900)  && !Player.HasBuff("IreliaE"))
            {
                if (target != null)
                {
                    E1.Cast(target.Position -400);
                }
            }
            if (E2.IsReady() && target.IsValidTarget(800) && Player.HasBuff("IreliaE"))
            {
                if (target != null)
                {
                    E2.Cast(target.Position +600);
                }
            }
            if (Q.IsReady() && target.IsValidTarget(Q.Range) && target.HasBuff("ireliamark"))
            {
                Q.CastOnUnit(target);
            }


        }
        public static void Item()
        {
            var item = Items["BOTRK"].GetValue<MenuBool>().Enabled;
            var Minhp = Items["ihp"].GetValue<MenuSlider>().Value;
            var Minhpp = Items["ihpp"].GetValue<MenuSlider>().Value;
            var target = TargetSelector.GetTarget(475, DamageType.Physical);
            if (target != null)
            {
                if (item && Bil.IsReady && Bil.IsOwned() && target.IsValidTarget(475))
                {
                    Bil.Cast(target);
                }

                if ((item && Botrk.IsReady && Botrk.IsOwned() && target.IsValidTarget(475)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }
            }
        }
        private static void Main(string[] args)
        {
            GameEvent.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "Irelia")
                return;
            Irelia.OnLoad();
            Chat.Print("DarkWars3131 Ireliaaa");
        }

        private static void OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || MenuGUI.IsChatOpen)
            {
                return;
            }
            if (Draws.DQ.Enabled && Q.IsReady())
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, Q.Range, Color.Crimson);
            }
            if (Draws.DW.Enabled && W.IsReady())
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, W.Range, Color.Crimson);
            }

            if (Draws.DE.Enabled && E1.IsReady())
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, E1.Range, Color.Blue);
            }
            if (Draws.DR.Enabled && R.IsReady())
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, R.Range, Color.Crimson);
            }
        }
    }
}
